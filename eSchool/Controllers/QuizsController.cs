using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using eSchool.Data;
using eSchool.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using eSchool.ViewModel;

namespace eSchool.Controllers
{
    [Authorize(Roles = "Teacher")]
    public class QuizsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        public QuizsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Quizs
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var userId = user.TeacherId;
            if (userId == null) return NotFound();

            var subjectsIDs = await _context.Subjects.Where(s => s.TeacherId == userId).Select(s => s.Id).ToListAsync();

            var exams = await _context.Quizzes.Where(q => subjectsIDs.Contains((int)q.SubjectId)).ToListAsync();
            return View(exams);
        }

        // GET: Quizs/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var quiz = await _context.Quizzes.FirstOrDefaultAsync(m => m.Id == id);


            var user = await _userManager.GetUserAsync(User);
            var userId = user.TeacherId;
            if (userId == null) return NotFound();

            var subjects = await _context.Subjects
                .Include(c => c.Class).Include(s => s.SubjectDetails)
                .Where(s => s.TeacherId == userId).FirstOrDefaultAsync(s => s.Id == quiz.SubjectId);

            quiz.Subject = subjects;
            if (quiz == null)
            {
                return NotFound();
            }

            return View(quiz);
        }

        // GET: Quizs/Create
        public async Task<IActionResult> Create()
        {
            var user = await _userManager.GetUserAsync(User);
            var userId = user.TeacherId;
            if (userId == null) return NotFound();

            var subjects = await _context.Subjects
                .Include(c => c.Class).Include(s => s.SubjectDetails)
                .Where(s => s.TeacherId == userId).ToListAsync();

            ViewData["subjectList"] = new SelectList(subjects.Select(s => new DropDownList
            {
                Id = s.Id,
                DisplayValue = $"{s.Class.Name} - {s.SubjectDetails.Name}"
            }), "Id", "DisplayValue");

            return View();
        }

        // POST: Quizs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Quiz quiz)
        {

            if (ModelState.IsValid)
            {
                _context.Add(quiz);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            var user = await _userManager.GetUserAsync(User);
            var userId = user.TeacherId;
            if (userId == null) return NotFound();

            var subjects = await _context.Subjects
                .Include(c => c.Class).Include(s => s.SubjectDetails)
                .Where(s => s.TeacherId == userId).ToListAsync();

            ViewData["subjectList"] = new SelectList(subjects.Select(s => new DropDownList
            {
                Id = s.Id,
                DisplayValue = $"{s.Class.Name} - {s.SubjectDetails.Name}"
            }), "Id", "DisplayValue", quiz.SubjectId);

            return View(quiz);
        }

        // GET: Quizs/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var quiz = await _context.Quizzes.FindAsync(id);
            if (quiz == null)
            {
                return NotFound();
            }
            return View(quiz);
        }

        // POST: Quizs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Quiz quiz)
        {
            if (id != quiz.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(quiz);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!QuizExists(quiz.Id))
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
            return View(quiz);
        }

        // GET: Quizs/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var quiz = await _context.Quizzes
                .FirstOrDefaultAsync(m => m.Id == id);
            if (quiz == null)
            {
                return NotFound();
            }

            return View(quiz);
        }

        // POST: Quizs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var quiz = await _context.Quizzes.FindAsync(id);
            _context.Quizzes.Remove(quiz);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool QuizExists(int id)
        {
            return _context.Quizzes.Any(e => e.Id == id);
        }
    }
}
