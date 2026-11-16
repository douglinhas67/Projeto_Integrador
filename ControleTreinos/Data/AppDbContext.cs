using ControleTreinos.Models;
using Microsoft.EntityFrameworkCore;

namespace ControleTreinos.Data;

public class AppDbContext : DbContext
{
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Treino> Treinos => Set<Treino>();

    protected override void OnConfiguring(
        DbContextOptionsBuilder optionsBuilder)
    {
        string? conexao = Environment.GetEnvironmentVariable(
            "CONTROLE_TREINOS_CONEXAO"
        );

        if (string.IsNullOrWhiteSpace(conexao))
        {
            throw new InvalidOperationException(
                "Configure a variável CONTROLE_TREINOS_CONEXAO."
            );
        }

        optionsBuilder.UseMySQL(conexao);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var usuario = modelBuilder.Entity<Usuario>();

        usuario.ToTable("usuarios");
        usuario.HasKey(u => u.Id);

        usuario.Property(u => u.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        usuario.Property(u => u.Nome)
            .HasColumnName("nome")
            .HasMaxLength(100)
            .IsRequired();

        usuario.Property(u => u.Idade)
            .HasColumnName("idade");

        var treino = modelBuilder.Entity<Treino>();

        treino.ToTable("treinos");
        treino.HasKey(t => t.Id);

        treino.Property(t => t.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        treino.Property(t => t.UsuarioId)
            .HasColumnName("usuario_id");

        treino.Property(t => t.Modalidade)
            .HasColumnName("modalidade")
            .HasMaxLength(50)
            .IsRequired();

        treino.Property(t => t.Data)
            .HasColumnName("data")
            .HasColumnType("date");

        treino.Property(t => t.DuracaoMinutos)
            .HasColumnName("duracao_minutos");

        treino.HasOne<Usuario>()
            .WithMany()
            .HasForeignKey(t => t.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_treinos_usuario");
    }
}