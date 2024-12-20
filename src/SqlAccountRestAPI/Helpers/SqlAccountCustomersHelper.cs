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
            mainDataset.Post();

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
            foreach (var field in _microORM.ItemsIterator(mainDataset.Fields))
            {
                results.Add(field.FieldName, field.value);
            }
            return results;
        }
    }

    public IEnumerable<IDictionary<string, object>> GetByEmail(string email, int limit, int offset)
    {
         if (string.IsNullOrEmpty(email) || !new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$").IsMatch(email))
        {
            throw new ArgumentException("Invalid email format.");
        }
        var mainFields = _microORM.GetFields("AR_CUSTOMER", limit, offset).Distinct().ToHashSet(); //app.ComServer.DBManager.NewDataSet("SELECT * FROM AR_CUSTOMER").Fields;

        var sql = $@"SELECT * 
FROM (
    SELECT *
    FROM AR_CUSTOMER
    WHERE AR_CUSTOMER.CODE IN (
        SELECT CODE 
        FROM AR_CUSTOMERBRANCH 
        WHERE AR_CUSTOMERBRANCH.EMAIL='{email}'
    )
    OFFSET {offset} ROWS
    FETCH NEXT {limit} ROWS ONLY
) AR_CUSTOMER_LIMIT
LEFT JOIN AR_CUSTOMERBRANCH 
    ON AR_CUSTOMER_LIMIT.CODE = AR_CUSTOMERBRANCH.CODE
";
        return _microORM.GroupQuery(sql, mainFields, "CODE", "cdsBranch", 0, offset);
    }
    public IEnumerable<IDictionary<string, object>> GetByCode(string code, int limit, int offset){
        var mainFields = _microORM.GetFields("AR_CUSTOMER", limit, offset).Distinct().ToHashSet(); //app.ComServer.DBManager.NewDataSet("SELECT * FROM AR_CUSTOMER").Fields;

        var sql = $@"SELECT * 
FROM (
    SELECT *
    FROM AR_CUSTOMER
    WHERE AR_CUSTOMER.CODE ='{code}'
    OFFSET {offset} ROWS
    FETCH NEXT {limit} ROWS ONLY
) AR_CUSTOMER_LIMIT
LEFT JOIN AR_CUSTOMERBRANCH 
    ON AR_CUSTOMER_LIMIT.CODE = AR_CUSTOMERBRANCH.CODE 
";
           
        return _microORM.GroupQuery(sql, mainFields, "CODE", "cdsBranch", 0, offset);
    }
    public IEnumerable<IDictionary<string, object>> GetFromDaysAgo(int days, int limit, int offset){
        var mainFields = _microORM.GetFields("AR_CUSTOMER", limit, offset).Distinct().ToHashSet(); 
        
        var currentUnixTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var convertedUnixTime = currentUnixTime - (days * 86400);
        var sql = $@"SELECT *
FROM (
    SELECT *
    FROM AR_CUSTOMER
    WHERE LASTMODIFIED >= {convertedUnixTime}
    OFFSET {offset} ROWS
    FETCH NEXT {limit} ROWS ONLY
) AR_CUSTOMER_LIMIT
LEFT JOIN AR_CUSTOMERBRANCH 
    ON AR_CUSTOMER_LIMIT.CODE = AR_CUSTOMERBRANCH.CODE;
";
        
           
        return _microORM.GroupQuery(sql, mainFields, "CODE", "cdsBranch", 0, offset);
    }
    public IEnumerable<IDictionary<string, object>> GetFromDate(string date, int limit, int offset){
        var mainFields = _microORM.GetFields("AR_CUSTOMER", limit, offset).Distinct().ToHashSet(); 

        DateTime.TryParse(date, out var parsedDate);
        var convertedUnixTime = new DateTimeOffset(parsedDate).ToUnixTimeSeconds();
        
        var sql = $@"SELECT *
FROM (
    SELECT *
    FROM AR_CUSTOMER
    WHERE LASTMODIFIED >= {convertedUnixTime}
    OFFSET {offset} ROWS
    FETCH NEXT {limit} ROWS ONLY
) AR_CUSTOMER_LIMIT
LEFT JOIN AR_CUSTOMERBRANCH 
    ON AR_CUSTOMER_LIMIT.CODE = AR_CUSTOMERBRANCH.CODE;
";
        
           
        return _microORM.GroupQuery(sql, mainFields, "CODE", "cdsBranch", 0, offset);
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

