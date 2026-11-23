using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace portal_por_elas.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTemporaryPasswordExpiry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "temporary_password_expiry_time",
                table: "users",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "temporary_password_expiry_time",
                table: "users");
        }
    }
}
