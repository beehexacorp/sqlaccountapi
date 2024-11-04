using static System.Runtime.InteropServices.JavaScript.JSType;
using System;
using System.Text.Json.Nodes;
using System.Xml;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.Json;
using System.ComponentModel.DataAnnotations;

namespace SqlAccountRestAPI.Lib
{
    public class BizObject
    {
        private SqlAccountingApp _app;
        public BizObject(SqlAccountingApp app)
        {
            if (!app.IsLoggedIn) throw new Exception("SQL Accounting App is not logged in yet.");
            _app = app;
        }
        public IEnumerable<IDictionary<string, object>> Query(
            string sql,
            IDictionary<string, object> @params,
            int offset = 0,
            int limit = 100)
        {
            // TODO: (later) add cursor-based query
            var results = _app.Query(sql, @params, offset, limit);
            return results;
        }

        /**
        {
            "entityType": "SL_SO",
            "data": {
               "ORDER_NO": ...,
               "ORDER_NAME": ...,
               "TOTAL_PRICE...,
               "SL_SOD": [...], // cds
               "SL_SO_CUSTOMER": {...}, // cds
            }
        }
        */
        public IDictionary<string, object> AddDetail(
            string entityType,
            IDictionary<string, object?> data,

            // TODO: children should be included in "data" as given example above
            IEnumerable<BizObjectAddChildrenRequest>? children)
        {
            using (var IvBizObj = _app.FindBizObject(entityType))
            {
                var lMainDataSet = IvBizObj.FindDataset("MainDataSet");
                IvBizObj.New();

                foreach (var prop in data)
                {
                    var field = lMainDataSet.Findfield(prop.Key);
                    if (field != null)
                    {
                        field.value = prop.Value?.ToString();
                    }
                    else
                    {
                        // TODO: this is cds
                        // AddChildrenDataset(IvBizObj, prop.Key, prop.Value);
                    }
                }

                // TODO: replace this with add children dataset above
                if (children != null)
                {
                    foreach (var cdsItem in children)
                    {
                        AddChildrenDataset(IvBizObj, cdsItem.EntityType, cdsItem.Data);
                    }
                }
                IvBizObj.Save();

                IDictionary<string, object> results = new Dictionary<string, object>();
                foreach (var field in lMainDataSet.Fields)
                {
                    results.Add(field.FieldName, field.value);
                }
                return results;
                // IvBizObj.Close();
                // System.Runtime.InteropServices.Marshal.ReleaseComObject(IvBizObj);
                // if (lMainDataSet.FindField("DOCNO") != null)
                //     return new JObject { { "DOCNO", lMainDataSet.FindField("DOCNO").value.ToString() } };
                // return new JObject { { "CODE", lMainDataSet.FindField("CODE").value.ToString() } };
            }
        }

        private void AddChildrenDataset(SqlAccountingBizObject bizObject, string entityType, IEnumerable<IDictionary<string, object?>>? cdsData)
        {
            var lCdsDataSet = bizObject.FindDataset(entityType);
            var defaultSubDataSetExistFlag = false;
            if (lCdsDataSet.RecordCount != 0)
                defaultSubDataSetExistFlag = true;
            cdsData = cdsData ?? new List<IDictionary<string, object?>>();
            foreach (var dataItem in cdsData)
            {
                if (defaultSubDataSetExistFlag)
                {
                    lCdsDataSet.Edit();
                    defaultSubDataSetExistFlag = false;
                }
                else
                    lCdsDataSet.Append();
                foreach (var prop in dataItem)
                {
                    var field = lCdsDataSet.Findfield(prop.Key);
                    if (field != null)
                    {
                        field.value = prop.Value?.ToString();
                    }
                }
                lCdsDataSet.Post();
            }
        }

        public IDictionary<string, object> Transfer(string fromEntityType, string toEntityType, string docNo)
        {
            // TODO: what if the entity does not have the DOCNO, but CODE instead?
            var mainObject = _app.QueryFirstOrDefault(
                "SELECT * FROM " + fromEntityType + " WHERE DOCNO='" + docNo + "'",
                new Dictionary<string, object>());
            if (mainObject == null)
            {
                throw new Exception($"The source object {fromEntityType}, DOCNO={docNo} to transform is not found");
            }
            var docKey = mainObject["DOCKEY"];
            using (var IvBizObj = _app.FindBizObject(toEntityType!.ToString()))
            {
                var lMainDataSet = IvBizObj.FindDataset("MainDataSet");

                IvBizObj.New();

                foreach (var prop in mainObject)
                {
                    if (new List<string> { "DOCNO", "DOCKEY", "DESCRIPTION" }.Any(k => prop.Key.Contains(k) == true))
                    {
                        continue;
                    }
                    var field = lMainDataSet.Findfield(prop.Key);
                    if (field != null)
                    {
                        // TODO: do we overwrite null values?
                        field.value = prop.Value;
                    }
                }

                var lCdsDataSet = IvBizObj.FindDataset("cdsDocDetail");
                var defaultSubDataSetExistFlag = false;
                if (lCdsDataSet.RecordCount != 0)
                    defaultSubDataSetExistFlag = true;

                var offset = 0;
                const int limit = 100;
                while (true)
                {
                    var toTransferDetails = _app.Query(
                        "SELECT * FROM " + fromEntityType + "DTL WHERE DOCKEY=" + docKey,
                        new Dictionary<string, object>(),
                        offset,
                        limit);
                    if (toTransferDetails == null || !toTransferDetails.Any())
                    {
                        break;
                    }
                    offset += limit;
                    foreach (var cdsItem in toTransferDetails)
                    {
                        if (defaultSubDataSetExistFlag)
                        {
                            lCdsDataSet.Edit();
                            defaultSubDataSetExistFlag = false;
                        }
                        else
                            lCdsDataSet.Append();
                        foreach (var prop in cdsItem)
                        {
                            if (new List<string> { "DOCNO", "DLTKEY", "DOCKEY" }.Any(k => prop.Key.Contains(k) == true))
                            {
                                continue;
                            }
                            var field = lCdsDataSet.Findfield(prop.Key);
                            if (field != null)
                            {
                                // TODO: do we overwrite null values?
                                field.value = prop.Value;
                            }
                        }

                        lCdsDataSet.Findfield("FROMDOCTYPE").value = string.Join("", fromEntityType!.ToString().Split('_').Skip(1));
                        lCdsDataSet.Findfield("FROMDOCKEY").value = docKey;
                        lCdsDataSet.Findfield("FROMDTLKEY").value = cdsItem["DTLKEY"];
                        lCdsDataSet.Post();
                    }
                }

                IvBizObj.Save();

                IDictionary<string, object> results = new Dictionary<string, object>();
                foreach (var field in lMainDataSet.Fields)
                {
                    results.Add(field.FieldName, field.value);
                }
                return results;
            }

            // if (lMainDataSet.FindField("DOCNO") != null)
            //     return new JObject { { "DOCNO", lMainDataSet.FindField("DOCNO").value.ToString() } };
            // return new JObject { { "CODE", lMainDataSet.FindField("CODE").value.ToString() } };
        }
    }
}