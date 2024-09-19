using static System.Runtime.InteropServices.JavaScript.JSType;
using System;
using System.Text.Json.Nodes;
using SqlAccountRestAPI.Models;
using System.Xml;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SqlAccountRestAPI.Lib
{
    public class StockItem
    {
        private SqlComServer app;
        public StockItem(SqlComServer comServer)
        {
            if (comServer == null) throw new Exception("Sql Accounting is not running");
            app = comServer;
        }

        public Item? Item { get; set; }

        //public JsonObject? ItemData { get; set; }

        //public Array? ItemItems { get; set; }

        //public void LoadStockItem()
        //{
        //    //if (DocNo == null) throw new Exception("DocNo is required");
        //    //if (DocNo == string.Empty) throw new Exception("DocNo is required");


        //    var IvBizObj = app.ComServer.BizObjects.Find("ST_ITEM");
        //    IvBizObj.New();

        //    /* get document key based on lDocNo */
        //    //var lDocKey = IvBizObj.FindKeyByRef("DocNo", DocNo);

        //    /* before Biz Object opened, assigned param - DocKey to Biz Object */
        //    //IvBizObj.Params.Find("DocKey").Value = lDocKey;

        //    /* if lDocKey has value */
        //    //if (lDocKey != null)
        //    //{
        //        var lMainDataSet = IvBizObj.DataSets.Find("MainDataSet");
        //        //var lDetailDataSet = IvBizObj.DataSets.Find("cdsDocDetail");

        //        IvBizObj.Open();

        //        if (Item == null) Item = new Item(); // ?????


        //        //Item.DocKey = lMainDataSet.FindField("DocKey").value;
        //        Item.Code = lMainDataSet.FindField("Code").value;
        //        Item.Description = lMainDataSet.FindField("Description").value;
        //        //Item.Stockgroup = lMainDataSet.FindField("Stockgroup").value;
        //        //Item.Stockcontrol = lMainDataSet.FindField("Stockcontrol").value;
        //        //Item.Costingmethod = lMainDataSet.FindField("Costingmethod").value;
        //        //Item.Serialnumber = lMainDataSet.FindField("Serialnumber").value;
        //        //Item.Remark1 = lMainDataSet.FindField("Remark1").value;
        //        //Item.Remark2 = lMainDataSet.FindField("Remark2").value;
        //        //Item.Minqty = double.Parse(lMainDataSet.FindField("Minqty").AsString);
        //        //Item.Maxqty = double.Parse(lMainDataSet.FindField("Maxqty").AsString);
        //        //Item.Reorderlevel = double.Parse(lMainDataSet.FindField("Reorderlevel").AsString);
        //        //Item.Reorderqty = double.Parse(lMainDataSet.FindField("Reorderqty").AsString);
        //        //Item.Shelf = lMainDataSet.FindField("Shelf").value;
        //        //Item.Itemtype = lMainDataSet.FindField("Itemtype").value;
        //        //Item.Isactive = lMainDataSet.FindField("Isactive").value;
        //        //Item.Balsqty = double.Parse(lMainDataSet.FindField("Balsqty").AsString);

        //        //Item.Balsuomqty = double.Parse(lMainDataSet.FindField("Balsuomqty").AsString);
        //        //Item.Note = lMainDataSet.FindField("Note").value;
        //        //Item.Refprice = double.Parse(lMainDataSet.FindField("Refprice").AsString);
        //        //Item.Refcost = double.Parse(lMainDataSet.FindField("Refcost").AsString);
        //        //Item.Barcode = lMainDataSet.FindField("Barcode").value;
        //        //Item.Creationdate = lMainDataSet.FindField("Creationdate").value;
        //        //Item.Lastmodified = lMainDataSet.FindField("Lastmodified").value;

        //    /* load Detail record */
        //    /* move to first record */

        //    //}
        //}
        public List<Item> LoadAllStockItems()
        {
            var stockItems = new List<Item>();
            var IvBizObj = app.ComServer.BizObjects.Find("ST_ITEM");
            IvBizObj.New();

            var lMainDataSet = IvBizObj.DataSets.Find("MainDataSet");

            IvBizObj.Open();

            lMainDataSet.First(); // Di chuyển con trỏ đến bản ghi đầu tiên

            while (!lMainDataSet.Eof())
            {
                var item = new Item
                {
                    Code = lMainDataSet.FindField("Code").value != DBNull.Value ? lMainDataSet.FindField("Code").value.ToString() : string.Empty,
                    Description = lMainDataSet.FindField("Description").value != DBNull.Value ? lMainDataSet.FindField("Description").value.ToString() : string.Empty,
                };

                stockItems.Add(item);
                lMainDataSet.Next(); // Di chuyển con trỏ đến bản ghi tiếp theo
            }

            return stockItems;
        }
        //public void LoadStockItem() //success nhưng null
        //{
        //    var IvBizObj = app.ComServer.BizObjects.Find("ST_ITEM");
        //    IvBizObj.New();

        //    var lMainDataSet = IvBizObj.DataSets.Find("MainDataSet");

        //    IvBizObj.Open();

        //    if (Item == null) Item = new Item();

        //    Item.Code = lMainDataSet.FindField("Code").value != DBNull.Value ? lMainDataSet.FindField("Code").AsString() : null;
        //    Item.Description = lMainDataSet.FindField("Description").value != DBNull.Value ? lMainDataSet.FindField("Description").AsString() : null;
        //    //Item.Description = lMainDataSet.FindField("Description").value != DBNull.Value ? lMainDataSet.FindField("Description").AsString() : null;
        //    //Item.Code = lMainDataSet.FindField("Code").value;
        //    //Item.Description = lMainDataSet.FindField("Description").value; Dockey
        //}
        public void LoadStockItemByCode(string Code)
        {
            if (Code == null) throw new Exception("Code is required");
            if (Code == string.Empty) throw new Exception("Code is required");


            var IvBizObj = app.ComServer.BizObjects.Find("ST_ITEM");
            IvBizObj.New();

            /* get document key based on lDocNo */
            var lDocKey = IvBizObj.FindKeyByRef("Code", Code);

            /* before Biz Object opened, assigned param - DocKey to Biz Object */
            IvBizObj.Params.Find("DocKey").Value = lDocKey;

            /* if lDocKey has value */
            if (lDocKey != null)
            {
                var lMainDataSet = IvBizObj.DataSets.Find("MainDataSet");

                IvBizObj.Open();

                if (Item == null) Item = new Item();


                Item.Code = lMainDataSet.FindField("Code").value;
                Item.Description = lMainDataSet.FindField("Description").value;
                Item.Stockgroup = lMainDataSet.FindField("Stockgroup").value;
                Item.Stockcontrol = lMainDataSet.FindField("Stockcontrol").value;
                //Item.Costingmethod = lMainDataSet.FindField("Costingmethod").value;
                //Item.Serialnumber = lMainDataSet.FindField("Serialnumber").value;
                //Item.Remark1 = lMainDataSet.FindField("Remark1").value;
                //Item.Remark2 = lMainDataSet.FindField("Remark2").value;
                Item.Minqty = double.Parse(lMainDataSet.FindField("Minqty").AsString);
                Item.Maxqty = double.Parse(lMainDataSet.FindField("Maxqty").AsString);
                Item.Reorderlevel = double.Parse(lMainDataSet.FindField("Reorderlevel").AsString);
                Item.Reorderqty = double.Parse(lMainDataSet.FindField("Reorderqty").AsString);
                //Item.Shelf = lMainDataSet.FindField("Shelf").value;
                Item.Itemtype = lMainDataSet.FindField("Itemtype").value;
                Item.Isactive = lMainDataSet.FindField("Isactive").value;
                Item.Balsqty = double.Parse(lMainDataSet.FindField("Balsqty").AsString);

                Item.Balsuomqty = double.Parse(lMainDataSet.FindField("Balsuomqty").AsString);
                Item.Refprice = double.Parse(lMainDataSet.FindField("Refprice").AsString);
                Item.Refcost = double.Parse(lMainDataSet.FindField("Refcost").AsString);
                //Item.Barcode = lMainDataSet.FindField("Barcode").value;
                //Item.Creationdate = lMainDataSet.FindField("Creationdate").value;
                //Item.Lastmodified = lMainDataSet.FindField("Lastmodified").value;
                //Item.Lastmodified = Convert.ToInt64(lMainDataSet.FindField("Lastmodified").value);
            }
        }

        public void LoadAll()
        {
            //FileName = BizObject.Select("Code,Description", "", "Code", "AD", ",", "C:\Data.txt");
            var IvBizObj = app.ComServer.BizObjects.Find("ST_ITEM");

            // Get the path to the debug folder
            string debugFolderPath = AppDomain.CurrentDomain.BaseDirectory;
            var fileName = "ST_ITEM_LIST.txt";

            // Define the file name and path
            string filePath = Path.Combine(debugFolderPath, fileName);
            var FileName = IvBizObj.Select("*", "", "Code", "AD", ",", filePath);
        }
        
    }
}
