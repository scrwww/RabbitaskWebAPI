using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RabbitaskWebAPI.Migrations
{
    public partial class SeedInitialData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                    { 17, "Desaprovação" }
                });

            migrationBuilder.InsertData(
                table: "tipo_sentimento",
                columns: new[] { "cd_tipo_sentimento", "nm_tipo_sentimento" },
                values: new object[,]
                {
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
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "prioridade",
                keyColumn: "cd_prioridade",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "prioridade",
                keyColumn: "cd_prioridade",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "prioridade",
                keyColumn: "cd_prioridade",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "prioridade",
                keyColumn: "cd_prioridade",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "prioridade",
                keyColumn: "cd_prioridade",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "prioridade",
                keyColumn: "cd_prioridade",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "prioridade",
                keyColumn: "cd_prioridade",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "tag",
                keyColumn: "cd_tag",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "tag",
                keyColumn: "cd_tag",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "tag",
                keyColumn: "cd_tag",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "tag",
                keyColumn: "cd_tag",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "tag",
                keyColumn: "cd_tag",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "tag",
                keyColumn: "cd_tag",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "tag",
                keyColumn: "cd_tag",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "tag",
                keyColumn: "cd_tag",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "tag",
                keyColumn: "cd_tag",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "tag",
                keyColumn: "cd_tag",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "tag",
                keyColumn: "cd_tag",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "tag",
                keyColumn: "cd_tag",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "tag",
                keyColumn: "cd_tag",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "tag",
                keyColumn: "cd_tag",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "tag",
                keyColumn: "cd_tag",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "tag",
                keyColumn: "cd_tag",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "tag",
                keyColumn: "cd_tag",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "tag",
                keyColumn: "cd_tag",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "tipo_sentimento",
                keyColumn: "cd_tipo_sentimento",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "tipo_sentimento",
                keyColumn: "cd_tipo_sentimento",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "tipo_sentimento",
                keyColumn: "cd_tipo_sentimento",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "tipo_sentimento",
                keyColumn: "cd_tipo_sentimento",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "tipo_sentimento",
                keyColumn: "cd_tipo_sentimento",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "tipo_sentimento",
                keyColumn: "cd_tipo_sentimento",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "tipo_sentimento",
                keyColumn: "cd_tipo_sentimento",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "tipo_sentimento",
                keyColumn: "cd_tipo_sentimento",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "tipo_sentimento",
                keyColumn: "cd_tipo_sentimento",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "tipo_sentimento",
                keyColumn: "cd_tipo_sentimento",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "tipo_sentimento",
                keyColumn: "cd_tipo_sentimento",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "tipo_sentimento",
                keyColumn: "cd_tipo_sentimento",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "tipo_sentimento",
                keyColumn: "cd_tipo_sentimento",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "tipo_sentimento",
                keyColumn: "cd_tipo_sentimento",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "tipo_sentimento",
                keyColumn: "cd_tipo_sentimento",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "tipo_sentimento",
                keyColumn: "cd_tipo_sentimento",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "tipo_sentimento",
                keyColumn: "cd_tipo_sentimento",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "tipo_sentimento",
                keyColumn: "cd_tipo_sentimento",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "tipo_sentimento",
                keyColumn: "cd_tipo_sentimento",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "tipo_usuario",
                keyColumn: "cd_tipo_usuario",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "tipo_usuario",
                keyColumn: "cd_tipo_usuario",
                keyValue: 2);
        }
    }
}
