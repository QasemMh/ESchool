using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using eSchool.Data;
using eSchool.Models;
using Microsoft.AspNetCore.Authorization;
using eSchool.ViewModel;
using eSchool.Constants;

namespace eSchool.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ClassesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ClassesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Classes
        public async Task<IActionResult> Index(string searchString, int? pageNumber)
        {

            ViewData["ClassCount"] = _context.Classes.CountAsync().Result;
            ViewData["CurrentFilter"] = searchString;
            int pageSize = 10;

            var classes = _context.Classes.Select(e => e);
            //var classes = _context.Classes.AsQueryable();


            if (!String.IsNullOrEmpty(searchString))
            {
                classes = classes.Where(s => s.Name.StartsWith(searchString)
                || s.Year.ToString().StartsWith(searchString)
                );
                return View(await PaginatedList<Class>
                    .CreateAsync(classes.AsNoTracking(), pageNumber ?? 1, pageSize));
            }

            return View(await PaginatedList<Class>
               .CreateAsync(classes.AsNoTracking(), pageNumber ?? 1, pageSize));
        }

        // GET: Classes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var classData = await _context.Classes.FirstOrDefaultAsync(c => c.Id == id);
            if (classData == null)
            {
                return NotFound();
            }
            var classDetails = await _context.Subjects.Where(c => c.ClassId == id)
                  .OrderBy(by => by.ClassId).ThenBy(by => by.StartTime)
                 .Include(s => s.SubjectDetails)
                 .Include(s => s.Teacher).ToListAsync();


            var viewModel = new ClassDataViewModel
            {
                Class = classData,
                Subjects = classDetails,
                SubjectsNames = classDetails.GroupBy(e => e.SubjectDetails.Name)
                .Select(s => s.FirstOrDefault()).ToList()
            };
            return View(viewModel);
        }

        // GET: Classes/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Classes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Year,Semester")] Class classData)
        {
            if (ModelState.IsValid)
            {
                _context.Add(classData);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(classData);

        }

        // GET: Classes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var classData = await _context.Classes.FindAsync(id);
            if (classData == null)
            {
                return NotFound();
            }
            return View(classData);
        }

        // POST: Classes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Year,Semester")] Class classData)
        {
            if (id != classData.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(classData);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ClassExists(classData.Id))
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
            return View(classData);
        }

        // GET: Classes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var classData = await _context.Classes
                .FirstOrDefaultAsync(m => m.Id == id);
            if (classData == null)
            {
                return NotFound();
            }

            return View(classData);
        }

        // POST: Classes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var classData = await _context.Classes.FindAsync(id);
            _context.Classes.Remove(classData);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ClassExists(int id)
        {
            return _context.Classes.Any(e => e.Id == id);
        }
    }
}
