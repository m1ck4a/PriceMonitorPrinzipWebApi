using Microsoft.AspNetCore.Mvc;

namespace PriceMonitorPrinzipWebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HomeController : ControllerBase
    {
        /// <summary>
        /// Проверка работоспособности сервиса
        /// </summary>
        [HttpGet("health")]
        public IActionResult HealthCheck()
        {
            return Ok(new { status = "healthy", message = "Service is running" });
        }
    }
}
