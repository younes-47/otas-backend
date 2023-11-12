using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OTAS.Migrations
{
    /// <inheritdoc />
    public partial class Remove_Unnecessary_UPDATETIME : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                table: "DepenseCaisse");

            migrationBuilder.DropColumn(
                name: "UpdateDate",
                table: "AvanceVoyage");

            migrationBuilder.DropColumn(
                name: "UpdateDate",
                table: "AvanceCaisse");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreateDate",
                table: "Trip",
                type: "datetime",
                nullable: false,
                defaultValueSql: "(getdate())",
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true,
                oldDefaultValueSql: "(getdate())");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreateDate",
                table: "StatusHistory",
                type: "datetime",
                nullable: false,
                defaultValueSql: "(getdate())",
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true,
                oldDefaultValueSql: "(getdate())");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreateDate",
                table: "OrdreMission",
                type: "datetime",
                nullable: false,
                defaultValueSql: "(getdate())",
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true,
                oldDefaultValueSql: "(getdate())");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreateDate",
                table: "Liquidation",
                type: "datetime",
                nullable: false,
                defaultValueSql: "(getdate())",
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true,
                oldDefaultValueSql: "(getdate())");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreateDate",
                table: "Expense",
                type: "datetime",
                nullable: false,
                defaultValueSql: "(getdate())",
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true,
                oldDefaultValueSql: "(getdate())");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreateDate",
                table: "DepenseCaisse",
                type: "datetime",
                nullable: false,
                defaultValueSql: "(getdate())",
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true,
                oldDefaultValueSql: "(getdate())");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreateDate",
                table: "Delegation",
                type: "datetime",
                nullable: false,
                defaultValueSql: "(getdate())");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreateDate",
                table: "AvanceVoyage",
                type: "datetime",
                nullable: false,
                defaultValueSql: "(getdate())",
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true,
                oldDefaultValueSql: "(getdate())");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreateDate",
                table: "AvanceCaisse",
                type: "datetime",
                nullable: false,
                defaultValueSql: "(getdate())",
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true,
                oldDefaultValueSql: "(getdate())");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreateDate",
                table: "ActualRequester",
                type: "datetime",
                nullable: false,
                defaultValueSql: "(getdate())");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreateDate",
                table: "Delegation");

            migrationBuilder.DropColumn(
                name: "CreateDate",
                table: "ActualRequester");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreateDate",
                table: "Trip",
                type: "datetime",
                nullable: true,
                defaultValueSql: "(getdate())",
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldDefaultValueSql: "(getdate())");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreateDate",
                table: "StatusHistory",
                type: "datetime",
                nullable: true,
                defaultValueSql: "(getdate())",
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldDefaultValueSql: "(getdate())");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdateDate",
                table: "StatusHistory",
                type: "datetime",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreateDate",
                table: "OrdreMission",
                type: "datetime",
                nullable: true,
                defaultValueSql: "(getdate())",
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldDefaultValueSql: "(getdate())");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdateDate",
                table: "OrdreMission",
                type: "datetime",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreateDate",
                table: "Liquidation",
                type: "datetime",
                nullable: true,
                defaultValueSql: "(getdate())",
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldDefaultValueSql: "(getdate())");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdateDate",
                table: "Liquidation",
                type: "datetime",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreateDate",
                table: "Expense",
                type: "datetime",
                nullable: true,
                defaultValueSql: "(getdate())",
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldDefaultValueSql: "(getdate())");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreateDate",
                table: "DepenseCaisse",
                type: "datetime",
                nullable: true,
                defaultValueSql: "(getdate())",
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldDefaultValueSql: "(getdate())");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdateDate",
                table: "DepenseCaisse",
                type: "datetime",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreateDate",
                table: "AvanceVoyage",
                type: "datetime",
                nullable: true,
                defaultValueSql: "(getdate())",
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldDefaultValueSql: "(getdate())");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdateDate",
                table: "AvanceVoyage",
                type: "datetime",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreateDate",
                table: "AvanceCaisse",
                type: "datetime",
                nullable: true,
                defaultValueSql: "(getdate())",
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldDefaultValueSql: "(getdate())");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdateDate",
                table: "AvanceCaisse",
                type: "datetime",
                nullable: true);
        }
    }
}
