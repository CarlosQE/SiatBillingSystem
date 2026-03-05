using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SiatBillingSystem.Domain.Entities;

namespace SiatBillingSystem.Infrastructure.Persistence.Configurations;

public class ServiceInvoiceConfiguration : IEntityTypeConfiguration<ServiceInvoice>
{
    public void Configure(EntityTypeBuilder<ServiceInvoice> builder)
    {
        builder.ToTable("Facturas");
        builder.HasKey(f => f.Id);

        // ── Campos fiscales críticos ──
        builder.Property(f => f.NitEmisor).IsRequired().HasMaxLength(20);
        builder.Property(f => f.NumeroFactura).IsRequired();
        builder.Property(f => f.Cuf).IsRequired().HasMaxLength(100);
        builder.Property(f => f.Cufd).IsRequired().HasMaxLength(100);

        // ── Índice único en CUF — ninguna factura puede repetir CUF ──
        builder.HasIndex(f => f.Cuf).IsUnique();

        // ── Índice en NumeroFactura para búsquedas rápidas ──
        builder.HasIndex(f => f.NumeroFactura);

        // ── Índice en EstadoEnvio para el worker de sincronización ──
        // El Background Worker filtra constantemente por PendienteEnvio y Contingencia
        builder.HasIndex(f => f.EstadoEnvio);

        // ── Índice en FechaEmision para filtros del historial ──
        builder.HasIndex(f => f.FechaEmision);

        // ── Importes: precisión decimal para cumplimiento fiscal ──
        builder.Property(f => f.MontoTotal).HasPrecision(18, 2).IsRequired();
        builder.Property(f => f.MontoTotalSujetoIva).HasPrecision(18, 2).IsRequired();

        // ── Campos de texto con límites razonables ──
        builder.Property(f => f.NombreRazonSocial).IsRequired().HasMaxLength(250);
        builder.Property(f => f.NumeroDocumento).IsRequired().HasMaxLength(20);
        builder.Property(f => f.Complemento).HasMaxLength(10);
        builder.Property(f => f.Leyenda).HasMaxLength(500);
        builder.Property(f => f.CodigoAutorizacion).HasMaxLength(100);
        builder.Property(f => f.MotivoRechazo).HasMaxLength(1000);

        // ── XML firmado: texto largo (puede ser varios KB) ──
        builder.Property(f => f.XmlFirmado).HasColumnType("TEXT");

        // ── Relación con detalle: cascade delete (borrar factura borra detalles) ──
        builder.HasMany(f => f.Details)
               .WithOne(d => d.ServiceInvoice)
               .HasForeignKey(d => d.ServiceInvoiceId)
               .OnDelete(DeleteBehavior.Cascade);

        // ── Relación con cliente frecuente: opcional ──
        builder.HasOne(f => f.ClienteFrecuente)
               .WithMany(c => c.Facturas)
               .HasForeignKey(f => f.ClienteFrecuenteId)
               .OnDelete(DeleteBehavior.SetNull);
    }
}

public class ServiceInvoiceDetailConfiguration : IEntityTypeConfiguration<ServiceInvoiceDetail>
{
    public void Configure(EntityTypeBuilder<ServiceInvoiceDetail> builder)
    {
        builder.ToTable("DetallesFactura");
        builder.HasKey(d => d.Id);

        builder.Property(d => d.ActividadEconomica).IsRequired().HasMaxLength(20);
        builder.Property(d => d.CodigoProducto).IsRequired().HasMaxLength(50);

        // Descripciones extensas — sector servicios puede necesitar párrafos completos
        builder.Property(d => d.Descripcion).IsRequired().HasMaxLength(2000);

        builder.Property(d => d.Cantidad).HasPrecision(18, 4);
        builder.Property(d => d.PrecioUnitario).HasPrecision(18, 2);
        builder.Property(d => d.SubTotal).HasPrecision(18, 2);
    }
}
