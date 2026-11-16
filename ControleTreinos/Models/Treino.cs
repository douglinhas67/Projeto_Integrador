namespace ControleTreinos.Models;

public class Treino
{
    public int Id { get; private set; }
    public int UsuarioId { get; private set; }
    public string Modalidade { get; private set; } = string.Empty;
    public DateTime Data { get; private set; }
    public int DuracaoMinutos { get; private set; }

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

        if (data.Year < 1000)
        {
            throw new ArgumentException(
                "A data deve ser a partir de 01/01/1000."
            );
        }

        if (data.Date > DateTime.Today)
        {
            throw new ArgumentException(
                "A data do treino não pode ser futura."
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