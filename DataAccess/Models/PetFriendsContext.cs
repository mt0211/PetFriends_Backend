using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Models;

public partial class PetFriendsContext : DbContext
{
    public PetFriendsContext()
    {
    }

    public PetFriendsContext(DbContextOptions<PetFriendsContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Appointment> Appointments { get; set; }

    public virtual DbSet<Clinicservice> Clinicservices { get; set; }

    public virtual DbSet<Feedback> Feedbacks { get; set; }

    public virtual DbSet<ForumPost> ForumPosts { get; set; }

    public virtual DbSet<OtpVerify> OtpVerifies { get; set; }

    public virtual DbSet<Pet> Pets { get; set; }

    public virtual DbSet<PetVaccine> PetVaccines { get; set; }

    public virtual DbSet<Promotion> Promotions { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<Vaccine> Vaccines { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=tcp:petfriends.database.windows.net,1433;Initial Catalog=PetFriends;Persist Security Info=False;User ID=admin123;Password=Admin@123;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=240;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_appointment");

            entity.ToTable("Appointment");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasPrecision(6);
            entity.Property(e => e.EndAt).HasPrecision(6);
            entity.Property(e => e.StartAt).HasPrecision(6);

            entity.HasOne(d => d.Pet).WithMany(p => p.Appointments)
                .HasForeignKey(d => d.PetId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_appointment_Pet");

            entity.HasOne(d => d.User).WithMany(p => p.Appointments)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_appointment_User");
        });

        modelBuilder.Entity<Clinicservice>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_clinicservice");

            entity.ToTable("Clinicservice");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreateAt).HasPrecision(6);
        });

        modelBuilder.Entity<Feedback>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_feedback");

            entity.ToTable("Feedback");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasPrecision(6);

            entity.HasOne(d => d.User).WithMany(p => p.Feedbacks)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_feedback_User");
        });

        modelBuilder.Entity<ForumPost>(entity =>
        {
            entity.ToTable("ForumPost");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasPrecision(6);
            entity.Property(e => e.Status).HasMaxLength(50);

            entity.HasOne(d => d.User).WithMany(p => p.ForumPosts)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_ForumPost_User");
        });

        modelBuilder.Entity<OtpVerify>(entity =>
        {
            entity.ToTable("OtpVerify");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasPrecision(6);
            entity.Property(e => e.ExpiredAt).HasPrecision(6);
            entity.Property(e => e.OtpCode).HasMaxLength(50);

            entity.HasOne(d => d.User).WithMany(p => p.OtpVerifies)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_OtpVerify_User");
        });

        modelBuilder.Entity<Pet>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Pet__3214EC07A2E4DD17");

            entity.ToTable("Pet");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Breed).HasMaxLength(50);
            entity.Property(e => e.DateOfBirth).HasPrecision(6);
            entity.Property(e => e.Description).HasMaxLength(250);
            entity.Property(e => e.Gender).HasMaxLength(50);
            entity.Property(e => e.Name).HasMaxLength(50);
            entity.Property(e => e.Species).HasMaxLength(50);
            entity.Property(e => e.UserPhoneNumber).HasMaxLength(50);
            entity.Property(e => e.Weight).HasColumnType("decimal(5, 2)");

            entity.HasOne(d => d.User).WithMany(p => p.Pets)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_Pet_UserId");
        });

        modelBuilder.Entity<PetVaccine>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__PetVacci__3214EC071628EA17");

            entity.ToTable("PetVaccine");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.DateGiven).HasPrecision(6);
            entity.Property(e => e.Notes).HasMaxLength(250);

            entity.HasOne(d => d.Pet).WithMany(p => p.PetVaccines)
                .HasForeignKey(d => d.PetId)
                .HasConstraintName("FK_PetVaccine_Pet");

            entity.HasOne(d => d.Vaccine).WithMany(p => p.PetVaccines)
                .HasForeignKey(d => d.VaccineId)
                .HasConstraintName("FK_PetVaccine_Vaccine");
        });

        modelBuilder.Entity<Promotion>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_promotion");

            entity.ToTable("Promotion");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.DiscountRate).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.EndDate).HasPrecision(6);
            entity.Property(e => e.IsActive).HasDefaultValue((byte)1);
            entity.Property(e => e.StartDate).HasPrecision(6);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("User");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Address).HasMaxLength(50);
            entity.Property(e => e.AvatarUrl).HasMaxLength(250);
            entity.Property(e => e.CreatedAt).HasPrecision(6);
            entity.Property(e => e.Dob)
                .HasPrecision(6)
                .HasColumnName("DOB");
            entity.Property(e => e.Email).HasMaxLength(50);
            entity.Property(e => e.FullName).HasMaxLength(50);
            entity.Property(e => e.Gender).HasMaxLength(50);
            entity.Property(e => e.LastLoggedIn).HasPrecision(6);
            entity.Property(e => e.Password).HasMaxLength(512);
            entity.Property(e => e.PhoneNumber).HasMaxLength(50);
            entity.Property(e => e.Role).HasMaxLength(50);
            entity.Property(e => e.Salt).HasMaxLength(512);
            entity.Property(e => e.Status).HasMaxLength(50);
        });

        modelBuilder.Entity<Vaccine>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Vaccine__3214EC07886CE678");

            entity.ToTable("Vaccine");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Name).HasMaxLength(50);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
