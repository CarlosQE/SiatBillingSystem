using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SiatBillingSystem.Desktop.ViewModels;
using SiatBillingSystem.Desktop.Views;
using SiatBillingSystem.Infrastructure.Persistence;
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
                    // DbContext factory — permite crear instancias bajo demanda desde ViewModels
                    services.AddDbContextFactory<SiatDbContext>(options =>
                        options.UseSqlite("Data Source=siat.db"));

                    // ViewModels y ventana principal
                    services.AddSingleton<MainWindowViewModel>();
                    services.AddSingleton<MainWindow>();
                })
                .Build();
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            await _host.StartAsync();

            // Migraciones automáticas
            var factory = _host.Services.GetRequiredService<IDbContextFactory<SiatDbContext>>();
            using var db = await factory.CreateDbContextAsync();
            await db.Database.MigrateAsync();

            // Pasar factory al MainWindowViewModel antes de mostrar
            var vm = _host.Services.GetRequiredService<MainWindowViewModel>();
            vm.SetDbFactory(factory);

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