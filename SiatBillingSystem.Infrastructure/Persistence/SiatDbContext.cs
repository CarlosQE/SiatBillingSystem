using Microsoft.EntityFrameworkCore;
using SiatBillingSystem.Domain.Entities;
using SiatBillingSystem.Infrastructure.Persistence.Configurations;

namespace SiatBillingSystem.Infrastructure.Persistence;

/// <summary>
/// DbContext principal del sistema. Usa SQLite para instalaciones On-Premise.
/// La arquitectura Offline-First requiere que TODOS los datos se persistan
/// localmente antes de cualquier intento de envío al SIN.
/// </summary>
public class SiatDbContext : DbContext
{
    public SiatDbContext(DbContextOptions<SiatDbContext> options) : base(options) { }

    // ── Tablas principales ──
    public DbSet<ServiceInvoice> Facturas => Set<ServiceInvoice>();
    public DbSet<ServiceInvoiceDetail> DetallesFactura => Set<ServiceInvoiceDetail>();
    public DbSet<ClienteFrecuente> ClientesFrecuentes => Set<ClienteFrecuente>();
    public DbSet<ConfiguracionEmpresa> ConfiguracionEmpresa => Set<ConfiguracionEmpresa>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Aplicar todas las configuraciones Fluent API desde su carpeta
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SiatDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
