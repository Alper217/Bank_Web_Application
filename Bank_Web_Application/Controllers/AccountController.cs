using Bank_Web_Application.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;

namespace Bank_Web_Application.Controllers
{
    public class AccountController : Controller
    {
        private readonly BankDbContext _context;

        public AccountController(BankDbContext context)
        {
            _context = context;
        }

        // Register view
        [HttpGet]
        public IActionResult Register()
        {
            var user = new User(); 
            return View(user);
        }

        // Handle the register form submission
        [HttpPost]
        public async Task<IActionResult> Register(User user)
        {
            if (ModelState.IsValid)
            {
                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == user.Email);
                if (existingUser != null)
                {
                    ModelState.AddModelError("Email", "This email is already registered.");
                    return View(user);
                }

                // Hash the password before saving
                user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);

                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                var newAccount = new Account
                {
                    UserId = user.Id,
                    Balance = 0,
                    Currency ="USD",
                    CreatedAt = DateTime.UtcNow
                };
                _context.Accounts.Add(newAccount);
                await _context.SaveChangesAsync();
                return RedirectToAction("Login", "Account");
            }

            return View(user);
        }
        // Handle the login form submission
        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            if (ModelState.IsValid)
            {
                // Check if the email exists
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
                if (user == null)
                {
                    ModelState.AddModelError("Email", "No user found with this email.");
                    return View();
                }

                // Check if the password matches (using BCrypt to verify the hash)
                bool isPasswordValid = BCrypt.Net.BCrypt.Verify(password, user.Password);
                if (!isPasswordValid)
                {
                    ModelState.AddModelError("Password", "Incorrect password.");
                    return View();
                }

                // Successfully logged in, redirect to the user's account or another page
                return RedirectToAction("Index", "Home"); // Change to wherever you want the user to go after login
            }

            return View();
        }

        // Login view 
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }
    }
}
