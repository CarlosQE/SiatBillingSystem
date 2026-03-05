using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using SiatBillingSystem.Application.Interfaces;
using SiatBillingSystem.Domain.Entities;
using SiatBillingSystem.Domain.Constants;
using SiatBillingSystem.Domain.Enums;
using SiatBillingSystem.Infrastructure.Security;

namespace SiatBillingSystem.Desktop.ViewModels
{
    public partial class PosGridViewModel : ObservableObject
    {
        // ── Dependencias ──────────────────────────────────────────────────────
        private readonly IInvoiceService          _invoiceService;
        private readonly IInvoiceRepository       _invoiceRepository;
        private readonly IClienteRepository       _clienteRepository;
        private readonly IConfiguracionRepository _configuracionRepo;
        private readonly IQrCodeService           _qrService;
        private readonly IPdfService              _pdfService;

        // ── Cliente ───────────────────────────────────────────────────────────
        [ObservableProperty] private string _nitCliente    = string.Empty;
        [ObservableProperty] private string _nombreCliente = string.Empty;
        [ObservableProperty] private string _nitError      = string.Empty;
        [ObservableProperty] private string _nombreError   = string.Empty;

        // ── Sugerencias autocompletado ────────────────────────────────────────
        [ObservableProperty] private ObservableCollection<ClienteFrecuente> _sugerenciasCliente = new();
        [ObservableProperty] private bool _mostrarSugerencias = false;

        // ── Ítem en edición ───────────────────────────────────────────────────
        [ObservableProperty] private string _descripcion      = string.Empty;
        [ObservableProperty] private string _cantidad         = "1";
        [ObservableProperty] private string _precioUnitario   = string.Empty;
        [ObservableProperty] private string _descuento        = "0";
        [ObservableProperty] private string _descripcionError = string.Empty;
        [ObservableProperty] private string _cantidadError    = string.Empty;
        [ObservableProperty] private string _precioError      = string.Empty;

        // ── Totales ───────────────────────────────────────────────────────────
        // SubTotal  = suma de ítems sin descuento (base neta antes de IVA)
        // MontoIva  = 13% sobre la base neta
        // MontoTotal = base neta + IVA
        [ObservableProperty] private decimal _subTotal       = 0m;
        [ObservableProperty] private decimal _totalDescuento = 0m;
        [ObservableProperty] private decimal _montoIva       = 0m;
        [ObservableProperty] private decimal _montoTotal     = 0m;

        // ── Estado UI ─────────────────────────────────────────────────────────
        [ObservableProperty] private string _statusMessage = string.Empty;
        [ObservableProperty] private bool   _isStatusError = false;
        [ObservableProperty] private bool   _isProcessing  = false;

        // ── Catálogos ─────────────────────────────────────────────────────────
        public ObservableCollection<MetodoPagoItem> MetodosPago { get; } = new()
        {
            new MetodoPagoItem { Codigo = "1", Descripcion = "Efectivo" },
            new MetodoPagoItem { Codigo = "2", Descripcion = "Tarjeta Débito" },
            new MetodoPagoItem { Codigo = "3", Descripcion = "Tarjeta Crédito" },
            new MetodoPagoItem { Codigo = "5", Descripcion = "Transferencia Bancaria" },
            new MetodoPagoItem { Codigo = "7", Descripcion = "Depósito Bancario" },
        };

        private MetodoPagoItem? _metodoPagoSeleccionado;
        public MetodoPagoItem? MetodoPagoSeleccionado
        {
            get => _metodoPagoSeleccionado;
            set => SetProperty(ref _metodoPagoSeleccionado, value);
        }

        // ── Helpers de parseo ─────────────────────────────────────────────────
        private static readonly System.Globalization.NumberStyles _numStyle =
            System.Globalization.NumberStyles.Number;
        private static readonly System.Globalization.CultureInfo _invCulture =
            System.Globalization.CultureInfo.InvariantCulture;

