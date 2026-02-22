using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;

namespace SiatBillingSystem.Desktop.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    private readonly IServiceProvider _services;

    [ObservableProperty]
    private ObservableObject? _currentViewModel;

    [ObservableProperty]
    private string _currentPageTitle = "Grilla POS";

    [ObservableProperty]
    private string _empresaNombre = "Mi Empresa";

    [ObservableProperty]
    private string _nitEmpresa = "";

    [ObservableProperty]
    private bool _isMenuExpanded = true;

    public MainWindowViewModel(IServiceProvider services)
    {
        _services = services;
        NavigateToPosCommand.Execute(null);
    }

    [RelayCommand]
    private void NavigateToPos()
    {
        CurrentViewModel = _services.GetRequiredService<PosGridViewModel>();
        CurrentPageTitle = "Grilla de Facturación POS";
    }

    [RelayCommand]
    private void NavigateToClientes()
    {
        CurrentViewModel = _services.GetRequiredService<ClientesViewModel>();
        CurrentPageTitle = "Clientes Frecuentes";
    }

    [RelayCommand]
    private void NavigateToConfiguracion()
    {
        CurrentViewModel = _services.GetRequiredService<ConfiguracionViewModel>();
        CurrentPageTitle = "Configuración de Empresa";
    }

    [RelayCommand]
    private void ToggleMenu() => IsMenuExpanded = !IsMenuExpanded;
}