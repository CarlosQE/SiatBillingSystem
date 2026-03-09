using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SiatBillingSystem.Application.Interfaces;
using SiatBillingSystem.Domain.Entities;
using SiatBillingSystem.Domain.Enums;
using System.Collections.ObjectModel;
using System.IO;
using System.Diagnostics;

namespace SiatBillingSystem.Desktop.ViewModels
{
    public partial class HistorialViewModel : ObservableObject
    {
        private readonly IInvoiceRepository       _invoiceRepo;
        private readonly IConfiguracionRepository _configuracionRepo;
        private readonly IPdfService              _pdfService;
        private readonly IQrCodeService           _qrService;

        [ObservableProperty] private DateTime _fechaDesde = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        [ObservableProperty] private DateTime _fechaHasta = DateTime.Today;
        [ObservableProperty] private string   _filtroNit  = string.Empty;

        [ObservableProperty] private ObservableCollection<FacturaResumen> _facturas = new();
        [ObservableProperty] private FacturaResumen? _facturaSeleccionada;

        [ObservableProperty] private int     _totalFacturas   = 0;
        [ObservableProperty] private decimal _totalMonto      = 0m;
        [ObservableProperty] private int     _pendientesEnvio = 0;

        [ObservableProperty] private bool   _isCargando    = false;
        [ObservableProperty] private string _statusMessage = string.Empty;
        [ObservableProperty] private bool   _isStatusError = false;

        public HistorialViewModel(
            IInvoiceRepository       invoiceRepo,
            IConfiguracionRepository configuracionRepo,
            IPdfService              pdfService,
            IQrCodeService           qrService)
        {
            _invoiceRepo       = invoiceRepo;
            _configuracionRepo = configuracionRepo;
            _pdfService        = pdfService;
            _qrService         = qrService;

            _ = InicializarAsync();
        }

        [RelayCommand]
        public async Task CargarFacturasAsync()
        {
            IsCargando    = true;
            IsStatusError = false;
            StatusMessage = "Buscando facturas...";

            try
            {
                var hasta    = FechaHasta.Date.AddDays(1).AddTicks(-1);
                var facturas = await _invoiceRepo.ObtenerHistorialAsync(desde: FechaDesde.Date, hasta: hasta);

                if (!string.IsNullOrWhiteSpace(FiltroNit))
                {
                    var nitFiltro = FiltroNit.Trim();
                    facturas = facturas
                        .Where(f => f.NumeroDocumento.Contains(nitFiltro, StringComparison.OrdinalIgnoreCase))
                        .ToList();
                }

                Facturas.Clear();
                foreach (var f in facturas.OrderByDescending(f => f.FechaEmision))
                    Facturas.Add(new FacturaResumen(f));

                TotalFacturas   = Facturas.Count;
                TotalMonto      = Facturas.Sum(f => f.MontoTotal);
                PendientesEnvio = Facturas.Count(f => f.EstadoEnvioSin == EstadoEnvioSin.PendienteEnvio);

                StatusMessage = TotalFacturas == 0
                    ? "No se encontraron facturas en el período seleccionado."
                    : $"✓  {TotalFacturas} factura(s)  —  Total: Bs. {TotalMonto:N2}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error al cargar historial: {ex.Message}";
                IsStatusError = true;
            }
            finally
            {
                IsCargando = false;
            }
        }

        [RelayCommand]
        private async Task FiltrarMesActual()
        {
            FechaDesde = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            FechaHasta = DateTime.Today;
            FiltroNit  = string.Empty;
            await CargarFacturasAsync();
        }

        [RelayCommand]
        private async Task FiltrarHoy()
        {
            FechaDesde = DateTime.Today;
            FechaHasta = DateTime.Today;
            FiltroNit  = string.Empty;
            await CargarFacturasAsync();
        }

