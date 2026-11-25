using ControleTreinos.Data;
using Microsoft.EntityFrameworkCore;

namespace ControleTreinos.WinForms;

public class FormPrincipal : Form
{
    private readonly Panel painelConteudo = new() { Dock = DockStyle.Fill, Margin = new Padding(0) };
    private readonly Label statusConexao = new();
    private readonly Button botaoVerificarConexao = new();
    private readonly Dictionary<string, Button> navegacao = new();
    private Form? conteudoAtual;
    private string telaAtual = "";
    private bool conectado;
    private bool verificandoConexao;

    public FormPrincipal()
    {
        Text = "Sistema de Controle de Treinos";
        StartPosition = FormStartPosition.CenterScreen;
        AutoScaleMode = AutoScaleMode.Dpi;
        Font = Tema.FontePadrao;
        BackColor = Tema.Fundo;
        ForeColor = Tema.Texto;
        ClientSize = new Size(1280, 760);
        MinimumSize = new Size(1100, 660);

        // Ajusta a abertura à área útil da tela, inclusive em notebooks.
        var areaUtil = Screen.FromControl(this).WorkingArea;
        Size = new Size(Math.Min(Width, areaUtil.Width), Math.Min(Height, areaUtil.Height));

        var estrutura = new TableLayoutPanel
        {
            Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 2,
            Margin = new Padding(0), Padding = new Padding(0)
        };
        estrutura.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 194));
        estrutura.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        estrutura.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        estrutura.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var lateral = new TableLayoutPanel
        {
            Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4,
            BackColor = Tema.AzulEscuro, Padding = new Padding(16, 28, 16, 20),
            Margin = new Padding(0)
        };
        lateral.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        lateral.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        lateral.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        lateral.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        lateral.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var marca = Componentes.Coluna();
        var simbolo = new Label
        {
            Text = "CT", AutoSize = false, Width = 46, Height = 42,
            Font = new Font("Segoe UI", 16F, FontStyle.Bold), TextAlign = ContentAlignment.MiddleCenter,
            BackColor = Tema.Destaque, ForeColor = Tema.TextoDestaque, Margin = new Padding(0, 0, 0, 12)
        };
        marca.Controls.Add(simbolo);
        var nome = Componentes.Rotulo("Controle de Treinos", Color.White);
        nome.Font = Tema.FonteSecao;
        marca.Controls.Add(nome);
        var descricao = Componentes.Rotulo("Seu histórico em movimento", Color.FromArgb(174, 190, 209));
        descricao.Margin = new Padding(0, 6, 0, 26);
        marca.Controls.Add(descricao);
        lateral.Controls.Add(marca, 0, 0);

        var menu = Componentes.Coluna();
        menu.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        AdicionarNavegacao(menu, "visao", "Visão geral");
        AdicionarNavegacao(menu, "usuarios", "Usuários");
        AdicionarNavegacao(menu, "treinos", "Treinos");
        AdicionarNavegacao(menu, "resumo", "Resumo");
        lateral.Controls.Add(menu, 0, 1);

        var rodape = Componentes.Coluna();
        rodape.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        var equipe = Componentes.Rotulo("PROJETO INTEGRADOR\nSENAC · Thomas e Igor", Color.FromArgb(174, 190, 209));
        equipe.Margin = new Padding(0, 0, 0, 18);
        rodape.Controls.Add(equipe);
        Componentes.Botao(botaoVerificarConexao, "Verificar conexão");
        botaoVerificarConexao.Dock = DockStyle.Fill;
        botaoVerificarConexao.Margin = new Padding(0, 0, 0, 8);
        botaoVerificarConexao.Click += async (_, _) => await VerificarConexaoAsync();
        rodape.Controls.Add(botaoVerificarConexao);
        var sair = new Button();
        Componentes.Botao(sair, "Sair");
        sair.Dock = DockStyle.Fill;
        sair.Margin = new Padding(0);
        sair.Click += (_, _) => Close();
        rodape.Controls.Add(sair);
        lateral.Controls.Add(rodape, 0, 3);

        var barra = new TableLayoutPanel
        {
            Dock = DockStyle.Fill, AutoSize = true, ColumnCount = 2,
            BackColor = Tema.Superficie, Padding = new Padding(26, 12, 26, 12), Margin = new Padding(0)
        };
        barra.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        barra.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        barra.Controls.Add(Componentes.Rotulo("Sistema de Controle de Treinos", Tema.TextoSecundario), 0, 0);
        statusConexao.AutoSize = true;
        statusConexao.Text = "Verificando conexão...";
        statusConexao.Font = Tema.FontePequena;
        statusConexao.ForeColor = Tema.TextoSecundario;
        statusConexao.TextAlign = ContentAlignment.MiddleRight;
        statusConexao.Margin = new Padding(18, 0, 0, 0);
        barra.Controls.Add(statusConexao, 1, 0);

        estrutura.Controls.Add(lateral, 0, 0);
        estrutura.SetRowSpan(lateral, 2);
        estrutura.Controls.Add(barra, 1, 0);
        estrutura.Controls.Add(painelConteudo, 1, 1);
        Controls.Add(estrutura);

        var boasVindas = Componentes.Rotulo("Organize seus treinos.\n\nVerificando o acesso ao banco de dados...", Tema.TextoSecundario);
        boasVindas.Dock = DockStyle.Fill;
        boasVindas.TextAlign = ContentAlignment.MiddleCenter;
        painelConteudo.Controls.Add(boasVindas);
        Shown += async (_, _) => await VerificarConexaoAsync();
        FormClosing += (_, e) =>
        {
            // Passa pela confirmação da tela atual antes de sair da aplicação.
            if (e.CloseReason == CloseReason.UserClosing && !FecharConteudo()) e.Cancel = true;
        };
    }

    private void AdicionarNavegacao(TableLayoutPanel menu, string chave, string texto)
    {
        var botao = new Button
        {
            Text = texto, Dock = DockStyle.Fill, Height = 46,
            TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(14, 0, 0, 0),
            Margin = new Padding(0, 0, 0, 8), FlatStyle = FlatStyle.Flat,
            Font = Tema.FontePadrao, BackColor = Tema.AzulEscuro,
            ForeColor = Color.FromArgb(190, 204, 222), Enabled = false,
            AccessibleName = "Abrir " + texto
        };
        botao.FlatAppearance.BorderSize = 0;
        botao.Click += (_, _) => AbrirTela(chave);
        navegacao.Add(chave, botao);
        menu.Controls.Add(botao);
    }

    private bool FecharConteudo()
    {
        if (conteudoAtual == null || conteudoAtual.IsDisposed) return true;
        conteudoAtual.Close();
        return conteudoAtual.IsDisposed;
    }

    private void AbrirTela(string chave)
    {
        if (!conectado || verificandoConexao || telaAtual == chave && conteudoAtual is { IsDisposed: false }) return;
        if (!FecharConteudo()) return;
        foreach (Control anterior in painelConteudo.Controls.Cast<Control>().ToArray()) anterior.Dispose();
        painelConteudo.Controls.Clear();
        conteudoAtual = chave switch
        {
            "usuarios" => new FormUsuarios(),
            "treinos" => new FormTreinos(),
            "resumo" => new FormResumo(),
            _ => new FormVisaoGeral()
        };
        conteudoAtual.TopLevel = false;
        conteudoAtual.FormBorderStyle = FormBorderStyle.None;
        conteudoAtual.MinimumSize = Size.Empty;
        conteudoAtual.Dock = DockStyle.Fill;
        painelConteudo.Controls.Add(conteudoAtual);
        telaAtual = chave;
        foreach (var item in navegacao)
        {
            bool selecionado = item.Key == chave;
            item.Value.BackColor = selecionado ? Tema.Destaque : Tema.AzulEscuro;
            item.Value.ForeColor = selecionado ? Tema.TextoDestaque : Color.FromArgb(190, 204, 222);
            item.Value.FlatAppearance.MouseOverBackColor = selecionado ? Tema.Destaque : Tema.Azul;
            item.Value.FlatAppearance.MouseDownBackColor = selecionado ? Tema.Destaque : Tema.Azul;
        }
        conteudoAtual.Show();
    }

    private async Task VerificarConexaoAsync()
    {
        if (verificandoConexao || IsDisposed || Disposing) return;
        verificandoConexao = true;
        botaoVerificarConexao.Enabled = false;
        painelConteudo.Enabled = false;
        foreach (var botao in navegacao.Values) botao.Enabled = false;
        statusConexao.Text = "Verificando MySQL...";
        statusConexao.ForeColor = Tema.TextoSecundario;
        try
        {
            using AppDbContext banco = new();
            await banco.Database.OpenConnectionAsync();
            await banco.Database.CloseConnectionAsync();
            if (IsDisposed || Disposing) return;
            conectado = true;
            statusConexao.Text = "● MySQL conectado";
            statusConexao.ForeColor = Tema.Verde;
        }
        catch (Exception erro)
        {
            if (IsDisposed || Disposing) return;
            conectado = false;
            statusConexao.Text = "● MySQL indisponível";
            statusConexao.ForeColor = Tema.Vermelho;
            MessageBox.Show(this, "Não foi possível conectar ao MySQL." + Environment.NewLine
                + erro.GetBaseException().Message, "Conexão com o banco", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        finally
        {
            verificandoConexao = false;
            if (!IsDisposed && !Disposing)
            {
                botaoVerificarConexao.Enabled = true;
                painelConteudo.Enabled = conectado;
                foreach (var botao in navegacao.Values) botao.Enabled = conectado;
                if (conectado && (conteudoAtual == null || conteudoAtual.IsDisposed)) AbrirTela("visao");
            }
        }
    }
}
