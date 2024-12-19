using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SqlAccountRestAPI.Core;
using SqlAccountRestAPI.Helpers;

namespace SqlAccountRestAPI.Controllers
{
    [Route("api/biz-objects")]
    [ApiController]
    public partial class BizObjectController : ControllerBase
    {
        private readonly SqlAccountingBizObjectHelper _bizObject;
        public BizObjectController(SqlAccountingBizObjectHelper bizObject)
        {
            _bizObject = bizObject;
        }

        [HttpPost("query")]
        public IActionResult GetByQuery([FromBody] BizObjectQueryRequest request)
        {
            var results = _bizObject.Query(request.Sql, request.Params, request.Offset, request.Limit);
            return Ok(results);
        }
        [HttpPost("{entityType}")]
        public IActionResult Add(string entityType, [FromBody] BizObjectRequest request)
        {
            var result = _bizObject.AddDetail(entityType, request.Data);
            return Ok(result);
        }

        [HttpPut("{entityType}/{fieldKey}/{fieldValue}")]
        public IActionResult Update(string entityType, string fieldKey, string fieldValue, [FromBody] BizObjectUpdateRequest request)
        {
            var result = _bizObject.Update(entityType, request.MainKey, fieldKey, fieldValue, request.Data);
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
