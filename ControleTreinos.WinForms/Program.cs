using System;
using System.Windows.Forms;

namespace ControleTreinos.WinForms;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();

        Application.Run(new FormPrincipal());
    }
}