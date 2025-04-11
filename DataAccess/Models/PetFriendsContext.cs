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

    public virtual DbSet<Activity> Activities { get; set; }

    public virtual DbSet<Appointment> Appointments { get; set; }

    public virtual DbSet<AppointmentClinicService> AppointmentClinicServices { get; set; }

    public virtual DbSet<AppointmentPromotion> AppointmentPromotions { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<ChatMessage> ChatMessages { get; set; }

    public virtual DbSet<ClinicService> ClinicServices { get; set; }

    public virtual DbSet<DailyRevenueSummary> DailyRevenueSummaries { get; set; }

    public virtual DbSet<Diagnosis> Diagnoses { get; set; }

    public virtual DbSet<Feedback> Feedbacks { get; set; }

    public virtual DbSet<ForumComment> ForumComments { get; set; }

    public virtual DbSet<ForumPost> ForumPosts { get; set; }

    public virtual DbSet<GuestPet> GuestPets { get; set; }

    public virtual DbSet<GuestUser> GuestUsers { get; set; }

    public virtual DbSet<OtpVerify> OtpVerifies { get; set; }

    public virtual DbSet<Pet> Pets { get; set; }

    public virtual DbSet<PetVaccine> PetVaccines { get; set; }

    public virtual DbSet<Promotion> Promotions { get; set; }

    public virtual DbSet<ServiceRevenue> ServiceRevenues { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserBookingSummary> UserBookingSummaries { get; set; }

    public virtual DbSet<UserCart> UserCarts { get; set; }

    public virtual DbSet<UserCartItem> UserCartItems { get; set; }

    public virtual DbSet<UserPetVaccine> UserPetVaccines { get; set; }

    public virtual DbSet<UserPetVaccineDose> UserPetVaccineDoses { get; set; }

    public virtual DbSet<UserPostReaction> UserPostReactions { get; set; }

    public virtual DbSet<Vaccine> Vaccines { get; set; }

    public virtual DbSet<VaccineDose> VaccineDoses { get; set; }

    public virtual DbSet<VideoCall> VideoCalls { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=160.250.133.192,1433;Database=petfriends;User Id=sa;Password=Admin@123;Encrypt=False;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Activity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Activity__3214EC07D750EB9D");

            entity.ToTable("Activity");

            entity.HasIndex(e => e.CreatedAt, "IX_Activity_CreatedAt").IsDescending();

            entity.HasIndex(e => e.UserId, "IX_Activity_UserId");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Title).HasMaxLength(255);
            entity.Property(e => e.Type).HasMaxLength(50);

            entity.HasOne(d => d.Appointment).WithMany(p => p.Activities)
                .HasForeignKey(d => d.AppointmentId)
                .HasConstraintName("FK_Activity_Appointment");

            entity.HasOne(d => d.ClinicService).WithMany(p => p.Activities)
                .HasForeignKey(d => d.ClinicServiceId)
                .HasConstraintName("FK_Activity_ClinicService");

            entity.HasOne(d => d.Pet).WithMany(p => p.Activities)
                .HasForeignKey(d => d.PetId)
                .HasConstraintName("FK_Activity_Pet");

            entity.HasOne(d => d.User).WithMany(p => p.Activities)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_Activity_User");
        });

        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Appointm__3214EC0796C4125D");

            entity.ToTable("Appointment");

            entity.HasIndex(e => e.CreatedAt, "IX_Appointment_CreatedAt").IsDescending();

            entity.HasIndex(e => e.GuestPetId, "IX_Appointment_GuestPetId");

            entity.HasIndex(e => e.GuestUserId, "IX_Appointment_GuestUserId");

            entity.HasIndex(e => e.PetId, "IX_Appointment_PetId");

            entity.HasIndex(e => e.StartAt, "IX_Appointment_StartAt");

            entity.HasIndex(e => e.Status, "IX_Appointment_Status");

            entity.HasIndex(e => new { e.Status, e.CreatedAt }, "IX_Appointment_StatusAndCreatedAt").IsDescending(false, true);

            entity.HasIndex(e => e.UserId, "IX_Appointment_UserId");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.DiscountAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.EndAt).HasColumnType("datetime");
            entity.Property(e => e.FinalAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.StartAt).HasColumnType("datetime");
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.GuestPet).WithMany(p => p.Appointments)
                .HasForeignKey(d => d.GuestPetId)
                .HasConstraintName("FK_Appointment_GuestPetId");

            entity.HasOne(d => d.GuestUser).WithMany(p => p.Appointments)
                .HasForeignKey(d => d.GuestUserId)
                .HasConstraintName("FK_Appointment_GuestUserId");

            entity.HasOne(d => d.Pet).WithMany(p => p.Appointments)
                .HasForeignKey(d => d.PetId)
                .HasConstraintName("FK__Appointme__PetId__693CA210");

            entity.HasOne(d => d.User).WithMany(p => p.Appointments)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__Appointme__UserI__6A30C649");
        });

        modelBuilder.Entity<AppointmentClinicService>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Appointm__3214EC0751FA4516");

            entity.ToTable("AppointmentClinicService");

            entity.HasIndex(e => e.AppointmentId, "IX_AppointmentClinicService_AppointmentId");

            entity.HasIndex(e => e.ClinicServiceId, "IX_AppointmentClinicService_ClinicServiceId");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.DateGiven).HasColumnType("datetime");
            entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Appointment).WithMany(p => p.AppointmentClinicServices)
                .HasForeignKey(d => d.AppointmentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Appointme__Appoi__6D0D32F4");

            entity.HasOne(d => d.ClinicService).WithMany(p => p.AppointmentClinicServices)
                .HasForeignKey(d => d.ClinicServiceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Appointme__Clini__6E01572D");
        });

        modelBuilder.Entity<AppointmentPromotion>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Appointm__3214EC0764A57822");

            entity.ToTable("AppointmentPromotion");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreateAt).HasColumnType("datetime");
            entity.Property(e => e.DiscountAmount).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Appointment).WithMany(p => p.AppointmentPromotions)
                .HasForeignKey(d => d.AppointmentId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_AppointmentPromotion_Appointment");

            entity.HasOne(d => d.Promotion).WithMany(p => p.AppointmentPromotions)
                .HasForeignKey(d => d.PromotionId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_AppointmentPromotion_Promotion");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Category__3214EC07C0C3DB31");

            entity.ToTable("Category");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Name).HasMaxLength(255);
        });

        modelBuilder.Entity<ChatMessage>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ChatMess__3214EC077B19C47B");

            entity.ToTable("ChatMessage");

            entity.HasIndex(e => e.ReceiverId, "IX_ChatMessage_ReceiverId");

            entity.HasIndex(e => e.SenderId, "IX_ChatMessage_SenderId");

            entity.HasIndex(e => e.SentTime, "IX_ChatMessage_SentTime").IsDescending();

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.MessageType)
                .HasMaxLength(20)
                .HasDefaultValue("Text");
            entity.Property(e => e.SentTime).HasColumnType("datetime");

            entity.HasOne(d => d.Receiver).WithMany(p => p.ChatMessageReceivers)
                .HasForeignKey(d => d.ReceiverId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ChatMessage_ReceiverUser");

            entity.HasOne(d => d.Sender).WithMany(p => p.ChatMessageSenders)
                .HasForeignKey(d => d.SenderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ChatMessage_SenderUser");
        });

        modelBuilder.Entity<ClinicService>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ClinicSe__3214EC071AA777F3");

            entity.ToTable("ClinicService");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CreateAt).HasColumnType("datetime");
            entity.Property(e => e.DiscountAmount).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.DiscountFrom).HasColumnType("datetime");
            entity.Property(e => e.DiscountTo).HasColumnType("datetime");
            entity.Property(e => e.DiscountedPrice)
                .HasComputedColumnSql("(case when getutcdate()>=[DiscountFrom] AND getutcdate()<=[DiscountTo] then [Price]-isnull([DiscountAmount],(0)) else [Price] end)", false)
                .HasColumnType("decimal(13, 2)");
            entity.Property(e => e.EstimateTime).HasMaxLength(50);
            entity.Property(e => e.Name).HasMaxLength(255);
            entity.Property(e => e.Price).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.Status).HasMaxLength(50);

            entity.HasOne(d => d.CategoryNavigation).WithMany(p => p.ClinicServices)
                .HasForeignKey(d => d.Category)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_ClinicService_Category");
        });

        modelBuilder.Entity<DailyRevenueSummary>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__DailyRev__3214EC07776ACFF8");

            entity.ToTable("DailyRevenueSummary");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.TotalRevenue).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
        });

        modelBuilder.Entity<Diagnosis>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Diagnose__3213E83FD25F3E66");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Description)
                .HasMaxLength(500)
                .HasColumnName("description");
            entity.Property(e => e.FirstAid)
                .HasMaxLength(500)
                .HasColumnName("firstAid");
            entity.Property(e => e.Label)
                .HasMaxLength(255)
                .HasColumnName("label");
            entity.Property(e => e.Symptoms)
                .HasMaxLength(500)
                .HasColumnName("symptoms");
        });

        modelBuilder.Entity<Feedback>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Feedback__3214EC0791A23056");

            entity.ToTable("Feedback");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Sentiment).HasMaxLength(8);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.Appointment).WithMany(p => p.Feedbacks)
                .HasForeignKey(d => d.AppointmentId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_Feedback_Appointment");

            entity.HasOne(d => d.User).WithMany(p => p.Feedbacks)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Feedback__UserId__6FE99F9F");
        });

        modelBuilder.Entity<ForumComment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ForumCom__3214EC07B5CF1C8C");

            entity.ToTable("ForumComment");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Post).WithMany(p => p.ForumComments)
                .HasForeignKey(d => d.PostId)
                .HasConstraintName("FK_ForumComment_PostId");

            entity.HasOne(d => d.User).WithMany(p => p.ForumComments)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ForumComm__UserI__5AB9788F");
        });

        modelBuilder.Entity<ForumPost>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ForumPos__3214EC07D541B015");

            entity.ToTable("ForumPost");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.DislikeCount).HasDefaultValue(0);
            entity.Property(e => e.LikeCount).HasDefaultValue(0);
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.User).WithMany(p => p.ForumPosts)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__ForumPost__UserI__5224328E");
        });

        modelBuilder.Entity<GuestPet>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__GuestPet__3214EC07107266DF");

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
            entity.HasKey(e => e.Id).HasName("PK__GuestUse__3214EC07C057D53C");

            entity.HasIndex(e => e.PhoneNumber, "UQ__GuestUse__85FB4E38E38A6B36").IsUnique();

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
            entity.HasKey(e => e.Id).HasName("PK__OtpVerif__3214EC07EF011F7A");

            entity.ToTable("OtpVerify");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CreatedAt).HasPrecision(6);
            entity.Property(e => e.ExpiredAt).HasPrecision(6);
            entity.Property(e => e.OtpCode).HasMaxLength(50);

            entity.HasOne(d => d.User).WithMany(p => p.OtpVerifies)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__OtpVerify__UserI__72C60C4A");
        });

        modelBuilder.Entity<Pet>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Pet__3214EC0799932B4D");

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
                .HasConstraintName("FK__Pet__UserId__73BA3083");
        });

        modelBuilder.Entity<PetVaccine>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__PetVacci__3214EC07507A3640");

            entity.ToTable("PetVaccine");

            entity.Property(e => e.Id).HasDefaultValueSql("(newsequentialid())");
            entity.Property(e => e.DateGiven).HasColumnType("datetime");

            entity.HasOne(d => d.Pet).WithMany(p => p.PetVaccines)
                .HasForeignKey(d => d.PetId)
                .HasConstraintName("FK__PetVaccin__PetId__74AE54BC");

            entity.HasOne(d => d.Vaccine).WithMany(p => p.PetVaccines)
                .HasForeignKey(d => d.VaccineId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PetVaccin__Vacci__75A278F5");
        });

        modelBuilder.Entity<Promotion>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Promotio__3214EC07EEA3E054");

            entity.ToTable("Promotion");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Description).HasMaxLength(100);
            entity.Property(e => e.DiscountDetail).HasColumnType("decimal(14, 2)");
            entity.Property(e => e.EndDate).HasColumnType("datetime");
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.StartDate).HasColumnType("datetime");
            entity.Property(e => e.Status)
                .HasMaxLength(9)
                .IsUnicode(false)
                .HasComputedColumnSql("(case when getdate()>=[StartDate] AND getdate()<=[EndDate] then 'Active' when getdate()<[StartDate] then 'Scheduled' when getdate()>[EndDate] then 'Expired' else 'Unknown' end)", false);
            entity.Property(e => e.TargetGroup).HasMaxLength(50);

            entity.HasOne(d => d.Category).WithMany(p => p.Promotions)
                .HasForeignKey(d => d.CategoryId)
                .HasConstraintName("FK_Promotion_Category");
        });

        modelBuilder.Entity<ServiceRevenue>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ServiceR__3214EC076C7529BB");

            entity.ToTable("ServiceRevenue");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Revenue).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.ClinicService).WithMany(p => p.ServiceRevenues)
                .HasForeignKey(d => d.ClinicServiceId)
                .HasConstraintName("FK_ServiceRevenue_ClinicService");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__User__3214EC07A0EF3AC3");

            entity.ToTable("User");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Address).HasMaxLength(255);
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Dob).HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.FullName).HasMaxLength(255);
            entity.Property(e => e.Gender).HasMaxLength(50);
            entity.Property(e => e.LastLoggedIn).HasColumnType("datetime");
            entity.Property(e => e.PhoneNumber).HasMaxLength(50);
            entity.Property(e => e.Role).HasMaxLength(50);
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.Property(e => e.TypeGroup).HasMaxLength(20);
        });

        modelBuilder.Entity<UserBookingSummary>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__UserBook__3214EC07C33BA062");

            entity.ToTable("UserBookingSummary");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Amount).HasColumnType("decimal(14, 2)");

            entity.HasOne(d => d.User).WithMany(p => p.UserBookingSummaries)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_UserBookingSummary_User");
        });

        modelBuilder.Entity<UserCart>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__UserCart__3214EC07B8BCC9C2");

            entity.ToTable("UserCart");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Datebook).HasColumnType("datetime");

            entity.HasOne(d => d.User).WithMany(p => p.UserCarts)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_UserCart_User");
        });

        modelBuilder.Entity<UserCartItem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__UserCart__3214EC078D7833F7");

            entity.ToTable("UserCartItem");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Cart).WithMany(p => p.UserCartItems)
                .HasForeignKey(d => d.CartId)
                .HasConstraintName("FK__UserCartI__CartI__43A1090D");

            entity.HasOne(d => d.ClinicService).WithMany(p => p.UserCartItems)
                .HasForeignKey(d => d.ClinicServiceId)
                .HasConstraintName("FK_UserCartItem_ClinicService");

            entity.HasOne(d => d.Pet).WithMany(p => p.UserCartItems)
                .HasForeignKey(d => d.PetId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_UserCartItem_Pet");
        });

        modelBuilder.Entity<UserPetVaccine>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__UserPetV__3214EC0722618450");

            entity.ToTable("UserPetVaccine");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Name).HasMaxLength(255);
            entity.Property(e => e.NumberOfDoses).HasColumnName("Number_of_Doses");

            entity.HasOne(d => d.Pet).WithMany(p => p.UserPetVaccines)
                .HasForeignKey(d => d.PetId)
                .HasConstraintName("FK__UserPetVa__PetId__2EA5EC27");

            entity.HasOne(d => d.Vaccine).WithMany(p => p.UserPetVaccines)
                .HasForeignKey(d => d.VaccineId)
                .HasConstraintName("FK__UserPetVa__Vacci__2F9A1060");
        });

        modelBuilder.Entity<UserPetVaccineDose>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__UserPetV__3214EC077AF9E4A0");

            entity.ToTable("UserPetVaccineDose");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.DateGiven).HasColumnType("datetime");

            entity.HasOne(d => d.UserPetVaccine).WithMany(p => p.UserPetVaccineDoses)
                .HasForeignKey(d => d.UserPetVaccineId)
                .HasConstraintName("FK__UserPetVa__UserP__32767D0B");
        });

        modelBuilder.Entity<UserPostReaction>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__UserPost__3214EC0706AD0336");

            entity.ToTable("UserPostReaction");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.Post).WithMany(p => p.UserPostReactions)
                .HasForeignKey(d => d.PostId)
                .HasConstraintName("FK_UserPostReaction_Post");

            entity.HasOne(d => d.User).WithMany(p => p.UserPostReactions)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_UserPostReaction_User");
        });

        modelBuilder.Entity<Vaccine>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Vaccine__3214EC073BA4BE59");

            entity.ToTable("Vaccine");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Name).HasMaxLength(255);
            entity.Property(e => e.NumberOfDoses).HasColumnName("Number_of_Doses");
        });

        modelBuilder.Entity<VaccineDose>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__VaccineD__3214EC07609FEAE2");

            entity.ToTable("VaccineDose");

            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.HasOne(d => d.Vaccine).WithMany(p => p.VaccineDoses)
                .HasForeignKey(d => d.VaccineId)
                .HasConstraintName("FK__VaccineDo__Vacci__0880433F");
        });

        modelBuilder.Entity<VideoCall>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__VideoCal__3214EC078E38D705");

            entity.ToTable("VideoCall");

            entity.HasIndex(e => e.CallerId, "IX_VideoCall_CallerId");

            entity.HasIndex(e => e.ReceiverId, "IX_VideoCall_ReceiverId");

            entity.HasIndex(e => e.StartTime, "IX_VideoCall_StartTime").IsDescending();

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CallType)
                .HasMaxLength(20)
                .HasDefaultValue("Video");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.EndTime).HasColumnType("datetime");
            entity.Property(e => e.StartTime).HasColumnType("datetime");
            entity.Property(e => e.Status).HasMaxLength(50);

            entity.HasOne(d => d.Caller).WithMany(p => p.VideoCallCallers)
                .HasForeignKey(d => d.CallerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_VideoCall_CallerUser");

            entity.HasOne(d => d.Receiver).WithMany(p => p.VideoCallReceivers)
                .HasForeignKey(d => d.ReceiverId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_VideoCall_ReceiverUser");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
