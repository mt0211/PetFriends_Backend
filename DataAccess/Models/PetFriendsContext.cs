using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Models;

public partial class PetfriendsContext : DbContext
{
    public PetfriendsContext()
    {
    }

    public PetfriendsContext(DbContextOptions<PetfriendsContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Appointment> Appointments { get; set; }

    public virtual DbSet<AppointmentClinicService> AppointmentClinicServices { get; set; }

    public virtual DbSet<ClinicService> ClinicServices { get; set; }

    public virtual DbSet<Feedback> Feedbacks { get; set; }

    public virtual DbSet<ForumPost> ForumPosts { get; set; }

    public virtual DbSet<GuestPet> GuestPets { get; set; }

    public virtual DbSet<GuestUser> GuestUsers { get; set; }

    public virtual DbSet<OtpVerify> OtpVerifies { get; set; }

    public virtual DbSet<Pet> Pets { get; set; }

    public virtual DbSet<PetVaccine> PetVaccines { get; set; }

    public virtual DbSet<Promotion> Promotions { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<Vaccine> Vaccines { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=103.75.180.192,1433;Database=petfriends;User Id=sa;Password=Admin@123;Encrypt=False;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Appointm__3214EC07DB5DA627");

            entity.ToTable("Appointment");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.EndAt).HasColumnType("datetime");
            entity.Property(e => e.StartAt).HasColumnType("datetime");
            entity.Property(e => e.Status).HasMaxLength(50);

            entity.HasOne(d => d.GuestPet).WithMany(p => p.Appointments)
                .HasForeignKey(d => d.GuestPetId)
                .HasConstraintName("FK_Appointment_GuestPetId");

            entity.HasOne(d => d.GuestUser).WithMany(p => p.Appointments)
                .HasForeignKey(d => d.GuestUserId)
                .HasConstraintName("FK_Appointment_GuestUserId");

            entity.HasOne(d => d.Pet).WithMany(p => p.Appointments)
                .HasForeignKey(d => d.PetId)
                .HasConstraintName("FK__Appointme__PetId__7D439ABD");

            entity.HasOne(d => d.User).WithMany(p => p.Appointments)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__Appointme__UserI__7E37BEF6");
        });

        modelBuilder.Entity<AppointmentClinicService>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Appointm__3214EC0743306B69");

            entity.ToTable("AppointmentClinicService");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.DateGiven).HasColumnType("datetime");

            entity.HasOne(d => d.Appointment).WithMany(p => p.AppointmentClinicServices)
                .HasForeignKey(d => d.AppointmentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Appointme__Appoi__114A936A");

            entity.HasOne(d => d.ClinicService).WithMany(p => p.AppointmentClinicServices)
                .HasForeignKey(d => d.ClinicServiceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Appointme__Clini__123EB7A3");
        });

        modelBuilder.Entity<ClinicService>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ClinicSe__3214EC072F6208C0");

            entity.ToTable("ClinicService");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CreateAt).HasColumnType("datetime");
            entity.Property(e => e.Name).HasMaxLength(255);
        });

        modelBuilder.Entity<Feedback>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Feedback__3214EC071EDB6BC2");

            entity.ToTable("Feedback");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.User).WithMany(p => p.Feedbacks)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Feedback__UserId__6754599E");
        });

        modelBuilder.Entity<ForumPost>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ForumPos__3214EC07B8519389");

            entity.ToTable("ForumPost");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Status).HasMaxLength(50);

            entity.HasOne(d => d.User).WithMany(p => p.ForumPosts)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ForumPost__UserI__68487DD7");
        });

        modelBuilder.Entity<GuestPet>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__GuestPet__3214EC075CA90B40");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.DateOfBirth).HasColumnType("datetime");
            entity.Property(e => e.Gender).HasMaxLength(50);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Species).HasMaxLength(50);

            entity.HasOne(d => d.GuestUser).WithMany(p => p.GuestPets)
                .HasForeignKey(d => d.GuestUserId)
                .HasConstraintName("FK_GuestPets_GuestUsers");
        });

        modelBuilder.Entity<GuestUser>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__GuestUse__3214EC072E091C34");

            entity.HasIndex(e => e.PhoneNumber, "UQ__GuestUse__85FB4E3809B0CA6E").IsUnique();

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Address).HasMaxLength(255);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.PhoneNumber).HasMaxLength(15);
        });

        modelBuilder.Entity<OtpVerify>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__OtpVerif__3214EC077A4649AE");

            entity.ToTable("OtpVerify");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CreatedAt).HasPrecision(6);
            entity.Property(e => e.ExpiredAt).HasPrecision(6);
            entity.Property(e => e.OtpCode).HasMaxLength(50);

            entity.HasOne(d => d.User).WithMany(p => p.OtpVerifies)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__OtpVerify__UserI__6A30C649");
        });

        modelBuilder.Entity<Pet>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Pet__3214EC07252B8EDC");

            entity.ToTable("Pet");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Breed).HasMaxLength(50);
            entity.Property(e => e.DateOfBirth).HasColumnType("datetime");
            entity.Property(e => e.Gender).HasMaxLength(50);
            entity.Property(e => e.Name).HasMaxLength(255);
            entity.Property(e => e.Species).HasMaxLength(50);
            entity.Property(e => e.UserPhoneNumber).HasMaxLength(50);
            entity.Property(e => e.Weight).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.User).WithMany(p => p.Pets)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__Pet__UserId__6B24EA82");
        });

        modelBuilder.Entity<PetVaccine>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__PetVacci__3214EC0757BB1F16");

            entity.ToTable("PetVaccine");

            entity.Property(e => e.Id).HasDefaultValueSql("(newsequentialid())");
            entity.Property(e => e.DateGiven).HasColumnType("datetime");

            entity.HasOne(d => d.Pet).WithMany(p => p.PetVaccines)
                .HasForeignKey(d => d.PetId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PetVaccin__PetId__6C190EBB");

            entity.HasOne(d => d.Vaccine).WithMany(p => p.PetVaccines)
                .HasForeignKey(d => d.VaccineId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PetVaccin__Vacci__6D0D32F4");
        });

        modelBuilder.Entity<Promotion>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Promotio__3214EC07BFAFAB00");

            entity.ToTable("Promotion");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.DiscountRate).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.EndDate).HasColumnType("datetime");
            entity.Property(e => e.StartDate).HasColumnType("datetime");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__User__3214EC07AC849BC5");

            entity.ToTable("User");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Address).HasMaxLength(255);
            entity.Property(e => e.AvatarUrl).HasMaxLength(255);
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Dob).HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.FullName).HasMaxLength(255);
            entity.Property(e => e.Gender).HasMaxLength(50);
            entity.Property(e => e.LastLoggedIn).HasColumnType("datetime");
            entity.Property(e => e.PhoneNumber).HasMaxLength(50);
            entity.Property(e => e.Role).HasMaxLength(50);
            entity.Property(e => e.Status).HasMaxLength(50);
        });

        modelBuilder.Entity<Vaccine>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Vaccine__3214EC07F8B7F98F");

            entity.ToTable("Vaccine");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Name).HasMaxLength(255);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
