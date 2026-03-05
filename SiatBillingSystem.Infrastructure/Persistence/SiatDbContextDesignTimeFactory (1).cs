using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace SiatBillingSystem.Infrastructure.Persistence;

/// <summary>
/// Factory usada exclusivamente por las herramientas de EF Core en tiempo de diseño
/// (dotnet ef migrations add, dotnet ef database update).
/// NO se usa en tiempo de ejecución — solo permite que las migraciones funcionen
/// sin necesitar levantar el host completo de la aplicación.
/// </summary>
public class SiatDbContextDesignTimeFactory : IDesignTimeDbContextFactory<SiatDbContext>
{
    public SiatDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<SiatDbContext>();
        optionsBuilder.UseSqlite("Data Source=siat.db");
        return new SiatDbContext(optionsBuilder.Options);
    }
}
