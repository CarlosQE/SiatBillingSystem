using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SiatBillingSystem.Application.Interfaces;
using SiatBillingSystem.Application.Services;
using SiatBillingSystem.Desktop.ViewModels;
using SiatBillingSystem.Infrastructure.Persistence;
using SiatBillingSystem.Infrastructure.Repositories;
using SiatBillingSystem.Infrastructure.Services;
using System.IO;
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
                .ConfigureServices(RegisterServices)
                .Build();
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            await _host.StartAsync();
            await MigrateDatabase();

            _host.Services.GetRequiredService<MainWindow>().Show();
            base.OnStartup(e);
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            await _host.StopAsync();
            _host.Dispose();
            base.OnExit(e);
        }

        private static void RegisterServices(HostBuilderContext _, IServiceCollection services)
        {
            services.AddDbContextFactory<SiatDbContext>(options =>
            {
                var dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "siat.db");
                options.UseSqlite($"Data Source={dbPath}");
            });

            services.AddTransient<ISignatureService,        SignatureService>();
            services.AddTransient<IInvoiceRepository,       InvoiceRepository>();
            services.AddTransient<IClienteRepository,       ClienteRepository>();
            services.AddTransient<IConfiguracionRepository, ConfiguracionRepository>();
            services.AddTransient<IInvoiceService,          InvoiceService>();

            services.AddTransient<PosGridViewModel>();
            services.AddTransient<ClientesViewModel>();
            services.AddTransient<ConfiguracionViewModel>();
            services.AddTransient<HistorialViewModel>();

            services.AddSingleton<MainWindowViewModel>();
            services.AddSingleton<MainWindow>();
        }

        private async Task MigrateDatabase()
        {
            var factory = _host.Services.GetRequiredService<IDbContextFactory<SiatDbContext>>();
            await using var db = await factory.CreateDbContextAsync();
            await db.Database.MigrateAsync();
        }
    }
}