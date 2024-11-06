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
public class CustomerController : ControllerBase
{
    private readonly SqlAccountingCustomerHelper _customerHelper;
    public CustomerController(SqlAccountingCustomerHelper customerHelper)
    {
        _customerHelper = customerHelper;
    }
    // [HttpGet("AllDaysToNow")]
    // public IActionResult GetByDaysToNow([FromQuery] int days = 0)
    // {
    //     try
    //     {
    //         var ivHelper = new Customer(_customerHelper);
    //         string jsonResult = ivHelper.LoadAllByDaysToNow(days);
    //         return Ok(jsonResult);
    //     }
    //     catch (Exception ex)
    //     {
    //         var errorResponse = new
    //         {
    //             error = ex.ToString(),
    //             code = 400
    //         };
    //         return BadRequest(errorResponse);
    //     }
    // }

    [HttpGet("email/{email}")]
    // TODO: validate email
    public IActionResult GetByEmail([FromRoute] string email = "")
    {
        try
        {
            var result = _customerHelper.GetByEmail(email);
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

    [HttpPost("payment")]
    public IActionResult Payment([FromBody] AddCustomerPaymentRequest request)
    {
        try
        {
            var result = _customerHelper.AddPayment(
                request.DocumentNo,
                request.Code,
                request.PaymentMethod,
                request.Project);
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

    [HttpGet("code/{code}")]
    // TODO: validate email
    public IActionResult GetByCode([FromRoute] string code = "")
    {
        try
        {
            var result = _customerHelper.GetByCode(code);
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