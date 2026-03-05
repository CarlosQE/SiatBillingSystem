using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using SiatBillingSystem.Domain.Entities;
using SiatBillingSystem.Infrastructure.Persistence;
using System.Collections.ObjectModel;
using System.Windows;

namespace SiatBillingSystem.Desktop.ViewModels
{
    public partial class ClientesViewModel : ObservableObject
    {
        private readonly IDbContextFactory<SiatDbContext>? _dbFactory;

        [ObservableProperty] private string _filtro = string.Empty;
        [ObservableProperty] private ClienteFrecuente? _clienteSeleccionado;
        [ObservableProperty] private bool _modoEdicion = false;
        [ObservableProperty] private string _editNit = string.Empty;
        [ObservableProperty] private string _editNombre = string.Empty;
        [ObservableProperty] private string _editEmail = string.Empty;
        [ObservableProperty] private string _editTelefono = string.Empty;
        [ObservableProperty] private string _editNitError = string.Empty;
        [ObservableProperty] private string _editNombreError = string.Empty;
        [ObservableProperty] private string _statusMessage = string.Empty;
        [ObservableProperty] private bool _isStatusError = false;

        private bool _esNuevoCliente = false;
        public ObservableCollection<ClienteFrecuente> ClientesFiltrados { get; } = new();
        private List<ClienteFrecuente> _todosLosClientes = new();

        public ClientesViewModel(IDbContextFactory<SiatDbContext> dbFactory)
        {
            _dbFactory = dbFactory;
            _ = CargarClientesAsync();
        }

        private async Task CargarClientesAsync()
        {
            try
            {
                using var db = _dbFactory!.CreateDbContext();
                _todosLosClientes = await db.ClientesFrecuentes
                    .OrderBy(c => c.NombreRazonSocial)
                    .ToListAsync();
                AplicarFiltro();
            }
            catch (Exception ex)
            {
                SetStatus($"Error cargando clientes: {ex.Message}", true);
            }
        }

        partial void OnFiltroChanged(string value) => AplicarFiltro();

        private void AplicarFiltro()
        {
            ClientesFiltrados.Clear();
            var termino = Filtro.ToLower();
            foreach (var c in _todosLosClientes.Where(x =>
                x.NumeroDocumento.Contains(termino, StringComparison.OrdinalIgnoreCase) ||
                x.NombreRazonSocial.Contains(termino, StringComparison.OrdinalIgnoreCase)))
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
            EditNitError = string.Empty;
            EditNombreError = string.Empty;
            StatusMessage = string.Empty;
            _esNuevoCliente = true;
            ModoEdicion = true;
        }

        [RelayCommand]
        private void EditarCliente(ClienteFrecuente cliente)
        {
            ClienteSeleccionado = cliente;
            EditNit = cliente.NumeroDocumento;
            EditNombre = cliente.NombreRazonSocial;
            EditEmail = cliente.Email ?? string.Empty;
            EditTelefono = cliente.Telefono ?? string.Empty;
            EditNitError = string.Empty;
            EditNombreError = string.Empty;
            StatusMessage = string.Empty;
            _esNuevoCliente = false;
            ModoEdicion = true;
        }

        [RelayCommand]
        private void EliminarCliente(ClienteFrecuente cliente)
        {
            var result = MessageBox.Show(
                $"Eliminar al cliente '{cliente.NombreRazonSocial}'?",
                "Nexus - Confirmar",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes) return;

            try
            {
                using var db = _dbFactory!.CreateDbContext();
                var entidad = db.ClientesFrecuentes.Find(cliente.Id);
                if (entidad != null)
                {
                    db.ClientesFrecuentes.Remove(entidad);
                    db.SaveChanges();
                }
                _todosLosClientes.Remove(cliente);
                AplicarFiltro();
                SetStatus("Cliente eliminado correctamente.", false);
            }
            catch (Exception ex)
            {
                SetStatus($"Error al eliminar: {ex.Message}", true);
            }
        }

        [RelayCommand]
        private void GuardarCliente()
        {
            if (!Validar()) return;

            try
            {
                using var db = _dbFactory!.CreateDbContext();

                if (_esNuevoCliente)
                {
                    if (_todosLosClientes.Any(c => c.NumeroDocumento == EditNit.Trim()))
                    {
                        EditNitError = "Ya existe un cliente con este NIT.";
                        return;
                    }
                    var nuevo = new ClienteFrecuente
                    {
                        NumeroDocumento = EditNit.Trim(),
                        NombreRazonSocial = EditNombre.Trim(),
                        Email = EditEmail.Trim(),
                        Telefono = EditTelefono.Trim(),
                        TotalFacturas = 0,
                        UltimaFactura = DateTime.Now
                    };
                    db.ClientesFrecuentes.Add(nuevo);
                    db.SaveChanges();
                    _todosLosClientes.Add(nuevo);
                    SetStatus($"Cliente '{nuevo.NombreRazonSocial}' guardado correctamente.", false);
                }

                else if (ClienteSeleccionado is not null)
                {
                    if (_todosLosClientes.Any(c => c.NumeroDocumento == EditNit.Trim() 
                                                && c.Id != ClienteSeleccionado.Id))
                    {
                        EditNitError = "Ya existe un cliente con este NIT.";
                        return;
                    }

                    var existente = db.ClientesFrecuentes.Find(ClienteSeleccionado.Id);
                    if (existente is not null)
                    {
                        existente.NumeroDocumento = EditNit.Trim();
                        existente.NombreRazonSocial = EditNombre.Trim();
                        existente.Email = EditEmail.Trim();
                        existente.Telefono = EditTelefono.Trim();
                        db.SaveChanges();
                        var local = _todosLosClientes.FirstOrDefault(c => c.Id == existente.Id);
                        if (local is not null)
                        {
                            local.NumeroDocumento = existente.NumeroDocumento;
                            local.NombreRazonSocial = existente.NombreRazonSocial;
                            local.Email = existente.Email;
                            local.Telefono = existente.Telefono;
                        }
                    }
                    SetStatus("Cliente actualizado correctamente.", false);
                }

                AplicarFiltro();
                ModoEdicion = false;
            }
            catch (Exception ex)
            {
                SetStatus($"Error al guardar: {ex.Message}", true);
            }
        }

        [RelayCommand]
        private void CancelarEdicion()
        {
            ModoEdicion = false;
            StatusMessage = string.Empty;
        }

        private bool Validar()
        {
            var ok = true;
            EditNitError = string.Empty;
            EditNombreError = string.Empty;
            if (string.IsNullOrWhiteSpace(EditNit))
            { EditNitError = "El NIT/CI es obligatorio."; ok = false; }
            else if (!System.Text.RegularExpressions.Regex.IsMatch(EditNit.Trim(), @"^\d+$"))
            { EditNitError = "Solo se permiten numeros."; ok = false; }
            if (string.IsNullOrWhiteSpace(EditNombre))
            { EditNombreError = "El nombre es obligatorio."; ok = false; }
            return ok;
        }

        private void SetStatus(string msg, bool isError)
        {
            StatusMessage = msg;
            IsStatusError = isError;
        }
    }
}