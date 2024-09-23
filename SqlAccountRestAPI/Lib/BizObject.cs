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
            return LoadByQuery(
                type,
                "LastModified>" + searchDayTimeStamp.ToString() + "AND LastModified<" + (searchDayTimeStamp + 24 * 3600).ToString(),
                "LastModified"
            );
        }

        public string LoadAllByDaysToNow(string type, int days)
        {
            long todayTimeStamp = new DateTimeOffset(DateTime.UtcNow.Date).ToUnixTimeSeconds();
            long searchDayTimeStamp = todayTimeStamp - days * 24 * 3600;
            return LoadByQuery(
                type,
                "LastModified>" + searchDayTimeStamp.ToString(),
                "LastModified"
            );
        }
        public string LoadByQueryDetail(JObject query)
        {
            var IvBizObj = app.ComServer.BizObjects.Find(query["type"]);

            var fields = IvBizObj.DataSets.Find(query["dataset"]).Fields;

            string xmlString = IvBizObj.Select("*", "", "", "SX", ",", "");

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
                IvBizObj.New();
                Console.WriteLine(row["DOCNO"]);
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
                        var keyDetail = lField.FieldName;
                        var valueDetail = lField.value;

                        if (keyDetail is string && valueDetail is string)
                            jsonData[keyDetail] = valueDetail;
                    }
                    jsonArray.Add(jsonData);
                    lDataset.Next();
                }
                row[query["dataset"].ToString()] = jsonArray;
            }
            return rows.ToString(Newtonsoft.Json.Formatting.Indented);

        }
        public string LoadByQuery(string type, string where, string orderBy, string dataset = "MainDataSet")
        {
            var IvBizObj = app.ComServer.BizObjects.Find(type);

            var fields = IvBizObj.DataSets.Find(dataset).Fields;
            for (int i = 0; i < fields.Count; i++)
            {
                Console.WriteLine(fields.Items(i).FieldName);
            }

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

            return rows.ToString(Newtonsoft.Json.Formatting.Indented);
        }
        public void Add(JObject jsonBody)
        {
            var IvBizObj = app.ComServer.BizObjects.Find(jsonBody["type"]);
            var lMainDataSet = IvBizObj.DataSets.Find("MainDataSet");
            if (jsonBody["dataset"] != null)
                lMainDataSet = IvBizObj.DataSets.Find(jsonBody["dataset"]);

            IvBizObj.New();

            foreach (var prop in jsonBody["data"].ToObject<JObject>().Properties())
            {
                var fieldName = prop.Name;
                var fieldValue = prop.Value;
                Console.WriteLine(fieldValue.ToString());
                var field = lMainDataSet.Findfield(fieldName);
                if (field != null && fieldValue != null)
                {
                    field.value = fieldValue.ToString();
                }
            }
            IvBizObj.Save();
        }
    }
}