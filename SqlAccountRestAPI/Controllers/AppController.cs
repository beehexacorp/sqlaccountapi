using Microsoft.AspNetCore.Mvc;
using SqlAccountRestAPI.Lib;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace SqlAccountRestAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public partial class AppController : ControllerBase
{

    private readonly SqlAccountingApp _app;
    public AppController(SqlAccountingApp comServer)
    {
        _app = comServer;
    }

    // GET: api/<LoginController>
    [HttpGet("login")]
    public IActionResult GetLogin([FromBody] LoginRequest request)
    {
        _app.Login(request.Username, request.Password);
        return Ok(_app.GetInfo());
    }

    // GET: api/<AppController>
    [HttpGet("info")]
    public IActionResult Get()
    {
        return Ok(_app.GetInfo());
    }

    /// <summary>
    /// Get Actions
    /// </summary>
    /// <returns></returns>
    [HttpGet("actions")]
    public IActionResult GetActions()
    {
        return Ok(_app.GetActions());
    }

    /// <summary>
    /// Get Modules
    /// </summary>
    /// <returns></returns>
    [HttpGet("modules")]
    public IActionResult GetModules()
    {
        return Ok(_app.GetModules());
    }

    /// <summary>
    /// Get Business Objects
    /// </summary>
    /// <returns></returns>
    [HttpGet("objects")]
    public IActionResult GetBizObjects()
    {
        return Ok(_app.GetBizObjects());
    }

    [HttpGet("objects/{name}")]
    public IActionResult GetBizObjectInfo(string name)
    {
        return Ok(_app.GetBizObjectInfo(name));
    }
}