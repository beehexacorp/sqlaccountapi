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

    public IEnumerable<IDictionary<string, object>> GetByCode(string code){
        var mainFields = _microORM.GetFields("ST_ITEM").Distinct().ToHashSet(); //app.ComServer.DBManager.NewDataSet("SELECT * FROM ST_ITEM").Fields;

        var sql = $@"SELECT * 
FROM ST_ITEM 
LEFT JOIN ST_ITEM_UOM ON ST_ITEM.CODE = ST_ITEM_UOM.CODE 
WHERE ST_ITEM.CODE ='{code}'
";
           
        return _microORM.GroupQuery(sql, mainFields, "CODE", "cdsUOM");
    }
    public IEnumerable<IDictionary<string, object>> GetFromDaysAgo(int days){
        var customerFields = _microORM.GetFields("ST_ITEM").Distinct().ToHashSet(); 
        
        var currentUnixTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var convertedUnixTime = currentUnixTime - (days * 86400);
        var sql = $@"SELECT * 
FROM ST_ITEM 
LEFT JOIN ST_ITEM_UOM ON ST_ITEM.CODE = ST_ITEM_UOM.CODE 
WHERE ST_ITEM.LASTMODIFIED >= {convertedUnixTime}
";
        
           
        return _microORM.GroupQuery(sql, customerFields, "CODE", "cdsUOM");
    }
    public IEnumerable<IDictionary<string, object>> GetFromDate(string date){
        var customerFields = _microORM.GetFields("ST_ITEM").Distinct().ToHashSet(); 

        DateTime.TryParse(date, out var parsedDate);
        var convertedUnixTime = new DateTimeOffset(parsedDate).ToUnixTimeSeconds();
        
        var sql = $@"SELECT * 
FROM ST_ITEM 
LEFT JOIN ST_ITEM_UOM ON ST_ITEM.CODE = ST_ITEM_UOM.CODE 
WHERE ST_ITEM.LASTMODIFIED >= {convertedUnixTime}
";
        
           
        return _microORM.GroupQuery(sql, customerFields, "CODE", "cdsUOM");
    }
}
