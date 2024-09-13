namespace SqlAccountRestAPI.Models
{
    public class Item
    {

        //public string? DtlKey { get; set; }

        public string? DocKey { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public string Stockgroup { get; set; }
        public string Stockcontrol { get; set; }
        public string Costingmethod { get; set; }
        public string Serialnumber { get; set; }
        public string Remark1 { get; set; }
        public string Remark2 { get; set; }
        public double Minqty { get; set; }
        public double Maxqty { get; set; }
        public double Reorderlevel { get; set; }
        public double Reorderqty { get; set; }
        public string Shelf { get; set; }
        public string Suom { get; set; }
        public string Itemtype { get; set; }
        public string Isactive { get; set; }
        public double Balsqty { get; set; }
        public double Balsuomqty { get; set; }
        public double Refprice { get; set; }
        public double Refcost { get; set; }
        public string Barcode { get; set; }
        public DateTime Creationdate { get; set; }
        public DateTime Lastmodified { get; set; }
        public double Dirty { get; set; }
        public double Sdsbarcode { get; set; }
        public string Autokey { get; set; }

    }
}
