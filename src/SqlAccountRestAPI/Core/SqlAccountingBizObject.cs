using Microsoft.VisualBasic;

namespace SqlAccountRestAPI.Core;

public class SqlAccountingBizObject : IDisposable
{
    private dynamic _bizObject;

    public SqlAccountingBizObject(dynamic bizObject)
    {
        this._bizObject = bizObject;
    }

    public void Dispose()
    {

        if (_bizObject == null)
        {
            return;
        }
        System.Runtime.InteropServices.Marshal.ReleaseComObject(_bizObject);
    }

    public dynamic FindDataset(string datasetName)
    {
        return _bizObject.DataSets.Find(datasetName);
    }

    public dynamic FindMainDataset()
    {
        return _bizObject.DataSets.Find("MainDataSet");
    }
    public dynamic FindKeyByRef(string keyName, string keyValue)
    {
        return _bizObject.FindKeyByRef(keyName, keyValue);
    }

    public void New()
    {
        _bizObject.New();
    }

    public void Open()
    {
        _bizObject.Open();
    }
    public void Edit()
    {
        _bizObject.Edit();
    }
    public void Params(string fieldKey, string fieldValue)
    {
        var lDocKey = _bizObject.FindKeyByRef(fieldKey, fieldValue);
        _bizObject.Params.Find("DocKey").Value = lDocKey;
    }
    public void Find(string entityType)
    {
        _bizObject.Find(entityType);
    }

    public void Save()
    {
        _bizObject.Save();
    }
}