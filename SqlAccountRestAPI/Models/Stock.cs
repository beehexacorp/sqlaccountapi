namespace SqlAccountRestAPI.Models
{
    public class Stock
    {
        public string? DtlKey { get; set; }

        public string? DocKey { get; set; }
        public string DocNo { get; set; }
        public DateTime DocDate { get; set; }
        public DateTime PostDate { get; set; }
        public string? Description { get; set; }
        //public string Code { get; set; }
        //public string CompanyName { get; set; }
        //public string Address1 { get; set; }
        //public string? Address2 { get; set; }
        //public string? Address3 { get; set; }
        //public string? Address4 { get; set; }
        //public string Agent { get; set; }
        //public string Terms { get; set; }
        //public double DocAmt { get; set; }
        //public double LocalDocAmt { get; set; }
        //public double D_Amount { get; set; }
        //public string Cancelled { get; set; }

        public List<StockItems> Itemss { get; set; }
    }
    public class StockItems
    {
        public string ItemCode { get; set; } = "MPCT";
        public string Description { get; set; } = "MPCT";
        public string Location { get; set; } = "KL";
        public string Project { get; set; } = "KL";
        public double Qty { get; set; } = 99;
        public string UOM { get; set; } = "UNIT";
        public double UnitCost { get; set; } = 1999;
        public double Amount { get; set; } = 5005;
        //public double Tax { get; set; } = 0;
    }
}
