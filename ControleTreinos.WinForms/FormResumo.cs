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

    public FormResumo()
    {
        Text = "Resumo de atividades - Controle de Treinos";
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(840, 500);
        MinimumSize = new Size(700, 420);

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
            Text = "Resumo de atividades",
            AutoSize = true,
            Font = new Font("Segoe UI", 20F, FontStyle.Bold),
            ForeColor = Tema.AzulEscuro,
            Margin = new Padding(0, 0, 0, 8)
        };

        Label descricao = new Label
        {
            Text = "Quantidade de treinos e total de minutos por usuário.",
            AutoSize = true,
            ForeColor = Color.DimGray,
            Margin = new Padding(0, 0, 0, 16)
        };

        FlowLayoutPanel botoes = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            WrapContents = true,
            Margin = new Padding(0, 0, 0, 12)
        };

        botaoAtualizar.Text = "Atualizar resumo";
        botaoAtualizar.AutoSize = true;
        botaoAtualizar.Margin = new Padding(0, 0, 12, 0);

        botaoAtualizar.Click += async (sender, e) =>
        {
            await AtualizarResumoAsync();
        };

        botaoFechar.Text = "Fechar";
        botaoFechar.AutoSize = true;
        botaoFechar.Margin = new Padding(0);

        botaoFechar.Click += (sender, e) => Close();

        botoes.Controls.Add(botaoAtualizar);
        botoes.Controls.Add(botaoFechar);

        tabelaResumo.Dock = DockStyle.Fill;
        tabelaResumo.ReadOnly = true;
        tabelaResumo.AllowUserToAddRows = false;
        tabelaResumo.AllowUserToDeleteRows = false;
        tabelaResumo.AutoGenerateColumns = false;
        tabelaResumo.RowHeadersVisible = false;
        tabelaResumo.MultiSelect = false;

        tabelaResumo.SelectionMode =
            DataGridViewSelectionMode.FullRowSelect;

        tabelaResumo.AutoSizeColumnsMode =
            DataGridViewAutoSizeColumnsMode.Fill;

        tabelaResumo.Margin = new Padding(0);

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

        status.Text = "Aguardando carregamento.";
        status.AutoSize = true;
        status.ForeColor = Color.DimGray;
        status.Margin = new Padding(0, 12, 0, 0);

        estrutura.Controls.Add(titulo, 0, 0);
        estrutura.Controls.Add(descricao, 0, 1);
        estrutura.Controls.Add(botoes, 0, 2);
        estrutura.Controls.Add(tabelaResumo, 0, 3);
        estrutura.Controls.Add(status, 0, 4);

        Controls.Add(estrutura);

        Shown += async (sender, e) =>
        {
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