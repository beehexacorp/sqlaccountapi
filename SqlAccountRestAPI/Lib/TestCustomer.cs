using static System.Runtime.InteropServices.JavaScript.JSType;
using System;
using System.Text.Json.Nodes;
using SqlAccountRestAPI.Models;
using System.Xml;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.Json;
using System.ComponentModel.DataAnnotations;

namespace SqlAccountRestAPI.Lib
{
    public class TestCustomer
    {

        private SqlComServer app;
        public TestCustomer(SqlComServer comServer)
        {
            if (comServer == null) throw new Exception("Sql Accounting is not running");
            app = comServer;
        }
        public void run(JObject query)
        {
            var bizObj = app.ComServer.BizObjects.Find("ST_AJ");
            var dataset = bizObj.DataSets.Find("cdsDocDetail");
            Console.WriteLine(dataset.Fields.Count);
            dataset.Open();
            dataset.First();
            while(!dataset.eof){
                Console.WriteLine(dataset.FindField("ITEMCODE").value);
                dataset.Next();
            }

            Type comType = Type.GetTypeFromProgID("SQLAcc.BizApp");
            dynamic comObject = Activator.CreateInstance(comType);

            if (!comObject.IsLogin)
            {
                comObject.Login("ADMIN", "ADMIN");
                Console.WriteLine("Login successful!");
            }
            else
            {
                Console.WriteLine("Already logged in!");
            }

            var modules = comObject.BizObjects;
            Console.WriteLine(modules.Count);
            for(int i=0; i<modules.Count; i++){
                // Console.WriteLine(modules.Items(i));
                // Console.WriteLine(modules.Items(i).Value);
                // Console.WriteLine("----");
            }

            dynamic lSQL = "SELECT * FROM "+query["dataset"];
            dynamic lMain = app.ComServer.DBManager.NewDataSet(lSQL);

            try{
                lSQL = "SELECT * FROM "+query["dataset"];
                lMain = app.ComServer.DBManager.NewDataSet(lSQL);
            }
            catch {
                
            }

            Console.WriteLine("hello----------------------------");
            var Fields = lMain.Fields;
            for (int i = 0; i < Fields.Count; i++)
            {
                var lField = Fields.Items(i);
                var key = lField.FieldName;
                var value = lField.value;
                Console.WriteLine(key+"---"+value);
            }

           
        }
        public void TryFindTable(string prefix, string suffix)
        {
            string FIND = prefix + suffix;

            // Thử truy vấn với tên bảng hiện tại
            try
            {
                string lSQL = "SELECT * FROM " + FIND;
                var lMain = app.ComServer.DBManager.NewDataSet(lSQL); // Giả sử đây là cách bạn truy vấn
                Console.WriteLine("Found table: " + FIND);
            }
            catch
            {
                // Nếu lỗi, tiếp tục thử các ký tự khác
                if (suffix.Length < 5) // Giới hạn độ dài tối đa để tránh chạy vô hạn
                {
                    for (char c = 'A'; c <= 'Z'; c++)
                    {
                        TryFindTable(prefix, suffix + c); // Gọi đệ quy với thêm một ký tự
                    }
                }
            }
        }


    }
}