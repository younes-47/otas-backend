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

    public virtual DbSet<AvanceCaisse> AvanceCaisses { get; set; }

    public virtual DbSet<AvanceVoyage> AvanceVoyages { get; set; }

    public virtual DbSet<Delegation> Delegations { get; set; }

    public virtual DbSet<DepenseCaisse> DepenseCaisses { get; set; }

    public virtual DbSet<Expense> Expenses { get; set; }

    public virtual DbSet<Liquidation> Liquidations { get; set; }

    public virtual DbSet<OrdreMission> OrdreMissions { get; set; }

    public virtual DbSet<StatusCode> StatusCodes { get; set; }

    public virtual DbSet<StatusHistory> StatusHistories { get; set; }

    public virtual DbSet<Trip> Trips { get; set; }

    public virtual DbSet<User> Users { get; set; }

    //    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    //#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
    //        => optionsBuilder.UseSqlServer("Data Source=LENOVO-LAPTOP\\SQLEXPRESS;Initial Catalog=otas;Integrated Security=True;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ActualRequester>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ActualRe__3214EC07847546DD");

            entity.ToTable("ActualRequester");

            entity.Property(e => e.Department)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.FirstName).HasMaxLength(50);
            entity.Property(e => e.HiringDate).HasColumnType("date");
            entity.Property(e => e.JobTitle)
                .HasMaxLength(120)
                .IsUnicode(false);
            entity.Property(e => e.LastName).HasMaxLength(50);
            entity.Property(e => e.Manager)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.AvanceCaisse).WithMany(p => p.ActualRequesters)
                .HasForeignKey(d => d.AvanceCaisseId)
                .HasConstraintName("AC_ActualRequester");

            entity.HasOne(d => d.DepenseCaisse).WithMany(p => p.ActualRequesters)
                .HasForeignKey(d => d.DepenseCaisseId)
                .HasConstraintName("DC_ActualRequester");

            entity.HasOne(d => d.OrdreMission).WithMany(p => p.ActualRequesters)
                .HasForeignKey(d => d.OrdreMissionId)
                .HasConstraintName("OM_ActualRequester");

            entity.HasOne(d => d.ProxyUser).WithMany(p => p.ActualRequesters)
                .HasForeignKey(d => d.ProxyUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProxyUser_ActualRequeser");
        });

        modelBuilder.Entity<AvanceCaisse>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__AvanceCa__3214EC07AF7BD7A9");

            entity.ToTable("AvanceCaisse");

            entity.Property(e => e.ActualTotal).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.CreateDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Currency)
                .HasMaxLength(5)
                .IsUnicode(false);
            entity.Property(e => e.Description).HasMaxLength(2000);
            entity.Property(e => e.EstimatedTotal).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.User).WithMany(p => p.AvanceCaisses)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("AC_Requester");
        });

        modelBuilder.Entity<AvanceVoyage>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__AvanceVo__3214EC0792551DF8");

            entity.ToTable("AvanceVoyage");

            entity.Property(e => e.ActualTotal).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.CreateDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Currency)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasDefaultValueSql("('')");
            entity.Property(e => e.EstimatedTotal).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.OrdreMission).WithMany(p => p.AvanceVoyages)
                .HasForeignKey(d => d.OrdreMissionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("AV_OrdreMission");

            entity.HasOne(d => d.User).WithMany(p => p.AvanceVoyages)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("AV_Requester");
        });

        modelBuilder.Entity<Delegation>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Delegati__3214EC07F307B061");

            entity.ToTable("Delegation");

            entity.Property(e => e.EndDate).HasColumnType("datetime");
            entity.Property(e => e.StartDate).HasColumnType("datetime");

            entity.HasOne(d => d.DeciderUser).WithMany(p => p.DelegationDeciderUsers)
                .HasForeignKey(d => d.DeciderUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("The_Choosing_Decider");

            entity.HasOne(d => d.DelegateUser).WithMany(p => p.DelegationDelegateUsers)
                .HasForeignKey(d => d.DelegateUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("The_Chosen_Delegate");
        });

        modelBuilder.Entity<DepenseCaisse>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__DepenseC__3214EC078B091408");

            entity.ToTable("DepenseCaisse");

            entity.Property(e => e.CreateDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Currency)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.Description).HasMaxLength(2000);
            entity.Property(e => e.ReceiptsFilePath)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.Total).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.User).WithMany(p => p.DepenseCaisses)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("DC_Requester");
        });

        modelBuilder.Entity<Expense>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Expense__3214EC078E2B09D4");

            entity.ToTable("Expense");

            entity.Property(e => e.ActualFee).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.CreateDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Currency)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.Description).HasMaxLength(2000);
            entity.Property(e => e.EstimatedFee).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.ExpenseDate).HasColumnType("datetime");

            entity.HasOne(d => d.AvanceCaisse).WithMany(p => p.Expenses)
                .HasForeignKey(d => d.AvanceCaisseId)
                .HasConstraintName("AC_Expenses");

            entity.HasOne(d => d.AvanceVoyage).WithMany(p => p.Expenses)
                .HasForeignKey(d => d.AvanceVoyageId)
                .HasConstraintName("AV_Expenses");

            entity.HasOne(d => d.DepenseCaisse).WithMany(p => p.Expenses)
                .HasForeignKey(d => d.DepenseCaisseId)
                .HasConstraintName("DC_Expenses");
        });

        modelBuilder.Entity<Liquidation>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Liquidat__3214EC077943EE19");

            entity.ToTable("Liquidation");

            entity.Property(e => e.ActualTotal).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.CreateDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.LiquidationOption)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ReceiptsFilePath)
                .HasMaxLength(500)
                .IsUnicode(false);

            entity.HasOne(d => d.AvanceCaisse).WithMany(p => p.Liquidations)
                .HasForeignKey(d => d.AvanceCaisseId)
                .HasConstraintName("AC_Liquidation");

            entity.HasOne(d => d.AvanceVoyage).WithMany(p => p.Liquidations)
                .HasForeignKey(d => d.AvanceVoyageId)
                .HasConstraintName("AV_Liquidation");

            entity.HasOne(d => d.User).WithMany(p => p.Liquidations)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Liquidation_Requester");
        });

        modelBuilder.Entity<OrdreMission>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__OrdreMis__3214EC073391896D");

            entity.ToTable("OrdreMission");

            entity.Property(e => e.CreateDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.DepartureDate).HasColumnType("datetime");
            entity.Property(e => e.ReturnDate).HasColumnType("datetime");

            entity.HasOne(d => d.User).WithMany(p => p.OrdreMissions)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("OM_Requester");
        });

        modelBuilder.Entity<StatusCode>(entity =>
        {
            entity.HasKey(e => e.StatusInt).HasName("PK__StatusCo__09ECF5E798AD3CA9");

            entity.ToTable("StatusCode");

            entity.Property(e => e.StatusInt).ValueGeneratedNever();
            entity.Property(e => e.StatusString)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<StatusHistory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__StatusHi__3214EC07448F87E0");

            entity.ToTable("StatusHistory");

            entity.Property(e => e.CreateDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.DeciderComment)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.AvanceCaisse).WithMany(p => p.StatusHistories)
                .HasForeignKey(d => d.AvanceCaisseId)
                .HasConstraintName("AC_StatusHistory");

            entity.HasOne(d => d.AvanceVoyage).WithMany(p => p.StatusHistories)
                .HasForeignKey(d => d.AvanceVoyageId)
                .HasConstraintName("AV_StatusHistory");

            entity.HasOne(d => d.DepenseCaisse).WithMany(p => p.StatusHistories)
                .HasForeignKey(d => d.DepenseCaisseId)
                .HasConstraintName("DC_StatusHistory");

            entity.HasOne(d => d.Liquidation).WithMany(p => p.StatusHistories)
                .HasForeignKey(d => d.LiquidationId)
                .HasConstraintName("LQ_StatusHistory");

            entity.HasOne(d => d.OrdreMission).WithMany(p => p.StatusHistories)
                .HasForeignKey(d => d.OrdreMissionId)
                .HasConstraintName("OM_StatusHistory");

            entity.HasOne(d => d.StatusNavigation).WithMany(p => p.StatusHistories)
                .HasForeignKey(d => d.Status)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("SH_StatusCode_To_String");
        });

        modelBuilder.Entity<Trip>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Trip__3214EC07D58D14C4");

            entity.ToTable("Trip");

            entity.Property(e => e.ActualFee).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.CreateDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.DepartureDate).HasColumnType("datetime");
            entity.Property(e => e.DeparturePlace)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Destination)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.EstimatedFee).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.HighwayFee)
                .HasDefaultValueSql("((0.00))")
                .HasColumnType("decimal(10, 2)");
            entity.Property(e => e.TransportationMethod)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Unit)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Value).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.AvanceVoyage).WithMany(p => p.Trips)
                .HasForeignKey(d => d.AvanceVoyageId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("AV_Trips");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__User__3214EC07294BC6DE");

            entity.ToTable("User");

            entity.Property(e => e.FirstName)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.LastName)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.Username)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
