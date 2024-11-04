namespace SqlAccountRestAPI.Lib;

public class SqlAccountingBizAppFactory
{
    public dynamic Create()
    {

        var lBizType = Type.GetTypeFromProgID("SQLAcc.BizApp");

        if (lBizType == null)
        {
            throw new Exception("Cannot load SQLAcc.BizApp Assembly");
        }

        var app = Activator.CreateInstance(lBizType);

        if (app == null)
        {
            throw new Exception("Cannot create instance of SQLAcc.BizApp");
        }
        return app;
    }
}
