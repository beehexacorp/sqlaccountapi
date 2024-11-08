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
public class StockItemTemplateController : ControllerBase
{
    private readonly SqlAccountingStockItemTemplateHelper _stockItemTemplateHelper;
    public StockItemTemplateController(SqlAccountingStockItemTemplateHelper stockItemTemplateHelper)
    {
        _stockItemTemplateHelper = stockItemTemplateHelper;
    }

    [HttpGet("code/{code}")]
    public IActionResult GetByDocno([FromRoute] string code = "")
    {
        try
        {
            var result = _stockItemTemplateHelper.GetByCode(code);
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
    [HttpGet("days-ago/{days}")]
    public IActionResult GetFromDaysAgo([FromRoute] int days = 0)
    {
        try
        {
            var result = _stockItemTemplateHelper.GetFromDaysAgo(days);
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
    [HttpGet("from-date/{date}")]
    public IActionResult GetFromDate([FromRoute] string date = "")
    {
        try
        {
            var result = _stockItemTemplateHelper.GetFromDate(date);
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