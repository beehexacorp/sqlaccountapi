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

public class SqlAccountingSaleInvoiceHelper
{
    private SqlAccountingORM _microORM;
    public SqlAccountingSaleInvoiceHelper(SqlAccountingORM microORM)
    {
        _microORM = microORM;
    }

    public IEnumerable<IDictionary<string, object>> GetByDocno(string documentNumber){
        var mainFields = _microORM.GetFields("SL_IV").Distinct().ToHashSet(); //app.ComServer.DBManager.NewDataSet("SELECT * FROM AR_CUSTOMER").Fields;

        var sql = $@"SELECT * 
FROM SL_IV 
LEFT JOIN SL_IVDTL ON SL_IV.DOCKEY = SL_IVDTL.DOCKEY 
WHERE SL_IV.DOCNO ='{documentNumber}'
";
           
        return _microORM.GroupQuery(sql, mainFields, "DOCKEY", "cdsDocDetail");
    }
}
