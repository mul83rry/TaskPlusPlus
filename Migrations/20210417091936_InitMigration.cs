using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace TaskPlusPlus.API.Migrations
{
    public partial class InitMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "SignupDate",
                table: "Users",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2021, 4, 17, 13, 49, 35, 832, DateTimeKind.Local).AddTicks(5561),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2021, 4, 17, 13, 48, 3, 390, DateTimeKind.Local).AddTicks(3047));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreationAt",
                table: "Tasks",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2021, 4, 17, 13, 49, 35, 832, DateTimeKind.Local).AddTicks(8536),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2021, 4, 17, 13, 48, 3, 390, DateTimeKind.Local).AddTicks(6104));

            migrationBuilder.AlterColumn<DateTime>(
                name: "GrantedAccessAt",
                table: "SharedBoards",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2021, 4, 17, 13, 49, 35, 832, DateTimeKind.Local).AddTicks(4497),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2021, 4, 17, 13, 48, 3, 390, DateTimeKind.Local).AddTicks(1988));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreationAt",
                table: "Sessions",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2021, 4, 17, 13, 49, 35, 832, DateTimeKind.Local).AddTicks(6421),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2021, 4, 17, 13, 48, 3, 390, DateTimeKind.Local).AddTicks(3931));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreationAt",
                table: "Boards",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2021, 4, 17, 13, 49, 35, 829, DateTimeKind.Local).AddTicks(457),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2021, 4, 17, 13, 48, 3, 386, DateTimeKind.Local).AddTicks(8758));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "SignupDate",
                table: "Users",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2021, 4, 17, 13, 48, 3, 390, DateTimeKind.Local).AddTicks(3047),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2021, 4, 17, 13, 49, 35, 832, DateTimeKind.Local).AddTicks(5561));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreationAt",
                table: "Tasks",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2021, 4, 17, 13, 48, 3, 390, DateTimeKind.Local).AddTicks(6104),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2021, 4, 17, 13, 49, 35, 832, DateTimeKind.Local).AddTicks(8536));

            migrationBuilder.AlterColumn<DateTime>(
                name: "GrantedAccessAt",
                table: "SharedBoards",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2021, 4, 17, 13, 48, 3, 390, DateTimeKind.Local).AddTicks(1988),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2021, 4, 17, 13, 49, 35, 832, DateTimeKind.Local).AddTicks(4497));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreationAt",
                table: "Sessions",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2021, 4, 17, 13, 48, 3, 390, DateTimeKind.Local).AddTicks(3931),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2021, 4, 17, 13, 49, 35, 832, DateTimeKind.Local).AddTicks(6421));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreationAt",
                table: "Boards",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2021, 4, 17, 13, 48, 3, 386, DateTimeKind.Local).AddTicks(8758),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2021, 4, 17, 13, 49, 35, 829, DateTimeKind.Local).AddTicks(457));
        }
    }
}
