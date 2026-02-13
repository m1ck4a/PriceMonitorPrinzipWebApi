namespace PriceMonitorPrinzipWebApi.Models
{
    public class Subscription
    {
        public int Id { get; set; }
        public string AdUrl { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public decimal CurrentPrice { get; set; }
        public DateTime LastChecked { get; set; }
    }
}
