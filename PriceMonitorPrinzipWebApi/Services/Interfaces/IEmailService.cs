namespace PriceMonitorPrinzipWebApi.Services.Interfaces
{
    public interface IEmailService
    {
        Task SendPriceChangeEmailAsync(string email, string adUrl, decimal oldPrice, decimal newPrice);
        Task SendPriceAddEmailAsync(string email, string adUrl, decimal Price);
    }
}
