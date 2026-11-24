using ControleTreinos.Data;
using System.Globalization;

namespace ControleTreinos.WinForms;

public class FormVisaoGeral : Form
{
    private readonly Indicador usuarios = new("Usuários cadastrados");
    private readonly Indicador treinos = new("Treinos registrados");
    private readonly Indicador minutos = new("Minutos de atividade");
    private readonly DataGridView tabela = new();
    private readonly Label status = new();
    private readonly Button atualizar = new();
    private bool carregando;

    public FormVisaoGeral()
    {
        Text = "Visão geral";
        AutoScaleMode = AutoScaleMode.Dpi;
        var pagina = new LayoutPagina("Visão geral",
            "Seu histórico de atividades, em um só lugar.");
        Componentes.Botao(atualizar, "Atualizar visão geral");
        atualizar.Click += async (_, _) => await CarregarAsync();
        pagina.Acoes.Controls.Add(atualizar);

        var conteudo = new TableLayoutPanel
        {
            Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2,
            Margin = new Padding(0)
        };
        conteudo.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        conteudo.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        conteudo.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        conteudo.Controls.Add(Componentes.Indicadores(usuarios, treinos, minutos), 0, 0);

        tabela.ReadOnly = true;
        tabela.AllowUserToAddRows = false;
        tabela.AllowUserToDeleteRows = false;
        tabela.RowHeadersVisible = false;
        tabela.AutoGenerateColumns = false;
        tabela.MultiSelect = false;
        tabela.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        tabela.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        tabela.AccessibleName = "Treinos mais recentes";
        tabela.Columns.AddRange(
            new DataGridViewTextBoxColumn { DataPropertyName = "Usuario", HeaderText = "Usuário", FillWeight = 30 },
            new DataGridViewTextBoxColumn { DataPropertyName = "Modalidade", HeaderText = "Modalidade", FillWeight = 30 },
            new DataGridViewTextBoxColumn { DataPropertyName = "Data", HeaderText = "Data", FillWeight = 22,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "dd/MM/yyyy", FormatProvider = CultureInfo.InvariantCulture } },
            new DataGridViewTextBoxColumn { DataPropertyName = "DuracaoMinutos", HeaderText = "Minutos", FillWeight = 18,
                DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleRight, Format = "N0", FormatProvider = CultureInfo.GetCultureInfo("pt-BR") } }
        );
        var nota = Componentes.Rotulo("Até 10 atividades, ordenadas pela data do treino.", Tema.TextoSecundario);
        conteudo.Controls.Add(Componentes.Lista("Atividades recentes", tabela, acoes: nota), 0, 1);
        pagina.Conteudo(conteudo);
        pagina.Rodape(status);
        Controls.Add(pagina);
        Tema.Aplicar(this);
        Shown += async (_, _) => await CarregarAsync();
    }

    private async Task CarregarAsync()
    {
        if (carregando || IsDisposed || Disposing) return;
        carregando = true;
        atualizar.Enabled = false;
        status.Text = "Carregando atividades...";
        try
        {
            var dados = await Task.Run(() =>
            {
                var pessoas = new UsuarioRepositorio().Listar();
                var atividades = new TreinoRepositorio().Listar();
                var nomes = pessoas.ToDictionary(u => u.Id, u => u.Nome);
                return new
                {
                    Pessoas = pessoas.Count,
                    Treinos = atividades.Count,
                    Minutos = atividades.Sum(t => (long)t.DuracaoMinutos),
                    Recentes = atividades.OrderByDescending(t => t.Data).ThenByDescending(t => t.Id)
                        .Take(10).Select(t => new
                        {
                            Usuario = nomes.GetValueOrDefault(t.UsuarioId, "Usuário não encontrado"),
                            t.Modalidade, t.Data, t.DuracaoMinutos
                        }).ToList()
                };
            });
            if (IsDisposed || Disposing) return;
            usuarios.Definir(dados.Pessoas);
            treinos.Definir(dados.Treinos);
            minutos.Definir(dados.Minutos);
            tabela.DataSource = dados.Recentes;
            tabela.ClearSelection();
            tabela.CurrentCell = null;
            status.Text = dados.Treinos == 0
                ? "Nenhum treino registrado. Comece cadastrando usuários e treinos pelo menu lateral."
                : "Dados atualizados. Use o menu lateral para cadastrar, consultar e editar.";
        }
        catch (Exception erro)
        {
            if (IsDisposed || Disposing) return;
            status.Text = "Não foi possível carregar a visão geral. Tente atualizar novamente.";
            MessageBox.Show(this, status.Text + Environment.NewLine + erro.GetBaseException().Message,
                "Visão geral", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        finally
        {
            carregando = false;
            if (!IsDisposed && !Disposing) atualizar.Enabled = true;
        }
    }
}
