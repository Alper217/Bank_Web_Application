using System.Diagnostics;
using Newtonsoft.Json;
using Bank_Web_Application.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bank_Web_Application.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly BankDbContext _context;

        public HomeController(ILogger<HomeController> logger, BankDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var userId = 1;

            if (string.IsNullOrEmpty(userId.ToString()))
                return RedirectToAction("Login", "Account");

            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.UserId == userId);
            if (account == null)
                return NotFound("Hesap bulunamadı.");

            ViewBag.Balance = account.Balance;

            // Eğer hesapta hiç para yoksa, para birimlerini kullanıcıya göstermemek
            if (account.Balance <= 0)
            {
                ViewBag.UserCurrencies = null; // Para birimlerini boş bırakıyoruz
            }
            else
            {
                // Kullanıcının sahip olduğu para birimlerini veritabanından almak
                var userCurrencies = await _context.UserCurrencies
                    .Where(uc => uc.UserId == userId)
                    .ToListAsync();

                var userCurrencyDict = userCurrencies.ToDictionary(uc => uc.Currency, uc => uc.Balance);
                ViewBag.UserCurrencies = userCurrencyDict;
            }

            // Döviz kurları
            var exchangeRates = await GetExchangeRatesAsync();
            if (exchangeRates == null)
                return View("Error", new ErrorViewModel { RequestId = "Döviz kuru alınamadı." });

            ViewBag.Rates = exchangeRates.Rates;

            // Toplam USD karşılığı
            decimal totalInUSD = account.Currency == "USD"
                ? account.Balance
                : account.Balance / exchangeRates.Rates.GetValueOrDefault(account.Currency, 1);

            ViewBag.TotalInUSD = totalInUSD;

            return View(exchangeRates);
        }

        [HttpPost]
        public async Task<IActionResult> Deposit(int accountId, decimal amount, string description)
        {
            var account = await _context.Accounts.FindAsync(accountId);
            if (account == null)
                return NotFound("Hesap bulunamadı.");

            account.Balance += amount;

            _context.Transactions.Add(new Transaction
            {
                AccountId = accountId,
                Type = "Deposit",
                Amount = amount,
                Currency = account.Currency,
                Description = description,
                Timestamp = DateTime.UtcNow,
                IPAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown"
            });

            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Withdraw(int accountId, decimal amount, string description)
        {
            var account = await _context.Accounts.FindAsync(accountId);
            if (account == null)
                return NotFound("Hesap bulunamadı.");

            if (account.Balance < amount)
            {
                ModelState.AddModelError("", "Yetersiz bakiye.");
                return View("Error", new ErrorViewModel { RequestId = "Yetersiz bakiye" });
            }

            account.Balance -= amount;

            _context.Transactions.Add(new Transaction
            {
                AccountId = accountId,
                Type = "Withdraw",
                Amount = -amount,
                Currency = account.Currency,
                Description = description,
                Timestamp = DateTime.UtcNow,
                IPAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown"
            });

            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> ConvertCurrency(string fromCurrency, string toCurrency, decimal amount)
        {
            var userId = 1;
            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.UserId == userId);
            if (account == null)
                return NotFound("Hesap bulunamadı.");

            var exchangeRates = await GetExchangeRatesAsync();
            if (exchangeRates == null)
                return View("Error", new ErrorViewModel { RequestId = "Döviz kuru alınamadı." });

            // Kaynak ve hedef kur oranları
            decimal fromRate = exchangeRates.Rates.GetValueOrDefault(fromCurrency, 1);
            decimal toRate = exchangeRates.Rates.GetValueOrDefault(toCurrency, 1);

            decimal amountInUSD = amount / fromRate;
            decimal convertedAmount = amountInUSD * toRate;

            if (account.Balance < amount && fromCurrency == account.Currency)
            {
                ModelState.AddModelError("", "Yetersiz bakiye.");
                return View("Error", new ErrorViewModel { RequestId = "Yetersiz bakiye" });
            }

            // Bakiyeden düş ve yeni bakiyeyi ayarla (USD bazlı kabul ediliyor)
            account.Balance -= amount;

            _context.Transactions.Add(new Transaction
            {
                AccountId = account.Id,
                Type = $"Convert {fromCurrency} to {toCurrency}",
                Amount = -amount,
                Currency = fromCurrency,
                Description = $"Dönüştürüldü: {convertedAmount} {toCurrency}",
                Timestamp = DateTime.UtcNow,
                IPAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown"
            });

            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> AccountDetails()
        {
            var userId = 1;
            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.UserId == userId);
            if (account == null)
                return NotFound("Hesap bulunamadı.");

            var exchangeRates = await GetExchangeRatesAsync();
            if (exchangeRates == null)
                return View("Error", new ErrorViewModel { RequestId = "Döviz kuru alınamadı." });

            decimal totalInUSD = account.Currency == "USD"
                ? account.Balance
                : account.Balance / exchangeRates.Rates.GetValueOrDefault(account.Currency, 1);

            ViewBag.TotalInUSD = totalInUSD;
            return View(account);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        // Yardımcı metod: Döviz kurlarını getirir
        private async Task<ExchangeRateResponse?> GetExchangeRatesAsync()
        {
            try
            {
                var apiKey = "a617c9faf54a512d3e47179f93414706";
                var url = $"https://api.exchangerate-api.com/v4/latest/USD?apikey={apiKey}";

                using var client = new HttpClient();
                var response = await client.GetStringAsync(url);

                if (string.IsNullOrEmpty(response)) return null;

                return JsonConvert.DeserializeObject<ExchangeRateResponse>(response);
            }
            catch
            {
                return null;
            }
        }
    }
}
