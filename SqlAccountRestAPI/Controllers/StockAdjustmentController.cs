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
    public class StockAdjustmentController : ControllerBase
    {
        private readonly SqlComServer app;
        public StockAdjustmentController(SqlComServer comServer)
        {
            app = comServer;
        }

        // GET: api/<StockAdjustmentController>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        //GET api/<StockAdjustmentController>/5
        [HttpGet("StockNo")]
        public IActionResult Get(string stockNo)
        {
            try
            {
                var ivHelper = new StockAdjustment(app);
                ivHelper.GetStockAdjustment(stockNo);
                if (ivHelper.Stock == null) NotFound();
                string jsonResult = string.Empty;
                jsonResult = JsonConvert.SerializeObject(ivHelper.Stock);
                return Ok(jsonResult);
            }
            catch (Exception ex)
            {
                var errorResponse = new
                {
                    error = "StockAdjustment not exist",
                    code = 400
                };
                return BadRequest(errorResponse);

                //return BadRequest(ex.ToString());
            }
        }
        //[HttpPost]
        //public IActionResult Post([FromBody] StockAdjustment StockAdjustment)
        //{
        //    try
        //    {
        //        var ivHelper = new SalesInvoice(app);
        //        ivHelper.StockAdjustment = StockAdjustment;
        //        ivHelper.AddSalesInvoice();

        //        var StockAdjustmentResponse = new
        //        {
        //            message = "Create StockAdjustment successful",
        //            code = 200,
        //            docNo = StockAdjustment.DocNo
        //        };
        //        return Ok(StockAdjustmentResponse);
        //        //return Ok(StockAdjustment.DocNo);
        //    }             
        //    catch (Exception ex)
        //    {
        //        return BadRequest(ex.ToString());
        //    }
        //}

        // PUT api/<StockAdjustmentController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<StockAdjustmentController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
