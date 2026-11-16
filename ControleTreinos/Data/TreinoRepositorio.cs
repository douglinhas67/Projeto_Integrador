using ControleTreinos.Models;
using Microsoft.EntityFrameworkCore;

namespace ControleTreinos.Data;

public class TreinoRepositorio
{
    public bool UsuarioExiste(int usuarioId)
    {
        using AppDbContext banco = new AppDbContext();

        return banco.Usuarios.Any(
            usuario => usuario.Id == usuarioId
        );
    }

    public void Cadastrar(Treino treino)
    {
        using AppDbContext banco = new AppDbContext();

        bool usuarioExiste = banco.Usuarios.Any(
            usuario => usuario.Id == treino.UsuarioId
        );

        if (!usuarioExiste)
        {
            throw new ArgumentException(
                "Usuário não encontrado. Cadastre o usuário primeiro."
            );
        }

        banco.Treinos.Add(treino);
        banco.SaveChanges();
    }

    public List<Treino> Listar()
    {
        using AppDbContext banco = new AppDbContext();

        return banco.Treinos
            .AsNoTracking()
            .OrderBy(treino => treino.Id)
            .ToList();
    }

    public List<Treino> BuscarPorUsuario(int usuarioId)
    {
        using AppDbContext banco = new AppDbContext();

        return banco.Treinos
            .AsNoTracking()
            .Where(treino => treino.UsuarioId == usuarioId)
            .OrderBy(treino => treino.Id)
            .ToList();
    }

    public Treino? BuscarPorId(int treinoId)
    {
        using AppDbContext banco = new AppDbContext();

        return banco.Treinos
            .AsNoTracking()
            .FirstOrDefault(treino => treino.Id == treinoId);
    }

    public bool Atualizar(
        int treinoId,
        string modalidade,
        DateTime data,
        int duracaoMinutos)
    {
        using AppDbContext banco = new AppDbContext();

        Treino? treino = banco.Treinos.FirstOrDefault(
            treino => treino.Id == treinoId
        );

        if (treino == null)
        {
            return false;
        }

        treino.AtualizarDados(modalidade, data, duracaoMinutos);

        banco.SaveChanges();

        return true;
    }
}