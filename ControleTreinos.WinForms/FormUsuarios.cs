using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using ControleTreinos.Data;
using ControleTreinos.Models;

namespace ControleTreinos.WinForms;

public class FormUsuarios : Form
{
    private readonly UsuarioRepositorio usuarioRepositorio =
        new UsuarioRepositorio();

    private readonly TextBox campoNome = new TextBox();
    private readonly NumericUpDown campoIdade = new NumericUpDown();

    private readonly Button botaoCadastrar = new Button();
    private readonly Button botaoAtualizar = new Button();
    private readonly Button botaoFechar = new Button();

    private readonly DataGridView tabelaUsuarios = new DataGridView();
    private readonly Label status = new Label();

    private bool operacaoEmAndamento;

    public FormUsuarios()
    {
        Text = "Usuários - Controle de Treinos";
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(840, 520);
        MinimumSize = new Size(700, 420);

        Font = new Font("Segoe UI", 10F);
        BackColor = Color.FromArgb(245, 247, 250);

        TableLayoutPanel estrutura = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(20),
            ColumnCount = 1,
            RowCount = 5
        };

        estrutura.ColumnStyles.Add(
            new ColumnStyle(SizeType.Percent, 100)
        );

        estrutura.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        estrutura.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        estrutura.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        estrutura.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        estrutura.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        Label titulo = new Label
        {
            Text = "Usuários",
            AutoSize = true,
            Font = new Font("Segoe UI", 20F, FontStyle.Bold),
            ForeColor = Color.FromArgb(22, 43, 73),
            Margin = new Padding(0, 0, 0, 16)
        };

