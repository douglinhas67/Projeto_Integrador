using System.Drawing.Drawing2D;

namespace ControleTreinos.WinForms;

// Componentes compartilhados pelas telas, sem bibliotecas visuais externas.
public class Cartao : Panel
{
    public Cartao()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint
            | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw
            | ControlStyles.SupportsTransparentBackColor, true);
        BackColor = Color.Transparent;
        Padding = new Padding(18);
        Margin = new Padding(0);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        if (Width < 2 || Height < 2) return;
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        float curva = Math.Min(24F, Math.Min(Width - 1, Height - 1));
        using GraphicsPath caminho = new();
        caminho.AddArc(0, 0, curva, curva, 180, 90);
        caminho.AddArc(Width - curva - 1, 0, curva, curva, 270, 90);
        caminho.AddArc(Width - curva - 1, Height - curva - 1, curva, curva, 0, 90);
        caminho.AddArc(0, Height - curva - 1, curva, curva, 90, 90);
        caminho.CloseFigure();
        using SolidBrush preenchimento = new(Tema.Superficie);
        using Pen borda = new(Tema.Borda);
        e.Graphics.FillPath(preenchimento, caminho);
        e.Graphics.DrawPath(borda, caminho);
    }
}

public class Indicador : Cartao
{
    private readonly Label valor = new();

    public Indicador(string titulo)
    {
        Dock = DockStyle.Fill;
        MinimumSize = new Size(0, 110);
        var conteudo = Componentes.Coluna();
        conteudo.Controls.Add(Componentes.Rotulo(titulo, Tema.TextoSecundario));
        valor.Text = "—";
        valor.AutoSize = true;
        valor.Font = Tema.FonteNumero;
        valor.ForeColor = Tema.Texto;
        valor.Margin = new Padding(0, 3, 0, 0);
        conteudo.Controls.Add(valor);
        Controls.Add(conteudo);
    }

    public void Definir(long numero) => valor.Text = numero.ToString("N0", System.Globalization.CultureInfo.GetCultureInfo("pt-BR"));
}

public class LayoutPagina : TableLayoutPanel
{
    public FlowLayoutPanel Acoes { get; } = Componentes.BarraAcoes();

    public LayoutPagina(string titulo, string descricao)
    {
        Dock = DockStyle.Fill;
        Padding = new Padding(26, 24, 26, 18);
        Margin = new Padding(0);
        ColumnCount = 1;
        RowCount = 3;
        ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        RowStyles.Add(new RowStyle(SizeType.AutoSize));
        RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var cabecalho = new TableLayoutPanel
        {
            Dock = DockStyle.Fill, AutoSize = true, ColumnCount = 2,
            Margin = new Padding(0, 0, 0, 22)
        };
        cabecalho.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        cabecalho.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        var textos = Componentes.Coluna();
        var nome = Componentes.Rotulo(titulo, Tema.Texto);
        nome.Font = Tema.FonteTitulo;
        textos.Controls.Add(nome);
        var apoio = Componentes.Rotulo(descricao, Tema.TextoSecundario);
        apoio.Margin = new Padding(0, 5, 0, 0);
        textos.Controls.Add(apoio);
        cabecalho.Controls.Add(textos, 0, 0);
        Acoes.Anchor = AnchorStyles.Right;
        cabecalho.Controls.Add(Acoes, 1, 0);
        Controls.Add(cabecalho, 0, 0);
    }

    public void Conteudo(Control controle) { controle.Dock = DockStyle.Fill; Controls.Add(controle, 0, 1); }
    public void Rodape(Label status)
    {
        status.AutoSize = true;
        status.Dock = DockStyle.Fill;
        status.Font = Tema.FontePequena;
        status.ForeColor = Tema.TextoSecundario;
        status.Margin = new Padding(0, 14, 0, 0);
        Controls.Add(status, 0, 2);
    }
}

public static class Componentes
{
    public static Label Rotulo(string texto, Color? cor = null) => new()
    {
        Text = texto, AutoSize = true, Dock = DockStyle.Fill,
        Font = Tema.FontePequena, ForeColor = cor ?? Tema.Texto,
        Margin = new Padding(0), BackColor = Color.Transparent
    };

