using static System.Runtime.InteropServices.JavaScript.JSType;
using System;
using System.Text.Json.Nodes;
using SqlAccountRestAPI.Models;

namespace SqlAccountRestAPI.Lib
{
    public class SalesInvoice
    {
        private SqlComServer app;
        public SalesInvoice(SqlComServer comServer)
        {
            if (comServer == null) throw new Exception("Sql Accounting is not running");
            app = comServer;
        }

        public Order? Order { get; set; }

        //public JsonObject? OrderData { get; set; }

        //public Array? OrderItems { get; set; }

        /// <summary>
        /// Add a new Sales Invoice
        /// </summary>
        /// <param name="OrderData"></param>
        /// <param name="orderItems">Array of JsonObject</param>
        /// <exception cref="Exception"></exception>
        public void AddSalesInvoice()
        {
            if(Order == null) throw new Exception("OrderData is required");
            if (Order.Items == null) throw new Exception("orderItems are required");
            //if (Order.Items.ItemCode == null) throw new Exception("ItemCode is required");

            //app = new SqlComServer();

            var IvBizObj = app.ComServer.BizObjects.Find("SL_IV");
            var lMainDataSet = IvBizObj.DataSets.Find("MainDataSet");
            var lDetailDataSet = IvBizObj.DataSets.Find("cdsDocDetail");
            IvBizObj.New();

            /* update main order data */
            lMainDataSet.FindField("DocKey").value = -1;
            lMainDataSet.FindField("DocNo").value = Order.DocNo;
            lMainDataSet.FindField("DocDate").value = Order.DocDate;
            lMainDataSet.FindField("PostDate").value = Order.PostDate;
            lMainDataSet.FindField("Code").value = Order.Code;
            lMainDataSet.FindField("CompanyName").value = Order.CompanyName;
            lMainDataSet.FindField("Address1").value = Order.Address1;
            lMainDataSet.FindField("Address2").value = Order.Address2;
            lMainDataSet.FindField("Address3").value = Order.Address3;
            lMainDataSet.FindField("Address4").value = Order.Address4;
            lMainDataSet.FindField("Agent").value = Order.Agent;
            lMainDataSet.FindField("Terms").value = Order.Terms;
            lMainDataSet.FindField("CurrencyCode").value = Order.CurrencyCode;
            lMainDataSet.FindField("CurrencyRate").value = Order.CurrencyRate;
            lMainDataSet.FindField("Description").value = Order.Description;
            lMainDataSet.FindField("DocAmt").value = Order.DocAmt;
            lMainDataSet.FindField("LocalDocAmt").value = Order.LocalDocAmt;
            lMainDataSet.FindField("D_Amount").value = Order.D_Amount;

            /* order items */

            for (int i = 0; i < Order.Items.Count; i++)
            {
                var item = Order.Items[i];
                if (item == null) throw new Exception("orderItem is required");
                //if (Order.Items[i].ItemCode == null) throw new Exception("ItemCode is required");

                lDetailDataSet.Append();
                lDetailDataSet.FindField("DtlKey").value = -1;
                lDetailDataSet.FindField("DocKey").value = -1;
                lDetailDataSet.FindField("ItemCode").value = item.ItemCode;
                lDetailDataSet.FindField("Location").value = item.Location;
                lDetailDataSet.FindField("Description").value = item.Description;
                lDetailDataSet.FindField("Qty").value = item.Quantity;
                lDetailDataSet.FindField("UOM").value = item.UOM;
                lDetailDataSet.FindField("UnitPrice").value = item.UniPrice;
                //lDetailDataSet.FindField("Disc").value = item.Disc;
                lDetailDataSet.FindField("Amount").value = item.Amount;
                lDetailDataSet.FindField("Tax").value = item.Tax;
                lDetailDataSet.FindField("TaxRate").value = item.TaxRate;
                lDetailDataSet.FindField("TaxAmt").value = item.TaxAmt;
                //lDetailDataSet.FindField("Amount").value = item.Amount;
                lDetailDataSet.Post();
            }

            // save all order information
            IvBizObj.Save();
        }

