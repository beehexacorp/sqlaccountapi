using System.Reflection;
using System.Text.Json.Nodes;
using SqlAccountRestAPI.Core;
using SqlAccountRestAPI.ViewModels;

namespace SqlAccountRestAPI.Helpers;

public class SqlAccountingAppHelper
{
    private readonly SqlAccountingORM _microORM;
    private readonly SqlAccountingFactory _factory;

    public SqlAccountingAppHelper(
        SqlAccountingORM microORM,
        SqlAccountingFactory factory)
    {
        _microORM = microORM;
        _factory = factory;
    }

    public SqlAccountingAppInfo GetInfo()
    {
        dynamic app = _factory.GetInstance();
        return new SqlAccountingAppInfo
        {
            Title = app.Title,
            ReleaseDate = app.ReleaseDate,
            BuildNo = app.BuildNo
        };
    }

    public IEnumerable<SqlAccountingModuleInfo> GetModules()
    {
        dynamic app = _factory.GetInstance();
        var result = new List<SqlAccountingModuleInfo>();
        for (int i = 0; i < app.Modules.Count; i++)
        {
            var item = app.Modules.Items(i);
            result.Add(new SqlAccountingModuleInfo
            {
                Code = item.Code,
                Description = item.Description
            });
        }
        return result;
    }

    public IEnumerable<SqlAccountingActionInfo> GetActions()
    {
        dynamic app = _factory.GetInstance();
        var results = new List<SqlAccountingActionInfo>();
        for (int i = 0; i < app.Actions.Count; i++)
        {
            results.Add(new SqlAccountingActionInfo
            {
                Name = app.Actions.Items(i).Name
            });
        }
        return results;
    }

    public IEnumerable<BizObjectInfo> GetBizObjects()
    {
        dynamic app = _factory.GetInstance();
        var results = new List<BizObjectInfo>();
        for (int i = 0; i < app.BizObjects.Count; i++)
        {
            results.Add(new BizObjectInfo
            {
                Name = app.BizObjects.Items(i)
            });
        }
        return results;
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
}
