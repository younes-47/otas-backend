using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using OTAS.Models;

namespace OTAS.Data;

public partial class OtasContext : DbContext
{
    public OtasContext()
    {
    }

    public OtasContext(DbContextOptions<OtasContext> options)
        : base(options)
    {
    }

    public virtual DbSet<ActualRequester> ActualRequesters { get; set; }

    public virtual DbSet<Department> Departments { get; set; }

    public virtual DbSet<AvanceCaisse> AvanceCaisses { get; set; }

    public virtual DbSet<AvanceVoyage> AvanceVoyages { get; set; }

    public virtual DbSet<Delegation> Delegations { get; set; }

    public virtual DbSet<DepenseCaisse> DepenseCaisses { get; set; }

    public virtual DbSet<Expense> Expenses { get; set; }

    public virtual DbSet<Liquidation> Liquidations { get; set; }

    public virtual DbSet<OrdreMission> OrdreMissions { get; set; }

    public virtual DbSet<RoleCode> RoleCodes { get; set; }

    public virtual DbSet<StatusCode> StatusCodes { get; set; }

    public virtual DbSet<StatusHistory> StatusHistories { get; set; }

    public virtual DbSet<Trip> Trips { get; set; }

    public virtual DbSet<Decider> Deciders { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ActualRequester>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ActualRe__3214EC077090802F");

            entity.ToTable("ActualRequester");

            entity.Property(e => e.CreateDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.FirstName).HasMaxLength(50);
            entity.Property(e => e.RegistrationNumber).HasMaxLength(30);
            entity.Property(e => e.LastName).HasMaxLength(50);
            entity.Property(e => e.JobTitle).HasMaxLength(100);
            entity.Property(e => e.Department).HasMaxLength(100);
            //entity.Property(e => e.Manager)
            //    .HasMaxLength(50)
            //    .IsUnicode(false);

            entity.HasOne(d => d.AvanceCaisse).WithOne(p => p.ActualRequester)
                .HasForeignKey<ActualRequester>(d => d.AvanceCaisseId)
                .HasConstraintName("FK_AC_ActualRequester");

            entity.HasOne(d => d.DepenseCaisse).WithOne(p => p.ActualRequester)
                .HasForeignKey<ActualRequester>(d => d.DepenseCaisseId)
                .HasConstraintName("FK_DC_ActualRequester");

            entity.HasOne(d => d.OrderingUser).WithMany(p => p.OrderingUserActualRequesters)
                .HasForeignKey(d => d.OrderingUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_OrderingUser_ActualRequester");

            entity.HasOne(d => d.Manager).WithMany(p => p.ManagerUserActualRequesters)
                .HasForeignKey(d => d.ManagerUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Manager_ActualRequester");

            entity.HasOne(d => d.OrdreMission).WithOne(p => p.ActualRequester)
                .HasForeignKey<ActualRequester>(d => d.OrdreMissionId)
                .HasConstraintName("FK_OM_ActualRequester");

        });

        modelBuilder.Entity<AvanceCaisse>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__AvanceCa__3214EC07B6932794");

            entity.ToTable("AvanceCaisse");

            entity.Property(e => e.ActualTotal)
                .HasDefaultValueSql("((0.00))")
                .HasColumnType("decimal(10, 2)");
            entity.Property(e => e.CreateDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Currency)
                .HasMaxLength(5)
                .IsUnicode(false);
            entity.Property(e => e.DeciderComment).HasMaxLength(255);
            entity.Property(e => e.AdvanceOption).HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.EstimatedTotal)
                .HasColumnType("decimal(10, 2)");
            entity.Property(e => e.LatestStatus).HasDefaultValueSql("((99))");

            entity.HasOne(d => d.LatestStatusNavigation).WithMany(p => p.AvanceCaisses)
                .HasForeignKey(d => d.LatestStatus)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AC_StatusCode");

            entity.HasOne(d => d.User).WithMany(p => p.AvanceCaisses)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AC_Requester");

            entity.HasOne(d => d.Decider).WithMany(p => p.DeciderOnAvanceCaisses)
                .HasForeignKey(d => d.DeciderUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AC_Decider");

            entity.HasOne(d => d.NextDecider).WithMany(p => p.NextDeciderOnAvanceCaisses)
                .HasForeignKey(d => d.NextDeciderUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AC_NextDecider");
        });

