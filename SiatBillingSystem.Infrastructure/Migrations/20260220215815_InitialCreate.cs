using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SiatBillingSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ClientesFrecuentes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CodigoTipoDocumento = table.Column<int>(type: "INTEGER", nullable: false),
                    NumeroDocumento = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Complemento = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                    NombreRazonSocial = table.Column<string>(type: "TEXT", maxLength: 250, nullable: false),
                    Telefono = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    FechaRegistro = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UltimaFactura = table.Column<DateTime>(type: "TEXT", nullable: true),
                    TotalFacturas = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientesFrecuentes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ConfiguracionEmpresa",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nit = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    RazonSocial = table.Column<string>(type: "TEXT", maxLength: 250, nullable: false),
                    CodigoModalidad = table.Column<int>(type: "INTEGER", nullable: false),
                    CodigoSucursal = table.Column<int>(type: "INTEGER", nullable: false),
                    CodigoPuntoVenta = table.Column<int>(type: "INTEGER", nullable: true),
                    ActividadEconomica = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    LeyendaLey453 = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    RutaCertificado = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    PasswordCertificadoCifrado = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Cufd = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    FechaCufd = table.Column<DateTime>(type: "TEXT", nullable: true),
                    VencimientoCufd = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UltimoNumeroFactura = table.Column<long>(type: "INTEGER", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FechaUltimaActualizacion = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfiguracionEmpresa", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Facturas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ClienteFrecuenteId = table.Column<int>(type: "INTEGER", nullable: true),
                    EstadoEnvio = table.Column<int>(type: "INTEGER", nullable: false),
                    CodigoAutorizacion = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    MotivoRechazo = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    FechaRespuestaSin = table.Column<DateTime>(type: "TEXT", nullable: true),
                    XmlFirmado = table.Column<string>(type: "TEXT", nullable: true),
                    IntentosEnvio = table.Column<int>(type: "INTEGER", nullable: false),
                    NitEmisor = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    NumeroFactura = table.Column<long>(type: "INTEGER", nullable: false),
                    Cuf = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Cufd = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    CodigoSucursal = table.Column<int>(type: "INTEGER", nullable: false),
                    CodigoPuntoVenta = table.Column<int>(type: "INTEGER", nullable: true),
                    CodigoModalidad = table.Column<int>(type: "INTEGER", nullable: false),
                    TipoEmision = table.Column<int>(type: "INTEGER", nullable: false),
                    TipoFactura = table.Column<int>(type: "INTEGER", nullable: false),
                    TipoDocumentoSector = table.Column<int>(type: "INTEGER", nullable: false),
                    FechaEmision = table.Column<DateTime>(type: "TEXT", nullable: false),
                    NombreRazonSocial = table.Column<string>(type: "TEXT", maxLength: 250, nullable: false),
                    CodigoTipoDocumentoIdentidad = table.Column<int>(type: "INTEGER", nullable: false),
                    NumeroDocumento = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Complemento = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                    MontoTotal = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    MontoTotalSujetoIva = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    CodigoMetodoPago = table.Column<int>(type: "INTEGER", nullable: false),
                    Leyenda = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Facturas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Facturas_ClientesFrecuentes_ClienteFrecuenteId",
                        column: x => x.ClienteFrecuenteId,
                        principalTable: "ClientesFrecuentes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "DetallesFactura",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ServiceInvoiceId = table.Column<int>(type: "INTEGER", nullable: false),
                    ActividadEconomica = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    CodigoProductoSin = table.Column<int>(type: "INTEGER", nullable: false),
                    CodigoProducto = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Descripcion = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    Cantidad = table.Column<decimal>(type: "TEXT", precision: 18, scale: 4, nullable: false),
                    UnidadMedida = table.Column<int>(type: "INTEGER", nullable: false),
                    PrecioUnitario = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    SubTotal = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetallesFactura", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DetallesFactura_Facturas_ServiceInvoiceId",
                        column: x => x.ServiceInvoiceId,
                        principalTable: "Facturas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClientesFrecuentes_NombreRazonSocial",
                table: "ClientesFrecuentes",
                column: "NombreRazonSocial");

            migrationBuilder.CreateIndex(
                name: "IX_ClientesFrecuentes_NumeroDocumento",
                table: "ClientesFrecuentes",
                column: "NumeroDocumento");

            migrationBuilder.CreateIndex(
                name: "IX_DetallesFactura_ServiceInvoiceId",
                table: "DetallesFactura",
                column: "ServiceInvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_Facturas_ClienteFrecuenteId",
                table: "Facturas",
                column: "ClienteFrecuenteId");

            migrationBuilder.CreateIndex(
                name: "IX_Facturas_Cuf",
                table: "Facturas",
                column: "Cuf",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Facturas_EstadoEnvio",
                table: "Facturas",
                column: "EstadoEnvio");

            migrationBuilder.CreateIndex(
                name: "IX_Facturas_FechaEmision",
                table: "Facturas",
                column: "FechaEmision");

            migrationBuilder.CreateIndex(
                name: "IX_Facturas_NumeroFactura",
                table: "Facturas",
                column: "NumeroFactura");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConfiguracionEmpresa");

            migrationBuilder.DropTable(
                name: "DetallesFactura");

            migrationBuilder.DropTable(
                name: "Facturas");

            migrationBuilder.DropTable(
                name: "ClientesFrecuentes");
        }
    }
}
