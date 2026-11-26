using ControleTreinos.WinForms;
using System.Globalization;

var casos = new (string? Entrada, int? Esperado)[]
{
    ("1", 1), ("18", 18), ("45", 45), ("2147483647", int.MaxValue),
    (" 18 ", 18), ("+18", 18), ("0007", 7),
    (null, null), ("", null), ("   ", null), ("0", null), ("-1", null),
    ("1,5", null), ("1.5", null), ("1,0", null), ("1.0", null),
    ("1e2", null), ("1E+2", null), ("1 000", null), ("1,000", null), ("1.000", null),
    ("abc", null), ("NaN", null), ("Infinity", null),
    ("2147483648", null), ("-2147483649", null), ("1/2", null), ("12x", null)
};

int verificacoes = 0;
CultureInfo culturaOriginal = CultureInfo.CurrentCulture;
try
{
    foreach (string idioma in new[] { "pt-BR", "en-US" })
    {
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(idioma);
        foreach (var (entrada, esperado) in casos)
        {
            bool aceito = ValidacaoNumerica.TentarLerInteiroPositivo(entrada, out int valor);
            if (aceito != esperado.HasValue || aceito && valor != esperado.GetValueOrDefault())
            {
                throw new InvalidOperationException(
                    $"Falhou em {idioma}: '{entrada ?? "<null>"}' — aceito={aceito}, valor={valor}."
                );
            }
            verificacoes++;
        }
        Console.WriteLine($"OK: {casos.Length} casos com idioma {idioma}.");
    }
}
finally
{
    CultureInfo.CurrentCulture = culturaOriginal;
}

Console.WriteLine($"Concluído: {verificacoes} verificações da validação numérica usada na interface.");
