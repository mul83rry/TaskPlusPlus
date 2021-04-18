using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace TaskPlusPlus.API.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Boards",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Caption = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreationAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValue: new DateTime(2021, 4, 18, 9, 42, 0, 941, DateTimeKind.Local).AddTicks(7707)),
                    Deleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Boards", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Comments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Text = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Sender = table.Column<int>(type: "int", nullable: false),
                    ReplyTo = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Files",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Files", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Sessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AccessToken = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    IsValid = table.Column<bool>(type: "bit", nullable: false),
                    CreationAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValue: new DateTime(2021, 4, 18, 9, 42, 0, 946, DateTimeKind.Local).AddTicks(1504))
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sessions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SharedBoards",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BoardId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ShareTo = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GrantedAccessAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValue: new DateTime(2021, 4, 18, 9, 42, 0, 945, DateTimeKind.Local).AddTicks(8989))
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SharedBoards", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tasks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Caption = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Star = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    ParentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreationAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValue: new DateTime(2021, 4, 18, 9, 42, 0, 946, DateTimeKind.Local).AddTicks(3786)),
                    Deleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tasks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SignupDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValue: new DateTime(2021, 4, 18, 9, 42, 0, 946, DateTimeKind.Local).AddTicks(547)),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Boards_Id",
                table: "Boards",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Comments_Id",
                table: "Comments",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Files_Id",
                table: "Files",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_AccessToken",
                table: "Sessions",
                column: "AccessToken",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_Id",
                table: "Sessions",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SharedBoards_Id",
                table: "SharedBoards",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_Id",
                table: "Tasks",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Id",
                table: "Users",
                column: "Id",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Boards");

            migrationBuilder.DropTable(
                name: "Comments");

            migrationBuilder.DropTable(
                name: "Files");

            migrationBuilder.DropTable(
                name: "Sessions");

            migrationBuilder.DropTable(
                name: "SharedBoards");

            migrationBuilder.DropTable(
                name: "Tasks");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
