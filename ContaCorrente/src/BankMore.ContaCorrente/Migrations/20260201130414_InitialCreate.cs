using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BankMore.ContaCorrente.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "contacorrente",
                columns: table => new
                {
                    idcontacorrente = table.Column<Guid>(type: "TEXT", nullable: false),
                    numero = table.Column<int>(type: "INTEGER", nullable: false),
                    nome = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    ativo = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    senha = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    salt = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_contacorrente", x => x.idcontacorrente);
                });

            migrationBuilder.CreateTable(
                name: "idempotencia",
                columns: table => new
                {
                    chave_idempotencia = table.Column<Guid>(type: "TEXT", nullable: false),
                    requisicao = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    resultado = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_idempotencia", x => x.chave_idempotencia);
                });

            migrationBuilder.CreateTable(
                name: "movimento",
                columns: table => new
                {
                    idmovimento = table.Column<Guid>(type: "TEXT", nullable: false),
                    idcontacorrente = table.Column<Guid>(type: "TEXT", nullable: false),
                    datamovimento = table.Column<string>(type: "TEXT", maxLength: 25, nullable: false),
                    tipomovimento = table.Column<string>(type: "TEXT", maxLength: 1, nullable: false),
                    valor = table.Column<decimal>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_movimento", x => x.idmovimento);
                    table.ForeignKey(
                        name: "FK_movimento_contacorrente_idcontacorrente",
                        column: x => x.idcontacorrente,
                        principalTable: "contacorrente",
                        principalColumn: "idcontacorrente",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_contacorrente_numero",
                table: "contacorrente",
                column: "numero",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_movimento_idcontacorrente",
                table: "movimento",
                column: "idcontacorrente");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "idempotencia");

            migrationBuilder.DropTable(
                name: "movimento");

            migrationBuilder.DropTable(
                name: "contacorrente");
        }
    }
}
