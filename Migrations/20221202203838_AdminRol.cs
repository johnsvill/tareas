using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tareas.Migrations
{
    /// <inheritdoc />
    public partial class AdminRol : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF NOT EXISTS(SELECT Id FROM AspNetRoles WHERE Id = '30A3BEE5-3F94-4BB7-9866-FA241865A500')
                BEGIN
	                INSERT INTO [AspNetRoles](Id, [Name], [NormalizedName])
	                VALUES('30A3BEE5-3F94-4BB7-9866-FA241865A500','admin','ADMIN') 
                END");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DELETE FROM [AspNetRoles]
                    WHERE Id = '30A3BEE5-3F94-4BB7-9866-FA241865A500'");
        }
    }
}
