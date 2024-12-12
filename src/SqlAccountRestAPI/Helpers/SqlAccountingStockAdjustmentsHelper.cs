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

    public IEnumerable<IDictionary<string, object>> GetByDocno(string documentNumber, int limit, int offset){
        var mainFields = _microORM.GetFields("ST_AJ", limit, offset).Distinct().ToHashSet(); //app.ComServer.DBManager.NewDataSet("SELECT * FROM AR_CUSTOMER").Fields;

        var sql = $@"SELECT * 
FROM (
    SELECT *
    FROM ST_AJ
    WHERE ST_AJ.DOCNO ='{documentNumber}'
    OFFSET {offset} ROWS
    FETCH NEXT {limit} ROWS ONLY
) ST_AJ_LIMIT
LEFT JOIN ST_AJDTL 
    ON ST_AJ_LIMIT.DOCKEY = ST_AJDTL.DOCKEY 
";
           
        return _microORM.GroupQuery(sql, mainFields, "DOCKEY", "cdsDocDetail", 0, offset);
    }
    public IEnumerable<IDictionary<string, object>> GetFromDaysAgo(int days, int limit, int offset){
        var mainFields = _microORM.GetFields("ST_AJ", limit, offset).Distinct().ToHashSet(); 

        var date = DateTime.Now.AddDays(-days).ToString("yyyy-MM-dd");
        
        var sql = $@"SELECT * 
FROM (
    SELECT *
    FROM ST_AJ
    WHERE ST_AJ.DOCDATE >= '{date}'
    OFFSET {offset} ROWS
    FETCH NEXT {limit} ROWS ONLY
) ST_AJ_LIMIT
LEFT JOIN ST_AJDTL 
    ON ST_AJ_LIMIT.DOCKEY = ST_AJDTL.DOCKEY 
";
        
           
        return _microORM.GroupQuery(sql, mainFields, "DOCKEY", "cdsDocDetail", 0, offset);
    }
    public IEnumerable<IDictionary<string, object>> GetFromDate(string date, int limit, int offset){
        var mainFields = _microORM.GetFields("ST_AJ", limit, offset).Distinct().ToHashSet(); 
        
        var sql = $@"SELECT * 
FROM (
    SELECT *
    FROM ST_AJ
    WHERE ST_AJ.DOCDATE >= '{date}'
    OFFSET {offset} ROWS
    FETCH NEXT {limit} ROWS ONLY
) ST_AJ_LIMIT
LEFT JOIN ST_AJDTL 
    ON ST_AJ_LIMIT.DOCKEY = ST_AJDTL.DOCKEY 
";
        
           
        return _microORM.GroupQuery(sql, mainFields, "DOCKEY", "cdsDocDetail", 0, offset);
    }
}