using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using eSchool.Data;
using eSchool.Models;

namespace eSchool.Controllers
{
    public class SubjectDetailsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SubjectDetailsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: SubjectDetails
        public async Task<IActionResult> Index(string searchString, int? pageNumber)
        {
            ViewData["SubjectCount"] = _context.subjectDetails.CountAsync().Result;
            ViewData["CurrentFilter"] = searchString;
            int pageSize = 10;

            var subjcts = _context.subjectDetails.Select(e => e);

            if (!String.IsNullOrEmpty(searchString))
            {
                subjcts = subjcts.Where(s => s.Name.StartsWith(searchString));
                return View(await PaginatedList<SubjectDetails>
                    .CreateAsync(subjcts.AsNoTracking(), pageNumber ?? 1, pageSize));
            }

            return View(await PaginatedList<SubjectDetails>
               .CreateAsync(subjcts.AsNoTracking(), pageNumber ?? 1, pageSize));
        }

        // GET: SubjectDetails/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var subjectDetails = await _context.subjectDetails
                .FirstOrDefaultAsync(m => m.Id == id);
            if (subjectDetails == null)
            {
                return NotFound();
            }

            return View(subjectDetails);
        }

        // GET: SubjectDetails/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: SubjectDetails/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SubjectDetails subjectDetails)
        {
            if (ModelState.IsValid)
            {
                _context.Add(subjectDetails);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(subjectDetails);
        }

        // GET: SubjectDetails/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var subjectDetails = await _context.subjectDetails.FindAsync(id);
            if (subjectDetails == null)
            {
                return NotFound();
            }
            return View(subjectDetails);
        }

        // POST: SubjectDetails/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description")] SubjectDetails subjectDetails)
        {
            if (id != subjectDetails.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(subjectDetails);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SubjectDetailsExists(subjectDetails.Id))
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
            return View(subjectDetails);
        }

        // GET: SubjectDetails/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var subjectDetails = await _context.subjectDetails
                .FirstOrDefaultAsync(m => m.Id == id);
            if (subjectDetails == null)
            {
                return NotFound();
            }

            return View(subjectDetails);
        }

        // POST: SubjectDetails/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var subjectDetails = await _context.subjectDetails.FindAsync(id);
            _context.subjectDetails.Remove(subjectDetails);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SubjectDetailsExists(int id)
        {
            return _context.subjectDetails.Any(e => e.Id == id);
        }
    }
}
