using System.Globalization;
using ControleTreinos.Data;
using ControleTreinos.Models;

namespace ControleTreinos.Menu;

public class MenuPrincipal
{
    private readonly UsuarioRepositorio usuarioRepositorio =
        new UsuarioRepositorio();

    private readonly TreinoRepositorio treinoRepositorio =
        new TreinoRepositorio();

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

            try
            {
                switch (opcao)
                {
                    case "0":
                        Console.WriteLine("Programa encerrado.");
                        return;

                    case "1":
                        CadastrarUsuario();
                        break;

                    case "2":
                        ListarUsuarios();
                        break;

                    case "3":
                        CadastrarTreino();
                        break;

                    case "4":
                        ListarTreinos();
                        break;

                    case "5":
                        BuscarTreinosPorUsuario();
                        break;

                    case "6":
                        EditarTreino();
                        break;

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
            catch (ArgumentException erro)
            {
                Console.WriteLine($"Dados inválidos: {erro.Message}");
            }
            catch (Exception erro)
            {
                Console.WriteLine(
                    "Não foi possível concluir a operação: " +
                    erro.GetBaseException().Message
                );
            }
        }
    }

    private void CadastrarUsuario()
    {
        Console.WriteLine("=== CADASTRAR USUÁRIO ===");

        Console.Write("Nome: ");
        string nome = (Console.ReadLine() ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(nome))
        {
            Console.WriteLine("O nome é obrigatório.");
            return;
        }

        if (nome.Length > 100)
        {
            Console.WriteLine(
                "O nome deve ter no máximo 100 caracteres."
            );
            return;
        }

        Console.Write("Idade: ");

        if (!int.TryParse(Console.ReadLine(), out int idade)
            || idade <= 0)
        {
            Console.WriteLine(
                "A idade deve ser um número inteiro maior que zero."
            );
            return;
        }

        Usuario usuario = new Usuario(nome, idade);

        usuarioRepositorio.Cadastrar(usuario);

        Console.WriteLine(
            $"Usuário cadastrado com sucesso! Identificador: {usuario.Id}"
        );
    }

    private void ListarUsuarios()
    {
        Console.WriteLine("=== USUÁRIOS CADASTRADOS ===");

        List<Usuario> usuarios = usuarioRepositorio.Listar();

        if (usuarios.Count == 0)
        {
            Console.WriteLine("Nenhum usuário cadastrado.");
            return;
        }

        foreach (Usuario usuario in usuarios)
        {
            Console.WriteLine(
                $"ID: {usuario.Id} | Nome: {usuario.Nome} | " +
                $"Idade: {usuario.Idade} anos"
            );
        }
    }

    private void CadastrarTreino()
    {
        Console.WriteLine("=== CADASTRAR TREINO ===");
        Console.Write("Identificador do usuário: ");

        if (!int.TryParse(Console.ReadLine(), out int usuarioId)
            || usuarioId <= 0)
        {
            Console.WriteLine(
                "O identificador deve ser um número inteiro maior que zero."
            );
            return;
        }

        if (!treinoRepositorio.UsuarioExiste(usuarioId))
        {
            Console.WriteLine(
                "Usuário não encontrado. Use a opção 2 para consultar os IDs."
            );
            return;
        }

        if (!LerDadosTreino(
                out string modalidade,
                out DateTime data,
                out int duracaoMinutos))
        {
            return;
        }

        Treino treino = new Treino(
            usuarioId,
            modalidade,
            data,
            duracaoMinutos
        );

        treinoRepositorio.Cadastrar(treino);

        Console.WriteLine(
            $"Treino cadastrado com sucesso! Identificador: {treino.Id}"
        );
    }

    private void ListarTreinos()
    {
        Console.WriteLine("=== TREINOS CADASTRADOS ===");

        List<Treino> treinos = treinoRepositorio.Listar();

        if (treinos.Count == 0)
        {
            Console.WriteLine("Nenhum treino cadastrado.");
            return;
        }

        MostrarTreinos(treinos);
    }

