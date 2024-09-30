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
    public class Customer
    {
        private SqlComServer app;
        public Customer(SqlComServer comServer)
        {
            if (comServer == null) throw new Exception("Sql Accounting is not running");
            app = comServer;
        }
        public string LoadAllByDaysToNow(int days)
        {
            long todayTimeStamp = new DateTimeOffset(DateTime.UtcNow.Date).ToUnixTimeSeconds();
            long searchDayTimeStamp = todayTimeStamp - days * 24 * 3600;
            dynamic lSQL = "SELECT * FROM "
                +"AR_CUSTOMER JOIN AR_CUSTOMERBRANCH "
                +"ON AR_CUSTOMER.CODE = AR_CUSTOMERBRANCH.CODE "
                +"WHERE AR_CUSTOMER.LASTMODIFIED > "+searchDayTimeStamp.ToString()
                +" ORDER BY AR_CUSTOMER.LASTMODIFIED";
            dynamic lJoinDataset = app.ComServer.DBManager.NewDataSet(lSQL);
            
            dynamic lMainFields = app.ComServer.DBManager.NewDataSet("SELECT * FROM AR_CUSTOMER").Fields;
            dynamic lSubDataSetFields = app.ComServer.DBManager.NewDataSet("SELECT * FROM AR_CUSTOMERBRANCH").Fields;
            
            List<string> mainFieldsArray = new List<string>{};
            for(int i=0; i<lMainFields.Count; i++){
                mainFieldsArray.Add(lMainFields.Items(i).FieldName);
            }
            List<string> subDataSetFieldsArray = new List<string>{};
            for(int i=0; i<lSubDataSetFields.Count; i++){
                subDataSetFieldsArray.Add(lSubDataSetFields.Items(i).FieldName);
            }

            JArray rows = new JArray();
            lJoinDataset.First();
            var mark = "";
            JObject row = new JObject();
            JArray subRowArray = new JArray();
            while(!lJoinDataset.eof){
                var fields = lJoinDataset.Fields;
                
                if(mark != fields.FindField("CODE").value.ToString()){
                    if(mark != ""){
                        row["cdsBranch"] = new JArray();
                        rows.Add(row);
                    }
                    row["cdsBranch"] = subRowArray;
                    

                    row = new JObject();
                    mark = fields.FindField("CODE").value.ToString();

                    foreach(string mainField in mainFieldsArray){
                        if (fields.FindField(mainField).value is string)
                            row[mainField] = fields.FindField(mainField).value;
                    }
                }   

                JObject subRow = new JObject();
                foreach(string subDataSetField in subDataSetFieldsArray){
                    if (fields.FindField(subDataSetField).value is string)
                        subRow[subDataSetField] = fields.FindField(subDataSetField).value;
                }
                subRowArray.Add(subRow);
                lJoinDataset.Next();
                
            }
            return rows.ToString(Newtonsoft.Json.Formatting.Indented);
            
        }
        public string LoadByEmail(string email){
            dynamic lSQL = "SELECT * FROM "
                +"AR_CUSTOMER JOIN AR_CUSTOMERBRANCH "
                +"ON AR_CUSTOMER.CODE = AR_CUSTOMERBRANCH.CODE WHERE AR_CUSTOMER.CODE IN (SELECT CODE FROM AR_CUSTOMERBRANCH WHERE AR_CUSTOMERBRANCH.EMAIL='"+email+"')";

            dynamic lJoinDataset = app.ComServer.DBManager.NewDataSet(lSQL);
            
            dynamic lMainFields = app.ComServer.DBManager.NewDataSet("SELECT * FROM AR_CUSTOMER").Fields;
            dynamic lSubDataSetFields = app.ComServer.DBManager.NewDataSet("SELECT * FROM AR_CUSTOMERBRANCH").Fields;
            
            List<string> mainFieldsArray = new List<string>{};
            for(int i=0; i<lMainFields.Count; i++){
                mainFieldsArray.Add(lMainFields.Items(i).FieldName);
            }
            List<string> subDataSetFieldsArray = new List<string>{};
            for(int i=0; i<lSubDataSetFields.Count; i++){
                subDataSetFieldsArray.Add(lSubDataSetFields.Items(i).FieldName);
            }

            JArray rows = new JArray();
            lJoinDataset.First();
            var mark = "";
            JObject row = new JObject();
            JArray subRowArray = new JArray();
            while(!lJoinDataset.eof){
                var fields = lJoinDataset.Fields;
                
                // new customer
                if(mark != fields.FindField("CODE").value.ToString()){
                    subRowArray = new JArray();
                    row = new JObject();
                    rows.Add(row);
    
                    mark = fields.FindField("CODE").value.ToString();
                    foreach(string mainField in mainFieldsArray){
                        if (fields.FindField(mainField).value is string){
                            row[mainField] = fields.FindField(mainField).value;
                        }
                    }
                    row["cdsBranch"] = subRowArray;

                }   

                JObject subRow = new JObject();
                foreach(string subDataSetField in subDataSetFieldsArray){
                    if (fields.FindField(subDataSetField).value is string)
                        subRow[subDataSetField] = fields.FindField(subDataSetField).value;
                }
                subRowArray.Add(subRow);
                lJoinDataset.Next();
                
            }
            return rows.ToString(Newtonsoft.Json.Formatting.Indented);
        }
    }
}