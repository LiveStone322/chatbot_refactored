namespace GetContext
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class Model1 : DbContext
    {
        public Model1()
            : base("name=Model13")
        {
        }

        public virtual DbSet<files> files { get; set; }
        public virtual DbSet<notifications> notifications { get; set; }
        public virtual DbSet<questions> questions { get; set; }
        public virtual DbSet<questions_answers> questions_answers { get; set; }
        public virtual DbSet<sources> sources { get; set; }
        public virtual DbSet<table_types> table_types { get; set; }
        public virtual DbSet<users> users { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
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

            modelBuilder.Entity<users>()
                .Property(e => e.phone_number)
                .IsFixedLength();

            modelBuilder.Entity<users>()
                .HasMany(e => e.files)
                .WithRequired(e => e.users)
                .HasForeignKey(e => e.id_user)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<users>()
                .HasMany(e => e.notifications)
                .WithRequired(e => e.users)
                .HasForeignKey(e => e.id_user)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<users>()
                .HasMany(e => e.questions_answers)
                .WithRequired(e => e.users)
                .HasForeignKey(e => e.id_user)
                .WillCascadeOnDelete(false);
        }
    }
}
