using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Windows;

namespace SiatBillingSystem.Desktop.ViewModels
{
    public partial class PosGridViewModel : ObservableObject
    {
        // ── Cliente ──────────────────────────────────────────────
        [ObservableProperty] private string _nitCliente = string.Empty;
        [ObservableProperty] private string _nombreCliente = string.Empty;
        [ObservableProperty] private string _metodoPago = "1";
        [ObservableProperty] private string _nitError = string.Empty;
        [ObservableProperty] private string _nombreError = string.Empty;

        // ── Item en edición ──────────────────────────────────────
        [ObservableProperty] private string _descripcion = string.Empty;
        [ObservableProperty] private string _cantidad = "1";
        [ObservableProperty] private string _precioUnitario = string.Empty;
        [ObservableProperty] private string _descuento = "0";
        [ObservableProperty] private string _descripcionError = string.Empty;
        [ObservableProperty] private string _cantidadError = string.Empty;
        [ObservableProperty] private string _precioError = string.Empty;

        // ── Totales ──────────────────────────────────────────────
        [ObservableProperty] private decimal _subTotal = 0m;
        [ObservableProperty] private decimal _totalDescuento = 0m;
        [ObservableProperty] private decimal _montoTotal = 0m;
        [ObservableProperty] private decimal _montoIva = 0m;

        // ── Estado UI ────────────────────────────────────────────
        [ObservableProperty] private string _statusMessage = string.Empty;
        [ObservableProperty] private bool _isStatusError = false;

        // ── Catálogos ────────────────────────────────────────────
        public ObservableCollection<MetodoPagoItem> MetodosPago { get; } = new()
        {
            new MetodoPagoItem { Codigo = "1", Descripcion = "Efectivo" },
            new MetodoPagoItem { Codigo = "2", Descripcion = "Tarjeta Débito" },
            new MetodoPagoItem { Codigo = "3", Descripcion = "Tarjeta Crédito" },
            new MetodoPagoItem { Codigo = "7", Descripcion = "Transferencia" },
        };

        private MetodoPagoItem? _metodoPagoSeleccionado;
        public MetodoPagoItem? MetodoPagoSeleccionado
        {
            get => _metodoPagoSeleccionado;
            set => SetProperty(ref _metodoPagoSeleccionado, value);
        }

        // ── Grilla ───────────────────────────────────────────────
        public ObservableCollection<InvoiceDetailRow> Items { get; } = new();

        private InvoiceDetailRow? _selectedItem;
        public InvoiceDetailRow? SelectedItem
        {
            get => _selectedItem;
            set => SetProperty(ref _selectedItem, value);
        }

        public PosGridViewModel()
        {
            MetodoPagoSeleccionado = MetodosPago[0];
            Items.CollectionChanged += (_, _) => RecalcularTotales();
        }

        // ── Comandos ─────────────────────────────────────────────

        [RelayCommand]
        private void AgregarItem()
        {
            if (!ValidarItem()) return;

            var cant = decimal.Parse(Cantidad);
            var precio = decimal.Parse(PrecioUnitario);
            var desc = decimal.TryParse(Descuento, out var d) ? d : 0m;
            if (desc < 0) desc = 0m;

            var subtotalLinea = cant * precio - desc;
            if (subtotalLinea < 0) subtotalLinea = 0m;

            Items.Add(new InvoiceDetailRow
            {
                Descripcion = Descripcion.Trim(),
                Cantidad = cant,
                PrecioUnitario = precio,
                Descuento = desc,
                SubTotal = subtotalLinea
            });

            LimpiarFormItem();
            SetStatus($"✓ Ítem agregado. Total líneas: {Items.Count}", false);
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
                    "¿Limpiar la factura actual? Se perderán los ítems cargados.",
                    "Nexus — Nueva Factura",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result != MessageBoxResult.Yes) return;
            }

            LimpiarTodo();
            SetStatus("Nueva factura lista.", false);
        }

        [RelayCommand]
        private void EmitirFactura()
        {
            if (!ValidarCabecera()) return;
            if (Items.Count == 0)
            {
                SetStatus("Agrega al menos un ítem antes de emitir.", true);
                return;
            }

            // TODO Sprint 3: invocar servicio de emisión
            SetStatus($"✓ Factura emitida — Total: Bs. {MontoTotal:N2}", false);
        }

       [RelayCommand]
