using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PriceMonitorPrinzipWebApi.Data;
using PriceMonitorPrinzipWebApi.Models;
using PriceMonitorPrinzipWebApi.Services.Interfaces;

namespace PriceMonitorPrinzipWebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SubscriptionController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IPriceScraper _priceScraper;
        private readonly IEmailService _emailService;

        public SubscriptionController(AppDbContext context, IPriceScraper priceScraper, IEmailService emailService)
        {
            _context = context;
            _priceScraper = priceScraper;
            _emailService = emailService;
        }

        /// <summary>
        /// Подписаться на изменение цены
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Subscribe([FromBody] SubscriptionRequest request)
        {

            if (string.IsNullOrWhiteSpace(request.AdUrl))
            {
                return BadRequest(new { error = "AdUrl is required" });
            }

            if (string.IsNullOrWhiteSpace(request.Email) || !IsValidEmail(request.Email))
            {
                return BadRequest(new { error = "Valid email is required" });
            }

            try
            {
                var existing = await _context.Subscriptions
                    .FirstOrDefaultAsync(s => s.AdUrl == request.AdUrl && s.Email == request.Email);

                if (existing != null)
                {
                    return BadRequest(new { error = "Already subscribed to this apartment" });
                }

                decimal price = await _priceScraper.GetPriceAsync(request.AdUrl);

                var subscription = new Subscription
                {
                    AdUrl = request.AdUrl,
                    Email = request.Email,
                    CurrentPrice = price,
                    LastChecked = DateTime.UtcNow
                };

                _context.Subscriptions.Add(subscription);
                await _context.SaveChangesAsync();

                try
                {
                    await _emailService.SendPriceAddEmailAsync(
                        request.Email,
                        request.AdUrl,
                        price);

                    Console.WriteLine($"Письмо отправлено на {request.Email}");
                }
                catch (Exception emailEx)
                {
                    Console.WriteLine($"Не удалось отправить письмо: {emailEx.Message}");
                }

                return Ok(new
                {
                    message = "Subscribed successfully",
                    adUrl = subscription.AdUrl,
                    currentPrice = subscription.CurrentPrice,
                    lastChecked = subscription.LastChecked
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Отписаться от уведомлений
        /// </summary>
        [HttpDelete]
        public async Task<IActionResult> Unsubscribe([FromBody] SubscriptionRequest request)
        {
            var subscription = await _context.Subscriptions
                .FirstOrDefaultAsync(s => s.AdUrl == request.AdUrl && s.Email == request.Email);

            if (subscription == null)
            {
                return NotFound(new { error = "Subscription not found" });
            }

            _context.Subscriptions.Remove(subscription);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Unsubscribed successfully" });
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}
