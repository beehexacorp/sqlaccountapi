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
    public class StockItemController : ControllerBase
    {
        private readonly SqlComServer app;
        public StockItemController(SqlComServer comServer)
        {
            app = comServer;
        }
        //[HttpGet]
        //public IActionResult GetStockItems()
        //{
        //    try
        //    {
        //        var ivHelper = new StockItem(app);
        //        var stockItems = ivHelper.LoadAllStockItems();

        //        if (stockItems == null || stockItems.Count == 0)
        //        {
        //            return NotFound(new { message = "No stock items found" });
        //        }

        //        return Ok(stockItems);
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new
        //        {
        //            error = "THAI THANH TUAN",
        //            details = ex.Message,
        //            code = 500
        //        });
        //    }
        //}
        // GET: api/<OrderController>
        //[HttpGet] // Get success nhưng null
        //public IActionResult Get()
        //{
        //    try
        //    {
        //        var ivHelper = new StockItem(app);
        //        ivHelper.LoadAllStockItems();

        //        if (ivHelper.Item == null)
        //            return NotFound(); 


        //        var result = new
        //        {
        //            Code = ivHelper.Item.Code != null && !Convert.IsDBNull(ivHelper.Item.Code) ? ivHelper.Item.Code.ToString() : null,
        //            Description = ivHelper.Item.Description != null && !Convert.IsDBNull(ivHelper.Item.Description) ? ivHelper.Item.Description.ToString() : null,
        //            //Dockey
        //            //Dockey = ivHelper.Item.Dockey != null && !Convert.IsDBNull(ivHelper.Item.Dockey) ? ivHelper.Item.Dockey.ToString() : null,
        //        };

        //        string jsonResult = JsonConvert.SerializeObject(result);
        //        return Ok(jsonResult); 
        //    }
        //    catch (Exception ex)
        //    {
        //        var errorResponse = new
        //        {
        //            error = "Thai Thanh Tuan",
        //            details = ex.Message,
        //            code = 400
        //        };
        //        return BadRequest(errorResponse); 
        //    }
        //}
        [HttpGet("Code")]
        public IActionResult Get(string code)
        {
            try
            {
                var ivHelper = new StockItem(app);
                ivHelper.LoadStockItemByCode(code);
                if (ivHelper.Item == null) NotFound();
                string jsonResult = string.Empty;
                jsonResult = JsonConvert.SerializeObject(ivHelper.Item);
                return Ok(jsonResult);
            }
            catch (Exception ex)
            {
                var errorResponse = new
                {
                    error = "Stock Item not exist",
                    code = 400
                };
                return BadRequest(errorResponse);

                //return BadRequest(ex.ToString());
            }
        }

        [HttpGet("All")]
        public IActionResult GetAll()
        {
            try
            {
                var ivHelper = new StockItem(app);
                ivHelper.LoadAll();
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

                //return BadRequest(ex.ToString());
            }
        }

        //public IEnumerable<string> Get()
        //{
        //    try
        //    {
        //        var ivHelper = new StockItem(app);
        //        ivHelper.LoadStockItem();
        //        if (ivHelper.Item == null) NotFound();
        //        string jsonResult = string.Empty;
        //        jsonResult = JsonConvert.SerializeObject(ivHelper.Item);
        //        return Ok(jsonResult);
        //    }
        //    catch (Exception ex)
        //    {
        //        var errorResponse = new
        //        {
        //            error = "Thai Thanh Tuan",
        //            code = 400
        //        };
        //        return BadRequest(errorResponse);

        //        //return BadRequest(ex.ToString());
        //    }
        //    //return new string[] { "value1", "value2" };
        //}

        // GET api/<OrderController>/5
        //[HttpGet("OrderNo")]
        //public IActionResult Get(string orderNo)
        //{
        //    try
        //    {
        //        var ivHelper = new SalesInvoice(app);
        //        ivHelper.LoadOrder(orderNo);
        //        if(ivHelper.Order == null) NotFound();
        //        string jsonResult = string.Empty;
        //        jsonResult = JsonConvert.SerializeObject(ivHelper.Order);
        //        return Ok(jsonResult);
        //    }
        //    catch (Exception ex)
        //    {
        //        var errorResponse = new
        //        {
        //            error = "order not exist",
        //            code = 400
        //        };
        //        return BadRequest(errorResponse);

        //        //return BadRequest(ex.ToString());
        //    }
        //}
    }
}
