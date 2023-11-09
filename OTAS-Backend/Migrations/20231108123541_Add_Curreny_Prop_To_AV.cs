using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OTAS.Migrations
{
    /// <inheritdoc />
    public partial class Add_Curreny_Prop_To_AV : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "AvanceVoyage",
                type: "varchar(10)",
                unicode: false,
                maxLength: 10,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Currency",
                table: "AvanceVoyage");
        }
    }
}
