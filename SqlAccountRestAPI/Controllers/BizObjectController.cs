using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SqlAccountRestAPI.Lib;
using SqlAccountRestAPI.Models;
using StockItem = SqlAccountRestAPI.Lib.StockItem;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace SqlAccountRestAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BizObjectController : ControllerBase
    {
        private readonly SqlComServer app;
        public BizObjectController(SqlComServer comServer)
        {
            app = comServer;
        }
        [HttpGet("DaysToNow")]
        public IActionResult GetByDaysToNow([FromQuery] string Type="ST_ITEM", int Days=0)
        {
            try
            {
                var ivHelper = new BizObject(app);
                string jsonResult = ivHelper.LoadByDaysToNow(Type, Days);
                return Ok(jsonResult);
            }
            catch (Exception ex)
            {
                var errorResponse = new
                {
                    error = ex.ToString(),
                    code = 400
                };
                return BadRequest(errorResponse);
            }
        }
        [HttpGet("AllDaysToNow")]
        public IActionResult GetAllByDaysToNow([FromQuery] string Type="ST_ITEM", int Days=0)
        {
            try
            {
                var ivHelper = new BizObject(app);
                string jsonResult = ivHelper.LoadAllByDaysToNow(Type, Days);
                return Ok(jsonResult);
            }
            catch (Exception ex)
            {
                var errorResponse = new
                {
                    error = ex.ToString(),
                    code = 400
                };
                return BadRequest(errorResponse);
            }
        }
        [HttpGet("Query")]
        public IActionResult GetByQuery([FromQuery] Query query)
        {
            try
            {
                var ivHelper = new BizObject(app);
                string jsonResult = ivHelper.LoadByQuery(query);
                return Ok(jsonResult);
            }
            catch (Exception ex)
            {
                var errorResponse = new
                {
                    error = ex.ToString(),
                    code = 400
                };
                return BadRequest(errorResponse);
            }
        }
    }
}
