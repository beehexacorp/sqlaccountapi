using System.Reflection;
using System.Text.Json.Nodes;
using SqlAccountRestAPI.ViewModels;

namespace SqlAccountRestAPI.Lib;

public class SqlAccountingApp : IDisposable
{
    private dynamic _app;
    private readonly SqlAccountingBizAppFactory _appFactory;

    public SqlAccountingApp(SqlAccountingBizAppFactory factory)
    {
        _appFactory = factory;
        _app = factory.CreateApp();
    }

    public bool IsLoggedIn => _app.IsLogin;

    public void Login(string username = "ADMIN", string password = "ADMIN")
    {
        /** 
        TODO: 
        1. Store the User & Password in an encrypted file
        2. Whenever an application is stopped and restarted, it must re-login using the cached Username & Password
        */

        if (_app.IsLogin)
        {
            _app = _appFactory.CreateApp(true);
        }

        _app.Login(username, username);
    }

    public BizAppInfo GetInfo()
    {
        return new BizAppInfo
        {
            Title = _app.Title,
            ReleaseDate = _app.ReleaseDate,
            BuildNo = _app.BuildNo
        };
    }

    public IEnumerable<ModuleInfo> GetModules()
    {
        var result = new List<ModuleInfo>();
        for (int i = 0; i < _app.Modules.Count; i++)
        {
            var item = _app.Modules.Items(i);
            result.Add(new ModuleInfo
            {
                Code = item.Code,
                Description = item.Description
            });
        }
        return result;
    }

    public IEnumerable<ActionInfo> GetActions()
    {
        var results = new List<ActionInfo>();
        for (int i = 0; i < _app.Actions.Count; i++)
        {
            results.Add(new ActionInfo
            {
                Name = _app.Actions.Items(i).Name
            });
        }
        return results;
    }

    public IEnumerable<BizObjectInfo> GetBizObjects()
    {
        var results = new List<BizObjectInfo>();
        for (int i = 0; i < _app.BizObjects.Count; i++)
        {
            results.Add(new BizObjectInfo
            {
                Name = _app.BizObjects.Items(i)
            });
        }
        return results;
    }

    public void Dispose()
    {
        if (_app == null)
        {
            return;
        }
        System.Runtime.InteropServices.Marshal.ReleaseComObject(_app);
    }

    public object? GetBizObjectInfo(string name)
    {
        /**
        TODO: implement this function
        {
          "name": "...",
          "description": "...",
          "fields": Array<string>
          "cds": ...
        }
        */
        throw new NotImplementedException();
    }

    public SqlAccountingBizObject FindBizObject(string name)
    {
        var result = new SqlAccountingBizObject(_app.BizObjects.Find(name));
        return result;
    }


    public dynamic CreateDataset(string sql, IDictionary<string, object> @params)
    {
        // TODO: use params
        return _app.DBManager.NewDataSet(sql);
    }

    public IDictionary<string, object>? QueryFirstOrDefault(string sql, IDictionary<string, object> @params)
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
    public IEnumerable<IDictionary<string, object>> Query(string sql, IDictionary<string, object> @params, int offset, int limit)
    {
        // TODO: TEST THIS
        sql = $@"{sql}
OFFSET {offset} ROWS
FETCH NEXT {limit} ROWS ONLY";
        var dataset = CreateDataset(sql, @params);
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
