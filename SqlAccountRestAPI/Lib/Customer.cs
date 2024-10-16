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
                + "AR_CUSTOMER JOIN AR_CUSTOMERBRANCH "
                + "ON AR_CUSTOMER.CODE = AR_CUSTOMERBRANCH.CODE "
                + "WHERE AR_CUSTOMER.LASTMODIFIED > " + searchDayTimeStamp.ToString()
                + " ORDER BY AR_CUSTOMER.LASTMODIFIED";
            dynamic lJoinDataset = app.ComServer.DBManager.NewDataSet(lSQL);

            dynamic lMainFields = app.ComServer.DBManager.NewDataSet("SELECT * FROM AR_CUSTOMER").Fields;
            dynamic lSubDataSetFields = app.ComServer.DBManager.NewDataSet("SELECT * FROM AR_CUSTOMERBRANCH").Fields;

            List<string> mainFieldsArray = new List<string> { };
            for (int i = 0; i < lMainFields.Count; i++)
            {
                mainFieldsArray.Add(lMainFields.Items(i).FieldName);
            }
            List<string> subDataSetFieldsArray = new List<string> { };
            for (int i = 0; i < lSubDataSetFields.Count; i++)
            {
                subDataSetFieldsArray.Add(lSubDataSetFields.Items(i).FieldName);
            }

            JArray rows = new JArray();
            lJoinDataset.First();
            var mark = "";
            JObject row = new JObject();
            JArray subRowArray = new JArray();
            while (!lJoinDataset.eof)
            {
                var fields = lJoinDataset.Fields;

                if (mark != fields.FindField("CODE").value.ToString())
                {
                    subRowArray = new JArray();
                    row = new JObject();
                    rows.Add(row);

                    mark = fields.FindField("CODE").value.ToString();

                    foreach (string mainField in mainFieldsArray)
                    {
                        if (fields.FindField(mainField).value is string)
                            row[mainField] = fields.FindField(mainField).value;
                    }
                    row["cdsBranch"] = subRowArray;
                }

                JObject subRow = new JObject();
                foreach (string subDataSetField in subDataSetFieldsArray)
                {
                    if (fields.FindField(subDataSetField).value is string)
                        subRow[subDataSetField] = fields.FindField(subDataSetField).value;
                }
                subRowArray.Add(subRow);
                lJoinDataset.Next();

            }
            return rows.ToString(Newtonsoft.Json.Formatting.Indented);

        }
        public string LoadByEmail(string email)
        {
            dynamic lSQL = "SELECT * FROM "
                + "AR_CUSTOMER JOIN AR_CUSTOMERBRANCH "
                + "ON AR_CUSTOMER.CODE = AR_CUSTOMERBRANCH.CODE WHERE AR_CUSTOMER.CODE IN (SELECT CODE FROM AR_CUSTOMERBRANCH WHERE AR_CUSTOMERBRANCH.EMAIL='" + email + "')";

            dynamic lJoinDataset = app.ComServer.DBManager.NewDataSet(lSQL);

            dynamic lMainFields = app.ComServer.DBManager.NewDataSet("SELECT * FROM AR_CUSTOMER").Fields;
            dynamic lSubDataSetFields = app.ComServer.DBManager.NewDataSet("SELECT * FROM AR_CUSTOMERBRANCH").Fields;

            List<string> mainFieldsArray = new List<string> { };
            for (int i = 0; i < lMainFields.Count; i++)
            {
                mainFieldsArray.Add(lMainFields.Items(i).FieldName);
            }
            List<string> subDataSetFieldsArray = new List<string> { };
            for (int i = 0; i < lSubDataSetFields.Count; i++)
            {
                subDataSetFieldsArray.Add(lSubDataSetFields.Items(i).FieldName);
            }

            JArray rows = new JArray();
            lJoinDataset.First();
            var mark = "";
            JObject row = new JObject();
            JArray subRowArray = new JArray();
            while (!lJoinDataset.eof)
            {
                var fields = lJoinDataset.Fields;

                // new customer
                if (mark != fields.FindField("CODE").value.ToString())
                {
                    subRowArray = new JArray();
                    row = new JObject();
                    rows.Add(row);

                    mark = fields.FindField("CODE").value.ToString();
                    foreach (string mainField in mainFieldsArray)
                    {
                        if (fields.FindField(mainField).value is string)
                        {
                            row[mainField] = fields.FindField(mainField).value;
                        }
                    }
                    row["cdsBranch"] = subRowArray;

                }

                JObject subRow = new JObject();
                foreach (string subDataSetField in subDataSetFieldsArray)
                {
                    if (fields.FindField(subDataSetField).value is string)
                        subRow[subDataSetField] = fields.FindField(subDataSetField).value;
                }
                subRowArray.Add(subRow);
                lJoinDataset.Next();

            }
            return rows.ToString(Newtonsoft.Json.Formatting.Indented);
        }
        public JObject Payment(JObject jsonBody)
        {
            dynamic lDockey, lSQL, lMain, IvBizObj, lKnockOff, Fields, objectType, lDocAmt;
            
            lSQL = "SELECT FROMDOCTYPE FROM AR_IV WHERE DOCNO='" + jsonBody["DOCNO"] + "'";
            lMain = app.ComServer.DBManager.NewDataSet(lSQL);
            objectType = lMain.FindField("FROMDOCTYPE").value;

            lSQL = "SELECT DOCAMT, DOCKEY FROM SL_"+objectType+" WHERE DOCNO='" + jsonBody["DOCNO"] + "'";
            lMain = app.ComServer.DBManager.NewDataSet(lSQL);

            lDocAmt = lMain.FindField("DOCAMT").value;
            lDockey = lMain.FindField("DOCKEY").value;

            // ADD
            IvBizObj = app.ComServer.BizObjects.Find("AR_PM");
            var lMainDataSet = IvBizObj.DataSets.Find("MainDataSet");

            IvBizObj.New();

            lMainDataSet.FindField("CODE").value = jsonBody["CODE"];
            lMainDataSet.FindField("PAYMENTMETHOD").value = jsonBody["PAYMENTMETHOD"];
            lMainDataSet.FindField("DOCAMT").value = lDocAmt;
            lMainDataSet.FindField("LOCALDOCAMT").value = lDocAmt;
            lMainDataSet.FindField("FROMDOCTYPE").value = objectType;
            lMainDataSet.FindField("PROJECT").value = jsonBody["PROJECT"];
            lMainDataSet.FindField("PAYMENTPROJECT").value = jsonBody["PROJECT"];
            lMainDataSet.FindField("JOURNAL").value = "BANK";
        


            var lCdsDataSet = IvBizObj.DataSets.Find("cdsKnockOff");
            lCdsDataSet.Edit();
            dynamic field;
            field = lCdsDataSet.Findfield("DOCTYPE");
            if (field != null)
                field.value = "PM";

            field = lCdsDataSet.Findfield("TODOCTYPE");
            if (field != null)
                field.value = objectType;

            field = lCdsDataSet.Findfield("KNOCKOFFDOCKEY");
            if (field != null)
                field.value = lMainDataSet.FindField("DOCKEY").value;

            field = lCdsDataSet.Findfield("REFDOCKEY");
            if (field != null)
                field.value = lDockey;

            field = lCdsDataSet.Findfield("LOCALKOAMT");
            if (field != null)
                field.value = lDocAmt;

            field = lCdsDataSet.Findfield("KOAMT");
            if (field != null)
                field.value = lDocAmt;

            field = lCdsDataSet.Findfield("ACTUALLOCALKOAMT");
            if (field != null)
                field.value = lDocAmt;


            lCdsDataSet.Post();

            IvBizObj.Save();
            if (lMainDataSet.FindField("DOCNO") != null)
                return new JObject { { "DOCNO", lMainDataSet.FindField("DOCNO").value.ToString() } };
            return new JObject { { "CODE", lMainDataSet.FindField("CODE").value.ToString() } };
        }
    }
}
