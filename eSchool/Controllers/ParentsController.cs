using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using eSchool.Data;
using eSchool.Models;
using eSchool.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using eSchool.Constants;
using System.Diagnostics;

namespace eSchool.Controllers
{
    [Authorize(Roles = "Parent")]
    public class ParentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;


        public ParentsController(ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager
            )
        {
            _userManager = userManager;
            _context = context;
            _signInManager = signInManager;
        }


        public async Task<IActionResult> Home()
        {
            var user = await _userManager.GetUserAsync(User);
            var userId = user.ParentId;
            if (userId == null) return NotFound();

            var students = await _context.Students
                .Where(c => c.ParentId == userId).ToListAsync();
            var stdIds = students.Select(s => s.Id).ToList();

            ViewData["AllStudents"] = students.Count;
            ViewData["Exams"] = _context.QuizResults.Where(s => stdIds.Contains(s.StudentId)).CountAsync().Result;


            var viewModel = new AdminHomeViewModel
            {
                Notice = await _context.Notices.OrderByDescending(e => e.PostDateTime.Date).Take(10).ToListAsync(),
                Chats = await _context.Chats.OrderByDescending(o => o.SendDate)
               .Where(c => c.ToId == user.Id).Take(10).ToListAsync()
            };

            return View(viewModel);
        }
        public async Task<IActionResult> MyChildren()
        {
            var user = await _userManager.Users
                .FirstOrDefaultAsync(u => u.Id == GetCurrentUserId());
            if (user == null) return NotFound();

            var students = await _context.Students
                .Include(c => c.Class).Include(u => u.ApplicationUser)
                .Where(s => s.ParentId == user.ParentId)
                .ToListAsync();

            return View(students);
        }
        //profile
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var userId = GetCurrentUserId();
            if (userId == null) return NotFound();
            var user = await _userManager.Users.Where(u => u.Id == userId)
                .Include(t => t.Parent).FirstOrDefaultAsync();
            if (user == null) return NotFound();

            var viewModel = new TeacherViewModel
            {
                AccountId = userId,
                Id = user.Parent.Id,
                UserName = user.UserName,
                FirstName = user.Parent.FirstName,
                MidName = user.Parent.MidName,
                LastName = user.Parent.LastName,
                Email = user.Email,
                Phone = user.PhoneNumber,
                DateBirth = user.Parent.DateBirth,
                NationalId = user.Parent.NationalId,
                Gender = user.Parent.Gender
            };

