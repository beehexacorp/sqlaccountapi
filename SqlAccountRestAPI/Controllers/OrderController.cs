using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SqlAccountRestAPI.Lib;
using SqlAccountRestAPI.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace SqlAccountRestAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly SqlComServer app;
        public OrderController(SqlComServer comServer)
        {
            app = comServer;
        }

        // GET: api/<OrderController>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/<OrderController>/5
        [HttpGet("OrderNo")]
        public IActionResult Get(string orderNo)
        {
            try
            {
                var ivHelper = new SalesInvoice(app);
                ivHelper.LoadOrder(orderNo);
                if(ivHelper.Order == null) NotFound();
                string jsonResult = string.Empty;
                jsonResult = JsonConvert.SerializeObject(ivHelper.Order);
                return Ok(jsonResult);
            }
            catch (Exception ex)
            {
                var errorResponse = new
                {
                    error = "order not exist",
                    code = 400
                };
                return BadRequest(errorResponse);

                //return BadRequest(ex.ToString());
            }
        }
        [HttpPost]
        public IActionResult Post([FromBody] Order order)
        {
            try
            {
                var ivHelper = new SalesInvoice(app);
                ivHelper.Order = order;
                ivHelper.AddSalesInvoice();

                var orderResponse = new
                {
                    message = "Create Order successful",
                    code = 200,
                    docNo = order.DocNo
                };
                return Ok(orderResponse);
                //return Ok(order.DocNo);
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

        // PUT api/<OrderController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<OrderController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
