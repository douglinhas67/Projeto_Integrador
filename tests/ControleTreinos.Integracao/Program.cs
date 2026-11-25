using ControleTreinos.Data;
using ControleTreinos.Models;
using ControleTreinos.Menu;
using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;

// Execute somente em um banco de testes. O programa nunca lê a conexão de produção.
string? conexaoTeste = Environment.GetEnvironmentVariable("CONTROLE_TREINOS_TESTE_CONEXAO");
if (string.IsNullOrWhiteSpace(conexaoTeste))
{
    Console.Error.WriteLine("Configure CONTROLE_TREINOS_TESTE_CONEXAO para um banco separado, com nome terminado em _testes.");
    return 2;
}

var configuracao = new MySqlConnectionStringBuilder(conexaoTeste);
if (!configuracao.Database.EndsWith("_testes", StringComparison.OrdinalIgnoreCase))
{
    Console.Error.WriteLine("O nome do banco de integração deve terminar em _testes.");
    return 2;
}

Environment.SetEnvironmentVariable("CONTROLE_TREINOS_CONEXAO", conexaoTeste);
var usuarios = new UsuarioRepositorio();
var treinos = new TreinoRepositorio();
var criados = new List<int>();
int verificacoes = 0;

void Conferir(bool condicao, string descricao)
{
    if (!condicao) throw new InvalidOperationException("Falhou: " + descricao);
    verificacoes++;
    Console.WriteLine("OK: " + descricao);
}

string ExecutarMenu(string entrada)
{
    TextReader leitorOriginal = Console.In;
    TextWriter escritorOriginal = Console.Out;
    using var leitor = new StringReader(entrada);
    using var escritor = new StringWriter();
    try
    {
        Console.SetIn(leitor);
        Console.SetOut(escritor);
        new MenuPrincipal().Exibir();
        return escritor.ToString();
    }
    finally
    {
        Console.SetIn(leitorOriginal);
        Console.SetOut(escritorOriginal);
    }
}

