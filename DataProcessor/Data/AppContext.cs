using DataProcessor.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataProcessor.Data
{
    public class AppContext : DbContext
    {
        public DbSet<Module>? Modules { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=DataProcessorDatabase.db");
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Определение структуры таблицы в базе данных
            modelBuilder.Entity<Module>()
             .HasKey(e => e.Id);

            modelBuilder.Entity<Module>()
                .HasIndex(e => e.ModuleCategoryID)
                .IsUnique();

            modelBuilder.Entity<Module>()
                .Property(e => e.ModuleCategoryID)
                .IsRequired();

            modelBuilder.Entity<Module>()
                .Property(e => e.ModuleState)
                .IsRequired();
        }
    }
}
