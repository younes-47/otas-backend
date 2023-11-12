﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using OTAS.Data;

#nullable disable

namespace OTAS.Migrations
{
    [DbContext(typeof(OtasContext))]
    [Migration("20231112113214_Remove_Unnecessary_UPDATETIME")]
    partial class Remove_Unnecessary_UPDATETIME
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.13")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("OTAS.Models.ActualRequester", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int?>("AvanceCaisseId")
                        .HasColumnType("int");

                    b.Property<DateTime>("CreateDate")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime")
                        .HasDefaultValueSql("(getdate())");

                    b.Property<string>("Department")
                        .IsRequired()
                        .HasMaxLength(50)
                        .IsUnicode(false)
                        .HasColumnType("varchar(50)");

                    b.Property<int?>("DepenseCaisseId")
                        .HasColumnType("int");

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<DateTime>("HiringDate")
                        .HasColumnType("date");

                    b.Property<string>("JobTitle")
                        .IsRequired()
                        .HasMaxLength(120)
                        .IsUnicode(false)
                        .HasColumnType("varchar(120)");

                    b.Property<string>("LastName")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("Manager")
                        .IsRequired()
                        .HasMaxLength(50)
                        .IsUnicode(false)
                        .HasColumnType("varchar(50)");

                    b.Property<int>("OrderingUserId")
                        .HasColumnType("int");

                    b.Property<int?>("OrdreMissionId")
                        .HasColumnType("int");

                    b.Property<int>("RegistrationNumber")
                        .HasColumnType("int");

                    b.HasKey("Id")
                        .HasName("PK__ActualRe__3214EC075F746395");

                    b.HasIndex("AvanceCaisseId");

                    b.HasIndex("DepenseCaisseId");

                    b.HasIndex("OrderingUserId");

                    b.HasIndex("OrdreMissionId");

