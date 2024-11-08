using System;
using System.Text.Json.Nodes;
using System.Xml;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.Json;
using System.ComponentModel.DataAnnotations;
using SqlAccountRestAPI.Core;
using System.Text.RegularExpressions;

namespace SqlAccountRestAPI.Helpers;

public class SqlAccountingCustomerHelper
{
    private SqlAccountingORM _microORM;
    public SqlAccountingCustomerHelper(SqlAccountingORM microORM)
    {
        _microORM = microORM;
    }

    public IDictionary<string, object> AddPayment(
        string documentNo,
        string code,
        string paymentMethod,
        string project)
    {
        var customerInvoice = _microORM.QueryFirstOrDefault(
            "SELECT FROMDOCTYPE, DOCAMT FROM AR_IV WHERE DOCNO='" + documentNo + "'",
            new Dictionary<string, object?> {
                {"DOCNO", documentNo}
            });

        if (customerInvoice == null)
        {
            throw new Exception($"Could not find Customer Invoide with DOCNO {documentNo}");
        }

        // ADD Invoice
        using (var paymentBizObject = _microORM.FindBizObject("AR_PM"))
        {
            var mainDataset = paymentBizObject.FindMainDataset();

            paymentBizObject.New();

            mainDataset.FindField("CODE").value = code;
            mainDataset.FindField("PAYMENTMETHOD").value = paymentMethod;
            mainDataset.FindField("DOCAMT").value = customerInvoice["DOCAMT"];
            mainDataset.FindField("LOCALDOCAMT").value = customerInvoice["DOCAMT"];
            mainDataset.FindField("PROJECT").value = project;
            mainDataset.FindField("PAYMENTPROJECT").value = project;

            var knockOfCds = paymentBizObject.FindDataset("cdsKnockOff");
            //Step 5: Knock Off IV
            if (knockOfCds.Locate("DocType;DocNo", new object[2] { "IV", documentNo }, false, false))
            {
                knockOfCds.Edit();
                knockOfCds.FindField("KOAmt").AsFloat = customerInvoice["DOCAMT"];
                knockOfCds.Post();
            }

            paymentBizObject.Save();

            var results = new Dictionary<string, object>();
            foreach (var field in mainDataset.Fields)
            {
                results.Add(field.FieldName, field.value);
            }
            return results;
        }
    }

