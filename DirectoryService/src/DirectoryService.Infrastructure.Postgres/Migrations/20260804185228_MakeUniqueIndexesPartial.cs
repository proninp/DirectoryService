using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DirectoryService.Infrastructure.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class MakeUniqueIndexesPartial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_positions_name",
                table: "positions");

            migrationBuilder.DropIndex(
                name: "IX_departments_slug",
                table: "departments");

            migrationBuilder.DropIndex(
                name: "IX_department_positions_department_id_position_id",
                table: "department_positions");

            migrationBuilder.DropIndex(
                name: "IX_department_locations_department_id_location_id",
                table: "department_locations");

            migrationBuilder.CreateIndex(
                name: "IX_positions_name",
                table: "positions",
                column: "name",
                unique: true,
                filter: "is_active = true");

            migrationBuilder.CreateIndex(
                name: "IX_departments_slug",
                table: "departments",
                column: "slug",
                unique: true,
                filter: "is_active = true");

            migrationBuilder.CreateIndex(
                name: "IX_department_positions_department_id_position_id",
                table: "department_positions",
                columns: new[] { "department_id", "position_id" },
                unique: true,
                filter: "is_active = true");

            migrationBuilder.CreateIndex(
                name: "IX_department_locations_department_id_location_id",
                table: "department_locations",
                columns: new[] { "department_id", "location_id" },
                unique: true,
                filter: "is_active = true");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_positions_name",
                table: "positions");

            migrationBuilder.DropIndex(
                name: "IX_departments_slug",
                table: "departments");

            migrationBuilder.DropIndex(
                name: "IX_department_positions_department_id_position_id",
                table: "department_positions");

            migrationBuilder.DropIndex(
                name: "IX_department_locations_department_id_location_id",
                table: "department_locations");

            migrationBuilder.CreateIndex(
                name: "IX_positions_name",
                table: "positions",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_departments_slug",
                table: "departments",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_department_positions_department_id_position_id",
                table: "department_positions",
                columns: new[] { "department_id", "position_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_department_locations_department_id_location_id",
                table: "department_locations",
                columns: new[] { "department_id", "location_id" },
                unique: true);
        }
    }
}
