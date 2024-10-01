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
    public class _TestSQL
    {

        private SqlComServer app;
        public _TestSQL(SqlComServer comServer)
        {
            if (comServer == null) throw new Exception("Sql Accounting is not running");
            app = comServer;
        }
        public string run(string sql)
        {
            var dFields = app.ComServer.BizObjects;

            dynamic lMain = app.ComServer.DBManager.NewDataSet(sql);

            Console.WriteLine("----------------------------");
            JArray jsonArray = new JArray();
            lMain.First();

            for (int i=0; i<lMain.Fields.Count; i++){
                Console.WriteLine(lMain.Fields.Items(i).FieldName);
            }

            while (!lMain.eof)
            {
                var Fields = lMain.Fields;

                JObject jsonObject = new JObject();
                for (int i = 0; i < Fields.Count; i++)
                {

                    var lField = Fields.Items(i);
                    if(lField != null){
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
    }
}