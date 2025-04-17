namespace Bank_Web_Application.Models
{
    public class Account
    {
        public int Id { get; set; }

        //User Info 
        public int UserId { get; set; }
        public User User { get; set; }
        public decimal Balance { get; set; } = 0;

        // TRY, USD, EUR vs. for now just USD
        public string Currency { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Transaction> Transactions { get; set; }
    }
}
