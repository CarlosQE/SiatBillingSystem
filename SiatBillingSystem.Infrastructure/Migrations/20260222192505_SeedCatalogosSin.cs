using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SiatBillingSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedCatalogosSin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LeyendasFactura",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CodigoActividad = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    DescripcionLeyenda = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeyendasFactura", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MetodosPago",
                columns: table => new
                {
                    CodigoClasificador = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Descripcion = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MetodosPago", x => x.CodigoClasificador);
                });

            migrationBuilder.CreateTable(
                name: "TiposDocumentoIdentidad",
                columns: table => new
                {
                    CodigoClasificador = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Descripcion = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TiposDocumentoIdentidad", x => x.CodigoClasificador);
                });

            migrationBuilder.CreateTable(
                name: "TiposMoneda",
                columns: table => new
                {
                    CodigoClasificador = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Descripcion = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TiposMoneda", x => x.CodigoClasificador);
                });

            migrationBuilder.CreateTable(
                name: "UnidadesMedida",
                columns: table => new
                {
                    CodigoClasificador = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Descripcion = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnidadesMedida", x => x.CodigoClasificador);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LeyendasFactura_CodigoActividad",
                table: "LeyendasFactura",
                column: "CodigoActividad");
	
	    SiatBillingSystem.Infrastructure.Persistence.Seeds.CatalogosSinSeed.Sembrar(migrationBuilder);	
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LeyendasFactura");

            migrationBuilder.DropTable(
                name: "MetodosPago");

            migrationBuilder.DropTable(
                name: "TiposDocumentoIdentidad");

            migrationBuilder.DropTable(
                name: "TiposMoneda");

            migrationBuilder.DropTable(
                name: "UnidadesMedida");
        }
    }
}
