using static System.Runtime.InteropServices.JavaScript.JSType;
using System;
using System.Text.Json.Nodes;

namespace SqlAccountRestAPI.Lib
{
    public class SalesInvoice
    {
        private SqlComServer? App { get; set; }

        public JsonObject? OrderData { get; set; }

        public Array? OrderItems { get; set; }

        /// <summary>
        /// Add a new Sales Invoice
        /// </summary>
        /// <param name="OrderData"></param>
        /// <param name="orderItems">Array of JsonObject</param>
        /// <exception cref="Exception"></exception>
        public void AddSalesInvoice()
        {
            if(OrderData == null) throw new Exception("OrderData is required");
            if (OrderItems == null) throw new Exception("orderItems are required");

            App = new SqlComServer();

            var IvBizObj = App.ComServer.BizObjects.Find("SL_IV");
            var lMainDataSet = IvBizObj.DataSets.Find("MainDataSet");
            var lDetailDataSet = IvBizObj.DataSets.Find("cdsDocDetail");
            IvBizObj.New();

            /* update main order data */
            lMainDataSet.FindField("DocKey").value = -1;
            lMainDataSet.FindField("DocNo").value = OrderData["DocNo"]?.GetValue<string>();
            lMainDataSet.FindField("DocType").value = OrderData["DocType"]?.GetValue<string>();  
            lMainDataSet.FindField("DocDate").value = OrderData["DocDate"]?.GetValue<string>();
            lMainDataSet.FindField("PostDate").value = OrderData["DocDate"]?.GetValue<string>();
            lMainDataSet.FindField("Code").value = OrderData["Code"]?.GetValue<string>();
            lMainDataSet.FindField("CompanyName").value = OrderData["CompanyName"]?.GetValue<string>();
            lMainDataSet.FindField("Address1").value = OrderData["Address1"]?.GetValue<string>();
            lMainDataSet.FindField("Address2").value = OrderData["Address2"]?.GetValue<string>();
            lMainDataSet.FindField("Address3").value = OrderData["Address3"]?.GetValue<string>();
            lMainDataSet.FindField("Address4").value = OrderData["Address4"]?.GetValue<string>();
            lMainDataSet.FindField("Agent").value = OrderData["Agent"]?.GetValue<string>();
            lMainDataSet.FindField("Terms").value = OrderData["Terms"]?.GetValue<string>();
            lMainDataSet.FindField("CurrencyCode").value = OrderData["CurrencyCode"]?.GetValue<string>();
            lMainDataSet.FindField("CurrencyRate").value = OrderData["CurrencyRate"]?.GetValue<string>();
            lMainDataSet.FindField("Description").value = OrderData["Description"]?.GetValue<string>();
            lMainDataSet.FindField("DocAmt").value = OrderData["DocAmt"]?.GetValue<string>();
            lMainDataSet.FindField("LocalDocAmt").value = OrderData["LocalDocAmt"]?.GetValue<string>();
            lMainDataSet.FindField("D_Amount").value = OrderData["D_Amount"]?.GetValue<string>();

            /* order items */

            for (int i = 0; i < OrderItems.Length; i++)
            {
                var item = OrderItems.GetValue(i) as JsonObject;
                if (item == null) throw new Exception("orderItem is required");

                lDetailDataSet.Append();
                lDetailDataSet.FindField("DtlKey").value = -1;
                lDetailDataSet.FindField("DocKey").value = -1;
                lDetailDataSet.FindField("ItemCode").value = item["ItemCode"]?.GetValue<string>();
                lDetailDataSet.FindField("Location").value = item["ItemCode"]?.GetValue<string>();
                lDetailDataSet.FindField("Description").value = item["ItemCode"]?.GetValue<string>();
                lDetailDataSet.FindField("Qty").value = item["ItemCode"]?.GetValue<string>();
                lDetailDataSet.FindField("UOM").value = item["ItemCode"]?.GetValue<string>();
                lDetailDataSet.FindField("UnitPrice").value = item["ItemCode"]?.GetValue<string>();
                lDetailDataSet.FindField("Disc").value = item["ItemCode"]?.GetValue<string>();
                lDetailDataSet.FindField("Amount").value = item["ItemCode"]?.GetValue<string>();
                lDetailDataSet.Post();
            }

            // save all order information
            IvBizObj.Save();
        }

        public void ParseJsonOrder(string jsonData)
        {

        }
    }
}
