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
        public string LoadByDaysToNow(string Type, int Days)
        {
            long todayTimeStamp = new DateTimeOffset(DateTime.UtcNow.Date).ToUnixTimeSeconds();
            long searchDayTimeStamp = todayTimeStamp - Days * 24 * 3600;
            return LoadByQuery(new Query
            {
                Where = "LastModified>" + searchDayTimeStamp.ToString() + "AND LastModified<" + (searchDayTimeStamp + 24 * 3600).ToString(),
                OrderBy = "LastModified"
            });
        }

        public string LoadAllByDaysToNow(string Type, int Days)
        {
            long todayTimeStamp = new DateTimeOffset(DateTime.UtcNow.Date).ToUnixTimeSeconds();
            long searchDayTimeStamp = todayTimeStamp - Days * 24 * 3600;
            return LoadByQuery(new Query
            {
                Where = "LastModified>" + searchDayTimeStamp.ToString(),
                OrderBy = "LastModified"
            });
        }

        public string LoadByQuery(Query query)
        {
            var IvBizObj = app.ComServer.BizObjects.Find(query.Type);

            string xmlString = IvBizObj.Select("*", query.Where, query.OrderBy, "SX", ",", "");

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
    }
}