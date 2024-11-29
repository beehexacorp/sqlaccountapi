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

    public IEnumerable<IDictionary<string, object>> GetByDocno(string documentNumber, int limit, int offset){
        var mainFields = _microORM.GetFields("AR_PM", limit, offset).Distinct().ToHashSet(); //app.ComServer.DBManager.NewDataSet("SELECT * FROM AR_CUSTOMER").Fields;

        var sql = $@"SELECT * 
FROM (
    SELECT *
    FROM AR_PM
    WHERE AR_PM.DOCNO ='{documentNumber}'
    OFFSET {offset} ROWS
    FETCH NEXT {limit} ROWS ONLY
) AR_PM_LIMIT
LEFT JOIN AR_KNOCKOFF 
    ON AR_PM_LIMIT.DOCKEY = AR_KNOCKOFF.FROMDOCKEY 
";
           
        return _microORM.GroupQuery(sql, mainFields, "DOCKEY", "cdsKnockOff", 0, offset);
    }
    public IEnumerable<IDictionary<string, object>> GetFromDaysAgo(int days, int limit, int offset){
        var mainFields = _microORM.GetFields("AR_PM", limit, offset).Distinct().ToHashSet(); 

        var date = DateTime.Now.AddDays(-days).ToString("yyyy-MM-dd");
        
        var sql = $@"SELECT * 
FROM (
    SELECT *
    FROM AR_PM
    WHERE AR_PM.DOCDATE >= '{date}'
    OFFSET {offset} ROWS
    FETCH NEXT {limit} ROWS ONLY
) AR_PM_LIMIT
LEFT JOIN AR_KNOCKOFF 
    ON AR_PM_LIMIT.DOCKEY = AR_KNOCKOFF.FROMDOCKEY
";
        
           
        return _microORM.GroupQuery(sql, mainFields, "DOCKEY", "cdsDocDetail", 0, offset);
    }
    public IEnumerable<IDictionary<string, object>> GetFromDate(string date, int limit, int offset){
        var mainFields = _microORM.GetFields("AR_PM", limit, offset).Distinct().ToHashSet(); 
        
        var sql = $@"SELECT * 
FROM (
    SELECT *
    FROM AR_PM
    WHERE AR_PM.DOCDATE >= '{date}'
    OFFSET {offset} ROWS
    FETCH NEXT {limit} ROWS ONLY
) AR_PM_LIMIT
LEFT JOIN AR_KNOCKOFF 
    ON AR_PM_LIMIT.DOCKEY = AR_KNOCKOFF.FROMDOCKEY
";
        
           
        return _microORM.GroupQuery(sql, mainFields, "DOCKEY", "cdsDocDetail", 0, offset);
    }
}