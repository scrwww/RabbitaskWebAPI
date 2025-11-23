using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RabbitaskWebAPI.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "prioridade",
                columns: table => new
                {
                    cd_prioridade = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    nm_prioridade = table.Column<string>(type: "varchar(250)", maxLength: 250, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.cd_prioridade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "tag",
                columns: table => new
                {
                    cd_tag = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    nm_tag = table.Column<string>(type: "varchar(250)", maxLength: 250, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.cd_tag);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "TagTarefa",
                columns: table => new
                {
                    CdTag = table.Column<int>(type: "int", nullable: false),
                    CdUsuario = table.Column<int>(type: "int", nullable: false),
                    CdTarefa = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TagTarefa", x => new { x.CdTag, x.CdUsuario, x.CdTarefa });
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "tipo_sentimento",
                columns: table => new
                {
                    cd_tipo_sentimento = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    nm_tipo_sentimento = table.Column<string>(type: "varchar(250)", maxLength: 250, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.cd_tipo_sentimento);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "tipo_usuario",
                columns: table => new
                {
                    cd_tipo_usuario = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    nm_tipo_usuario = table.Column<string>(type: "varchar(250)", maxLength: 250, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.cd_tipo_usuario);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "usuario",
                columns: table => new
                {
                    cd_usuario = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    nm_usuario = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    nm_email = table.Column<string>(type: "varchar(254)", maxLength: 254, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    nm_senha = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    cd_telefone = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    cd_tipo_usuario = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.cd_usuario);
                    table.ForeignKey(
                        name: "fk_usuario_tipo_usuario",
                        column: x => x.cd_tipo_usuario,
                        principalTable: "tipo_usuario",
                        principalColumn: "cd_tipo_usuario");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "codigo_conexao",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    codigo = table.Column<string>(type: "varchar(8)", maxLength: 8, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    cd_usuario = table.Column<int>(type: "int", nullable: false),
                    data_criacao = table.Column<DateTime>(type: "datetime", nullable: false),
                    data_expiracao = table.Column<DateTime>(type: "datetime", nullable: false),
                    usado = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                    table.ForeignKey(
                        name: "fk_codigo_conexao_usuario",
                        column: x => x.cd_usuario,
                        principalTable: "usuario",
                        principalColumn: "cd_usuario",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "conexao_usuario",
                columns: table => new
                {
                    cd_usuario = table.Column<int>(type: "int", nullable: false),
                    cd_usuario_agente = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => new { x.cd_usuario, x.cd_usuario_agente });
                    table.ForeignKey(
                        name: "fk_conexao_usuario_agente_usuario",
                        column: x => x.cd_usuario_agente,
                        principalTable: "usuario",
                        principalColumn: "cd_usuario");
                    table.ForeignKey(
                        name: "fk_conexao_usuario_usuario",
                        column: x => x.cd_usuario,
                        principalTable: "usuario",
                        principalColumn: "cd_usuario");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "tarefa",
                columns: table => new
                {
                    cd_tarefa = table.Column<int>(type: "int", nullable: false),
                    cd_usuario = table.Column<int>(type: "int", nullable: false),
                    nm_tarefa = table.Column<string>(type: "varchar(250)", maxLength: 250, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    cd_prioridade = table.Column<int>(type: "int", nullable: true),
                    ds_tarefa = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    dt_prazo = table.Column<DateTime>(type: "datetime", nullable: true),
                    cd_usuario_proprietario = table.Column<int>(type: "int", nullable: true),
                    dt_criacao = table.Column<DateTime>(type: "datetime", nullable: true),
                    dt_conclusao = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => new { x.cd_usuario, x.cd_tarefa });
                    table.ForeignKey(
                        name: "fk_tarefa_prioridade",
                        column: x => x.cd_prioridade,
                        principalTable: "prioridade",
                        principalColumn: "cd_prioridade");
                    table.ForeignKey(
                        name: "fk_tarefa_usuario",
                        column: x => x.cd_usuario,
                        principalTable: "usuario",
                        principalColumn: "cd_usuario");
                    table.ForeignKey(
                        name: "fk_tarefa_usuario_proprietario",
                        column: x => x.cd_usuario_proprietario,
                        principalTable: "usuario",
                        principalColumn: "cd_usuario");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "UsuarioUsuario",
                columns: table => new
                {
                    CdUsuario = table.Column<int>(type: "int", nullable: false),
                    CdUsuarioAgente = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsuarioUsuario", x => new { x.CdUsuario, x.CdUsuarioAgente });
                    table.ForeignKey(
                        name: "FK_UsuarioUsuario_usuario_CdUsuario",
                        column: x => x.CdUsuario,
                        principalTable: "usuario",
                        principalColumn: "cd_usuario",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UsuarioUsuario_usuario_CdUsuarioAgente",
                        column: x => x.CdUsuarioAgente,
                        principalTable: "usuario",
                        principalColumn: "cd_usuario",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "anexo",
                columns: table => new
                {
                    cd_anexo = table.Column<int>(type: "int", nullable: false),
                    cd_usuario = table.Column<int>(type: "int", nullable: false),
                    cd_tarefa = table.Column<int>(type: "int", nullable: false),
                    nm_caminho_anexo = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => new { x.cd_anexo, x.cd_usuario, x.cd_tarefa });
                    table.ForeignKey(
                        name: "fk_anexo_tarefa",
                        columns: x => new { x.cd_usuario, x.cd_tarefa },
                        principalTable: "tarefa",
                        principalColumns: new[] { "cd_usuario", "cd_tarefa" });
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "tarefa_feedback",
                columns: table => new
                {
                    cd_tipo_sentimento = table.Column<int>(type: "int", nullable: false),
                    cd_usuario = table.Column<int>(type: "int", nullable: false),
                    cd_tarefa = table.Column<int>(type: "int", nullable: false),
                    ds_feedback = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => new { x.cd_tipo_sentimento, x.cd_usuario, x.cd_tarefa });
                    table.ForeignKey(
                        name: "fk_tarefa_feedback_tarefa",
                        columns: x => new { x.cd_usuario, x.cd_tarefa },
                        principalTable: "tarefa",
                        principalColumns: new[] { "cd_usuario", "cd_tarefa" });
                    table.ForeignKey(
                        name: "fk_tarefa_feedback_tipo_sentimento",
                        column: x => x.cd_tipo_sentimento,
                        principalTable: "tipo_sentimento",
                        principalColumn: "cd_tipo_sentimento");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "tarefa_tag",
                columns: table => new
                {
                    cd_tag = table.Column<int>(type: "int", nullable: false),
                    cd_usuario = table.Column<int>(type: "int", nullable: false),
                    cd_tarefa = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => new { x.cd_tag, x.cd_usuario, x.cd_tarefa });
                    table.ForeignKey(
                        name: "fk_tarefa_tag_tag",
                        column: x => x.cd_tag,
                        principalTable: "tag",
                        principalColumn: "cd_tag");
                    table.ForeignKey(
                        name: "fk_tarefa_tag_tarefa",
                        columns: x => new { x.cd_usuario, x.cd_tarefa },
                        principalTable: "tarefa",
                        principalColumns: new[] { "cd_usuario", "cd_tarefa" });
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "topico",
                columns: table => new
                {
                    cd_topico = table.Column<int>(type: "int", nullable: false),
                    cd_usuario = table.Column<int>(type: "int", nullable: false),
                    cd_tarefa = table.Column<int>(type: "int", nullable: false),
                    nm_topico = table.Column<string>(type: "varchar(250)", maxLength: 250, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ic_concluido = table.Column<bool>(type: "tinyint(1)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => new { x.cd_topico, x.cd_usuario, x.cd_tarefa });
                    table.ForeignKey(
                        name: "fk_topico_tarefa",
                        columns: x => new { x.cd_usuario, x.cd_tarefa },
                        principalTable: "tarefa",
                        principalColumns: new[] { "cd_usuario", "cd_tarefa" });
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.InsertData(
                table: "prioridade",
                columns: new[] { "cd_prioridade", "nm_prioridade" },
                values: new object[,]
                {
                    { 1, "Super Alta" },
                    { 2, "Alta" },
                    { 3, "Média Alta" },
                    { 4, "Média" },
                    { 5, "Média Baixa" },
                    { 6, "Baixa" },
                    { 7, "Super Baixa" }
                });

            migrationBuilder.InsertData(
                table: "tag",
                columns: new[] { "cd_tag", "nm_tag" },
                values: new object[,]
                {
                    { 1, "Escolar" },
                    { 2, "Escola" },
                    { 3, "Português" },
                    { 4, "Matemática" },
                    { 5, "Inglês" },
                    { 6, "Geografia" },
                    { 7, "Espanhol" },
                    { 8, "Filosofia" },
                    { 9, "Casa" },
                    { 10, "Trabalho" },
                    { 11, "Trabalho Escolar" },
                    { 12, "Lição de Casa" },
                    { 13, "Limpeza da Casa" },
                    { 14, "Extras" },
                    { 15, "Extra" },
                    { 16, "Sociologia" },
                    { 17, "Biologia" },
                    { 18, "Física" }
                });

            migrationBuilder.InsertData(
                table: "tipo_sentimento",
                columns: new[] { "cd_tipo_sentimento", "nm_tipo_sentimento" },
                values: new object[,]
                {
                    { 1, "Satisfação" },
                    { 2, "Confiança" },
                    { 3, "Surpresa" },
                    { 4, "Alegria" },
                    { 5, "Felicidade" },
                    { 6, "Gratidão" },
                    { 7, "Orgulho" },
                    { 8, "Inspiração" },
                    { 9, "Apreciação" },
                    { 10, "Aprovação" },
                    { 11, "Curiosidade" },
                    { 12, "Dúvida" },
                    { 13, "Confusão" },
                    { 14, "Desapontamento" },
                    { 15, "Frustração" },
                    { 16, "Preocupação" },
                    { 17, "Desaprovação" },
                    { 18, "Decepção" },
                    { 19, "Aversão" }
                });

            migrationBuilder.InsertData(
                table: "tipo_usuario",
                columns: new[] { "cd_tipo_usuario", "nm_tipo_usuario" },
                values: new object[,]
                {
                    { 1, "Usuário Comum" },
                    { 2, "Agente" }
                });

            migrationBuilder.CreateIndex(
                name: "fk_anexo_tarefa",
                table: "anexo",
                columns: new[] { "cd_usuario", "cd_tarefa" });

            migrationBuilder.CreateIndex(
                name: "idx_codigo_conexao_codigo",
                table: "codigo_conexao",
                column: "codigo");

            migrationBuilder.CreateIndex(
                name: "idx_codigo_conexao_usuario",
                table: "codigo_conexao",
                column: "cd_usuario");

            migrationBuilder.CreateIndex(
                name: "fk_conexao_usuario_agente_usuario",
                table: "conexao_usuario",
                column: "cd_usuario_agente");

            migrationBuilder.CreateIndex(
                name: "fk_tarefa_prioridade",
                table: "tarefa",
                column: "cd_prioridade");

            migrationBuilder.CreateIndex(
                name: "fk_tarefa_usuario_proprietario",
                table: "tarefa",
                column: "cd_usuario_proprietario");

            migrationBuilder.CreateIndex(
                name: "fk_tarefa_feedback_tarefa",
                table: "tarefa_feedback",
                columns: new[] { "cd_usuario", "cd_tarefa" });

            migrationBuilder.CreateIndex(
                name: "fk_tarefa_tag_tarefa",
                table: "tarefa_tag",
                columns: new[] { "cd_usuario", "cd_tarefa" });

            migrationBuilder.CreateIndex(
                name: "fk_topico_tarefa",
                table: "topico",
                columns: new[] { "cd_usuario", "cd_tarefa" });

            migrationBuilder.CreateIndex(
                name: "fk_usuario_tipo_usuario",
                table: "usuario",
                column: "cd_tipo_usuario");

            migrationBuilder.CreateIndex(
                name: "nm_email",
                table: "usuario",
                column: "nm_email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UsuarioUsuario_CdUsuarioAgente",
                table: "UsuarioUsuario",
                column: "CdUsuarioAgente");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "anexo");

            migrationBuilder.DropTable(
                name: "codigo_conexao");

            migrationBuilder.DropTable(
                name: "conexao_usuario");

            migrationBuilder.DropTable(
                name: "TagTarefa");

            migrationBuilder.DropTable(
                name: "tarefa_feedback");

            migrationBuilder.DropTable(
                name: "tarefa_tag");

            migrationBuilder.DropTable(
                name: "topico");

            migrationBuilder.DropTable(
                name: "UsuarioUsuario");

            migrationBuilder.DropTable(
                name: "tipo_sentimento");

            migrationBuilder.DropTable(
                name: "tag");

            migrationBuilder.DropTable(
                name: "tarefa");

            migrationBuilder.DropTable(
                name: "prioridade");

            migrationBuilder.DropTable(
                name: "usuario");

            migrationBuilder.DropTable(
                name: "tipo_usuario");
        }
    }
}
