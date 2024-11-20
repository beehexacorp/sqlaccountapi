using SqlAccountRestAPI.Helpers;

namespace SqlAccountRestAPI.Core;
public class SqlAccountingORM
{
    protected readonly SqlAccountingFactory _factory;

    public SqlAccountingORM(SqlAccountingFactory factory)
    {
        _factory = factory;
    }

    public virtual void Login(string username, string password)
    {
        /** 
        TODO: 
        1. Store the User & Password in an encrypted file
        2. Whenever an application is stopped and restarted, it must re-login using the cached Username & Password
        */        
        dynamic app = _factory.GetInstance();
        if (app.IsLogin == true)
        {
            app.Logout();
            _factory.Release();
            
            app = _factory.GetInstance();
        }
        app.Login(username, password);

    }

    public SqlAccountingBizObject FindBizObject(string name)
    {
        dynamic app = _factory.GetInstance();
        var result = new SqlAccountingBizObject(app.BizObjects.Find(name));
        return result;
    }

    public IEnumerable<string> GetFields(string entityType)
    {
        dynamic app = _factory.GetInstance();
        var fields = app.DBManager.NewDataSet($@"SELECT * 
FROM {entityType}
OFFSET 0 ROWS
FETCH NEXT 1 ROWS ONLY").Fields;
        // var results = new List<string>();
        // foreach (var field in fields)
        // {
        //     results.Add(field.FieldName);
        // }
        // return results;
        return FieldIterator(fields);
    }

    public dynamic CreateDataset(string sql, IDictionary<string, object?>? @params = null)
    {
        // TODO: use params
        dynamic app = _factory.GetInstance();
        return app.DBManager.NewDataSet(sql);
    }

    public IDictionary<string, object>? QueryFirstOrDefault(string sql, IDictionary<string, object?>? @params = null)
    {
        sql = $@"{sql}
OFFSET 0 ROWS
FETCH NEXT 1 ROWS ONLY";
        var dataset = CreateDataset(sql, @params);
        try
        {
            dataset.First();
            if (dataset.eof)
            {
                return null;
            }
            var fields = dataset.Fields;

            var item = ((IEnumerable<dynamic>)ItemsIterator(fields))
                .ToDictionary(
                    field => (string)field.FieldName,
                    field => field.value
                );

            return item;
        }
        finally
        {
            if (dataset != null)
            {
                System.Runtime.InteropServices.Marshal.ReleaseComObject(dataset);
            }
        }
    }

    public virtual IEnumerable<IDictionary<string, object>> Query(string sql, IDictionary<string, object?>? @params = null, int offset = 0, int limit = 100)
    {
        var results = new List<IDictionary<string, object>>();
        foreach (var item in AsIterator(sql, @params, offset, limit))
        {
            results.Add(item);
        }
        return results;
    }
    public IEnumerable<IDictionary<string, object>> AsIterator(string sql, IDictionary<string, object?>? @params = null, int offset = 0, int limit = 100)
    {
        sql = $@"{sql} 
OFFSET {offset} ROWS 
FETCH NEXT {limit} ROWS ONLY";
        var dataset = CreateDataset(sql, @params);
        try
        {
            dataset.First();
            while (!dataset.eof)
            {
                IEnumerable<dynamic> fields = ItemsIterator(dataset.Fields);

                List<string> check = new List<string> { };
                foreach (var field in fields){
                    check.Add(field.FieldName);
                }
                

                var item = fields
                .ToDictionary(
                    field => (string)field.FieldName,
                    field => field.value
                );
                dataset.Next();

                yield return item;
            }
        }
        finally
        {
            if (dataset != null)
            {
                System.Runtime.InteropServices.Marshal.ReleaseComObject(dataset);
            }
        }
    }
    public IEnumerable<string> FieldIterator(dynamic fields){
        foreach (var field in ItemsIterator(fields)){
            yield return field.FieldName;
        }
    }
    public IEnumerable<dynamic> ItemsIterator(dynamic fields){
        for (var i = 0; i < fields.Count; i++){
            yield return fields.Items(i);
        }
    }
    public IEnumerable<IDictionary<string, object>> GroupQuery(
        string sql,
        HashSet<string> mainFields,
        string groupBy,
        string cdsName
    )
    {
        var results = AsIterator(sql)
            .GroupBy(x => x[groupBy].ToString()!)
            .Select(groupped =>
            {
                var firstRecord = groupped.First();
                var customerItem = mainFields.ToDictionary(f => f, f => firstRecord[f]);
                var childrenDataset = groupped
                    .Select(item => item
                        .Where(field => !mainFields.Contains(field.Key)) 
                        .ToDictionary(field => field.Key, field => field.Value)  
                    )
                    .ToList();
                customerItem.Add(cdsName, childrenDataset);
                return customerItem;
            })
            .ToList();
        return results;
    }
}