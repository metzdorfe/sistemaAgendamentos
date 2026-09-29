namespace EspacoFestas.Models;

public class Cliente
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Endereco { get; set; }
    public string? Cidade { get; set; }
    public string? Uf { get; set; }
    public string? Telefone { get; set; }
    public string? Cpf { get; set; }
    public bool Ativo { get; set; } = true;

    public ICollection<Agendamento> Agendamentos { get; set; } = new List<Agendamento>();
}
