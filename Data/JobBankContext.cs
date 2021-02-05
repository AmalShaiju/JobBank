using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using sol_Job_Bank.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace sol_Job_Bank.Data
{
    public class JobBankContext : DbContext
    {
        //To give access to IHttpContextAccessor for Audit Data with IAuditable
        private readonly IHttpContextAccessor _httpContextAccessor;

        //Property to hold the UserName value
        public string UserName
        {
            get; private set;
        }

        public JobBankContext(DbContextOptions<JobBankContext> options)
            : base(options)
        {
            UserName = "SeedData";
        }
        public JobBankContext(DbContextOptions<JobBankContext> options,
            IHttpContextAccessor httpContextAccessor)
            : base(options)
        {
            _httpContextAccessor = httpContextAccessor;
            UserName = _httpContextAccessor.HttpContext?.User.Identity.Name;
            UserName = UserName ?? "Unknown";
        }

        public DbSet<Occupation> Occupations { get; set; }
        public DbSet<Position> Positions { get; set; }
        public DbSet<Posting> Postings { get; set; }
        public DbSet<Applicant> Applicants { get; set; }
        public DbSet<Application> Applications { get; set; }
        public DbSet<ApplicantSkill> ApplicantSkills { get; set; }
        public DbSet<Skill> Skills { get; set; }
        public DbSet<RetrainingProgram> RetrainingPrograms { get; set; }
        public DbSet<PositionSkill> PositionSkills { get; set; }
        public DbSet<PostingDocument> PostingDocuments { get; set; }
        public DbSet<ApplicantDocument> ApplicantDocuments { get; set; }
        public DbSet<ApplicantPhoto> ApplicantPhotos { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("JB");

            //Unique Skill Name
            modelBuilder.Entity<Skill>()
                .HasIndex(p => p.Name)
                .IsUnique();

            //Intersection Primary Key
            modelBuilder.Entity<PositionSkill>()
                .HasKey(a => new { a.PositionID, a.SkillID });

            //Intersection Primary Key
            modelBuilder.Entity<ApplicantSkill>()
                .HasKey(a => new { a.ApplicantID, a.SkillID });

            //Unique Position Name
            modelBuilder.Entity<Position>()
                .HasIndex(p => p.Name)
                .IsUnique();

            //Unique Posting
            modelBuilder.Entity<Posting>()
                .HasIndex(p => new { p.PositionID, p.ClosingDate })
                .IsUnique();

            //Unique Applicant eMail
            modelBuilder.Entity<Applicant>()
                .HasIndex(p => p.eMail)
                .IsUnique()
                .HasName("IX_Unique_Applicant_email");

            //Unique Application
            modelBuilder.Entity<Application>()
                .HasIndex(p => new { p.ApplicantID, p.PostingID })
                .IsUnique()
                .HasName("IX_Unique_Application");

            //Prevent Cascade Delete from Occupation to Position
            //so we are prevented from deleting an Occupation with
            //Positions referencing it
            modelBuilder.Entity<Occupation>()
                .HasMany<Position>(d => d.Positions)
                .WithOne(p => p.Occupation)
                .HasForeignKey(p => p.OccupationID)
                .OnDelete(DeleteBehavior.Restrict);

            //Add this so you don't get Cascade Delete
            modelBuilder.Entity<Skill>()
                .HasMany<PositionSkill>(p => p.PositionSkills)
                .WithOne(c => c.Skill)
                .HasForeignKey(c => c.SkillID)
                .OnDelete(DeleteBehavior.Restrict);

            //Prevent Cascade Delete from Skill to ApplicantSkill
            modelBuilder.Entity<Skill>()
                .HasMany<ApplicantSkill>(d => d.ApplicantSkills)
                .WithOne(p => p.Skill)
                .HasForeignKey(p => p.SkillID)
                .OnDelete(DeleteBehavior.Restrict);

            //Prevent Cascade Delete from Position to Posting
            modelBuilder.Entity<Position>()
                .HasMany<Posting>(d => d.Postings)
                .WithOne(p => p.Position)
                .HasForeignKey(p => p.PositionID)
                .OnDelete(DeleteBehavior.Restrict);

            //Prevent Cascade Delete from Posting to Application
            modelBuilder.Entity<Posting>()
                .HasMany<Application>(d => d.Applications)
                .WithOne(p => p.Posting)
                .HasForeignKey(p => p.PostingID)
                .OnDelete(DeleteBehavior.Restrict);

            //NOTE: We will allow Cascade Delete from Applicant to Application
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            OnBeforeSaving();
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default(CancellationToken))
        {
            OnBeforeSaving();
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        private void OnBeforeSaving()
        {
            var entries = ChangeTracker.Entries();
            foreach (var entry in entries)
            {
                if (entry.Entity is IAuditable trackable)
                {
                    var now = DateTime.UtcNow;
                    switch (entry.State)
                    {
                        case EntityState.Modified:
                            trackable.UpdatedOn = now;
                            trackable.UpdatedBy = UserName;
                            break;

                        case EntityState.Added:
                            trackable.CreatedOn = now;
                            trackable.CreatedBy = UserName;
                            trackable.UpdatedOn = now;
                            trackable.UpdatedBy = UserName;
                            break;
                    }
                }
            }
        }
    }
}
