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
    public partial class ServicosWindow : Window
    {
        private readonly EspacoFestasContext _contexto;
        private readonly ObservableCollection<Servico> _servicos = new();
        private readonly ObservableCollection<ServicoFaixa> _faixas = new();
        private Servico? _servicoSelecionado;
        private ServicoFaixa? _faixaSelecionada;

        public ServicosWindow()
        {
            InitializeComponent();
            _contexto = App.CriarContexto();
            GridServicos.ItemsSource = _servicos;
            GridFaixas.ItemsSource = _faixas;
            Closed += (_, _) => _contexto.Dispose();
            CarregarServicos();
        }

        private void CarregarServicos()
        {
            _servicos.Clear();
            foreach (var servico in _contexto.Servicos.AsNoTracking().OrderBy(s => s.Nome))
            {
                _servicos.Add(servico);
            }

            LimparFormularioServico();
            CarregarFaixas();
        }

        private void CarregarFaixas()
        {
            _faixas.Clear();
            if (_servicoSelecionado is not null)
            {
                foreach (var faixa in _contexto.ServicoFaixas.AsNoTracking()
                             .Where(f => f.ServicoId == _servicoSelecionado.Id)
                             .OrderBy(f => f.OrdemExibicao))
                {
                    _faixas.Add(faixa);
                }
            }

            LimparFormularioFaixa();
        }

        private void GridServicos_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _servicoSelecionado = GridServicos.SelectedItem as Servico;
            if (_servicoSelecionado is null)
            {
                LimparFormularioServico();
                TxtFaixasTitulo.Text = "Faixas de preço (selecione um serviço)";
            }
            else
            {
                TxtServicoNome.Text = _servicoSelecionado.Nome;
                TxtServicoDescricao.Text = _servicoSelecionado.Descricao;
                BtnServicoAtivarInativar.Content = _servicoSelecionado.Ativo ? "Inativar" : "Ativar";
                TxtFaixasTitulo.Text = $"Faixas de preço — {_servicoSelecionado.Nome}";
            }

            CarregarFaixas();
        }

        private void LimparFormularioServico()
        {
            _servicoSelecionado = null;
            GridServicos.SelectedItem = null;
            TxtServicoNome.Text = string.Empty;
            TxtServicoDescricao.Text = string.Empty;
            BtnServicoAtivarInativar.Content = "Inativar";
            TxtServicoMensagem.Text = string.Empty;
        }

        private void BtnServicoNovo_Click(object sender, RoutedEventArgs e) => LimparFormularioServico();

        private void BtnServicoSalvar_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtServicoNome.Text))
            {
                TxtServicoMensagem.Text = "Nome é obrigatório.";
                return;
            }

            Servico servico;
            if (_servicoSelecionado is null)
            {
                servico = new Servico { Ativo = true };
                _contexto.Servicos.Add(servico);
            }
            else
            {
                servico = _contexto.Servicos.Single(s => s.Id == _servicoSelecionado.Id);
            }

            servico.Nome = TxtServicoNome.Text.Trim();
            servico.Descricao = string.IsNullOrWhiteSpace(TxtServicoDescricao.Text) ? null : TxtServicoDescricao.Text.Trim();

            _contexto.SaveChanges();
            CarregarServicos();
        }

        private void BtnServicoAtivarInativar_Click(object sender, RoutedEventArgs e)
        {
            if (_servicoSelecionado is null)
            {
                TxtServicoMensagem.Text = "Selecione um serviço na lista.";
                return;
            }

            var servico = _contexto.Servicos.Single(s => s.Id == _servicoSelecionado.Id);
            servico.Ativo = !servico.Ativo;
            _contexto.SaveChanges();
            CarregarServicos();
        }

        // Faixas de preço

        private void GridFaixas_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _faixaSelecionada = GridFaixas.SelectedItem as ServicoFaixa;
            if (_faixaSelecionada is null)
            {
                LimparFormularioFaixa();
                return;
            }

            TxtFaixaNome.Text = _faixaSelecionada.Nome;
            TxtFaixaValor.Text = _faixaSelecionada.Valor.ToString(CultureInfo.InvariantCulture);
            TxtFaixaOrdem.Text = _faixaSelecionada.OrdemExibicao.ToString();
            BtnFaixaAtivarInativar.Content = _faixaSelecionada.Ativo ? "Inativar" : "Ativar";
            TxtFaixaMensagem.Text = string.Empty;
        }

        private void LimparFormularioFaixa()
        {
            _faixaSelecionada = null;
            GridFaixas.SelectedItem = null;
            TxtFaixaNome.Text = string.Empty;
            TxtFaixaValor.Text = string.Empty;
            TxtFaixaOrdem.Text = string.Empty;
            BtnFaixaAtivarInativar.Content = "Inativar";
            TxtFaixaMensagem.Text = string.Empty;
        }

        private void BtnFaixaNovo_Click(object sender, RoutedEventArgs e) => LimparFormularioFaixa();

        private void BtnFaixaSalvar_Click(object sender, RoutedEventArgs e)
        {
            if (_servicoSelecionado is null)
            {
                TxtFaixaMensagem.Text = "Selecione um serviço primeiro.";
                return;
            }

            if (string.IsNullOrWhiteSpace(TxtFaixaNome.Text))
            {
                TxtFaixaMensagem.Text = "Nome da faixa é obrigatório.";
                return;
            }

            if (!decimal.TryParse(TxtFaixaValor.Text, NumberStyles.Number, CultureInfo.InvariantCulture, out var valor))
            {
                TxtFaixaMensagem.Text = "Valor inválido.";
                return;
            }

            short ordem = short.TryParse(TxtFaixaOrdem.Text, out var o) ? o : (short)0;

            ServicoFaixa faixa;
            if (_faixaSelecionada is null)
            {
                faixa = new ServicoFaixa { ServicoId = _servicoSelecionado.Id, Ativo = true };
                _contexto.ServicoFaixas.Add(faixa);
            }
            else
            {
                faixa = _contexto.ServicoFaixas.Single(f => f.Id == _faixaSelecionada.Id);
            }

            faixa.Nome = TxtFaixaNome.Text.Trim();
            faixa.Valor = valor;
            faixa.OrdemExibicao = ordem;

            _contexto.SaveChanges();
            CarregarFaixas();
        }

        private void BtnFaixaAtivarInativar_Click(object sender, RoutedEventArgs e)
        {
            if (_faixaSelecionada is null)
            {
                TxtFaixaMensagem.Text = "Selecione uma faixa na lista.";
                return;
            }

            var faixa = _contexto.ServicoFaixas.Single(f => f.Id == _faixaSelecionada.Id);
            faixa.Ativo = !faixa.Ativo;
            _contexto.SaveChanges();
            CarregarFaixas();
        }
    }
}
