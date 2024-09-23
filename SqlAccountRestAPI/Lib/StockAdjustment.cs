using static System.Runtime.InteropServices.JavaScript.JSType;
using System;
using System.Text.Json.Nodes;
using SqlAccountRestAPI.Models;

namespace SqlAccountRestAPI.Lib
{
    public class StockAdjustment
    {
        private SqlComServer app;
        public StockAdjustment(SqlComServer comServer)
        {
            if (comServer == null) throw new Exception("Sql Accounting is not running");
            app = comServer;
        }

        public Stock? Stock { get; set; }

        //public JsonObject? ItemData { get; set; }

        //public Array? ItemItems { get; set; }

        public void GetStockAdjustment(string DocNo)
        {
            if (DocNo == null) throw new Exception("DocNo is required");
            if (DocNo == string.Empty) throw new Exception("DocNo is required");


            var IvBizObj = app.ComServer.BizObjects.Find("ST_AJ");
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

                if (Stock == null) Stock = new Stock();


                Stock.DocNo = lMainDataSet.FindField("DocNo").value;
                Stock.DocDate = lMainDataSet.FindField("DocDate").value;
                Stock.PostDate = lMainDataSet.FindField("PostDate").value;
                //StockAdjustment.Code = lMainDataSet.FindField("Code").value;
                //StockAdjustment.CompanyName = lMainDataSet.FindField("CompanyName").value;
                //StockAdjustment.Address1 = lMainDataSet.FindField("Address1").value;
                //StockAdjustment.Address2 = lMainDataSet.FindField("Address2")?.value;
                //StockAdjustment.Address3 = lMainDataSet.FindField("Address3")?.value;
                //StockAdjustment.Address4 = lMainDataSet.FindField("Address4")?.value;
                //StockAdjustment.Agent = lMainDataSet.FindField("Agent").value;
                //StockAdjustment.Terms = lMainDataSet.FindField("Terms").value;
                //StockAdjustment.CurrencyCode = lMainDataSet.FindField("CurrencyCode").value;
                //StockAdjustment.CurrencyRate = lMainDataSet.FindField("CurrencyRate").value;
                Stock.Description = lMainDataSet.FindField("Description").value;
                //StockAdjustment.LocalDocAmt = double.Parse(lMainDataSet.FindField("LocalDocAmt").AsString);
                //StockAdjustment.DocAmt = double.Parse(lMainDataSet.FindField("DocAmt").AsString);
                //StockAdjustment.D_Amount = double.Parse(lMainDataSet.FindField("D_Amount").AsString);
                //StockAdjustment.Cancelled = lMainDataSet.Findfield("Cancelled").value;

                /* load Detail record */
                /* move to first record */
                lDetailDataSet.First();
                var i = 0;

                while (!lDetailDataSet.eof)
                {
                    var stockItem = new StockItems();

                    /* retrieve Detail DataSet record */
                    stockItem.ItemCode = lDetailDataSet.FindField("ItemCode").value;
                    stockItem.Description = lDetailDataSet.FindField("Description").value;
                    stockItem.Location = lDetailDataSet.FindField("Location").value;
                    //stockItem.Project = double.Parse(lDetailDataSet.FindField("Project").AsString);
                    stockItem.Qty = double.Parse(lDetailDataSet.FindField("Qty").AsString);
                    stockItem.UOM = lDetailDataSet.FindField("UOM").value;
                    stockItem.UnitCost = double.Parse(lDetailDataSet.FindField("UnitCost").AsString);
                    stockItem.Amount = double.Parse(lDetailDataSet.FindField("Amount").AsString);

                    if (Stock.Itemss == null) Stock.Itemss = new List<StockItems>();

                    Stock.Itemss.Add(stockItem);

                    /* next record */
                    lDetailDataSet.Next();
                    i++;
                }
            }
        }
    }
}
