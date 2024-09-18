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
    public class GenericController : ControllerBase
    {
        private readonly SqlComServer app;
        public GenericController(SqlComServer comServer)
        {
            app = comServer;
        }
        [HttpGet("Query")]
        public IActionResult GetByQuery([FromQuery] Query query)
        {
            try
            {
                var ivHelper = new Generic(app);
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

                //return BadRequest(ex.ToString());
            }
        }
    }
}
