using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LorArchApi.Migrations
{
    /// <inheritdoc />
    public partial class FixLocalizacaoLatLon : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "Longitude",
                table: "Localizacoes",
                type: "DECIMAL(18, 2)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "NVARCHAR2(2000)");

            migrationBuilder.AlterColumn<decimal>(
                name: "Latitude",
                table: "Localizacoes",
                type: "DECIMAL(18, 2)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "NVARCHAR2(2000)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Longitude",
                table: "Localizacoes",
                type: "NVARCHAR2(2000)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "DECIMAL(18, 2)");

            migrationBuilder.AlterColumn<string>(
                name: "Latitude",
                table: "Localizacoes",
                type: "NVARCHAR2(2000)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "DECIMAL(18, 2)");
        }
    }
}
