namespace EspacoFestas.Models;

public class Receber
{
    public long Id { get; set; }
    public int AgendamentoId { get; set; }
    public Agendamento? Agendamento { get; set; }
    public int EspecieId { get; set; }
    public Especie? Especie { get; set; }
    public short? NumeroParcela { get; set; }
    public decimal ValorParcela { get; set; }
    public DateTime DataVencimento { get; set; }
    public StatusReceber Status { get; set; }
    public decimal ValorJuros { get; set; }
    public decimal ValorMulta { get; set; }
    public decimal ValorRecebido { get; set; }
    public DateTime? DataRecebimento { get; set; }
    public long? ParcelaOrigemId { get; set; }
    public Receber? ParcelaOrigem { get; set; }

    public ICollection<Financeiro> Lancamentos { get; set; } = new List<Financeiro>();
}
