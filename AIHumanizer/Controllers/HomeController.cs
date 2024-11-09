using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AIHumanizer.Data;
using AIHumanizer.Models;
using Mscc.GenerativeAI;
namespace AIHumanizer.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: AIContents
        public async Task<IActionResult> Index()
        {
            return View();
        }

        public async Task<IActionResult> Pricing()
        {
            return View();
        }

        // Static field to persist count between requests
        public class ApiResponse
        {
            public string ModifiedContent { get; set; }
            public int RemainingSamples { get; set; }
            public double HoursUntilReset { get; set; }
        }

        [HttpPost]
        public IActionResult CreateAjax([FromBody] AIContent aicontent)
        {
            // Check if the user is signed in
            bool isAuthenticated = User.Identity.IsAuthenticated;

            // Use user ID if authenticated; otherwise, fall back to IP address
            string identifier = isAuthenticated ? User.Identity.Name : HttpContext.Connection.RemoteIpAddress?.ToString()
                ?? throw new InvalidOperationException("Cannot determine client IP address");

            if (string.IsNullOrWhiteSpace(aicontent.Content))
            {
                return BadRequest(new { error = "Content cannot be empty." });
            }

            // Retrieve or create usage based on identifier
            var usage = SampleTracker.GetOrCreateUsage(identifier, isAuthenticated);

            if (usage.RemainingCount <= 0)
            {
                if (!isAuthenticated)
                {
                    return BadRequest(new
                    {
                        error = "No more samples available. Please sign in or create an account to increase your limit.",
                        promptSignIn = true
                    });
                }

                var hoursUntilReset = 24 - DateTime.UtcNow.Subtract(usage.LastReset).TotalHours;
                return BadRequest(new
                {
                    error = $"No more samples available. Try again in {Math.Round(hoursUntilReset, 1)} hours.",
                    hoursUntilReset = Math.Round(hoursUntilReset, 1)
                });
            }

            // Process AI content generation
            var apiKey = "AIzaSyCi1OqWEfZ5Lcm5oMXIRiUbXg3ace4Hn7Y";
            string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "text", "context.txt");
            using StreamReader reader = new(filePath);
            string contentHumanizer = reader.ReadToEnd();
            string userInput = aicontent.Content;
            string prompt = contentHumanizer + userInput;
            var googleAI = new GoogleAI(apiKey: apiKey);
            var model = googleAI.GenerativeModel(model: Model.Gemini15Pro);
            var response = model.GenerateContent(prompt).Result;
            string modifiedContent = response.Text;

            // Decrement sample count after successful generation
            SampleTracker.DecrementSample(identifier);

            var updatedUsage = SampleTracker.GetOrCreateUsage(identifier, isAuthenticated);

            return Json(new ApiResponse
            {
                ModifiedContent = modifiedContent,
                RemainingSamples = updatedUsage.RemainingCount,
                HoursUntilReset = Math.Round(24 - DateTime.UtcNow.Subtract(updatedUsage.LastReset).TotalHours, 1)
            });
        }


    }
}