private void BuscarCliente()
{
    NitError = string.Empty;
    if (string.IsNullOrWhiteSpace(NitCliente))
    {
        NitError = "Ingresa el NIT para buscar.";
        return;
    }
    // Sprint 3: conectar con IClienteRepository.ObtenerPorDocumentoAsync()
    SetStatus($"Buscar NIT {NitCliente} disponible en Sprint 3.", false);
}

        // ── Validaciones ─────────────────────────────────────────

        private bool ValidarCabecera()
        {
            var ok = true;
            NitError = string.Empty;
            NombreError = string.Empty;

            if (string.IsNullOrWhiteSpace(NitCliente))
            { NitError = "El NIT/CI es obligatorio."; ok = false; }
            else if (!System.Text.RegularExpressions.Regex.IsMatch(NitCliente, @"^\d+$"))
            { NitError = "Solo se permiten números."; ok = false; }

            if (string.IsNullOrWhiteSpace(NombreCliente))
            { NombreError = "El nombre/razón social es obligatorio."; ok = false; }

            return ok;
        }

        private bool ValidarItem()
        {
            var ok = true;
            DescripcionError = string.Empty;
            CantidadError = string.Empty;
            PrecioError = string.Empty;

            if (string.IsNullOrWhiteSpace(Descripcion))
            { DescripcionError = "La descripción es obligatoria."; ok = false; }

            if (!decimal.TryParse(Cantidad, out var cant) || cant <= 0)
            { CantidadError = "Cantidad debe ser mayor a 0."; ok = false; }

            if (!decimal.TryParse(PrecioUnitario, out var precio) || precio <= 0)
            { PrecioError = "Precio debe ser mayor a 0."; ok = false; }

            if (decimal.TryParse(Descuento, out var desc) && desc < 0)
            { PrecioError = "El descuento no puede ser negativo."; ok = false; }

            return ok;
        }

        // ── Helpers ──────────────────────────────────────────────

        private void RecalcularTotales()
        {
            SubTotal = Items.Sum(i => i.Cantidad * i.PrecioUnitario);
            TotalDescuento = Items.Sum(i => i.Descuento);
            MontoTotal = Items.Sum(i => i.SubTotal);
            MontoIva = Math.Round(MontoTotal / 1.13m * 0.13m, 2);
        }

        private void LimpiarFormItem()
        {
            Descripcion = string.Empty;
            Cantidad = "1";
            PrecioUnitario = string.Empty;
            Descuento = "0";
            DescripcionError = string.Empty;
            CantidadError = string.Empty;
            PrecioError = string.Empty;
        }

        private void LimpiarTodo()
        {
            NitCliente = string.Empty;
            NombreCliente = string.Empty;
            MetodoPagoSeleccionado = MetodosPago[0];
            NitError = string.Empty;
            NombreError = string.Empty;
            Items.Clear();
            LimpiarFormItem();
            SubTotal = TotalDescuento = MontoTotal = MontoIva = 0m;
        }

        private void SetStatus(string msg, bool isError)
        {
            StatusMessage = msg;
            IsStatusError = isError;
        }
    }

    // ── Modelos locales ──────────────────────────────────────────
    public class MetodoPagoItem
    {
        public string Codigo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public override string ToString() => Descripcion;
    }

    public class InvoiceDetailRow : ObservableObject
    {
        private string _descripcion = string.Empty;
        private decimal _cantidad;
        private decimal _precioUnitario;
        private decimal _descuento;
        private decimal _subTotal;

        public string Descripcion
        { get => _descripcion; set => SetProperty(ref _descripcion, value); }
        public decimal Cantidad
        { get => _cantidad; set => SetProperty(ref _cantidad, value); }
        public decimal PrecioUnitario
        { get => _precioUnitario; set => SetProperty(ref _precioUnitario, value); }
        public decimal Descuento
        { get => _descuento; set => SetProperty(ref _descuento, value); }
        public decimal SubTotal
        { get => _subTotal; set => SetProperty(ref _subTotal, value); }
    }
}