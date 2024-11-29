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

public class SqlAccountingStockItemHelper
{
    private SqlAccountingORM _microORM;
    public SqlAccountingStockItemHelper(SqlAccountingORM microORM)
    {
        _microORM = microORM;
    }

    public IEnumerable<IDictionary<string, object>> GetByCode(string code, int limit, int offset){
        var mainFields = _microORM.GetFields("ST_ITEM", limit, offset).Distinct().ToHashSet(); //app.ComServer.DBManager.NewDataSet("SELECT * FROM ST_ITEM").Fields;

        var sql = $@"SELECT * 
FROM (
    SELECT *
    FROM ST_ITEM
    WHERE ST_ITEM.CODE ='{code}'
    OFFSET {offset} ROWS
    FETCH NEXT {limit} ROWS ONLY
) ST_ITEM_LIMIT
LEFT JOIN ST_ITEM_UOM 
    ON ST_ITEM_LIMIT.CODE = ST_ITEM_UOM.CODE 
";
           
        return _microORM.GroupQuery(sql, mainFields, "CODE", "cdsUOM", 0, offset);
    }
    public IEnumerable<IDictionary<string, object>> GetFromDaysAgo(int days, int limit, int offset){
        var mainFields = _microORM.GetFields("ST_ITEM", limit, offset).Distinct().ToHashSet(); 
        
        var currentUnixTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var convertedUnixTime = currentUnixTime - (days * 86400);
        var sql = $@"SELECT * 
FROM (
    SELECT * 
    FROM ST_ITEM 
    WHERE ST_ITEM.LASTMODIFIED >= {convertedUnixTime}
    OFFSET {offset} ROWS
    FETCH NEXT {limit} ROWS ONLY
) ST_ITEM_LIMIT
LEFT JOIN ST_ITEM_UOM 
    ON ST_ITEM_LIMIT.CODE = ST_ITEM_UOM.CODE 
";
        
           
        return _microORM.GroupQuery(sql, mainFields, "CODE", "cdsUOM", 0, offset);
    }
    public IEnumerable<IDictionary<string, object>> GetFromDate(string date, int limit, int offset){
        var mainFields = _microORM.GetFields("ST_ITEM", limit, offset).Distinct().ToHashSet(); 

        DateTime.TryParse(date, out var parsedDate);
        var convertedUnixTime = new DateTimeOffset(parsedDate).ToUnixTimeSeconds();
        
        var sql = $@"SELECT * 
FROM (
    SELECT * 
    FROM ST_ITEM 
    WHERE ST_ITEM.LASTMODIFIED >= {convertedUnixTime}
    OFFSET {offset} ROWS
    FETCH NEXT {limit} ROWS ONLY
) ST_ITEM_LIMIT
LEFT JOIN ST_ITEM_UOM 
    ON ST_ITEM_LIMIT.CODE = ST_ITEM_UOM.CODE 
";
        
           
        return _microORM.GroupQuery(sql, mainFields, "CODE", "cdsUOM", 0, offset);
    }
}
