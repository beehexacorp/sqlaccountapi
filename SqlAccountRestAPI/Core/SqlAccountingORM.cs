namespace SqlAccountRestAPI.Core;
public class SqlAccountingORM
{
    private readonly SqlAccountingFactory _factory;

    public SqlAccountingORM(SqlAccountingFactory factory)
    {
        _factory = factory;
    }

    public void Login(string username = "ADMIN", string password = "ADMIN")
    {
        /** 
        TODO: 
        1. Store the User & Password in an encrypted file
        2. Whenever an application is stopped and restarted, it must re-login using the cached Username & Password
        */
        dynamic app = _factory.GetInstance();

        if (app.IsLogin == true)
        {
            _factory.Release();
            app = _factory.GetInstance();
        }

        app.Login(username, username);
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
        var fields = app.ComServer.DBManager.NewDataSet($@"SELECT * 
FROM AR_CUSTOMER 
OFFSET 0 ROWS
FETCH NEXT 1 ROWS ONLY").Fields;
        var results = new List<string>();
        foreach (var field in fields)
        {
            results.Add(field.FieldName);
        }
        return results;
    }

    public dynamic CreateDataset(string sql, IDictionary<string, object?>? @params = null)
    {
        // TODO: use params
        dynamic app = _factory.GetInstance();
        return app.DBManager.NewDataSet(sql);
    }

    public IDictionary<string, object>? QueryFirstOrDefault(string sql, IDictionary<string, object?>? @params = null)
    {
        // TODO: TEST THIS
        sql = $@"{sql}
OFFSET 0 ROWS
FETCH NEXT 1 ROWS ONLY";
        var dataset = CreateDataset(sql, @params);
        try
        {
            dataset.First();
            if (dataset.eof) // TODO: is this there is no data?
            {
                return null;
            }
            var fields = dataset.Fields;

            var item = new Dictionary<string, object>();
            for (int i = 0; i < fields.Count; i++)
            {
                var datasetField = fields.Items(i);
                item[datasetField.FieldName] = datasetField.value;
            }

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

    public IEnumerable<IDictionary<string, object>> Query(string sql, IDictionary<string, object?>? @params = null, int offset = 0, int limit = 100)
    {
        // TODO: TEST THIS
        sql = $@"{sql}
OFFSET {offset} ROWS
FETCH NEXT {limit} ROWS ONLY";
        var dataset = CreateDataset(sql, @params);
        /// <param name="offset">The number of rows to skip before returning results.</param>
        /// <param name="limit">The maximum number of rows to return.</param>
        /// <returns>A list of dictionaries, where each dictionary represents a row.</returns>
        try
        {
            var results = new List<IDictionary<string, object>>();
            dataset.First();
            while (!dataset.eof) // TODO: what if the dataset has only 1 item?
            {
                var fields = dataset.Fields;

                var item = new Dictionary<string, object>();
                for (int i = 0; i < fields.Count; i++)
                {
                    var datasetField = fields.Items(i);
                    item[datasetField.FieldName] = datasetField.value;
                }
                results.Add(item);
                dataset.Next();
            }

            return results;
        }
        finally
        {
            if (dataset != null)
            {
                System.Runtime.InteropServices.Marshal.ReleaseComObject(dataset);
            }
        }
    }
}