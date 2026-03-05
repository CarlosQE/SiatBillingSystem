using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;

namespace SiatBillingSystem.Desktop.ViewModels
{
    /// <summary>
    /// ViewModel de la ventana principal.
    /// Gestiona navegación entre vistas mediante ContentControl + DataTemplate.
    /// Cada NavigateTo... pide al contenedor DI una instancia nueva del ViewModel
    /// (Transient), garantizando estado limpio al volver a una vista.
    /// </summary>
    public partial class MainWindowViewModel : ObservableObject
    {
        private readonly IServiceProvider _serviceProvider;

        [ObservableProperty] private ObservableObject? _currentViewModel;
        [ObservableProperty] private string _currentPageTitle = "Grilla POS";
        [ObservableProperty] private bool   _isMenuExpanded   = true;

        public MainWindowViewModel(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;

            // Vista inicial al arrancar
            NavigateToPos();
        }

        // ─────────────────────────────────────────────────────────────────────
        // NAVEGACIÓN
        // ─────────────────────────────────────────────────────────────────────

        [RelayCommand]
        private void NavigateToPos()
        {
            CurrentViewModel = _serviceProvider.GetRequiredService<PosGridViewModel>();
            CurrentPageTitle = "Facturación POS  [F1]";
        }

        [RelayCommand]
        private void NavigateToClientes()
        {
            CurrentViewModel = _serviceProvider.GetRequiredService<ClientesViewModel>();
            CurrentPageTitle = "Clientes Frecuentes  [F2]";
        }

        [RelayCommand]
        private void NavigateToHistorial()
        {
            CurrentViewModel = _serviceProvider.GetRequiredService<HistorialViewModel>();
            CurrentPageTitle = "Historial de Facturas  [F3]";
        }

        [RelayCommand]
        private void NavigateToConfiguracion()
        {
            CurrentViewModel = _serviceProvider.GetRequiredService<ConfiguracionViewModel>();
            CurrentPageTitle = "Configuración de Empresa  [F10]";
        }

        [RelayCommand]
        private void ToggleMenu() => IsMenuExpanded = !IsMenuExpanded;
    }
}
