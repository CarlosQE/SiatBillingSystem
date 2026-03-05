using SiatBillingSystem.Desktop.ViewModels;
using System.Windows;

namespace SiatBillingSystem.Desktop
{
    /// <summary>
    /// Ventana principal de NEXUS.
    /// MainWindowViewModel es inyectado por el contenedor DI (Singleton).
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow(MainWindowViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
