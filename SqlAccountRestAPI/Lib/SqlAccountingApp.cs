using System.Reflection;
using System.Text.Json.Nodes;
using SqlAccountRestAPI.ViewModels;

namespace SqlAccountRestAPI.Lib;

public class SqlAccountingApp : IDisposable
{
    private dynamic _sqlAccountingBizApp;
    private readonly SqlAccountingBizAppFactory _appFactory;

    public SqlAccountingApp(SqlAccountingBizAppFactory factory)
    {
        _appFactory = factory;
        _sqlAccountingBizApp = factory.Create();
    }

    public bool IsLoggedIn => _sqlAccountingBizApp.IsLogin;

    public void Login(string username = "ADMIN", string password = "ADMIN")
    {
        /** 
        TODO: 
        1. Store the User & Password in an encrypted file
        2. Whenever an application is stopped and restarted, it must re-login using the cached Username & Password
        */

        if (_sqlAccountingBizApp.IsLogin)
        {
            System.Runtime.InteropServices.Marshal.ReleaseComObject(_sqlAccountingBizApp);
            _sqlAccountingBizApp = _appFactory.Create();
        }

        _sqlAccountingBizApp.Login(username, username);
    }

    public BizAppInfo GetInfo()
    {
        return new BizAppInfo
        {
            Title = _sqlAccountingBizApp.Title,
            ReleaseDate = _sqlAccountingBizApp.ReleaseDate,
            BuildNo = _sqlAccountingBizApp.BuildNo
        };
    }

    public IEnumerable<ModuleInfo> GetModules()
    {
        var result = new List<ModuleInfo>();
        for (int i = 0; i < _sqlAccountingBizApp.Modules.Count; i++)
        {
            var item = _sqlAccountingBizApp.Modules.Items(i);
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
        for (int i = 0; i < _sqlAccountingBizApp.Actions.Count; i++)
        {
            results.Add(new ActionInfo
            {
                Name = _sqlAccountingBizApp.Actions.Items(i).Name
            });
        }
        return results;
    }

    public IEnumerable<BizObjectInfo> GetBizObjects()
    {
        var results = new List<BizObjectInfo>();
        for (int i = 0; i < _sqlAccountingBizApp.BizObjects.Count; i++)
        {
            results.Add(new BizObjectInfo
            {
                Name = _sqlAccountingBizApp.BizObjects.Items(i)
            });
        }
        return results;
    }

    public void Dispose()
    {
        if (_sqlAccountingBizApp == null)
        {
            return;
        }
        System.Runtime.InteropServices.Marshal.ReleaseComObject(_sqlAccountingBizApp);
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

        var result = new SqlAccountingBizObject(_sqlAccountingBizApp.BizObjects.Find(name));
        return result;
    }

    public dynamic CreateDataset(string sql)
    {
        return _sqlAccountingBizApp.DBManager.NewDataSet(sql);
    }

    public IDictionary<string, object> QueryFirstOrDefault(string sql, IDictionary<string, object> @params)
    {
        // TODO: TEST THIS
        sql = $@"{sql}
OFFSET 0 ROWS
FETCH NEXT 1 ROWS ONLY";
        var dataset = CreateDataset(sql);
        try
        {
            // TODO: what if there is no data found? please add if/else check to return null
            dataset.First();
            var fields = dataset.Fields;

            var item = new Dictionary<string, object>();
            for (int i = 0; i < fields.Count; i++)
            {
                var lField = fields.Items(i);
                item[lField.FieldName] = lField.value;
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
        var dataset = CreateDataset(sql);
        try
        {
            var results = new List<IDictionary<string, object>>();
            dataset.First();
            while (!dataset.eof)
            {
                var fields = dataset.Fields;

                var item = new Dictionary<string, object>();
                for (int i = 0; i < fields.Count; i++)
                {
                    var lField = fields.Items(i);
                    item[lField.FieldName] = lField.value;
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
