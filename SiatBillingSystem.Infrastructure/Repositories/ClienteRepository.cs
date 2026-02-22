using Microsoft.EntityFrameworkCore;
using SiatBillingSystem.Application.Interfaces;
using SiatBillingSystem.Domain.Entities;
using SiatBillingSystem.Infrastructure.Persistence;

namespace SiatBillingSystem.Infrastructure.Repositories;

public class ClienteRepository : IClienteRepository
{
    private readonly SiatDbContext _context;

    public ClienteRepository(SiatDbContext context)
    {
        _context = context;
    }

    public async Task<List<ClienteFrecuente>> BuscarAsync(string termino)
    {
        if (string.IsNullOrWhiteSpace(termino))
            return new List<ClienteFrecuente>();

        var terminoLimpio = termino.Trim().ToLower();

        return await _context.ClientesFrecuentes
            .Where(c =>
                c.NumeroDocumento.ToLower().Contains(terminoLimpio) ||
                c.NombreRazonSocial.ToLower().Contains(terminoLimpio))
            .OrderByDescending(c => c.UltimaFactura) // Más recientes primero — relevancia en POS
            .Take(10) // Máximo 10 sugerencias para no saturar el autocompletado
            .ToListAsync();
    }

    public async Task<ClienteFrecuente?> ObtenerPorDocumentoAsync(string numeroDocumento)
    {
        return await _context.ClientesFrecuentes
            .FirstOrDefaultAsync(c => c.NumeroDocumento == numeroDocumento.Trim());
    }

    public async Task<int> GuardarAsync(ClienteFrecuente cliente)
    {
        if (cliente.Id == 0)
            _context.ClientesFrecuentes.Add(cliente);
        else
            _context.ClientesFrecuentes.Update(cliente);

        await _context.SaveChangesAsync();
        return cliente.Id;
    }

    public async Task RegistrarFacturaEmitidaAsync(int clienteId)
    {
        var cliente = await _context.ClientesFrecuentes.FindAsync(clienteId);
        if (cliente is null) return;

        cliente.UltimaFactura = DateTime.Now;
        cliente.TotalFacturas++;
        await _context.SaveChangesAsync();
    }

    public async Task<List<ClienteFrecuente>> ObtenerTodosAsync()
    {
        return await _context.ClientesFrecuentes
            .OrderByDescending(c => c.UltimaFactura)
            .ThenBy(c => c.NombreRazonSocial)
            .ToListAsync();
    }
}
