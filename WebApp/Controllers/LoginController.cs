using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text.Json;
using WebApp.Models;

namespace WebApp.Controllers
{
    public class LoginController : Controller
    {
        private readonly HttpClient client;

        public LoginController()
        {
            client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public IActionResult Index()
        {
            // Check if user is already logged in
            if (HttpContext.Session.GetString("Token") != null)
            {
                return RedirectToAction("Index", "Students");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index([Bind("Username,Password")] LoginModel loginModel)
        {
            if (!ModelState.IsValid)
            {
                return View(loginModel);
            }
            var response = await client.PostAsJsonAsync("https://localhost:7009/api/auth/login", loginModel);
            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return View(loginModel);
            }
            var stringData = await response.Content.ReadAsStringAsync();
            var token = JsonSerializer.Deserialize<string>(stringData);
            HttpContext.Session.SetString("Token", token);
            return RedirectToAction("Index", "Students");
        }

        public IActionResult Error([FromQuery] string? message, [FromQuery] string? code)
        {
            ViewData["Message"] = message;
            ViewData["Code"] = code;
            return View();
        }
    }
}
