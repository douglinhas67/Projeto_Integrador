using System.Runtime.CompilerServices;

namespace ControleTreinos.WinForms;

public enum EstiloBotao { Primario, Secundario, Perigo }

public static class Tema
{
    public static readonly Color Fundo = Color.FromArgb(244, 246, 248);
    public static readonly Color Superficie = Color.White;
    public static readonly Color AzulEscuro = Color.FromArgb(23, 35, 49);
    public static readonly Color Azul = Color.FromArgb(42, 68, 91);
    public static readonly Color Verde = Color.FromArgb(49, 110, 55);
    public static readonly Color Destaque = Color.FromArgb(200, 243, 106);
    public static readonly Color TextoDestaque = Color.FromArgb(32, 52, 13);
    public static readonly Color Vermelho = Color.FromArgb(175, 48, 56);
    public static readonly Color Texto = Color.FromArgb(24, 35, 51);
    public static readonly Color TextoSecundario = Color.FromArgb(101, 113, 130);
    public static readonly Color Borda = Color.FromArgb(229, 233, 239);
    public static readonly Color Suave = Color.FromArgb(241, 244, 247);

    public static readonly Font FontePadrao = new("Segoe UI", 10F);
    public static readonly Font FonteBotoes = new("Segoe UI", 9F, FontStyle.Bold);
    public static readonly Font FonteTitulo = new("Segoe UI", 24F, FontStyle.Bold);
    public static readonly Font FonteSecao = new("Segoe UI", 12F, FontStyle.Bold);
    public static readonly Font FontePequena = new("Segoe UI", 9F);
    public static readonly Font FonteNumero = new("Segoe UI", 25F, FontStyle.Bold);

    private static readonly ConditionalWeakTable<Button, object> BotoesPreparados = new();

    public static void Aplicar(Form janela)
    {
        janela.SuspendLayout();
        janela.Font = FontePadrao;
        janela.BackColor = Fundo;
        janela.ForeColor = Texto;
        AplicarNosControles(janela.Controls);
        janela.ResumeLayout(true);
    }

    private static void AplicarNosControles(Control.ControlCollection controles)
    {
        foreach (Control controle in controles)
        {
            switch (controle)
            {
                case Button botao:
                    PrepararBotao(botao);
                    break;
                case DataGridView tabela:
                    PrepararTabela(tabela);
                    break;
                case NumericUpDown numero:
                    numero.BorderStyle = BorderStyle.FixedSingle;
                    numero.BackColor = Superficie;
                    numero.ForeColor = Texto;
                    break;
                case ComboBox lista:
                    lista.FlatStyle = FlatStyle.Flat;
                    lista.BackColor = Superficie;
                    lista.ForeColor = Texto;
                    lista.IntegralHeight = false;
                    lista.DropDownHeight = 240;
                    break;
                case TextBoxBase campo:
                    campo.BorderStyle = BorderStyle.FixedSingle;
                    campo.BackColor = Superficie;
                    campo.ForeColor = Texto;
                    break;
                default:
                    if (controle.HasChildren) AplicarNosControles(controle.Controls);
                    break;
            }
        }
    }

    public static void PrepararBotao(Button botao)
    {
        botao.FlatStyle = FlatStyle.Flat;
        botao.UseVisualStyleBackColor = false;
        botao.FlatAppearance.BorderSize = 1;
        botao.Font = FonteBotoes;
        botao.MinimumSize = new Size(0, 38);
        botao.Padding = new Padding(12, 7, 12, 7);
        AtualizarCorBotao(botao);

        if (BotoesPreparados.TryGetValue(botao, out _)) return;
        BotoesPreparados.Add(botao, new object());
        botao.EnabledChanged += (_, _) => AtualizarCorBotao(botao);
        botao.TextChanged += (_, _) => AtualizarCorBotao(botao);
    }

    private static void AtualizarCorBotao(Button botao)
    {
        if (botao.IsDisposed) return;
        string texto = botao.Text.Trim().ToLowerInvariant();
        EstiloBotao estilo = botao.Tag is EstiloBotao definido ? definido
            : texto.StartsWith("cadastrar") || texto.StartsWith("salvar") || texto.StartsWith("novo")
                ? EstiloBotao.Primario
                : texto.StartsWith("excluir") ? EstiloBotao.Perigo : EstiloBotao.Secundario;

        Color fundo = estilo switch
        {
            EstiloBotao.Primario => Destaque,
            EstiloBotao.Perigo => Color.FromArgb(255, 240, 240),
            _ => Superficie
        };
        Color corTexto = estilo switch
        {
            EstiloBotao.Primario => TextoDestaque,
            EstiloBotao.Perigo => Vermelho,
            _ => Texto
        };

        if (!botao.Enabled) { fundo = Suave; corTexto = TextoSecundario; }
        botao.BackColor = fundo;
        botao.ForeColor = corTexto;
        botao.FlatAppearance.BorderColor = estilo == EstiloBotao.Primario && botao.Enabled ? Destaque : Borda;
        botao.FlatAppearance.MouseOverBackColor = ControlPaint.Dark(fundo, 0.04F);
        botao.FlatAppearance.MouseDownBackColor = ControlPaint.Dark(fundo, 0.10F);
    }

    public static void PrepararTabela(DataGridView tabela)
    {
        tabela.EnableHeadersVisualStyles = false;
        tabela.BackgroundColor = Superficie;
        tabela.BorderStyle = BorderStyle.None;
        tabela.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        tabela.GridColor = Borda;
        tabela.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
        tabela.ColumnHeadersDefaultCellStyle.BackColor = Suave;
        tabela.ColumnHeadersDefaultCellStyle.ForeColor = TextoSecundario;
        tabela.ColumnHeadersDefaultCellStyle.Font = FontePequena;
        tabela.ColumnHeadersDefaultCellStyle.Padding = new Padding(8, 8, 8, 8);
        tabela.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        tabela.DefaultCellStyle.Font = FontePadrao;
        tabela.DefaultCellStyle.BackColor = Superficie;
        tabela.DefaultCellStyle.ForeColor = Texto;
        tabela.DefaultCellStyle.Padding = new Padding(8, 5, 8, 5);
        tabela.DefaultCellStyle.SelectionBackColor = Color.FromArgb(235, 245, 218);
        tabela.DefaultCellStyle.SelectionForeColor = Texto;
        tabela.AlternatingRowsDefaultCellStyle.BackColor = Superficie;
        tabela.RowTemplate.Height = 46;
        tabela.AllowUserToResizeRows = false;
        tabela.ShowCellToolTips = true;
        tabela.AccessibleName ??= "Lista de registros";
    }
}
