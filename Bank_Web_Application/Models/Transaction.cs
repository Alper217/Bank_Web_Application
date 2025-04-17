using Bank_Web_Application.Models;

public class Transaction
{
    public int Id { get; set; }

    public int AccountId { get; set; }
    public Account Account { get; set; }

    // İşlem türü: Deposit, Withdraw, Transfer, Exchange
    public string Type { get; set; }

    // İşlem miktarı
    public decimal Amount { get; set; }

    // Para birimi: TRY, USD, EUR vs.
    public string Currency { get; set; }

    // İşlem açıklaması (isteğe bağlı)
    public string Description { get; set; }

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    // IP adresi loglama
    public string IPAddress { get; set; }
}
