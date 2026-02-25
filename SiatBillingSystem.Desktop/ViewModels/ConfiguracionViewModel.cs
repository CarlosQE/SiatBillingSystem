using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using SiatBillingSystem.Domain.Entities;
using SiatBillingSystem.Infrastructure.Persistence;

namespace SiatBillingSystem.Desktop.ViewModels
{
    public partial class ConfiguracionViewModel : ObservableObject
    {
        private readonly IDbContextFactory<SiatDbContext>? _dbFactory;

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

        public ConfiguracionViewModel(IDbContextFactory<SiatDbContext> dbFactory)
        {
            _dbFactory = dbFactory;
            _ = CargarConfiguracionAsync();
        }

        private async Task CargarConfiguracionAsync()
        {
            try
            {
                using var db = _dbFactory!.CreateDbContext();
                var config = await db.ConfiguracionEmpresa.FirstOrDefaultAsync();
                if (config is not null)
                {
                    Nit = config.Nit ?? string.Empty;
                    RazonSocial = config.RazonSocial ?? string.Empty;
                    ActividadEconomica = config.ActividadEconomica ?? string.Empty;
                    CodigoSucursal = config.CodigoSucursal;
                    CodigoPuntoVenta = config.CodigoPuntoVenta ?? 0;
                    RutaCertificado = config.RutaCertificado ?? string.Empty;
                    if (!string.IsNullOrEmpty(RutaCertificado))
                        EstadoCertificado = "Certificado configurado";
                }
            }
            catch { /* Primera ejecucion sin config */ }
        }

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
                EstadoCertificado = "Pendiente de validacion";
            }
        }

        [RelayCommand]
        private void ValidarCertificado()
        {
            if (string.IsNullOrWhiteSpace(RutaCertificado))
            {
                EstadoCertificado = "Selecciona un archivo primero";
                return;
            }
            EstadoCertificado = "Certificado valido (simulado - Sprint 3)";
        }

        [RelayCommand]
        private async Task GuardarConfiguracionAsync()
        {
            try
            {
                using var db = _dbFactory!.CreateDbContext();
                var config = await db.ConfiguracionEmpresa.FirstOrDefaultAsync();

                if (config is null)
                {
                    config = new ConfiguracionEmpresa();
                    db.ConfiguracionEmpresa.Add(config);
                }

                config.Nit = Nit.Trim();
                config.RazonSocial = RazonSocial.Trim();
                config.ActividadEconomica = ActividadEconomica.Trim();
                config.CodigoSucursal = CodigoSucursal;
                config.CodigoPuntoVenta = CodigoPuntoVenta;
                config.RutaCertificado = RutaCertificado;
                config.FechaUltimaActualizacion = DateTime.Now;

                await db.SaveChangesAsync();

                MensajeEstado = "Configuracion guardada correctamente.";
                GuardadoExitoso = true;
            }
            catch (Exception ex)
            {
                MensajeEstado = $"Error: {ex.Message}";
                GuardadoExitoso = false;
            }
        }
    }
}