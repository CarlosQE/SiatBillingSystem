using Microsoft.EntityFrameworkCore;
using SiatBillingSystem.Application.Interfaces;
using SiatBillingSystem.Domain.Entities;
using SiatBillingSystem.Infrastructure.Persistence;

namespace SiatBillingSystem.Infrastructure.Repositories;

public class ConfiguracionRepository : IConfiguracionRepository
{
<<<<<<< HEAD
    private readonly IDbContextFactory<SiatDbContext> _factory;
    private static readonly SemaphoreSlim _lockNumeroFactura = new(1, 1);

    public ConfiguracionRepository(IDbContextFactory<SiatDbContext> factory)
    {
        _factory = factory;
=======
    private readonly IDbContextFactory<SiatDbContext> _contextFactory;

    // Lock para garantizar que el incremento del número de factura
    // sea atómico en escenarios de 2-5 usuarios simultáneos.
    // CRÍTICO: Dos facturas no pueden tener el mismo número — viola las reglas del SIN.
    private static readonly SemaphoreSlim _lockNumeroFactura = new(1, 1);

    public ConfiguracionRepository(IDbContextFactory<SiatDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
>>>>>>> 71335d2 (feat(sprint3): CUF real, PDF+QR, historial facturas, IVA 13%, sidebar colapsable con iconos)
    }

    public async Task<ConfiguracionEmpresa?> ObtenerAsync()
    {
<<<<<<< HEAD
        await using var db = _factory.CreateDbContext();
        return await db.ConfiguracionEmpresa.FirstOrDefaultAsync();
=======
        await using var ctx = await _contextFactory.CreateDbContextAsync();
        return await ctx.ConfiguracionEmpresa.FirstOrDefaultAsync();
>>>>>>> 71335d2 (feat(sprint3): CUF real, PDF+QR, historial facturas, IVA 13%, sidebar colapsable con iconos)
    }

    public async Task GuardarAsync(ConfiguracionEmpresa configuracion)
    {
<<<<<<< HEAD
        await using var db = _factory.CreateDbContext();

        if (configuracion.Id == 0)
            db.ConfiguracionEmpresa.Add(configuracion);
        else
            db.ConfiguracionEmpresa.Update(configuracion);

        configuracion.FechaUltimaActualizacion = DateTime.Now;
        await db.SaveChangesAsync();
=======
        await using var ctx = await _contextFactory.CreateDbContextAsync();
        if (configuracion.Id == 0)
            ctx.ConfiguracionEmpresa.Add(configuracion);
        else
            ctx.ConfiguracionEmpresa.Update(configuracion);
        configuracion.FechaUltimaActualizacion = DateTime.Now;
        await ctx.SaveChangesAsync();
>>>>>>> 71335d2 (feat(sprint3): CUF real, PDF+QR, historial facturas, IVA 13%, sidebar colapsable con iconos)
    }

    public async Task ActualizarCufdAsync(string nuevoCufd, DateTime vencimiento)
    {
<<<<<<< HEAD
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
=======
        await using var ctx = await _contextFactory.CreateDbContextAsync();
        var config = await ctx.ConfiguracionEmpresa.FirstOrDefaultAsync()
            ?? throw new InvalidOperationException(
                "No existe configuración de empresa. Configure el sistema antes de facturar.");
        config.Cufd                    = nuevoCufd;
        config.FechaCufd               = DateTime.Now;
        config.VencimientoCufd         = vencimiento;
        config.FechaUltimaActualizacion = DateTime.Now;
        await ctx.SaveChangesAsync();
>>>>>>> 71335d2 (feat(sprint3): CUF real, PDF+QR, historial facturas, IVA 13%, sidebar colapsable con iconos)
    }

    public async Task<long> ObtenerSiguienteNumeroFacturaAsync()
    {
        await _lockNumeroFactura.WaitAsync();
        try
        {
<<<<<<< HEAD
            // ✓ Un solo db para toda la operación atómica
            await using var db = _factory.CreateDbContext();

            var config = await db.ConfiguracionEmpresa.FirstOrDefaultAsync()
=======
            await using var ctx = await _contextFactory.CreateDbContextAsync();
            var config = await ctx.ConfiguracionEmpresa.FirstOrDefaultAsync()
>>>>>>> 71335d2 (feat(sprint3): CUF real, PDF+QR, historial facturas, IVA 13%, sidebar colapsable con iconos)
                ?? throw new InvalidOperationException(
                    "No existe configuración de empresa. Configure el sistema antes de facturar.");
            config.UltimoNumeroFactura++;
            config.FechaUltimaActualizacion = DateTime.Now;
<<<<<<< HEAD
            await db.SaveChangesAsync();

=======
            await ctx.SaveChangesAsync();
>>>>>>> 71335d2 (feat(sprint3): CUF real, PDF+QR, historial facturas, IVA 13%, sidebar colapsable con iconos)
            return config.UltimoNumeroFactura;
        }
        finally
        {
            _lockNumeroFactura.Release();
        }
    }
}