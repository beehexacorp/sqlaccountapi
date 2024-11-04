// using StockItem = SqlAccountRestAPI.Lib.StockItem;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace SqlAccountRestAPI.Controllers
{
    public partial class BizObjectController
    {
        public class QueryRequest
        {
            public string Sql { get; set; } = null!;
            public IDictionary<string, object> Params { get; set; } = new Dictionary<string, object>();
            public int Offset { get; set; } = 0;
            public int Limit { get; set; } = 100;
        }
    }
}
