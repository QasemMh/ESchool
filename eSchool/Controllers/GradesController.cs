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
using Microsoft.AspNetCore.Identity;
using eSchool.ViewModel;

namespace eSchool.Controllers
{
    [Authorize(Roles = "Teacher")]
    public class GradesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public GradesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var subject = await GetSubjecsAsync();
            return View(subject);
        }

        public async Task<IActionResult> ViewMarks(int? id)
        {
            if (id == null) return NotFound();
            var subject = await _context.Subjects
                .Include(c => c.Class).Include(s => s.SubjectDetails)
                .FirstOrDefaultAsync(s => s.Id == id);
            if (subject == null) return NotFound();

            var students = await _context.Students.Where(c => c.ClassId == subject.ClassId).ToListAsync();
            var gradesList = await _context.Grades
                .Include(s => s.Student)
                .Where(g => g.SubjectId == subject.Id).ToListAsync();


            if (gradesList.Count == 0)
            {
                return View();
            }

            var studentGradeViewModel = gradesList.Select(g => new GradeViewModel
            {
                StudentId = g.Student.Id,
                StudentName = $"{g.Student.FirstName} {g.Student.MidName} {g.Student.LastName}",
                FirstMark = g.FirstMark.HasValue ? (double)g.FirstMark : 0,
                MidtMark = g.MidtMark.HasValue ? (double)g.MidtMark : 0,
                FinalMark = g.FinalMark.HasValue ? (double)g.FinalMark : 0,
                ActivityMark = g.ActivityMark.HasValue ? (double)g.ActivityMark : 0,
                GradeId = g.Id,
                Total = g.Total.HasValue ? (double)g.Total : 0
            }).ToList();

            var viewModel = new AddGradeViewModel
            {
                StudentsGrades = studentGradeViewModel,
                ClassName = subject.Class.Name,
                SubjectName = subject.SubjectDetails.Name,
                ClassAvarage = gradesList.Select(g => g.Total.HasValue ? (double)g.Total : 0).Average(),
                FirstAvarage = gradesList.Select(g => g.FirstMark.HasValue ? (double)g.FirstMark : 0).Average(),
                MidAvarage = gradesList.Select(g => g.MidtMark.HasValue ? (double)g.MidtMark : 0).Average(),
                FinalAvarage = gradesList.Select(g => g.FinalMark.HasValue ? (double)g.FinalMark : 0).Average(),
            };
            return View(viewModel);
        }
        [HttpGet]
        public async Task<IActionResult> GiveMarks(int? id)
        {
            if (id == null) return NotFound();
            var subject = await _context.Subjects
                .Include(c => c.Class).Include(s => s.SubjectDetails)
                .FirstOrDefaultAsync(s => s.Id == id);
            if (subject == null) return NotFound();
            var students = await _context.Students.Where(c => c.ClassId == subject.ClassId).ToListAsync();
            var gradesList = await _context.Grades
                .Include(s => s.Student)
                .Where(g => g.SubjectId == subject.Id).ToListAsync();
            if (gradesList.Count == 0)
            {
                var subjectGradeList = new List<Grade>();
                for (int i = 0; i < students.Count; i++)
                {
                    subjectGradeList.Add(new Grade
                    {
                        StudentId = students[i].Id,
                        SubjectId = (int)id
                    });
                }
                _context.Grades.AddRange(subjectGradeList);
                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
                gradesList = await _context.Grades
                    .Include(s => s.Student)
                    .Where(g => g.SubjectId == subject.Id).ToListAsync();
            }

            var studentGradeViewModel = gradesList.Select(g => new GradeViewModel
            {
                StudentId = g.Student.Id,
                StudentName = $"{g.Student.FirstName} {g.Student.MidName} {g.Student.LastName}",
                FirstMark = g.FirstMark.HasValue ? (double)g.FirstMark : 0,
                MidtMark = g.MidtMark.HasValue ? (double)g.MidtMark : 0,
                FinalMark = g.FinalMark.HasValue ? (double)g.FinalMark : 0,
                ActivityMark = g.ActivityMark.HasValue ? (double)g.ActivityMark : 0,


                GradeId = g.Id
            }).ToList();
            var viewModel = new AddGradeViewModel
            {
                StudentsGrades = studentGradeViewModel,
                ClassName = subject.Class.Name,
                SubjectName = subject.SubjectDetails.Name
            };
            return View(viewModel);
        }
        [HttpPost]
        public async Task<IActionResult> GiveMarks(AddGradeViewModel model)
        {
            if (!ModelState.IsValid) return NotFound();

            var gradeListIds = model.StudentsGrades.Select(g => g.GradeId).ToList();
            var gradeList = await _context.Grades.Where(g => gradeListIds.Contains(g.Id)).ToListAsync();
            var modelData = model.StudentsGrades;
            for (int i = 0; i < gradeList.Count; i++)
            {
                gradeList[i].FirstMark = modelData[i].FirstMark;
                gradeList[i].MidtMark = modelData[i].MidtMark;
                gradeList[i].FinalMark = modelData[i].FinalMark;
                gradeList[i].ActivityMark = modelData[i].ActivityMark;
            }
            _context.Grades.UpdateRange(gradeList);
            try
            {
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                ViewData["errors"] = "An Error Occurs, Please Try Again...";
                return View(model);

            }

        }













        private async Task<List<Subject>> GetSubjecsAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            var userId = user.TeacherId;

            var subjects = await _context.Subjects
                .Include(c => c.Class).Include(s => s.SubjectDetails).Include(t => t.Teacher).OrderBy(b => b.StartTime)
                .Where(s => s.TeacherId == userId).ToListAsync();

            return subjects;
        }
    }
}
