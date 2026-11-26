using System.Globalization;

namespace ControleTreinos.WinForms;

public static class ValidacaoNumerica
{
    public static bool TentarLerInteiroPositivo(string? texto, out int valor)
    {
        return int.TryParse(texto, NumberStyles.Integer, CultureInfo.InvariantCulture, out valor)
            && valor > 0;
    }
}
