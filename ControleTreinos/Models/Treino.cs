namespace ControleTreinos.Models;

public class Treino
{
    public int Id { get; private set; }
    public int UsuarioId { get; private set; }
    public string Modalidade { get; private set; } = string.Empty;
    public DateTime Data { get; private set; }
    public int DuracaoMinutos { get; private set; }

    // Usado pelo Entity Framework ao carregar registros do banco.
    private Treino()
    {
    }

    public Treino(
        int usuarioId,
        string modalidade,
        DateTime data,
        int duracaoMinutos)
    {
        if (usuarioId <= 0)
        {
            throw new ArgumentException(
                "O identificador do usuário deve ser maior que zero."
            );
        }

        UsuarioId = usuarioId;

        AtualizarDados(modalidade, data, duracaoMinutos);
    }

    public void AtualizarDados(
        string modalidade,
        DateTime data,
        int duracaoMinutos)
    {
        if (string.IsNullOrWhiteSpace(modalidade))
        {
            throw new ArgumentException("A modalidade é obrigatória.");
        }

        modalidade = modalidade.Trim();

        if (modalidade.Length > 50)
        {
            throw new ArgumentException(
                "A modalidade deve ter no máximo 50 caracteres."
            );
        }

        if (modalidade.Any(char.IsDigit))
        {
            throw new ArgumentException(
                "A modalidade não pode conter números."
            );
        }

        DateTime dataLimite = new DateTime(2026, 12, 14);

        if (data.Year < 1000 || data.Date > dataLimite)
        {
            throw new ArgumentException(
                "A data deve estar entre 01/01/1000 e 14/12/2026."
            );
        }

        if (duracaoMinutos <= 0)
        {
            throw new ArgumentException(
                "A duração deve ser maior que zero."
            );
        }

        Modalidade = modalidade;
        Data = data.Date;
        DuracaoMinutos = duracaoMinutos;
    }
}