            return View(viewModel);
        }
        [HttpPost]
        public async Task<IActionResult> Profile(string oldPassword,
            string newPassword, string confirmPassword)
        {
            var user = await GetCurrentUser();
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


        public async Task<IActionResult> ViewClass(int? id)
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
        public async Task<IActionResult> ViewTeacher(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.Users.Where(u => u.TeacherId == id)
                .Include(t => t.Teacher).FirstOrDefaultAsync();
            if (user == null) return NotFound();

            var viewModel = new TeacherViewModel
            {
                AccountId = user.Id,
                Id = user.Teacher.Id,
                UserName = user.UserName,
                FirstName = user.Teacher.FirstName,
                MidName = user.Teacher.MidName,
                LastName = user.Teacher.LastName,
                Email = user.Email,
                Phone = user.PhoneNumber,
                DateBirth = user.Teacher.DateBirth,
                NationalId = user.Teacher.NationalId,
                Gender = user.Teacher.Gender
            };

            return View(viewModel);
        }
        public async Task<IActionResult> ViewAttendance(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var absance = await _context.Absences
                .Include(s => s.Student)
                .Include(c => c.Lesson).ThenInclude(s => s.Subject).ThenInclude(ss => ss.SubjectDetails)
                .Where(s => s.StudentId == id)
                .OrderBy(b => b.Lesson.Date)
                .ToListAsync();

            ViewData["ClassName"] = await _context.Students.Include(c => c.Class)
                .Where(s => s.Id == id)
                .Select(c => c.Class.Name).FirstOrDefaultAsync();
            return View(absance);
        }
        public async Task<IActionResult> ViewExams(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var exams = await _context.QuizResults
                .Where(q => q.StudentId == id)
                .Include(s => s.Student).ThenInclude(c => c.Class)
                .Include(s => s.Subject).ThenInclude(c => c.SubjectDetails)
                .Include(q => q.Quiz)
                .ToListAsync();

            return View(exams);
        }
        public async Task<IActionResult> ViewMarks(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _context.Students.Where(s => s.Id == id).FirstOrDefaultAsync();
            var marks = await _context.Grades
                .Where(s => s.StudentId == id).ToListAsync();

            var subjectIds = marks.Select(m => m.SubjectId).ToList();

            var subjects = await _context.Subjects
                .Include(t => t.Teacher).Include(s => s.SubjectDetails).Include(c => c.Class)
                .Where(s => subjectIds.Contains(s.Id))
                .ToListAsync();

            var viewModel = new List<StudentMarkViewModel>();
            for (int i = 0; i < marks.Count; i++)
            {
                viewModel.Add(new StudentMarkViewModel
                {
                    Grade = marks[i],
                    Subject = subjects[i]
                });
            }
            ViewData["studentName"] = $"{student.FirstName} {student.MidName} {student.LastName}";
            ViewData["avg"] = marks.Select(m => m.Total + 0.0).Average();
            return View(viewModel);
        }


        [HttpGet]
        public async Task<IActionResult> Chat(int? toId)
        {
            var currentUserId = _userManager.GetUserId(User);
            var users = await _userManager.Users.Where(u => u.Id != currentUserId).ToListAsync();
            ViewBag.userToList = new SelectList(users.Select(u => new DropDownList
            {
                AccountId = u.Id,
                DisplayValue = $"{u.UserName} - {GetRoleName(u.UserName).Result}"
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
                    DisplayValue = $"{u.UserName} - {GetRoleName(u.UserName).Result}"
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
        public async Task<IActionResult> ShowNotice(int? id)
        {
            if (id == null) return NotFound();
            var notice = await _context.Notices.FirstOrDefaultAsync(n => n.Id == id);
            return View(notice);
        }

        private string GetCurrentUserId()
        {
            return _userManager.GetUserId(User);
        }
        private async Task<ApplicationUser> GetCurrentUser()
        {
            var user = await _userManager.GetUserAsync(User);
            return user;
        }

        private async Task<string> GetRoleName(string username)
        {
            var user = await _userManager.Users.Where(u => u.UserName == username).FirstOrDefaultAsync();
            if (user.TeacherId != null)
            {
                return Roles.Teacher.ToString();
            }
            else if (user.ParentId != null)
            {
                return Roles.Parent.ToString();

            }
            else if (user.StudentId != null)
            {
                return Roles.Student.ToString();
            }
            else
            {
                return Roles.Admin.ToString();
            }
        }




        //get schedule
        public async Task<IActionResult> GetSchedule()
        {
            var user = await _userManager.GetUserAsync(User);
            var userId = user.ParentId;
            if (userId == null) return NotFound();
            var currentDate = DateTime.Now.Date;
            var student = await _context.Students.Where(s => s.ParentId == userId).ToListAsync();
            var classesIds = student.Select(s => (int)s.ClassId).ToList();

            var schedule = await _context.Subjects
                .Include(c => c.Class).Include(s => s.SubjectDetails)
                .Where(s => classesIds.Contains(s.ClassId))
                .Select(e => new
                {
                    id = e.Id,
                    title = $"{e.Class.Name}-{e.SubjectDetails.Name}",
                    description = e.SubjectDetails.Description ?? "",
                    start = currentDate.ToString("yyyy-MM-dd") + " " + e.StartTime.ToString(@"hh\:mm"),
                    end = currentDate.ToString("yyyy-MM-dd") + " " + e.EndTime.ToString(@"hh\:mm"),
                }).ToListAsync();

            return new JsonResult(schedule);
        }









        //For Admin
        // GET: Parents
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index(string searchString, int? pageNumber)
        {
            ViewData["ParentCount"] = _context.Parents.CountAsync().Result;
            ViewData["CurrentFilter"] = searchString;
            int pageSize = 20;

            var parents = _userManager.Users
                .Where(u => u.ParentId != null).Include(p => p.Parent)
                .Select(u => new ParentViewModel
                {
                    Id = u.Parent.Id,
                    AccountId = u.Id,
                    UserName = u.UserName,
                    FirstName = u.Parent.FirstName,
                    MidName = u.Parent.MidName,
                    LastName = u.Parent.LastName,
                    Gender = u.Parent.Gender,
                    NationalId = u.Parent.NationalId,
                });
            if (!String.IsNullOrEmpty(searchString))
            {
                parents = parents.Where(s => s.LastName.StartsWith(searchString)
                || s.FirstName.StartsWith(searchString)
                || s.NationalId.StartsWith(searchString)
                 || s.UserName.StartsWith(searchString)
                 );
                return View(await PaginatedList<ParentViewModel>
                    .CreateAsync(parents.AsNoTracking(), pageNumber ?? 1, pageSize));
            }

            return View(await PaginatedList<ParentViewModel>
               .CreateAsync(parents.AsNoTracking(), pageNumber ?? 1, pageSize));
        }
        [Authorize(Roles = "Admin")]
        // GET: Parents/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var parent = await _context.Parents
                .Where(m => m.Id == id).Include(s => s.Students)
                .ThenInclude(s => s.Class)
                 .FirstOrDefaultAsync();
            if (parent == null)
            {
                return NotFound();
            }

            return View(parent);
        }

        [Authorize(Roles = "Admin")]
        // GET: Parents/Create
        public IActionResult Create()
        {
            return View();
        }
        [Authorize(Roles = "Admin")]
        // POST: Parents/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ParentViewModel parent)
        {
            if (!ModelState.IsValid)
            {
                return View(parent);
            }
            if (parent.Email != null)
                if (await _userManager.FindByEmailAsync(parent.Email) != null)
                {
                    ModelState.AddModelError("Email", "Email is Already Exists");
                    return View(parent);
                }

            if (await _userManager.FindByNameAsync(parent.UserName) != null)
            {
                ModelState.AddModelError("Username", "Username is Already Exists");
                return View(parent);
            }
            if (parent.Phone != null)
                if (await _userManager.Users.AnyAsync(u => u.PhoneNumber == parent.Phone))
                {
                    ModelState.AddModelError("Phone", "Phone Number is Already Exists");
                    return View(parent);
                }
            if (await _userManager.Users.Include(p => p.Parent).AnyAsync(u => u.Parent.NationalId == parent.NationalId))
            {
                ModelState.AddModelError("NationalId", "NationalId Number is Already Exists");
                return View(parent);
            }

            if (parent.Password != parent.ConfirmPassword || parent.Password == null)
            {
                ModelState.AddModelError("Password", "Invlid or not match Password");
                return View(parent);
            }

            var user = new ApplicationUser
            {
                UserName = parent.UserName,
                Email = parent.Email,
                PhoneNumber = parent.Phone,
            };
            var newUser = new Parent
            {
                FirstName = parent.FirstName,
                MidName = parent.MidName,
                LastName = parent.LastName,
                DateBirth = parent.DateBirth,
                Gender = parent.Gender,
                NationalId = parent.NationalId,
            };
            user.Parent = newUser;
            var result = await _userManager.CreateAsync(user, parent.Password);
            if (!result.Succeeded) return View(parent);

            await _userManager.AddToRoleAsync(user, Roles.Parent.ToString());
            return RedirectToAction(nameof(Index));

        }
        [Authorize(Roles = "Admin")]
        // GET: Parents/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var parent = await _userManager.Users.Where(u => u.ParentId == id)
                .Include(p => p.Parent).Select(u => new ParentViewModel
                {
                    AccountId = u.Id,
                    Id = u.Parent.Id,
                    FirstName = u.Parent.FirstName,
                    MidName = u.Parent.MidName,
                    LastName = u.Parent.LastName,
                    DateBirth = u.Parent.DateBirth,
                    Gender = u.Parent.Gender,
                    NationalId = u.Parent.NationalId,
                    UserName = u.UserName,
                    Email = u.Email,
                    Phone = u.PhoneNumber

                }).FirstOrDefaultAsync();

            if (parent == null)
            {
                return NotFound();
            }
            return View(parent);
        }
        [Authorize(Roles = "Admin")]
        // POST: Parents/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ParentViewModel parent)
        {
            if (id != parent.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var user = await _userManager.Users
                        .Where(u => u.ParentId == id).Include(p => p.Parent).FirstOrDefaultAsync();
                    if (user == null) return NotFound();

                    if (parent.Email != null)
                        if (parent.Email != user.Email)
                            if (await _userManager.FindByEmailAsync(parent.Email) != null)
                            {
                                ModelState.AddModelError("Email", "Email is Already Exists");
                                return View(parent);
                            }

                    if (user.UserName != parent.UserName)
                        if (await _userManager.FindByNameAsync(parent.UserName) != null)
                        {
                            ModelState.AddModelError("Username", "Username is Already Exists");
                            return View(parent);
                        }
                    if (parent.Phone != null)
                        if (parent.Phone != user.PhoneNumber)
                            if (parent.Phone != null)
                                if (await _userManager.Users.AnyAsync(u => u.PhoneNumber == parent.Phone))
                                {
                                    ModelState.AddModelError("Phone", "Phone Number is Already Exists");
                                    return View(parent);
                                }
                    if (parent.NationalId != user.Parent.NationalId)
                        if (await _userManager.Users.Include(p => p.Parent).AnyAsync(u => u.Parent.NationalId == parent.NationalId))
                        {
                            ModelState.AddModelError("NationalId", "NationalId Number is Already Exists");
                            return View(parent);
                        }




                    if (parent.Password != null)
                    {
                        if (parent.Password == parent.ConfirmPassword)
                        {
                            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                            await _userManager.ResetPasswordAsync(user, token, parent.Password);
                        }
                        else
                        {
                            ModelState.AddModelError("Password", "Password Not match");
                            return View(parent);
                        }
                    }

                    user.UserName = parent.UserName;
                    user.Email = parent.Email;
                    user.PhoneNumber = parent.Phone;
                    user.Parent.FirstName = parent.FirstName;
                    user.Parent.LastName = parent.LastName;
                    user.Parent.MidName = parent.MidName;
                    user.Parent.DateBirth = parent.DateBirth;
                    user.Parent.Gender = parent.Gender;

                    await _userManager.UpdateAsync(user);

                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ParentExists(parent.Id))
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
            return View(parent);
        }

        private bool ParentExists(int id)
        {
            return _context.Parents.Any(e => e.Id == id);
        }
    }
}
