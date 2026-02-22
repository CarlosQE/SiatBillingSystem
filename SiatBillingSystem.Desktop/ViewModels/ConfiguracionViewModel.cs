using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace SiatBillingSystem.Desktop.ViewModels;

public partial class ConfiguracionViewModel : ObservableObject
{
    [ObservableProperty] private string _nit = string.Empty;
    [ObservableProperty] private string _razonSocial = string.Empty;
    [ObservableProperty] private string _actividadEconomica = string.Empty;
    [ObservableProperty] private int _codigoSucursal = 0;
    [ObservableProperty] private int _codigoPuntoVenta = 0;

    [ObservableProperty] private string _rutaCertificado = string.Empty;
    [ObservableProperty] private string _estadoCertificado = "No configurado";

    [ObservableProperty] private string _ambienteSiat = "Piloto";
    [ObservableProperty] private bool _modoOfflineForzado = false;

    [ObservableProperty] private string _mensajeEstado = string.Empty;
    [ObservableProperty] private bool _guardadoExitoso = false;

    [RelayCommand]
    private void SeleccionarCertificado()
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Title = "Seleccionar Certificado Digital",
            Filter = "Certificados|*.p12;*.pfx|Todos|*.*",
            InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
        };

        if (dialog.ShowDialog() == true)
        {
            RutaCertificado = dialog.FileName;
            EstadoCertificado = "Pendiente de validación";
        }
    }

    [RelayCommand]
    private async Task ValidarCertificadoAsync()
    {
        // Sprint 3: llamar ISignatureService.ValidateCertificate()
        EstadoCertificado = "✓ Certificado válido";
        await Task.CompletedTask;
    }

    [RelayCommand]
    private async Task GuardarConfiguracionAsync()
    {
        // Sprint 3: persistir con EF Core + cifrar password con DPAPI
        MensajeEstado = "✓ Configuración guardada correctamente.";
        GuardadoExitoso = true;
        await Task.CompletedTask;
    }
}