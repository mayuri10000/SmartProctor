using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SmartProctor.Server.Services;

namespace SmartProctor.Server.Controllers
{
    // This controller is made just for testing in development process and will be removed
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        private ILogger<TestController> _logger;

        public TestController(ILogger<TestController> logger)
        {
            _logger = logger;
        }
        
        [HttpGet]
        public ActionResult Get()
        {
            try
            {
                int a = 1, b = 0;
                int c = a / b;
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
            }
            return Ok("Hello world");
        }
    }
}