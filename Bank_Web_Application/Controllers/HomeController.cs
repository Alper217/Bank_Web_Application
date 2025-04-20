using System.Diagnostics;
using Newtonsoft.Json;
using Bank_Web_Application.Models;
using Microsoft.AspNetCore.Mvc;

namespace Bank_Web_Application.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly BankDbContext _context;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }
        public async Task<IActionResult> Index()
        {
            var apiKey = "a617c9faf54a512d3e47179f93414706";
            var url = $"https://api.exchangerate-api.com/v4/latest/USD?apikey={apiKey}";

            using var client = new HttpClient();
            var response = await client.GetStringAsync(url);

            // Yanýtý debug amaçlý yazdýralým
            Console.WriteLine($"API Response: {response}");

            if (string.IsNullOrEmpty(response))
            {
                return View("Error", new ErrorViewModel { RequestId = "API Yanýtý Alýnamadý" });
            }

            // Newtonsoft.Json ile deserialize edelim
            var result = JsonConvert.DeserializeObject<ExchangeRateResponse>(response);

            if (result == null || result.Rates == null || result.Rates.Count == 0)
            {
                return View("Error", new ErrorViewModel { RequestId = "Veri Deserialize Edilemedi veya Rates Boþ" });
            }

            return View(result);
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
    }
}
