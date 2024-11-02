using System;
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
    public class AIContentsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AIContentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: AIContents
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.AIContent.Include(a => a.User);
            return View(await applicationDbContext.ToListAsync());
        }

       
        // GET: AIContents/Create
        public IActionResult Create()
        {
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id");
            return View();
        }

        // POST: AIContents/Create 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Content,UserId")] AIContent aIContent)
        {
            if (ModelState.IsValid)
            {
                _context.Add(aIContent);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", aIContent.UserId);
            return View(aIContent);
        }

        [HttpPost]
        public IActionResult CreateAjax([FromBody] AIContent aicontent)
        {
            if (string.IsNullOrWhiteSpace(aicontent.Content))
            {
                return BadRequest("Content cannot be empty.");
            }


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
            return Json(new { modifiedContent });
        }

         
    }
}
