using Bank_Web_Application.Models;

public class Transaction
{
    public int Id { get; set; }

    public int AccountId { get; set; }
    public Account Account { get; set; }
    public string Type { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; }
      //Not Neccessary, Depend The Person
    public string Description { get; set; }

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    // IP Check
    public string IPAddress { get; set; }
}
