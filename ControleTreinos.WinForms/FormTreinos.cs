using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using ControleTreinos.Data;
using ControleTreinos.Models;

namespace ControleTreinos.WinForms;

public class FormTreinos : Form
{
    private readonly UsuarioRepositorio usuarioRepositorio =
        new UsuarioRepositorio();

    private readonly TreinoRepositorio treinoRepositorio =
        new TreinoRepositorio();

    private readonly DateTime dataLimite = new DateTime(2026, 12, 14);

    private readonly ComboBox campoUsuario = new ComboBox();
    private readonly TextBox campoModalidade = new TextBox();
    private readonly MaskedTextBox campoData = new MaskedTextBox();
    private readonly NumericUpDown campoDuracao = new NumericUpDown();

    private readonly ComboBox campoFiltroUsuario = new ComboBox();
    private readonly Button botaoBuscar = new Button();
    private readonly Button botaoMostrarTodos = new Button();

    private readonly Button botaoSalvar = new Button();
    private readonly Button botaoEditar = new Button();
    private readonly Button botaoExcluir = new Button();
    private readonly Button botaoCancelarEdicao = new Button();
    private readonly Button botaoAtualizar = new Button();
    private readonly Button botaoFechar = new Button();

    private readonly DataGridView tabelaTreinos = new DataGridView();
    private readonly Label rotuloModo = new Label();
    private readonly Label status = new Label();

    private bool operacaoEmAndamento;
    private bool usuariosDisponiveis;

    private int? treinoEmEdicao;
    private int? usuarioFiltrado;

    public FormTreinos()
    {
        Text = "Treinos - Controle de Treinos";
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(960, 620);
        MinimumSize = new Size(820, 560);

        Font = new Font("Segoe UI", 10F);
        BackColor = Color.FromArgb(245, 247, 250);

        TableLayoutPanel estrutura = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            Padding = new Padding(20),
            ColumnCount = 1,
            RowCount = 7
        };

        estrutura.ColumnStyles.Add(
            new ColumnStyle(SizeType.Percent, 100)
        );

        estrutura.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        estrutura.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        estrutura.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        estrutura.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        estrutura.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        estrutura.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        estrutura.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        Label titulo = new Label
        {
            Text = "Treinos",
            AutoSize = true,
            Font = new Font("Segoe UI", 20F, FontStyle.Bold),
            ForeColor = Color.FromArgb(22, 43, 73),
            Margin = new Padding(0, 0, 0, 8)
        };

        rotuloModo.AutoSize = true;
        rotuloModo.ForeColor = Color.DarkBlue;
        rotuloModo.Margin = new Padding(0, 0, 0, 12);

