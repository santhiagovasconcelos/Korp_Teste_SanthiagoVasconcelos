using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace backend_notas.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDemoSeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Clientes",
                keyColumn: "Id",
                keyValue: -3);

            migrationBuilder.DeleteData(
                table: "Clientes",
                keyColumn: "Id",
                keyValue: -2);

            migrationBuilder.DeleteData(
                table: "Clientes",
                keyColumn: "Id",
                keyValue: -1);

            migrationBuilder.DeleteData(
                table: "Empresas",
                keyColumn: "Id",
                keyValue: -1);

            migrationBuilder.InsertData(
                table: "Clientes",
                columns: new[] { "Id", "Documento", "Nome" },
                values: new object[,]
                {
                    { 1, "11111111111", "João da Silva" },
                    { 2, "22222222222", "Maria Oliveira" },
                    { 3, "33333333333", "Carlos Almeida" },
                    { 4, "44444444444", "Ana Souza" },
                    { 5, "55555555555", "Rafael Santos" }
                });

            migrationBuilder.InsertData(
                table: "Empresas",
                columns: new[] { "Id", "Cnpj", "RazaoSocial" },
                values: new object[] { 1, "12345678000199", "Empresa Demonstração Ltda." });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Clientes",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Clientes",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Clientes",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Clientes",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Clientes",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Empresas",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.InsertData(
                table: "Clientes",
                columns: new[] { "Id", "Documento", "Nome" },
                values: new object[,]
                {
                    { -3, "33333333333", "Cliente Teste 3" },
                    { -2, "22222222222", "Cliente Teste 2" },
                    { -1, "11111111111", "Cliente Teste 1" }
                });

            migrationBuilder.InsertData(
                table: "Empresas",
                columns: new[] { "Id", "Cnpj", "RazaoSocial" },
                values: new object[] { -1, "12345678000199", "Korp ERP Demo Ltda" });
        }
    }
}
