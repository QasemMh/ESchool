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
using eSchool.Constants;
using Microsoft.AspNetCore.Authorization;

namespace eSchool.Controllers
{
    [Authorize(Roles = "Teacher")]
    public class TeachersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;


        public TeachersController(ApplicationDbContext context,
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
            var userId = user.TeacherId;
            if (userId == null) return NotFound();

            var subjects = await _context.Subjects
                .Include(c => c.Class).Include(s => s.SubjectDetails)
                .Where(s => s.TeacherId == userId)
                .ToListAsync();

            ViewData["coursesList"] = subjects.Select(s => s.Class.Name + " - " + s.SubjectDetails.Name).ToArray();

            var classesIds = subjects
                .Select(s => s.ClassId)
                .ToList();
            var courseIds = subjects
               .Select(s => s.Id)
               .ToList();

            var exams = await _context.Quizzes.Where(q => courseIds.Contains((int)q.SubjectId)).CountAsync();
            var students = await _context.Students.Where(c => classesIds.Contains((int)c.ClassId)).ToListAsync();

            ViewData["AllStudents"] = students.Count;
            ViewData["AllParents"] = students.Where(s => s.ParentId != null).Count();
            ViewData["Exams"] = exams;
            ViewData["AllClasses"] = courseIds.Count();
            ViewData["Male"] = students.Where(s => s.Gender == 'M').Count();
            ViewData["Female"] = students.Where(s => s.Gender == 'F').Count();


            ////
            var currentUserId = _userManager.GetUserId(User);
            var viewModel = new AdminHomeViewModel
            {
                Notice = await _context.Notices.OrderByDescending(e => e.PostDateTime.Date).Take(10).ToListAsync(),
                Chats = await _context.Chats.OrderByDescending(o => o.SendDate)
                .Where(c => c.ToId == currentUserId).Take(10).ToListAsync()
            };

            return View(viewModel);
        }

        //profile
        public async Task<IActionResult> Profile()
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return NotFound();
            var user = await _userManager.Users.Where(u => u.Id == userId)
                .Include(t => t.Teacher).FirstOrDefaultAsync();
            if (user == null) return NotFound();

