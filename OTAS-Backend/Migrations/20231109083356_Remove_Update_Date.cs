using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OTAS.Migrations
{
    /// <inheritdoc />
    public partial class Remove_Update_Date : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BehalfUser",
                table: "ActualRequester");

            migrationBuilder.DropColumn(
                name: "UpdateDate",
                table: "Trip");

            migrationBuilder.DropColumn(
                name: "UpdateDate",
                table: "StatusHistory");

            migrationBuilder.DropColumn(
                name: "UpdateDate",
                table: "OrdreMission");

            migrationBuilder.DropColumn(
                name: "UpdateDate",
                table: "Liquidation");

            migrationBuilder.DropColumn(
                name: "UpdateDate",
                table: "Expense");

            migrationBuilder.DropColumn(
                name: "UpdateDate",
                table: "DepenseCaisse");

            migrationBuilder.DropColumn(
                name: "UpdateDate",
                table: "AvanceVoyage");

            migrationBuilder.DropColumn(
                name: "UpdateDate",
                table: "AvanceCaisse");

            migrationBuilder.RenameColumn(
                name: "BehalfUserId",
                table: "ActualRequester",
                newName: "ProxyUserId");

            migrationBuilder.RenameIndex(
                name: "IX_ActualRequester_BehalfUserId",
                table: "ActualRequester",
                newName: "IX_ActualRequester_ProxyUserId");

            migrationBuilder.AlterColumn<decimal>(
                name: "ActualFee",
                table: "Trip",
                type: "decimal(10,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)");

            migrationBuilder.AlterColumn<int>(
                name: "Region",
                table: "OrdreMission",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AddForeignKey(
                name: "FK_ProxyUser",
                table: "ActualRequester",
                column: "ProxyUserId",
                principalTable: "User",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProxyUser",
                table: "ActualRequester");

            migrationBuilder.RenameColumn(
                name: "ProxyUserId",
                table: "ActualRequester",
                newName: "BehalfUserId");

            migrationBuilder.RenameIndex(
                name: "IX_ActualRequester_ProxyUserId",
                table: "ActualRequester",
                newName: "IX_ActualRequester_BehalfUserId");

            migrationBuilder.AlterColumn<decimal>(
                name: "ActualFee",
                table: "Trip",
                type: "decimal(10,2)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdateDate",
                table: "Trip",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdateDate",
                table: "StatusHistory",
                type: "datetime",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Region",
                table: "OrdreMission",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdateDate",
                table: "OrdreMission",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdateDate",
                table: "Liquidation",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdateDate",
                table: "Expense",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdateDate",
                table: "DepenseCaisse",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdateDate",
                table: "AvanceVoyage",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdateDate",
                table: "AvanceCaisse",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_BehalfUser",
                table: "ActualRequester",
                column: "BehalfUserId",
                principalTable: "User",
                principalColumn: "Id");
        }
    }
}
