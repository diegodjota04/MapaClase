using System;
using System.Windows.Forms;
using MapaClaseApp.Forms;

namespace MapaClaseApp
{
    internal static class Program
    {
        /// <summary>
        /// Punto de entrada principal de la aplicación
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Habilitar estilos visuales modernos de Windows
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            // Para .NET 6+ usar esta línea en lugar de las dos anteriores:
            // ApplicationConfiguration.Initialize();
            
            // Ejecutar la aplicación con el formulario principal
            Application.Run(new ClassMapForm());
        }
    }
}