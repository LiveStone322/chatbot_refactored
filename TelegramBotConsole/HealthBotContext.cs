using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace TelegramBotConsole
{
    class HealthBotContext : DbContext
    {
        public DbSet<users> Users { get; set; }
        public DbSet<files> Files { get; set; }
        public DbSet<questions> Questions { get; set; }
        public DbSet<questions_answers> Questions_answers { get; set; }
        public DbSet<sources> Sources { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<questions>()
                .HasMany(e => e.biomarkers_values)
                .WithOne(e => e.biomarkers)
                .HasForeignKey(e => e.id_question)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<sources>()
                .HasMany(e => e.files)
                .WithOne(e => e.sources)
                .HasForeignKey(e => e.id_source)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<users>()
                .Property(e => e.phone_number)
                .IsFixedLength();

            modelBuilder.Entity<users>()
                .HasMany(e => e.biomarkers_values)
                .WithOne(e => e.users)
                .HasForeignKey(e => e.id_user)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<users>()
                .HasMany(e => e.files)
                .WithOne(e => e.users)
                .HasForeignKey(e => e.id_user)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<questions_answers>()
                .HasKey(e => new { e.id_question, e.id_user });
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseNpgsql($"Host={AppInfo.DbHost};Database={AppInfo.DbName};" +
                $"Username={AppInfo.DbLogin};Password={AppInfo.DbPassword}");

    }
}