        modelBuilder.Entity<AvanceVoyage>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__AvanceVo__3214EC0742D682A4");

            entity.ToTable("AvanceVoyage");

            entity.Property(e => e.ActualTotal).HasDefaultValueSql("((0.00))").HasColumnType("decimal(10, 2)");
            entity.Property(e => e.CreateDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Currency)
                .HasMaxLength(5)
                .IsUnicode(false);
            entity.Property(e => e.DeciderComment).HasMaxLength(255);
            entity.Property(e => e.AdvanceOption).HasMaxLength(50);
            entity.Property(e => e.EstimatedTotal).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.LatestStatus).HasDefaultValueSql("((99))");


            entity.HasOne(d => d.LatestStatusNavigation).WithMany(p => p.AvanceVoyages)
                .HasForeignKey(d => d.LatestStatus)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AV_StatusCode");

            entity.HasOne(d => d.OrdreMission).WithMany(p => p.AvanceVoyages)
                .HasForeignKey(d => d.OrdreMissionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AV_OrdreMission");

            entity.HasOne(d => d.User).WithMany(p => p.AvanceVoyages)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AV_Requester");

            entity.HasOne(d => d.Decider).WithMany(p => p.DeciderOnAvanceVoyages)
                .HasForeignKey(d => d.DeciderUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AV_Decider");

            entity.HasOne(d => d.NextDecider).WithMany(p => p.NextDeciderOnAvanceVoyages)
                .HasForeignKey(d => d.NextDeciderUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AV_NextDecider");
        });

        modelBuilder.Entity<Delegation>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Delegati__3214EC076B575C3D");

            entity.ToTable("Delegation");

            entity.Property(e => e.CreateDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.EndDate).HasColumnType("datetime");
            entity.Property(e => e.StartDate).HasColumnType("datetime");

            entity.HasOne(d => d.DeciderUser).WithMany(p => p.DelegationDeciderUsers)
                .HasForeignKey(d => d.DeciderUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Decider_User");

            entity.HasOne(d => d.DelegateUser).WithMany(p => p.DelegationDelegateUsers)
                .HasForeignKey(d => d.DelegateUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Delegate_User");
        });

        modelBuilder.Entity<DepenseCaisse>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__DepenseC__3214EC07A4DE8291");

            entity.ToTable("DepenseCaisse");

            entity.Property(e => e.CreateDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.Property(e => e.Currency)
                .HasMaxLength(10)
                .IsUnicode(false);

            entity.Property(e => e.DeciderComment).HasMaxLength(255);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.LatestStatus).HasDefaultValueSql("((99))");
            entity.Property(e => e.ReceiptsFileName)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Total).HasColumnType("decimal(10, 2)");


            entity.HasOne(d => d.LatestStatusNavigation).WithMany(p => p.DepenseCaisses)
                .HasForeignKey(d => d.LatestStatus)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DP_StatusCode");

            entity.HasOne(d => d.User).WithMany(p => p.DepenseCaisses)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DC_Requester");

            entity.HasOne(d => d.Decider).WithMany(p => p.DeciderOnDepenseCaisses)
                .HasForeignKey(d => d.DeciderUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DC_Decider");

            entity.HasOne(d => d.NextDecider).WithMany(p => p.NextDeciderOnDepenseCaisses)
                .HasForeignKey(d => d.NextDeciderUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DC_NextDecider");
        });

        modelBuilder.Entity<Expense>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Expense__3214EC0797565966");

            entity.ToTable("Expense");

            entity.Property(e => e.ActualFee)
                .HasDefaultValueSql("((0.00))")
                .HasColumnType("decimal(10, 2)");
            entity.Property(e => e.CreateDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Currency)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.EstimatedFee)
                .HasDefaultValueSql("((0.00))")
                .HasColumnType("decimal(10, 2)");
            entity.Property(e => e.ExpenseDate).HasColumnType("datetime");
            entity.Property(e => e.UpdateDate).HasColumnType("datetime");

            entity.HasOne(d => d.AvanceCaisse).WithMany(p => p.Expenses)
                .HasForeignKey(d => d.AvanceCaisseId)
                .HasConstraintName("FK_AC_Expenses");

            entity.HasOne(d => d.AvanceVoyage).WithMany(p => p.Expenses)
                .HasForeignKey(d => d.AvanceVoyageId)
                .HasConstraintName("FK_AV_Expenses");

            entity.HasOne(d => d.DepenseCaisse).WithMany(p => p.Expenses)
                .HasForeignKey(d => d.DepenseCaisseId)
                .HasConstraintName("FK_DC_Expenses");
        });

        modelBuilder.Entity<Liquidation>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Liquidat__3214EC07477D93CC");

            entity.ToTable("Liquidation");

            entity.Property(e => e.ActualTotal).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Result).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Currency)
                  .HasMaxLength(10)
                  .IsUnicode(false);
            entity.Property(e => e.CreateDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.DeciderComment).HasMaxLength(255);
            entity.Property(e => e.LatestStatus).HasDefaultValueSql("((99))");
            entity.Property(e => e.ReceiptsFileName)
                .HasMaxLength(100)
                .IsUnicode(false);


            entity.HasOne(d => d.AvanceCaisse).WithOne(p => p.Liquidation)
                .HasForeignKey<Liquidation>(d => d.AvanceCaisseId)
                .HasConstraintName("FK_AC_Liquidation");

            entity.HasOne(d => d.AvanceVoyage).WithOne(p => p.Liquidation)
                .HasForeignKey<Liquidation>(d => d.AvanceVoyageId)
                .HasConstraintName("FK_AV_Liquidation");

            entity.HasOne(d => d.LatestStatusNavigation).WithMany(p => p.Liquidations)
                .HasForeignKey(d => d.LatestStatus)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_LQ_StatusCode");

            entity.HasOne(d => d.User).WithMany(p => p.Liquidations)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Liquidation_Requester");

            entity.HasOne(d => d.Decider).WithMany(p => p.DeciderOnLiquidations)
               .HasForeignKey(d => d.DeciderUserId)
               .OnDelete(DeleteBehavior.ClientSetNull)
               .HasConstraintName("FK_Liquidation_Decider");

            entity.HasOne(d => d.NextDecider).WithMany(p => p.NextDeciderOnLiquidations)
               .HasForeignKey(d => d.NextDeciderUserId)
               .OnDelete(DeleteBehavior.ClientSetNull)
               .HasConstraintName("fk_Liquidation_NextDecider");
        });

        modelBuilder.Entity<OrdreMission>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__OrdreMis__3214EC07F1443C00");

            entity.ToTable("OrdreMission");

            entity.Property(e => e.CreateDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.DeciderComment).HasMaxLength(255);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.DepartureDate).HasColumnType("datetime");
            entity.Property(e => e.LatestStatus).HasDefaultValueSql("((99))");
            entity.Property(e => e.ReturnDate).HasColumnType("datetime");

            entity.HasOne(d => d.LatestStatusString).WithMany(p => p.OrdreMissions)
                .HasForeignKey(d => d.LatestStatus)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_OM_StatusCode");

            entity.HasOne(d => d.User).WithMany(p => p.OrdreMissions)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_OM_Requester");

            entity.HasOne(d => d.NextDecider).WithMany(p => p.NextDeciderOnOrdreMissions)
                .HasForeignKey(d => d.NextDeciderUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_OM_NextDecider");

            entity.HasOne(d => d.Decider).WithMany(p => p.DeciderOnOrdreMissions)
                .HasForeignKey(d => d.DeciderUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_OM_Decider");
        });

        modelBuilder.Entity<RoleCode>(entity =>
        {
            entity.HasKey(e => e.RoleInt).HasName("PK__RoleCode__D27556AC90DF92AF");

            entity.ToTable("RoleCode");

            entity.Property(e => e.RoleInt).ValueGeneratedNever();
            entity.Property(e => e.RoleString)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<StatusCode>(entity =>
        {
            entity.HasKey(e => e.StatusInt).HasName("PK__StatusCo__09ECF5E73FF578E5");

            entity.ToTable("StatusCode");

            entity.Property(e => e.StatusInt).ValueGeneratedNever();
            entity.Property(e => e.StatusString)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Department>(entity =>
        {
            entity.ToTable("Department");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<StatusHistory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__StatusHi__3214EC071D11D24C");

            entity.ToTable("StatusHistory");

            entity.Property(e => e.CreateDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.DeciderComment)
                .HasMaxLength(255)
                .IsUnicode(false);

            entity.Property(e => e.Status).HasDefaultValueSql("((99))");
            entity.Property(e => e.Total).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.AvanceCaisse).WithMany(p => p.StatusHistories)
                .HasForeignKey(d => d.AvanceCaisseId)
                .HasConstraintName("FK_AC_StatusHistory");

            entity.HasOne(d => d.AvanceVoyage).WithMany(p => p.StatusHistories)
                .HasForeignKey(d => d.AvanceVoyageId)
                .HasConstraintName("FK_AV_StatusHistory");

            entity.HasOne(d => d.Decider).WithMany(p => p.DeciderStatusHistories)
                .HasForeignKey(d => d.DeciderUserId)
                .HasConstraintName("FK_SH_Decider");

            entity.HasOne(d => d.NextDecider).WithMany(p => p.NextDeciderStatusHistories)
                .HasForeignKey(d => d.NextDeciderUserId)
                .HasConstraintName("FK_SH_NextDecider");

            entity.HasOne(d => d.DepenseCaisse).WithMany(p => p.StatusHistories)
                .HasForeignKey(d => d.DepenseCaisseId)
                .HasConstraintName("FK_DC_StatusHistory");

            entity.HasOne(d => d.Liquidation).WithMany(p => p.StatusHistories)
                .HasForeignKey(d => d.LiquidationId)
                .HasConstraintName("FK_LQ_StatusHistory");

            entity.HasOne(d => d.OrdreMission).WithMany(p => p.StatusHistories)
                .HasForeignKey(d => d.OrdreMissionId)
                .HasConstraintName("FK_OM_StatusHistory");

            entity.HasOne(d => d.StatusNavigation).WithMany(p => p.StatusHistories)
                .HasForeignKey(d => d.Status)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SH_StatusCode");
        });

        modelBuilder.Entity<Trip>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Trip__3214EC074485C5EB");

            entity.ToTable("Trip");

            entity.Property(e => e.ActualFee)
                .HasDefaultValueSql("((0.00))")
                .HasColumnType("decimal(10, 2)");
            entity.Property(e => e.CreateDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.DepartureDate).HasColumnType("datetime");
            entity.Property(e => e.ArrivalDate).HasColumnType("datetime2");
            entity.Property(e => e.DeparturePlace)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Destination)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.EstimatedFee)
                .HasDefaultValueSql("((0.00))")
                .HasColumnType("decimal(10, 2)");
            entity.Property(e => e.HighwayFee)
                .HasDefaultValueSql("((0.00))")
                .HasColumnType("decimal(10, 2)");
            entity.Property(e => e.TransportationMethod)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Unit)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.UpdateDate).HasColumnType("datetime");
            entity.Property(e => e.Value)
            .HasDefaultValueSql("((0.00))")
            .HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.AvanceVoyage).WithMany(p => p.Trips)
                .HasForeignKey(d => d.AvanceVoyageId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AVoyage_Trips");
        });

        modelBuilder.Entity<Decider>(entity =>
        {
            entity.HasKey(v => v.Id);

            entity.ToTable("Decider");

            entity.Property(v => v.Level)
                .HasMaxLength(30)
                .IsUnicode(false)
                .IsRequired();

            entity.Property(v => v.Department)
                .HasMaxLength(50)
                .IsRequired()
                .IsUnicode(false);

            entity.Property(v => v.SignatureImageName)
               .HasMaxLength(100)
               .IsUnicode(false);

            entity.HasOne(v => v.User).WithMany(u => u.Deciders)
                .HasForeignKey(v => v.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_User_Decider");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__User__3214EC076D2914FA");

            entity.ToTable("User");

            modelBuilder.Entity<User>()
                .Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .UseIdentityColumn(2, 1); // Id 1 is reserved for system user

            entity.Property(e => e.FirstName)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.LastName)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.Username)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.PreferredLanguage)
                .HasDefaultValueSql("'en'")
                .HasMaxLength(5)
                .IsUnicode(false);

            entity.HasOne(d => d.RoleString).WithMany(p => p.Users)
                .HasForeignKey(d => d.Role)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_User_RoleCode");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
