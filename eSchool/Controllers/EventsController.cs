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

namespace eSchool.Controllers
{

    [Authorize(Roles = "Admin")]
    public class EventsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EventsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Events
        public async Task<IActionResult> Index(string searchString, int? pageNumber)
        {
            ViewData["CurrentFilter"] = searchString;
            int pageSize = 20;

            var events = _context.Events.OrderBy(by => by.Start).AsQueryable();
            if (!String.IsNullOrEmpty(searchString))
            {
                events = events.Where(s => s.Title.StartsWith(searchString)
                || s.Start.ToString().Contains(searchString) || s.End.ToString().Contains(searchString)
                );
                return View(await PaginatedList<Event>
                    .CreateAsync(events.AsNoTracking(), pageNumber ?? 1, pageSize));
            }

            return View(await PaginatedList<Event>
               .CreateAsync(events.AsNoTracking(), pageNumber ?? 1, pageSize));
        }

        // GET: Events/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var eventData = await _context.Events
                .FirstOrDefaultAsync(m => m.Id == id);
            if (eventData == null)
            {
                return NotFound();
            }

            return View(eventData);
        }

        // GET: Events/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Events/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Description,Start,End")] Event eventData)
        {
            if (ModelState.IsValid)
            {
                _context.Add(eventData);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(eventData);
        }

        // GET: Events/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var eventData = await _context.Events.FindAsync(id);
            if (eventData == null)
            {
                return NotFound();
            }
            return View(eventData);
        }

        // POST: Events/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,Start,End")] Event eventData)
        {
            if (id != eventData.Id)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(eventData);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EventExists(eventData.Id))
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
            return View(eventData);
        }

        // GET: Events/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var eventData = await _context.Events
                .FirstOrDefaultAsync(m => m.Id == id);
            if (eventData == null)
            {
                return NotFound();
            }

            return View(eventData);
        }

        // POST: Events/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var eventData = await _context.Events.FindAsync(id);
            _context.Events.Remove(eventData);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EventExists(int id)
        {
            return _context.Events.Any(e => e.Id == id);
        }
    }
}
