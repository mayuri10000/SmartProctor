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
        private ILogger _logger;

        public TestController(ILogger logger)
        {
            logger = _logger;
        }
        
        [HttpGet]
        public ActionResult Get()
        {
            _logger.LogError("Fuck you");
            return Ok("Hello world");
        }
    }
}