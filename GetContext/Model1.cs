namespace GetContext
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class Model1 : DbContext
    {
        public Model1()
            : base("name=Model11")
        {
        }

        public virtual DbSet<appointments> appointments { get; set; }
        public virtual DbSet<conclusions> conclusions { get; set; }
        public virtual DbSet<cures> cures { get; set; }
        public virtual DbSet<cures_in_conclusions> cures_in_conclusions { get; set; }
        public virtual DbSet<doctors> doctors { get; set; }
        public virtual DbSet<users> users { get; set; }
        public virtual DbSet<files> files { get; set; }
        public virtual DbSet<notifications> notifications { get; set; }
        public virtual DbSet<questions> questions { get; set; }
        public virtual DbSet<questions_answers> questions_answers { get; set; }
        public virtual DbSet<sources> sources { get; set; }
        public virtual DbSet<table_types> table_types { get; set; }
        public virtual DbSet<users1> users1 { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<conclusions>()
                .HasMany(e => e.cures_in_conclusions)
                .WithRequired(e => e.conclusions)
                .HasForeignKey(e => e.id_conclusion)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<cures>()
                .HasMany(e => e.cures_in_conclusions)
                .WithRequired(e => e.cures)
                .HasForeignKey(e => e.id_cure)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<doctors>()
                .HasMany(e => e.appointments)
                .WithRequired(e => e.doctors)
                .HasForeignKey(e => e.id_doctor)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<doctors>()
                .HasMany(e => e.conclusions)
                .WithRequired(e => e.doctors)
                .HasForeignKey(e => e.id_doctor)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<users>()
                .HasMany(e => e.conclusions)
                .WithRequired(e => e.users)
                .HasForeignKey(e => e.id_user)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<questions>()
                .HasMany(e => e.questions_answers)
                .WithRequired(e => e.questions)
                .HasForeignKey(e => e.id_question)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<sources>()
                .HasMany(e => e.files)
                .WithRequired(e => e.sources)
                .HasForeignKey(e => e.id_source)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<sources>()
                .HasMany(e => e.users1)
                .WithRequired(e => e.sources)
                .HasForeignKey(e => e.id_source)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<users1>()
                .Property(e => e.phone_number)
                .IsFixedLength();

            modelBuilder.Entity<users1>()
                .HasMany(e => e.files)
                .WithRequired(e => e.users1)
                .HasForeignKey(e => e.id_user)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<users1>()
                .HasMany(e => e.questions_answers)
                .WithRequired(e => e.users1)
                .HasForeignKey(e => e.id_user)
                .WillCascadeOnDelete(false);
        }
    }
}
