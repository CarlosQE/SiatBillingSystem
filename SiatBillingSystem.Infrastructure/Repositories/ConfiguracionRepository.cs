using Microsoft.EntityFrameworkCore;
using SiatBillingSystem.Application.Interfaces;
using SiatBillingSystem.Domain.Entities;
using SiatBillingSystem.Infrastructure.Persistence;

namespace SiatBillingSystem.Infrastructure.Repositories;

public class ConfiguracionRepository : IConfiguracionRepository
{
    private readonly IDbContextFactory<SiatDbContext> _factory;
    private static readonly SemaphoreSlim _lockNumeroFactura = new(1, 1);

    public ConfiguracionRepository(IDbContextFactory<SiatDbContext> factory)
    {
        _factory = factory;
    }

    public async Task<ConfiguracionEmpresa?> ObtenerAsync()
    {
        await using var db = _factory.CreateDbContext();
        return await db.ConfiguracionEmpresa.FirstOrDefaultAsync();
    }

    public async Task GuardarAsync(ConfiguracionEmpresa configuracion)
    {
        await using var db = _factory.CreateDbContext();

        if (configuracion.Id == 0)
            db.ConfiguracionEmpresa.Add(configuracion);
        else
            db.ConfiguracionEmpresa.Update(configuracion);

        configuracion.FechaUltimaActualizacion = DateTime.Now;
        await db.SaveChangesAsync();
    }

    public async Task ActualizarCufdAsync(string nuevoCufd, DateTime vencimiento)
    {
        await using var db = _factory.CreateDbContext();

        // ✓ Usar el mismo db para leer Y guardar
        var config = await db.ConfiguracionEmpresa.FirstOrDefaultAsync()
            ?? throw new InvalidOperationException(
                "No existe configuración de empresa. Configure el sistema antes de facturar.");

        config.Cufd                  = nuevoCufd;
        config.FechaCufd             = DateTime.Now;
        config.VencimientoCufd       = vencimiento;
        config.FechaUltimaActualizacion = DateTime.Now;

        await db.SaveChangesAsync();
    }

    public async Task<long> ObtenerSiguienteNumeroFacturaAsync()
    {
        await _lockNumeroFactura.WaitAsync();
        try
        {
            // ✓ Un solo db para toda la operación atómica
            await using var db = _factory.CreateDbContext();

            var config = await db.ConfiguracionEmpresa.FirstOrDefaultAsync()
                ?? throw new InvalidOperationException(
                    "No existe configuración de empresa. Configure el sistema antes de facturar.");

            config.UltimoNumeroFactura++;
            config.FechaUltimaActualizacion = DateTime.Now;
            await db.SaveChangesAsync();

            return config.UltimoNumeroFactura;
        }
        finally
        {
            _lockNumeroFactura.Release();
        }
    }
}