            var viewModel = new TeacherViewModel
            {
                AccountId = userId,
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
        public async Task<IActionResult> Courses(string searchString)
        {
            ViewData["CurrentFilter"] = searchString;

            var user = await _userManager.GetUserAsync(User);
            var classes = await _context.Subjects
                .Where(e => e.TeacherId == user.TeacherId).OrderBy(by => by.StartTime)
                .Include(c => c.Class).Include(t => t.Teacher).Include(s => s.SubjectDetails)
                .ToListAsync();

            if (!String.IsNullOrEmpty(searchString))
            {
                classes = classes.Where(s => s.Class.Name.StartsWith(searchString)
                || s.Class.Year.ToString().StartsWith(searchString)
                || s.SubjectDetails.Name.StartsWith(searchString)
                 ).ToList();
                return View(classes);
            }

            return View(classes);
        }
        public async Task<IActionResult> TakeAttendance(int? classId, int? subjectId)
        {
            if (classId == null || subjectId == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);

            var students = await _context.Students.Where(s => s.ClassId == classId).ToListAsync();

            var studentAttend = students.Select(s => new CheckboxViewModel
            {
                Id = s.Id,
                NationalId = s.NationalId,
                DisplayValue = $"{s.FirstName} {s.MidName} {s.LastName}",
            }).ToList();

            ViewData["className"] = _context.Classes.Where(c => c.Id == classId).FirstOrDefaultAsync().Result.Name;

            var viewModel = new AttendanceViewModel
            {
                AttendencesList = studentAttend,
            };
            AttendanceViewModel.SubjectId = (int)subjectId;
            AttendanceViewModel.ClassId = (int)classId;
            return View(viewModel);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TakeAttendance(AttendanceViewModel model)
        {
            if (!ModelState.IsValid) return NotFound();

            var currentLesson = await _context.Lessons.Where(e =>
                  e.Date.Date == DateTime.Now.Date
                  && e.SubjectId == AttendanceViewModel.SubjectId
                 ).FirstOrDefaultAsync();
            if (currentLesson != null)
            {
                _context.Lessons.Remove(currentLesson);
                await _context.SaveChangesAsync();
            }

            Lesson lesson = new Lesson
            {
                SubjectId = AttendanceViewModel.SubjectId,
                Date = DateTime.Now
            };
            _context.Lessons.Add(lesson);
            await _context.SaveChangesAsync();
            var lessonId = _context.Lessons.OrderByDescending(b => b.Id).FirstOrDefaultAsync().Result.Id;

            var attend = model.AttendencesList.Where(e => e.IsSelected).ToList();
            var absence = model.AttendencesList.Where(e => e.IsSelected == false).ToList();

            var attendStudent = attend.Select(e => new Attendance
            {
                LessonId = lessonId,
                StudentId = e.Id
            });
            var absenceStudent = absence.Select(e => new Absence
            {
                LessonId = lessonId,
                StudentId = e.Id,
                Resone = "NONE"
            });
            _context.Attendances.AddRange(attendStudent);
            _context.Absences.AddRange(absenceStudent);

            await _context.SaveChangesAsync();

            return RedirectToAction("ViewAttendance", new
            {
                subjectId = AttendanceViewModel.SubjectId
            });
        }
        [HttpGet]
        public async Task<IActionResult> ViewAttendance(int? subjectId)
        {
            if (subjectId == null) return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);
            var currentUserId = currentUser.TeacherId;

            if (currentUserId == null) return NotFound();

            var subjects = await _context.Subjects.Include(s => s.SubjectDetails).Include(c => c.Class)
                .Where(s => s.Id == subjectId).FirstOrDefaultAsync();

            var lessons = await _context.Lessons
              .Where(e => e.SubjectId == subjects.Id)
              .OrderBy(by => by.Date).ToListAsync();

            var students = await _context.Students
                .Where(e => e.ClassId == subjects.ClassId).ToListAsync();

            var lastSession = lessons
                 .Where(e => e.SubjectId == subjectId &&
                 e.Date.ToString("yyyy/MM/dd").Equals(DateTime.Now.ToString("yyyy/MM/dd")))
                 .FirstOrDefault();

            var studentsAttends = students.Select(s => new StudentAttendViewModel
            {
                Student = s,
                Status = GetStatus(s.Id, lastSession),
                TotalAbsences = GetTotalAbsences(s.Id),
                TotalAttendances = GetTotalAttend(s.Id)
            }).ToList();

            var viewModle = new ViewAttendanceViewModel
            {
                StudentsAttends = studentsAttends,
                ClassName = subjects.Class.Name,
                SubjectName = subjects.SubjectDetails.Name,
                SubjectTime = $"{subjects.StartTime.ToString(@"hh\:mm")} - {subjects.EndTime.ToString(@"hh\:mm")}"
               ,
                LessonDate = lastSession?.Date.ToString("yyyy/MM/dd HH:mm:ss tt"),
                NoLessons = lessons.Count == 0
            };

            ViewData["subjects"] = new SelectList(_context.Subjects.Include(c => c.Class).Include(s => s.SubjectDetails)
                .Where(t => t.TeacherId == subjects.TeacherId).Select(e => new DropDownList
                {
                    Id = e.Id,
                    DisplayValue = $"{e.Class.Name} - {e.SubjectDetails.Name}",
                }),
                "Id", "DisplayValue", subjectId);


            return View(viewModle);
        }
        public async Task<IActionResult> StudentAttendance(int? studentId, int? subjectId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var currentUserId = currentUser.TeacherId;
            if (currentUserId == null) return NotFound();

            var classes = await _context.Subjects.Include(c => c.Class).Include(c => c.SubjectDetails)
                .Where(s => s.TeacherId == currentUserId)
                .ToListAsync();
            var lessons = await _context.Lessons
             .Where(s => s.SubjectId == subjectId)
             .ToListAsync();

            var classesIDs = new List<int>();
            foreach (var item in classes) classesIDs.Add(item.ClassId);
            var lessonsIDs = new List<int>();
            foreach (var item in lessons) lessonsIDs.Add(item.Id);

            List<Absence> students = await _context.Absences
                .Include(e => e.Student)
                .Include(e => e.Lesson)
                .ThenInclude(s => s.Subject).ThenInclude(ss => ss.SubjectDetails)
                .Where(less => lessonsIDs.Contains(less.LessonId))
                .ToListAsync();

            if (studentId != null)
            {
                students = students.Where(s => s.StudentId == studentId).ToList();
            }
            if (subjectId != null)
            {
                students = students.Where(e => lessonsIDs.Contains(e.LessonId)).ToList();
            }

            ViewData["students"] = new SelectList(_context.Students.Include(c => c.Class)
                .Where(s => classesIDs.Contains((int)s.ClassId))
                .Select(e => new DropDownList
                {
                    Id = e.Id,
                    DisplayValue = $"{e.NationalId} - {e.FirstName} {e.MidName} {e.LastName}",
                }), "Id", "DisplayValue", studentId);

            ViewData["subjects"] = new SelectList(classes
                          .Select(e => new DropDownList
                          {
                              Id = e.Id,
                              DisplayValue = $"{e.Class.Name} - {e.SubjectDetails.Name}",
                          }), "Id", "DisplayValue", subjectId);

            return View(students);
        }
        public async Task<IActionResult> AllStudents(int? classId, int? pageNumber)
        {
            int pageSize = 30;
            var currentUser = await _userManager.GetUserAsync(User);
            var currentUserId = currentUser.TeacherId;
            if (currentUserId == null) return NotFound();

            var classes = await _context.Subjects.Include(c => c.Class).Include(s => s.SubjectDetails)
                .Where(s => s.TeacherId == currentUserId)
                .ToListAsync();

            var testList = new List<int>();
            foreach (var item in classes)
            {
                testList.Add(item.ClassId);
            }

            var students = _context.Students.Include(c => c.Class).Include(p => p.Parent)
                           .Where(s => testList.Contains((int)s.ClassId)).AsQueryable();

            ViewData["classes"] = new SelectList(classes
                    .Select(e => new DropDownList
                    {
                        Id = e.ClassId,
                        DisplayValue = $"{e.Class.Name} - {e.SubjectDetails.Name}",
                    }), "Id", "DisplayValue", classId);

            if (classId != null)
            {
                students = students.Where(c => c.ClassId == classId).AsQueryable();
                return View(await PaginatedList<Student>
                                .CreateAsync(students.AsNoTracking(), pageNumber ?? 1, pageSize));
            }

            return View(await PaginatedList<Student>
                 .CreateAsync(students.AsNoTracking(), pageNumber ?? 1, pageSize));
        }
        public async Task<IActionResult> AllParents(int? studentId, int? pageNumber)
        {
            int pageSize = 30;
            var currentUser = await _userManager.GetUserAsync(User);
            var currentUserId = currentUser.TeacherId;
            if (currentUserId == null) return NotFound();

            var classes = await _context.Subjects.Include(c => c.Class).Include(s => s.SubjectDetails)
                .Where(s => s.TeacherId == currentUserId)
                .ToListAsync();

            var testList = new List<int>();
            foreach (var item in classes)
            {
                testList.Add(item.ClassId);
            }

            var studentsParents = _context.Students.Include(p => p.Parent)
                           .Where(s => testList.Contains((int)s.ClassId)).AsQueryable();

            ViewData["students"] = new SelectList(studentsParents
                          .Select(e => new DropDownList
                          {
                              Id = e.Id,
                              DisplayValue = $"{e.NationalId} {e.FirstName} {e.LastName}",
                          }), "Id", "DisplayValue", studentId);


            if (studentId != null)
            {
                studentsParents = studentsParents.Where(c => c.Id == studentId).AsQueryable();
                return View(await PaginatedList<Student>
                                .CreateAsync(studentsParents.AsNoTracking(), pageNumber ?? 1, pageSize));
            }

            return View(await PaginatedList<Student>
                 .CreateAsync(studentsParents.AsNoTracking(), pageNumber ?? 1, pageSize));
        }


        public async Task<IActionResult> ExamResult(int? id)
        {
            var user = await _userManager.GetUserAsync(User);
            var userId = user.TeacherId;
            if (userId == null) return NotFound();

            var subjectIds = await _context.Subjects.FirstOrDefaultAsync(s => s.Id == id);

            var exams = await _context.Quizzes.Where(q => subjectIds.Id == (int)q.SubjectId)
                .Select(q => new DropDownList
                {
                    Id = q.Id,
                    DisplayValue = $"{q.Name},{q.Date.ToString("yyyy/MM/dd")}, Mark:{q.Mark}"
                }).ToListAsync();

            var subject = await _context.Subjects.Include(c => c.Class).Include(s => s.SubjectDetails).FirstOrDefaultAsync(s => s.Id == id);
            var examsList = new SelectList(exams, "Id", "DisplayValue");
            var viewModel = new ExamViewModel
            {
                ExamList = examsList,
                CourseId = (int)id,
                Subject = subject,
                SubjectId = subject.Id
            };
            return View(viewModel);
        }
        [HttpGet]
        public async Task<IActionResult> AddExamResult(int? id, int? subjectId)
        {
            if (id == null) return NotFound();
            if (subjectId == null) return NotFound();
            var exam = await _context.Quizzes.FirstOrDefaultAsync(q => q.Id == id);
            if (exam == null) return NotFound();
            var subject = await _context.Subjects
                //  .Include(c => c.Class)
                // .Include(s => s.SubjectDetails)
                //.Include(s => s.Teacher)
                .FirstOrDefaultAsync(e => e.Id == subjectId);
            if (subject == null) return NotFound();

            var students = await _context.Students.Where(c => c.ClassId == subject.ClassId).ToListAsync();

            var viewModel = students.Select(s => new QuizResultViewModel
            {
                StudentId = s.Id,
                StudentName = $"{s.FirstName} {s.MidName} {s.LastName}",
            }).ToList();
            QuizResultViewModel.MaxMark = exam.Mark;

            var viewModel22 = new QuizResultListViewModel
            {
                quizResultViewModels = viewModel,
                QuizId = (int)id,
                SubjectId = (int)subjectId
            };
            return View(viewModel22);
        }
        [ActionName("AddExamResult")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddExamResult(QuizResultListViewModel model)
        {
            if (!ModelState.IsValid) return NotFound();

            if (model.quizResultViewModels == null || model.quizResultViewModels?.Count == 0) return NotFound();

            var results = model.quizResultViewModels.Select(q => new QuizResult
            {
                StudentId = q.StudentId,
                Result = q.Result,
                QuizId = model.QuizId,
                SubjectId = model.SubjectId
            });
            _context.QuizResults.AddRange(results);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Courses));
        }

