public class ExchangeRateResponse
{
    public string Base { get; set; }
    public string Date { get; set; }
    public long TimeLastUpdated { get; set; }
    public Dictionary<string, decimal> Rates { get; set; }
}
