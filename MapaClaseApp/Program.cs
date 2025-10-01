using MapaClaseApp.Forms;
using System;
using System.Windows.Forms;
using MapaClaseApp.Utils; 

namespace MapaClaseApp
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            SimpleLogger.Initialize();
            SimpleLogger.CleanOldLogs();
            ApplicationConfiguration.Initialize();
            Application.Run(new ClassMapForm());
            
            
        }
    }
}