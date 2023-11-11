using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OTAS.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StatusCode",
                columns: table => new
                {
                    StatusInt = table.Column<int>(type: "int", nullable: false),
                    StatusString = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__StatusCo__09ECF5E7BBE84A35", x => x.StatusInt);
                });

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    FirstName = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: true),
                    LastName = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: true),
                    Role = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__User__3214EC0714C2BE1A", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AvanceCaisse",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Currency = table.Column<string>(type: "varchar(5)", unicode: false, maxLength: 5, nullable: false),
                    EstimatedTotal = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    ActualTotal = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    ConfirmationNumber = table.Column<int>(type: "int", nullable: true),
                    LatestStatus = table.Column<int>(type: "int", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    UpdateDate = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__AvanceCa__3214EC07F25631E8", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AC_Requester",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Delegation",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DeciderUserId = table.Column<int>(type: "int", nullable: false),
                    DelegateUserId = table.Column<int>(type: "int", nullable: false),
                    IsCancelled = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Delegati__3214EC071ABE2E66", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Decider_User",
                        column: x => x.DeciderUserId,
                        principalTable: "User",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Delegate_User",
                        column: x => x.DelegateUserId,
                        principalTable: "User",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DepenseCaisse",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    OnBehalf = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Currency = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: false),
                    Total = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    ReceiptsFilePath = table.Column<string>(type: "varchar(500)", unicode: false, maxLength: 500, nullable: false),
                    ConfirmationNumber = table.Column<int>(type: "int", nullable: true),
                    LatestStatus = table.Column<int>(type: "int", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    UpdateDate = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__DepenseC__3214EC076F493C39", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DC_Requester",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "OrdreMission",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    OnBehalf = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Region = table.Column<int>(type: "int", nullable: false),
                    DepartureDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    ReturnDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    LatestStatus = table.Column<int>(type: "int", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    UpdateDate = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__OrdreMis__3214EC07E98E377F", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OM_Requester",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ActualRequester",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderingUserId = table.Column<int>(type: "int", nullable: false),
                    AvanceCaisseId = table.Column<int>(type: "int", nullable: true),
                    DepenseCaisseId = table.Column<int>(type: "int", nullable: true),
                    OrdreMissionId = table.Column<int>(type: "int", nullable: true),
                    FirstName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    RegistrationNumber = table.Column<int>(type: "int", nullable: false),
                    JobTitle = table.Column<string>(type: "varchar(1)", unicode: false, maxLength: 1, nullable: false),
                    HiringDate = table.Column<DateTime>(type: "date", nullable: false),
                    Department = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    Manager = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__ActualRe__3214EC075F746395", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AC_ActualRequester",
                        column: x => x.AvanceCaisseId,
                        principalTable: "AvanceCaisse",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DC_ActualRequester",
                        column: x => x.DepenseCaisseId,
                        principalTable: "DepenseCaisse",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_OM_ActualRequester",
                        column: x => x.OrdreMissionId,
                        principalTable: "OrdreMission",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_OrderingUser_ActualRequester",
                        column: x => x.OrderingUserId,
                        principalTable: "User",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AvanceVoyage",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    OrdreMissionId = table.Column<int>(type: "int", nullable: false),
                    EstimatedTotal = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    ActualTotal = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    Currency = table.Column<string>(type: "varchar(5)", unicode: false, maxLength: 5, nullable: false),
                    ConfirmationNumber = table.Column<int>(type: "int", nullable: true),
                    LatestStatus = table.Column<int>(type: "int", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    UpdateDate = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__AvanceVo__3214EC07711625B2", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AV_OrdreMission",
                        column: x => x.OrdreMissionId,
                        principalTable: "OrdreMission",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AV_Requester",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Expense",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AvanceVoyageId = table.Column<int>(type: "int", nullable: true),
                    AvanceCaisseId = table.Column<int>(type: "int", nullable: true),
                    DepenseCaisseId = table.Column<int>(type: "int", nullable: true),
                    Currency = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: false),
                    EstimatedFee = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    ActualFee = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    ExpenseDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    UpdateDate = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Expense__3214EC07CB7BD2A4", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AC_Expenses",
                        column: x => x.AvanceCaisseId,
                        principalTable: "AvanceCaisse",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AV_Expenses",
                        column: x => x.AvanceVoyageId,
                        principalTable: "AvanceVoyage",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DC_Expenses",
                        column: x => x.DepenseCaisseId,
                        principalTable: "DepenseCaisse",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Liquidation",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    AvanceVoyageId = table.Column<int>(type: "int", nullable: true),
                    AvanceCaisseId = table.Column<int>(type: "int", nullable: true),
                    ActualTotal = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    ReceiptsFilePath = table.Column<string>(type: "varchar(500)", unicode: false, maxLength: 500, nullable: true),
                    LiquidationOption = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    Result = table.Column<int>(type: "int", nullable: true),
                    LatestStatus = table.Column<int>(type: "int", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    UpdateDate = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Liquidat__3214EC07E8A9D63B", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AC_Liquidation",
                        column: x => x.AvanceCaisseId,
                        principalTable: "AvanceCaisse",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AV_Liquidation",
                        column: x => x.AvanceVoyageId,
                        principalTable: "AvanceVoyage",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "Liquidation_Requester",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Trip",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AvanceVoyageId = table.Column<int>(type: "int", nullable: false),
                    DeparturePlace = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    Destination = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    DepartureDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    TransportationMethod = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    Unit = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    Value = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    HighwayFee = table.Column<decimal>(type: "decimal(10,2)", nullable: true, defaultValueSql: "((0.00))"),
                    EstimatedFee = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    ActualFee = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    CreateDate = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    UpdateDate = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Trip__3214EC07F005332E", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AVoyage_Trips",
                        column: x => x.AvanceVoyageId,
                        principalTable: "AvanceVoyage",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "StatusHistory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AvanceVoyageId = table.Column<int>(type: "int", nullable: true),
                    AvanceCaisseId = table.Column<int>(type: "int", nullable: true),
                    DepenseCaisseId = table.Column<int>(type: "int", nullable: true),
                    OrdreMissionId = table.Column<int>(type: "int", nullable: true),
                    LiquidationId = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    DeciderUsername = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    DeciderComment = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    CreateDate = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    UpdateDate = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__StatusHi__3214EC0734700B76", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AC_StatusHistory",
                        column: x => x.AvanceCaisseId,
                        principalTable: "AvanceCaisse",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AV_StatusHistory",
                        column: x => x.AvanceVoyageId,
                        principalTable: "AvanceVoyage",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DC_StatusHistory",
                        column: x => x.DepenseCaisseId,
                        principalTable: "DepenseCaisse",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LQ_StatusHistory",
                        column: x => x.LiquidationId,
                        principalTable: "Liquidation",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_OM_StatusHistory",
                        column: x => x.OrdreMissionId,
                        principalTable: "OrdreMission",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SH_StatusCode",
                        column: x => x.Status,
                        principalTable: "StatusCode",
                        principalColumn: "StatusInt");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ActualRequester_AvanceCaisseId",
                table: "ActualRequester",
                column: "AvanceCaisseId");

            migrationBuilder.CreateIndex(
                name: "IX_ActualRequester_DepenseCaisseId",
                table: "ActualRequester",
                column: "DepenseCaisseId");

            migrationBuilder.CreateIndex(
                name: "IX_ActualRequester_OrderingUserId",
                table: "ActualRequester",
                column: "OrderingUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ActualRequester_OrdreMissionId",
                table: "ActualRequester",
                column: "OrdreMissionId");

            migrationBuilder.CreateIndex(
                name: "IX_AvanceCaisse_UserId",
                table: "AvanceCaisse",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AvanceVoyage_OrdreMissionId",
                table: "AvanceVoyage",
                column: "OrdreMissionId");

            migrationBuilder.CreateIndex(
                name: "IX_AvanceVoyage_UserId",
                table: "AvanceVoyage",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Delegation_DeciderUserId",
                table: "Delegation",
                column: "DeciderUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Delegation_DelegateUserId",
                table: "Delegation",
                column: "DelegateUserId");

            migrationBuilder.CreateIndex(
                name: "IX_DepenseCaisse_UserId",
                table: "DepenseCaisse",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Expense_AvanceCaisseId",
                table: "Expense",
                column: "AvanceCaisseId");

            migrationBuilder.CreateIndex(
                name: "IX_Expense_AvanceVoyageId",
                table: "Expense",
                column: "AvanceVoyageId");

            migrationBuilder.CreateIndex(
                name: "IX_Expense_DepenseCaisseId",
                table: "Expense",
                column: "DepenseCaisseId");

            migrationBuilder.CreateIndex(
                name: "IX_Liquidation_AvanceCaisseId",
                table: "Liquidation",
                column: "AvanceCaisseId");

            migrationBuilder.CreateIndex(
                name: "IX_Liquidation_AvanceVoyageId",
                table: "Liquidation",
                column: "AvanceVoyageId");

            migrationBuilder.CreateIndex(
                name: "IX_Liquidation_UserId",
                table: "Liquidation",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_OrdreMission_UserId",
                table: "OrdreMission",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_StatusHistory_AvanceCaisseId",
                table: "StatusHistory",
                column: "AvanceCaisseId");

            migrationBuilder.CreateIndex(
                name: "IX_StatusHistory_AvanceVoyageId",
                table: "StatusHistory",
                column: "AvanceVoyageId");

            migrationBuilder.CreateIndex(
                name: "IX_StatusHistory_DepenseCaisseId",
                table: "StatusHistory",
                column: "DepenseCaisseId");

            migrationBuilder.CreateIndex(
                name: "IX_StatusHistory_LiquidationId",
                table: "StatusHistory",
                column: "LiquidationId");

            migrationBuilder.CreateIndex(
                name: "IX_StatusHistory_OrdreMissionId",
                table: "StatusHistory",
                column: "OrdreMissionId");

            migrationBuilder.CreateIndex(
                name: "IX_StatusHistory_Status",
                table: "StatusHistory",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Trip_AvanceVoyageId",
                table: "Trip",
                column: "AvanceVoyageId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActualRequester");

            migrationBuilder.DropTable(
                name: "Delegation");

            migrationBuilder.DropTable(
                name: "Expense");

            migrationBuilder.DropTable(
                name: "StatusHistory");

            migrationBuilder.DropTable(
                name: "Trip");

            migrationBuilder.DropTable(
                name: "DepenseCaisse");

            migrationBuilder.DropTable(
                name: "Liquidation");

            migrationBuilder.DropTable(
                name: "StatusCode");

            migrationBuilder.DropTable(
                name: "AvanceCaisse");

            migrationBuilder.DropTable(
                name: "AvanceVoyage");

            migrationBuilder.DropTable(
                name: "OrdreMission");

            migrationBuilder.DropTable(
                name: "User");
        }
    }
}
