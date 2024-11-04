namespace SqlAccountRestAPI.Lib;

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
        return _bizObject.DataSets.Find("MainDataSet");
    }

    public void New()
    {
        _bizObject.New();
    }

    public void Save()
    {
        _bizObject.Save();
    }
}