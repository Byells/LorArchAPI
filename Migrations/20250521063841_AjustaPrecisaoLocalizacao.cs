using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LorArchApi.Migrations
{
    /// <inheritdoc />
    public partial class AjustaPrecisaoLocalizacao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "Longitude",
                table: "Localizacoes",
                type: "DECIMAL(12,8)",
                precision: 12,
                scale: 8,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "DECIMAL(18,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "Latitude",
                table: "Localizacoes",
                type: "DECIMAL(12,8)",
                precision: 12,
                scale: 8,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "DECIMAL(18,2)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "Longitude",
                table: "Localizacoes",
                type: "DECIMAL(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "DECIMAL(12,8)",
                oldPrecision: 12,
                oldScale: 8);

            migrationBuilder.AlterColumn<decimal>(
                name: "Latitude",
                table: "Localizacoes",
                type: "DECIMAL(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "DECIMAL(12,8)",
                oldPrecision: 12,
                oldScale: 8);
        }
    }
}
