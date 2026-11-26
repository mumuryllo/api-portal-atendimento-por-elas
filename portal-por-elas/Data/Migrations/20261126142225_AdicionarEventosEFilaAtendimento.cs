using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace portal_por_elas.Data.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarEventosEFilaAtendimento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "eventos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    data_evento = table.Column<DateOnly>(type: "date", nullable: false),
                    hora_inicio = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    hora_fim = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    ativo = table.Column<bool>(type: "boolean", nullable: false),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_eventos", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "servicos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ativo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_servicos", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "fila_atendimentos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    evento_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome_mulher = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    chamado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    finalizado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fila_atendimentos", x => x.id);
                    table.ForeignKey(
                        name: "FK_fila_atendimentos_eventos_evento_id",
                        column: x => x.evento_id,
                        principalTable: "eventos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "fila_atendimento_servicos",
                columns: table => new
                {
                    fila_atendimento_id = table.Column<Guid>(type: "uuid", nullable: false),
                    servico_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fila_atendimento_servicos", x => new { x.fila_atendimento_id, x.servico_id });
                    table.ForeignKey(
                        name: "FK_fila_atendimento_servicos_fila_atendimentos_fila_atendiment~",
                        column: x => x.fila_atendimento_id,
                        principalTable: "fila_atendimentos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_fila_atendimento_servicos_servicos_servico_id",
                        column: x => x.servico_id,
                        principalTable: "servicos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "servicos",
                columns: new[] { "id", "ativo", "nome" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111101"), true, "Manicure e Pedicure" },
                    { new Guid("11111111-1111-1111-1111-111111111102"), true, "Corte e Escova" },
                    { new Guid("11111111-1111-1111-1111-111111111103"), true, "Coloração e Hidratação" },
                    { new Guid("11111111-1111-1111-1111-111111111104"), true, "Limpeza de Pele" },
                    { new Guid("11111111-1111-1111-1111-111111111105"), true, "Design de Sobrancelhas" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_fila_atendimento_servicos_servico_id",
                table: "fila_atendimento_servicos",
                column: "servico_id");

            migrationBuilder.CreateIndex(
                name: "IX_fila_atendimentos_evento_id",
                table: "fila_atendimentos",
                column: "evento_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "fila_atendimento_servicos");

            migrationBuilder.DropTable(
                name: "fila_atendimentos");

            migrationBuilder.DropTable(
                name: "servicos");

            migrationBuilder.DropTable(
                name: "eventos");
        }
    }
}
