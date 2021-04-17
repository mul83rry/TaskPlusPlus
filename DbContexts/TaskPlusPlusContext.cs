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
            modelBuilder.Entity<User>().HasIndex(u => u.PhoneNumber).IsUnique();

            modelBuilder.Entity<Board>().Property(p => p.Deleted).HasDefaultValue(false);
            modelBuilder.Entity<Board>().Property(p => p.CreationAt).HasDefaultValue(DateTime.Now);
            modelBuilder.Entity<SharedBoard>().Property(p => p.GrantedAccessAt).HasDefaultValue(DateTime.Now);
            modelBuilder.Entity<User>().Property(p => p.SignupDate).HasDefaultValue(DateTime.Now);
            modelBuilder.Entity<Session>().Property(p => p.CreationAt).HasDefaultValue(DateTime.Now);
            modelBuilder.Entity<Task>().Property(p => p.Deleted).HasDefaultValue(false);
            modelBuilder.Entity<Task>().Property(p => p.Star).HasDefaultValue(false);
            modelBuilder.Entity<Task>().Property(p => p.CreationAt).HasDefaultValue(DateTime.Now);

            // seed the database with dummy data
            modelBuilder.Entity<User>().HasData(
                new User()
                {
                    Id = Guid.Parse("d28888e9-2ba9-473a-a40f-e38cb54f9b35"),
                    FirstName = "Berry",
                    LastName = "Griffin Beak Eldritch",
                    SignupDate = new DateTime(1650, 7, 23),
                    PhoneNumber = "Ships"
                },
                new User()
                {
                    Id = Guid.Parse("da2fd609-d754-4feb-8acd-c4f9ff13ba96"),
                    FirstName = "Nancy",
                    LastName = "Swashbuckler Rye",
                    SignupDate = new DateTime(1668, 5, 21),
                    PhoneNumber = "Rum"
                },
                new User()
                {
                    Id = Guid.Parse("2902b665-1190-4c70-9915-b9c2d7680450"),
                    FirstName = "Eli",
                    LastName = "Ivory Bones Sweet",
                    SignupDate = new DateTime(1701, 12, 16),
                    PhoneNumber = "Singing"
                },
                new User()
                {
                    Id = Guid.Parse("102b566b-ba1f-404c-b2df-e2cde39ade09"),
                    FirstName = "Arnold",
                    LastName = "The Unseen Stafford",
                    SignupDate = new DateTime(1702, 3, 6),
                    PhoneNumber = "Singing"
                },
                new User()
                {
                    Id = Guid.Parse("5b3621c0-7b12-4e80-9c8b-3398cba7ee05"),
                    FirstName = "Seabury",
                    LastName = "Toxic Reyson",
                    SignupDate = new DateTime(1690, 11, 23),
                    PhoneNumber = "Maps"
                },
                new User()
                {
                    Id = Guid.Parse("2aadd2df-7caf-45ab-9355-7f6332985a87"),
                    FirstName = "Rutherford",
                    LastName = "Fearless Cloven",
                    SignupDate = new DateTime(1723, 4, 5),
                    PhoneNumber = "General debauchery"
                },
                new User()
                {
                    Id = Guid.Parse("2ee49fe3-edf2-4f91-8409-3eb25ce6ca51"),
                    FirstName = "Atherton",
                    LastName = "Crow Ridley",
                    SignupDate = new DateTime(1721, 10, 11),
                    PhoneNumber = "Rum"
                }
                );

            base.OnModelCreating(modelBuilder);
        }
    }
}
