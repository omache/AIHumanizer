using System;
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

        // GET: AIContents/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var aIContent = await _context.AIContent
                .Include(a => a.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (aIContent == null)
            {
                return NotFound();
            }

            return View(aIContent);
        }

        // GET: AIContents/Create
        public IActionResult Create()
        {
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id");
            return View();
        }

        // POST: AIContents/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
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
            string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "text", "content.txt");

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

        // GET: AIContents/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var aIContent = await _context.AIContent.FindAsync(id);
            if (aIContent == null)
            {
                return NotFound();
            }
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", aIContent.UserId);
            return View(aIContent);
        }

        // POST: AIContents/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Content,UserId")] AIContent aIContent)
        {
            if (id != aIContent.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(aIContent);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AIContentExists(aIContent.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", aIContent.UserId);
            return View(aIContent);
        }

        // GET: AIContents/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var aIContent = await _context.AIContent
                .Include(a => a.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (aIContent == null)
            {
                return NotFound();
            }

            return View(aIContent);
        }

        // POST: AIContents/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var aIContent = await _context.AIContent.FindAsync(id);
            if (aIContent != null)
            {
                _context.AIContent.Remove(aIContent);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AIContentExists(int id)
        {
            return _context.AIContent.Any(e => e.Id == id);
        }
    }
}