        TableLayoutPanel campos = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            ColumnCount = 4,
            RowCount = 3,
            Margin = new Padding(0, 0, 0, 12)
        };

        campos.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        campos.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        campos.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        campos.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

        for (int i = 0; i < 3; i++)
        {
            campos.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        }

        campoUsuario.Dock = DockStyle.Fill;
        ConfigurarListaUsuarios(campoUsuario);

        campoModalidade.Dock = DockStyle.Fill;
        campoModalidade.MaxLength = 50;

        campoData.Dock = DockStyle.Fill;
        campoData.Culture = CultureInfo.GetCultureInfo("pt-BR");
        campoData.Mask = "00/00/0000";
        campoData.TextMaskFormat = MaskFormat.IncludeLiterals;

        campoData.Text = ObterDataPadrao().ToString(
            "dd/MM/yyyy",
            CultureInfo.InvariantCulture
        );

        campoDuracao.Dock = DockStyle.Fill;
        campoDuracao.Minimum = 1;
        campoDuracao.Maximum = int.MaxValue;
        campoDuracao.Value = 1;

        campos.Controls.Add(CriarRotulo("Usuário:"), 0, 0);
        campos.Controls.Add(campoUsuario, 1, 0);
        campos.SetColumnSpan(campoUsuario, 3);

        campos.Controls.Add(CriarRotulo("Modalidade:"), 0, 1);
        campos.Controls.Add(campoModalidade, 1, 1);
        campos.SetColumnSpan(campoModalidade, 3);

        campos.Controls.Add(
            CriarRotulo("Data (dd/MM/aaaa):"),
            0,
            2
        );

        campos.Controls.Add(campoData, 1, 2);

        campos.Controls.Add(
            CriarRotulo("Duração (minutos):"),
            2,
            2
        );

        campos.Controls.Add(campoDuracao, 3, 2);

        FlowLayoutPanel botoes = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            WrapContents = true,
            Margin = new Padding(0, 0, 0, 12)
        };

        ConfigurarBotao(botaoSalvar, "Cadastrar treino");
        ConfigurarBotao(botaoEditar, "Editar selecionado");
        ConfigurarBotao(botaoExcluir, "Excluir selecionado");
        ConfigurarBotao(botaoCancelarEdicao, "Cancelar edição");
        ConfigurarBotao(botaoAtualizar, "Atualizar lista");
        ConfigurarBotao(botaoFechar, "Fechar");

        botaoSalvar.Click += async (sender, e) =>
        {
            await SalvarTreinoAsync();
        };

        botaoEditar.Click += async (sender, e) =>
        {
            await CarregarEdicaoAsync();
        };

        botaoExcluir.Click += async (sender, e) =>
        {
            await ExcluirTreinoAsync();
        };

        botaoCancelarEdicao.Click += (sender, e) =>
        {
            CancelarEdicao();
        };

        botaoAtualizar.Click += async (sender, e) =>
        {
            await AtualizarListaAsync();
        };

        botaoFechar.Click += (sender, e) => Close();

        botoes.Controls.Add(botaoSalvar);
        botoes.Controls.Add(botaoEditar);
        botoes.Controls.Add(botaoExcluir);
        botoes.Controls.Add(botaoCancelarEdicao);
        botoes.Controls.Add(botaoAtualizar);
        botoes.Controls.Add(botaoFechar);

        FlowLayoutPanel filtros = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            WrapContents = true,
            Margin = new Padding(0, 0, 0, 12)
        };

        Label rotuloFiltro = CriarRotulo(
            "Filtrar treinos por usuário:"
        );

        rotuloFiltro.Margin = new Padding(0, 8, 8, 0);

        ConfigurarListaUsuarios(campoFiltroUsuario);
        campoFiltroUsuario.Width = 260;
        campoFiltroUsuario.Margin = new Padding(0, 3, 12, 3);

        ConfigurarBotao(botaoBuscar, "Buscar");
        ConfigurarBotao(botaoMostrarTodos, "Mostrar todos");

        botaoBuscar.Click += async (sender, e) =>
        {
            await BuscarPorUsuarioAsync();
        };

        botaoMostrarTodos.Click += async (sender, e) =>
        {
            await MostrarTodosAsync();
        };

        filtros.Controls.Add(rotuloFiltro);
        filtros.Controls.Add(campoFiltroUsuario);
        filtros.Controls.Add(botaoBuscar);
        filtros.Controls.Add(botaoMostrarTodos);

        tabelaTreinos.Dock = DockStyle.Fill;
        tabelaTreinos.MinimumSize = new Size(0, 140);
        tabelaTreinos.ReadOnly = true;
        tabelaTreinos.AllowUserToAddRows = false;
        tabelaTreinos.AllowUserToDeleteRows = false;
        tabelaTreinos.AutoGenerateColumns = false;
        tabelaTreinos.RowHeadersVisible = false;
        tabelaTreinos.MultiSelect = false;

        tabelaTreinos.SelectionMode =
            DataGridViewSelectionMode.FullRowSelect;

        tabelaTreinos.AutoSizeColumnsMode =
            DataGridViewAutoSizeColumnsMode.Fill;

        tabelaTreinos.BackgroundColor = Color.White;
        tabelaTreinos.Margin = new Padding(0);

        tabelaTreinos.Columns.AddRange(
            new DataGridViewTextBoxColumn
            {
                Name = nameof(Treino.Id),
                DataPropertyName = nameof(Treino.Id),
                HeaderText = "ID treino",
                FillWeight = 12
            },
            new DataGridViewTextBoxColumn
            {
                Name = "Usuario",
                DataPropertyName = "Usuario",
                HeaderText = "Usuário",
                FillWeight = 30
            },
            new DataGridViewTextBoxColumn
            {
                Name = nameof(Treino.Modalidade),
                DataPropertyName = nameof(Treino.Modalidade),
                HeaderText = "Modalidade",
                FillWeight = 26
            },
            new DataGridViewTextBoxColumn
            {
                Name = nameof(Treino.Data),
                DataPropertyName = nameof(Treino.Data),
                HeaderText = "Data",
                FillWeight = 18,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Format = "dd/MM/yyyy",
                    FormatProvider = CultureInfo.InvariantCulture
                }
            },
            new DataGridViewTextBoxColumn
            {
                Name = nameof(Treino.DuracaoMinutos),
                DataPropertyName = nameof(Treino.DuracaoMinutos),
                HeaderText = "Duração (min)",
                FillWeight = 18
            }
        );

        status.Text = "Aguardando carregamento.";
        status.AutoSize = true;
        status.ForeColor = Color.DimGray;
        status.Margin = new Padding(0, 12, 0, 0);

        estrutura.Controls.Add(titulo, 0, 0);
        estrutura.Controls.Add(rotuloModo, 0, 1);
        estrutura.Controls.Add(campos, 0, 2);
        estrutura.Controls.Add(botoes, 0, 3);
        estrutura.Controls.Add(filtros, 0, 4);
        estrutura.Controls.Add(tabelaTreinos, 0, 5);
        estrutura.Controls.Add(status, 0, 6);

        Controls.Add(estrutura);

        Shown += async (sender, e) =>
        {
            await AtualizarListaAsync();

            if (!IsDisposed && !Disposing)
            {
                campoUsuario.Focus();
            }
        };

        FormClosing += (sender, e) =>
        {
            if (e.CloseReason != CloseReason.UserClosing)
            {
                return;
            }

            if (operacaoEmAndamento)
            {
                e.Cancel = true;

                MessageBox.Show(
                    this,
                    "Aguarde a operação terminar para fechar a janela.",
                    "Operação em andamento",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );

                return;
            }

            if (treinoEmEdicao.HasValue)
            {
                DialogResult resposta = MessageBox.Show(
                    this,
                    "Deseja fechar e descartar os dados da edição?",
                    "Edição em andamento",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button2
                );

                e.Cancel = resposta != DialogResult.Yes;
            }
        };

        AtualizarControles();
    }

    private Label CriarRotulo(string texto)
    {
        return new Label
        {
            Text = texto,
            AutoSize = true,
            Anchor = AnchorStyles.Left,
            Margin = new Padding(0, 0, 8, 0)
        };
    }

    private void ConfigurarBotao(Button botao, string texto)
    {
        botao.Text = texto;
        botao.AutoSize = true;
        botao.Padding = new Padding(12, 6, 12, 6);
        botao.Margin = new Padding(0, 0, 12, 0);
    }

    private void ConfigurarListaUsuarios(ComboBox lista)
    {
        lista.DropDownStyle = ComboBoxStyle.DropDownList;
        lista.DisplayMember = nameof(Usuario.Nome);
        lista.ValueMember = nameof(Usuario.Id);
        lista.FormattingEnabled = true;

        lista.Format += (sender, e) =>
        {
            if (e.ListItem is Usuario usuario)
            {
                e.Value = $"{usuario.Id} - {usuario.Nome}";
            }
        };
    }

    private DateTime ObterDataPadrao()
    {
        DateTime hoje = DateTime.Today;

        return hoje > dataLimite ? dataLimite : hoje;
    }

    private void DefinirOcupado(bool ocupado)
    {
        operacaoEmAndamento = ocupado;
        AtualizarControles();
    }

    private void AtualizarControles()
    {
        if (IsDisposed || Disposing)
        {
            return;
        }

        bool editando = treinoEmEdicao.HasValue;

        bool podePreencher =
            !operacaoEmAndamento && usuariosDisponiveis;

        bool podeSelecionar =
            podePreencher
            && !editando
            && tabelaTreinos.Rows.Count > 0;

        campoUsuario.Enabled = podePreencher && !editando;
        campoModalidade.Enabled = podePreencher;
        campoData.Enabled = podePreencher;
        campoDuracao.Enabled = podePreencher;

        botaoSalvar.Enabled = podePreencher;

        botaoSalvar.Text = editando
            ? "Salvar alterações"
            : "Cadastrar treino";

        botaoEditar.Enabled = podeSelecionar;
        botaoExcluir.Enabled = podeSelecionar;

        botaoCancelarEdicao.Enabled =
            !operacaoEmAndamento && editando;

        botaoAtualizar.Enabled =
            !operacaoEmAndamento && !editando;

        campoFiltroUsuario.Enabled =
            podePreencher && !editando;

        botaoBuscar.Enabled =
            podePreencher && !editando;

        botaoMostrarTodos.Enabled =
            !operacaoEmAndamento && !editando;

        botaoFechar.Enabled = !operacaoEmAndamento;

        tabelaTreinos.Enabled =
            !operacaoEmAndamento && !editando;

        rotuloModo.Text = editando
            ? $"Editando treino #{treinoEmEdicao.Value}. O usuário permanece o mesmo."
            : "Novo treino";
    }

    private async Task AtualizarListaAsync()
    {
        if (operacaoEmAndamento
            || treinoEmEdicao.HasValue
            || IsDisposed
            || Disposing)
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

    private async Task BuscarPorUsuarioAsync()
    {
        if (operacaoEmAndamento
            || treinoEmEdicao.HasValue
            || IsDisposed
            || Disposing)
        {
            return;
        }

        if (campoFiltroUsuario.SelectedItem is not Usuario usuario)
        {
            MostrarValidacao(
                "Selecione um usuário na lista do filtro.",
                campoFiltroUsuario
            );

            return;
        }

        usuarioFiltrado = usuario.Id;

        await AtualizarListaAsync();
    }

    private async Task MostrarTodosAsync()
    {
        if (operacaoEmAndamento
            || treinoEmEdicao.HasValue
            || IsDisposed
            || Disposing)
        {
            return;
        }

        usuarioFiltrado = null;
        campoFiltroUsuario.SelectedIndex = -1;

        await AtualizarListaAsync();
    }

    private async Task CarregarDadosAsync()
    {
        try
        {
            status.Text = "Carregando usuários e treinos...";

            int? usuarioSelecionado =
                (campoUsuario.SelectedItem as Usuario)?.Id;

            int? filtroAtual = usuarioFiltrado;

            var usuarios = await Task.Run(
                () => usuarioRepositorio.Listar()
            );

            List<Treino> treinos;

            if (filtroAtual.HasValue)
            {
                int usuarioId = filtroAtual.Value;

                treinos = await Task.Run(
                    () => treinoRepositorio.BuscarPorUsuario(usuarioId)
                );
            }
            else
            {
                treinos = await Task.Run(
                    () => treinoRepositorio.Listar()
                );
            }

            if (IsDisposed || Disposing)
            {
                return;
            }

            campoUsuario.DisplayMember = nameof(Usuario.Nome);
            campoUsuario.ValueMember = nameof(Usuario.Id);
            campoUsuario.DataSource = usuarios;
            campoUsuario.SelectedIndex = -1;

            if (usuarioSelecionado.HasValue)
            {
                campoUsuario.SelectedValue = usuarioSelecionado.Value;
            }

            // Listas separadas mantêm as duas seleções independentes.
            campoFiltroUsuario.DisplayMember = nameof(Usuario.Nome);
            campoFiltroUsuario.ValueMember = nameof(Usuario.Id);
            campoFiltroUsuario.DataSource = usuarios.ToList();
            campoFiltroUsuario.SelectedIndex = -1;

            if (filtroAtual.HasValue)
            {
                campoFiltroUsuario.SelectedValue = filtroAtual.Value;
            }

            var dados = treinos.Select(t => new
            {
                t.Id,

                Usuario = $"{t.UsuarioId} - " +
                    (usuarios.FirstOrDefault(
                        u => u.Id == t.UsuarioId
                    )?.Nome ?? "Usuário não encontrado"),

                t.Modalidade,
                t.Data,
                t.DuracaoMinutos
            }).ToList();

            tabelaTreinos.DataSource = dados;
            tabelaTreinos.ClearSelection();
            tabelaTreinos.CurrentCell = null;

            usuariosDisponiveis = usuarios.Count > 0;

            if (!usuariosDisponiveis)
            {
                status.Text =
                    "Cadastre um usuário pela tela Usuários para registrar treinos.";
            }
            else if (filtroAtual.HasValue)
            {
                int usuarioId = filtroAtual.Value;

                bool usuarioExiste = usuarios.Any(
                    u => u.Id == usuarioId
                );

                if (!usuarioExiste)
                {
                    status.Text =
                        "Usuário do filtro não encontrado. Escolha outro ou clique em Mostrar todos.";
                }
                else if (treinos.Count == 0)
                {
                    status.Text =
                        $"Nenhum treino encontrado para o usuário #{usuarioId}.";
                }
                else
                {
                    status.Text =
                        $"Usuário #{usuarioId} | Treinos encontrados: {treinos.Count}.";
                }
            }
            else
            {
                status.Text =
                    $"Todos os usuários | Treinos cadastrados: {treinos.Count}.";
            }
        }
        catch (Exception erro)
        {
            usuariosDisponiveis = false;

            MostrarErro(
                "Não foi possível carregar os dados.",
                erro
            );
        }
    }

    private int? ObterTreinoSelecionado()
    {
        if (tabelaTreinos.SelectedRows.Count == 0)
        {
            MostrarValidacao(
                "Selecione um treino na tabela.",
                tabelaTreinos
            );

            return null;
        }

        object? valor = tabelaTreinos.SelectedRows[0]
            .Cells[nameof(Treino.Id)].Value;

        if (valor is int treinoId && treinoId > 0)
        {
            return treinoId;
        }

        MostrarValidacao(
            "Não foi possível identificar o treino selecionado.",
            tabelaTreinos
        );

        return null;
    }

    private async Task CarregarEdicaoAsync()
    {
        if (operacaoEmAndamento
            || treinoEmEdicao.HasValue
            || IsDisposed
            || Disposing)
        {
            return;
        }

        int? selecionado = ObterTreinoSelecionado();

        if (!selecionado.HasValue)
        {
            return;
        }

        int treinoId = selecionado.Value;

        DefinirOcupado(true);
        status.Text = "Carregando treino para edição...";

        try
        {
            Treino? treino = await Task.Run(
                () => treinoRepositorio.BuscarPorId(treinoId)
            );

            if (IsDisposed || Disposing)
            {
                return;
            }

            if (treino == null)
            {
                Informar(
                    "Treino não encontrado. A lista será atualizada."
                );

                await CarregarDadosAsync();
                return;
            }

            bool usuarioNaLista = campoUsuario.Items
                .Cast<Usuario>()
                .Any(u => u.Id == treino.UsuarioId);

            if (!usuarioNaLista)
            {
                await CarregarDadosAsync();

                if (!usuariosDisponiveis || IsDisposed || Disposing)
                {
                    return;
                }
            }

            campoUsuario.SelectedValue = treino.UsuarioId;

            if (campoUsuario.SelectedItem is not Usuario usuario
                || usuario.Id != treino.UsuarioId)
            {
                MostrarValidacao(
                    "Não foi possível carregar o usuário deste treino.",
                    campoUsuario
                );

                return;
            }

            campoModalidade.Text = treino.Modalidade;

            campoData.Text = treino.Data.ToString(
                "dd/MM/yyyy",
                CultureInfo.InvariantCulture
            );

            campoDuracao.Value = treino.DuracaoMinutos;

            treinoEmEdicao = treino.Id;

            status.Text =
                "Altere os campos e clique em Salvar alterações.";
        }
        catch (Exception erro)
        {
            LimparFormulario();

            MostrarErro(
                "Não foi possível carregar o treino para edição.",
                erro
            );
        }
        finally
        {
            DefinirOcupado(false);

            if (!IsDisposed
                && !Disposing
                && treinoEmEdicao.HasValue)
            {
                campoModalidade.Focus();
                campoModalidade.SelectAll();
            }
        }
    }

    private async Task ExcluirTreinoAsync()
    {
        if (operacaoEmAndamento
            || treinoEmEdicao.HasValue
            || IsDisposed
            || Disposing)
        {
            return;
        }

        int? selecionado = ObterTreinoSelecionado();

        if (!selecionado.HasValue)
        {
            return;
        }

        int treinoId = selecionado.Value;

        DefinirOcupado(true);
        status.Text = "Carregando treino para confirmação...";

        try
        {
            Treino? treino = await Task.Run(
                () => treinoRepositorio.BuscarPorId(treinoId)
            );

            if (IsDisposed || Disposing)
            {
                return;
            }

            if (treino == null)
            {
                Informar(
                    "Treino não encontrado. A lista será atualizada."
                );

                await CarregarDadosAsync();
                return;
            }

            string nomeUsuario = campoUsuario.Items
                .Cast<Usuario>()
                .FirstOrDefault(u => u.Id == treino.UsuarioId)
                ?.Nome ?? "Nome indisponível";

            string mensagem =
                $"Confirma a exclusão do treino #{treino.Id}?" +
                Environment.NewLine +
                Environment.NewLine +
                $"Usuário: {treino.UsuarioId} - {nomeUsuario}" +
                Environment.NewLine +
                $"Modalidade: {treino.Modalidade}" +
                Environment.NewLine +
                $"Data: {treino.Data:dd/MM/yyyy}" +
                Environment.NewLine +
                $"Duração: {treino.DuracaoMinutos} minutos";

            DialogResult resposta = MessageBox.Show(
                this,
                mensagem,
                "Confirmar exclusão",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button2
            );

            if (resposta != DialogResult.Yes)
            {
                status.Text = "Exclusão cancelada.";
                return;
            }

            status.Text = "Excluindo treino...";

            bool excluido = await Task.Run(
                () => treinoRepositorio.Excluir(treinoId)
            );

            if (IsDisposed || Disposing)
            {
                return;
            }

            Informar(
                excluido
                    ? "Treino excluído com sucesso!"
                    : "Treino não encontrado. A lista será atualizada."
            );

            await CarregarDadosAsync();
        }
        catch (Exception erro)
        {
            MostrarErro(
                "Não foi possível excluir o treino.",
                erro
            );
        }
        finally
        {
            DefinirOcupado(false);
        }
    }

    private bool LerDadosTreino(
        out int usuarioId,
        out string modalidade,
        out DateTime data,
        out int duracaoMinutos)
    {
        usuarioId = 0;
        modalidade = string.Empty;
        data = DateTime.MinValue;
        duracaoMinutos = 0;

        if (campoUsuario.SelectedItem is not Usuario usuario)
        {
            MostrarValidacao(
                "Selecione um usuário.",
                campoUsuario
            );

            return false;
        }

        usuarioId = usuario.Id;
        modalidade = campoModalidade.Text.Trim();

        if (string.IsNullOrWhiteSpace(modalidade))
        {
            MostrarValidacao(
                "A modalidade é obrigatória.",
                campoModalidade
            );

            return false;
        }

        if (modalidade.Length > 50)
        {
            MostrarValidacao(
                "A modalidade deve ter no máximo 50 caracteres.",
                campoModalidade
            );

            return false;
        }

        if (modalidade.Any(char.IsDigit))
        {
            MostrarValidacao(
                "A modalidade não pode conter números.",
                campoModalidade
            );

            return false;
        }

        if (!campoData.MaskCompleted
            || !DateTime.TryParseExact(
                campoData.Text,
                "dd/MM/yyyy",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out data)
            || data.Year < 1000
            || data.Date > dataLimite)
        {
            MostrarValidacao(
                "Informe uma data válida entre 01/01/1000 e 14/12/2026.",
                campoData
            );

            return false;
        }

        duracaoMinutos = (int)campoDuracao.Value;

        if (duracaoMinutos <= 0)
        {
            MostrarValidacao(
                "A duração deve ser maior que zero.",
                campoDuracao
            );

            return false;
        }

        return true;
    }

    private async Task SalvarTreinoAsync()
    {
        if (operacaoEmAndamento || IsDisposed || Disposing)
        {
            return;
        }

        if (!LerDadosTreino(
                out int usuarioId,
                out string modalidade,
                out DateTime data,
                out int duracaoMinutos))
        {
            return;
        }

        int? idDaEdicao = treinoEmEdicao;

        DefinirOcupado(true);
        status.Text = "Salvando treino...";

        try
        {
            string mensagem;

            if (idDaEdicao.HasValue)
            {
                int treinoId = idDaEdicao.Value;

                bool atualizado = await Task.Run(
                    () => treinoRepositorio.Atualizar(
                        treinoId,
                        modalidade,
                        data,
                        duracaoMinutos
                    )
                );

                if (IsDisposed || Disposing)
                {
                    return;
                }

                if (!atualizado)
                {
                    Informar(
                        "Treino não encontrado. A lista será atualizada."
                    );

                    LimparFormulario();
                    await CarregarDadosAsync();
                    return;
                }

                mensagem = "Treino atualizado com sucesso!";
            }
            else
            {
                Treino novoTreino = new Treino(
                    usuarioId,
                    modalidade,
                    data,
                    duracaoMinutos
                );

                await Task.Run(
                    () => treinoRepositorio.Cadastrar(novoTreino)
                );

                if (IsDisposed || Disposing)
                {
                    return;
                }

                mensagem = "Treino cadastrado com sucesso!" +
                    Environment.NewLine +
                    $"Identificador: {novoTreino.Id}";
            }

            if (usuarioFiltrado.HasValue
                && usuarioFiltrado.Value != usuarioId)
            {
                usuarioFiltrado = null;

                mensagem += Environment.NewLine +
                    "A lista será exibida sem filtro para mostrar o treino.";
            }

            Informar(mensagem);

            LimparFormulario();
            await CarregarDadosAsync();
        }
        catch (ArgumentException erro)
        {
            MostrarValidacao(
                erro.Message,
                campoModalidade
            );
        }
        catch (Exception erro)
        {
            MostrarErro(
                "Não foi possível salvar o treino.",
                erro
            );
        }
        finally
        {
            DefinirOcupado(false);

            if (!IsDisposed && !Disposing)
            {
                campoModalidade.Focus();
            }
        }
    }

    private void CancelarEdicao()
    {
        if (operacaoEmAndamento || !treinoEmEdicao.HasValue)
        {
            return;
        }

        LimparFormulario();
        AtualizarControles();

        status.Text = "Edição cancelada.";
    }

    private void LimparFormulario()
    {
        treinoEmEdicao = null;

        if (IsDisposed || Disposing)
        {
            return;
        }

        campoModalidade.Clear();
        campoDuracao.Value = 1;

        campoData.Text = ObterDataPadrao().ToString(
            "dd/MM/yyyy",
            CultureInfo.InvariantCulture
        );

        tabelaTreinos.ClearSelection();
        tabelaTreinos.CurrentCell = null;
    }

    private void Informar(string mensagem)
    {
        if (IsDisposed || Disposing)
        {
            return;
        }

        MessageBox.Show(
            this,
            mensagem,
            "Treinos",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information
        );
    }

    private void MostrarValidacao(string mensagem, Control campo)
    {
        if (IsDisposed || Disposing)
        {
            return;
        }

        MessageBox.Show(
            this,
            mensagem,
            "Dados inválidos",
            MessageBoxButtons.OK,
            MessageBoxIcon.Warning
        );

        campo.Focus();

        if (campo is TextBoxBase texto)
        {
            texto.SelectAll();
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