    private void BuscarTreinosPorUsuario()
    {
        Console.WriteLine("=== BUSCAR TREINOS POR USUÁRIO ===");
        Console.Write("Identificador do usuário: ");

        if (!int.TryParse(Console.ReadLine(), out int usuarioId)
            || usuarioId <= 0)
        {
            Console.WriteLine(
                "O identificador deve ser um número inteiro maior que zero."
            );
            return;
        }

        if (!treinoRepositorio.UsuarioExiste(usuarioId))
        {
            Console.WriteLine("Usuário não encontrado.");
            return;
        }

        List<Treino> treinos =
            treinoRepositorio.BuscarPorUsuario(usuarioId);

        if (treinos.Count == 0)
        {
            Console.WriteLine(
                "Nenhum treino encontrado para este usuário."
            );
            return;
        }

        MostrarTreinos(treinos);
    }

    private void EditarTreino()
    {
        Console.WriteLine("=== EDITAR TREINO ===");
        Console.Write("Identificador do treino: ");

        if (!int.TryParse(Console.ReadLine(), out int treinoId)
            || treinoId <= 0)
        {
            Console.WriteLine(
                "O identificador deve ser um número inteiro maior que zero."
            );
            return;
        }

        Treino? treino = treinoRepositorio.BuscarPorId(treinoId);

        if (treino == null)
        {
            Console.WriteLine("Treino não encontrado.");
            return;
        }

        Console.WriteLine("Dados atuais:");
        MostrarTreinos(new List<Treino> { treino });

        Console.WriteLine("Informe os novos dados:");

        if (!LerDadosTreino(
                out string modalidade,
                out DateTime data,
                out int duracaoMinutos))
        {
            return;
        }

        bool atualizado = treinoRepositorio.Atualizar(
            treinoId,
            modalidade,
            data,
            duracaoMinutos
        );

        if (!atualizado)
        {
            Console.WriteLine("Treino não encontrado.");
            return;
        }

        Console.WriteLine("Treino atualizado com sucesso!");
    }

    private bool LerDadosTreino(
        out string modalidade,
        out DateTime data,
        out int duracaoMinutos)
    {
        modalidade = string.Empty;
        data = DateTime.MinValue;
        duracaoMinutos = 0;

        Console.Write("Modalidade: ");
        modalidade = (Console.ReadLine() ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(modalidade))
        {
            Console.WriteLine("A modalidade é obrigatória.");
            return false;
        }

        if (modalidade.Length > 50)
        {
            Console.WriteLine(
                "A modalidade deve ter no máximo 50 caracteres."
            );
            return false;
        }

        if (modalidade.Any(char.IsDigit))
        {
            Console.WriteLine(
                "A modalidade não pode conter números."
            );
            return false;
        }

        Console.Write("Data do treino (dd/MM/aaaa): ");

        string textoData =
            (Console.ReadLine() ?? string.Empty).Trim();

        DateTime dataLimite = new DateTime(2026, 12, 14);

        if (!DateTime.TryParseExact(
                textoData,
                "dd/MM/yyyy",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out data)
            || data.Year < 1000
            || data.Date > dataLimite)
        {
            Console.WriteLine(
                "Informe uma data válida entre 01/01/1000 e 14/12/2026, " +
                "no formato dd/MM/aaaa."
            );
            return false;
        }

        Console.Write("Duração em minutos: ");

        if (!int.TryParse(Console.ReadLine(), out duracaoMinutos)
            || duracaoMinutos <= 0)
        {
            Console.WriteLine(
                "A duração deve ser um número inteiro maior que zero."
            );
            return false;
        }

        return true;
    }

    private void MostrarTreinos(List<Treino> treinos)
    {
        List<Usuario> usuarios = usuarioRepositorio.Listar();

        foreach (Treino treino in treinos)
        {
            Usuario? usuario = usuarios.FirstOrDefault(
                u => u.Id == treino.UsuarioId
            );

            string nomeUsuario =
                usuario?.Nome ?? "Usuário não encontrado";

            Console.WriteLine(
                $"Treino #{treino.Id} | Usuário: {nomeUsuario} " +
                $"(ID: {treino.UsuarioId})"
            );

            Console.WriteLine(
                $"Modalidade: {treino.Modalidade} | " +
                $"Data: {treino.Data:dd/MM/yyyy} | " +
                $"Duração: {treino.DuracaoMinutos} minutos"
            );

            Console.WriteLine();
        }
    }
}