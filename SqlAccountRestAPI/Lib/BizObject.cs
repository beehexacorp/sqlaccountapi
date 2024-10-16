using static System.Runtime.InteropServices.JavaScript.JSType;
using System;
using System.Text.Json.Nodes;
using SqlAccountRestAPI.Models;
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
        private SqlComServer app;
        public BizObject(SqlComServer comServer)
        {
            if (comServer == null) throw new Exception("Sql Accounting is not running");
            app = comServer;
        }
        public string LoadByDaysToNow(string type, int days, int offset, int limit)
        {
            long todayTimeStamp = new DateTimeOffset(DateTime.UtcNow.Date).ToUnixTimeSeconds();
            long searchDayTimeStamp = todayTimeStamp - days * 24 * 3600;
            if (app.ComServer.BizObjects.Find(type).DataSets.Find("MainDataSet").FindField("LastModified") == null)
            {
                DateTime dateBeforeNDays = DateTime.Now.AddDays(-days);
                string formattedDate = dateBeforeNDays.ToString("yyyy-MM-dd");
                string queryWhere = "DOCDATE>='" + formattedDate + "' AND DOCDATE<'" + DateTime.Now.AddDays(-days + 1).ToString("yyyy-MM-dd") + "'";
                return LoadByQuery(
                    type,
                    queryWhere,
                    "DOCDATE",
                    offset,
                    limit
                );
            }
            return LoadByQuery(
                type,
                "LastModified>=" + searchDayTimeStamp.ToString() + "AND LastModified<" + (searchDayTimeStamp + 24 * 3600).ToString(),
                "LastModified",
                offset,
                limit
            );
        }

        public string LoadAllByDaysToNow(string type, int days, int offset, int limit)
        {
            long todayTimeStamp = new DateTimeOffset(DateTime.UtcNow.Date).ToUnixTimeSeconds();
            long searchDayTimeStamp = todayTimeStamp - days * 24 * 3600;
            if (app.ComServer.BizObjects.Find(type).DataSets.Find("MainDataSet").FindField("LastModified") == null)
            {
                DateTime dateBeforeNDays = DateTime.Now.AddDays(-days);
                string formattedDate = dateBeforeNDays.ToString("yyyy-MM-dd");
                string queryWhere = "DOCDATE>='" + formattedDate + "'";
                return LoadByQuery(
                    type,
                    queryWhere,
                    "DOCDATE",
                    offset, limit
                );
            }

            return LoadByQuery(
                type,
                "LastModified>=" + searchDayTimeStamp.ToString(),
                "LastModified",
                offset, limit
            );
        }
        public string LoadAllByDaysToNowDetail(JObject query)
        {
            long todayTimeStamp = new DateTimeOffset(DateTime.UtcNow.Date).ToUnixTimeSeconds();
            long searchDayTimeStamp = todayTimeStamp - query["days"].Value<int>() * 24 * 3600;
            string queryWhere = "LastModified>=" + searchDayTimeStamp.ToString();
            query["where"] = queryWhere;
            if (app.ComServer.BizObjects.Find(query["type"]).DataSets.Find("MainDataSet").FindField("LastModified") == null)
            {
                DateTime dateBeforeNDays = DateTime.Now.AddDays(-query["days"].Value<int>());
                string formattedDate = dateBeforeNDays.ToString("yyyy-MM-dd");
                queryWhere = "DOCDATE>='" + formattedDate + "'";
                query["where"] = queryWhere;
            }

            return LoadByQueryDetail(query);
        }
        public string LoadByQueryDetail(JObject query)
        {
            var lSQL = "SELECT * FROM (SELECT * FROM ";
            lSQL += query["type"];
            if (query["where"].ToString() != "") lSQL += " WHERE " + query["where"];
            lSQL += " ORDER BY " + query["key"];
            lSQL += " OFFSET " + query["offset"] + " ROWS ";
            if (query["limit"].ToString() != "0") lSQL += " FETCH NEXT " + query["limit"].ToString() + " ROWS ONLY ";
            lSQL += " ) AS MAINOBJECT";
            lSQL += " LEFT JOIN " + query["dataset"];
            lSQL += " ON MAINOBJECT." + query["key"] + "=" + query["dataset"] + "." + query["param"];
            var lMain = app.ComServer.DBManager.NewDataSet(lSQL);
            JArray jsonArray = new JArray();

            var IvBizObj = app.ComServer.BizObjects.Find(query["type"]);
            var lMainFields = IvBizObj.DataSets.Find("MainDataSet").Fields;
            List<string> listMainFields = new List<string> { };
            for (int i = 0; i < lMainFields.Count; i++)
            {
                listMainFields.Add(lMainFields.Items(i).FieldName);
            }
            List<string> listSubFields = new List<string> { };
            for (int i = 0; i < lMain.Fields.Count; i++)
            {
                var fieldName = lMain.Fields.Items(i).FieldName;
                if (!listMainFields.Contains(fieldName))
                    listSubFields.Add(lMain.Fields.Items(i).FieldName);
            }

            JArray rows = new JArray();
            lMain.First();
            var mark = "";
            JObject row = new JObject();
            JArray subRowArray = new JArray();
            while (!lMain.eof)
            {
                var fields = lMain.Fields;

                if (mark != fields.FindField(query["key"]).value.ToString())
                {
                    subRowArray = new JArray();
                    row = new JObject();
                    rows.Add(row);

                    mark = fields.FindField(query["key"]).value.ToString();

                    foreach (string mainField in listMainFields)
                    {
                        if (fields.FindField(mainField) != null && fields.FindField(mainField).value != null && fields.FindField(mainField).ToString() != null)
                            row[mainField] = fields.FindField(mainField).value.ToString();
                    }
                    row[query["dataset"].ToString()] = subRowArray;
                }

                JObject subRow = new JObject();
                foreach (string subDataSetField in listSubFields)
                {
                    if (fields.FindField(subDataSetField) != null && fields.FindField(subDataSetField).value != null && fields.FindField(subDataSetField).ToString() != null)
                        subRow[subDataSetField] = fields.FindField(subDataSetField).value.ToString();
                }
                subRowArray.Add(subRow);
                lMain.Next();

            }
            System.Runtime.InteropServices.Marshal.ReleaseComObject(IvBizObj);
            return rows.ToString(Newtonsoft.Json.Formatting.Indented);

        }
        public string LoadByQuery(string type = "", string where = "", string orderBy = "", int offset = 0, int limit = 0)
        {
            var lSQL = "SELECT * FROM " + type;
            if (where != "") lSQL += " WHERE " + where;
            if (orderBy != "") lSQL += " ORDER BY " + orderBy;
            lSQL += " OFFSET " + offset.ToString() + " ROWS ";
            if (limit != 0) lSQL += " FETCH NEXT " + limit.ToString() + " ROWS ONLY ";

            var lMain = app.ComServer.DBManager.NewDataSet(lSQL);
            JArray jsonArray = new JArray();
            lMain.First();
            while (!lMain.eof)
            {
                var Fields = lMain.Fields;

                JObject jsonObject = new JObject();
                for (int i = 0; i < Fields.Count; i++)
                {

                    var lField = Fields.Items(i);
                    if (lField != null)
                    {
                        var key = lField.FieldName;
                        var value = lField.value;
                        if (value != null && value.ToString() != null)
                            jsonObject[key] = value.ToString();
                    }

                }
                jsonArray.Add(jsonObject);
                lMain.Next();
            }
            return jsonArray.ToString();
        }
        public JObject Add(JObject jsonBody)
        {
            var IvBizObj = app.ComServer.BizObjects.Find(jsonBody["type"]);
            var lMainDataSet = IvBizObj.DataSets.Find("MainDataSet");

            IvBizObj.New();

            foreach (var prop in jsonBody["data"].ToObject<JObject>().Properties())
            {
                var fieldName = prop.Name;
                var fieldValue = prop.Value;
                var field = lMainDataSet.Findfield(fieldName);
                if (field != null && fieldValue != null)
                {
                    field.value = fieldValue.ToString();
                }
            }
            IvBizObj.Save();
            // IvBizObj.Close();
            // System.Runtime.InteropServices.Marshal.ReleaseComObject(IvBizObj);
            if (lMainDataSet.FindField("DOCNO") != null)
                return new JObject { { "DOCNO", lMainDataSet.FindField("DOCNO").value.ToString() } };
            return new JObject { { "CODE", lMainDataSet.FindField("CODE").value.ToString() } };

        }
        public JObject AddDetail(JObject jsonBody)
        {
            var IvBizObj = app.ComServer.BizObjects.Find(jsonBody["type"]);
            var lMainDataSet = IvBizObj.DataSets.Find("MainDataSet");

            IvBizObj.New();

            foreach (var prop in jsonBody["data"].ToObject<JObject>().Properties())
            {
                var fieldName = prop.Name;
                var fieldValue = prop.Value;
                var field = lMainDataSet.Findfield(fieldName);
                if (field != null && fieldValue != null)
                {
                    field.value = fieldValue.ToString();
                }
            }

            foreach (var cdsItem in jsonBody["cds"])
            {
                var lCdsDataSet = IvBizObj.DataSets.Find(cdsItem["type"].ToString());
                // var lDockey = IvBizObj.FindKeyByRef(cdsItem["key"], lMainDataSet.FindField(cdsItem["key"]).value);
                var defaultSubDataSetExistFlag = false;
                if (lCdsDataSet.RecordCount != 0)
                    defaultSubDataSetExistFlag = true;
                foreach (var dataItem in cdsItem["data"])
                {
                    if (defaultSubDataSetExistFlag)
                    {
                        lCdsDataSet.Edit();
                        defaultSubDataSetExistFlag = false;
                    }
                    else
                        lCdsDataSet.Append();
                    foreach (var prop in dataItem.ToObject<JObject>().Properties())
                    {
                        var fieldName = prop.Name;
                        var fieldValue = prop.Value;
                        var field = lCdsDataSet.Findfield(fieldName);
                        if (field != null && fieldValue != null)
                        {
                            field.value = fieldValue.ToString();
                        }
                    }
                    lCdsDataSet.Post();
                }
            }
            IvBizObj.Save();
            // IvBizObj.Close();
            // System.Runtime.InteropServices.Marshal.ReleaseComObject(IvBizObj);
            if (lMainDataSet.FindField("DOCNO") != null)
                return new JObject { { "DOCNO", lMainDataSet.FindField("DOCNO").value.ToString() } };
            return new JObject { { "CODE", lMainDataSet.FindField("CODE").value.ToString() } };
        }

        public JObject Transfer(JObject jsonBody)
        {
            dynamic lSQL, lMain, IvBizObj, lDetail, Fields;
            lSQL = "SELECT * FROM " + jsonBody["from"] + " WHERE DOCNO='" + jsonBody["docno"] + "'";
            lMain = app.ComServer.DBManager.NewDataSet(lSQL);

            lMain.First();

            JObject MainObject = new JObject();
            Fields = lMain.Fields;
            for (int i = 0; i < Fields.Count; i++)
            {
                var lField = Fields.Items(i);
                if (lField != null)
                {
                    var key = lField.FieldName;
                    var value = lField.value;
                    if (value != null && value.ToString() != null && value.ToString() != "")
                        MainObject[key] = value.ToString();
                }
            }

            lSQL = "SELECT * FROM " + jsonBody["from"] + "DTL WHERE DOCKEY=" + MainObject["DOCKEY"];
            lDetail = app.ComServer.DBManager.NewDataSet(lSQL);

            lDetail.First();
            JArray DetailObjects = new JArray();
            while (!lDetail.eof)
            {
                JObject DetailObject = new JObject();
                Fields = lDetail.Fields;
                for (int i = 0; i < Fields.Count; i++)
                {
                    var lField = Fields.Items(i);
                    if (lField != null)
                    {
                        var key = lField.FieldName;
                        var value = lField.value;
                        if (value != null && value.ToString() != null && value.ToString() != "")
                            DetailObject[key] = value.ToString();
                    }
                }
                DetailObjects.Add(DetailObject);
                lDetail.Next();
            }

            // ADD
            IvBizObj = app.ComServer.BizObjects.Find(jsonBody["to"]);
            var lMainDataSet = IvBizObj.DataSets.Find("MainDataSet");

            IvBizObj.New();

            foreach (var prop in MainObject.Properties())
            {
                var fieldName = prop.Name;
                var fieldValue = prop.Value;
                var field = lMainDataSet.Findfield(fieldName);
                if (field != null && fieldValue != null
                    && !fieldName.ToString().Contains("DOCNO")
                    && !fieldName.ToString().Contains("DOCKEY")
                    && !fieldName.ToString().Contains("DESCRIPTION")
                )
                {
                    field.value = fieldValue.ToString();
                }
            }

            // Console.WriteLine(MainObject);
            // Console.WriteLine(DetailObjects);
            var lCdsDataSet = IvBizObj.DataSets.Find("cdsDocDetail");
            var defaultSubDataSetExistFlag = false;
            if (lCdsDataSet.RecordCount != 0)
                defaultSubDataSetExistFlag = true;
            foreach (var cdsItem in DetailObjects)
            {
                if (defaultSubDataSetExistFlag)
                {
                    lCdsDataSet.Edit();
                    defaultSubDataSetExistFlag = false;
                }
                else
                    lCdsDataSet.Append();
                if (cdsItem is JObject jsonItem)
                {
                    foreach (var prop in jsonItem.Properties())
                    {
                        var fieldName = prop.Name;
                        var fieldValue = prop.Value;
                        var field = lCdsDataSet.Findfield(fieldName);
                        if (field != null && fieldValue != null
                            && !fieldName.ToString().Contains("DOCNO")
                            && !fieldName.ToString().Contains("DLTKEY")
                            && !fieldName.ToString().Contains("DOCKEY")
                            // && !fieldName.ToString().Contains("QTY")
                        )
                        {
                            Console.WriteLine(fieldName);
                            field.value = fieldValue.ToString();
                        }
                    }
                }

                Console.WriteLine(MainObject["DOCKEY"]);
                lCdsDataSet.Findfield("FROMDOCTYPE").value = string.Join("",jsonBody["from"].ToString().Split('_').Skip(1));
                lCdsDataSet.Findfield("FROMDOCKEY").value = MainObject["DOCKEY"];
                lCdsDataSet.Findfield("FROMDTLKEY").value = cdsItem["DTLKEY"];
                lCdsDataSet.Post();
            }

            IvBizObj.Save();
            if (lMainDataSet.FindField("DOCNO") != null)
                return new JObject { { "DOCNO", lMainDataSet.FindField("DOCNO").value.ToString() } };
            return new JObject { { "CODE", lMainDataSet.FindField("CODE").value.ToString() } };
        }
    }
}