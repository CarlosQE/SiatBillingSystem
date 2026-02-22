using Microsoft.EntityFrameworkCore.Migrations;

namespace SiatBillingSystem.Infrastructure.Persistence.Seeds;

/// <summary>
/// Seed de catálogos oficiales del SIN (Servicio de Impuestos Nacionales - Bolivia).
/// Datos vigentes según Manual Técnico SIAT v1.0.
/// IMPORTANTE: Si el SIN actualiza sus catálogos, crear una nueva migración
/// con los datos actualizados en lugar de modificar esta clase.
/// </summary>
public static class CatalogosSinSeed
{
    public static void Sembrar(MigrationBuilder migrationBuilder)
    {
        SembrarTiposDocumentoIdentidad(migrationBuilder);
        SembrarMetodosPago(migrationBuilder);
        SembrarUnidadesMedida(migrationBuilder);
        SembrarLeyendasLey453(migrationBuilder);
        SembrarTiposMoneda(migrationBuilder);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // TIPOS DE DOCUMENTO DE IDENTIDAD — Catálogo 2 del SIN
    // ─────────────────────────────────────────────────────────────────────────
    private static void SembrarTiposDocumentoIdentidad(MigrationBuilder mb)
    {
        mb.InsertData(
            table: "TiposDocumentoIdentidad",
            columns: new[] { "CodigoClasificador", "Descripcion" },
            values: new object[,]
            {
                { 1,  "CI - Cédula de Identidad" },
                { 2,  "CEX - Cédula de Identidad de Extranjero" },
                { 3,  "PAS - Pasaporte" },
                { 4,  "OD - Otro Documento de Identidad" },
                { 5,  "NIT - Número de Identificación Tributaria" },
            });
    }

    // ─────────────────────────────────────────────────────────────────────────
    // MÉTODOS DE PAGO — Catálogo 2 del SIN
    // ─────────────────────────────────────────────────────────────────────────
    private static void SembrarMetodosPago(MigrationBuilder mb)
    {
        mb.InsertData(
            table: "MetodosPago",
            columns: new[] { "CodigoClasificador", "Descripcion" },
            values: new object[,]
            {
                { 1,  "Efectivo" },
                { 2,  "Tarjeta de débito" },
                { 3,  "Tarjeta de crédito" },
                { 4,  "Cheque" },
                { 5,  "Transferencia bancaria" },
                { 6,  "Giro bancario" },
                { 7,  "Depósito bancario" },
                { 8,  "Vale" },
                { 9,  "Otros" },
                { 10, "Tarjeta de débito Master Débito" },
                { 11, "Tarjeta de débito Maestro" },
                { 12, "Bonos" },
                { 13, "Voucher" },
            });
    }

    // ─────────────────────────────────────────────────────────────────────────
    // UNIDADES DE MEDIDA — Catálogo 48 del SIN
    // Solo las más relevantes para el sector servicios.
    // El código 58 "Servicio" es el default para todas las facturas de servicios.
    // ─────────────────────────────────────────────────────────────────────────
    private static void SembrarUnidadesMedida(MigrationBuilder mb)
    {
        mb.InsertData(
            table: "UnidadesMedida",
            columns: new[] { "CodigoClasificador", "Descripcion" },
            values: new object[,]
            {
                { 1,   "Barril" },
                { 2,   "Bobina" },
                { 3,   "Bolsa" },
                { 4,   "Caja" },
                { 5,   "Cartón" },
                { 6,   "Centímetro" },
                { 7,   "Centímetro cúbico" },
                { 8,   "Decímetro cúbico" },
                { 9,   "Docena" },
                { 10,  "Envase" },
                { 11,  "Fardo" },
                { 12,  "Frasco" },
                { 13,  "Galón" },
                { 14,  "Garrafa" },
                { 15,  "Gramo" },
                { 16,  "Gruesa" },
                { 17,  "Hectolitro" },
                { 18,  "Juego" },
                { 19,  "Kilogramo" },
                { 20,  "Kilometro" },
                { 21,  "Kit" },
                { 22,  "Lata" },
                { 23,  "Libra" },
                { 24,  "Litro" },
                { 25,  "Metro" },
                { 26,  "Metro cúbico" },
                { 27,  "Metro cuadrado" },
                { 28,  "Miligramo" },
                { 29,  "Mililitro" },
                { 30,  "Milímetro" },
                { 31,  "Onza" },
                { 32,  "Par" },
                { 33,  "Paquete" },
                { 34,  "Pieza" },
                { 35,  "Pulgada" },
                { 36,  "Resma" },
                { 37,  "Rollo" },
                { 38,  "Set" },
                { 39,  "Sobre" },
                { 40,  "Tonelada" },
                { 41,  "Tubo" },
                { 42,  "Unidad" },
                { 43,  "Vasija" },
                { 44,  "Pieza dental" },
                { 45,  "Evento" },
                { 46,  "Consulta" },
                { 47,  "Sesión" },             // Relevante para fisioterapia
                { 48,  "Hora" },               // Relevante para servicios por hora
                { 49,  "Día" },
                { 50,  "Semana" },
                { 51,  "Mes" },                // Relevante para suscripciones/cuotas
                { 52,  "Año" },
                { 53,  "Tratamiento" },        // Relevante para fisioterapia/salud
                { 54,  "Procedimiento" },      // Relevante para sector salud
                { 55,  "Kilómetro recorrido" },
                { 56,  "Viaje" },
                { 57,  "Transacción" },
                { 58,  "Servicio" },           // DEFAULT para sector servicios generales
                { 59,  "Actividad" },
                { 60,  "Global" },             // Para servicios cobrados como monto global
            });
    }

    // ─────────────────────────────────────────────────────────────────────────
    // LEYENDAS LEY 453 — Derechos del Consumidor
    // Texto obligatorio en facturas según actividad económica.
    // Fuente: Resolución Normativa de Directorio del SIN.
    // ─────────────────────────────────────────────────────────────────────────
    private static void SembrarLeyendasLey453(MigrationBuilder mb)
    {
        mb.InsertData(
            table: "LeyendasFactura",
            columns: new[] { "Id", "CodigoActividad", "DescripcionLeyenda" },
            values: new object[,]
            {
                // Leyenda general — aplica a la mayoría de servicios
                { 1, "TODAS",
                  "\"LEY N° 453: EL PROVEEDOR DE SERVICIOS DEBE ENTREGAR ESTA FACTURA AL CONSUMIDOR.\"" },

                // Servicios de salud y fisioterapia
                { 2, "860000",
                  "\"LEY N° 453: EN SERVICIOS DE SALUD, USTED TIENE DERECHO A RECIBIR INFORMACIÓN CLARA SOBRE EL TRATAMIENTO Y COSTOS.\"" },

                { 3, "869000",
                  "\"LEY N° 453: EN SERVICIOS DE SALUD, USTED TIENE DERECHO A RECIBIR INFORMACIÓN CLARA SOBRE EL TRATAMIENTO Y COSTOS.\"" },

                // Servicios de fisioterapia específicos
                { 4, "869010",
                  "\"LEY N° 453: EN SERVICIOS DE FISIOTERAPIA, USTED TIENE DERECHO A CONOCER EL PLAN DE TRATAMIENTO Y SU COSTO TOTAL.\"" },

                // Servicios de consultoría y profesionales
                { 5, "691000",
                  "\"LEY N° 453: EN SERVICIOS PROFESIONALES, USTED TIENE DERECHO A RECIBIR CONTRATO ESCRITO Y FACTURA POR LOS SERVICIOS PRESTADOS.\"" },

                // Servicios de enseñanza y educación
                { 6, "850000",
                  "\"LEY N° 453: EN SERVICIOS EDUCATIVOS, EL ESTABLECIMIENTO DEBE INFORMAR OPORTUNAMENTE SOBRE ARANCELES Y PENALIDADES.\"" },

                // Servicios de telecomunicaciones
                { 7, "610000",
                  "\"LEY N° 453: EN SERVICIOS DE TELECOMUNICACIONES, USTED TIENE DERECHO A RECIBIR INFORMACIÓN SOBRE TARIFAS Y CONDICIONES DEL SERVICIO.\"" },

                // Servicios jurídicos
                { 8, "691100",
                  "\"LEY N° 453: EN SERVICIOS JURÍDICOS, USTED TIENE DERECHO A CONOCER LOS HONORARIOS ANTES DE CONTRATAR EL SERVICIO.\"" },

                // Servicios de informática y tecnología
                { 9, "620000",
                  "\"LEY N° 453: EN SERVICIOS DE TECNOLOGÍA, USTED TIENE DERECHO A RECIBIR DOCUMENTACIÓN DEL SERVICIO PRESTADO.\"" },

                // Servicios contables y auditoría
                { 10, "692000",
                  "\"LEY N° 453: EN SERVICIOS CONTABLES, USTED TIENE DERECHO A RECIBIR INFORMES DETALLADOS DEL TRABAJO REALIZADO.\"" },

                // Servicios de arquitectura e ingeniería
                { 11, "711000",
                  "\"LEY N° 453: EN SERVICIOS DE ARQUITECTURA E INGENIERÍA, USTED TIENE DERECHO A RECIBIR PLANOS Y MEMORIA DESCRIPTIVA.\"" },

                // Servicios de mantenimiento y reparación
                { 12, "452000",
                  "\"LEY N° 453: EN SERVICIOS DE MANTENIMIENTO, EL PROVEEDOR DEBE INFORMAR SOBRE EL DIAGNÓSTICO Y PRESUPUESTO PREVIO.\"" },

                // Servicios de transporte
                { 13, "492000",
                  "\"LEY N° 453: EN SERVICIOS DE TRANSPORTE, USTED TIENE DERECHO A CONOCER LAS TARIFAS Y CONDICIONES DEL SERVICIO.\"" },
            });
    }

    // ─────────────────────────────────────────────────────────────────────────
    // TIPOS DE MONEDA — Catálogo 100 del SIN
    // ─────────────────────────────────────────────────────────────────────────
    private static void SembrarTiposMoneda(MigrationBuilder mb)
    {
        mb.InsertData(
            table: "TiposMoneda",
            columns: new[] { "CodigoClasificador", "Descripcion" },
            values: new object[,]
            {
                { 1,  "Boliviano (BOB)" },
                { 2,  "Dólar Americano (USD)" },
                { 3,  "Euro (EUR)" },
                { 4,  "UFV - Unidad de Fomento a la Vivienda" },
            });
    }
}
