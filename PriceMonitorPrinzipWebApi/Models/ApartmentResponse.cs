namespace PriceMonitorPrinzipWebApi.Models
{
    public class ApartmentResponse
    {
        public List<Offer> offers { get; set; } = new();
    }

    public class Offer
    {
        public decimal price { get; set; }
        public string name { get; set; } = string.Empty;
    }
}
