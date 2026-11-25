using ControleTreinos.Models;
using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;

namespace ControleTreinos.Data;

public enum ResultadoExclusaoUsuario
{
    Excluido,
    NaoEncontrado,
    PossuiTreinos
}

public class UsuarioRepositorio
{
    public void Cadastrar(Usuario usuario)
    {
        using AppDbContext banco = new AppDbContext();

        banco.Usuarios.Add(usuario);
        banco.SaveChanges();
    }

    public List<Usuario> Listar()
    {
        using AppDbContext banco = new AppDbContext();

        return banco.Usuarios
            .AsNoTracking()
            .OrderBy(usuario => usuario.Id)
            .ToList();
    }

    public Usuario? BuscarPorId(int usuarioId)
    {
        using AppDbContext banco = new AppDbContext();

        return banco.Usuarios
            .AsNoTracking()
            .FirstOrDefault(usuario => usuario.Id == usuarioId);
    }

    public ResultadoExclusaoUsuario Excluir(int usuarioId)
    {
        using AppDbContext banco = new AppDbContext();

        Usuario? usuario = banco.Usuarios.FirstOrDefault(
            u => u.Id == usuarioId
        );

        if (usuario == null)
        {
            return ResultadoExclusaoUsuario.NaoEncontrado;
        }

        if (banco.Treinos.Any(t => t.UsuarioId == usuarioId))
        {
            return ResultadoExclusaoUsuario.PossuiTreinos;
        }

        banco.Usuarios.Remove(usuario);

        try
        {
            banco.SaveChanges();
        }
        catch (DbUpdateException erro)
            when (erro.GetBaseException() is MySqlException { Number: 1451 })
        {
            // Outro processo pode cadastrar um treino após a consulta acima.
            // A chave estrangeira mantém o usuário e seus treinos no banco.
            return ResultadoExclusaoUsuario.PossuiTreinos;
        }

        return ResultadoExclusaoUsuario.Excluido;
    }
}
