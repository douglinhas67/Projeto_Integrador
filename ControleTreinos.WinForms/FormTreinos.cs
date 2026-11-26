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
    private readonly TextBox campoDuracao = new TextBox();

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
        AutoScaleMode = AutoScaleMode.Dpi;
        ClientSize = new Size(1120, 720);
        MinimumSize = new Size(900, 600);

        var pagina = new LayoutPagina("Treinos",
            "Registre atividades e mantenha o histórico organizado.");

        ConfigurarListaUsuarios(campoUsuario);
        campoModalidade.MaxLength = 50;
        campoModalidade.PlaceholderText = "Ex.: musculação";
        campoData.Culture = CultureInfo.GetCultureInfo("pt-BR");
        campoData.Mask = "00/00/0000";
        campoData.TextMaskFormat = MaskFormat.IncludeLiterals;
        campoData.Text = ObterDataPadrao().ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
        campoDuracao.Text = "1";

        Componentes.Botao(botaoSalvar, "Cadastrar treino");
        Componentes.Botao(botaoEditar, "Editar selecionado");
        Componentes.Botao(botaoExcluir, "Excluir selecionado");
        Componentes.Botao(botaoCancelarEdicao, "Cancelar edição");
        Componentes.Botao(botaoAtualizar, "Atualizar lista");
        Componentes.Botao(botaoFechar, "Fechar");
        Componentes.Botao(botaoBuscar, "Buscar");
        Componentes.Botao(botaoMostrarTodos, "Mostrar todos");
        botaoBuscar.Tag = EstiloBotao.Primario;
        Tema.PrepararBotao(botaoBuscar);

        botaoSalvar.Click += async (_, _) => await SalvarTreinoAsync();
        botaoEditar.Click += async (_, _) => await CarregarEdicaoAsync();
        botaoExcluir.Click += async (_, _) => await ExcluirTreinoAsync();
        botaoCancelarEdicao.Click += (_, _) => CancelarEdicao();
        botaoAtualizar.Click += async (_, _) => await AtualizarListaAsync();
        botaoFechar.Click += (_, _) => Close();
        botaoBuscar.Click += async (_, _) => await BuscarPorUsuarioAsync();
        botaoMostrarTodos.Click += async (_, _) => await MostrarTodosAsync();
        pagina.Acoes.Controls.Add(botaoAtualizar);
        pagina.Acoes.Controls.Add(botaoFechar);

        ConfigurarListaUsuarios(campoFiltroUsuario);
        var filtro = Componentes.Coluna();
        filtro.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        filtro.Controls.Add(Componentes.Campo("Filtrar por usuário", campoFiltroUsuario));
        var acoesFiltro = Componentes.BarraAcoes();
        acoesFiltro.Controls.Add(botaoBuscar);
        acoesFiltro.Controls.Add(botaoMostrarTodos);
        acoesFiltro.Margin = new Padding(0, 0, 0, 12);
        filtro.Controls.Add(acoesFiltro);

        tabelaTreinos.ReadOnly = true;
        tabelaTreinos.AllowUserToAddRows = false;
        tabelaTreinos.AllowUserToDeleteRows = false;
        tabelaTreinos.AutoGenerateColumns = false;
        tabelaTreinos.RowHeadersVisible = false;
        tabelaTreinos.MultiSelect = false;
        tabelaTreinos.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        tabelaTreinos.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        tabelaTreinos.AccessibleName = "Histórico de treinos";
        tabelaTreinos.Columns.AddRange(
            new DataGridViewTextBoxColumn { Name = nameof(Treino.Id), DataPropertyName = nameof(Treino.Id), HeaderText = "ID", FillWeight = 10, MinimumWidth = 45 },
            new DataGridViewTextBoxColumn { Name = "Usuario", DataPropertyName = "Usuario", HeaderText = "Usuário", FillWeight = 28, MinimumWidth = 100 },
            new DataGridViewTextBoxColumn { Name = nameof(Treino.Modalidade), DataPropertyName = nameof(Treino.Modalidade), HeaderText = "Modalidade", FillWeight = 27, MinimumWidth = 100 },
            new DataGridViewTextBoxColumn { Name = nameof(Treino.Data), DataPropertyName = nameof(Treino.Data), HeaderText = "Data", FillWeight = 22, MinimumWidth = 104,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "dd/MM/yyyy", FormatProvider = CultureInfo.InvariantCulture } },
            new DataGridViewTextBoxColumn { Name = nameof(Treino.DuracaoMinutos), DataPropertyName = nameof(Treino.DuracaoMinutos), HeaderText = "Minutos", FillWeight = 15, MinimumWidth = 78,
                DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleRight, Format = "N0", FormatProvider = CultureInfo.GetCultureInfo("pt-BR") } }
        );
        tabelaTreinos.SelectionChanged += (_, _) => AtualizarControles();
        tabelaTreinos.CellDoubleClick += async (_, e) =>
        {
            if (e.RowIndex >= 0) await CarregarEdicaoAsync();
        };
        var acoesLista = Componentes.BarraAcoes();
        acoesLista.Controls.Add(botaoEditar);
        acoesLista.Controls.Add(botaoExcluir);
        var lista = Componentes.Lista("Histórico de treinos", tabelaTreinos, filtro, acoesLista);

        var acoesEditor = Componentes.Coluna();
        acoesEditor.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        botaoSalvar.Dock = DockStyle.Fill;
        botaoCancelarEdicao.Dock = DockStyle.Fill;
        acoesEditor.Controls.Add(botaoSalvar);
        acoesEditor.Controls.Add(botaoCancelarEdicao);
        var nota = Componentes.Rotulo("Cada treino pertence a um usuário cadastrado.", Tema.TextoSecundario);
        nota.Margin = new Padding(0, 0, 0, 18);
        var editor = Componentes.Editor(rotuloModo,
            Componentes.Campo("Usuário", campoUsuario),
            Componentes.Campo("Modalidade", campoModalidade),
            Componentes.Campo("Data do treino", campoData, "De 01/01/1000 até 14/12/2026"),
            Componentes.Campo("Duração em minutos", campoDuracao),
            nota, acoesEditor);

        pagina.Conteudo(Componentes.Dividir(lista, editor));
        status.Text = "Carregando histórico...";
        pagina.Rodape(status);
        Controls.Add(pagina);
        Tema.Aplicar(this);

        Shown += async (sender, e) =>
        {
            botaoFechar.Visible = TopLevel;
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
            && tabelaTreinos.SelectedRows.Count > 0;

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

        botaoCancelarEdicao.Visible = editando;
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

        rotuloModo.Text = treinoEmEdicao is int id
            ? $"Editar treino #{id}"
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

            campoDuracao.Text = treino.DuracaoMinutos.ToString(CultureInfo.InvariantCulture);

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

        if (!ValidacaoNumerica.TentarLerInteiroPositivo(campoDuracao.Text, out duracaoMinutos))
        {
            MostrarValidacao(
                "A duração deve ser um número inteiro de minutos maior que zero, sem casas decimais.",
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
        campoDuracao.Text = "1";

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
