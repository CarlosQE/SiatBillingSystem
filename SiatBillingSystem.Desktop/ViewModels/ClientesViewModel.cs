using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace SiatBillingSystem.Desktop.ViewModels;

public partial class ClientesViewModel : ObservableObject
{
    [ObservableProperty]
    private string _filtro = string.Empty;

    [ObservableProperty]
    private ClienteFrecuenteItem? _clienteSeleccionado;

    [ObservableProperty]
    private bool _modoEdicion = false;

    [ObservableProperty] private string _editNit = string.Empty;
    [ObservableProperty] private string _editNombre = string.Empty;
    [ObservableProperty] private string _editEmail = string.Empty;
    [ObservableProperty] private string _editTelefono = string.Empty;

    public ObservableCollection<ClienteFrecuenteItem> Clientes { get; } = new();
    public ObservableCollection<ClienteFrecuenteItem> ClientesFiltrados { get; } = new();

    public ClientesViewModel()
    {
        // Datos de prueba para verificar que la UI funciona
        Clientes.Add(new ClienteFrecuenteItem { Nit = "1234567", Nombre = "Juan Pérez", Email = "juan@email.com", Telefono = "70012345", TotalFacturas = 12 });
        Clientes.Add(new ClienteFrecuenteItem { Nit = "9876543", Nombre = "Clínica San Lucas", Email = "info@sanlucas.bo", Telefono = "22334455", TotalFacturas = 48 });
        AplicarFiltro();
    }

    partial void OnFiltroChanged(string value) => AplicarFiltro();

    private void AplicarFiltro()
    {
        ClientesFiltrados.Clear();
        var termino = Filtro.ToLower();
        foreach (var c in Clientes.Where(x =>
            x.Nit.Contains(termino, StringComparison.OrdinalIgnoreCase) ||
            x.Nombre.Contains(termino, StringComparison.OrdinalIgnoreCase)))
        {
            ClientesFiltrados.Add(c);
        }
    }

    [RelayCommand]
    private void NuevoCliente()
    {
        ClienteSeleccionado = null;
        EditNit = string.Empty;
        EditNombre = string.Empty;
        EditEmail = string.Empty;
        EditTelefono = string.Empty;
        ModoEdicion = true;
    }

    [RelayCommand]
    private void EditarCliente(ClienteFrecuenteItem cliente)
    {
        ClienteSeleccionado = cliente;
        EditNit = cliente.Nit;
        EditNombre = cliente.Nombre;
        EditEmail = cliente.Email;
        EditTelefono = cliente.Telefono;
        ModoEdicion = true;
    }

    [RelayCommand]
    private async Task GuardarClienteAsync()
    {
        // Sprint 3: conectar con IClienteRepository
        ModoEdicion = false;
        await Task.CompletedTask;
    }

    [RelayCommand]
    private void CancelarEdicion() => ModoEdicion = false;
}

public partial class ClienteFrecuenteItem : ObservableObject
{
    [ObservableProperty] private string _nit = string.Empty;
    [ObservableProperty] private string _nombre = string.Empty;
    [ObservableProperty] private string _email = string.Empty;
    [ObservableProperty] private string _telefono = string.Empty;
    [ObservableProperty] private int _totalFacturas;
}