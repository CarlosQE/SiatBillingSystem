using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SiatBillingSystem.Application.Interfaces;
using SiatBillingSystem.Application.Services;
using SiatBillingSystem.Desktop.ViewModels;
using SiatBillingSystem.Desktop.Views;
using SiatBillingSystem.Infrastructure.Persistence;
using SiatBillingSystem.Infrastructure.Repositories;
using SiatBillingSystem.Infrastructure.Services;
using System.Windows;
using WpfApplication = System.Windows.Application;
namespace SiatBillingSystem.Desktop;

public partial class App : WpfApplication
{
    private readonly IHost _host;

    public App()
    {
        _host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                // ── Base de datos ──
                services.AddDbContext<SiatDbContext>(options =>
                    options.UseSqlite("Data Source=siat.db"));

                // ── Repositorios ──
                services.AddScoped<IInvoiceRepository, InvoiceRepository>();
                services.AddScoped<IClienteRepository, ClienteRepository>();
                services.AddScoped<IConfiguracionRepository, ConfiguracionRepository>();

                // ── Servicios ──
                services.AddScoped<ISignatureService, SignatureService>();
                services.AddScoped<IInvoiceService, InvoiceService>();

                // ── ViewModels ──
                services.AddTransient<PosGridViewModel>();
                services.AddTransient<ClientesViewModel>();
                services.AddTransient<ConfiguracionViewModel>();
                services.AddSingleton<MainWindowViewModel>();

                // ── Ventana principal ──
                services.AddSingleton<MainWindow>();
            })
            .Build();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        await _host.StartAsync();

        // Aplicar migraciones automáticamente al iniciar
        using var scope = _host.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SiatDbContext>();
        await db.Database.MigrateAsync();

        // Mostrar ventana principal
        var mainWindow = _host.Services.GetRequiredService<MainWindow>();
        mainWindow.Show();

        base.OnStartup(e);
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        await _host.StopAsync();
        _host.Dispose();
        base.OnExit(e);
    }
}