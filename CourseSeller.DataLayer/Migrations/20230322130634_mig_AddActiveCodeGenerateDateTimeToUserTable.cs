using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CourseSeller.DataLayer.Migrations
{
    public partial class mig_AddActiveCodeGenerateDateTimeToUserTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ActiveCodeGenerateDateTime",
                table: "Users",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActiveCodeGenerateDateTime",
                table: "Users");
        }
    }
}