    public IEnumerable<IDictionary<string, object>> GetByEmail(string email)
    {
         if (string.IsNullOrEmpty(email) || !new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$").IsMatch(email))
        {
            throw new ArgumentException("Invalid email format.");
        }
        var customerFields = _microORM.GetFields("AR_CUSTOMER").Distinct().ToHashSet(); //app.ComServer.DBManager.NewDataSet("SELECT * FROM AR_CUSTOMER").Fields;
        var customerBranchFields = _microORM.GetFields("AR_CUSTOMERBRANCH").Distinct().ToHashSet(); //app.ComServer.DBManager.NewDataSet("SELECT * FROM AR_CUSTOMERBRANCH").Fields;

        var results = _microORM.AsIterator($@"SELECT * 
FROM AR_CUSTOMER
LEFT JOIN AR_CUSTOMERBRANCH ON AR_CUSTOMER.CODE = AR_CUSTOMERBRANCH.CODE
WHERE AR_CUSTOMER.CODE IN (
    SELECT CODE 
    FROM AR_CUSTOMERBRANCH 
    WHERE AR_CUSTOMERBRANCH.EMAIL='{email}'
)")
            .GroupBy(x => x["CODE"].ToString()!)
            .Select(groupped =>
            {
                var firstRecord = groupped.First();
                var customerItem = customerFields.ToDictionary(f => f, f => firstRecord[f]);
                var customerBranches = new List<IDictionary<string, object>>();
                foreach (var item in groupped)
                {
                    customerBranches.Add(customerBranchFields.ToDictionary(f => f, f => item[f]));
                }
                customerItem.Add("cdsBranch", customerBranches);
                return customerItem;
            })
            .ToList();
        return results;
    }
    public IEnumerable<IDictionary<string, object>> GetByCode(string code){
        var customerFields = _microORM.GetFields("AR_CUSTOMER").Distinct().ToHashSet(); //app.ComServer.DBManager.NewDataSet("SELECT * FROM AR_CUSTOMER").Fields;

        var sql = $@"SELECT * 
FROM AR_CUSTOMER 
LEFT JOIN AR_CUSTOMERBRANCH ON AR_CUSTOMER.CODE = AR_CUSTOMERBRANCH.CODE 
WHERE AR_CUSTOMER.CODE ='{code}'
";
           
        return _microORM.GroupQuery(sql, customerFields, "CODE", "cdsBranch");
    }
    public IEnumerable<IDictionary<string, object>> GetFromDaysAgo(int days){
        var customerFields = _microORM.GetFields("AR_CUSTOMER").Distinct().ToHashSet(); 
        
        var currentUnixTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var convertedUnixTime = currentUnixTime - (days * 86400);
        var sql = $@"SELECT * 
FROM AR_CUSTOMER 
LEFT JOIN AR_CUSTOMERBRANCH ON AR_CUSTOMER.CODE = AR_CUSTOMERBRANCH.CODE 
WHERE AR_CUSTOMER.LASTMODIFIED >= {convertedUnixTime}
";
        
           
        return _microORM.GroupQuery(sql, customerFields, "CODE", "cdsBranch");
    }
    public IEnumerable<IDictionary<string, object>> GetFromDate(string date){
        var customerFields = _microORM.GetFields("AR_CUSTOMER").Distinct().ToHashSet(); 

        DateTime.TryParse(date, out var parsedDate);
        var convertedUnixTime = new DateTimeOffset(parsedDate).ToUnixTimeSeconds();
        
        var sql = $@"SELECT * 
FROM AR_CUSTOMER 
LEFT JOIN AR_CUSTOMERBRANCH ON AR_CUSTOMER.CODE = AR_CUSTOMERBRANCH.CODE 
WHERE AR_CUSTOMER.LASTMODIFIED >= {convertedUnixTime}
";
        
           
        return _microORM.GroupQuery(sql, customerFields, "CODE", "cdsBranch");
    }
}
//     public string LoadAllByDaysToNow(int days)
//     {
//         long todayTimeStamp = new DateTimeOffset(DateTime.UtcNow.Date).ToUnixTimeSeconds();
//         long searchDayTimeStamp = todayTimeStamp - days * 24 * 3600;
//         dynamic lSQL = "SELECT * FROM "
//             + "AR_CUSTOMER LEFT JOIN AR_CUSTOMERBRANCH "
//             + "ON AR_CUSTOMER.CODE = AR_CUSTOMERBRANCH.CODE "
//             + "WHERE AR_CUSTOMER.LASTMODIFIED > " + searchDayTimeStamp.ToString()
//             + " ORDER BY AR_CUSTOMER.LASTMODIFIED";
//         dynamic lJoinDataset = app.ComServer.DBManager.NewDataSet(lSQL);

//         dynamic lMainFields = app.ComServer.DBManager.NewDataSet("SELECT * FROM AR_CUSTOMER").Fields;
//         dynamic lSubDataSetFields = app.ComServer.DBManager.NewDataSet("SELECT * FROM AR_CUSTOMERBRANCH").Fields;

//         List<string> mainFieldsArray = new List<string> { };
//         for (int i = 0; i < lMainFields.Count; i++)
//         {
//             mainFieldsArray.Add(lMainFields.Items(i).FieldName);
//         }
//         List<string> subDataSetFieldsArray = new List<string> { };
//         for (int i = 0; i < lSubDataSetFields.Count; i++)
//         {
//             subDataSetFieldsArray.Add(lSubDataSetFields.Items(i).FieldName);
//         }

//         JArray rows = new JArray();
//         lJoinDataset.First();
//         var mark = "";
//         JObject row = new JObject();
//         JArray subRowArray = new JArray();
//         while (!lJoinDataset.eof)
//         {
//             var fields = lJoinDataset.Fields;

//             if (mark != fields.FindField("CODE").value.ToString())
//             {
//                 subRowArray = new JArray();
//                 row = new JObject();
//                 rows.Add(row);

//                 mark = fields.FindField("CODE").value.ToString();

//                 foreach (string mainField in mainFieldsArray)
//                 {
//                     if (fields.FindField(mainField).value is string)
//                         row[mainField] = fields.FindField(mainField).value;
//                 }
//                 row["cdsBranch"] = subRowArray;
//             }

//             JObject subRow = new JObject();
//             foreach (string subDataSetField in subDataSetFieldsArray)
//             {
//                 if (fields.FindField(subDataSetField).value is string)
//                     subRow[subDataSetField] = fields.FindField(subDataSetField).value;
//             }
//             subRowArray.Add(subRow);
//             lJoinDataset.Next();

//         }
//         return rows.ToString(Newtonsoft.Json.Formatting.Indented);

//     }
//     public string LoadByEmail(string email)
//     {

//     }

// }

