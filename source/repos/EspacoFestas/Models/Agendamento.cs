namespace EspacoFestas.Models;

public class Agendamento
{
    public int Id { get; set; }
    public int ClienteId { get; set; }
    public Cliente? Cliente { get; set; }
    public DateTime DataEvento { get; set; }
    public TimeSpan? HoraEvento { get; set; }
    public int? QuantidadeConvidados { get; set; }
    public StatusAgendamento Status { get; set; }
    public decimal ValorTotal { get; set; }
    public decimal ValorDesconto { get; set; }
    public DateTime? DataCancelamento { get; set; }
    public string? MotivoCancelamento { get; set; }

    public ICollection<ServicoAgendamento> Servicos { get; set; } = new List<ServicoAgendamento>();
    public ICollection<AdicionalAgendamento> Adicionais { get; set; } = new List<AdicionalAgendamento>();
    public ICollection<Receber> Parcelas { get; set; } = new List<Receber>();
    public ICollection<Financeiro> Lancamentos { get; set; } = new List<Financeiro>();
}
