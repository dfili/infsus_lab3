using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace tamb.Data.Migrations
{
    /// <inheritdoc />
    public partial class _202505181515_AddedPersonsAndReservations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReservedBy",
                table: "Reservations");

            migrationBuilder.AddColumn<int>(
                name: "ReservedById",
                table: "Reservations",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PersonId",
                table: "Instruments",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Persons",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ImePrezime = table.Column<string>(type: "text", nullable: false),
                    PhoneNumber = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    DatumRodjenja = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Persons", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SheetMusic",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Composer = table.Column<string>(type: "text", nullable: true),
                    Arranger = table.Column<string>(type: "text", nullable: true),
                    Instrumentation = table.Column<string>(type: "text", nullable: true),
                    Genre = table.Column<string>(type: "text", nullable: true),
                    FileName = table.Column<string>(type: "text", nullable: false),
                    DateAdded = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SheetMusic", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_ReservedById",
                table: "Reservations",
                column: "ReservedById");

            migrationBuilder.CreateIndex(
                name: "IX_Instruments_PersonId",
                table: "Instruments",
                column: "PersonId");

            migrationBuilder.AddForeignKey(
                name: "FK_Instruments_Persons_PersonId",
                table: "Instruments",
                column: "PersonId",
                principalTable: "Persons",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_Persons_ReservedById",
                table: "Reservations",
                column: "ReservedById",
                principalTable: "Persons",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Instruments_Persons_PersonId",
                table: "Instruments");

            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_Persons_ReservedById",
                table: "Reservations");

            migrationBuilder.DropTable(
                name: "Persons");

            migrationBuilder.DropTable(
                name: "SheetMusic");

            migrationBuilder.DropIndex(
                name: "IX_Reservations_ReservedById",
                table: "Reservations");

            migrationBuilder.DropIndex(
                name: "IX_Instruments_PersonId",
                table: "Instruments");

            migrationBuilder.DropColumn(
                name: "ReservedById",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "PersonId",
                table: "Instruments");

            migrationBuilder.AddColumn<string>(
                name: "ReservedBy",
                table: "Reservations",
                type: "text",
                nullable: true);
        }
    }
}
