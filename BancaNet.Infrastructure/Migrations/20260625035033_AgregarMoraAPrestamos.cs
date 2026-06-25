using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BancaNet.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AgregarMoraAPrestamos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DiasGracia",
                table: "Prestamos",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaVencimientoActual",
                table: "Prestamos",
                type: "date",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<decimal>(
                name: "TasaMoraDiaria",
                table: "Prestamos",
                type: "numeric(5,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Mora",
                table: "DetallesPrestamos",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DiasGracia",
                table: "Prestamos");

            migrationBuilder.DropColumn(
                name: "FechaVencimientoActual",
                table: "Prestamos");

            migrationBuilder.DropColumn(
                name: "TasaMoraDiaria",
                table: "Prestamos");

            migrationBuilder.DropColumn(
                name: "Mora",
                table: "DetallesPrestamos");
        }
    }
}
