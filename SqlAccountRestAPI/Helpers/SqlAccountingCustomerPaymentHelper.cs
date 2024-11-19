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

public class SqlAccountingCustomerPaymentHelper
{
    private SqlAccountingORM _microORM;
    public SqlAccountingCustomerPaymentHelper(SqlAccountingORM microORM)
    {
        _microORM = microORM;
    }

    public IEnumerable<IDictionary<string, object>> GetByDocno(string documentNumber){
        var mainFields = _microORM.GetFields("AR_PM").Distinct().ToHashSet(); //app.ComServer.DBManager.NewDataSet("SELECT * FROM AR_CUSTOMER").Fields;

        var sql = $@"SELECT * 
FROM AR_PM
LEFT JOIN AR_KNOCKOFF ON AR_PM.DOCKEY = AR_KNOCKOFF.FROMDOCKEY 
WHERE AR_PM.DOCNO ='{documentNumber}'
";
           
        return _microORM.GroupQuery(sql, mainFields, "DOCKEY", "cdsKnockOff");
    }
    public IEnumerable<IDictionary<string, object>> GetFromDaysAgo(int days){
        var customerFields = _microORM.GetFields("AR_PM").Distinct().ToHashSet(); 

        var date = DateTime.Now.AddDays(-days).ToString("yyyy-MM-dd");
        
        var sql = $@"SELECT * 
FROM AR_PM
LEFT JOIN AR_KNOCKOFF ON AR_PM.DOCKEY = AR_KNOCKOFF.FROMDOCKEY
WHERE AR_PM.DOCDATE >= '{date}'
";
        
           
        return _microORM.GroupQuery(sql, customerFields, "DOCKEY", "cdsDocDetail");
    }
    public IEnumerable<IDictionary<string, object>> GetFromDate(string date){
        var customerFields = _microORM.GetFields("AR_PM").Distinct().ToHashSet(); 
        
        var sql = $@"SELECT * 
FROM AR_PM
LEFT JOIN AR_KNOCKOFF ON AR_PM.DOCKEY = AR_KNOCKOFF.FROMDOCKEY
WHERE AR_PM.DOCDATE >= '{date}'
";
        
           
        return _microORM.GroupQuery(sql, customerFields, "DOCKEY", "cdsDocDetail");
    }
}