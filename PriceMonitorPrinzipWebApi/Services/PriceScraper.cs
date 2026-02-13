using HtmlAgilityPack;
using PriceMonitorPrinzipWebApi.Services.Interfaces;

namespace PriceMonitorPrinzipWebApi.Services
{
    public class PriceScraper : IPriceScraper
    {
        private readonly HttpClient _httpClient;

        public PriceScraper(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.Timeout = TimeSpan.FromSeconds(15);
        }

        public async Task<decimal> GetPriceAsync(string adUrl)
        {
            try
            {
                Console.WriteLine($"Запрашиваем: {adUrl}");

                var response = await _httpClient.GetAsync(adUrl);
                response.EnsureSuccessStatusCode();

                var html = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Получено {html.Length} символов");

                var price = ParsePriceFromHtml(html);

                Console.WriteLine($"Найдена цена: {price:N0} ₽");
                return price;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
                throw;
            }
        }

        private decimal ParsePriceFromHtml(string html)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var selectors = new[]
            {
                "//span[contains(@class, 'full_price')]", // Можно выше добавить "//span[contains(@class, 'price')]", для того, чтобы проверить работает ли сервис отправки уведомлений об изм. цены
                "//span[@class='price']",
            };

            foreach (var selector in selectors)
            {
                var nodes = doc.DocumentNode.SelectNodes(selector);
                if (nodes != null)
                {
                    foreach (var node in nodes)
                    {
                        var text = node.InnerText.Trim();
                        Console.WriteLine($"Нашли текст: '{text}'");

                        var cleanText = System.Text.RegularExpressions.Regex.Replace(text, @"[^\d]", "");
                        if (decimal.TryParse(cleanText, out decimal price) && price > 0)
                        {
                            Console.WriteLine($"Нашли цену в '{selector}': {price}");
                            return price;
                        }
                    }
                }
            }

            Console.WriteLine("Цена не найдена. Выводим часть HTML:");
            Console.WriteLine(html.Substring(0, Math.Min(2000, html.Length)));

            throw new Exception("Price not found in HTML");
        }
    }
}