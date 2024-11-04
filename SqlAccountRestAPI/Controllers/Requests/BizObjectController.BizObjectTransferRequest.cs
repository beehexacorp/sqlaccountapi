// using StockItem = SqlAccountRestAPI.Lib.StockItem;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace SqlAccountRestAPI.Controllers
{
    public partial class BizObjectController
    {
        public class BizObjectTransferRequest
        {
            public string FromEntityType { get; set; } = null!;
            public string ToEntityType { get; set; } = null!;
            public string DocNo { get; set; } = null!;
        }
    }
}
