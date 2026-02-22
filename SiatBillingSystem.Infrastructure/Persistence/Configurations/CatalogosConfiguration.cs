using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SiatBillingSystem.Domain.Entities;

namespace SiatBillingSystem.Infrastructure.Persistence.Configurations;

public class TipoDocumentoIdentidadConfiguration : IEntityTypeConfiguration<TipoDocumentoIdentidad>
{
    public void Configure(EntityTypeBuilder<TipoDocumentoIdentidad> builder)
    {
        builder.ToTable("TiposDocumentoIdentidad");
        builder.HasKey(t => t.CodigoClasificador);
        builder.Property(t => t.Descripcion).IsRequired().HasMaxLength(100);
    }
}

public class MetodoPagoConfiguration : IEntityTypeConfiguration<MetodoPago>
{
    public void Configure(EntityTypeBuilder<MetodoPago> builder)
    {
        builder.ToTable("MetodosPago");
        builder.HasKey(m => m.CodigoClasificador);
        builder.Property(m => m.Descripcion).IsRequired().HasMaxLength(100);
    }
}

public class UnidadMedidaConfiguration : IEntityTypeConfiguration<UnidadMedida>
{
    public void Configure(EntityTypeBuilder<UnidadMedida> builder)
    {
        builder.ToTable("UnidadesMedida");
        builder.HasKey(u => u.CodigoClasificador);
        builder.Property(u => u.Descripcion).IsRequired().HasMaxLength(100);
    }
}

public class LeyendaFacturaConfiguration : IEntityTypeConfiguration<LeyendaFactura>
{
    public void Configure(EntityTypeBuilder<LeyendaFactura> builder)
    {
        builder.ToTable("LeyendasFactura");
        builder.HasKey(l => l.Id);
        builder.Property(l => l.CodigoActividad).IsRequired().HasMaxLength(20);
        builder.Property(l => l.DescripcionLeyenda).IsRequired().HasMaxLength(500);
        builder.HasIndex(l => l.CodigoActividad);
    }
}

public class TipoMonedaConfiguration : IEntityTypeConfiguration<TipoMoneda>
{
    public void Configure(EntityTypeBuilder<TipoMoneda> builder)
    {
        builder.ToTable("TiposMoneda");
        builder.HasKey(t => t.CodigoClasificador);
        builder.Property(t => t.Descripcion).IsRequired().HasMaxLength(100);
    }
}
