using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SqlAccountRestAPI.Core;
using SqlAccountRestAPI.Helpers;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace SqlAccountRestAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class StockItemController : ControllerBase
{
    private readonly SqlAccountingStockItemHelper _stockItemHelper;
    public StockItemController(SqlAccountingStockItemHelper stockItemHelper)
    {
        _stockItemHelper = stockItemHelper;
    }

    [HttpGet("code/{code}")]
    public IActionResult GetByCode([FromRoute] string code = "")
    {
        try
        {
            var result = _stockItemHelper.GetByCode(code);
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