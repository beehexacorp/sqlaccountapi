using System.Text.Json.Nodes;

namespace SqlAccountRestAPI.Lib
{
    public class SqlComServer
    {
        Int32 lBuildNo;
        public dynamic ComServer;
        Type lBizType;

        public SqlComServer()
        {
            lBizType = Type.GetTypeFromProgID("SQLAcc.BizApp");
            ComServer = Activator.CreateInstance(lBizType);
            if (!ComServer.IsLogin)
            {            
                /* check whether user has logon */
                ComServer.Login("ADMIN", "ADMIN");
            }
        }

        void Login()
        {
            if (!ComServer.IsLogin)
            {
                ComServer.Login("ADMIN", "ADMIN");
            }
        }

        public JsonObject GetAppInfo()
        {
            var json = new JsonObject();
            json["Title"] = ComServer.Title;
            json["ReleaseDate"] = ComServer.ReleaseDate;
            json["BuildNo"] = ComServer.BuildNo;
            return json;
        }

        public JsonArray GetModules()
        {
            var arr = new JsonArray();
            for (int i = 0; i < ComServer.Modules.Count; i++)
            {
                var obj = new JsonObject();
                obj["Code"] = ComServer.Modules.Items(i).Code;
                obj["Description"] = ComServer.Modules.Items(i).Description;    
                arr.Add(obj);
            }
            return arr;
        }

        public JsonArray GetActions()
        {
            var arr = new JsonArray();
            for (int i = 0; i < ComServer.Actions.Count; i++)
            {
                var obj = new JsonObject();
                obj["Name"] = ComServer.Actions.Items(i).Name;
                arr.Add(obj);
            }
            return arr;
        }

        public JsonArray GetBizObjects() {            
            var arr = new JsonArray();
            for (int i = 0; i < ComServer.BizObjects.Count; i++)
            {
                var obj = new JsonObject();
                obj["Name"] = ComServer.BizObjects.Items(i);
                arr.Add(obj);
            }
            return arr;
        }
    }
}
