namespace SqlAccountRestAPI.Models
{

    public class Query
    {
        public string? Type { get; set; } = "ST_ITEM";
        public string? Where { get; set; } = "";
        public string? OrderBy { get; set; } = "Code";
    }
}