        public async Task<IActionResult> ViewExamResult(int? id, int? examId)
        {
            if (id == null) return NotFound();
            var subject = await _context.Subjects
                .Include(c => c.Class).Include(s => s.SubjectDetails)
                .FirstOrDefaultAsync(s => s.Id == id);

            var examsResults = await _context.QuizResults
                .Include(s => s.Student)
                .Where(q => q.SubjectId == id).ToListAsync();

            //check null ??
            var quizIds = examsResults.Select(q => q.QuizId).Distinct();
            var examsList = await _context.Quizzes.Where(q => quizIds.Contains(q.Id)).ToListAsync();


            var test = examsList.Select(exam => new DropDownList
            {
                Id = exam.Id,
                DisplayValue = $"{exam.Name}, Mark:{exam.Mark}"
            });
            ViewData["examList"] = new SelectList(test, "Id", "DisplayValue", examId);
            ViewData["subjectid"] = id;

            var examResult = examsResults.Where(exam => exam.QuizId == examId).ToList();
            if (examResult.Count == 0)
            {
                var viewModel = new ViewExamResultViewModel
                {
                    Subject = subject,
                    QuizResults = new List<QuizResult>()
                };
                return View(viewModel);
            }
            else
            {
                var viewModel = new ViewExamResultViewModel
                {
                    Subject = subject,
                    QuizResults = examResult,
                    ExamData = examsList.Where(e => e.Id == examId).FirstOrDefault(),
                    Avarage = examResult.Select(q => q.Result).Average()
                };
                return View(viewModel);
            }

        }



