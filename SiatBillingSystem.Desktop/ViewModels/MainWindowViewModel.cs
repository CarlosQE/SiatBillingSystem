using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using SiatBillingSystem.Application.Interfaces;
using SiatBillingSystem.Infrastructure.Persistence;

namespace SiatBillingSystem.Desktop.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        private readonly PosGridViewModel        _posVm;
        private readonly ClientesViewModel       _clientesVm;
        private readonly ConfiguracionViewModel  _configuracionVm;

        [ObservableProperty] private ObservableObject? _currentViewModel;
        [ObservableProperty] private string _currentPageTitle = "Grilla POS";
        [ObservableProperty] private bool   _isMenuExpanded   = true;
        [ObservableProperty] private string _activeSection    = "POS";

        public MainWindowViewModel(
            IDbContextFactory<SiatDbContext> dbFactory,
            IInvoiceService                  invoiceService,
            IConfiguracionRepository         configuracionRepository)
        {
            _posVm           = new PosGridViewModel(invoiceService, configuracionRepository);
            _clientesVm      = new ClientesViewModel(dbFactory);
            _configuracionVm = new ConfiguracionViewModel(dbFactory);
        }

        public void Initialize() => NavigateToPos();

        [RelayCommand] private void NavigateToPos()
        {
            CurrentViewModel = _posVm;
            CurrentPageTitle = "Grilla de Facturación POS";
            ActiveSection    = "POS";
        }

        [RelayCommand] private void NavigateToClientes()
        {
            CurrentViewModel = _clientesVm;
            CurrentPageTitle = "Clientes Frecuentes";
            ActiveSection    = "Clientes";
        }

        [RelayCommand] private void NavigateToConfiguracion()
        {
            CurrentViewModel = _configuracionVm;
            CurrentPageTitle = "Configuración de Empresa";
            ActiveSection    = "Configuracion";
        }

        [RelayCommand] private void ToggleMenu() => IsMenuExpanded = !IsMenuExpanded;
    }
}