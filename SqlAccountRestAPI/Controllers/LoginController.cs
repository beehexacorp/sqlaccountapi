using System;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SqlAccountRestAPI.Lib;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace SqlAccountRestAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
       
        

        // GET: api/<LoginController>
        [HttpGet]
        public IActionResult Get()
        {
            try
            {
                var helper = new SqlComServer();
                return Ok(helper.ComServer.Title);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // GET api/<LoginController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<LoginController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<LoginController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<LoginController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
