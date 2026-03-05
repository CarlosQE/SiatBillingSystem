using SiatBillingSystem.Domain.Entities;
using SiatBillingSystem.Domain.Enums;

namespace SiatBillingSystem.Application.Interfaces;

/// <summary>
/// Repositorio de facturas. Define las operaciones de persistencia
/// sin acoplarse a EF Core ni a SQLite (Clean Architecture).
/// </summary>
public interface IInvoiceRepository
{
    /// <summary>Guarda una nueva factura en la BD local. Retorna el Id asignado.</summary>
    Task<int> GuardarAsync(ServiceInvoice factura);

    /// <summary>Actualiza el estado de una factura existente (ej: de Pendiente a Aceptada).</summary>
    Task ActualizarEstadoAsync(int id, EstadoEnvioSin nuevoEstado,
        string? codigoAutorizacion = null, string? motivoRechazo = null);

    /// <summary>Obtiene facturas filtradas para el historial del administrador.</summary>
    Task<List<ServiceInvoice>> ObtenerHistorialAsync(
        DateTime? desde = null,
        DateTime? hasta = null,
        EstadoEnvioSin? estado = null,
        string? numeroDocumentoCliente = null);

    /// <summary>
    /// Obtiene todas las facturas pendientes de envío al SIN.
    /// Usado por el Background Worker de sincronización.
    /// </summary>
    Task<List<ServiceInvoice>> ObtenerPendientesEnvioAsync();

    /// <summary>Obtiene una factura por su Id local.</summary>
    Task<ServiceInvoice?> ObtenerPorIdAsync(int id);

    /// <summary>Obtiene una factura por su CUF.</summary>
    Task<ServiceInvoice?> ObtenerPorCufAsync(string cuf);

    /// <summary>Obtiene el último número de factura para calcular el siguiente.</summary>
    Task<long> ObtenerUltimoNumeroFacturaAsync();
}

/// <summary>
/// Repositorio de clientes frecuentes.
/// </summary>
public interface IClienteRepository
{
    /// <summary>Busca clientes por NIT/CI (búsqueda parcial, para autocompletar en POS).</summary>
    Task<List<ClienteFrecuente>> BuscarAsync(string termino);

    /// <summary>Obtiene un cliente por su número de documento exacto.</summary>
    Task<ClienteFrecuente?> ObtenerPorDocumentoAsync(string numeroDocumento);

    /// <summary>Guarda o actualiza un cliente frecuente.</summary>
    Task<int> GuardarAsync(ClienteFrecuente cliente);

    /// <summary>Actualiza la fecha de última factura y el contador total.</summary>
    Task RegistrarFacturaEmitidaAsync(int clienteId);

    /// <summary>Obtiene todos los clientes ordenados por última factura (más recientes primero).</summary>
    Task<List<ClienteFrecuente>> ObtenerTodosAsync();
}

/// <summary>
/// Repositorio de configuración de la empresa.
/// </summary>
public interface IConfiguracionRepository
{
    /// <summary>Obtiene la configuración actual de la empresa (registro único).</summary>
    Task<ConfiguracionEmpresa?> ObtenerAsync();

    /// <summary>Guarda o actualiza la configuración de la empresa.</summary>
    Task GuardarAsync(ConfiguracionEmpresa configuracion);

    /// <summary>Actualiza únicamente el CUFD y sus fechas de vigencia.</summary>
    Task ActualizarCufdAsync(string nuevoCufd, DateTime vencimiento);

    /// <summary>Incrementa y retorna el siguiente número de factura de forma atómica.</summary>
    Task<long> ObtenerSiguienteNumeroFacturaAsync();
}
