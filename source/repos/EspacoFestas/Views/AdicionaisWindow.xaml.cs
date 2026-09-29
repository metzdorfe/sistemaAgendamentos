using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using EspacoFestas.Data;
using EspacoFestas.Models;
using Microsoft.EntityFrameworkCore;

namespace EspacoFestas.Views
{
    public partial class AdicionaisWindow : Window
    {
        private readonly EspacoFestasContext _contexto;
        private readonly ObservableCollection<Adicional> _adicionais = new();
        private Adicional? _selecionado;

        public AdicionaisWindow()
        {
            InitializeComponent();
            _contexto = App.CriarContexto();
            Grid.ItemsSource = _adicionais;
            Closed += (_, _) => _contexto.Dispose();
            Carregar();
        }

        private void Carregar()
        {
            _adicionais.Clear();
            foreach (var adicional in _contexto.Adicionais.AsNoTracking().OrderBy(a => a.Nome))
            {
                _adicionais.Add(adicional);
            }

            LimparFormulario();
        }

        private void Grid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selecionado = Grid.SelectedItem as Adicional;
            if (_selecionado is null)
            {
                LimparFormulario();
                return;
            }

            TxtNome.Text = _selecionado.Nome;
            TxtDescricao.Text = _selecionado.Descricao;
            TxtValorVenda.Text = _selecionado.ValorVenda.ToString(CultureInfo.InvariantCulture);
            TxtValorCusto.Text = _selecionado.ValorCusto.ToString(CultureInfo.InvariantCulture);
            ChkCobraPorQuantidade.IsChecked = _selecionado.CobraPorQuantidade;
            BtnAtivarInativar.Content = _selecionado.Ativo ? "Inativar" : "Ativar";
            TxtMensagem.Text = string.Empty;
        }

        private void LimparFormulario()
        {
            _selecionado = null;
            Grid.SelectedItem = null;
            TxtNome.Text = string.Empty;
            TxtDescricao.Text = string.Empty;
            TxtValorVenda.Text = string.Empty;
            TxtValorCusto.Text = string.Empty;
            ChkCobraPorQuantidade.IsChecked = false;
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

            if (!decimal.TryParse(TxtValorVenda.Text, NumberStyles.Number, CultureInfo.InvariantCulture, out var valorVenda))
            {
                TxtMensagem.Text = "Valor de venda inválido.";
                return;
            }

            if (!decimal.TryParse(TxtValorCusto.Text, NumberStyles.Number, CultureInfo.InvariantCulture, out var valorCusto))
            {
                TxtMensagem.Text = "Valor de custo inválido.";
                return;
            }

            Adicional adicional;
            if (_selecionado is null)
            {
                adicional = new Adicional { Ativo = true };
                _contexto.Adicionais.Add(adicional);
            }
            else
            {
                adicional = _contexto.Adicionais.Single(a => a.Id == _selecionado.Id);
            }

            adicional.Nome = TxtNome.Text.Trim();
            adicional.Descricao = string.IsNullOrWhiteSpace(TxtDescricao.Text) ? null : TxtDescricao.Text.Trim();
            adicional.ValorVenda = valorVenda;
            adicional.ValorCusto = valorCusto;
            adicional.CobraPorQuantidade = ChkCobraPorQuantidade.IsChecked == true;

            _contexto.SaveChanges();
            Carregar();
        }

        private void BtnAtivarInativar_Click(object sender, RoutedEventArgs e)
        {
            if (_selecionado is null)
            {
                TxtMensagem.Text = "Selecione um adicional na lista.";
                return;
            }

            var adicional = _contexto.Adicionais.Single(a => a.Id == _selecionado.Id);
            adicional.Ativo = !adicional.Ativo;
            _contexto.SaveChanges();
            Carregar();
        }
    }
}
