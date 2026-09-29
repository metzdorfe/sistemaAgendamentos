namespace EspacoFestas.Models;

public class Configuracao
{
    public int Id { get; set; } = 1;
    public bool BloqueioUmPorDia { get; set; }
    public decimal PercentualJurosMes { get; set; }
    public decimal PercentualMulta { get; set; }
    public int PrazoCancelamentoDias { get; set; }
}
