using static System.Runtime.InteropServices.JavaScript.JSType;
using System;
using System.Text.Json.Nodes;
using SqlAccountRestAPI.Models;
using System.Xml;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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

            return rows.ToString(Newtonsoft.Json.Formatting.Indented);
        }
        public void Add(JObject jsonBody){
            var IvBizObj = app.ComServer.BizObjects.Find(jsonBody["type"]);
            var lMainDataSet = IvBizObj.DataSets.Find("MainDataSet");
            if(jsonBody["dataset"] != null)
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