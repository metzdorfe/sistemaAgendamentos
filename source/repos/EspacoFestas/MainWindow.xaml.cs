using System.Windows;
using EspacoFestas.Views;

namespace EspacoFestas
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void AbrirClientes_Click(object sender, RoutedEventArgs e) => new ClientesWindow().Show();

        private void AbrirServicos_Click(object sender, RoutedEventArgs e) => new ServicosWindow().Show();

        private void AbrirAdicionais_Click(object sender, RoutedEventArgs e) => new AdicionaisWindow().Show();

        private void AbrirEspecies_Click(object sender, RoutedEventArgs e) => new EspeciesWindow().Show();
    }
}
