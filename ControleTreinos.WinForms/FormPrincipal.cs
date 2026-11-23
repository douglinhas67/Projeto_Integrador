using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using ControleTreinos.Data;
using Microsoft.EntityFrameworkCore;

namespace ControleTreinos.WinForms;

public class FormPrincipal : Form
{
    private readonly Label statusConexao = new Label();
    private readonly Button botaoVerificarConexao = new Button();

    private readonly Button botaoUsuarios;
    private readonly Button botaoTreinos;
    private readonly Button botaoResumo;

    public FormPrincipal()
    {
        Text = "Sistema de Controle de Treinos";
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(820, 460);
        MinimumSize = new Size(700, 420);

        Font = new Font("Segoe UI", 10F);
        BackColor = Color.FromArgb(245, 247, 250);

        TableLayoutPanel estrutura = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(24),
            ColumnCount = 1,
            RowCount = 5
        };

        estrutura.ColumnStyles.Add(
            new ColumnStyle(SizeType.Percent, 100)
        );

        estrutura.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        estrutura.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        estrutura.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        estrutura.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        estrutura.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        Label titulo = new Label
        {
            Text = "Sistema de Controle de Treinos",
            AutoSize = true,
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 20F, FontStyle.Bold),
            ForeColor = Color.FromArgb(22, 43, 73),
            Margin = new Padding(0, 0, 0, 8)
        };

        Label subtitulo = new Label
        {
            Text = "Organize usuários, treinos e histórico de atividades.",
            AutoSize = true,
            Dock = DockStyle.Fill,
            ForeColor = Color.DimGray,
            Margin = new Padding(0, 0, 0, 16)
        };

        FlowLayoutPanel painelOpcoes = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            WrapContents = true,
            Margin = new Padding(0)
        };

        botaoUsuarios = CriarBotaoOpcao("Usuários");
        botaoTreinos = CriarBotaoOpcao("Treinos");
        botaoResumo = CriarBotaoOpcao("Resumo de atividades");

        botaoUsuarios.Click += (sender, e) =>
        {
            using FormUsuarios janela = new FormUsuarios();
            janela.ShowDialog(this);
        };

        painelOpcoes.Controls.Add(botaoUsuarios);
        painelOpcoes.Controls.Add(botaoTreinos);
        painelOpcoes.Controls.Add(botaoResumo);

        statusConexao.Text = "Conexão ainda não verificada.";
        statusConexao.AutoSize = true;
        statusConexao.Dock = DockStyle.Fill;
        statusConexao.ForeColor = Color.DimGray;
        statusConexao.Margin = new Padding(0, 8, 0, 12);

        FlowLayoutPanel rodape = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            WrapContents = true,
            Margin = new Padding(0)
        };

        botaoVerificarConexao.Text = "Verificar conexão";
        botaoVerificarConexao.AutoSize = true;
        botaoVerificarConexao.Padding = new Padding(12, 6, 12, 6);
        botaoVerificarConexao.Margin = new Padding(0, 0, 12, 0);

        botaoVerificarConexao.Click += async (sender, e) =>
        {
            await VerificarConexaoAsync();
        };

        Button botaoSair = new Button
        {
            Text = "Sair",
            AutoSize = true,
            Padding = new Padding(12, 6, 12, 6),
            Margin = new Padding(0)
        };

        botaoSair.Click += (sender, e) => Close();

        rodape.Controls.Add(botaoVerificarConexao);
        rodape.Controls.Add(botaoSair);

        estrutura.Controls.Add(titulo, 0, 0);
        estrutura.Controls.Add(subtitulo, 0, 1);
        estrutura.Controls.Add(painelOpcoes, 0, 2);
        estrutura.Controls.Add(statusConexao, 0, 3);
        estrutura.Controls.Add(rodape, 0, 4);

        Controls.Add(estrutura);

        Shown += async (sender, e) =>
        {
            await VerificarConexaoAsync();
        };
    }

    private Button CriarBotaoOpcao(string texto)
    {
        return new Button
        {
            Text = texto,
            Width = 220,
            Height = 72,
            Margin = new Padding(0, 12, 12, 0),
            Enabled = false
        };
    }

    private async Task VerificarConexaoAsync()
    {
        botaoVerificarConexao.Enabled = false;
        botaoUsuarios.Enabled = false;

        statusConexao.Text = "Verificando conexão com o MySQL...";
        statusConexao.ForeColor = Color.DimGray;

        try
        {
            using AppDbContext banco = new AppDbContext();

            await banco.Database.OpenConnectionAsync();
            await banco.Database.CloseConnectionAsync();

            if (IsDisposed || Disposing)
            {
                return;
            }

            statusConexao.Text = "Conectado ao MySQL.";
            statusConexao.ForeColor = Color.DarkGreen;

            botaoUsuarios.Enabled = true;
        }
        catch (Exception erro)
        {
            if (IsDisposed || Disposing)
            {
                return;
            }

            statusConexao.Text = "Conexão com o MySQL indisponível.";
            statusConexao.ForeColor = Color.Firebrick;

            MessageBox.Show(
                this,
                "Não foi possível conectar ao MySQL." +
                Environment.NewLine +
                erro.GetBaseException().Message,
                "Conexão com o banco",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning
            );
        }
        finally
        {
            if (!IsDisposed && !Disposing)
            {
                botaoVerificarConexao.Enabled = true;
            }
        }
    }
}