using Microsoft.EntityFrameworkCore;
using SiatBillingSystem.Application.Interfaces;
using SiatBillingSystem.Domain.Entities;
using SiatBillingSystem.Domain.Enums;
using SiatBillingSystem.Infrastructure.Persistence;

namespace SiatBillingSystem.Infrastructure.Repositories;

/// <summary>
/// Implementación de IInvoiceRepository usando EF Core + SQLite.
/// Usa IDbContextFactory para ser thread-safe en WPF (sin Scoped lifetime).
/// </summary>
public class InvoiceRepository : IInvoiceRepository
{
    private readonly IDbContextFactory<SiatDbContext> _contextFactory;

    public InvoiceRepository(IDbContextFactory<SiatDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // ESCRITURA
    // ─────────────────────────────────────────────────────────────────────────

    public async Task<int> GuardarAsync(ServiceInvoice factura)
    {
        await using var ctx = await _contextFactory.CreateDbContextAsync();
        ctx.Facturas.Add(factura);
        await ctx.SaveChangesAsync();
        return factura.Id;
    }

    public async Task ActualizarEstadoAsync(
        int id,
        EstadoEnvioSin nuevoEstado,
        string? codigoAutorizacion = null,
        string? motivoRechazo = null)
    {
        await using var ctx = await _contextFactory.CreateDbContextAsync();
        var factura = await ctx.Facturas.FindAsync(id);
        if (factura is null) return;

        factura.EstadoEnvio          = nuevoEstado;
        factura.CodigoAutorizacion   = codigoAutorizacion ?? factura.CodigoAutorizacion;
        factura.MotivoRechazo        = motivoRechazo      ?? factura.MotivoRechazo;
        factura.FechaRespuestaSin    = DateTime.Now;

        await ctx.SaveChangesAsync();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // LECTURA
    // ─────────────────────────────────────────────────────────────────────────

    public async Task<ServiceInvoice?> ObtenerPorIdAsync(int id)
    {
        await using var ctx = await _contextFactory.CreateDbContextAsync();
        return await ctx.Facturas
            .Include(f => f.Details)
            .FirstOrDefaultAsync(f => f.Id == id);
    }

    public async Task<ServiceInvoice?> ObtenerPorCufAsync(string cuf)
    {
        await using var ctx = await _contextFactory.CreateDbContextAsync();
        return await ctx.Facturas
            .Include(f => f.Details)
            .FirstOrDefaultAsync(f => f.Cuf == cuf);
    }

    public async Task<List<ServiceInvoice>> ObtenerHistorialAsync(
        DateTime? desde = null,
        DateTime? hasta = null,
        EstadoEnvioSin? estado = null,
        string? numeroDocumentoCliente = null)
    {
        await using var ctx = await _contextFactory.CreateDbContextAsync();

        var query = ctx.Facturas.Include(f => f.Details).AsQueryable();

        if (desde.HasValue)
            query = query.Where(f => f.FechaEmision >= desde.Value);

        if (hasta.HasValue)
            query = query.Where(f => f.FechaEmision <= hasta.Value);

        if (estado.HasValue)
            query = query.Where(f => f.EstadoEnvio == estado.Value);

        if (!string.IsNullOrWhiteSpace(numeroDocumentoCliente))
            query = query.Where(f => f.NumeroDocumento.Contains(numeroDocumentoCliente));

        return await query.OrderByDescending(f => f.FechaEmision).ToListAsync();
    }

    public async Task<List<ServiceInvoice>> ObtenerPendientesEnvioAsync()
    {
        await using var ctx = await _contextFactory.CreateDbContextAsync();
        return await ctx.Facturas
            .Include(f => f.Details)
            .Where(f => f.EstadoEnvio == EstadoEnvioSin.PendienteEnvio)
            .OrderBy(f => f.FechaEmision)
            .ToListAsync();
    }

    public async Task<long> ObtenerUltimoNumeroFacturaAsync()
    {
        await using var ctx = await _contextFactory.CreateDbContextAsync();
        if (!await ctx.Facturas.AnyAsync())
            return 0;
        return await ctx.Facturas.MaxAsync(f => f.NumeroFactura);
    }
}
