using Microsoft.AspNetCore.Mvc;
using SqlAccountRestAPI.Core;
using SqlAccountRestAPI.Helpers;

namespace SqlAccountRestAPI.Controllers;
[Route("api/[controller]")]
[ApiController]
public partial class AppController : ControllerBase
{

    private readonly SqlAccountingAppHelper _app;
    private readonly SqlAccountingORM _microORM;

    public AppController(SqlAccountingAppHelper app, SqlAccountingORM microORM)
    {
        _app = app;
        _microORM = microORM;
    }

    [HttpGet("login")]
    public IActionResult GetLogin([FromBody] LoginRequest request)
    {
        _microORM.Login(request.Username, request.Password);
        return Ok(_app.GetInfo());
    }

    // GET: api/<AppController>
    [HttpGet("info")]
    public IActionResult Get()
    {
        return Ok(_app.GetInfo());
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
}