        //get schedule
        public async Task<IActionResult> GetSchedule()
        {
            var user = await _userManager.GetUserAsync(User);
            var userId = user.TeacherId;
            if (userId == null) return NotFound();

            var currentDate = DateTime.Now.Date;
            var schedule = _context.Subjects
                .Include(c => c.Class).Include(s => s.SubjectDetails)
                .Where(s => s.TeacherId == userId)
                .Select(e => new
                {
                    id = e.Id,
                    title = $"{e.Class.Name}-{e.SubjectDetails.Name}",
                    description = e.SubjectDetails.Description ?? "",
                    start = currentDate.ToString("yyyy-MM-dd") + " " + e.StartTime.ToString(@"hh\:mm"),
                    end = currentDate.ToString("yyyy-MM-dd") + " " + e.EndTime.ToString(@"hh\:mm"),
                }).ToList();

            return new JsonResult(schedule);
        }
        //Chat 
        [HttpGet]
        public async Task<IActionResult> Chat(int? toId)
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

            //var users = await _userManager.Users.Where(u => u.Id == currentUserId).FirstOrDefaultAsync();

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


        [HttpGet]
        public async Task<IActionResult> SendMessageToStudents(int? id)
        {
            var course = await _context.Subjects
               .Include(c => c.Class).Include(s => s.SubjectDetails)
               .Where(c => c.ClassId == id).FirstOrDefaultAsync();

            if (id == null) return NotFound();

            var viewModel = new MessageStudentViewModel
            {
                CourseName = $"{course.Class.Name} - {course.SubjectDetails.Name}",
                FromId = _userManager.GetUserId(User),
                ClassId = (int)id
            };
            return View(viewModel);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendMessageToStudents(MessageStudentViewModel model)
        {
            var students = await _context.Students
                .Include(u => u.ApplicationUser)
                .Where(s => s.ClassId == model.ClassId).ToListAsync();

            var chatsList = students.Select(s => new Chat
            {
                FromId = model.FromId,
                ToId = s.ApplicationUser.Id,
                Title = model.Title,
                Message = model.Message,
                SendDate = DateTime.Now
            }).ToList();
            _context.Chats.AddRange(chatsList);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Courses));
        }







        //method
        private string GetStatus(int studentId, Lesson lastSession)
        {
            if (lastSession == null) return AttendanceStatus.Unspecific.ToString();

            var attend = _context.Attendances
                .Where(s => s.StudentId == studentId && s.LessonId == lastSession.Id)
                .FirstOrDefault();
            if (attend != null) return AttendanceStatus.Present.ToString();

            var absance = _context.Absences
                .Where(s => s.StudentId == studentId && s.LessonId == lastSession.Id)
                .FirstOrDefault();
            if (absance != null) return AttendanceStatus.Absent.ToString();
            else return AttendanceStatus.Unspecific.ToString();
        }
        private int GetTotalAttend(int studentId)
        {
            return _context.Attendances.Where(s => s.StudentId == studentId).Count();
        }
        private int GetTotalAbsences(int studentId)
        {
            return _context.Absences.Where(s => s.StudentId == studentId).Count();
        }








        //CRUD For Admin
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index(string searchString, int? pageNumber)
        {
            ViewData["teachersCount"] = _context.Teachers.CountAsync().Result;
            ViewData["CurrentFilter"] = searchString;
            int pageSize = 10;

            var teachers = _userManager.Users
                .Where(u => u.TeacherId != null).Include(p => p.Teacher)
                .Select(u => new TeacherViewModel
                {
                    Id = u.Teacher.Id,
                    AccountId = u.Id,
                    UserName = u.UserName,
                    FirstName = u.Teacher.FirstName,
                    MidName = u.Teacher.MidName,
                    LastName = u.Teacher.LastName,
                    Gender = u.Teacher.Gender,
                    NationalId = u.Teacher.NationalId,
                });
            if (!String.IsNullOrEmpty(searchString))
            {
                teachers = teachers.Where(s => s.LastName.StartsWith(searchString)
                || s.FirstName.StartsWith(searchString)
                || s.NationalId.StartsWith(searchString)
                 || s.UserName.StartsWith(searchString)
                 );
                return View(await PaginatedList<TeacherViewModel>
                    .CreateAsync(teachers.AsNoTracking(), pageNumber ?? 1, pageSize));
            }

            return View(await PaginatedList<TeacherViewModel>
               .CreateAsync(teachers.AsNoTracking(), pageNumber ?? 1, pageSize));
        }
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var classesData = await _context.Subjects
                .Where(u => u.TeacherId == id)
                .GroupBy(by => by.ClassId)
                .Select(u => new
                {
                    classId = u.Key,
                    subjectCount = u.Count()
                }).ToListAsync();

            var enroll = classesData
                .Select(u => new EnrollmentViewModel
                {
                    Subjects = _context.Subjects.Where(c => c.TeacherId == id && c.ClassId == u.classId)
                    .Include(c => c.Class).Include(s => s.SubjectDetails)
                    .ToListAsync().Result,
                    SubjectCount = u.subjectCount,
                    ClassId = u.classId,
                    ClassName = _context.Classes.Where(c => c.Id == u.classId).FirstOrDefaultAsync().Result.Name
                }).ToList();

            var teacher = await _userManager.Users.Where(u => u.TeacherId == id)
                 .Include(t => t.Teacher)
                 .Select(u => new TeacherViewModel
                 {
                     Id = u.Teacher.Id,
                     UserName = u.UserName,
                     Email = u.Email,
                     Phone = u.PhoneNumber,
                     FirstName = u.Teacher.FirstName,
                     MidName = u.Teacher.MidName,
                     LastName = u.Teacher.LastName,
                     DateBirth = u.Teacher.DateBirth,
                     Gender = u.Teacher.Gender,
                     NationalId = u.Teacher.NationalId,
                     ClassCount = classesData.Count,
                     Enrollments = enroll

                 }).FirstOrDefaultAsync();

            if (teacher == null)
            {
                return NotFound();
            }

            return View(teacher);
        }
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TeacherViewModel teacher)
        {
            if (!ModelState.IsValid)
            {
                return View(teacher);
            }
            if (teacher.Email != null)
                if (await _userManager.FindByEmailAsync(teacher.Email) != null)
                {
                    ModelState.AddModelError("Email", "Email is Already Exists");
                    return View(teacher);
                }

            if (await _userManager.FindByNameAsync(teacher.UserName) != null)
            {
                ModelState.AddModelError("Username", "Username is Already Exists");
                return View(teacher);
            }
            if (teacher.Phone != null)
                if (await _userManager.Users.AnyAsync(u => u.PhoneNumber == teacher.Phone))
                {
                    ModelState.AddModelError("Phone", "Phone Number is Already Exists");
                    return View(teacher);
                }
            if (await _userManager.Users.Include(p => p.Teacher)
                .AnyAsync(u => u.Teacher.NationalId == teacher.NationalId))
            {
                ModelState.AddModelError("NationalId", "NationalId Number is Already Exists");
                return View(teacher);
            }

            if (teacher.Password != teacher.ConfirmPassword || teacher.Password == null)
            {
                ModelState.AddModelError("Password", "Invlid or not match Password");
                return View(teacher);
            }

            var user = new ApplicationUser
            {
                UserName = teacher.UserName,
                Email = teacher.Email,
                PhoneNumber = teacher.Phone,
            };
            var newUser = new Teacher
            {
                FirstName = teacher.FirstName,
                MidName = teacher.MidName,
                LastName = teacher.LastName,
                DateBirth = teacher.DateBirth,
                Gender = teacher.Gender,
                NationalId = teacher.NationalId,
            };
            user.Teacher = newUser;
            var result = await _userManager.CreateAsync(user, teacher.Password);
            if (!result.Succeeded) return View(teacher);

            await _userManager.AddToRoleAsync(user, Roles.Teacher.ToString());
            return RedirectToAction(nameof(Index));

        }
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var teacher = await _userManager.Users.Where(u => u.TeacherId == id)
                .Include(t => t.Teacher)
                .Select(u => new TeacherViewModel
                {
                    AccountId = u.Id,
                    Id = u.Teacher.Id,
                    FirstName = u.Teacher.FirstName,
                    MidName = u.Teacher.MidName,
                    LastName = u.Teacher.LastName,
                    DateBirth = u.Teacher.DateBirth,
                    Gender = u.Teacher.Gender,
                    NationalId = u.Teacher.NationalId,
                    UserName = u.UserName,
                    Email = u.Email,
                    Phone = u.PhoneNumber

                }).FirstOrDefaultAsync();

            if (teacher == null)
            {
                return NotFound();
            }
            return View(teacher);
        }
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TeacherViewModel teacher)
        {
            if (id != teacher.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var user = await _userManager.Users
                        .Where(u => u.TeacherId == id).Include(p => p.Teacher).FirstOrDefaultAsync();
                    if (user == null) return NotFound();

                    if (teacher.Email != null)
                        if (teacher.Email != user.Email)
                            if (await _userManager.FindByEmailAsync(teacher.Email) != null)
                            {
                                ModelState.AddModelError("Email", "Email is Already Exists");
                                return View(teacher);
                            }

                    if (user.UserName != teacher.UserName)
                        if (await _userManager.FindByNameAsync(teacher.UserName) != null)
                        {
                            ModelState.AddModelError("Username", "Username is Already Exists");
                            return View(teacher);
                        }
                    if (teacher.Phone != null)
                        if (teacher.Phone != user.PhoneNumber)
                            if (teacher.Phone != null)
                                if (await _userManager.Users.AnyAsync(u => u.PhoneNumber == teacher.Phone))
                                {
                                    ModelState.AddModelError("Phone", "Phone Number is Already Exists");
                                    return View(teacher);
                                }
                    if (teacher.NationalId != user.Teacher.NationalId)
                        if (await _userManager.Users.Include(p => p.Teacher)
                            .AnyAsync(u => u.Teacher.NationalId == teacher.NationalId))
                        {
                            ModelState.AddModelError("NationalId", "NationalId Number is Already Exists");
                            return View(teacher);
                        }
                    if (teacher.Password != null)
                    {
                        if (teacher.Password == teacher.ConfirmPassword)
                        {
                            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                            await _userManager.ResetPasswordAsync(user, token, teacher.Password);
                        }
                        else
                        {
                            ModelState.AddModelError("Password", "Password Not match");
                            return View(teacher);
                        }
                    }

                    user.UserName = teacher.UserName;
                    user.Email = teacher.Email;
                    user.PhoneNumber = teacher.Phone;
                    user.Teacher.FirstName = teacher.FirstName;
                    user.Teacher.LastName = teacher.LastName;
                    user.Teacher.MidName = teacher.MidName;
                    user.Teacher.DateBirth = teacher.DateBirth;
                    user.Teacher.Gender = teacher.Gender;

                    await _userManager.UpdateAsync(user);

                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TeacherExists(teacher.Id))
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
            return View(teacher);
        }


        private bool TeacherExists(int id)
        {
            return _context.Teachers.Any(e => e.Id == id);
        }
    }
}
