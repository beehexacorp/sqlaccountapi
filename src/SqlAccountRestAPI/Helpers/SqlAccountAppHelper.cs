using System.Reflection;
using System.Text.Json.Nodes;
using SqlAccountRestAPI.Core;
using SqlAccountRestAPI.Helpers;
using SqlAccountRestAPI.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.IO.Compression;

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

    public async Task<SqlAccountingAppInfo> GetInfo()
    {
        dynamic app = _factory.GetInstance();
        var result = new SqlAccountingAppInfo
        {
            Title = app.Title,
            ReleaseDate = app.ReleaseDate.ToString("yyyy-MM-dd"),
            BuildNo = app.BuildNo.ToString()
        };
        var npmFolder = SystemHelper.RunPowerShellCommand("npm -g root");
        var configPath = Path.Combine(npmFolder, ApplicationConstants.NPM_PACKAGE_NAME,
            ApplicationConstants.CONFIGURATION_FOLDER_NAME, ApplicationConstants.CONFIGURATION_FILE_NAME);
        if (File.Exists(configPath))
        {
            try
            {
                string fileContent = File.ReadAllText(configPath);

                // Parse JSON to Dictionary
                var applicationInfo = JsonSerializer.Deserialize<Dictionary<string, object>>(fileContent);
                var releaseInfo = await GithubHelper.GetLatestReleaseInfo();
                applicationInfo!["LATEST_VERSION"] = releaseInfo["tag_name"];
                result.ApplicationInfo = applicationInfo;
            }
            catch (Exception ex)
            {
                result.ApplicationInfo = new Dictionary<string, object>
                {
                    { "Error", ex.Message }
                };
            }
        }
        else
        {
            result.ApplicationInfo = new Dictionary<string, object>
                {
                    { "Error", "The configuration file does not exist." }
                };
        }
        return result;
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
        // alternative
        {
          "name": "...",
          "datasets": [
            {
              "name": "...",
              "fields": Array<string>
            },
            {
              "name": "...",
              "fields": Array<string>
            },...
          ]      
        
        }
        */
        var result = new Dictionary<string, object?>
        {
            { "name", name }
        };
        var datasetList = new List<Dictionary<string, object?>>();
        dynamic app = _factory.GetInstance();
        var datasets = app.BizObjects.Find(name).Datasets;

        for (int i = 0; i < datasets.Count; i++)
        {
            var dataset = datasets.Items(i);
            var fields = _microORM.FieldIterator(dataset.Fields);
            var datasetData = new Dictionary<string, object?>
            {
                { "name", dataset.Name },
                { "fields", fields }
            };
            datasetList.Add(datasetData);
        }
        result.Add("datasets", datasetList);
        return result;

        throw new NotImplementedException();
    }
}
