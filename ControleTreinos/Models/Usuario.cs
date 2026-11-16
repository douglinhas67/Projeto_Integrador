namespace ControleTreinos.Models;

public class Usuario
{
    public int Id { get; private set; }
    public string Nome { get; private set; }
    public int Idade { get; private set; }

    public Usuario(string nome, int idade)
    {
        if (string.IsNullOrWhiteSpace(nome))
        {
            throw new ArgumentException("O nome é obrigatório.");
        }

        nome = nome.Trim();

        if (nome.Length > 100)
        {
            throw new ArgumentException(
                "O nome deve ter no máximo 100 caracteres."
            );
        }

        if (idade <= 0)
        {
            throw new ArgumentException(
                "A idade deve ser maior que zero."
            );
        }

        Nome = nome;
        Idade = idade;
    }
}