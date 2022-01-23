using eSchool.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace eSchool.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);


            /* security database */
            builder.Entity<ApplicationUser>().ToTable("Users", "security");
            builder.Entity<IdentityRole>().ToTable("Roles", "security");
            builder.Entity<IdentityUserRole<string>>().ToTable("UserRole", "security");
            builder.Entity<IdentityUserLogin<string>>().ToTable("UserLogin", "security");
            builder.Entity<IdentityUserClaim<string>>().ToTable("UserClaim", "security");
            builder.Entity<IdentityRoleClaim<string>>().ToTable("RoleClaim", "security");
            builder.Entity<IdentityUserToken<string>>().ToTable("UserToken", "security");

            /**/
            builder.Entity<Student>().HasIndex(e => e.NationalId).IsUnique();
            builder.Entity<Teacher>().HasIndex(e => e.NationalId).IsUnique();
            builder.Entity<Parent>().HasIndex(e => e.NationalId).IsUnique();
            /**/
            builder.Entity<Student>(e => e.Property(p => p.DateBirth).HasColumnType("Date"));
            builder.Entity<Teacher>(e => e.Property(p => p.DateBirth).HasColumnType("Date"));
            builder.Entity<Parent>(e => e.Property(p => p.DateBirth).HasColumnType("Date"));
            /**/
            builder.Entity<Grade>(e => e.Property(p => p.Total)
            .HasComputedColumnSql("[FirstMark]+[MidtMark]+[FinalMark]+[ActivityMark]")
            );
            /**/
            builder.Entity<Event>(e => e.Property(p => p.Start).HasColumnType("Date"));
            builder.Entity<Event>(e => e.Property(p => p.End).HasColumnType("Date"));
            /**/





        }

        public DbSet<Student> Students { get; set; }
        public DbSet<Parent> Parents { get; set; }
        public DbSet<Teacher> Teachers { get; set; }

        public DbSet<Address> Addresses { get; set; }

        public DbSet<Subject> Subjects { get; set; }
        public DbSet<Class> Classes { get; set; }
        public DbSet<SubjectDetails> subjectDetails { get; set; }

        public DbSet<Grade> Grades { get; set; }
        public DbSet<Quiz> Quizzes { get; set; }
        public DbSet<QuizResult> QuizResults { get; set; }

        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<Absence> Absences { get; set; }
        public DbSet<Lesson> Lessons { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Notice> Notices { get; set; }
        public DbSet<Chat> Chats { get; set; }

    }
}
