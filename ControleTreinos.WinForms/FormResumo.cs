using System;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using ControleTreinos.Data;

namespace ControleTreinos.WinForms;

public class FormResumo : Form
{
    private readonly UsuarioRepositorio usuarioRepositorio =
        new UsuarioRepositorio();

    private readonly TreinoRepositorio treinoRepositorio =
        new TreinoRepositorio();

    private readonly Button botaoAtualizar = new Button();
    private readonly Button botaoFechar = new Button();

    private readonly DataGridView tabelaResumo = new DataGridView();
    private readonly Label status = new Label();

    private bool operacaoEmAndamento;
    private readonly Indicador totalUsuarios = new("Usuários no resumo");
    private readonly Indicador totalTreinos = new("Treinos registrados");
    private readonly Indicador totalMinutos = new("Minutos de atividade");

    public FormResumo()
    {
        Text = "Resumo de atividades - Controle de Treinos";
        StartPosition = FormStartPosition.CenterParent;
        AutoScaleMode = AutoScaleMode.Dpi;
        ClientSize = new Size(1120, 700);
        MinimumSize = new Size(800, 560);
        var pagina = new LayoutPagina("Resumo de atividades",
            "Consulte a quantidade de treinos e o tempo registrado por pessoa.");
        Componentes.Botao(botaoAtualizar, "Atualizar resumo");
        Componentes.Botao(botaoFechar, "Fechar");
        botaoAtualizar.Click += async (_, _) => await AtualizarResumoAsync();
        botaoFechar.Click += (_, _) => Close();
        pagina.Acoes.Controls.Add(botaoAtualizar);
        pagina.Acoes.Controls.Add(botaoFechar);

        tabelaResumo.ReadOnly = true;
        tabelaResumo.AllowUserToAddRows = false;
        tabelaResumo.AllowUserToDeleteRows = false;
        tabelaResumo.AutoGenerateColumns = false;
        tabelaResumo.RowHeadersVisible = false;
        tabelaResumo.MultiSelect = false;
        tabelaResumo.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        tabelaResumo.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        tabelaResumo.AccessibleName = "Resumo de atividades por usuário";
        tabelaResumo.Columns.AddRange(
            new DataGridViewTextBoxColumn
            {
                Name = "Id",
                DataPropertyName = "Id",
                HeaderText = "ID usuário",
                FillWeight = 15
            },
            new DataGridViewTextBoxColumn
            {
                Name = "Nome",
                DataPropertyName = "Nome",
                HeaderText = "Nome",
                FillWeight = 45
            },
            new DataGridViewTextBoxColumn
            {
                Name = "QuantidadeTreinos",
                DataPropertyName = "QuantidadeTreinos",
                HeaderText = "Treinos",
                FillWeight = 18,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Alignment =
                        DataGridViewContentAlignment.MiddleRight,
                    Format = "N0",
                    FormatProvider =
                        CultureInfo.GetCultureInfo("pt-BR")
                }
            },
            new DataGridViewTextBoxColumn
            {
                Name = "TotalMinutos",
                DataPropertyName = "TotalMinutos",
                HeaderText = "Total (minutos)",
                FillWeight = 22,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Alignment =
                        DataGridViewContentAlignment.MiddleRight,
                    Format = "N0",
                    FormatProvider =
                        CultureInfo.GetCultureInfo("pt-BR")
                }
            }
        );

        var conteudo = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, Margin = new Padding(0) };
        conteudo.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        conteudo.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        conteudo.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        conteudo.Controls.Add(Componentes.Indicadores(totalUsuarios, totalTreinos, totalMinutos), 0, 0);
        var nota = Componentes.Rotulo("Inclui usuários que ainda não possuem treinos.", Tema.TextoSecundario);
        conteudo.Controls.Add(Componentes.Lista("Atividades por usuário", tabelaResumo, acoes: nota), 0, 1);
        pagina.Conteudo(conteudo);
        status.Text = "Carregando resumo...";
        pagina.Rodape(status);
        Controls.Add(pagina);
        Tema.Aplicar(this);
        Shown += async (_, _) =>
        {
            botaoFechar.Visible = TopLevel;
            await AtualizarResumoAsync();
        };
    }

    private void DefinirOcupado(bool ocupado)
    {
        operacaoEmAndamento = ocupado;

        if (IsDisposed || Disposing)
        {
            return;
        }

        botaoAtualizar.Enabled = !ocupado;
        tabelaResumo.Enabled = !ocupado;
    }

    private async Task AtualizarResumoAsync()
    {
        if (operacaoEmAndamento || IsDisposed || Disposing)
        {
            return;
        }

        DefinirOcupado(true);
        status.Text = "Carregando resumo...";

        try
        {
            var dados = await Task.Run(() =>
            {
                var usuarios = usuarioRepositorio.Listar();
                var treinos = treinoRepositorio.Listar();

                return usuarios.Select(usuario =>
                {
                    var treinosDoUsuario = treinos
                        .Where(t => t.UsuarioId == usuario.Id)
                        .ToList();

                    return new
                    {
                        usuario.Id,
                        usuario.Nome,

                        QuantidadeTreinos = treinosDoUsuario.Count,

                        TotalMinutos = treinosDoUsuario.Sum(
                            t => (long)t.DuracaoMinutos
                        )
                    };
                }).ToList();
            });

            if (IsDisposed || Disposing)
            {
                return;
            }

            totalUsuarios.Definir(dados.Count);
            totalTreinos.Definir(dados.Sum(d => (long)d.QuantidadeTreinos));
            totalMinutos.Definir(dados.Sum(d => d.TotalMinutos));
            tabelaResumo.DataSource = dados;
            tabelaResumo.ClearSelection();
            tabelaResumo.CurrentCell = null;

            status.Text = dados.Count == 0
                ? "Nenhum usuário cadastrado."
                : $"Usuários no resumo: {dados.Count}.";
        }
        catch (Exception erro)
        {
            if (IsDisposed || Disposing)
            {
                return;
            }

            status.Text = "Não foi possível carregar o resumo.";

            MessageBox.Show(
                this,
                "Não foi possível carregar o resumo." +
                Environment.NewLine +
                erro.GetBaseException().Message,
                "Resumo de atividades",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning
            );
        }
        finally
        {
            DefinirOcupado(false);
        }
    }
}
