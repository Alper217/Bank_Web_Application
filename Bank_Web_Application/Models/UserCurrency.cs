﻿public class UserCurrency
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Currency { get; set; } = string.Empty;
    public decimal Balance { get; set; }
}