        private static decimal ParseDecimal(string input) =>
            decimal.TryParse(input?.Replace(',', '.'), _numStyle, _invCulture, out var v) ? v : 0m;

        // ── Grilla ────────────────────────────────────────────────────────────
        public ObservableCollection<InvoiceDetailRow> Items { get; } = new();

        private InvoiceDetailRow? _selectedItem;
        public InvoiceDetailRow? SelectedItem
        {
            get => _selectedItem;
            set => SetProperty(ref _selectedItem, value);
        }

        // ── Constructor ───────────────────────────────────────────────────────
        public PosGridViewModel(
            IInvoiceService          invoiceService,
            IInvoiceRepository       invoiceRepository,
            IClienteRepository       clienteRepository,
            IConfiguracionRepository configuracionRepo,
            IQrCodeService           qrService,
            IPdfService              pdfService)
        {
            _invoiceService    = invoiceService;
            _invoiceRepository = invoiceRepository;
            _clienteRepository = clienteRepository;
            _configuracionRepo = configuracionRepo;
            _qrService         = qrService;
            _pdfService        = pdfService;

            MetodoPagoSeleccionado = MetodosPago[0];
            Items.CollectionChanged += (_, _) => RecalcularTotales();
        }

        // ── Comandos ──────────────────────────────────────────────────────────

        [RelayCommand]
        private void AgregarItem()
        {
            if (!ValidarItem()) return;

            var cant   = ParseDecimal(Cantidad);
            var precio = ParseDecimal(PrecioUnitario);
            var desc   = ParseDecimal(Descuento);

            Items.Add(new InvoiceDetailRow
            {
                Descripcion    = Descripcion.Trim(),
                Cantidad       = cant,
                PrecioUnitario = precio,
                Descuento      = desc,
                SubTotal       = Math.Max(cant * precio - desc, 0m)
            });

            LimpiarFormItem();
            SetStatus($"✔ Ítem #{Items.Count} agregado. Total: Bs. {MontoTotal:N2}", false);
        }

        [RelayCommand]
        private void EliminarItem()
        {
            if (SelectedItem is null)
            {
                SetStatus("Selecciona un ítem para eliminar.", true);
                return;
            }
            Items.Remove(SelectedItem);
            SelectedItem = null;
            SetStatus("Ítem eliminado.", false);
        }

        [RelayCommand]
        private void NuevaFactura()
        {
            if (Items.Count > 0)
            {
                var result = MessageBox.Show(
                    "¿Limpiar la factura actual?\nSe perderán los ítems cargados.",
                    "Nexus — Nueva Factura",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result != MessageBoxResult.Yes) return;
            }

            LimpiarTodo();
            SetStatus("✔ Lista para nueva factura.", false);
        }

