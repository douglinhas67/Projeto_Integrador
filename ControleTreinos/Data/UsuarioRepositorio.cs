using ControleTreinos.Models;
using Microsoft.EntityFrameworkCore;

namespace ControleTreinos.Data;

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
}