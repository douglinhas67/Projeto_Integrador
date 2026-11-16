namespace ControleTreinos.Menu;

public class MenuPrincipal
{
    public void Exibir()
    {
        while (true)
        {
            Console.WriteLine();
            Console.WriteLine("=== SISTEMA DE CONTROLE DE TREINOS ===");
            Console.WriteLine("1 - Cadastrar usuário");
            Console.WriteLine("2 - Listar usuários");
            Console.WriteLine("3 - Cadastrar treino");
            Console.WriteLine("4 - Listar todos os treinos");
            Console.WriteLine("5 - Buscar treinos por usuário");
            Console.WriteLine("6 - Editar treino");
            Console.WriteLine("7 - Excluir treino");
            Console.WriteLine("8 - Consultar resumo por usuário");
            Console.WriteLine("0 - Sair");
            Console.Write("Escolha uma opção: ");

            string opcao = Console.ReadLine()?.Trim() ?? "0";

            Console.WriteLine();

            switch (opcao)
            {
                case "0":
                    Console.WriteLine("Programa encerrado.");
                    return;

                case "1":
                case "2":
                case "3":
                case "4":
                case "5":
                case "6":
                case "7":
                case "8":
                    Console.WriteLine(
                        "Esta funcionalidade será implementada nas próximas etapas."
                    );
                    break;

                default:
                    Console.WriteLine(
                        "Opção inválida. Escolha uma opção do menu."
                    );
                    break;
            }
        }
    }
}