using System.Windows;
using SiatBillingSystem.Desktop.ViewModels;

namespace SiatBillingSystem.Desktop;

public partial class MainWindow : Window
{
    public MainWindow(MainWindowViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}