        public void LoadOrder(string DocNo)
        {
            if (DocNo == null) throw new Exception("DocNo is required");
            if (DocNo == string.Empty) throw new Exception("DocNo is required");
            

            var IvBizObj = app.ComServer.BizObjects.Find("SL_IV");
            IvBizObj.New();

            /* get document key based on lDocNo */
            var lDocKey = IvBizObj.FindKeyByRef("DocNo", DocNo);

            /* before Biz Object opened, assigned param - DocKey to Biz Object */
            IvBizObj.Params.Find("DocKey").Value = lDocKey;

            /* if lDocKey has value */
            if (lDocKey != null)
            {
                var lMainDataSet = IvBizObj.DataSets.Find("MainDataSet");
                var lDetailDataSet = IvBizObj.DataSets.Find("cdsDocDetail");

                IvBizObj.Open();

                if (Order == null) Order = new Order();


                Order.DocNo = lMainDataSet.FindField("DocNo").value;
                Order.DocDate = lMainDataSet.FindField("DocDate").value;
                Order.PostDate = lMainDataSet.FindField("PostDate").value;
                Order.Code = lMainDataSet.FindField("Code").value;
                Order.CompanyName = lMainDataSet.FindField("CompanyName").value;
                Order.Address1 = lMainDataSet.FindField("Address1").value;
                Order.Address2 = lMainDataSet.FindField("Address2")?.value;
                //Order.Address3 = lMainDataSet.FindField("Address3")?.value;
                //Order.Address4 = lMainDataSet.FindField("Address4")?.value;

                Order.Agent = lMainDataSet.FindField("Agent").value;
                Order.Terms = lMainDataSet.FindField("Terms").value;
                Order.CurrencyCode = lMainDataSet.FindField("CurrencyCode").value;
                Order.CurrencyRate = lMainDataSet.FindField("CurrencyRate").value;
                Order.Description = lMainDataSet.FindField("Description").value;
                Order.LocalDocAmt = double.Parse(lMainDataSet.FindField("LocalDocAmt").AsString);
                Order.DocAmt = double.Parse(lMainDataSet.FindField("DocAmt").AsString); 
                Order.D_Amount = double.Parse(lMainDataSet.FindField("D_Amount").AsString);
                Order.Cancelled = lMainDataSet.Findfield("Cancelled").value;

                /* load Detail record */
                /* move to first record */
                lDetailDataSet.First();
                var i = 0;

                while (!lDetailDataSet.eof)
                {
                    var orderItem = new OrderItem();

                    /* retrieve Detail DataSet record */
                    orderItem.ItemCode = lDetailDataSet.FindField("ItemCode").value;
                    orderItem.Description = lDetailDataSet.FindField("Description").value;
                    orderItem.Location = lDetailDataSet.FindField("Location").value;
                    orderItem.Quantity = double.Parse(lDetailDataSet.FindField("Qty").AsString);
                    orderItem.UOM = lDetailDataSet.FindField("UOM").value;
                    orderItem.UniPrice = double.Parse(lDetailDataSet.FindField("UnitPrice").AsString);
                    //orderItem.Disc = double.Parse(lDetailDataSet.FindField("Disc")?.AsString);
                    orderItem.Amount = double.Parse(lDetailDataSet.FindField("Amount").AsString);
                    //orderItem.Tax = lDetailDataSet.FindField("Tax")?.value;
                    //orderItem.TaxRate = lDetailDataSet.FindField("TaxRate")?.value;
                    //orderItem.TaxAmt = double.Parse(lDetailDataSet.FindField("TaxAmt")?.AsString);
                    //orderItem.TaxInclusive = double.Parse(lDetailDataSet.FindField("TaxInclusive")?.AsString);

                    if (Order.Items == null) Order.Items = new List<OrderItem>(); 

                    Order.Items.Add(orderItem);

                    /* next record */
                    lDetailDataSet.Next();
                    i++;
                }
            }
        }
    }
}
