using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace SiatBillingSystem.Desktop.ViewModels;

public partial class PosGridViewModel : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PuedeEmitir))]
    private string _nitCliente = string.Empty;

    [ObservableProperty]
    private string _nombreCliente = string.Empty;

    [ObservableProperty]
    private string _complemento = string.Empty;

    [ObservableProperty]
    private int _tipoDocumento = 1;

    [ObservableProperty]
    private string _descripcionItem = string.Empty;

    [ObservableProperty]
    private decimal _cantidad = 1;

    [ObservableProperty]
    private decimal _precioUnitario;

    public ObservableCollection<DetalleFacturaItem> Items { get; } = new();

    [ObservableProperty]
    private decimal _subtotal;

    [ObservableProperty]
    private decimal _descuento;

    [ObservableProperty]
    private decimal _total;

    [ObservableProperty]
    private decimal _iva;

    [ObservableProperty]
    private string _estadoEnvio = "Sin enviar";

    [ObservableProperty]
    private bool _modoOffline = false;

    [ObservableProperty]
    private int _metodoPago = 1;

    [ObservableProperty]
    private string _leyendaFactura = "Ley N 453: Usted tiene derecho a recibir informacion veraz, suficiente y oportuna.";

    public bool PuedeEmitir =>
        !string.IsNullOrWhiteSpace(NitCliente) && Items.Count > 0;

    public bool TieneItems => Items.Count > 0;

    [RelayCommand(CanExecute = nameof(PuedeAgregarItem))]
    private void AgregarItem()
    {
        if (string.IsNullOrWhiteSpace(DescripcionItem) || PrecioUnitario <= 0) return;

        var item = new DetalleFacturaItem
        {
            Numero = Items.Count + 1,
            Descripcion = DescripcionItem,
            Cantidad = Cantidad,
            PrecioUnitario = PrecioUnitario,
            Subtotal = Cantidad * PrecioUnitario
        };

        Items.Add(item);
        RecalcularTotales();

        DescripcionItem = string.Empty;
        Cantidad = 1;
        PrecioUnitario = 0;

        OnPropertyChanged(nameof(PuedeEmitir));
        OnPropertyChanged(nameof(TieneItems));
    }

    private bool PuedeAgregarItem() =>
        !string.IsNullOrWhiteSpace(DescripcionItem) && PrecioUnitario > 0;

    [RelayCommand]
    private void EliminarItem(DetalleFacturaItem item)
    {
        Items.Remove(item);
        for (int i = 0; i < Items.Count; i++)
            Items[i].Numero = i + 1;

        RecalcularTotales();
        OnPropertyChanged(nameof(PuedeEmitir));
        OnPropertyChanged(nameof(TieneItems));
    }

    [RelayCommand(CanExecute = nameof(PuedeEmitir))]
    private async Task EmitirFacturaAsync()
    {
        EstadoEnvio = "Guardando localmente...";
        await Task.Delay(150);
        EstadoEnvio = "Guardado localmente - pendiente sincronizacion SIAT";
        NuevaFactura();
    }

    [RelayCommand]
    private void NuevaFactura()
    {
        NitCliente = string.Empty;
        NombreCliente = string.Empty;
        Complemento = string.Empty;
        Items.Clear();
        DescripcionItem = string.Empty;
        Cantidad = 1;
        PrecioUnitario = 0;
        Subtotal = 0;
        Total = 0;
        Iva = 0;
        Descuento = 0;
        EstadoEnvio = "Sin enviar";
        OnPropertyChanged(nameof(PuedeEmitir));
        OnPropertyChanged(nameof(TieneItems));
    }

    private void RecalcularTotales()
    {
        Subtotal = Items.Sum(i => i.Subtotal);
        Total = Subtotal - Descuento;
        Iva = Math.Round(Total * 13m / 113m, 2);
    }

    partial void OnDescripcionItemChanged(string value) =>
        AgregarItemCommand.NotifyCanExecuteChanged();

    partial void OnPrecioUnitarioChanged(decimal value) =>
        AgregarItemCommand.NotifyCanExecuteChanged();

    partial void OnDescuentoChanged(decimal value)
    {
        Total = Subtotal - value;
        Iva = Math.Round(Total * 13m / 113m, 2);
    }
}

public partial class DetalleFacturaItem : ObservableObject
{
    [ObservableProperty] private int _numero;
    [ObservableProperty] private string _descripcion = string.Empty;
    [ObservableProperty] private decimal _cantidad;
    [ObservableProperty] private decimal _precioUnitario;
    [ObservableProperty] private decimal _subtotal;

    partial void OnCantidadChanged(decimal value) =>
        Subtotal = value * PrecioUnitario;

    partial void OnPrecioUnitarioChanged(decimal value) =>
        Subtotal = Cantidad * value;
}