namespace SqlAccountRestAPI.Lib;

public class SqlAccountingBizAppFactory
{
    private dynamic? _app;
    public dynamic CreateApp(bool recreate = false)
    {
        if (_app != null)
        {
            if (!recreate)
            {
                return _app;
            }
            else
            {
                System.Runtime.InteropServices.Marshal.ReleaseComObject(_app);
            }
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
}
