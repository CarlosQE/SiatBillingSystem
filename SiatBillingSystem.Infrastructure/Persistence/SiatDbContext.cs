using Microsoft.EntityFrameworkCore;
using SiatBillingSystem.Domain.Entities;

namespace SiatBillingSystem.Infrastructure.Persistence;

public class SiatDbContext : DbContext
{
    public SiatDbContext(DbContextOptions<SiatDbContext> options) : base(options) { }

    // ── Tablas principales ──
    public DbSet<ServiceInvoice> Facturas => Set<ServiceInvoice>();
    public DbSet<ServiceInvoiceDetail> DetallesFactura => Set<ServiceInvoiceDetail>();
    public DbSet<ClienteFrecuente> ClientesFrecuentes => Set<ClienteFrecuente>();
    public DbSet<ConfiguracionEmpresa> ConfiguracionEmpresa => Set<ConfiguracionEmpresa>();

    // ── Catálogos del SIN ──
    public DbSet<TipoDocumentoIdentidad> TiposDocumentoIdentidad => Set<TipoDocumentoIdentidad>();
    public DbSet<MetodoPago> MetodosPago => Set<MetodoPago>();
    public DbSet<UnidadMedida> UnidadesMedida => Set<UnidadMedida>();
    public DbSet<LeyendaFactura> LeyendasFactura => Set<LeyendaFactura>();
    public DbSet<TipoMoneda> TiposMoneda => Set<TipoMoneda>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SiatDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
