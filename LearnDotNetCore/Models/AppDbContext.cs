using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LearnDotNetCore.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> dbContextOptions) : base(dbContextOptions)
        {

        }

        public DbSet<Employee> Employees { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Employee>().HasData(
                new Employee()
                {
                    Name = "Auto Seed",
                    Department = Department.Hardware,
                    Email = "autoseed@smarttech.com",
                    Id = -1
                }
            );

            modelBuilder.Entity<Employee>().Ignore(x => x.Photo);
            base.OnModelCreating(modelBuilder);
            
        }
    }
}
