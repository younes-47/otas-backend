using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OTAS.Migrations
{
    /// <inheritdoc />
    public partial class Increase_JobTitle_VARCHAR_LENGTH : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "JobTitle",
                table: "ActualRequester",
                type: "nvarchar(120)",
                unicode: false,
                maxLength: 120,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(1)",
                oldUnicode: false,
                oldMaxLength: 1);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "JobTitle",
                table: "ActualRequester",
                type: "varchar(1)",
                unicode: false,
                maxLength: 1,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(120)",
                oldUnicode: false,
                oldMaxLength: 120);
        }
    }
}
