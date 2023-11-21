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
            entity.HasKey(e => e.Id).HasName("PK__ActualRe__3214EC075F746395");

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
            entity.Property(e => e.CreateDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");


            entity.HasOne(d => d.AvanceCaisse).WithOne(p => p.ActualRequester)
                .HasForeignKey<ActualRequester>(d => d.AvanceCaisseId)
                .HasConstraintName("FK_AC_ActualRequester");

            entity.HasOne(d => d.DepenseCaisse).WithOne(p => p.ActualRequester)
                .HasForeignKey<ActualRequester>(d => d.DepenseCaisseId)
                .HasConstraintName("FK_DC_ActualRequester");

            entity.HasOne(d => d.OrderingUser).WithMany(p => p.ActualRequesters)
                .HasForeignKey(d => d.OrderingUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_OrderingUser_ActualRequester");

            entity.HasOne(d => d.OrdreMission).WithOne(p => p.ActualRequester)
                .HasForeignKey<ActualRequester>(d => d.OrdreMissionId)
                .HasConstraintName("FK_OM_ActualRequester");
        });

        modelBuilder.Entity<AvanceCaisse>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__AvanceCa__3214EC07F25631E8");

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

            entity.Property(e => e.LatestStatus)
                   .HasDefaultValue(99);

            entity.HasOne(d => d.User).WithMany(p => p.AvanceCaisses)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AC_Requester");

            entity.HasOne(d => d.StatusNavigation).WithMany(p => p.AvanceCaisses)
                .HasForeignKey(d => d.LatestStatus)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AC_StatusCode");

            entity.HasOne(d => d.User).WithMany(p => p.AvanceCaisses)
                .HasForeignKey(d => d.DeciderUserId)
                .HasConstraintName("FK_AC_Decider");

            entity.Property(e => e.DeciderComment)
                .HasMaxLength(350)
                .IsUnicode(false);

        });

        modelBuilder.Entity<AvanceVoyage>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__AvanceVo__3214EC07711625B2");

            entity.ToTable("AvanceVoyage");

            entity.Property(e => e.ActualTotal).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.CreateDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Currency)
                .HasMaxLength(5)
                .IsUnicode(false);
            entity.Property(e => e.EstimatedTotal).HasColumnType("decimal(10, 2)");

            entity.Property(e => e.LatestStatus)
                  .HasDefaultValue(99);

            entity.HasOne(d => d.OrdreMission).WithMany(p => p.AvanceVoyages)
                .HasForeignKey(d => d.OrdreMissionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AV_OrdreMission");

            entity.HasOne(d => d.User).WithMany(p => p.AvanceVoyages)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AV_Requester");

            entity.HasOne(d => d.StatusNavigation).WithMany(p => p.AvanceVoyages)
                .HasForeignKey(d => d.LatestStatus)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AV_StatusCode");

            entity.HasOne(d => d.User).WithMany(p => p.AvanceVoyages)
                .HasForeignKey(d => d.DeciderUserId)
                .HasConstraintName("FK_AV_Decider");

            entity.Property(e => e.DeciderComment)
                .HasMaxLength(350)
                .IsUnicode(false);

        });

        modelBuilder.Entity<Delegation>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Delegati__3214EC071ABE2E66");

            entity.ToTable("Delegation");

            entity.Property(e => e.EndDate).HasColumnType("datetime");
            entity.Property(e => e.StartDate).HasColumnType("datetime");
            entity.Property(e => e.CreateDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

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
            entity.HasKey(e => e.Id).HasName("PK__DepenseC__3214EC076F493C39");

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

            entity.Property(e => e.LatestStatus)
                .HasDefaultValue(99);

            entity.HasOne(d => d.User).WithMany(p => p.DepenseCaisses)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DC_Requester");

            entity.HasOne(d => d.StatusNavigation).WithMany(p => p.DepenseCaisses)
                .HasForeignKey(d => d.LatestStatus)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DC_StatusCode");

            entity.HasOne(d => d.User).WithMany(p => p.DepenseCaisses)
                .HasForeignKey(d => d.DeciderUserId)
                .HasConstraintName("FK_DC_Decider");

            entity.Property(e => e.DeciderComment)
                .HasMaxLength(350)
                .IsUnicode(false);

        });

        modelBuilder.Entity<Expense>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Expense__3214EC07CB7BD2A4");

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
            entity.HasKey(e => e.Id).HasName("PK__Liquidat__3214EC07E8A9D63B");

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

            entity.Property(e => e.LatestStatus)
                 .HasDefaultValue(99);


            entity.HasOne(d => d.AvanceCaisse).WithMany(p => p.Liquidations)
                .HasForeignKey(d => d.AvanceCaisseId)
                .HasConstraintName("FK_AC_Liquidation");

            entity.HasOne(d => d.AvanceVoyage).WithMany(p => p.Liquidations)
                .HasForeignKey(d => d.AvanceVoyageId)
                .HasConstraintName("FK_AV_Liquidation");

            entity.HasOne(d => d.User).WithMany(p => p.Liquidations)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Liquidation_Requester");

            entity.HasOne(d => d.StatusNavigation).WithMany(p => p.Liquidations)
                .HasForeignKey(d => d.LatestStatus)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_LQ_StatusCode");

            entity.HasOne(d => d.User).WithMany(p => p.Liquidations)
                .HasForeignKey(d => d.DeciderUserId)
                .HasConstraintName("FK_LS_Decider");

            entity.Property(e => e.DeciderComment)
                .HasMaxLength(350)
                .IsUnicode(false);

        });

        modelBuilder.Entity<OrdreMission>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__OrdreMis__3214EC07E98E377F");

            entity.ToTable("OrdreMission");

            entity.Property(e => e.CreateDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.DepartureDate).HasColumnType("datetime");
            entity.Property(e => e.ReturnDate).HasColumnType("datetime");

            entity.Property(e => e.LatestStatus)
                .HasDefaultValue(99);

            entity.HasOne(d => d.User).WithMany(p => p.OrdreMissions)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_OM_Requester");

            entity.HasOne(d => d.StatusNavigation).WithMany(p => p.OrdreMissions)
                .HasForeignKey(d => d.LatestStatus)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_OM_StatusCode");

            entity.HasOne(d => d.User).WithMany(p => p.OrdreMissions)
                .HasForeignKey(d => d.DeciderUserId)
                .HasConstraintName("FK_OM_Decider");

            entity.Property(e => e.DeciderComment)
                .HasMaxLength(350)
                .IsUnicode(false);

        });

        modelBuilder.Entity<StatusCode>(entity =>
        {
            entity.HasKey(e => e.StatusInt).HasName("PK__StatusCo__09ECF5E7BBE84A35");

            entity.ToTable("StatusCode");

            entity.Property(e => e.StatusInt).ValueGeneratedNever();
            entity.Property(e => e.StatusString)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<StatusHistory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__StatusHi__3214EC0734700B76");

            entity.ToTable("StatusHistory");

            entity.Property(e => e.CreateDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.DeciderComment)
                .HasMaxLength(350)
                .IsUnicode(false);

            entity.Property(e => e.Status)
                .HasDefaultValue(99);

            entity.HasOne(d => d.AvanceCaisse).WithMany(p => p.StatusHistories)
                .HasForeignKey(d => d.AvanceCaisseId)
                .HasConstraintName("FK_AC_StatusHistory");

            entity.HasOne(d => d.AvanceVoyage).WithMany(p => p.StatusHistories)
                .HasForeignKey(d => d.AvanceVoyageId)
                .HasConstraintName("FK_AV_StatusHistory");

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

            entity.HasOne(d => d.Decider).WithMany(p => p.StatusHistories)
                .HasForeignKey(d => d.DeciderUserId)
                .HasConstraintName("FK_SH_Decider");

        });

        modelBuilder.Entity<Trip>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Trip__3214EC07F005332E");

            entity.ToTable("Trip");

            entity.Property(e => e.ActualFee).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.CreateDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.UpdateDate).HasColumnType("datetime");
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
                .HasConstraintName("FK_AVoyage_Trips");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__User__3214EC0714C2BE1A");

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
