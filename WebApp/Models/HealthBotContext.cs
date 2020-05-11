using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace WebApp
{
    class HealthBotContext : DbContext
    {
        public virtual DbSet<biomarks> biomarks { get; set; }
        public virtual DbSet<files> files { get; set; }
        public virtual DbSet<notifications> notifications { get; set; }
        public virtual DbSet<questions> questions { get; set; }
        public virtual DbSet<questions_answers> questions_answers { get; set; }
        public virtual DbSet<sources> sources { get; set; }
        public virtual DbSet<system_messages> system_messages { get; set; }
        public virtual DbSet<users> users { get; set; }
        public virtual DbSet<users_biomarks> users_biomarks { get; set; }
        public virtual DbSet<knowledge_base> knowledge_base { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<biomarks>()
               .HasMany(e => e.questions)
               .WithOne(e => e.biomarks)
               .HasForeignKey(e => e.id_biomark);

            modelBuilder.Entity<biomarks>()
                .HasMany(e => e.questions_answers)
                .WithOne(e => e.questions)
                .HasForeignKey(e => e.id_question)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<biomarks>()
                .HasMany(e => e.users_biomarks)
                .WithOne(e => e.biomarks)
                .HasForeignKey(e => e.id_biomark);

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

            modelBuilder.Entity<users>()
                .HasMany(e => e.users_biomarks)
                .WithOne(e => e.users)
                .HasForeignKey(e => e.id_user)
                .OnDelete(DeleteBehavior.Restrict);

        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseNpgsql($"Host={AppInfo.DbHost};Database={AppInfo.DbName};" +
                $"Username={AppInfo.DbLogin};Password={AppInfo.DbPassword}");

    }
}