        /// <summary>
        /// F5 — Emitir Factura. Flujo completo offline-first:
        /// 1. Validar → 2. Obtener config → 3. Verificar CUFD → 4. Construir entidad
        /// 5. Calcular CUF → 6. Firmar XML (si hay cert) → 7. Guardar SQLite
        /// 8. Generar QR → 9. Generar PDF → 10. Abrir PDF → 11. Limpiar grilla
        /// </summary>
        [RelayCommand]
        private async Task EmitirFactura()
        {
            if (!ValidarCabecera()) return;
            if (Items.Count == 0)
            {
                SetStatus("⚠  Agrega al menos un ítem antes de emitir.", true);
                return;
            }

            IsProcessing = true;
            SetStatus("Procesando factura...", false);

            try
            {
                // Paso 1: Configuración de empresa
                var config = await _configuracionRepo.ObtenerAsync();
                if (config is null || string.IsNullOrWhiteSpace(config.Nit))
                {
                    SetStatus("⚠  Configura los datos de la empresa antes de emitir (F10).", true);
                    return;
                }

                // Paso 2: Verificar CUFD vigente
                if (config.CufdVencido)
                {
                    SetStatus("⚠  El CUFD está vencido. Renuévalo en Configuración (F10).", true);
                    return;
                }

                // Paso 3: Siguiente número de factura (atómico)
                var numeroFactura = await _configuracionRepo.ObtenerSiguienteNumeroFacturaAsync();

                // Paso 4: Tipo de emisión (online si hay cert, contingencia si no)
                var tieneCertificado = !string.IsNullOrWhiteSpace(config.RutaCertificado)
                                    && File.Exists(config.RutaCertificado);
                var tipoEmision = tieneCertificado
                    ? SiatConstants.TipoEmisionOnline
                    : SiatConstants.TipoEmisionOffline;

                var cufd = string.IsNullOrWhiteSpace(config.Cufd) ? "PENDIENTE-CUFD" : config.Cufd;

                // Paso 5: Construir entidad — MontoTotal incluye IVA, MontoTotalSujetoIva es la base
                var factura = new ServiceInvoice
                {
                    NitEmisor                    = config.Nit,
                    NumeroFactura                = numeroFactura,
                    Cufd                         = cufd,
                    CodigoSucursal               = config.CodigoSucursal,
                    CodigoPuntoVenta             = config.CodigoPuntoVenta,
                    CodigoModalidad              = config.CodigoModalidad > 0
                                                    ? config.CodigoModalidad
                                                    : SiatConstants.ModalidadComputarizadaEnLinea,
                    TipoEmision                  = tipoEmision,
                    TipoFactura                  = SiatConstants.TipoFacturaConDerechoCreditoFiscal,
                    TipoDocumentoSector          = SiatConstants.SectorCompraVenta,
                    FechaEmision                 = DateTime.Now,
                    NombreRazonSocial            = NombreCliente.Trim(),
                    NumeroDocumento              = NitCliente.Trim(),
                    CodigoTipoDocumentoIdentidad = NitCliente.Trim().Length > 8
                                                    ? SiatConstants.TipoDocumentoNIT
                                                    : SiatConstants.TipoDocumentoCedulaIdentidad,
                    CodigoMetodoPago             = int.Parse(MetodoPagoSeleccionado!.Codigo),
                    MontoTotal                   = MontoTotal,          // Base + IVA
                    MontoTotalSujetoIva          = SubTotal,            // Base neta (sin IVA)
                    Leyenda                      = string.IsNullOrWhiteSpace(config.LeyendaLey453)
                                                    ? "LEY N° 453: EL PROVEEDOR DEBE ENTREGAR ESTA FACTURA AL CONSUMIDOR."
                                                    : config.LeyendaLey453,
                    EstadoEnvio                  = EstadoEnvioSin.PendienteEnvio,
                    Details = Items.Select(i => new ServiceInvoiceDetail
                    {
                        ActividadEconomica = string.IsNullOrWhiteSpace(config.ActividadEconomica)
                                             ? "000000"
                                             : config.ActividadEconomica,
                        CodigoProductoSin  = 83111,   // Código SIN — servicios generales
                        CodigoProducto     = "SRV-001",
                        Descripcion        = i.Descripcion,
                        Cantidad           = i.Cantidad,
                        UnidadMedida       = 58,       // 58 = Servicio (catálogo SIN)
                        PrecioUnitario     = i.PrecioUnitario,
                        SubTotal           = i.SubTotal
                    }).ToList()
                };

                // Paso 6: CUF + XML + Firma (o solo CUF en contingencia)
                InvoiceResult resultado;
                if (tieneCertificado)
                {
                    var password = string.IsNullOrWhiteSpace(config.PasswordCertificadoCifrado)
                        ? string.Empty
                        : DpapiProtector.Descifrar(config.PasswordCertificadoCifrado);

                    resultado = await _invoiceService.PrepararFacturaAsync(
                        factura, config.RutaCertificado!, password);
                }
                else
                {
                    // Modo contingencia — CUF calculado localmente sin certificado
                    var cuf = _invoiceService.CalcularCuf(factura);
                    factura.Cuf = cuf;
                    resultado = InvoiceResult.Ok(cuf, string.Empty);
                }

                if (!resultado.Exitoso)
                {
                    SetStatus($"⚠  Error al preparar factura: {resultado.Error}", true);
                    return;
                }

                factura.XmlFirmado = resultado.XmlFirmado;

                // Paso 7: Persistir en SQLite
                await _invoiceRepository.GuardarAsync(factura);

                // Paso 8: Actualizar frecuencia del cliente si ya está registrado
                var clienteExistente = await _clienteRepository
                    .ObtenerPorDocumentoAsync(NitCliente.Trim());
                if (clienteExistente is not null)
                    await _clienteRepository.RegistrarFacturaEmitidaAsync(clienteExistente.Id);

                // Paso 9: Generar QR
                byte[] qrBytes;
                try
                {
                    var urlQr = _qrService.GenerarUrlVerificacion(
                        config.Nit, factura.Cuf, factura.NumeroFactura);
                    qrBytes = _qrService.GenerarQrPng(urlQr, pixelesPorModulo: 4);
                }
                catch { qrBytes = Array.Empty<byte>(); }

                // Paso 10: Generar y abrir PDF
                try
                {
                    var rutaPdf = _pdfService.GuardarPdf(factura, config, qrBytes);
                    _pdfService.AbrirPdf(rutaPdf);
                }
                catch (Exception pdfEx)
                {
                    SetStatus($"⚠  Factura guardada pero error al generar PDF: {pdfEx.Message}", true);
                    return;
                }

                // Paso 11: Éxito
                var modoStr = tieneCertificado ? "en línea" : "contingencia";
                SetStatus(
                    $"✅  Factura N° {numeroFactura} emitida ({modoStr}).  " +
                    $"CUF: {factura.Cuf[..Math.Min(16, factura.Cuf.Length)]}...  " +
                    $"Total: Bs. {MontoTotal:N2}", false);

                LimpiarTodo();
            }
            catch (Exception ex)
            {
                SetStatus($"⚠  Error al emitir: {ex.Message}", true);
            }
            finally
            {
                IsProcessing = false;
            }
        }

