using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SqlAccountRestAPI.Lib;
// using StockItem = SqlAccountRestAPI.Lib.StockItem;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace SqlAccountRestAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public partial class BizObjectController : ControllerBase
    {
        private readonly BizObject _bizObject;
        public BizObjectController(BizObject bizObject)
        {
            _bizObject = bizObject;
        }

        [HttpPost("query")]
        public IActionResult GetByQuery([FromBody] QueryRequest request)
        {
            var results = _bizObject.Query(request.Sql, request.Params, request.Offset, request.Limit);
            return Ok(results);
        }
        [HttpPost("add")]
        public IActionResult Add([FromBody] BizObjectAddRequest request)
        {
            var result = _bizObject.AddDetail(request.EntityType, request.Data, request.Children);
            return Ok(result);
        }

        [HttpPost("transfer")]
        public IActionResult Transfer([FromBody] BizObjectTransferRequest request)
        {
            var result = _bizObject.Transfer(request.FromEntityType, request.ToEntityType, request.DocNo);
            return Ok(result);
        }
    }
}
