using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OTAS.Migrations
{
    /// <inheritdoc />
    public partial class Change_StatusHistory_FK_DeciderUsername_To_DeciderUserId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeciderUsername",
                table: "StatusHistory");

            migrationBuilder.AddColumn<int>(
                name: "DeciderUserId",
                table: "StatusHistory",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeciderUserId",
                table: "StatusHistory");

            migrationBuilder.AddColumn<string>(
                name: "DeciderUsername",
                table: "StatusHistory",
                type: "varchar(50)",
                unicode: false,
                maxLength: 50,
                nullable: true);
        }
    }
}