    public static TableLayoutPanel Coluna() => new()
    {
        Dock = DockStyle.Top, AutoSize = true, ColumnCount = 1,
        GrowStyle = TableLayoutPanelGrowStyle.AddRows,
        Margin = new Padding(0), BackColor = Color.Transparent
    };

    public static FlowLayoutPanel BarraAcoes() => new()
    {
        AutoSize = true, WrapContents = true, Dock = DockStyle.Fill,
        Margin = new Padding(0), BackColor = Color.Transparent
    };

    public static void Botao(Button botao, string texto)
    {
        botao.Text = texto;
        botao.AutoSize = true;
        botao.Margin = new Padding(0, 0, 8, 6);
        Tema.PrepararBotao(botao);
    }

    public static TableLayoutPanel Campo(string titulo, Control campo, string? dica = null)
    {
        var bloco = Coluna();
        bloco.Dock = DockStyle.Fill;
        bloco.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        bloco.Margin = new Padding(0, 0, 0, 18);
        var rotulo = Rotulo(titulo);
        rotulo.Margin = new Padding(0, 0, 0, 7);
        bloco.Controls.Add(rotulo);
        campo.Dock = DockStyle.Fill;
        campo.Margin = new Padding(0);
        campo.AccessibleName = titulo;
        bloco.Controls.Add(campo);
        if (dica != null)
        {
            var ajuda = Rotulo(dica, Tema.TextoSecundario);
            ajuda.Margin = new Padding(0, 5, 0, 0);
            bloco.Controls.Add(ajuda);
        }
        return bloco;
    }

    public static TableLayoutPanel Dividir(Control lista, Control editor)
    {
        var painel = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, Margin = new Padding(0) };
        painel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        painel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 306));
        painel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        lista.Dock = DockStyle.Fill;
        lista.Margin = new Padding(0, 0, 18, 0);
        editor.Dock = DockStyle.Fill;
        painel.Controls.Add(lista, 0, 0);
        painel.Controls.Add(editor, 1, 0);
        return painel;
    }

    public static Cartao Lista(string titulo, Control tabela, Control? filtro = null, Control? acoes = null)
    {
        var cartao = new Cartao { Dock = DockStyle.Fill };
        var estrutura = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4, Margin = new Padding(0), BackColor = Color.Transparent };
        estrutura.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        estrutura.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        estrutura.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        estrutura.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        estrutura.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        var nome = Rotulo(titulo);
        nome.Font = Tema.FonteSecao;
        nome.Margin = new Padding(0, 0, 0, 16);
        estrutura.Controls.Add(nome, 0, 0);
        if (filtro != null) estrutura.Controls.Add(filtro, 0, 1);
        tabela.Dock = DockStyle.Fill;
        tabela.Margin = new Padding(0);
        estrutura.Controls.Add(tabela, 0, 2);
        if (acoes != null) { acoes.Margin = new Padding(0, 14, 0, 0); estrutura.Controls.Add(acoes, 0, 3); }
        cartao.Controls.Add(estrutura);
        return cartao;
    }

    public static Cartao Editor(Label titulo, params Control[] campos)
    {
        var cartao = new Cartao { Dock = DockStyle.Fill, AutoScroll = true };
        var coluna = Coluna();
        coluna.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        titulo.AutoSize = true;
        titulo.Dock = DockStyle.Fill;
        titulo.Font = Tema.FonteSecao;
        titulo.ForeColor = Tema.Texto;
        titulo.Margin = new Padding(0, 0, 0, 22);
        coluna.Controls.Add(titulo);
        foreach (var campo in campos) coluna.Controls.Add(campo);
        cartao.Controls.Add(coluna);
        return cartao;
    }

    public static TableLayoutPanel Indicadores(params Indicador[] itens)
    {
        var painel = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = itens.Length, RowCount = 1, Margin = new Padding(0, 0, 0, 20) };
        for (int i = 0; i < itens.Length; i++)
        {
            painel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F / itens.Length));
            itens[i].Margin = new Padding(0, 0, i < itens.Length - 1 ? 14 : 0, 0);
            painel.Controls.Add(itens[i], i, 0);
        }
        return painel;
    }
}