        [RelayCommand]
        private async Task BuscarCliente()
        {
            NitError = string.Empty;
            MostrarSugerencias = false;

            if (string.IsNullOrWhiteSpace(NitCliente))
            {
                NitError = "Ingresa el NIT/CI para buscar.";
                return;
            }

            try
            {
                var clientes = await _clienteRepository.BuscarAsync(NitCliente.Trim());
                if (clientes.Count == 0)
                {
                    SetStatus("No se encontró cliente con ese NIT/CI. Puedes ingresarlo manualmente.", false);
                    return;
                }

                if (clientes.Count == 1)
                {
                    SeleccionarCliente(clientes[0]);
                    return;
                }

                SugerenciasCliente.Clear();
                foreach (var c in clientes)
                    SugerenciasCliente.Add(c);
                MostrarSugerencias = true;
            }
            catch (Exception ex)
            {
                SetStatus($"Error al buscar cliente: {ex.Message}", true);
            }
        }

        [RelayCommand]
        private void SeleccionarClienteSugerencia(ClienteFrecuente cliente)
        {
            SeleccionarCliente(cliente);
            MostrarSugerencias = false;
        }

        private void SeleccionarCliente(ClienteFrecuente cliente)
        {
            NitCliente    = cliente.NumeroDocumento;
            NombreCliente = cliente.NombreRazonSocial;
            SetStatus($"✔ Cliente: {cliente.NombreRazonSocial}", false);
        }

        // ── Validaciones ──────────────────────────────────────────────────────