        TableLayoutPanel campos = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            ColumnCount = 4,
            RowCount = 1,
            Margin = new Padding(0, 0, 0, 12)
        };

        campos.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        campos.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        campos.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        campos.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));

        Label rotuloNome = new Label
        {
            Text = "Nome:",
            AutoSize = true,
            Anchor = AnchorStyles.Left,
            Margin = new Padding(0, 0, 8, 0)
        };

        campoNome.Dock = DockStyle.Fill;
        campoNome.MaxLength = 100;
        campoNome.AccessibleName = "Nome do usuário";
        campoNome.Margin = new Padding(0, 3, 16, 3);

        Label rotuloIdade = new Label
        {
            Text = "Idade:",
            AutoSize = true,
            Anchor = AnchorStyles.Left,
            Margin = new Padding(0, 0, 8, 0)
        };

        campoIdade.Dock = DockStyle.Fill;
        campoIdade.Minimum = 1;
        campoIdade.Maximum = int.MaxValue;
        campoIdade.Value = 1;
        campoIdade.AccessibleName = "Idade do usuário";

        campos.Controls.Add(rotuloNome, 0, 0);
        campos.Controls.Add(campoNome, 1, 0);
        campos.Controls.Add(rotuloIdade, 2, 0);
        campos.Controls.Add(campoIdade, 3, 0);

        FlowLayoutPanel botoes = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            WrapContents = true,
            Margin = new Padding(0, 0, 0, 12)
        };

        ConfigurarBotao(botaoCadastrar, "Cadastrar usuário");
        ConfigurarBotao(botaoAtualizar, "Atualizar lista");
        ConfigurarBotao(botaoFechar, "Fechar");

        botaoCadastrar.Click += async (sender, e) =>
        {
            await CadastrarUsuarioAsync();
        };

        botaoAtualizar.Click += async (sender, e) =>
        {
            await AtualizarListaAsync();
        };

        botaoFechar.Click += (sender, e) => Close();

        botoes.Controls.Add(botaoCadastrar);
        botoes.Controls.Add(botaoAtualizar);
        botoes.Controls.Add(botaoFechar);

        tabelaUsuarios.Dock = DockStyle.Fill;
        tabelaUsuarios.ReadOnly = true;
        tabelaUsuarios.AllowUserToAddRows = false;
        tabelaUsuarios.AllowUserToDeleteRows = false;
        tabelaUsuarios.AutoGenerateColumns = false;
        tabelaUsuarios.RowHeadersVisible = false;
        tabelaUsuarios.MultiSelect = false;
        tabelaUsuarios.SelectionMode =
            DataGridViewSelectionMode.FullRowSelect;

        tabelaUsuarios.AutoSizeColumnsMode =
            DataGridViewAutoSizeColumnsMode.Fill;

        tabelaUsuarios.BackgroundColor = Color.White;
        tabelaUsuarios.Margin = new Padding(0);
        tabelaUsuarios.AccessibleName = "Usuários cadastrados";

        tabelaUsuarios.Columns.AddRange(
            new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(Usuario.Id),
                HeaderText = "ID",
                FillWeight = 15
            },
            new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(Usuario.Nome),
                HeaderText = "Nome",
                FillWeight = 65
            },
            new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(Usuario.Idade),
                HeaderText = "Idade",
                FillWeight = 20
            }
        );

        status.Text = "Aguardando carregamento.";
        status.AutoSize = true;
        status.ForeColor = Color.DimGray;
        status.Margin = new Padding(0, 12, 0, 0);

        estrutura.Controls.Add(titulo, 0, 0);
        estrutura.Controls.Add(campos, 0, 1);
        estrutura.Controls.Add(botoes, 0, 2);
        estrutura.Controls.Add(tabelaUsuarios, 0, 3);
        estrutura.Controls.Add(status, 0, 4);

        Controls.Add(estrutura);

        Shown += async (sender, e) =>
        {
            await AtualizarListaAsync();
            campoNome.Focus();
        };

        FormClosing += (sender, e) =>
        {
            if (operacaoEmAndamento
                && e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;

                MessageBox.Show(
                    this,
                    "Aguarde a operação terminar para fechar a janela.",
                    "Operação em andamento",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            }
        };
    }

    private void ConfigurarBotao(Button botao, string texto)
    {
        botao.Text = texto;
        botao.AutoSize = true;
        botao.Padding = new Padding(12, 6, 12, 6);
        botao.Margin = new Padding(0, 0, 12, 0);
    }

    private void DefinirOcupado(bool ocupado)
    {
        operacaoEmAndamento = ocupado;

        if (IsDisposed || Disposing)
        {
            return;
        }

        campoNome.Enabled = !ocupado;
        campoIdade.Enabled = !ocupado;
        botaoCadastrar.Enabled = !ocupado;
        botaoAtualizar.Enabled = !ocupado;
        botaoFechar.Enabled = !ocupado;
    }

    private async Task AtualizarListaAsync()
    {
        if (operacaoEmAndamento || IsDisposed || Disposing)
        {
            return;
        }

        DefinirOcupado(true);

        try
        {
            await CarregarDadosAsync();
        }
        finally
        {
            DefinirOcupado(false);
        }
    }

    private async Task CarregarDadosAsync()
    {
        try
        {
            status.Text = "Carregando usuários...";

            var usuarios = await Task.Run(
                () => usuarioRepositorio.Listar()
            );

            if (IsDisposed || Disposing)
            {
                return;
            }

            tabelaUsuarios.DataSource = usuarios;
            status.Text = $"Usuários cadastrados: {usuarios.Count}.";
        }
        catch (Exception erro)
        {
            MostrarErro(
                "Não foi possível carregar a lista de usuários.",
                erro
            );
        }
    }

    private async Task CadastrarUsuarioAsync()
    {
        if (operacaoEmAndamento || IsDisposed || Disposing)
        {
            return;
        }

        Usuario usuario;

        try
        {
            usuario = new Usuario(
                campoNome.Text.Trim(),
                (int)campoIdade.Value
            );
        }
        catch (ArgumentException erro)
        {
            MessageBox.Show(
                this,
                erro.Message,
                "Dados inválidos",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning
            );

            campoNome.Focus();
            campoNome.SelectAll();
            return;
        }

        DefinirOcupado(true);
        status.Text = "Salvando usuário...";

        try
        {
            await Task.Run(
                () => usuarioRepositorio.Cadastrar(usuario)
            );

            if (IsDisposed || Disposing)
            {
                return;
            }

            campoNome.Clear();
            campoIdade.Value = 1;

            MessageBox.Show(
                this,
                "Usuário cadastrado com sucesso!" +
                Environment.NewLine +
                $"Identificador: {usuario.Id}",
                "Cadastro de usuário",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );

            await CarregarDadosAsync();
        }
        catch (Exception erro)
        {
            MostrarErro(
                "Não foi possível cadastrar o usuário.",
                erro
            );
        }
        finally
        {
            DefinirOcupado(false);

            if (!IsDisposed && !Disposing)
            {
                campoNome.Focus();
            }
        }
    }

    private void MostrarErro(string mensagem, Exception erro)
    {
        if (IsDisposed || Disposing)
        {
            return;
        }

        status.Text = mensagem;

        MessageBox.Show(
            this,
            mensagem +
            Environment.NewLine +
            erro.GetBaseException().Message,
            "Controle de Treinos",
            MessageBoxButtons.OK,
            MessageBoxIcon.Warning
        );
    }
}