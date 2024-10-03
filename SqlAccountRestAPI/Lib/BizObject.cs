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
        public string LoadByDaysToNow(string type, int days)
        {
            long todayTimeStamp = new DateTimeOffset(DateTime.UtcNow.Date).ToUnixTimeSeconds();
            long searchDayTimeStamp = todayTimeStamp - days * 24 * 3600;
            if(app.ComServer.BizObjects.Find(type).DataSets.Find("MainDataSet").FindField("LastModified")==null)
            {
                DateTime dateBeforeNDays = DateTime.Now.AddDays(-days);
                string formattedDate = dateBeforeNDays.ToString("yyyy-MM-dd");
                string queryWhere = "DOCDATE>='" + formattedDate + "' AND DOCDATE<'"+DateTime.Now.AddDays(-days+1).ToString("yyyy-MM-dd")+"'";
                return LoadByQuery(
                    type,
                    queryWhere,
                    "DOCDATE"
                );
            }
            return LoadByQuery(
                type,
                "LastModified>=" + searchDayTimeStamp.ToString() + "AND LastModified<" + (searchDayTimeStamp + 24 * 3600).ToString(),
                "LastModified"
            );
        }

        public string LoadAllByDaysToNow(string type, int days)
        {
            long todayTimeStamp = new DateTimeOffset(DateTime.UtcNow.Date).ToUnixTimeSeconds();
            long searchDayTimeStamp = todayTimeStamp - days * 24 * 3600;
            if(app.ComServer.BizObjects.Find(type).DataSets.Find("MainDataSet").FindField("LastModified")==null)
            {
                DateTime dateBeforeNDays = DateTime.Now.AddDays(-days);
                string formattedDate = dateBeforeNDays.ToString("yyyy-MM-dd");
                string queryWhere = "DOCDATE>='" + formattedDate + "'";
                return LoadByQuery(
                    type,
                    queryWhere,
                    "DOCDATE"
                );
            }
           
            return LoadByQuery(
                type,
                "LastModified>=" + searchDayTimeStamp.ToString(),
                "LastModified"
            );
        }
        public string LoadAllByDaysToNowDetail(JObject query)
        {
            long todayTimeStamp = new DateTimeOffset(DateTime.UtcNow.Date).ToUnixTimeSeconds();
            long searchDayTimeStamp = todayTimeStamp - int.Parse(query["days"].ToString()) * 24 * 3600;            
            query["where"] = "LastModified>=" + searchDayTimeStamp.ToString();
            return LoadByQueryDetail(query);
        }
        public string LoadByQueryDetail(JObject query)
        {
            var IvBizObj = app.ComServer.BizObjects.Find(query["type"]);

            var fields = IvBizObj.DataSets.Find(query["dataset"]).Fields;

            string queryWhere = "";
            string queryOrderBy = "";
            string xmlString = "";
            if(query.ContainsKey("days")){
                queryOrderBy = "LastModified";
            }
            if(query.ContainsKey("where")){
                queryWhere = query["where"].ToString();
            }
            try{
                xmlString = IvBizObj.Select("*", queryWhere, queryOrderBy, "SX", ",", "");
            }
            catch (Exception ex){
                DateTime dateBeforeNDays = DateTime.Now.AddDays(-int.Parse(query["days"].ToString()));
                string formattedDate = dateBeforeNDays.ToString("yyyy-MM-dd");
                queryWhere = "DOCDATE>='" + formattedDate + "'";
                queryOrderBy = "DOCDATE";
                xmlString = IvBizObj.Select("*", queryWhere, queryOrderBy, "SX", ",", "");
            }

            // Convert XML to Json
            var doc = new XmlDocument();
            doc.LoadXml(xmlString);
            var rowDataNode = doc.SelectSingleNode("//ROWDATA");
            if (rowDataNode == null)
                return "[]";
            var jsonText = JsonConvert.SerializeXmlNode(rowDataNode, Newtonsoft.Json.Formatting.Indented, true);
            if (jsonText == "null")
                return "[]";
            var jsonObj = JObject.Parse(jsonText);
            var rows = jsonObj["ROW"];
            if (rows == null)
                return "[]";
            if (rows.Type == JTokenType.Object)
                rows = new JArray(rows);
            // Convert Row atts into key-value

            
            foreach (var row in rows)
            {
                var propertiesToRename = row.Children<JProperty>().Where(p => p.Name.StartsWith("@")).ToList();
                foreach (var prop in propertiesToRename)
                {
                    prop.Replace(new JProperty(prop.Name.TrimStart('@'), prop.Value));
                }

                IvBizObj = app.ComServer.BizObjects.Find(query["type"]);
                
                var key = IvBizObj.FindKeyByRef(query["key"], row[query["key"].ToString()]);
                IvBizObj.Params.Find(query["param"]).Value = key;
                var lDataset = IvBizObj.DataSets.Find(query["dataset"]);
                IvBizObj.Open();
                var jsonArray = new JArray();

                lDataset.First();
                while (!lDataset.eof)
                {
                    fields = lDataset.Fields;
                    var jsonData = new JObject();
                    for (int i = 0; i < fields.Count; i++)
                    {
                        var lField = fields.Items(i);
                        if(lField!=null){
                            var keyDetail = lField.FieldName;
                            var valueDetail = lField.value;
                            if (valueDetail != null && valueDetail.ToString() != null)
                                jsonData[keyDetail] = valueDetail.ToString();
                        }
                        
                    }
                    jsonArray.Add(jsonData);
                    lDataset.Next();
                }
                row[query["dataset"].ToString()] = jsonArray;
                IvBizObj.Close();
                System.Runtime.InteropServices.Marshal.ReleaseComObject(IvBizObj);

            }
            System.Runtime.InteropServices.Marshal.ReleaseComObject(IvBizObj);
            return rows.ToString(Newtonsoft.Json.Formatting.Indented);

        }
        public string LoadByQuery(string type, string where, string orderBy)
        {
            var IvBizObj = app.ComServer.BizObjects.Find(type);

            string xmlString = IvBizObj.Select("*", where, orderBy, "SX", ",", "");

            // Convert XML to Json
            var doc = new XmlDocument();
            doc.LoadXml(xmlString);
            var rowDataNode = doc.SelectSingleNode("//ROWDATA");
            if (rowDataNode == null)
                return "[]";
            var jsonText = JsonConvert.SerializeXmlNode(rowDataNode, Newtonsoft.Json.Formatting.Indented, true);
            if (jsonText == "null")
                return "[]";
            var jsonObj = JObject.Parse(jsonText);
            var rows = jsonObj["ROW"];
            if (rows == null)
                return "[]";
            if (rows.Type == JTokenType.Object)
                rows = new JArray(rows);

            // Convert Row atts into key-value
            foreach (var row in rows)
            {
                var propertiesToRename = row.Children<JProperty>().Where(p => p.Name.StartsWith("@")).ToList();
                foreach (var prop in propertiesToRename)
                    prop.Replace(new JProperty(prop.Name.TrimStart('@'), prop.Value));
            }
            System.Runtime.InteropServices.Marshal.ReleaseComObject(IvBizObj);
            return rows.ToString(Newtonsoft.Json.Formatting.Indented);
        }
        public string Add(JObject jsonBody)
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
            IvBizObj.Close();
            // System.Runtime.InteropServices.Marshal.ReleaseComObject(IvBizObj);
            if(lMainDataSet.FindField("CODE") != null)
                return lMainDataSet.FindField("CODE").value.ToString();
            return lMainDataSet.FindField("DOCNO").value.ToString();
           
        }
        public string AddDetail(JObject jsonBody)
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

            foreach(var cdsItem in jsonBody["cds"]){
                var lCdsDataSet = IvBizObj.DataSets.Find(cdsItem["type"].ToString());
                var lDockey = IvBizObj.FindKeyByRef(cdsItem["key"], lMainDataSet.FindField(cdsItem["key"]).value);
                var defaultSubDataSetExistFlag = false;
                if(lCdsDataSet == null) Console.WriteLine("hello");
                if(lCdsDataSet.RecordCount != 0)
                    defaultSubDataSetExistFlag = true;
                foreach(var dataItem in cdsItem["data"]){
                    if(defaultSubDataSetExistFlag){
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
            IvBizObj.Close();
            // System.Runtime.InteropServices.Marshal.ReleaseComObject(IvBizObj);
            if(lMainDataSet.FindField("CODE") != null)
                return lMainDataSet.FindField("CODE").value.ToString();
            return lMainDataSet.FindField("DOCNO").value.ToString();
        }
    }
}