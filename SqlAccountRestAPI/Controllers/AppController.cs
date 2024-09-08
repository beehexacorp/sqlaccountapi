using Microsoft.AspNetCore.Mvc;
using SqlAccountRestAPI.Lib;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace SqlAccountRestAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppController : ControllerBase
    {
        // GET: api/<AppController>
        [HttpGet]
        public IActionResult Get()
        {
            try
            {
                var helper = new SqlComServer();
                var jsonObject = helper.GetAppInfo();
                return Ok(jsonObject);
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Get Actions
        /// </summary>
        /// <returns></returns>
        [HttpGet("Actions")]
        public IActionResult GetActions()
        {
            try
            {
                var helper = new SqlComServer();
                var arr = helper.GetActions();
                return Ok(arr);
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Get Modules
        /// </summary>
        /// <returns></returns>
        [HttpGet("Modules")]
        public IActionResult GetModules()
        {
            try
            {
                var helper = new SqlComServer();
                var arr = helper.GetModules();
                return Ok(arr);
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Get Business Objects
        /// </summary>
        /// <returns></returns>
        [HttpGet("BizObjects")]
        public IActionResult GetBizObjects()
        {
            try
            {
                var helper = new SqlComServer();
                var arr = helper.GetBizObjects();
                return Ok(arr);
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
