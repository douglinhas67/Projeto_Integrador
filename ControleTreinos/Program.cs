using ControleTreinos.Data;

Console.WriteLine("Sistema de Controle de Treinos");

try
{
    using AppDbContext banco = new AppDbContext();

    if (banco.Database.CanConnect())
    {
        Console.WriteLine("Conexão com o MySQL realizada com sucesso!");
    }
    else
    {
        Console.WriteLine(
            "Não foi possível conectar. Confira o MySQL e as credenciais."
        );
    }
}
catch (Exception erro)
{
    Console.WriteLine($"Erro ao iniciar: {erro.Message}");
}
