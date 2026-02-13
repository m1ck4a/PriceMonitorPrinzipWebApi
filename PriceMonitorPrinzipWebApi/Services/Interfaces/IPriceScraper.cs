namespace PriceMonitorPrinzipWebApi.Services.Interfaces
{
    public interface IPriceScraper
    {
        Task<decimal> GetPriceAsync(string adUrl);
    }
}
