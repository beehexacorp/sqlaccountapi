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

public class SqlAccountingStockItemTemplateHelper
{
    private SqlAccountingORM _microORM;
    public SqlAccountingStockItemTemplateHelper(SqlAccountingORM microORM)
    {
        _microORM = microORM;
    }

    public IEnumerable<IDictionary<string, object>> GetByCode(string code){
        var mainFields = _microORM.GetFields("ST_ITEM_TPL").Distinct().ToHashSet(); //app.ComServer.DBManager.NewDataSet("SELECT * FROM AR_CUSTOMER").Fields;

        var sql = $@"SELECT * 
FROM ST_ITEM_TPL
LEFT JOIN ST_ITEM_TPLDTL ON ST_ITEM_TPL.CODE = ST_ITEM_TPLDTL.CODE 
WHERE ST_ITEM_TPL.CODE ='{code}'
";
           
        return _microORM.GroupQuery(sql, mainFields, "CODE", "cdsItemTplDtl");
    }
}