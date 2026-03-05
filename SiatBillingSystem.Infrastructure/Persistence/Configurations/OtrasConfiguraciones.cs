using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SiatBillingSystem.Domain.Entities;

namespace SiatBillingSystem.Infrastructure.Persistence.Configurations;

public class ClienteFrecuenteConfiguration : IEntityTypeConfiguration<ClienteFrecuente>
{
    public void Configure(EntityTypeBuilder<ClienteFrecuente> builder)
    {
        builder.ToTable("ClientesFrecuentes");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.NumeroDocumento).IsRequired().HasMaxLength(20);
        builder.Property(c => c.NombreRazonSocial).IsRequired().HasMaxLength(250);
        builder.Property(c => c.Complemento).HasMaxLength(10);
        builder.Property(c => c.Telefono).HasMaxLength(20);
        builder.Property(c => c.Email).HasMaxLength(100);

        // Índice en NumeroDocumento para búsqueda rápida desde la grilla POS
        builder.HasIndex(c => c.NumeroDocumento);

        // Índice en NombreRazonSocial para búsqueda por nombre
        builder.HasIndex(c => c.NombreRazonSocial);
    }
}

public class ConfiguracionEmpresaConfiguration : IEntityTypeConfiguration<ConfiguracionEmpresa>
{
    public void Configure(EntityTypeBuilder<ConfiguracionEmpresa> builder)
    {
        builder.ToTable("ConfiguracionEmpresa");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Nit).IsRequired().HasMaxLength(20);
        builder.Property(c => c.RazonSocial).IsRequired().HasMaxLength(250);
        builder.Property(c => c.ActividadEconomica).IsRequired().HasMaxLength(20);
        builder.Property(c => c.LeyendaLey453).HasMaxLength(500);

        // Ruta del certificado: ruta larga posible en Windows
        builder.Property(c => c.RutaCertificado).HasMaxLength(500);

        // Contraseña cifrada con DPAPI — se almacena como Base64
        builder.Property(c => c.PasswordCertificadoCifrado).HasMaxLength(500);

        // CUFD
        builder.Property(c => c.Cufd).HasMaxLength(100);

        // Propiedades calculadas — no se persisten en BD
        builder.Ignore(c => c.CufdProximoAVencer);
        builder.Ignore(c => c.CufdVencido);
    }
}
