using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using EspacoFestas.Data;
using EspacoFestas.Models;
using Microsoft.EntityFrameworkCore;

namespace EspacoFestas.Views
{
    public partial class ClientesWindow : Window
    {
        private readonly EspacoFestasContext _contexto;
        private readonly ObservableCollection<Cliente> _clientes = new();
        private Cliente? _selecionado;

        public ClientesWindow()
        {
            InitializeComponent();
            _contexto = App.CriarContexto();
            Grid.ItemsSource = _clientes;
            Closed += (_, _) => _contexto.Dispose();
            Carregar();
        }

        private void Carregar()
        {
            var query = _contexto.Clientes.AsNoTracking().OrderBy(c => c.Nome).AsQueryable();
            if (ChkMostrarInativos.IsChecked != true)
            {
                query = query.Where(c => c.Ativo);
            }

            _clientes.Clear();
            foreach (var cliente in query)
            {
                _clientes.Add(cliente);
            }

            LimparFormulario();
        }

        private void ChkMostrarInativos_Changed(object sender, RoutedEventArgs e) => Carregar();

        private void Grid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selecionado = Grid.SelectedItem as Cliente;
            if (_selecionado is null)
            {
                LimparFormulario();
                return;
            }

            TxtNome.Text = _selecionado.Nome;
            TxtEndereco.Text = _selecionado.Endereco;
            TxtCidade.Text = _selecionado.Cidade;
            TxtUf.Text = _selecionado.Uf;
            TxtTelefone.Text = _selecionado.Telefone;
            TxtCpf.Text = _selecionado.Cpf;
            BtnAtivarInativar.Content = _selecionado.Ativo ? "Inativar" : "Ativar";
            TxtMensagem.Text = string.Empty;
        }

        private void LimparFormulario()
        {
            _selecionado = null;
            Grid.SelectedItem = null;
            TxtNome.Text = string.Empty;
            TxtEndereco.Text = string.Empty;
            TxtCidade.Text = string.Empty;
            TxtUf.Text = string.Empty;
            TxtTelefone.Text = string.Empty;
            TxtCpf.Text = string.Empty;
            BtnAtivarInativar.Content = "Inativar";
            TxtMensagem.Text = string.Empty;
        }

        private void BtnNovo_Click(object sender, RoutedEventArgs e) => LimparFormulario();

        private void BtnSalvar_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtNome.Text))
            {
                TxtMensagem.Text = "Nome é obrigatório.";
                return;
            }

            try
            {
                Cliente cliente;
                if (_selecionado is null)
                {
                    cliente = new Cliente { Ativo = true };
                    _contexto.Clientes.Add(cliente);
                }
                else
                {
                    cliente = _contexto.Clientes.Single(c => c.Id == _selecionado.Id);
                }

                cliente.Nome = TxtNome.Text.Trim();
                cliente.Endereco = TextoOuNulo(TxtEndereco.Text);
                cliente.Cidade = TextoOuNulo(TxtCidade.Text);
                cliente.Uf = TextoOuNulo(TxtUf.Text)?.ToUpperInvariant();
                cliente.Telefone = TextoOuNulo(TxtTelefone.Text);
                cliente.Cpf = TextoOuNulo(TxtCpf.Text);

                _contexto.SaveChanges();
                Carregar();
            }
            catch (DbUpdateException)
            {
                TxtMensagem.Text = "Não foi possível salvar. Confira se o CPF já não está em uso por outro cliente (RN01).";
            }
        }

        private void BtnAtivarInativar_Click(object sender, RoutedEventArgs e)
        {
            if (_selecionado is null)
            {
                TxtMensagem.Text = "Selecione um cliente na lista.";
                return;
            }

            var cliente = _contexto.Clientes.Single(c => c.Id == _selecionado.Id);
            cliente.Ativo = !cliente.Ativo;
            _contexto.SaveChanges();
            Carregar();
        }

        private static string? TextoOuNulo(string texto) => string.IsNullOrWhiteSpace(texto) ? null : texto.Trim();
    }
}
