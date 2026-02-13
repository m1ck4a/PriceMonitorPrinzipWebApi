using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PriceMonitorPrinzipWebApi.Data;

namespace PriceMonitorPrinzipWebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PricesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PricesController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Получить все подписки с актуальными ценами
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetPrices()
        {
            var subscriptions = await _context.Subscriptions.ToListAsync();

            var result = subscriptions.Select(s => new
            {
                id = s.Id,
                adUrl = s.AdUrl,
                email = s.Email,
                currentPrice = s.CurrentPrice,
                lastChecked = s.LastChecked
            });

            return Ok(result);
        }

        /// <summary>
        /// Получить подписки по email
        /// </summary>
        [HttpGet("email/{email}")]
        public async Task<ActionResult<IEnumerable<object>>> GetPricesByEmail(string email)
        {
            var subscriptions = await _context.Subscriptions
                .Where(s => s.Email == email)
                .ToListAsync();

            var result = subscriptions.Select(s => new
            {
                id = s.Id,
                adUrl = s.AdUrl,
                currentPrice = s.CurrentPrice,
                lastChecked = s.LastChecked
            });

            return Ok(result);
        }
    }
}
