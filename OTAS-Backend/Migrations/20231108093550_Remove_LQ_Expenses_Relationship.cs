using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OTAS.Migrations
{
    /// <inheritdoc />
    public partial class Remove_LQ_Expenses_Relationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "LQ_Expenses",
                table: "Expense");


            migrationBuilder.DropColumn(
                name: "LiquidationId",
                table: "Expense");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LiquidationId",
                table: "Expense",
                type: "int",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "LQ_Expenses",
                table: "Expense",
                column: "LiquidationId",
                principalTable: "Liquidation",
                principalColumn: "Id");
        }
    }
}
