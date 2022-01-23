using eSchool.Models;
using eSchool.Constants;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using eSchool.Data;
using Microsoft.AspNetCore.Authorization;
using eSchool.ViewModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Diagnostics;

namespace eSchool.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;


        public StudentsController(ApplicationDbContext context,
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
            var userId = user.StudentId;
            if (userId == null) return NotFound();

            var student = await _context.Students.FirstOrDefaultAsync(s => s.Id == userId);

            ViewData["myClass"] = _context.Classes.FirstOrDefaultAsync(s => s.Id == student.ClassId).Result.Name;

            var viewModel = new AdminHomeViewModel
            {
                Notice = await _context.Notices.OrderByDescending(e => e.PostDateTime.Date).Take(10).ToListAsync()

            };

            return View(viewModel);
        }

        public async Task<IActionResult> Profile()
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return NotFound();
            var user = await _userManager.Users.Where(u => u.Id == userId)
                .Include(t => t.Student).FirstOrDefaultAsync();
            if (user == null) return NotFound();

            var viewModel = new TeacherViewModel
            {
                AccountId = userId,
                Id = user.Student.Id,
                UserName = user.UserName,
                FirstName = user.Student.FirstName,
                MidName = user.Student.MidName,
                LastName = user.Student.LastName,
                Email = user.Email,
                Phone = user.PhoneNumber,
                DateBirth = user.Student.DateBirth,
                NationalId = user.Student.NationalId,
                Gender = user.Student.Gender
            };

            return View(viewModel);
        }
        [HttpPost]
        public async Task<IActionResult> Profile(string oldPassword, string newPassword, string confirmPassword)
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


        public async Task<IActionResult> MyClass()
        {
            var user = await _userManager.GetUserAsync(User);
            var userId = user.StudentId;
            if (userId == null) return NotFound();

            var student = await _context.Students.FirstOrDefaultAsync(s => s.Id == userId);
            var classData = await _context.Classes.FirstOrDefaultAsync(c => c.Id == student.ClassId);
            if (classData == null)
            {
                return NotFound();
            }
            var classDetails = await _context.Subjects.Where(c => c.ClassId == student.ClassId)
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
            };
            return View(viewModel);
        }
        public async Task<IActionResult> ViewMark(int? id)
        {
            var user = await _userManager.GetUserAsync(User);
            var userId = user.StudentId;
            if (userId == null) return NotFound();

            var student = await _context.Students.FirstOrDefaultAsync(s => s.Id == userId);
            var classData = await _context.Classes.FirstOrDefaultAsync(c => c.Id == student.ClassId);
            if (classData == null)
            {
                return NotFound();
            }
            var classDetails = await _context.Subjects
                .Where(c => c.ClassId == student.ClassId)
                  .Include(s => s.SubjectDetails)
                  .ToListAsync();


            return View(classDetails);
        }

        public async Task<IActionResult> SubjectMark(int? id)
        {
            if (id == null) return NotFound();
            var user = await _userManager.GetUserAsync(User);
            var userId = user.StudentId;
            if (userId == null) return NotFound();

            var student = await _context.Students.FirstOrDefaultAsync(s => s.Id == userId);
            var mark = await _context.Grades
                .Include(s => s.Student)
                .Include(s => s.Subject).ThenInclude(s => s.SubjectDetails)
                .FirstOrDefaultAsync(g => g.SubjectId == id && g.StudentId == userId);

            if (mark == null)
            {
                return View(null);
            }

            ViewData["myClass"] = _context.Classes.FirstOrDefaultAsync(s => s.Id == student.ClassId).Result.Name;
            ViewData["avg"] = mark.Total > 0 ? mark.Total : "";
            return View(mark);
        }
        public async Task<IActionResult> Grades()
        {
            var user = await _userManager.GetUserAsync(User);
            var id = user.StudentId;
            if (id == null) return NotFound();

            var student = await _context.Students.Where(s => s.Id == id).FirstOrDefaultAsync();
            var marks = await _context.Grades
                .Where(s => s.StudentId == id).ToListAsync();

            var subjectIds = marks.Select(m => m.SubjectId).ToList();

            var subjects = await _context.Subjects
                 .Include(s => s.SubjectDetails).Include(c => c.Class)
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
            ViewData["avg"] = marks.All(m => m.Total.HasValue) ? marks.Select(m => m.Total + 0.0).Average() : "";
            return View(viewModel);
        }


        public async Task<IActionResult> Truancy()
        {
            var user = await _userManager.GetUserAsync(User);
            var id = user.StudentId;
            if (id == null) return NotFound();

            var absance = await _context.Absences
                .Include(c => c.Lesson).ThenInclude(s => s.Subject).ThenInclude(ss => ss.SubjectDetails)
                .Where(s => s.StudentId == id)
                .OrderBy(b => b.Lesson.Date)
                .ToListAsync();

            var student = await _context.Students
                .Include(c => c.Class)
                .FirstOrDefaultAsync(s => s.Id == id);

            ViewData["ClassName"] = student.Class.Name;
            ViewData["studentName"] = $"{student.FirstName} {student.MidName} {student.LastName}";

            return View(absance);
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


        //get schedule
        public async Task<IActionResult> GetSchedule()
        {
            var user = await _userManager.GetUserAsync(User);
            var userId = user.StudentId;
            if (userId == null) return NotFound();

            var currentDate = DateTime.Now.Date;
            var student = await _context.Students.Where(s => s.Id == userId).FirstOrDefaultAsync();
            var classesId = student.ClassId;

            var schedule = await _context.Subjects
                .Include(c => c.Class).Include(s => s.SubjectDetails)
                .Where(s => classesId == s.ClassId)
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


        //Admin
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AllStudents(string searchString, int? pageNumber)
        {
            ViewData["CurrentFilter"] = searchString;

            var viewModel = _context.Users
               .Join(
               _context.Students,
               users => users.StudentId,
               students => students.Id,
               (users, students) => new
               {
                   Id = users.Id,
                   userId = students.Id,
                   UserName = users.UserName,

                   FirstName = students.FirstName,
                   LastName = students.LastName,
                   MidName = students.MidName,
                   Gender = students.Gender,
                   DateBirth = students.DateBirth,
                   NationalId = students.NationalId,
                   ClassId = students.ClassId
               }
               ).Join(
               _context.Classes,
               student => student.ClassId,
               classRoom => classRoom.Id,
               (student, classRoom) => new
               {
                   Id = student.Id,
                   userId = student.userId,
                   UserName = student.UserName,

                   FirstName = student.FirstName,
                   LastName = student.LastName,
                   MidName = student.MidName,
                   Gender = student.Gender,
                   DateBirth = student.DateBirth,
                   NationalId = student.NationalId,
                   ClassId = student.ClassId,
                   ClassName = classRoom.Name,
               }
               )
               .Select(
               u => new UserProfileViewModel
               {
                   UserId = u.userId,
                   Id = u.Id,
                   UserName = u.UserName,

                   FirstName = u.FirstName,
                   LastName = u.LastName,
                   MidName = u.MidName,
                   DateBirth = u.DateBirth,
                   Gender = u.Gender,
                   NationalId = u.NationalId,
                   ClassName = u.ClassName
               }
               );

            int pageSize = 20;

            if (!String.IsNullOrEmpty(searchString))
            {
                viewModel = viewModel.Where(s => s.LastName.Contains(searchString)
                                       || s.FirstName.Contains(searchString)
                                       || s.UserName.StartsWith(searchString)
                                       || s.NationalId.StartsWith(searchString)
                                       || s.ClassName.StartsWith(searchString)

                                       );
                return View(await PaginatedList<UserProfileViewModel>
                    .CreateAsync(viewModel.AsNoTracking(), pageNumber ?? 1, pageSize));
            }

            return View(await PaginatedList<UserProfileViewModel>
               .CreateAsync(viewModel.AsNoTracking(), pageNumber ?? 1, pageSize));

        }
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ViewStudent(int? userId)
        {
            if (userId == null) return NotFound();

            var student = await _context.Students.Where(s => s.Id == userId)
                .Include(c => c.Class).Include(a => a.Address).Include(p => p.Parent)
                .FirstOrDefaultAsync();

            var ViewModel = await _userManager.Users.Where(u => u.StudentId == userId)
                .Select(u => new UserProfileViewModel
                {
                    Id = u.Id,
                    UserName = u.UserName,
                    Email = u.Email,
                    Phone = u.PhoneNumber,
                    UserId = student.Id,
                    FirstName = student.FirstName,
                    LastName = student.LastName,
                    MidName = student.MidName,
                    Gender = student.Gender,
                    DateBirth = student.DateBirth,
                    NationalId = student.NationalId,
                    Address = student.Address,
                    Class = student.Class,
                    ParentId = student.ParentId,
                    Parent = student.Parent
                }).FirstOrDefaultAsync();

            if (student == null || ViewModel == null) return NotFound();

            return View(ViewModel);
        }
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> EditStudent(int? userId)
        {
            if (userId == null) return NotFound();

            var student = await _userManager.Users.Where(u => u.StudentId == userId)
                .Include(s => s.Student).ThenInclude(a => a.Address)
                .Select(u => new UserProfileViewModel
                {
                    Id = u.Id,
                    UserName = u.UserName,
                    Email = u.Email,
                    Phone = u.PhoneNumber,
                    UserId = u.Student.Id,
                    FirstName = u.Student.FirstName,
                    LastName = u.Student.LastName,
                    MidName = u.Student.MidName,
                    Gender = u.Student.Gender,
                    DateBirth = u.Student.DateBirth,
                    NationalId = u.Student.NationalId,
                    Address = u.Student.Address,
                    AddressId = u.Student.AddressId,
                    ClassId = (int)u.Student.ClassId,
                    ParentId = u.Student.ParentId
                }).FirstOrDefaultAsync();

            if (student == null) return NotFound();


            ViewData["ClassList"] = new SelectList(await _context.Classes.ToListAsync(), "Id", "Name");
            ViewData["parents"] = new SelectList(await _context.Parents.Select(p => new DropDownList
            {
                Id = p.Id,
                DisplayValue = $"{p.NationalId}-{p.FirstName} {p.MidName} {p.LastName}"
            }).ToListAsync(), "Id", "DisplayValue");


            return View(student);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditStudent(int userId, UserProfileViewModel model)
        {
            if (userId != model.UserId) return NotFound();

            if (!ModelState.IsValid)
            {
                ViewData["ClassList"] = new SelectList(await _context.Classes.ToListAsync(), "Id", "Name");
                ViewData["parents"] = new SelectList(await _context.Parents.Select(p => new DropDownList
                {
                    Id = p.Id,
                    DisplayValue = $"{p.NationalId}-{p.FirstName} {p.MidName} {p.LastName}"
                }).ToListAsync(), "Id", "DisplayValue");

                return View(model);
            }

            var user = await _userManager.Users.Include(u => u.Student).ThenInclude(a => a.Address)
                .Where(u => u.Id == model.Id).FirstOrDefaultAsync();
            if (user == null) return NotFound();


            if (user.Email != model.Email)
                if (await _userManager.FindByEmailAsync(model.Email) != null)
                {
                    ModelState.AddModelError("Email", "Email is Already Exists");
                    ViewData["ClassList"] = new SelectList(await _context.Classes.ToListAsync(), "Id", "Name");
                    ViewData["parents"] = new SelectList(await _context.Parents.Select(p => new DropDownList
                    {
                        Id = p.Id,
                        DisplayValue = $"{p.NationalId}-{p.FirstName} {p.MidName} {p.LastName}"
                    }).ToListAsync(), "Id", "DisplayValue");

                    return View(model);
                }

            if (user.UserName != model.UserName)
                if (await _userManager.FindByNameAsync(model.UserName) != null)
                {
                    ModelState.AddModelError("Username", "Username is Already Exists");
                    ViewData["ClassList"] = new SelectList(await _context.Classes.ToListAsync(), "Id", "Name");
                    ViewData["parents"] = new SelectList(await _context.Parents.Select(p => new DropDownList
                    {
                        Id = p.Id,
                        DisplayValue = $"{p.NationalId}-{p.FirstName} {p.MidName} {p.LastName}"
                    }).ToListAsync(), "Id", "DisplayValue");

                    return View(model);
                }
            if (user.PhoneNumber != model.Phone)
                if (await _userManager.Users.AnyAsync(u => u.PhoneNumber == model.Phone))
                {
                    ModelState.AddModelError("Phone", "Phone Number is Already Exists");
                    ViewData["ClassList"] = new SelectList(await _context.Classes.ToListAsync(), "Id", "Name");
                    ViewData["parents"] = new SelectList(await _context.Parents.Select(p => new DropDownList
                    {
                        Id = p.Id,
                        DisplayValue = $"{p.NationalId}-{p.FirstName} {p.MidName} {p.LastName}"
                    }).ToListAsync(), "Id", "DisplayValue");

                    return View(model);
                }
            if (user.Student.NationalId != model.NationalId)
                if (await _userManager.Users.AnyAsync(u => u.Student.NationalId == model.NationalId))
                {
                    ModelState.AddModelError("NationalId", "NationalId Number is Already Exists");
                    ViewData["ClassList"] = new SelectList(await _context.Classes.ToListAsync(), "Id", "Name");
                    ViewData["parents"] = new SelectList(await _context.Parents.Select(p => new DropDownList
                    {
                        Id = p.Id,
                        DisplayValue = $"{p.NationalId}-{p.FirstName} {p.MidName} {p.LastName}"
                    }).ToListAsync(), "Id", "DisplayValue");

                    return View(model);
                }

            if (model.Password != null)
            {
                if (model.Password == model.ConfirmPassword)
                {
                    var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                    await _userManager.ResetPasswordAsync(user, token, model.Password);
                }
                else
                {
                    ModelState.AddModelError("Password", "Password Not match");
                    ViewData["ClassList"] = new SelectList(await _context.Classes.ToListAsync(), "Id", "Name");
                    ViewData["parents"] = new SelectList(await _context.Parents.Select(p => new DropDownList
                    {
                        Id = p.Id,
                        DisplayValue = $"{p.NationalId}-{p.FirstName} {p.MidName} {p.LastName}"
                    }).ToListAsync(), "Id", "DisplayValue");

                    return View(model);
                }
            }
            user.UserName = model.UserName;
            user.Email = model.Email;
            user.PhoneNumber = model.Phone;

            user.Student.FirstName = model.FirstName;
            user.Student.LastName = model.LastName;
            user.Student.MidName = model.MidName;
            user.Student.Gender = model.Gender;
            user.Student.DateBirth = model.DateBirth.Date;
            user.Student.NationalId = model.NationalId;
            user.Student.ParentId = model.ParentId;

            user.Student.ClassId = model.ClassId;
            user.Student.Address.Address1 = model.Address.Address1;
            user.Student.Address.Address2 = model.Address.Address2;
            user.Student.Address.District = model.Address.District;
            user.Student.Address.Location = model.Address.Location;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
                return RedirectToAction(nameof(AllStudents));
            else
            {
                ViewData["ClassList"] = new SelectList(await _context.Classes.ToListAsync(), "Id", "Name");
                ViewData["parents"] = new SelectList(await _context.Parents.Select(p => new DropDownList
                {
                    Id = p.Id,
                    DisplayValue = $"{p.NationalId}-{p.FirstName} {p.MidName} {p.LastName}"
                }).ToListAsync(), "Id", "DisplayValue");

                return View(model);

            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> AddStudent()
        {
            var parents = await _context.Parents.Select(e => new
            {
                Id = e.Id,
                Name = $"{e.NationalId}-{e.FirstName}  {e.LastName}"
            }).ToListAsync();
            ViewData["parents"] = new SelectList(parents, "Id", "Name");
            ViewData["ClassList"] = new SelectList(await _context.Classes.ToListAsync(), "Id", "Name");
            return View();
        }
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddStudent(UserProfileViewModel user)
        {
            if (!ModelState.IsValid)
            {
                var parents = await _context.Parents.Select(e => new
                {
                    Id = e.Id,
                    Name = $"{e.NationalId}-{e.FirstName}  {e.LastName}"
                }).ToListAsync();
                ViewData["parents"] = new SelectList(parents, "Id", "Name");
                ViewData["ClassList"] = new SelectList(await _context.Classes.ToListAsync(), "Id", "Name");
                return View(user);
            }
            if (user.Email != null)
                if (await _userManager.FindByEmailAsync(user.Email) != null)
                {
                    var parents = await _context.Parents.Select(e => new
                    {
                        Id = e.Id,
                        Name = $"{e.NationalId}-{e.FirstName}  {e.LastName}"
                    }).ToListAsync();
                    ViewData["parents"] = new SelectList(parents, "Id", "Name");
                    ViewData["ClassList"] = new SelectList(await _context.Classes.ToListAsync(), "Id", "Name");
                    ModelState.AddModelError("Email", "Email is Already Exists");
                    return View(user);
                }

            if (await _userManager.FindByNameAsync(user.UserName) != null)
            {
                var parents = await _context.Parents.Select(e => new
                {
                    Id = e.Id,
                    Name = $"{e.NationalId}-{e.FirstName}  {e.LastName}"
                }).ToListAsync();
                ViewData["parents"] = new SelectList(parents, "Id", "Name");
                ViewData["ClassList"] = new SelectList(await _context.Classes.ToListAsync(), "Id", "Name");
                ModelState.AddModelError("Username", "Username is Already Exists");
                return View(user);
            }
            if (user.Phone != null)
                if (await _userManager.Users.AnyAsync(u => u.PhoneNumber == user.Phone))
                {
                    var parents = await _context.Parents.Select(e => new
                    {
                        Id = e.Id,
                        Name = $"{e.NationalId}-{e.FirstName}  {e.LastName}"
                    }).ToListAsync();
                    ViewData["parents"] = new SelectList(parents, "Id", "Name");
                    ViewData["ClassList"] = new SelectList(await _context.Classes.ToListAsync(), "Id", "Name");
                    ModelState.AddModelError("Phone", "Phone Number is Already Exists");
                    return View(user);
                }
            if (await _userManager.Users.Include(s => s.Student).AnyAsync(u => u.Student.NationalId == user.NationalId))
            {
                var parents = await _context.Parents.Select(e => new
                {
                    Id = e.Id,
                    Name = $"{e.NationalId}-{e.FirstName}  {e.LastName}"
                }).ToListAsync();
                ViewData["parents"] = new SelectList(parents, "Id", "Name");
                ViewData["ClassList"] = new SelectList(await _context.Classes.ToListAsync(), "Id", "Name");
                ModelState.AddModelError("NationalId", "NationalId Number is Already Exists");
                return View(user);
            }
            if (user.Password != user.ConfirmPassword || user.Password == null)
            {
                var parents = await _context.Parents.Select(e => new
                {
                    Id = e.Id,
                    Name = $"{e.NationalId}-{e.FirstName}  {e.LastName}"
                }).ToListAsync();
                ViewData["parents"] = new SelectList(parents, "Id", "Name");
                ViewData["ClassList"] = new SelectList(await _context.Classes.ToListAsync(), "Id", "Name");
                ModelState.AddModelError("Password", "Invlid or not match Password");
                return View(user);
            }


            var newUser = new ApplicationUser
            {
                UserName = user.UserName,
                Email = user.Email,
                PhoneNumber = user.Phone
            };

            var newUserStudent = new Student
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                MidName = user.MidName,
                Gender = user.Gender,
                DateBirth = user.DateBirth,
                NationalId = user.NationalId,
                Address = user.Address,
                ClassId = user.ClassId,
                ParentId = user.ParentId
            };
            newUser.Student = newUserStudent;

            var result = await _userManager.CreateAsync(newUser, user.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(newUser, Roles.Student.ToString());
                return RedirectToAction(nameof(AllStudents));
            }
            else return View(user);
        }


    }
}
