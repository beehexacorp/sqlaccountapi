using System;
using System.Text.Json.Nodes;
using System.Xml;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.Json;
using System.ComponentModel.DataAnnotations;
using SqlAccountRestAPI.Core;

namespace SqlAccountRestAPI.Helpers;

public class SqlAccountingCustomerInvoiceHelper
{
    private SqlAccountingORM _microORM;
    public SqlAccountingCustomerInvoiceHelper(SqlAccountingORM microORM)
    {
        _microORM = microORM;
    }

    public IEnumerable<IDictionary<string, object>> GetByDocno(string documentNumber){
        var mainFields = _microORM.GetFields("AR_IV").Distinct().ToHashSet(); 

        var sql = $@"SELECT * 
FROM AR_IV 
LEFT JOIN AR_IVDTL ON AR_IV.DOCKEY = AR_IVDTL.DOCKEY 
WHERE AR_IV.DOCNO ='{documentNumber}'
";
           
        return _microORM.GroupQuery(sql, mainFields, "DOCKEY", "cdsDocDetail");
    }
    public IEnumerable<IDictionary<string, object>> GetFromDaysAgo(int days){
        var customerFields = _microORM.GetFields("AR_IV").Distinct().ToHashSet(); 

        var date = DateTime.Now.AddDays(-days).ToString("yyyy-MM-dd");
        
        var sql = $@"SELECT * 
FROM AR_IV 
LEFT JOIN AR_IVDTL ON AR_IV.DOCKEY = AR_IVDTL.DOCKEY 
WHERE AR_IV.DOCDATE >= '{date}'
";
        
           
        return _microORM.GroupQuery(sql, customerFields, "DOCKEY", "cdsDocDetail");
    }
    public IEnumerable<IDictionary<string, object>> GetFromDate(string date){
        var customerFields = _microORM.GetFields("AR_IV").Distinct().ToHashSet(); 
        
        var sql = $@"SELECT * 
FROM AR_IV 
LEFT JOIN AR_IVDTL ON AR_IV.DOCKEY = AR_IVDTL.DOCKEY 
WHERE AR_IV.DOCDATE >= '{date}'
";
        
           
        return _microORM.GroupQuery(sql, customerFields, "DOCKEY", "cdsDocDetail");
    }
}
