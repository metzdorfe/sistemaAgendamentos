namespace EspacoFestas.Models;

public class Financeiro
{
    public long Id { get; set; }
    public long? ReceberId { get; set; }
    public Receber? Receber { get; set; }
    public int? AgendamentoId { get; set; }
    public Agendamento? Agendamento { get; set; }
    public int EspecieId { get; set; }
    public Especie? Especie { get; set; }
    public TipoLancamento TipoLancamento { get; set; }
    public string? Descricao { get; set; }
    public decimal Valor { get; set; }
    public DateTime DataLancamento { get; set; }
}
