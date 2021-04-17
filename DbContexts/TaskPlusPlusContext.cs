using TaskPlusPlus.API.Entities;
using Microsoft.EntityFrameworkCore;
using System;

namespace TaskPlusPlus.API.DbContexts
{
    public class TaskPlusPlusContext : DbContext
    {
        public TaskPlusPlusContext(DbContextOptions<TaskPlusPlusContext> options)
           : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<File> Files { get; set; }
        public DbSet<Task> Tasks { get; set; }
        public DbSet<Board> Boards { get; set; }
        public DbSet<Session> Sessions { get; set; }
        public DbSet<SharedBoard> SharedBoards { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Board>().HasIndex(u => u.Id).IsUnique();
            modelBuilder.Entity<Comment>().HasIndex(u => u.Id).IsUnique();
            modelBuilder.Entity<File>().HasIndex(u => u.Id).IsUnique();
            modelBuilder.Entity<Session>().HasIndex(u => u.Id).IsUnique();
            modelBuilder.Entity<Session>().HasIndex(u => u.AccessToken).IsUnique();
            modelBuilder.Entity<SharedBoard>().HasIndex(u => u.Id).IsUnique();
            modelBuilder.Entity<Task>().HasIndex(u => u.Id).IsUnique();
            modelBuilder.Entity<User>().HasIndex(u => u.Id).IsUnique();
            //modelBuilder.Entity<User>().HasIndex(u => u.PhoneNumber).IsUnique();

            modelBuilder.Entity<Board>().Property(p => p.Deleted).HasDefaultValue(false);
            modelBuilder.Entity<Board>().Property(p => p.CreationAt).HasDefaultValue(DateTime.Now);
            modelBuilder.Entity<SharedBoard>().Property(p => p.GrantedAccessAt).HasDefaultValue(DateTime.Now);
            modelBuilder.Entity<User>().Property(p => p.SignupDate).HasDefaultValue(DateTime.Now);
            modelBuilder.Entity<Session>().Property(p => p.CreationAt).HasDefaultValue(DateTime.Now);
            modelBuilder.Entity<Task>().Property(p => p.Deleted).HasDefaultValue(false);
            modelBuilder.Entity<Task>().Property(p => p.Star).HasDefaultValue(false);
            modelBuilder.Entity<Task>().Property(p => p.CreationAt).HasDefaultValue(DateTime.Now);

            // seed the database with dummy data
            /*modelBuilder.Entity<User>().HasData(
                new User()
                {
                    Id = Guid.Parse("d28888e9-2ba9-473a-a40f-e38cb54f9b35"),
                    FirstName = "Hossein",
                    LastName = "",
                    SignupDate = new DateTime(2021, 7, 23),
                    PhoneNumber = "09399674687"
                });*/

            base.OnModelCreating(modelBuilder);
        }
    }
}
