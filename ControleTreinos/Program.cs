using ControleTreinos.Data;
using ControleTreinos.Menu;
using Microsoft.EntityFrameworkCore;

Console.WriteLine("Sistema de Controle de Treinos");

try
{
    using AppDbContext banco = new AppDbContext();

    banco.Database.OpenConnection();

    Console.WriteLine(
        "Conexão com o MySQL realizada com sucesso!"
    );

    banco.Database.CloseConnection();
}
catch (Exception erro)
{
    Console.WriteLine(
        $"Erro ao conectar: {erro.GetBaseException().Message}"
    );

    return;
}

MenuPrincipal menu = new MenuPrincipal();
menu.Exibir();