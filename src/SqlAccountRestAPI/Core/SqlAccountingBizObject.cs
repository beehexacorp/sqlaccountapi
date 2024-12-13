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
    public dynamic FindKeyByRef(string keyName, string keyValue){
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
    public dynamic Params(string fieldKey)
    {
       var lMain = _bizObject.DataSets.Find("MainDataSet");  //lMain contains master data
           var lDtl = _bizObject.DataSets.Find("cdsUOM");
        var lDocKey = _bizObject.FindKeyByRef("CODE", "E-BAT");
        var paramsTest = _bizObject.Params;
        // var field
        _bizObject.Params.Find("CODE").Value = lDocKey;
        return _bizObject.Params.Find(fieldKey);
    }
    public void Find(string entityType){
        _bizObject.Find(entityType);
    }

    public void Save()
    {
        _bizObject.Save();
    }
}