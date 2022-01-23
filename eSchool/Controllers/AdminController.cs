using eSchool.Constants;
using eSchool.Data;
using eSchool.Models;
using eSchool.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eSchool.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {

        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;


        public AdminController(ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager
            )
        {
            _userManager = userManager;
            _context = context;
            _signInManager = signInManager;
        }




        public async Task<IActionResult> Index()
        {
            var students = await _context.Students.ToListAsync();
            ViewData["AllStudents"] = students.Count;
            ViewData["AllParents"] = _context.Parents.CountAsync().Result;
            ViewData["AllTeachers"] = _context.Teachers.CountAsync().Result;
            ViewData["AllClasses"] = _context.Classes.CountAsync().Result;
            ViewData["Male"] = students.Where(s => s.Gender == 'M').Count();
            ViewData["Female"] = students.Where(s => s.Gender == 'F').Count();

            var currentUserId = _userManager.GetUserId(User);
            var viewModel = new AdminHomeViewModel
            {
                Notice = await _context.Notices.OrderByDescending(e => e.PostDateTime.Date).Take(10).ToListAsync(),
                Chats = await _context.Chats.OrderByDescending(o => o.SendDate)
                .Where(c => c.ToId == currentUserId).Take(10).ToListAsync()
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Profile(string edit)
        {
            if (edit != null) ViewData["Edit"] = "Edit";

            var user = await _userManager.GetUserAsync(User);
            return View(user);
        }
        [HttpPost]
        public async Task<IActionResult> Profile(string id, ApplicationUser model)
        {
            if (id != model.Id) return NotFound();

            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null) return NotFound();

            if (model.UserName != user.UserName)
                if (await _userManager.FindByNameAsync(model.UserName) != null)
                {
                    ModelState.AddModelError("UserName", "Username Is Already Exist");
                    ViewData["Edit"] = "Edit";
                    return View(model);
                }
            if (user.Email != model.Email)
            {
                if (await _userManager.FindByEmailAsync(model.Email) != null)
                {
                    ModelState.AddModelError("Email", "Email Is Already Exist");
                    ViewData["Edit"] = "Edit";
                    return View(model);
                }
            }
            if (user.PhoneNumber != model.PhoneNumber)
            {
                if (_userManager.Users.Any(e => e.PhoneNumber == model.PhoneNumber))
                {
                    ModelState.AddModelError("PhoneNumber", "Phone Number Is Already Exist");
                    ViewData["Edit"] = "Edit";
                    return View(model);
                }
            }

            user.UserName = model.UserName;
            user.Email = model.Email;
            user.PhoneNumber = model.PhoneNumber;
            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded) return RedirectToAction(nameof(Profile));
            else { ViewData["Edit"] = "Edit"; return View(model); }

        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(string oldPassword, string newPassword, string confirmPassword)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return BadRequest("Failed,try again");
            }
            if (oldPassword == null || newPassword == null || confirmPassword == null)
            {
                return BadRequest("Please Fill All Failed");
            }
            if (newPassword != confirmPassword)
            {
                return BadRequest("Password and rePassword not match");
            }

            var changePasswordResult = await _userManager.ChangePasswordAsync(user,
                    oldPassword, newPassword);
            if (!changePasswordResult.Succeeded)
            {
                string temp = "";
                foreach (var error in changePasswordResult.Errors)
                {
                    temp += error.Description + "<br/>";
                }
                return BadRequest(temp);
            }

            await _signInManager.RefreshSignInAsync(user);
            return Ok("Update success");
        }

        public async Task<IActionResult> Marks(int? classId, int? studentId, int? subjectId)
        {
            var marks = await _context.Grades
                .Include(s => s.Student).ThenInclude(s => s.Class)
                .Include(s => s.Subject).ThenInclude(s => s.SubjectDetails)
                .ToListAsync();
            var classes = await _context.Classes.ToListAsync();
            var stduents = await _context.Students.ToListAsync();
            var subjects = await _context.Subjects.Include(s => s.SubjectDetails).Include(c => c.Class).ToListAsync();

            if (classId != null)
            {
                marks = marks.Where(c => c.Student.ClassId == (int)classId).ToList();

            }
            if (subjectId != null)
            {
                marks = marks.Where(c => c.SubjectId == (int)subjectId).ToList();
            }
            if (studentId != null)
            {
                marks = marks.Where(c => c.Student.Id == (int)studentId).ToList();

            }

            ViewData["classes"] = new SelectList(classes.Select(c => new DropDownList
            {
                Id = c.Id,
                DisplayValue = c.Name
            }), "Id", "DisplayValue", classId);

            ViewData["students"] = new SelectList(stduents.Select(s => new DropDownList
            {
                Id = s.Id,
                DisplayValue = $"{s.NationalId} {s.FirstName} {s.LastName}"
            }), "Id", "DisplayValue", studentId);


            var subjectList = subjects.Select(m => new DropDownList
            {
                Id = m.Id,
                DisplayValue = $"{m.Class.Name} - {m.SubjectDetails.Name}"
            });
            ViewData["subjects"] = new SelectList(subjectList, "Id", "DisplayValue", subjectId);

            if (studentId == null && subjectId == null && classId == null)
            {
                return View(new List<Grade>());
            }
            return View(marks);
        }


        //search 
        public async Task<IActionResult> Search(string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm))
            {
                return View(new SearchPageViewModel
                {
                    Students = new List<Student>(),
                    Classes = new List<Subject>(),
                    Parents = new List<Parent>(),
                    Teachers = new List<Teacher>(),
                });
            }

            var students = await _context.Students
                .Where(s => s.NationalId.StartsWith(searchTerm) || s.DateBirth.ToString().StartsWith(searchTerm))
                .Take(10).ToListAsync();

            var teacher = await _context.Teachers
                  .Where(s => s.NationalId.StartsWith(searchTerm) || s.DateBirth.ToString().StartsWith(searchTerm))
                  .Take(10).ToListAsync();

            var parent = await _context.Parents
                 .Where(s => s.NationalId.StartsWith(searchTerm) || s.DateBirth.ToString().StartsWith(searchTerm))
                 .Take(10).ToListAsync();

            var classRoom = await _context.Subjects
                .Include(c => c.Class).Include(s => s.SubjectDetails)
                .Where(s => s.Class.Name.StartsWith(searchTerm) || s.SubjectDetails.Name.StartsWith(searchTerm))
                .Take(10).ToListAsync();


            var viewModel = new SearchPageViewModel
            {
                Students = students,
                Classes = classRoom,
                Parents = parent,
                Teachers = teacher,
            };
            return View(viewModel);
        }



        //Chat 
        [HttpGet]
        public async Task<IActionResult> Chat(string toId)
        {
            var currentUserId = _userManager.GetUserId(User);
            var users = await _userManager.Users.Where(u => u.Id != currentUserId).ToListAsync();
            ViewBag.userToList = new SelectList(users.Select(u => new DropDownList
            {
                AccountId = u.Id,
                DisplayValue = u.UserName
            }), "AccountId", "DisplayValue", toId);
            ViewBag.userId = currentUserId;
            return View();
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> Chat(Chat chat)
        {
            var currentUserId = _userManager.GetUserId(User);

            if (!ModelState.IsValid)
            {
                var users = await _userManager.Users.Where(u => u.Id != currentUserId).ToListAsync();
                ViewBag.userToList = new SelectList(users.Select(u => new DropDownList
                {
                    AccountId = u.Id,
                    DisplayValue = u.UserName
                }), "AccountId", "DisplayValue", currentUserId);
                return View(chat);
            }
            chat.SendDate = DateTime.Now;
            chat.FromId = currentUserId;
            _context.Chats.Add(chat);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Chat));
        }
        public async Task<IActionResult> ShowChat()
        {
            var currentUserId = _userManager.GetUserId(User);
            var chats = await _context.Chats
                .Include(c => c.From).OrderByDescending(b => b.SendDate)
                .Where(u => u.ToId == currentUserId).ToListAsync();

            return View(chats);
        }
        public async Task<IActionResult> ShowMessage(int? id)
        {
            if (id == null) return NotFound();
            var msg = await _context.Chats
                .Include(c => c.From)
                .Where(u => u.Id == id).FirstOrDefaultAsync();

            return View(msg);
        }





        [AllowAnonymous]
        public IActionResult GetEvents()
        {
            var events = _context.Events.Select(e => new
            {
                id = e.Id,
                title = e.Title,
                description = e.Description ?? "",
                start = e.Start.ToString("yyyy-MM-dd"),
                end = e.End.HasValue ?
                Convert.ToDateTime(e.End).ToString("yyyy-MM-dd") : null
            }).ToList();

            return new JsonResult(events);
        }

    }
}