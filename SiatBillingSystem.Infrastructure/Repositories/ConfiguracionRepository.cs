using Microsoft.EntityFrameworkCore;
using SiatBillingSystem.Application.Interfaces;
using SiatBillingSystem.Domain.Entities;
using SiatBillingSystem.Infrastructure.Persistence;

namespace SiatBillingSystem.Infrastructure.Repositories;

public class ConfiguracionRepository : IConfiguracionRepository
{
    private readonly SiatDbContext _context;

    // Lock para garantizar que el incremento del número de factura
    // sea atómico en escenarios de 2-5 usuarios simultáneos
    private static readonly SemaphoreSlim _lockNumeroFactura = new(1, 1);

    public ConfiguracionRepository(SiatDbContext context)
    {
        _context = context;
    }

    public async Task<ConfiguracionEmpresa?> ObtenerAsync()
    {
        return await _context.ConfiguracionEmpresa.FirstOrDefaultAsync();
    }

    public async Task GuardarAsync(ConfiguracionEmpresa configuracion)
    {
        if (configuracion.Id == 0)
            _context.ConfiguracionEmpresa.Add(configuracion);
        else
            _context.ConfiguracionEmpresa.Update(configuracion);

        configuracion.FechaUltimaActualizacion = DateTime.Now;
        await _context.SaveChangesAsync();
    }

    public async Task ActualizarCufdAsync(string nuevoCufd, DateTime vencimiento)
    {
        var config = await ObtenerAsync()
            ?? throw new InvalidOperationException(
                "No existe configuración de empresa. Configure el sistema antes de facturar.");

        config.Cufd = nuevoCufd;
        config.FechaCufd = DateTime.Now;
        config.VencimientoCufd = vencimiento;
        config.FechaUltimaActualizacion = DateTime.Now;

        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Incrementa y retorna el siguiente número de factura de forma atómica.
    /// Usa SemaphoreSlim para evitar números duplicados con múltiples usuarios simultáneos.
    /// CRÍTICO: Dos facturas no pueden tener el mismo número — viola las reglas del SIN.
    /// </summary>
    public async Task<long> ObtenerSiguienteNumeroFacturaAsync()
    {
        await _lockNumeroFactura.WaitAsync();
        try
        {
            var config = await ObtenerAsync()
                ?? throw new InvalidOperationException(
                    "No existe configuración de empresa. Configure el sistema antes de facturar.");

            config.UltimoNumeroFactura++;
            config.FechaUltimaActualizacion = DateTime.Now;
            await _context.SaveChangesAsync();

            return config.UltimoNumeroFactura;
        }
        finally
        {
            _lockNumeroFactura.Release();
        }
    }
}
