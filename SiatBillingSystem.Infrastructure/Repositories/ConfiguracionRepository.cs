using Microsoft.EntityFrameworkCore;
using SiatBillingSystem.Application.Interfaces;
using SiatBillingSystem.Domain.Entities;
using SiatBillingSystem.Infrastructure.Persistence;

namespace SiatBillingSystem.Infrastructure.Repositories;

public class ConfiguracionRepository : IConfiguracionRepository
{
    private readonly IDbContextFactory<SiatDbContext> _contextFactory;

    // Lock para garantizar que el incremento del número de factura
    // sea atómico en escenarios de 2-5 usuarios simultáneos.
    // CRÍTICO: Dos facturas no pueden tener el mismo número — viola las reglas del SIN.
    private static readonly SemaphoreSlim _lockNumeroFactura = new(1, 1);

    public ConfiguracionRepository(IDbContextFactory<SiatDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<ConfiguracionEmpresa?> ObtenerAsync()
    {
        await using var ctx = await _contextFactory.CreateDbContextAsync();
        return await ctx.ConfiguracionEmpresa.FirstOrDefaultAsync();
    }

    public async Task GuardarAsync(ConfiguracionEmpresa configuracion)
    {
        await using var ctx = await _contextFactory.CreateDbContextAsync();
        if (configuracion.Id == 0)
            ctx.ConfiguracionEmpresa.Add(configuracion);
        else
            ctx.ConfiguracionEmpresa.Update(configuracion);
        configuracion.FechaUltimaActualizacion = DateTime.Now;
        await ctx.SaveChangesAsync();
    }

    public async Task ActualizarCufdAsync(string nuevoCufd, DateTime vencimiento)
    {
        await using var ctx = await _contextFactory.CreateDbContextAsync();
        var config = await ctx.ConfiguracionEmpresa.FirstOrDefaultAsync()
            ?? throw new InvalidOperationException(
                "No existe configuración de empresa. Configure el sistema antes de facturar.");
        config.Cufd                    = nuevoCufd;
        config.FechaCufd               = DateTime.Now;
        config.VencimientoCufd         = vencimiento;
        config.FechaUltimaActualizacion = DateTime.Now;
        await ctx.SaveChangesAsync();
    }

    public async Task<long> ObtenerSiguienteNumeroFacturaAsync()
    {
        await _lockNumeroFactura.WaitAsync();
        try
        {
            await using var ctx = await _contextFactory.CreateDbContextAsync();
            var config = await ctx.ConfiguracionEmpresa.FirstOrDefaultAsync()
                ?? throw new InvalidOperationException(
                    "No existe configuración de empresa. Configure el sistema antes de facturar.");
            config.UltimoNumeroFactura++;
            config.FechaUltimaActualizacion = DateTime.Now;
            await ctx.SaveChangesAsync();
            return config.UltimoNumeroFactura;
        }
        finally
        {
            _lockNumeroFactura.Release();
        }
    }
}
