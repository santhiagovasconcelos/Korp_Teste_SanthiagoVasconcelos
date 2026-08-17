using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace backend_produtos.Migrations
{
    /// <inheritdoc />
    public partial class AddDemoProductSeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Produtos",
                columns: new[] { "Id", "Ativo", "Codigo", "Descricao", "Preco", "Saldo" },
                values: new object[,]
                {
                    { 1, true, "P001", "Notebook Industrial", 4500.00m, 15 },
                    { 2, true, "P002", "Monitor 24\"", 900.00m, 30 },
                    { 3, true, "P003", "Teclado Mecânico", 250.00m, 50 },
                    { 4, true, "P004", "Mouse Wireless", 120.00m, 80 },
                    { 5, true, "P005", "SSD NVMe 1TB", 480.00m, 40 },
                    { 6, true, "P006", "Placa de Vídeo", 2800.00m, 12 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Produtos",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Produtos",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Produtos",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Produtos",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Produtos",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Produtos",
                keyColumn: "Id",
                keyValue: 6);
        }
    }
}
