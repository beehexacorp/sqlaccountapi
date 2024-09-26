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
    public class _TestCustomerController : ControllerBase
    {
        private readonly SqlComServer app;
        public _TestCustomerController(SqlComServer comServer)
        {
            app = comServer;
        }
        [HttpGet]
        public IActionResult test(string type="AR_CUSTOMER", string dataset="MainDataSet", string query="SELECT * FROM AR_CUSTOMER"){
            try
            {
                var ivHelper = new TestCustomer(app);
                ivHelper.run(new JObject{
                    {"type",type},
                    {"dataset",dataset},
                    {"query",query}
                });
                return Ok("OK");
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