                    b.ToTable("ActualRequester", (string)null);
                });

            modelBuilder.Entity("OTAS.Models.AvanceCaisse", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<decimal?>("ActualTotal")
                        .HasColumnType("decimal(10, 2)");

                    b.Property<int?>("ConfirmationNumber")
                        .HasColumnType("int");

                    b.Property<DateTime>("CreateDate")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime")
                        .HasDefaultValueSql("(getdate())");

                    b.Property<string>("Currency")
                        .IsRequired()
                        .HasMaxLength(5)
                        .IsUnicode(false)
                        .HasColumnType("varchar(5)");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasMaxLength(2000)
                        .HasColumnType("nvarchar(2000)");

                    b.Property<decimal>("EstimatedTotal")
                        .HasColumnType("decimal(10, 2)");

                    b.Property<int>("LatestStatus")
                        .HasColumnType("int");

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.HasKey("Id")
                        .HasName("PK__AvanceCa__3214EC07F25631E8");

                    b.HasIndex("UserId");

                    b.ToTable("AvanceCaisse", (string)null);
                });

            modelBuilder.Entity("OTAS.Models.AvanceVoyage", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<decimal?>("ActualTotal")
                        .HasColumnType("decimal(10, 2)");

                    b.Property<int?>("ConfirmationNumber")
                        .HasColumnType("int");

                    b.Property<DateTime>("CreateDate")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime")
                        .HasDefaultValueSql("(getdate())");

                    b.Property<string>("Currency")
                        .IsRequired()
                        .HasMaxLength(5)
                        .IsUnicode(false)
                        .HasColumnType("varchar(5)");

                    b.Property<decimal>("EstimatedTotal")
                        .HasColumnType("decimal(10, 2)");

                    b.Property<int>("LatestStatus")
                        .HasColumnType("int");

                    b.Property<int>("OrdreMissionId")
                        .HasColumnType("int");

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.HasKey("Id")
                        .HasName("PK__AvanceVo__3214EC07711625B2");

                    b.HasIndex("OrdreMissionId");

                    b.HasIndex("UserId");

                    b.ToTable("AvanceVoyage", (string)null);
                });

            modelBuilder.Entity("OTAS.Models.Delegation", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("CreateDate")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime")
                        .HasDefaultValueSql("(getdate())");

                    b.Property<int>("DeciderUserId")
                        .HasColumnType("int");

                    b.Property<int>("DelegateUserId")
                        .HasColumnType("int");

                    b.Property<DateTime>("EndDate")
                        .HasColumnType("datetime");

                    b.Property<int>("IsCancelled")
                        .HasColumnType("int");

                    b.Property<DateTime>("StartDate")
                        .HasColumnType("datetime");

                    b.HasKey("Id")
                        .HasName("PK__Delegati__3214EC071ABE2E66");

                    b.HasIndex("DeciderUserId");

                    b.HasIndex("DelegateUserId");

                    b.ToTable("Delegation", (string)null);
                });

            modelBuilder.Entity("OTAS.Models.DepenseCaisse", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int?>("ConfirmationNumber")
                        .HasColumnType("int");

                    b.Property<DateTime>("CreateDate")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime")
                        .HasDefaultValueSql("(getdate())");

                    b.Property<string>("Currency")
                        .IsRequired()
                        .HasMaxLength(10)
                        .IsUnicode(false)
                        .HasColumnType("varchar(10)");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasMaxLength(2000)
                        .HasColumnType("nvarchar(2000)");

                    b.Property<int>("LatestStatus")
                        .HasColumnType("int");

                    b.Property<int>("OnBehalf")
                        .HasColumnType("int");

                    b.Property<string>("ReceiptsFilePath")
                        .IsRequired()
                        .HasMaxLength(500)
                        .IsUnicode(false)
                        .HasColumnType("varchar(500)");

                    b.Property<decimal>("Total")
                        .HasColumnType("decimal(10, 2)");

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.HasKey("Id")
                        .HasName("PK__DepenseC__3214EC076F493C39");

                    b.HasIndex("UserId");

                    b.ToTable("DepenseCaisse", (string)null);
                });

            modelBuilder.Entity("OTAS.Models.Expense", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<decimal?>("ActualFee")
                        .HasColumnType("decimal(10, 2)");

                    b.Property<int?>("AvanceCaisseId")
                        .HasColumnType("int");

                    b.Property<int?>("AvanceVoyageId")
                        .HasColumnType("int");

                    b.Property<DateTime>("CreateDate")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime")
                        .HasDefaultValueSql("(getdate())");

                    b.Property<string>("Currency")
                        .IsRequired()
                        .HasMaxLength(10)
                        .IsUnicode(false)
                        .HasColumnType("varchar(10)");

                    b.Property<int?>("DepenseCaisseId")
                        .HasColumnType("int");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasMaxLength(2000)
                        .HasColumnType("nvarchar(2000)");

                    b.Property<decimal>("EstimatedFee")
                        .HasColumnType("decimal(10, 2)");

                    b.Property<DateTime>("ExpenseDate")
                        .HasColumnType("datetime");

                    b.Property<DateTime?>("UpdateDate")
                        .HasColumnType("datetime");

                    b.HasKey("Id")
                        .HasName("PK__Expense__3214EC07CB7BD2A4");

                    b.HasIndex("AvanceCaisseId");

                    b.HasIndex("AvanceVoyageId");

                    b.HasIndex("DepenseCaisseId");

                    b.ToTable("Expense", (string)null);
                });

            modelBuilder.Entity("OTAS.Models.Liquidation", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<decimal?>("ActualTotal")
                        .HasColumnType("decimal(10, 2)");

                    b.Property<int?>("AvanceCaisseId")
                        .HasColumnType("int");

                    b.Property<int?>("AvanceVoyageId")
                        .HasColumnType("int");

                    b.Property<DateTime>("CreateDate")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime")
                        .HasDefaultValueSql("(getdate())");

                    b.Property<int>("LatestStatus")
                        .HasColumnType("int");

                    b.Property<string>("LiquidationOption")
                        .HasMaxLength(50)
                        .IsUnicode(false)
                        .HasColumnType("varchar(50)");

                    b.Property<string>("ReceiptsFilePath")
                        .HasMaxLength(500)
                        .IsUnicode(false)
                        .HasColumnType("varchar(500)");

                    b.Property<int?>("Result")
                        .HasColumnType("int");

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.HasKey("Id")
                        .HasName("PK__Liquidat__3214EC07E8A9D63B");

                    b.HasIndex("AvanceCaisseId");

                    b.HasIndex("AvanceVoyageId");

                    b.HasIndex("UserId");

                    b.ToTable("Liquidation", (string)null);
                });

            modelBuilder.Entity("OTAS.Models.OrdreMission", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("CreateDate")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime")
                        .HasDefaultValueSql("(getdate())");

                    b.Property<DateTime>("DepartureDate")
                        .HasColumnType("datetime");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("LatestStatus")
                        .HasColumnType("int");

                    b.Property<int>("OnBehalf")
                        .HasColumnType("int");

                    b.Property<int>("Region")
                        .HasColumnType("int");

                    b.Property<DateTime>("ReturnDate")
                        .HasColumnType("datetime");

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.HasKey("Id")
                        .HasName("PK__OrdreMis__3214EC07E98E377F");

                    b.HasIndex("UserId");

                    b.ToTable("OrdreMission", (string)null);
                });

            modelBuilder.Entity("OTAS.Models.StatusCode", b =>
                {
                    b.Property<int>("StatusInt")
                        .HasColumnType("int");

                    b.Property<string>("StatusString")
                        .HasMaxLength(100)
                        .IsUnicode(false)
                        .HasColumnType("varchar(100)");

                    b.HasKey("StatusInt")
                        .HasName("PK__StatusCo__09ECF5E7BBE84A35");

                    b.ToTable("StatusCode", (string)null);
                });

            modelBuilder.Entity("OTAS.Models.StatusHistory", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int?>("AvanceCaisseId")
                        .HasColumnType("int");

                    b.Property<int?>("AvanceVoyageId")
                        .HasColumnType("int");

                    b.Property<DateTime>("CreateDate")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime")
                        .HasDefaultValueSql("(getdate())");

                    b.Property<string>("DeciderComment")
                        .HasMaxLength(50)
                        .IsUnicode(false)
                        .HasColumnType("varchar(50)");

                    b.Property<string>("DeciderUsername")
                        .HasMaxLength(50)
                        .IsUnicode(false)
                        .HasColumnType("varchar(50)");

                    b.Property<int?>("DepenseCaisseId")
                        .HasColumnType("int");

                    b.Property<int?>("LiquidationId")
                        .HasColumnType("int");

                    b.Property<int?>("OrdreMissionId")
                        .HasColumnType("int");

                    b.Property<int>("Status")
                        .HasColumnType("int");

                    b.HasKey("Id")
                        .HasName("PK__StatusHi__3214EC0734700B76");

                    b.HasIndex("AvanceCaisseId");

                    b.HasIndex("AvanceVoyageId");

                    b.HasIndex("DepenseCaisseId");

                    b.HasIndex("LiquidationId");

                    b.HasIndex("OrdreMissionId");

                    b.HasIndex("Status");

                    b.ToTable("StatusHistory", (string)null);
                });

            modelBuilder.Entity("OTAS.Models.Trip", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<decimal?>("ActualFee")
                        .HasColumnType("decimal(10, 2)");

                    b.Property<int>("AvanceVoyageId")
                        .HasColumnType("int");

                    b.Property<DateTime>("CreateDate")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime")
                        .HasDefaultValueSql("(getdate())");

                    b.Property<DateTime>("DepartureDate")
                        .HasColumnType("datetime");

                    b.Property<string>("DeparturePlace")
                        .IsRequired()
                        .HasMaxLength(50)
                        .IsUnicode(false)
                        .HasColumnType("varchar(50)");

                    b.Property<string>("Destination")
                        .IsRequired()
                        .HasMaxLength(50)
                        .IsUnicode(false)
                        .HasColumnType("varchar(50)");

                    b.Property<decimal>("EstimatedFee")
                        .HasColumnType("decimal(10, 2)");

                    b.Property<decimal?>("HighwayFee")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("decimal(10, 2)")
                        .HasDefaultValueSql("((0.00))");

                    b.Property<string>("TransportationMethod")
                        .IsRequired()
                        .HasMaxLength(50)
                        .IsUnicode(false)
                        .HasColumnType("varchar(50)");

                    b.Property<string>("Unit")
                        .IsRequired()
                        .HasMaxLength(50)
                        .IsUnicode(false)
                        .HasColumnType("varchar(50)");

                    b.Property<DateTime?>("UpdateDate")
                        .HasColumnType("datetime");

                    b.Property<decimal>("Value")
                        .HasColumnType("decimal(10, 2)");

                    b.HasKey("Id")
                        .HasName("PK__Trip__3214EC07F005332E");

                    b.HasIndex("AvanceVoyageId");

                    b.ToTable("Trip", (string)null);
                });

            modelBuilder.Entity("OTAS.Models.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("FirstName")
                        .HasMaxLength(30)
                        .IsUnicode(false)
                        .HasColumnType("varchar(30)");

                    b.Property<string>("LastName")
                        .HasMaxLength(30)
                        .IsUnicode(false)
                        .HasColumnType("varchar(30)");

                    b.Property<int>("Role")
                        .HasColumnType("int");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasMaxLength(50)
                        .IsUnicode(false)
                        .HasColumnType("varchar(50)");

                    b.HasKey("Id")
                        .HasName("PK__User__3214EC0714C2BE1A");

                    b.ToTable("User", (string)null);
                });

            modelBuilder.Entity("OTAS.Models.ActualRequester", b =>
                {
                    b.HasOne("OTAS.Models.AvanceCaisse", "AvanceCaisse")
                        .WithMany("ActualRequesters")
                        .HasForeignKey("AvanceCaisseId")
                        .HasConstraintName("FK_AC_ActualRequester");

                    b.HasOne("OTAS.Models.DepenseCaisse", "DepenseCaisse")
                        .WithMany("ActualRequesters")
                        .HasForeignKey("DepenseCaisseId")
                        .HasConstraintName("FK_DC_ActualRequester");

                    b.HasOne("OTAS.Models.User", "OrderingUser")
                        .WithMany("ActualRequesters")
                        .HasForeignKey("OrderingUserId")
                        .IsRequired()
                        .HasConstraintName("FK_OrderingUser_ActualRequester");

                    b.HasOne("OTAS.Models.OrdreMission", "OrdreMission")
                        .WithMany("ActualRequesters")
                        .HasForeignKey("OrdreMissionId")
                        .HasConstraintName("FK_OM_ActualRequester");

                    b.Navigation("AvanceCaisse");

                    b.Navigation("DepenseCaisse");

                    b.Navigation("OrderingUser");

                    b.Navigation("OrdreMission");
                });

            modelBuilder.Entity("OTAS.Models.AvanceCaisse", b =>
                {
                    b.HasOne("OTAS.Models.User", "User")
                        .WithMany("AvanceCaisses")
                        .HasForeignKey("UserId")
                        .IsRequired()
                        .HasConstraintName("FK_AC_Requester");

                    b.Navigation("User");
                });

            modelBuilder.Entity("OTAS.Models.AvanceVoyage", b =>
                {
                    b.HasOne("OTAS.Models.OrdreMission", "OrdreMission")
                        .WithMany("AvanceVoyages")
                        .HasForeignKey("OrdreMissionId")
                        .IsRequired()
                        .HasConstraintName("FK_AV_OrdreMission");

                    b.HasOne("OTAS.Models.User", "User")
                        .WithMany("AvanceVoyages")
                        .HasForeignKey("UserId")
                        .IsRequired()
                        .HasConstraintName("FK_AV_Requester");

                    b.Navigation("OrdreMission");

                    b.Navigation("User");
                });

            modelBuilder.Entity("OTAS.Models.Delegation", b =>
                {
                    b.HasOne("OTAS.Models.User", "DeciderUser")
                        .WithMany("DelegationDeciderUsers")
                        .HasForeignKey("DeciderUserId")
                        .IsRequired()
                        .HasConstraintName("FK_Decider_User");

                    b.HasOne("OTAS.Models.User", "DelegateUser")
                        .WithMany("DelegationDelegateUsers")
                        .HasForeignKey("DelegateUserId")
                        .IsRequired()
                        .HasConstraintName("FK_Delegate_User");

                    b.Navigation("DeciderUser");

                    b.Navigation("DelegateUser");
                });

            modelBuilder.Entity("OTAS.Models.DepenseCaisse", b =>
                {
                    b.HasOne("OTAS.Models.User", "User")
                        .WithMany("DepenseCaisses")
                        .HasForeignKey("UserId")
                        .IsRequired()
                        .HasConstraintName("FK_DC_Requester");

                    b.Navigation("User");
                });

            modelBuilder.Entity("OTAS.Models.Expense", b =>
                {
                    b.HasOne("OTAS.Models.AvanceCaisse", "AvanceCaisse")
                        .WithMany("Expenses")
                        .HasForeignKey("AvanceCaisseId")
                        .HasConstraintName("FK_AC_Expenses");

                    b.HasOne("OTAS.Models.AvanceVoyage", "AvanceVoyage")
                        .WithMany("Expenses")
                        .HasForeignKey("AvanceVoyageId")
                        .HasConstraintName("FK_AV_Expenses");

                    b.HasOne("OTAS.Models.DepenseCaisse", "DepenseCaisse")
                        .WithMany("Expenses")
                        .HasForeignKey("DepenseCaisseId")
                        .HasConstraintName("FK_DC_Expenses");

                    b.Navigation("AvanceCaisse");

                    b.Navigation("AvanceVoyage");

                    b.Navigation("DepenseCaisse");
                });

            modelBuilder.Entity("OTAS.Models.Liquidation", b =>
                {
                    b.HasOne("OTAS.Models.AvanceCaisse", "AvanceCaisse")
                        .WithMany("Liquidations")
                        .HasForeignKey("AvanceCaisseId")
                        .HasConstraintName("FK_AC_Liquidation");

                    b.HasOne("OTAS.Models.AvanceVoyage", "AvanceVoyage")
                        .WithMany("Liquidations")
                        .HasForeignKey("AvanceVoyageId")
                        .HasConstraintName("FK_AV_Liquidation");

                    b.HasOne("OTAS.Models.User", "User")
                        .WithMany("Liquidations")
                        .HasForeignKey("UserId")
                        .IsRequired()
                        .HasConstraintName("Liquidation_Requester");

                    b.Navigation("AvanceCaisse");

                    b.Navigation("AvanceVoyage");

                    b.Navigation("User");
                });

            modelBuilder.Entity("OTAS.Models.OrdreMission", b =>
                {
                    b.HasOne("OTAS.Models.User", "User")
                        .WithMany("OrdreMissions")
                        .HasForeignKey("UserId")
                        .IsRequired()
                        .HasConstraintName("FK_OM_Requester");

                    b.Navigation("User");
                });

            modelBuilder.Entity("OTAS.Models.StatusHistory", b =>
                {
                    b.HasOne("OTAS.Models.AvanceCaisse", "AvanceCaisse")
                        .WithMany("StatusHistories")
                        .HasForeignKey("AvanceCaisseId")
                        .HasConstraintName("FK_AC_StatusHistory");

                    b.HasOne("OTAS.Models.AvanceVoyage", "AvanceVoyage")
                        .WithMany("StatusHistories")
                        .HasForeignKey("AvanceVoyageId")
                        .HasConstraintName("FK_AV_StatusHistory");

                    b.HasOne("OTAS.Models.DepenseCaisse", "DepenseCaisse")
                        .WithMany("StatusHistories")
                        .HasForeignKey("DepenseCaisseId")
                        .HasConstraintName("FK_DC_StatusHistory");

                    b.HasOne("OTAS.Models.Liquidation", "Liquidation")
                        .WithMany("StatusHistories")
                        .HasForeignKey("LiquidationId")
                        .HasConstraintName("FK_LQ_StatusHistory");

                    b.HasOne("OTAS.Models.OrdreMission", "OrdreMission")
                        .WithMany("StatusHistories")
                        .HasForeignKey("OrdreMissionId")
                        .HasConstraintName("FK_OM_StatusHistory");

                    b.HasOne("OTAS.Models.StatusCode", "StatusNavigation")
                        .WithMany("StatusHistories")
                        .HasForeignKey("Status")
                        .IsRequired()
                        .HasConstraintName("FK_SH_StatusCode");

                    b.Navigation("AvanceCaisse");

                    b.Navigation("AvanceVoyage");

                    b.Navigation("DepenseCaisse");

                    b.Navigation("Liquidation");

                    b.Navigation("OrdreMission");

                    b.Navigation("StatusNavigation");
                });

            modelBuilder.Entity("OTAS.Models.Trip", b =>
                {
                    b.HasOne("OTAS.Models.AvanceVoyage", "AvanceVoyage")
                        .WithMany("Trips")
                        .HasForeignKey("AvanceVoyageId")
                        .IsRequired()
                        .HasConstraintName("FK_AVoyage_Trips");

                    b.Navigation("AvanceVoyage");
                });

            modelBuilder.Entity("OTAS.Models.AvanceCaisse", b =>
                {
                    b.Navigation("ActualRequesters");

                    b.Navigation("Expenses");

                    b.Navigation("Liquidations");

                    b.Navigation("StatusHistories");
                });

            modelBuilder.Entity("OTAS.Models.AvanceVoyage", b =>
                {
                    b.Navigation("Expenses");

                    b.Navigation("Liquidations");

                    b.Navigation("StatusHistories");

                    b.Navigation("Trips");
                });

            modelBuilder.Entity("OTAS.Models.DepenseCaisse", b =>
                {
                    b.Navigation("ActualRequesters");

                    b.Navigation("Expenses");

                    b.Navigation("StatusHistories");
                });

            modelBuilder.Entity("OTAS.Models.Liquidation", b =>
                {
                    b.Navigation("StatusHistories");
                });

            modelBuilder.Entity("OTAS.Models.OrdreMission", b =>
                {
                    b.Navigation("ActualRequesters");

                    b.Navigation("AvanceVoyages");

                    b.Navigation("StatusHistories");
                });

            modelBuilder.Entity("OTAS.Models.StatusCode", b =>
                {
                    b.Navigation("StatusHistories");
                });

            modelBuilder.Entity("OTAS.Models.User", b =>
                {
                    b.Navigation("ActualRequesters");

                    b.Navigation("AvanceCaisses");

                    b.Navigation("AvanceVoyages");

                    b.Navigation("DelegationDeciderUsers");

                    b.Navigation("DelegationDelegateUsers");

                    b.Navigation("DepenseCaisses");

                    b.Navigation("Liquidations");

                    b.Navigation("OrdreMissions");
                });
#pragma warning restore 612, 618
        }
    }
}
