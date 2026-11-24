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
        AutoScaleMode = AutoScaleMode.Dpi;
        ClientSize = new Size(1120, 700);
        MinimumSize = new Size(900, 560);
        var pagina = new LayoutPagina("Usuários",
            "Cadastre as pessoas que terão seus treinos acompanhados.");

        campoNome.MaxLength = 100;
        campoNome.PlaceholderText = "Nome completo";
        campoIdade.Minimum = 1;
        campoIdade.Maximum = int.MaxValue;
        campoIdade.Value = 1;
        Componentes.Botao(botaoCadastrar, "Cadastrar usuário");
        Componentes.Botao(botaoAtualizar, "Atualizar lista");
        Componentes.Botao(botaoFechar, "Fechar");
        botaoCadastrar.Dock = DockStyle.Fill;
        botaoCadastrar.Click += async (_, _) => await CadastrarUsuarioAsync();
        botaoAtualizar.Click += async (_, _) => await AtualizarListaAsync();
        botaoFechar.Click += (_, _) => Close();
        pagina.Acoes.Controls.Add(botaoAtualizar);
        pagina.Acoes.Controls.Add(botaoFechar);

        tabelaUsuarios.ReadOnly = true;
        tabelaUsuarios.AllowUserToAddRows = false;
        tabelaUsuarios.AllowUserToDeleteRows = false;
        tabelaUsuarios.AutoGenerateColumns = false;
        tabelaUsuarios.RowHeadersVisible = false;
        tabelaUsuarios.MultiSelect = false;
        tabelaUsuarios.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        tabelaUsuarios.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        tabelaUsuarios.AccessibleName = "Usuários cadastrados";
        tabelaUsuarios.Columns.AddRange(
            new DataGridViewTextBoxColumn { DataPropertyName = nameof(Usuario.Id), HeaderText = "ID", FillWeight = 15, MinimumWidth = 55 },
            new DataGridViewTextBoxColumn { DataPropertyName = nameof(Usuario.Nome), HeaderText = "Nome", FillWeight = 65, MinimumWidth = 160 },
            new DataGridViewTextBoxColumn { DataPropertyName = nameof(Usuario.Idade), HeaderText = "Idade", FillWeight = 20, MinimumWidth = 80 }
        );
        var editor = Componentes.Editor(new Label { Text = "Novo usuário" },
            Componentes.Campo("Nome", campoNome, "Até 100 caracteres."),
            Componentes.Campo("Idade em anos", campoIdade),
            botaoCadastrar);
        pagina.Conteudo(Componentes.Dividir(Componentes.Lista("Pessoas cadastradas", tabelaUsuarios), editor));
        status.Text = "Carregando usuários...";
        pagina.Rodape(status);
        Controls.Add(pagina);
        Tema.Aplicar(this);

        Shown += async (sender, e) =>
        {
            botaoFechar.Visible = TopLevel;
            await AtualizarListaAsync();
            if (!IsDisposed && !Disposing) campoNome.Focus();
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