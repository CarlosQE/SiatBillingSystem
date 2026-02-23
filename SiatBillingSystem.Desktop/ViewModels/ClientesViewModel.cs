using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Windows;

namespace SiatBillingSystem.Desktop.ViewModels
{
    public partial class ClientesViewModel : ObservableObject
    {
        [ObservableProperty] private string _filtro = string.Empty;
        [ObservableProperty] private ClienteFrecuenteItem? _clienteSeleccionado;
        [ObservableProperty] private bool _modoEdicion = false;

        // Campos del formulario de edición
        [ObservableProperty] private string _editNit = string.Empty;
        [ObservableProperty] private string _editNombre = string.Empty;
        [ObservableProperty] private string _editEmail = string.Empty;
        [ObservableProperty] private string _editTelefono = string.Empty;

        // Errores de validación
        [ObservableProperty] private string _editNitError = string.Empty;
        [ObservableProperty] private string _editNombreError = string.Empty;
        [ObservableProperty] private string _statusMessage = string.Empty;
        [ObservableProperty] private bool _isStatusError = false;

        private bool _esNuevoCliente = false;

        public ObservableCollection<ClienteFrecuenteItem> Clientes { get; } = new();
        public ObservableCollection<ClienteFrecuenteItem> ClientesFiltrados { get; } = new();

        public ClientesViewModel()
        {
            Clientes.Add(new ClienteFrecuenteItem
            {
                Nit = "1234567", Nombre = "Juan Pérez",
                Email = "juan@email.com", Telefono = "70012345", TotalFacturas = 12
            });
            Clientes.Add(new ClienteFrecuenteItem
            {
                Nit = "9876543", Nombre = "Clínica San Lucas",
                Email = "info@sanlucas.bo", Telefono = "22334455", TotalFacturas = 48
            });
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
            EditNitError = string.Empty;
            EditNombreError = string.Empty;
            StatusMessage = string.Empty;
            _esNuevoCliente = true;
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
            EditNitError = string.Empty;
            EditNombreError = string.Empty;
            StatusMessage = string.Empty;
            _esNuevoCliente = false;
            ModoEdicion = true;
        }

        [RelayCommand]
        private void EliminarCliente(ClienteFrecuenteItem cliente)
        {
            var result = MessageBox.Show(
                $"¿Eliminar al cliente {cliente.Nombre}?",
                "Nexus — Confirmar",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            Clientes.Remove(cliente);
            AplicarFiltro();
            SetStatus("Cliente eliminado.", false);
        }

        [RelayCommand]
        private void GuardarCliente()
        {
            if (!Validar()) return;

            if (_esNuevoCliente)
            {
                // Verificar NIT duplicado
                if (Clientes.Any(c => c.Nit == EditNit.Trim()))
                {
                    EditNitError = "Ya existe un cliente con este NIT.";
                    return;
                }

                var nuevo = new ClienteFrecuenteItem
                {
                    Nit = EditNit.Trim(),
                    Nombre = EditNombre.Trim(),
                    Email = EditEmail.Trim(),
                    Telefono = EditTelefono.Trim(),
                    TotalFacturas = 0
                };
                Clientes.Add(nuevo);
                SetStatus($"✓ Cliente '{nuevo.Nombre}' agregado.", false);
            }
            else if (ClienteSeleccionado is not null)
            {
                ClienteSeleccionado.Nit = EditNit.Trim();
                ClienteSeleccionado.Nombre = EditNombre.Trim();
                ClienteSeleccionado.Email = EditEmail.Trim();
                ClienteSeleccionado.Telefono = EditTelefono.Trim();
                SetStatus($"✓ Cliente actualizado.", false);
            }

            AplicarFiltro();
            ModoEdicion = false;
            // TODO Sprint 3: persistir en SQLite con IClienteRepository
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
            { EditNitError = "Solo se permiten números."; ok = false; }

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

    public partial class ClienteFrecuenteItem : ObservableObject
    {
        [ObservableProperty] private string _nit = string.Empty;
        [ObservableProperty] private string _nombre = string.Empty;
        [ObservableProperty] private string _email = string.Empty;
        [ObservableProperty] private string _telefono = string.Empty;
        [ObservableProperty] private int _totalFacturas;
    }
}