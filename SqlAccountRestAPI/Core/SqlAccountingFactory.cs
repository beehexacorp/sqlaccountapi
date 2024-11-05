namespace SqlAccountRestAPI.Core;

public class SqlAccountingFactory : IDisposable
{
    private dynamic? _app = null;
    public dynamic GetInstance()
    {
        if (_app != null)
        {
            return _app;
        }

        var lBizType = Type.GetTypeFromProgID("SQLAcc.BizApp");

        if (lBizType == null)
        {
            throw new Exception("Cannot load SQLAcc.BizApp Assembly");
        }

        _app = Activator.CreateInstance(lBizType);

        if (_app == null)
        {
            throw new Exception("Cannot create instance of SQLAcc.BizApp");
        }
        return _app!;
    }

    public void Dispose()
    {
        if (_app != null)
        {
            System.Runtime.InteropServices.Marshal.ReleaseComObject(_app);
        }
    }

    public void Release()
    {
        System.Runtime.InteropServices.Marshal.ReleaseComObject(_app);
        _app = null;
    }
}
