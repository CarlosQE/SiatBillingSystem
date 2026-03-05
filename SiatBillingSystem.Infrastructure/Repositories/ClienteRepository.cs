using Microsoft.EntityFrameworkCore;
using SiatBillingSystem.Application.Interfaces;
using SiatBillingSystem.Domain.Entities;
using SiatBillingSystem.Infrastructure.Persistence;

namespace SiatBillingSystem.Infrastructure.Repositories;

public class ClienteRepository : IClienteRepository
{
    private readonly IDbContextFactory<SiatDbContext> _contextFactory;

    public ClienteRepository(IDbContextFactory<SiatDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<List<ClienteFrecuente>> BuscarAsync(string termino)
    {
        if (string.IsNullOrWhiteSpace(termino))
            return new List<ClienteFrecuente>();

        await using var ctx = await _contextFactory.CreateDbContextAsync();
        var terminoLimpio = termino.Trim().ToLower();
        return await ctx.ClientesFrecuentes
            .Where(c =>
                c.NumeroDocumento.ToLower().Contains(terminoLimpio) ||
                c.NombreRazonSocial.ToLower().Contains(terminoLimpio))
            .OrderByDescending(c => c.UltimaFactura)
            .Take(10)
            .ToListAsync();
    }

    public async Task<ClienteFrecuente?> ObtenerPorDocumentoAsync(string numeroDocumento)
    {
        await using var ctx = await _contextFactory.CreateDbContextAsync();
        return await ctx.ClientesFrecuentes
            .FirstOrDefaultAsync(c => c.NumeroDocumento == numeroDocumento.Trim());
    }

    public async Task<int> GuardarAsync(ClienteFrecuente cliente)
    {
        await using var ctx = await _contextFactory.CreateDbContextAsync();
        if (cliente.Id == 0)
            ctx.ClientesFrecuentes.Add(cliente);
        else
            ctx.ClientesFrecuentes.Update(cliente);
        await ctx.SaveChangesAsync();
        return cliente.Id;
    }

    public async Task RegistrarFacturaEmitidaAsync(int clienteId)
    {
        await using var ctx = await _contextFactory.CreateDbContextAsync();
        var cliente = await ctx.ClientesFrecuentes.FindAsync(clienteId);
        if (cliente is null) return;
        cliente.UltimaFactura = DateTime.Now;
        cliente.TotalFacturas++;
        await ctx.SaveChangesAsync();
    }

    public async Task<List<ClienteFrecuente>> ObtenerTodosAsync()
    {
        await using var ctx = await _contextFactory.CreateDbContextAsync();
        return await ctx.ClientesFrecuentes
            .OrderByDescending(c => c.UltimaFactura)
            .ThenBy(c => c.NombreRazonSocial)
            .ToListAsync();
    }
}
