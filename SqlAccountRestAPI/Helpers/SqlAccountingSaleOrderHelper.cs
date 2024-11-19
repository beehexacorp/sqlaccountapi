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

public class SqlAccountingSaleOrderHelper
{
    private SqlAccountingORM _microORM;
    public SqlAccountingSaleOrderHelper(SqlAccountingORM microORM)
    {
        _microORM = microORM;
    }

    public IEnumerable<IDictionary<string, object>> GetByDocno(string documentNumber){
        var mainFields = _microORM.GetFields("SL_SO").Distinct().ToHashSet(); //app.ComServer.DBManager.NewDataSet("SELECT * FROM AR_CUSTOMER").Fields;

        var sql = $@"SELECT * 
FROM SL_SO
LEFT JOIN SL_SODTL ON SL_SO.DOCKEY = SL_SODTL.DOCKEY 
WHERE SL_SO.DOCNO ='{documentNumber}'
";
           
        return _microORM.GroupQuery(sql, mainFields, "DOCKEY", "cdsDocDetail");
    }
    public IEnumerable<IDictionary<string, object>> GetFromDaysAgo(int days){
        var customerFields = _microORM.GetFields("SL_SO").Distinct().ToHashSet(); 

        var date = DateTime.Now.AddDays(-days).ToString("yyyy-MM-dd");
        
        var sql = $@"SELECT * 
FROM SL_SO
LEFT JOIN SL_SODTL ON SL_SO.DOCKEY = SL_SODTL.DOCKEY 
WHERE SL_SO.DOCDATE >= '{date}'
";
        
           
        return _microORM.GroupQuery(sql, customerFields, "DOCKEY", "cdsDocDetail");
    }
    public IEnumerable<IDictionary<string, object>> GetFromDate(string date){
        var customerFields = _microORM.GetFields("SL_SO").Distinct().ToHashSet(); 
        
        var sql = $@"SELECT * 
FROM SL_SO
LEFT JOIN SL_SODTL ON SL_SO.DOCKEY = SL_SODTL.DOCKEY 
WHERE SL_SO.DOCDATE >= '{date}'
";
        
           
        return _microORM.GroupQuery(sql, customerFields, "DOCKEY", "cdsDocDetail");
    }
}