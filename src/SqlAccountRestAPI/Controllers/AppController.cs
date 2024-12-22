using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SqlAccountRestAPI.Core;
using SqlAccountRestAPI.Helpers;

namespace SqlAccountRestAPI.Controllers;
[Route("api/[controller]")]
[ApiController]
public partial class AppController : ControllerBase
{

    private readonly SqlAccountingAppHelper _app;
    private readonly SqlAccountingORM _microORM;
    private readonly ILogger<AppController> _logger;

    public AppController(SqlAccountingAppHelper app, SqlAccountingORM microORM, ILogger<AppController> logger)
    {
        _app = app;
        _microORM = microORM;
        _logger = logger;
    }

    [HttpPost("login")]
    public IActionResult GetLogin([FromBody] LoginRequest request)
    {
        _microORM.Login(request.Username, request.Password);
        return Ok(_app.GetInfo());
    }

    // GET: api/<AppController>
    [HttpGet("info")]
    public async Task<IActionResult> Get()
    {
        return Ok(await _app.GetInfo());
    }
    [HttpGet("actions")]
    public IActionResult GetActions()
    {
        return Ok(_app.GetActions());
    }

    [HttpGet("modules")]
    public IActionResult GetModules()
    {
        return Ok(_app.GetModules());
    }
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

    [HttpGet("logs/test")]
    public IActionResult TestLog()
    {
        try
        {
            _logger.LogInformation("This is a test message");
            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(ex);
        }
    }
    [HttpPost("update")]
    public async Task<IActionResult> Update()
    {
        return Ok(await _app.Update());
    }
}