using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SiatBillingSystem.Application.Interfaces;
using SiatBillingSystem.Application.Services;       // InvoiceService vive en Application
using SiatBillingSystem.Desktop.ViewModels;
using SiatBillingSystem.Infrastructure.Persistence;
using SiatBillingSystem.Infrastructure.Repositories;
using SiatBillingSystem.Infrastructure.Services;    // SignatureService, QrCodeService, PdfService
using System.Windows;
using WpfApplication = System.Windows.Application;

namespace SiatBillingSystem.Desktop
{
    public partial class App : WpfApplication
    {
        private readonly IHost _host;

        public App()
        {
            _host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    // ── Base de datos ─────────────────────────────────────────────────
                    services.AddDbContextFactory<SiatDbContext>(options =>
                        options.UseSqlite("Data Source=siat.db"));

                    // ── Repositorios (Infrastructure) ─────────────────────────────────
                    services.AddSingleton<IInvoiceRepository,       InvoiceRepository>();
                    services.AddSingleton<IClienteRepository,       ClienteRepository>();
                    services.AddSingleton<IConfiguracionRepository, ConfiguracionRepository>();

                    // ── Servicios de dominio ──────────────────────────────────────────
                    services.AddSingleton<ISignatureService, SignatureService>();   // Infrastructure
                    services.AddSingleton<IInvoiceService,   InvoiceService>();     // Application

                    // ── Servicios de presentación (Infrastructure) ────────────────────
                    services.AddSingleton<IQrCodeService, QrCodeService>();
                    services.AddSingleton<IPdfService,    PdfService>();

                    // ── ViewModels (Transient = instancia fresca al navegar) ───────────
                    services.AddTransient<PosGridViewModel>();
                    services.AddTransient<ClientesViewModel>();
                    services.AddTransient<ConfiguracionViewModel>();
                    services.AddTransient<HistorialViewModel>();

                    // ── Ventana principal (Singleton) ─────────────────────────────────
                    services.AddSingleton<MainWindowViewModel>();
                    services.AddSingleton<MainWindow>();
                })
                .Build();
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            await _host.StartAsync();

            var factory = _host.Services.GetRequiredService<IDbContextFactory<SiatDbContext>>();
            await using var db = await factory.CreateDbContextAsync();
            await db.Database.MigrateAsync();

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
}
