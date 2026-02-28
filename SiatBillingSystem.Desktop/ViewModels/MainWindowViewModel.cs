using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using SiatBillingSystem.Infrastructure.Persistence;

namespace SiatBillingSystem.Desktop.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        
        private PosGridViewModel?       _posVm;
        private ClientesViewModel?      _clientesVm;
        private ConfiguracionViewModel? _configuracionVm;

        [ObservableProperty] private ObservableObject? _currentViewModel;
        [ObservableProperty] private string _currentPageTitle = "Grilla POS";
        [ObservableProperty] private string _empresaNombre    = "Mi Empresa";
        [ObservableProperty] private string _nitEmpresa       = string.Empty;
        [ObservableProperty] private bool   _isMenuExpanded   = true;

        public MainWindowViewModel() { }

        public void SetDbFactory(IDbContextFactory<SiatDbContext> factory)
        {
            _posVm           = new PosGridViewModel();
            _clientesVm      = new ClientesViewModel(factory);
            _configuracionVm = new ConfiguracionViewModel(factory);
        }

        public void Initialize() => NavigateToPos();

        [RelayCommand]
        private void NavigateToPos()
        {
            CurrentViewModel = _posVm ??= new PosGridViewModel();
            CurrentPageTitle = "Grilla de Facturación POS";
            ActiveSection = "POS";
        }

        [RelayCommand]
        private void NavigateToClientes()
        {
            CurrentViewModel = _clientesVm;
            CurrentPageTitle = "Clientes Frecuentes";
            ActiveSection = "Clientes";
        }

        [RelayCommand]
        private void NavigateToConfiguracion()
        {
            CurrentViewModel = _configuracionVm;
            CurrentPageTitle = "Configuración de Empresa";
            ActiveSection = "Configuracion";
        }

        [RelayCommand]
        private void ToggleMenu() => IsMenuExpanded = !IsMenuExpanded;

        [ObservableProperty] private string _activeSection = "POS";
    }
}