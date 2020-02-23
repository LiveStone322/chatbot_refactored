using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace WebApp
{
    class HealthBotContext : DbContext
    {
        public virtual DbSet<files> Files { get; set; }
        public virtual DbSet<notifications> Notifications { get; set; }
        public virtual DbSet<questions> Questions { get; set; }
        public virtual DbSet<questions_answers> Questions_answers { get; set; }
        public virtual DbSet<sources> Sources { get; set; }
        public virtual DbSet<users> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<questions>()
                .HasMany(e => e.questions_answers)
                .WithOne(e => e.questions)
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
                .HasMany(e => e.files)
                .WithOne(e => e.users)
                .HasForeignKey(e => e.id_user)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<users>()
                .HasMany(e => e.notifications)
                .WithOne(e => e.users)
                .HasForeignKey(e => e.id_user)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<users>()
                .HasMany(e => e.questions_answers)
                .WithOne(e => e.users)
                .HasForeignKey(e => e.id_user)
                .OnDelete(DeleteBehavior.Restrict);
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseNpgsql($"Host={AppInfo.DbHost};Database={AppInfo.DbName};" +
                $"Username={AppInfo.DbLogin};Password={AppInfo.DbPassword}");

    }
}
