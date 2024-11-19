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

public class SqlAccountingStockAdjustmentHelper
{
    private SqlAccountingORM _microORM;
    public SqlAccountingStockAdjustmentHelper(SqlAccountingORM microORM)
    {
        _microORM = microORM;
    }

    public IEnumerable<IDictionary<string, object>> GetByDocno(string documentNumber){
        var mainFields = _microORM.GetFields("ST_AJ").Distinct().ToHashSet(); //app.ComServer.DBManager.NewDataSet("SELECT * FROM AR_CUSTOMER").Fields;

        var sql = $@"SELECT * 
FROM ST_AJ
LEFT JOIN ST_AJDTL ON ST_AJ.DOCKEY = ST_AJDTL.DOCKEY 
WHERE ST_AJ.DOCNO ='{documentNumber}'
";
           
        return _microORM.GroupQuery(sql, mainFields, "DOCKEY", "cdsDocDetail");
    }
    public IEnumerable<IDictionary<string, object>> GetFromDaysAgo(int days){
        var customerFields = _microORM.GetFields("ST_AJ").Distinct().ToHashSet(); 

        var date = DateTime.Now.AddDays(-days).ToString("yyyy-MM-dd");
        
        var sql = $@"SELECT * 
FROM ST_AJ
LEFT JOIN ST_AJDTL ON ST_AJ.DOCKEY = ST_AJDTL.DOCKEY 
WHERE ST_AJ.DOCDATE >= '{date}'
";
        
           
        return _microORM.GroupQuery(sql, customerFields, "DOCKEY", "cdsDocDetail");
    }
    public IEnumerable<IDictionary<string, object>> GetFromDate(string date){
        var customerFields = _microORM.GetFields("ST_AJ").Distinct().ToHashSet(); 
        
        var sql = $@"SELECT * 
FROM ST_AJ
LEFT JOIN ST_AJDTL ON ST_AJ.DOCKEY = ST_AJDTL.DOCKEY 
WHERE ST_AJ.DOCDATE >= '{date}'
";
        
           
        return _microORM.GroupQuery(sql, customerFields, "DOCKEY", "cdsDocDetail");
    }
}