try
{
    using (var banco = new AppDbContext()) banco.Database.EnsureCreated();

    var semTreinos = new Usuario("Integração " + Guid.NewGuid().ToString("N"), 20);
    usuarios.Cadastrar(semTreinos);
    criados.Add(semTreinos.Id);
    Conferir(usuarios.BuscarPorId(semTreinos.Id)?.Nome == semTreinos.Nome,
        "carrega o usuário que será apresentado na confirmação");
    Conferir(usuarios.Excluir(semTreinos.Id) == ResultadoExclusaoUsuario.Excluido,
        "exclui um usuário sem treinos");
    Conferir(usuarios.BuscarPorId(semTreinos.Id) == null,
        "a exclusão persiste em uma nova consulta ao banco");
    Conferir(usuarios.Excluir(semTreinos.Id) == ResultadoExclusaoUsuario.NaoEncontrado,
        "trata um usuário removido por outra operação");

    var comTreinos = new Usuario("Integração " + Guid.NewGuid().ToString("N"), 21);
    usuarios.Cadastrar(comTreinos);
    criados.Add(comTreinos.Id);
    var treino = new Treino(comTreinos.Id, "Corrida", new DateTime(2026, 11, 24), 30);
    treinos.Cadastrar(treino);

    Conferir(usuarios.Excluir(comTreinos.Id) == ResultadoExclusaoUsuario.PossuiTreinos,
        "bloqueia a exclusão de usuário com treinos");
    Conferir(usuarios.BuscarPorId(comTreinos.Id) != null && treinos.BuscarPorId(treino.Id) != null,
        "preserva o usuário e o treino quando a exclusão é bloqueada");

    bool protegidoPelaChave = false;
    using (var conexao = new MySqlConnection(conexaoTeste))
    {
        conexao.Open();
        using var comando = new MySqlCommand("DELETE FROM usuarios WHERE id = @id", conexao);
        comando.Parameters.AddWithValue("@id", comTreinos.Id);
        try { comando.ExecuteNonQuery(); }
        catch (MySqlException erro) when (erro.Number == 1451) { protegidoPelaChave = true; }
    }
    Conferir(protegidoPelaChave,
        "a chave estrangeira também impede exclusão direta no MySQL");

    var outro = new Usuario("Integração " + Guid.NewGuid().ToString("N"), 22);
    usuarios.Cadastrar(outro);
    criados.Add(outro.Id);
    Conferir(usuarios.Excluir(outro.Id) == ResultadoExclusaoUsuario.Excluido
        && treinos.BuscarPorId(treino.Id) != null,
        "excluir outro usuário mantém os treinos existentes");
    Conferir(treinos.Excluir(treino.Id), "exclui o treino pela operação existente");
    Conferir(usuarios.Excluir(comTreinos.Id) == ResultadoExclusaoUsuario.Excluido,
        "permite excluir o usuário após remover seus treinos");

    var peloMenu = new Usuario("Menu " + Guid.NewGuid().ToString("N"), 23);
    usuarios.Cadastrar(peloMenu);
    criados.Add(peloMenu.Id);
    string saida = ExecutarMenu("0\n");
    Conferir(saida.Contains("9 - Excluir usuário"),
        "o menu apresenta a opção 9 para excluir usuários");

    foreach (string idInvalido in new[] { "abc", "0", "-1", "2147483648" })
    {
        saida = ExecutarMenu($"9\n{idInvalido}\n0\n");
        Conferir(saida.Contains("O identificador deve ser um número inteiro maior que zero.")
            && usuarios.BuscarPorId(peloMenu.Id) != null,
            $"o menu rejeita o identificador inválido {idInvalido}");
    }

    saida = ExecutarMenu($"9\n{semTreinos.Id}\n0\n");
    Conferir(saida.Contains("Usuário não encontrado."),
        "o menu informa quando o usuário não existe");

    saida = ExecutarMenu($"9\n{peloMenu.Id}\nN\n0\n");
    Conferir(saida.Contains(peloMenu.Nome) && saida.Contains("Exclusão de usuário cancelada.")
        && usuarios.BuscarPorId(peloMenu.Id) != null,
        "a confirmação mostra o usuário e cancelar preserva o registro");
    saida = ExecutarMenu($"9\n{peloMenu.Id}\ntalvez\nn\n0\n");
    Conferir(saida.Contains("Opção inválida. Digite S")
        && usuarios.BuscarPorId(peloMenu.Id) != null,
        "a confirmação inválida não exclui e aceita cancelar com n minúsculo");
    saida = ExecutarMenu($"9\n{peloMenu.Id}\n");
    Conferir(saida.Contains("Exclusão de usuário cancelada.")
        && usuarios.BuscarPorId(peloMenu.Id) != null,
        "o fim da entrada durante a confirmação cancela a exclusão");

    var treinoMenu = new Treino(peloMenu.Id, "Natação", new DateTime(2026, 11, 25), 45);
    treinos.Cadastrar(treinoMenu);
    saida = ExecutarMenu($"9\n{peloMenu.Id}\nS\n0\n");
    Conferir(saida.Contains("possui treinos cadastrados e não pode ser excluído.")
        && usuarios.BuscarPorId(peloMenu.Id) != null && treinos.BuscarPorId(treinoMenu.Id) != null,
        "o menu bloqueia a exclusão e mantém o usuário e seu treino");

    treinos.Excluir(treinoMenu.Id);
    saida = ExecutarMenu($"9\n{peloMenu.Id}\n s \n0\n");
    Conferir(saida.Contains("Usuário excluído com sucesso!") && usuarios.BuscarPorId(peloMenu.Id) == null,
        "o menu exclui o usuário sem treinos após confirmação com s");
    Console.WriteLine($"Concluído: {verificacoes} verificações de integração com MySQL.");
    return 0;
}
finally
{
    // Remove somente os registros criados por esta execução, mesmo se um teste falhar.
    using var conexao = new MySqlConnection(conexaoTeste);
    conexao.Open();
    foreach (int id in criados)
    {
        using var comando = new MySqlCommand(
            "DELETE FROM treinos WHERE usuario_id = @id; DELETE FROM usuarios WHERE id = @id;", conexao);
        comando.Parameters.AddWithValue("@id", id);
        comando.ExecuteNonQuery();
    }
}
