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
using eSchool.ViewModel;
using System.Diagnostics;

namespace eSchool.Controllers
{
    public class SubjectsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public SubjectsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
            _context = context;
        }


        public async Task<IActionResult> Index(string searchString,
            int? pageNumber, int? teacherId, int? subjectId, int? classId)
        {
            var teachers = await _context.Teachers.Select(u => new DropDownList
            {
                Id = u.Id,
                DisplayValue = $"{u.NationalId}-{u.FirstName} {u.LastName}"
            }).ToListAsync();

            var classes = await _context.Classes.Select(u => new DropDownList
            {
                Id = u.Id,
                DisplayValue = $"{u.Name} {u.Year}-{u.Semester}"
            }).ToListAsync();
            var subjects = await _context.subjectDetails.Select(u => new DropDownList
            {
                Id = u.Id,
                DisplayValue = u.Name
            }).ToListAsync();



            ViewData["CurrentFilter"] = searchString;
            ViewData["subjects"] = new SelectList(subjects, "Id", "DisplayValue", subjectId);
            ViewData["teachers"] = new SelectList(teachers, "Id", "DisplayValue", teacherId);
            ViewData["classes"] = new SelectList(classes, "Id", "DisplayValue", classId);

            int pageSize = 10;

            var enrollment = _context.Subjects
                 .Include(s => s.Class).Include(s => s.SubjectDetails).Include(s => s.Teacher)
                 .OrderBy(by => by.ClassId).ThenBy(then => then.StartTime)
                 .AsQueryable();

            if (!String.IsNullOrEmpty(searchString))
            {
                enrollment = enrollment.Where(s => s.SubjectDetails.Name.StartsWith(searchString)
                || s.Teacher.FirstName.StartsWith(searchString)
                || s.Class.Name.StartsWith(searchString)
                 );
                return View(await PaginatedList<Subject>
                    .CreateAsync(enrollment.AsNoTracking(), pageNumber ?? 1, pageSize));
            }
            if (classId.HasValue)
            {
                enrollment = enrollment.Where(c => c.Class.Id == classId);

            }
            if (teacherId.HasValue)
            {
                enrollment = enrollment.Where(c => c.Teacher.Id == teacherId);
            }
            if (subjectId.HasValue)
            {
                enrollment = enrollment.Where(c => c.SubjectDetails.Id == subjectId);
            }
            return View(await PaginatedList<Subject>
               .CreateAsync(enrollment.AsNoTracking(), pageNumber ?? 1, pageSize));
        }
        // GET: Subjects/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var subject = await _context.Subjects
                .Include(s => s.Class)
                .Include(s => s.SubjectDetails)
                .Include(s => s.Teacher)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (subject == null)
            {
                return NotFound();
            }

            return View(subject);
        }

        // GET: Subjects/Create
        public async Task<IActionResult> Create()
        {
            var teachers = await _context.Teachers.Select(u => new DropDownList
            {
                Id = u.Id,
                DisplayValue = $"{u.NationalId} - {u.FirstName} {u.LastName}"
            }).ToListAsync();
            ViewData["TeacherId"] = new SelectList(teachers, "Id", "DisplayValue");
            ViewData["ClassId"] = new SelectList(_context.Classes, "Id", "Name");
            ViewData["SubjectDetailsId"] = new SelectList(_context.subjectDetails, "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AssignSubjectViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var classTime = await _context.Subjects
                    .Where(c => c.ClassId == viewModel.ClassId)
                    .Select(s => new { s.StartTime, s.EndTime })
                    .ToListAsync();

                //check time based on class
                if (viewModel.EndTime.CompareTo(viewModel.StartTime) <= 0)
                {
                    ModelState.AddModelError("StartTime", "Start Time Must be Less Than End Time");
                    ViewData["ClassId"] = new SelectList(_context.Classes, "Id", "Name", viewModel.ClassId);
                    ViewData["SubjectDetailsId"] = new SelectList(_context.subjectDetails, "Id", "Name", viewModel.SubjectDetailsId);
                    ViewData["TeacherId"] = new SelectList(await _context.Teachers.Select(u => new DropDownList
                    {
                        Id = u.Id,
                        DisplayValue = $"{u.NationalId} - {u.FirstName} {u.LastName}"
                    }).ToListAsync(), "Id", "DisplayValue", viewModel.TeacherId);
                    return View(viewModel);
                }

                for (int i = 0; i < classTime.Count; i++)
                {
                    if (classTime[i].StartTime.CompareTo(viewModel.StartTime) <= 0)
                    {
                        if (classTime[i].EndTime.CompareTo(viewModel.StartTime) >= 1)
                        {
                            ModelState.AddModelError("StartTime", "There is an overlap in timing with another subject in class");
                            ViewData["ClassId"] = new SelectList(_context.Classes, "Id", "Name", viewModel.ClassId);
                            ViewData["SubjectDetailsId"] = new SelectList(_context.subjectDetails, "Id", "Name", viewModel.SubjectDetailsId);
                            ViewData["TeacherId"] = new SelectList(await _context.Teachers.Select(u => new DropDownList
                            {
                                Id = u.Id,
                                DisplayValue = $"{u.NationalId} - {u.FirstName} {u.LastName}"
                            }).ToListAsync(), "Id", "DisplayValue", viewModel.TeacherId);
                            return View(viewModel);
                        }
                    }
                }
                for (int i = 0; i < classTime.Count; i++)
                {
                    if (classTime[i].StartTime.CompareTo(viewModel.StartTime) >= 1)
                    {
                        if (classTime[i].StartTime.CompareTo(viewModel.EndTime) <= -1)
                        {
                            ModelState.AddModelError("StartTime", "There is an overlap in timing with another subject in class");
                            ViewData["ClassId"] = new SelectList(_context.Classes, "Id", "Name", viewModel.ClassId);
                            ViewData["SubjectDetailsId"] = new SelectList(_context.subjectDetails, "Id", "Name", viewModel.SubjectDetailsId);
                            ViewData["TeacherId"] = new SelectList(await _context.Teachers.Select(u => new DropDownList
                            {
                                Id = u.Id,
                                DisplayValue = $"{u.NationalId} - {u.FirstName} {u.LastName}"
                            }).ToListAsync(), "Id", "DisplayValue", viewModel.TeacherId);
                            return View(viewModel);
                        }
                    }
                }


                var teacherTime = await _context.Subjects
                 .Where(c => c.TeacherId == viewModel.TeacherId)
                 .Select(s => new { s.StartTime, s.EndTime })
                 .ToListAsync();

                //check time based on teacher
                for (int i = 0; i < teacherTime.Count; i++)
                {
                    if (teacherTime[i].StartTime.CompareTo(viewModel.StartTime) <= 0)
                    {
                        if (teacherTime[i].EndTime.CompareTo(viewModel.StartTime) >= 1)
                        {
                            ModelState.AddModelError("StartTime", "There is an overlap in timing with Teacher");
                            ViewData["ClassId"] = new SelectList(_context.Classes, "Id", "Name", viewModel.ClassId);
                            ViewData["SubjectDetailsId"] = new SelectList(_context.subjectDetails, "Id", "Name", viewModel.SubjectDetailsId);
                            ViewData["TeacherId"] = new SelectList(await _context.Teachers.Select(u => new DropDownList
                            {
                                Id = u.Id,
                                DisplayValue = $"{u.NationalId} - {u.FirstName} {u.LastName}"
                            }).ToListAsync(), "Id", "DisplayValue", viewModel.TeacherId);
                            return View(viewModel);
                        }
                    }
                }
                for (int i = 0; i < teacherTime.Count; i++)
                {
                    if (teacherTime[i].StartTime.CompareTo(viewModel.StartTime) >= 1)
                    {
                        if (teacherTime[i].StartTime.CompareTo(viewModel.EndTime) <= -1)
                        {
                            ModelState.AddModelError("StartTime", "There is an overlap in timing with Teacher");
                            ViewData["ClassId"] = new SelectList(_context.Classes, "Id", "Name", viewModel.ClassId);
                            ViewData["SubjectDetailsId"] = new SelectList(_context.subjectDetails, "Id", "Name", viewModel.SubjectDetailsId);
                            ViewData["TeacherId"] = new SelectList(await _context.Teachers.Select(u => new DropDownList
                            {
                                Id = u.Id,
                                DisplayValue = $"{u.NationalId} - {u.FirstName} {u.LastName}"
                            }).ToListAsync(), "Id", "DisplayValue", viewModel.TeacherId);
                            return View(viewModel);
                        }
                    }
                }


                Subject sub = new Subject();
                sub.EndTime = viewModel.EndTime;
                sub.StartTime = viewModel.StartTime;
                sub.ClassId = viewModel.ClassId;
                sub.SubjectDetailsId = viewModel.SubjectDetailsId;
                sub.TeacherId = viewModel.TeacherId;
                _context.Subjects.Add(sub);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            ViewData["ClassId"] = new SelectList(_context.Classes, "Id", "Name", viewModel.ClassId);
            ViewData["SubjectDetailsId"] = new SelectList(_context.subjectDetails, "Id", "Name", viewModel.SubjectDetailsId);
            var teachers = await _context.Teachers.Select(u => new DropDownList
            {
                Id = u.Id,
                DisplayValue = $"{u.NationalId} - {u.FirstName} {u.LastName}"
            }).ToListAsync();
            ViewData["TeacherId"] = new SelectList(teachers, "Id", "DisplayValue", viewModel.TeacherId);
            return View(viewModel);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var subject = await _context.Subjects.Where(s => s.Id == id)
                .Select(u => new AssignSubjectViewModel
                {
                    Id = u.Id,
                    ClassId = u.ClassId,
                    StartTime = u.StartTime,
                    EndTime = u.EndTime,
                    TeacherId = u.TeacherId,
                    SubjectDetailsId = u.SubjectDetailsId
                }).FirstOrDefaultAsync();
            if (subject == null)
            {
                return NotFound();
            }
            ViewData["ClassId"] = new SelectList(_context.Classes, "Id", "Name");
            ViewData["SubjectDetailsId"] = new SelectList(_context.subjectDetails, "Id", "Name");
            var teachers = await _context.Teachers.Select(u => new DropDownList
            {
                Id = u.Id,
                DisplayValue = $"{u.NationalId} - {u.FirstName} {u.LastName}"
            }).ToListAsync();
            ViewData["TeacherId"] = new SelectList(teachers, "Id", "DisplayValue");
            return View(subject);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AssignSubjectViewModel viewModel)
        {
            if (id != viewModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var sub = await _context.Subjects
                        .Where(s => s.Id == viewModel.Id)
                        .FirstOrDefaultAsync();

                    var classTime = await _context.Subjects
                   .Where(c => c.ClassId == viewModel.ClassId && c.Id != id)
                   .Select(s => new { s.StartTime, s.EndTime })
                   .ToListAsync();

                    //check time based on class
                    if (viewModel.EndTime.CompareTo(viewModel.StartTime) <= 0)
                    {
                        ModelState.AddModelError("StartTime", "Start Time Must be Less Than End Time");
                        ViewData["ClassId"] = new SelectList(_context.Classes, "Id", "Name", viewModel.ClassId);
                        ViewData["SubjectDetailsId"] = new SelectList(_context.subjectDetails, "Id", "Name", viewModel.SubjectDetailsId);
                        ViewData["TeacherId"] = new SelectList(await _context.Teachers.Select(u => new DropDownList
                        {
                            Id = u.Id,
                            DisplayValue = $"{u.NationalId} - {u.FirstName} {u.LastName}"
                        }).ToListAsync(), "Id", "DisplayValue", viewModel.TeacherId);
                        return View(viewModel);
                    }

                    if (viewModel.StartTime != sub.StartTime || viewModel.EndTime != sub.EndTime)
                        if (classTime.Any(e => e.StartTime.CompareTo(viewModel.StartTime) <= 0))
                        {
                            if (classTime.All(e => e.EndTime.CompareTo(viewModel.StartTime) >= 1))
                            {
                                ModelState.AddModelError("StartTime", "There is an overlap in timing with another subject in class");
                                ViewData["ClassId"] = new SelectList(_context.Classes, "Id", "Name", viewModel.ClassId);
                                ViewData["SubjectDetailsId"] = new SelectList(_context.subjectDetails, "Id", "Name", viewModel.SubjectDetailsId);
                                ViewData["TeacherId"] = new SelectList(await _context.Teachers.Select(u => new DropDownList
                                {
                                    Id = u.Id,
                                    DisplayValue = $"{u.NationalId} - {u.FirstName} {u.LastName}"
                                }).ToListAsync(), "Id", "DisplayValue", viewModel.TeacherId);
                                return View(viewModel);
                            }
                        }

                    if (viewModel.StartTime != sub.StartTime || viewModel.EndTime != sub.EndTime)
                        if (classTime.Any(e => e.StartTime.CompareTo(viewModel.StartTime) >= 1))
                        {
                            if (classTime.Any(e => e.StartTime.CompareTo(viewModel.EndTime) <= -1))
                            {
                                ModelState.AddModelError("StartTime", "There is an overlap in timing with another subject in class");
                                ViewData["ClassId"] = new SelectList(_context.Classes, "Id", "Name", viewModel.ClassId);
                                ViewData["SubjectDetailsId"] = new SelectList(_context.subjectDetails, "Id", "Name", viewModel.SubjectDetailsId);
                                ViewData["TeacherId"] = new SelectList(await _context.Teachers.Select(u => new DropDownList
                                {
                                    Id = u.Id,
                                    DisplayValue = $"{u.NationalId} - {u.FirstName} {u.LastName}"
                                }).ToListAsync(), "Id", "DisplayValue", viewModel.TeacherId);
                                return View(viewModel);
                            }
                        }

                    var teacherTime = await _context.Subjects
                      .Where(c => c.TeacherId == viewModel.TeacherId && c.Id != id)
                      .Select(s => new { s.StartTime, s.EndTime })
                      .ToListAsync();

                    //check time based on teacher
                    if (viewModel.StartTime != sub.StartTime || viewModel.EndTime != sub.EndTime)
                        if (teacherTime.Any(e => e.StartTime.CompareTo(viewModel.StartTime) <= 0))
                        {
                            if (teacherTime.All(e => e.EndTime.CompareTo(viewModel.StartTime) >= 1))
                            {
                                ModelState.AddModelError("StartTime", "There is an overlap timing with Teacher schedule");
                                ViewData["ClassId"] = new SelectList(_context.Classes, "Id", "Name", viewModel.ClassId);
                                ViewData["SubjectDetailsId"] = new SelectList(_context.subjectDetails, "Id", "Name", viewModel.SubjectDetailsId);
                                ViewData["TeacherId"] = new SelectList(await _context.Teachers.Select(u => new DropDownList
                                {
                                    Id = u.Id,
                                    DisplayValue = $"{u.NationalId} - {u.FirstName} {u.LastName}"
                                }).ToListAsync(), "Id", "DisplayValue", viewModel.TeacherId);
                                return View(viewModel);
                            }
                        }

                    if (viewModel.StartTime != sub.StartTime || viewModel.EndTime != sub.EndTime)
                        if (teacherTime.Any(e => e.StartTime.CompareTo(viewModel.StartTime) >= 1))
                        {
                            if (teacherTime.Any(e => e.StartTime.CompareTo(viewModel.EndTime) <= -1))
                            {
                                ModelState.AddModelError("StartTime", "There is an overlap timing with Teacher schedule");
                                ViewData["ClassId"] = new SelectList(_context.Classes, "Id", "Name", viewModel.ClassId);
                                ViewData["SubjectDetailsId"] = new SelectList(_context.subjectDetails, "Id", "Name", viewModel.SubjectDetailsId);
                                ViewData["TeacherId"] = new SelectList(await _context.Teachers.Select(u => new DropDownList
                                {
                                    Id = u.Id,
                                    DisplayValue = $"{u.NationalId} - {u.FirstName} {u.LastName}"
                                }).ToListAsync(), "Id", "DisplayValue", viewModel.TeacherId);
                                return View(viewModel);
                            }
                        }

                    sub.EndTime = viewModel.EndTime;
                    sub.StartTime = viewModel.StartTime;
                    sub.ClassId = viewModel.ClassId;
                    sub.SubjectDetailsId = viewModel.SubjectDetailsId;
                    sub.TeacherId = viewModel.TeacherId;
                    _context.Subjects.Update(sub);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SubjectExists(viewModel.Id))
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
            ViewData["ClassId"] = new SelectList(_context.Classes, "Id", "Name");
            ViewData["SubjectDetailsId"] = new SelectList(_context.subjectDetails, "Id", "Name");
            var teachers = await _context.Teachers.Select(u => new DropDownList
            {
                Id = u.Id,
                DisplayValue = $"{u.NationalId} - {u.FirstName} {u.LastName}"
            }).ToListAsync();
            ViewData["TeacherId"] = new SelectList(teachers, "Id", "DisplayValue");
            return View(viewModel);
        }

        // GET: Subjects/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var subject = await _context.Subjects
                .Include(s => s.Class)
                .Include(s => s.SubjectDetails)
                .Include(s => s.Teacher)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (subject == null)
            {
                return NotFound();
            }

            return View(subject);
        }

        // POST: Subjects/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var subject = await _context.Subjects.FindAsync(id);
            _context.Subjects.Remove(subject);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SubjectExists(int id)
        {
            return _context.Subjects.Any(e => e.Id == id);
        }
    }
}
