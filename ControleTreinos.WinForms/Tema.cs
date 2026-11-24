using System;
using System.Drawing;
using System.Windows.Forms;

namespace ControleTreinos.WinForms;

public static class Tema
{
    public static readonly Color Fundo =
        Color.FromArgb(242, 246, 250);

    public static readonly Color AzulEscuro =
        Color.FromArgb(15, 32, 55);

    public static readonly Color Azul =
        Color.FromArgb(37, 99, 235);

    public static readonly Color Verde =
        Color.FromArgb(22, 120, 62);

    public static readonly Color Vermelho =
        Color.FromArgb(185, 28, 28);

    public static readonly Color Texto =
        Color.FromArgb(30, 41, 59);

    public static readonly Color Borda =
        Color.FromArgb(203, 213, 225);

    private static readonly Font FontePadrao =
        new Font("Segoe UI", 10F);

    private static readonly Font FonteBotoes =
        new Font("Segoe UI", 10F, FontStyle.Bold);

    public static void Aplicar(Form janela)
    {
        janela.SuspendLayout();

        janela.Font = FontePadrao;
        janela.BackColor = Fundo;
        janela.ForeColor = Texto;

        AplicarNosControles(janela.Controls);

        janela.ResumeLayout(true);
    }

    private static void AplicarNosControles(
        Control.ControlCollection controles)
    {
        foreach (Control controle in controles)
        {
            if (controle is Button botao)
            {
                PrepararBotao(botao);
                continue;
            }

            if (controle is DataGridView tabela)
            {
                PrepararTabela(tabela);
                continue;
            }

            if (controle is NumericUpDown numero)
            {
                numero.BorderStyle = BorderStyle.FixedSingle;
                numero.BackColor = Color.White;
                numero.ForeColor = Texto;
                continue;
            }

            if (controle is ComboBox lista)
            {
                lista.FlatStyle = FlatStyle.Flat;
                lista.BackColor = Color.White;
                lista.ForeColor = Texto;
                continue;
            }

            if (controle is TextBoxBase campo)
            {
                campo.BorderStyle = BorderStyle.FixedSingle;
                campo.BackColor = Color.White;
                campo.ForeColor = Texto;
                continue;
            }

            if (controle.HasChildren)
            {
                AplicarNosControles(controle.Controls);
            }
        }
    }

    private static void PrepararBotao(Button botao)
    {
        botao.FlatStyle = FlatStyle.Flat;
        botao.UseVisualStyleBackColor = false;
        botao.FlatAppearance.BorderSize = 0;
        botao.Padding = new Padding(12, 8, 12, 8);

        if (botao.Font.SizeInPoints <= 10.5F)
        {
            botao.Font = FonteBotoes;
        }

        AtualizarCorBotao(botao);

        botao.EnabledChanged += (sender, e) =>
        {
            AtualizarCorBotao(botao);
        };

        botao.TextChanged += (sender, e) =>
        {
            AtualizarCorBotao(botao);
        };
    }

    private static void AtualizarCorBotao(Button botao)
    {
        if (botao.IsDisposed)
        {
            return;
        }

        string texto = botao.Text.Trim().ToLowerInvariant();

        Color fundo = Azul;
        Color corTexto = Color.White;

        if (!botao.Enabled)
        {
            fundo = Color.FromArgb(226, 232, 240);
            corTexto = Color.FromArgb(100, 116, 139);
        }
        else if (texto.StartsWith("cadastrar")
            || texto.StartsWith("salvar"))
        {
            fundo = Verde;
        }
        else if (texto.StartsWith("excluir"))
        {
            fundo = Vermelho;
        }
        else if (texto.StartsWith("cancelar")
            || texto.StartsWith("atualizar")
            || texto.StartsWith("mostrar todos")
            || texto == "fechar"
            || texto == "sair")
        {
            fundo = Color.FromArgb(226, 232, 240);
            corTexto = Texto;
        }

        botao.BackColor = fundo;
        botao.ForeColor = corTexto;

        botao.FlatAppearance.MouseOverBackColor =
            ControlPaint.Dark(fundo, 0.08F);

        botao.FlatAppearance.MouseDownBackColor =
            ControlPaint.Dark(fundo, 0.18F);
    }

    private static void PrepararTabela(DataGridView tabela)
    {
        tabela.EnableHeadersVisualStyles = false;
        tabela.BackgroundColor = Color.White;
        tabela.BorderStyle = BorderStyle.None;

        tabela.CellBorderStyle =
            DataGridViewCellBorderStyle.SingleHorizontal;

        tabela.GridColor = Borda;

        tabela.ColumnHeadersBorderStyle =
            DataGridViewHeaderBorderStyle.None;

        tabela.ColumnHeadersDefaultCellStyle.BackColor = AzulEscuro;
        tabela.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        tabela.ColumnHeadersDefaultCellStyle.Font = FonteBotoes;
        tabela.ColumnHeadersDefaultCellStyle.Padding =
            new Padding(8, 6, 8, 6);

        tabela.ColumnHeadersHeightSizeMode =
            DataGridViewColumnHeadersHeightSizeMode.AutoSize;

        tabela.DefaultCellStyle.BackColor = Color.White;
        tabela.DefaultCellStyle.ForeColor = Texto;
        tabela.DefaultCellStyle.Padding = new Padding(6, 3, 6, 3);

        tabela.DefaultCellStyle.SelectionBackColor =
            Color.FromArgb(219, 234, 254);

        tabela.DefaultCellStyle.SelectionForeColor = Texto;

        tabela.AlternatingRowsDefaultCellStyle.BackColor =
            Color.FromArgb(245, 248, 252);

        tabela.RowTemplate.Height = 34;
    }
}