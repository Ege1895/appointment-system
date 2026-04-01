using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppointmentSystem.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddBusinessProfileFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "JobTitle",
                table: "Employees",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(150)",
                oldMaxLength: 150);

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "Businesses",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Businesses",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "District",
                table: "Businesses",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<List<string>>(
                name: "GalleryUrls",
                table: "Businesses",
                type: "text[]",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsOpen",
                table: "Businesses",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "Neighborhood",
                table: "Businesses",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "WorkingHourEnd",
                table: "Businesses",
                type: "interval",
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "WorkingHourStart",
                table: "Businesses",
                type: "interval",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "City",
                table: "Businesses");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Businesses");

            migrationBuilder.DropColumn(
                name: "District",
                table: "Businesses");

            migrationBuilder.DropColumn(
                name: "GalleryUrls",
                table: "Businesses");

            migrationBuilder.DropColumn(
                name: "IsOpen",
                table: "Businesses");

            migrationBuilder.DropColumn(
                name: "Neighborhood",
                table: "Businesses");

            migrationBuilder.DropColumn(
                name: "WorkingHourEnd",
                table: "Businesses");

            migrationBuilder.DropColumn(
                name: "WorkingHourStart",
                table: "Businesses");

            migrationBuilder.AlterColumn<string>(
                name: "JobTitle",
                table: "Employees",
                type: "character varying(150)",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);
        }
    }
}
