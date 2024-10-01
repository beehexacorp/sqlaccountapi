using System.Text.Json;
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
    public class _TestSQLController : ControllerBase
    {
        private readonly SqlComServer app;
        public _TestSQLController(SqlComServer comServer)
        {
            app = comServer;
        }
        [HttpGet]
        public IActionResult test(string sql){
            try
            {
                var ivHelper = new _TestSQL(app);
                string result = ivHelper.run(sql);
                return Ok(result);
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