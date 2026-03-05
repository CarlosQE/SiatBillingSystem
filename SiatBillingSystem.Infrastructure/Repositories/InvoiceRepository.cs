using Microsoft.EntityFrameworkCore;
using SiatBillingSystem.Application.Interfaces;
using SiatBillingSystem.Domain.Entities;
using SiatBillingSystem.Domain.Enums;
using SiatBillingSystem.Infrastructure.Persistence;

namespace SiatBillingSystem.Infrastructure.Repositories;

public class InvoiceRepository : IInvoiceRepository
{
    private readonly SiatDbContext _context;

    public InvoiceRepository(SiatDbContext context)
    {
        _context = context;
    }

    public async Task<int> GuardarAsync(ServiceInvoice factura)
    {
        _context.Facturas.Add(factura);
        await _context.SaveChangesAsync();
        return factura.Id;
    }

    public async Task ActualizarEstadoAsync(int id, EstadoEnvioSin nuevoEstado,
        string? codigoAutorizacion = null, string? motivoRechazo = null)
    {
        var factura = await _context.Facturas.FindAsync(id)
            ?? throw new InvalidOperationException($"Factura con Id {id} no encontrada.");

        factura.EstadoEnvio = nuevoEstado;
        factura.FechaRespuestaSin = DateTime.Now;

        if (codigoAutorizacion is not null)
            factura.CodigoAutorizacion = codigoAutorizacion;

        if (motivoRechazo is not null)
            factura.MotivoRechazo = motivoRechazo;

        await _context.SaveChangesAsync();
    }

    public async Task<List<ServiceInvoice>> ObtenerHistorialAsync(
        DateTime? desde = null,
        DateTime? hasta = null,
        EstadoEnvioSin? estado = null,
        string? numeroDocumentoCliente = null)
    {
        var query = _context.Facturas
            .Include(f => f.Details)
            .Include(f => f.ClienteFrecuente)
            .AsQueryable();

        if (desde.HasValue)
            query = query.Where(f => f.FechaEmision >= desde.Value);

        if (hasta.HasValue)
            query = query.Where(f => f.FechaEmision <= hasta.Value);

        if (estado.HasValue)
            query = query.Where(f => f.EstadoEnvio == estado.Value);

        if (!string.IsNullOrWhiteSpace(numeroDocumentoCliente))
            query = query.Where(f => f.NumeroDocumento == numeroDocumentoCliente);

        return await query
            .OrderByDescending(f => f.FechaEmision)
            .ToListAsync();
    }

    public async Task<List<ServiceInvoice>> ObtenerPendientesEnvioAsync()
    {
        return await _context.Facturas
            .Include(f => f.Details)
            .Where(f => f.EstadoEnvio == EstadoEnvioSin.PendienteEnvio ||
                        f.EstadoEnvio == EstadoEnvioSin.Contingencia)
            .OrderBy(f => f.FechaEmision) // Procesar en orden cronológico (FIFO)
            .ToListAsync();
    }

    public async Task<ServiceInvoice?> ObtenerPorIdAsync(int id)
    {
        return await _context.Facturas
            .Include(f => f.Details)
            .Include(f => f.ClienteFrecuente)
            .FirstOrDefaultAsync(f => f.Id == id);
    }

    public async Task<ServiceInvoice?> ObtenerPorCufAsync(string cuf)
    {
        return await _context.Facturas
            .Include(f => f.Details)
            .FirstOrDefaultAsync(f => f.Cuf == cuf);
    }

    public async Task<long> ObtenerUltimoNumeroFacturaAsync()
    {
        // Si no hay facturas aún, retorna 0
        if (!await _context.Facturas.AnyAsync())
            return 0;

        return await _context.Facturas.MaxAsync(f => f.NumeroFactura);
    }
}
