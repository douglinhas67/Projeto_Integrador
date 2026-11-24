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
        ClientSize = new Size(900, 500);
        MinimumSize = new Size(760, 460);

        TableLayoutPanel estrutura = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4,
            Margin = new Padding(0)
        };

        estrutura.ColumnStyles.Add(
            new ColumnStyle(SizeType.Percent, 100)
        );

        estrutura.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        estrutura.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        estrutura.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        estrutura.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        TableLayoutPanel cabecalho = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            ColumnCount = 1,
            RowCount = 2,
            Padding = new Padding(24),
            Margin = new Padding(0),
            BackColor = Tema.AzulEscuro
        };

        cabecalho.ColumnStyles.Add(
            new ColumnStyle(SizeType.Percent, 100)
        );

        cabecalho.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        cabecalho.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        Label titulo = new Label
        {
            Text = "Sistema de Controle de Treinos",
            AutoSize = true,
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 22F, FontStyle.Bold),
            ForeColor = Color.White,
            Margin = new Padding(0, 0, 0, 8)
        };

        Label subtitulo = new Label
        {
            Text = "Esporte e qualidade de vida — escolha uma área para continuar.",
            AutoSize = true,
            Dock = DockStyle.Fill,
            ForeColor = Color.FromArgb(214, 226, 242),
            Margin = new Padding(0)
        };

        cabecalho.Controls.Add(titulo, 0, 0);
        cabecalho.Controls.Add(subtitulo, 0, 1);

        FlowLayoutPanel painelOpcoes = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            WrapContents = true,
            Padding = new Padding(24, 24, 24, 12),
            Margin = new Padding(0)
        };

        botaoUsuarios = CriarBotaoMenu(
            "Usuários",
            "Cadastro e consulta"
        );

        botaoTreinos = CriarBotaoMenu(
            "Treinos",
            "Registro e histórico"
        );

        botaoResumo = CriarBotaoMenu(
            "Resumo",
            "Quantidade e minutos"
        );

        botaoUsuarios.Click += (sender, e) =>
        {
            using FormUsuarios janela = new FormUsuarios();

            Tema.Aplicar(janela);

            janela.ShowDialog(this);
        };

        botaoTreinos.Click += (sender, e) =>
        {
            using FormTreinos janela = new FormTreinos();

            Tema.Aplicar(janela);

            janela.ShowDialog(this);
        };

        botaoResumo.Click += (sender, e) =>
        {
            using FormResumo janela = new FormResumo();

            Tema.Aplicar(janela);

            janela.ShowDialog(this);
        };

        painelOpcoes.Controls.Add(botaoUsuarios);
        painelOpcoes.Controls.Add(botaoTreinos);
        painelOpcoes.Controls.Add(botaoResumo);

        statusConexao.Text = "Conexão ainda não verificada.";
        statusConexao.AutoSize = true;
        statusConexao.Dock = DockStyle.Fill;
        statusConexao.ForeColor = Color.DimGray;
        statusConexao.Margin = new Padding(24, 0, 24, 8);

        FlowLayoutPanel rodape = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            WrapContents = true,
            Padding = new Padding(24, 0, 24, 20),
            Margin = new Padding(0)
        };

        botaoVerificarConexao.Text = "Verificar conexão";
        botaoVerificarConexao.AutoSize = true;
        botaoVerificarConexao.Margin = new Padding(0, 0, 12, 0);

        botaoVerificarConexao.Click += async (sender, e) =>
        {
            await VerificarConexaoAsync();
        };

        Button botaoSair = new Button
        {
            Text = "Sair",
            AutoSize = true,
            Margin = new Padding(0)
        };

        botaoSair.Click += (sender, e) => Close();

        rodape.Controls.Add(botaoVerificarConexao);
        rodape.Controls.Add(botaoSair);

        estrutura.Controls.Add(cabecalho, 0, 0);
        estrutura.Controls.Add(painelOpcoes, 0, 1);
        estrutura.Controls.Add(statusConexao, 0, 2);
        estrutura.Controls.Add(rodape, 0, 3);

        Controls.Add(estrutura);

        Tema.Aplicar(this);

        Shown += async (sender, e) =>
        {
            await VerificarConexaoAsync();
        };
    }

    private Button CriarBotaoMenu(string titulo, string descricao)
    {
        return new Button
        {
            Text = titulo + Environment.NewLine + descricao,
            Width = 240,
            Height = 115,
            Font = new Font("Segoe UI", 12F, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleCenter,
            Margin = new Padding(0, 0, 16, 16),
            Enabled = false
        };
    }

    private async Task VerificarConexaoAsync()
    {
        botaoVerificarConexao.Enabled = false;
        botaoUsuarios.Enabled = false;
        botaoTreinos.Enabled = false;
        botaoResumo.Enabled = false;

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
            statusConexao.ForeColor = Tema.Verde;

            botaoUsuarios.Enabled = true;
            botaoTreinos.Enabled = true;
            botaoResumo.Enabled = true;
        }
        catch (Exception erro)
        {
            if (IsDisposed || Disposing)
            {
                return;
            }

            statusConexao.Text = "Conexão com o MySQL indisponível.";
            statusConexao.ForeColor = Tema.Vermelho;

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