        [RelayCommand]
        private async Task AbrirPdf(FacturaResumen? resumen)
        {
            if (resumen == null) return;

            IsCargando    = true;
            IsStatusError = false;
            StatusMessage = "Generando PDF...";

            try
            {
                var config = await _configuracionRepo.ObtenerAsync();
                if (config is null || string.IsNullOrWhiteSpace(config.Nit))
                {
                    StatusMessage = "⚠  Configura los datos de la empresa antes de ver el PDF (F10).";
                    IsStatusError = true;
                    return;
                }

                var factura = await _invoiceRepo.ObtenerPorIdAsync(resumen.Id);
                if (factura is null)
                {
                    StatusMessage = "⚠  No se encontró la factura en la base de datos local.";
                    IsStatusError = true;
                    return;
                }

                byte[] qrBytes;
                try
                {
                    var urlQr = _qrService.GenerarUrlVerificacion(config.Nit, factura.Cuf, factura.NumeroFactura);
                    qrBytes   = _qrService.GenerarQrPng(urlQr, pixelesPorModulo: 4);
                }
                catch { qrBytes = Array.Empty<byte>(); }

                var rutaPdf = _pdfService.GuardarPdf(factura, config, qrBytes);
                _pdfService.AbrirPdf(rutaPdf);

                StatusMessage = $"✓  PDF abierto: {Path.GetFileName(rutaPdf)}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error al generar PDF: {ex.Message}";
                IsStatusError = true;
            }
            finally
            {
                IsCargando = false;
            }
        }

        [RelayCommand]
        private void AbrirCarpetaFacturas()
        {
            var carpeta = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "NEXUS", "Facturas");
            Directory.CreateDirectory(carpeta);
            Process.Start(new ProcessStartInfo("explorer.exe", carpeta) { UseShellExecute = true });
        }

        public async Task InicializarAsync()
            => await CargarFacturasAsync();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // MODELO DE PRESENTACIÓN
    // ─────────────────────────────────────────────────────────────────────────

    public class FacturaResumen
    {
        public int            Id              { get; }
        public long           NumeroFactura   { get; }
        public string         FechaEmisionStr { get; }
        public string         NitCliente      { get; }
        public string         NombreCliente   { get; }
        public decimal        MontoTotal      { get; }
        public EstadoEnvioSin EstadoEnvioSin  { get; }

        /// <summary>CUF completo para usar en tooltips o copia al portapapeles.</summary>
        public string Cuf      { get; }

        /// <summary>
        /// Primeros 22 chars del hex + "…" — diseñado para la columna del DataGrid.
        /// Si el CUF es vacío o inválido muestra "—" en lugar de crashear.
        /// </summary>
        public string CufCorto { get; }

        // Badge coloreado — color como string hex para el converter en XAML
        public string EstadoBadge  { get; }
        public string EstadoColorHex { get; }

        public FacturaResumen(ServiceInvoice f)
        {
            Id              = f.Id;
            NumeroFactura   = f.NumeroFactura;
            FechaEmisionStr = f.FechaEmision.ToString("dd/MM/yyyy HH:mm");
            NitCliente      = f.NumeroDocumento;
            NombreCliente   = f.NombreRazonSocial;
            MontoTotal      = f.MontoTotal;
            EstadoEnvioSin  = f.EstadoEnvio;
            Cuf             = f.Cuf ?? string.Empty;

            // Defensivo: si el CUF tiene menos de 22 chars, muestra lo que hay
            CufCorto = Cuf.Length == 0
                ? "—"
                : Cuf.Length > 22
                    ? Cuf[..22] + "…"
                    : Cuf;

            (EstadoBadge, EstadoColorHex) = f.EstadoEnvio switch
            {
                EstadoEnvioSin.EnviandoAlSin => ("▶ Enviando…",   "#3b82f6"),
                EstadoEnvioSin.Aceptada      => ("✅ Aceptada",    "#22c55e"),
                EstadoEnvioSin.Rechazada     => ("❌ Rechazada",   "#ef4444"),
                EstadoEnvioSin.Anulada       => ("🚫 Anulada",     "#6b7280"),
                _                            => ("⏳ Pend. envío", "#f59e0b"),
            };
        }
    }
}