using static System.Runtime.InteropServices.JavaScript.JSType;
using System;
using System.Text.Json.Nodes;
using SqlAccountRestAPI.Models;
using System.Xml;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SqlAccountRestAPI.Lib
{
    public class Generic
    {
        private SqlComServer app;
        public Generic(SqlComServer comServer)
        {
            if (comServer == null) throw new Exception("Sql Accounting is not running");
            app = comServer;
        }
        public string LoadByQuery(Query query)
        {
            var IvBizObj = app.ComServer.BizObjects.Find(query.Type);

            string xmlString = IvBizObj.Select("*", query.Where, "", "SX", ",", "");

            // Convert XML to Json
            var doc = new XmlDocument();
            doc.LoadXml(xmlString);

            var rowDataNode = doc.SelectSingleNode("//ROWDATA");

            if (rowDataNode == null)
            {
                return "";
            }

            var jsonText = JsonConvert.SerializeXmlNode(rowDataNode, Newtonsoft.Json.Formatting.Indented, true);

            var jsonObj = JObject.Parse(jsonText);
            var rows = jsonObj["ROW"];
            if (rows == null)
                return "[]";
            if (rows.Type == JTokenType.Object)
            {
                rows = new JArray(rows);
            }

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