        private bool ValidarCabecera()
        {
            var ok = true;
            NitError = NombreError = string.Empty;

            if (string.IsNullOrWhiteSpace(NitCliente))
            { NitError = "El NIT/CI es obligatorio."; ok = false; }
            else if (!System.Text.RegularExpressions.Regex.IsMatch(NitCliente.Trim(), @"^\d+$"))
            { NitError = "Solo se permiten números."; ok = false; }

            if (string.IsNullOrWhiteSpace(NombreCliente))
            { NombreError = "El nombre/razón social es obligatorio."; ok = false; }

            return ok;
        }

        private bool ValidarItem()
        {
            var ok = true;
            DescripcionError = CantidadError = PrecioError = string.Empty;

            if (string.IsNullOrWhiteSpace(Descripcion))
            { DescripcionError = "La descripción es obligatoria."; ok = false; }

            if (ParseDecimal(Cantidad) <= 0)
            { CantidadError = "Cantidad debe ser mayor a 0."; ok = false; }

            if (ParseDecimal(PrecioUnitario) <= 0)
            { PrecioError = "Precio debe ser mayor a 0."; ok = false; }

            if (ParseDecimal(Descuento) < 0)
            { PrecioError = "El descuento no puede ser negativo."; ok = false; }

            return ok;
        }

        // ── Cálculo de totales ────────────────────────────────────────────────
        //
        // Flujo estándar Bolivia (precios son BASE sin IVA):
        //   SubTotal       = Σ(cantidad × precio) − descuentos      ← base neta
        //   IVA (13%)      = SubTotal × 0.13
        //   MontoTotal     = SubTotal + IVA                          ← lo que paga el cliente
        //
        // El SIN recibe:
        //   montoTotalSujetoIva = SubTotal   (base imponible)
        //   montoTotal          = MontoTotal (con IVA incluido)
        // ─────────────────────────────────────────────────────────────────────
        private void RecalcularTotales()
        {
            var baseItems  = Items.Sum(i => i.Cantidad * i.PrecioUnitario);
            TotalDescuento = Items.Sum(i => i.Descuento);
            SubTotal       = baseItems - TotalDescuento;        // Base neta sin IVA
            MontoIva       = Math.Round(SubTotal * 0.13m, 2);   // 13% sobre la base
            MontoTotal     = SubTotal + MontoIva;               // Total que paga el cliente
        }

        private void LimpiarFormItem()
        {
            Descripcion      = string.Empty;
            Cantidad         = "1";
            PrecioUnitario   = string.Empty;
            Descuento        = "0";
            DescripcionError = CantidadError = PrecioError = string.Empty;
        }

        private void LimpiarTodo()
        {
            NitCliente    = NombreCliente = string.Empty;
            NitError      = NombreError   = string.Empty;
            MetodoPagoSeleccionado = MetodosPago[0];
            MostrarSugerencias    = false;
            SugerenciasCliente.Clear();
            Items.Clear();
            LimpiarFormItem();
            SubTotal = TotalDescuento = MontoIva = MontoTotal = 0m;
        }

        private void SetStatus(string msg, bool isError)
        {
            StatusMessage = msg;
            IsStatusError = isError;
        }
    }

    // ── Modelos locales ───────────────────────────────────────────────────────
    public class MetodoPagoItem
    {
        public string Codigo      { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public override string ToString() => Descripcion;
    }

    public class InvoiceDetailRow : ObservableObject
    {
        private string  _descripcion    = string.Empty;
        private decimal _cantidad;
        private decimal _precioUnitario;
        private decimal _descuento;
        private decimal _subTotal;

        public string  Descripcion    { get => _descripcion;    set => SetProperty(ref _descripcion, value); }
        public decimal Cantidad       { get => _cantidad;       set => SetProperty(ref _cantidad, value); }
        public decimal PrecioUnitario { get => _precioUnitario; set => SetProperty(ref _precioUnitario, value); }
        public decimal Descuento      { get => _descuento;      set => SetProperty(ref _descuento, value); }
        public decimal SubTotal       { get => _subTotal;       set => SetProperty(ref _subTotal, value); }
    }
}