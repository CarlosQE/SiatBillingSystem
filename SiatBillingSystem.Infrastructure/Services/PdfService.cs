using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SiatBillingSystem.Application.Interfaces;
using SiatBillingSystem.Domain.Entities;
using System.Diagnostics;

namespace SiatBillingSystem.Infrastructure.Services;

/// <summary>
/// Genera la Representación Gráfica oficial de la factura SIAT.
/// Usa QuestPDF Community Edition (gratuito para proyectos de código abierto).
/// Layout cumple con el formato exigido por el SIN Bolivia.
/// </summary>
public class PdfService : IPdfService
{
    private static bool _licenciaConfigurada = false;

    public PdfService()
    {
        if (!_licenciaConfigurada)
        {
            QuestPDF.Settings.License = LicenseType.Community;
            _licenciaConfigurada = true;
        }
    }

    public byte[] GenerarPdf(ServiceInvoice factura, ConfiguracionEmpresa empresa, byte[] qrImageBytes)
    {
        return Document.Create(doc => ComponerDocumento(doc, factura, empresa, qrImageBytes))
                       .GeneratePdf();
    }

    public string GuardarPdf(ServiceInvoice factura, ConfiguracionEmpresa empresa, byte[] qrImageBytes)
    {
        var carpeta = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "NEXUS", "Facturas",
            factura.FechaEmision.ToString("yyyy-MM"));

        Directory.CreateDirectory(carpeta);

        var nombreArchivo = $"Factura_{factura.NumeroFactura:D6}_{factura.FechaEmision:yyyyMMdd_HHmmss}.pdf";
        var rutaCompleta = Path.Combine(carpeta, nombreArchivo);

