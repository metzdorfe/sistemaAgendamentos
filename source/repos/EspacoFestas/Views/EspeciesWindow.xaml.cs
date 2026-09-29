using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using EspacoFestas.Data;
using EspacoFestas.Models;
using Microsoft.EntityFrameworkCore;

namespace EspacoFestas.Views
{
    public partial class EspeciesWindow : Window
    {
        private readonly EspacoFestasContext _contexto;
        private readonly ObservableCollection<Especie> _especies = new();
        private Especie? _selecionado;

        public EspeciesWindow()
        {
            InitializeComponent();
            _contexto = App.CriarContexto();
            Grid.ItemsSource = _especies;
            Closed += (_, _) => _contexto.Dispose();
            Carregar();
        }

        private void Carregar()
        {
            _especies.Clear();
            foreach (var especie in _contexto.Especies.AsNoTracking().OrderBy(e => e.Descricao))
            {
                _especies.Add(especie);
            }

            LimparFormulario();
        }

        private void Grid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selecionado = Grid.SelectedItem as Especie;
            if (_selecionado is null)
            {
                LimparFormulario();
                return;
            }

            TxtDescricao.Text = _selecionado.Descricao;
            ChkPermiteParcelamento.IsChecked = _selecionado.PermiteParcelamento;
            BtnAtivarInativar.Content = _selecionado.Ativo ? "Inativar" : "Ativar";
            TxtMensagem.Text = string.Empty;
        }

        private void LimparFormulario()
        {
            _selecionado = null;
            Grid.SelectedItem = null;
            TxtDescricao.Text = string.Empty;
            ChkPermiteParcelamento.IsChecked = false;
            BtnAtivarInativar.Content = "Inativar";
            TxtMensagem.Text = string.Empty;
        }

        private void BtnNovo_Click(object sender, RoutedEventArgs e) => LimparFormulario();

        private void BtnSalvar_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtDescricao.Text))
            {
                TxtMensagem.Text = "Descrição é obrigatória.";
                return;
            }

            Especie especie;
            if (_selecionado is null)
            {
                especie = new Especie { Ativo = true };
                _contexto.Especies.Add(especie);
            }
            else
            {
                especie = _contexto.Especies.Single(e => e.Id == _selecionado.Id);
            }

            especie.Descricao = TxtDescricao.Text.Trim();
            especie.PermiteParcelamento = ChkPermiteParcelamento.IsChecked == true;

            _contexto.SaveChanges();
            Carregar();
        }

        private void BtnAtivarInativar_Click(object sender, RoutedEventArgs e)
        {
            if (_selecionado is null)
            {
                TxtMensagem.Text = "Selecione uma espécie na lista.";
                return;
            }

            var especie = _contexto.Especies.Single(e => e.Id == _selecionado.Id);
            especie.Ativo = !especie.Ativo;
            _contexto.SaveChanges();
            Carregar();
        }
    }
}
