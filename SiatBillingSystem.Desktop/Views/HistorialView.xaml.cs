using SiatBillingSystem.Desktop.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace SiatBillingSystem.Desktop.Views
{
    public partial class HistorialView : UserControl
    {
        public HistorialView()
        {
            InitializeComponent();
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is HistorialViewModel vm)
                await vm.InicializarAsync();
        }
    }
}
