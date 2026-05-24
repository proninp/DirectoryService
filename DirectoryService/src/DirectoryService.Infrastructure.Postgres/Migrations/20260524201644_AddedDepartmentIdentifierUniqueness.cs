using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DirectoryService.Infrastructure.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class AddedDepartmentIdentifierUniqueness : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_department_parent",
                table: "departments");

            migrationBuilder.CreateIndex(
                name: "IX_departments_identifier",
                table: "departments",
                column: "identifier",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "fk_department_parent",
                table: "departments",
                column: "parent_id",
                principalTable: "departments",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_department_parent",
                table: "departments");

            migrationBuilder.DropIndex(
                name: "IX_departments_identifier",
                table: "departments");

            migrationBuilder.AddForeignKey(
                name: "fk_department_parent",
                table: "departments",
                column: "parent_id",
                principalTable: "departments",
                principalColumn: "id");
        }
    }
}