        var bytes = GenerarPdf(factura, empresa, qrImageBytes);
        File.WriteAllBytes(rutaCompleta, bytes);
        return rutaCompleta;
    }

    public void AbrirPdf(string rutaPdf)
    {
        if (!File.Exists(rutaPdf)) return;
        Process.Start(new ProcessStartInfo(rutaPdf) { UseShellExecute = true });
    }

    // ─────────────────────────────────────────────────────────────────────────
    // COMPOSICIÓN DEL DOCUMENTO
    // ─────────────────────────────────────────────────────────────────────────

    private static void ComponerDocumento(
        IDocumentContainer doc,
        ServiceInvoice factura,
        ConfiguracionEmpresa empresa,
        byte[] qrImageBytes)
    {
        doc.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(1.5f, Unit.Centimetre);
            page.DefaultTextStyle(s => s.FontFamily("Arial").FontSize(9).FontColor("#1F2937"));

            page.Content().Column(col =>
            {
                // ══════════════════════════════════════════════════════
                // CABECERA: Empresa + Badge Factura
                // ══════════════════════════════════════════════════════
                col.Item().Row(row =>
                {
                    // Datos empresa (izquierda)
                    row.RelativeItem().Column(c =>
                    {
                        c.Item().Text(empresa.RazonSocial ?? "SIN CONFIGURAR")
                            .Bold().FontSize(15).FontColor("#0F1117");

                        c.Item().PaddingTop(3).Text(t =>
                        {
                            t.Span("NIT: ").Bold();
                            t.Span(empresa.Nit ?? "—");
                        });

                        if (!string.IsNullOrEmpty(empresa.ActividadEconomica))
                        {
                            c.Item().Text(t =>
                            {
                                t.Span("Actividad CAEB: ").Bold();
                                t.Span(empresa.ActividadEconomica);
                            });
                        }

                        c.Item().PaddingTop(2).Text(t =>
                        {
                            t.Span("Sucursal: ").Bold();
                            t.Span(empresa.CodigoSucursal == 0 ? "Casa Matriz" : $"Sucursal {empresa.CodigoSucursal}");
                        });
                    });

                    // Badge factura (derecha)
                    row.ConstantItem(130).Background("#0F1117").Padding(10).Column(c =>
                    {
                        c.Item().AlignCenter().Text("FACTURA").Bold().FontSize(18).FontColor("#6ED60E");
                        c.Item().AlignCenter().PaddingTop(2)
                            .Text($"N° {factura.NumeroFactura:D6}").Bold().FontSize(13).FontColor(Colors.White);
                        c.Item().PaddingTop(6).LineHorizontal(0.5f).LineColor("#2A3040");
                        c.Item().PaddingTop(4).AlignCenter()
                            .Text(factura.FechaEmision.ToString("dd/MM/yyyy")).FontSize(10).FontColor("#9BA3B8");
                        c.Item().AlignCenter()
                            .Text(factura.FechaEmision.ToString("HH:mm")).FontSize(9).FontColor("#9BA3B8");
                    });
                });

                col.Item().PaddingTop(6).LineHorizontal(1.5f).LineColor("#6ED60E");

                // ══════════════════════════════════════════════════════
                // DATOS DEL CLIENTE
                // ══════════════════════════════════════════════════════
                col.Item().PaddingTop(8).Border(1).BorderColor("#E5E7EB").Padding(8).Column(c =>
                {
                    c.Item().Text("DATOS DEL CLIENTE")
                        .Bold().FontSize(7).FontColor("#6C7F88").LetterSpacing(0.05f);

                    c.Item().PaddingTop(5).Row(r =>
                    {
                        r.RelativeItem().Column(inner =>
                        {
                            inner.Item().Text(t =>
                            {
                                t.Span("Nombre / Razón Social:  ").Bold();
                                t.Span(factura.NombreRazonSocial);
                            });
                        });

                        r.ConstantItem(220).Column(inner =>
                        {
                            inner.Item().Text(t =>
                            {
                                var tipoDoc = factura.CodigoTipoDocumentoIdentidad == 5 ? "NIT" : "CI";
                                t.Span($"{tipoDoc}:  ").Bold();
                                t.Span(factura.NumeroDocumento);
                                if (!string.IsNullOrEmpty(factura.Complemento))
                                    t.Span($" - {factura.Complemento}");
                            });
                        });
                    });
                });

                // ══════════════════════════════════════════════════════
                // INFORMACIÓN DE EMISIÓN
                // ══════════════════════════════════════════════════════
                col.Item().PaddingTop(6).Row(row =>
                {
                    row.RelativeItem().Text(t =>
                    {
                        t.Span("Modalidad: ").Bold();
                        t.Span(factura.CodigoModalidad == 1 ? "Electrónica en Línea" : "Computarizada en Línea");
                    });
                    row.RelativeItem().Text(t =>
                    {
                        t.Span("Tipo Emisión: ").Bold();
                        t.Span(factura.TipoEmision == 1 ? "En Línea" : "Fuera de Línea (Contingencia)");
                    });
                    row.RelativeItem().Text(t =>
                    {
                        t.Span("Método de Pago: ").Bold();
                        t.Span(ObtenerDescripcionMetodoPago(factura.CodigoMetodoPago));
                    });
                });

                // ══════════════════════════════════════════════════════
                // TABLA DE DETALLE
                // ══════════════════════════════════════════════════════
                col.Item().PaddingTop(10).Table(table =>
                {
                    table.ColumnsDefinition(cols =>
                    {
                        cols.RelativeColumn(5);     // Descripción
                        cols.RelativeColumn(1.2f);  // Cantidad
                        cols.RelativeColumn(1.2f);  // U.M.
                        cols.RelativeColumn(1.8f);  // P. Unitario
                        cols.RelativeColumn(1.8f);  // Subtotal
                    });

                    // Header
                    table.Header(h =>
                    {
                        IContainer Celda(IContainer c) =>
                            c.Background("#1C2128").PaddingVertical(6).PaddingHorizontal(8);

                        h.Cell().Element(Celda).Text("Descripción del Servicio").Bold().FontColor(Colors.White);
                        h.Cell().Element(Celda).AlignCenter().Text("Cant.").Bold().FontColor(Colors.White);
                        h.Cell().Element(Celda).AlignCenter().Text("U.M.").Bold().FontColor(Colors.White);
                        h.Cell().Element(Celda).AlignRight().Text("P. Unit. (Bs.)").Bold().FontColor(Colors.White);
                        h.Cell().Element(Celda).AlignRight().Text("Subtotal (Bs.)").Bold().FontColor(Colors.White);
                    });

                    // Filas de detalle
                    for (int i = 0; i < factura.Details.Count; i++)
                    {
                        var det = factura.Details[i];
                        var bgColor = i % 2 == 0 ? "#FFFFFF" : "#F9FAFB";

                        IContainer FilaCelda(IContainer c) =>
                            c.Background(bgColor).BorderBottom(0.5f).BorderColor("#E5E7EB")
                             .PaddingVertical(5).PaddingHorizontal(8);

                        table.Cell().Element(FilaCelda).Text(det.Descripcion);
                        table.Cell().Element(FilaCelda).AlignCenter().Text(det.Cantidad.ToString("N2"));
                        table.Cell().Element(FilaCelda).AlignCenter().Text(ObtenerDescripcionUnidadMedida(det.UnidadMedida));
                        table.Cell().Element(FilaCelda).AlignRight().Text(det.PrecioUnitario.ToString("N2"));
                        table.Cell().Element(FilaCelda).AlignRight().Text(det.SubTotal.ToString("N2"));
                    }
                });

                // ══════════════════════════════════════════════════════
                // TOTALES
                // ══════════════════════════════════════════════════════
                col.Item().PaddingTop(4).AlignRight().Width(250).Column(totales =>
                {
                    var subtotal = factura.Details.Sum(d => d.SubTotal);
                    var iva = Math.Round(factura.MontoTotal * 13m / 113m, 2); // IVA incluido en precio (método factura Bolivia)

                    FilaTotalNormal(totales, "Importe Total:", $"Bs. {subtotal:N2}");
                    FilaTotalNormal(totales, "IVA incluido (13%):", $"Bs. {iva:N2}");
                    FilaTotalDestacado(totales, "MONTO TOTAL:", $"Bs. {factura.MontoTotal:N2}");
                });

                col.Item().PaddingTop(12).LineHorizontal(0.5f).LineColor("#E5E7EB");

                // ══════════════════════════════════════════════════════
                // CUF + QR + LEYENDA
                // ══════════════════════════════════════════════════════
                col.Item().PaddingTop(8).Row(row =>
                {
                    // QR Code
                    row.ConstantItem(90).Column(qrCol =>
                    {
                        if (qrImageBytes != null && qrImageBytes.Length > 0)
                        {
                            qrCol.Item().Width(85).Height(85).Image(qrImageBytes).FitArea();
                        }
                        qrCol.Item().AlignCenter().PaddingTop(2)
                            .Text("Verifique en SIN").FontSize(6.5f).FontColor("#6C7F88");
                    });

                    // CUF + Leyenda
                    row.RelativeItem().PaddingLeft(8).Column(c =>
                    {
                        // CUF
                        c.Item().Background("#1C2128").Padding(6).Column(cufBox =>
                        {
                            cufBox.Item().Text("CÓDIGO ÚNICO DE FACTURA (CUF)")
                                .Bold().FontSize(7).FontColor("#6C7F88");
                            cufBox.Item().PaddingTop(3)
                                .Text(factura.Cuf)
                                .FontSize(7.5f).FontColor("#E8EAF0")
                                .FontFamily("Courier New");
                        });

                        // Leyenda Ley 453
                        c.Item().PaddingTop(6).Column(ley =>
                        {
                            ley.Item().Text("LEYENDA — LEY N° 453").Bold().FontSize(7).FontColor("#6C7F88");
                            ley.Item().PaddingTop(2).Text(factura.Leyenda).FontSize(7.5f).Italic();
                        });

                        // Nota legal
                        c.Item().PaddingTop(6).Background("#F9FAFB").Padding(5).Text(
                            "Este documento es la Representación Gráfica de una Factura Digital emitida " +
                            "bajo los estándares del SIAT (SIN - Bolivia). Tiene plena validez fiscal. " +
                            "Para verificar su autenticidad, escanee el código QR o ingrese el CUF en " +
                            "el portal oficial del SIN: siat.impuestos.gob.bo")
                            .FontSize(6.5f).FontColor("#6C7F88");
                    });
                });

                // ══════════════════════════════════════════════════════
                // PIE DE DOCUMENTO
                // ══════════════════════════════════════════════════════
                col.Item().PaddingTop(10).LineHorizontal(0.5f).LineColor("#E5E7EB");
                col.Item().PaddingTop(4).Row(r =>
                {
                    r.RelativeItem().Text(t =>
                    {
                        t.Span("Factura N° ").FontSize(7).FontColor("#9BA3B8");
                        t.Span(factura.NumeroFactura.ToString("D6")).Bold().FontSize(7).FontColor("#6ED60E");
                        t.Span($"  |  {factura.FechaEmision:dd/MM/yyyy HH:mm}").FontSize(7).FontColor("#9BA3B8");
                    });
                    r.ConstantItem(150).AlignRight().Text("Sistema NEXUS · INSEIN · Bolivia")
                        .FontSize(6.5f).FontColor("#9BA3B8");
                });
            });
        });
    }

    // ─────────────────────────────────────────────────────────────────────────
    // HELPERS DE DISEÑO
    // ─────────────────────────────────────────────────────────────────────────

    private static void FilaTotalNormal(ColumnDescriptor col, string label, string valor)
    {
        col.Item().PaddingTop(2).Row(r =>
        {
            r.RelativeItem().AlignRight().Text(label).FontColor("#6C7F88");
            r.ConstantItem(110).AlignRight().Padding(2).Text(valor);
        });
    }

    private static void FilaTotalDestacado(ColumnDescriptor col, string label, string valor)
    {
        col.Item().PaddingTop(4).Background("#0F1117").Padding(6).Row(r =>
        {
            r.RelativeItem().AlignRight().Text(label).Bold().FontSize(11).FontColor(Colors.White);
            r.ConstantItem(120).AlignRight().Text(valor).Bold().FontSize(11).FontColor("#6ED60E");
        });
    }

    // ─────────────────────────────────────────────────────────────────────────
    // CATÁLOGOS
    // ─────────────────────────────────────────────────────────────────────────

    private static string ObtenerDescripcionMetodoPago(int codigo) => codigo switch
    {
        1 => "Efectivo",
        2 => "Tarjeta de Débito",
        3 => "Tarjeta de Crédito",
        4 => "Cheque",
        5 => "Transferencia Bancaria",
        6 => "Giro Bancario",
        7 => "Depósito Bancario",
        8 => "Vale",
        _ => "Otro"
    };

    private static string ObtenerDescripcionUnidadMedida(int codigo) => codigo switch
    {
        42 => "UND",
        46 => "CONS.",
        47 => "SESIÓN",
        48 => "HORA",
        49 => "DÍA",
        50 => "SEMANA",
        51 => "MES",
        53 => "TRAT.",
        54 => "PROC.",
        58 => "SERVICIO",
        60 => "GLOBAL",
        _ => $"({codigo})"
    };
}
