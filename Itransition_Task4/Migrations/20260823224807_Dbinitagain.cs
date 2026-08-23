using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Itransition_Task4.Migrations
{
    /// <inheritdoc />
    public partial class Dbinitagain : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UserName",
                table: "Users",
                newName: "FullName");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FullName",
                table: "Users",
                newName: "UserName");
        }
    }
}
