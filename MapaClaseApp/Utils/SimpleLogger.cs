using System;
using System.IO;
using System.Text;

namespace MapaClaseApp.Utils
{
    /// <summary>
    /// Logger simple para depuración y registro de errores
    /// </summary>
    public static class SimpleLogger
    {
        private static readonly object _lock = new object();
        private static string? _logPath;
        private static bool _isEnabled = true;
        
        /// <summary>
        /// Inicializa el logger con la ruta del archivo de log
        /// </summary>
        public static void Initialize(string? customPath = null)
        {
            try
            {
                if (customPath != null)
                {
                    _logPath = customPath;
                }
                else
                {
                    // Crear carpeta de logs en AppData
                    string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                    string logDirectory = Path.Combine(appDataPath, "MapaClaseApp", "Logs");
                    
                    if (!Directory.Exists(logDirectory))
                    {
                        Directory.CreateDirectory(logDirectory);
                    }
                    
                    _logPath = Path.Combine(logDirectory, $"MapaClase_{DateTime.Now:yyyyMMdd}.log");
                }
                
                // Escribir encabezado inicial
                LogInfo("=== MapaClaseApp Iniciado ===");
                LogInfo($"Versión: 1.0.0");
                LogInfo($"Fecha: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                LogInfo($"Sistema: {Environment.OSVersion}");
                LogInfo($".NET Version: {Environment.Version}");
                LogInfo("================================\n");
            }
            catch
            {
                // Si falla la inicialización, deshabilitar el logger
                _isEnabled = false;
            }
        }
        
        /// <summary>
        /// Registra un mensaje de información
        /// </summary>
        public static void LogInfo(string message)
        {
            WriteLog("INFO", message);
        }
        
        /// <summary>
        /// Registra un mensaje de advertencia
        /// </summary>
        public static void LogWarning(string message)
        {
            WriteLog("WARN", message);
        }
        
        /// <summary>
        /// Registra un error
        /// </summary>
        public static void LogError(string message, Exception? ex = null)
        {
            string errorMessage = message;
            if (ex != null)
            {
                errorMessage += $"\nException: {ex.GetType().Name}\nMessage: {ex.Message}\nStackTrace: {ex.StackTrace}";
            }
            WriteLog("ERROR", errorMessage);
        }
        
        /// <summary>
        /// Registra información de depuración
        /// </summary>
        public static void LogDebug(string message)
        {
            #if DEBUG
            WriteLog("DEBUG", message);
            #endif
        }
        
        /// <summary>
        /// Escribe el log en el archivo
        /// </summary>
        private static void WriteLog(string level, string message)
        {
            if (!_isEnabled || string.IsNullOrEmpty(_logPath)) return;
            
            try
            {
                lock (_lock)
                {
                    string logEntry = $"[{DateTime.Now:HH:mm:ss.fff}] [{level,-5}] {message}\n";
                    File.AppendAllText(_logPath, logEntry, Encoding.UTF8);
                }
            }
            catch
            {
                // Ignorar errores de escritura para no afectar la aplicación
            }
        }
        
        /// <summary>
        /// Obtiene la ruta del archivo de log actual
        /// </summary>
        public static string? GetLogPath() => _logPath;
        
        /// <summary>
        /// Limpia logs antiguos (más de 7 días)
        /// </summary>
        public static void CleanOldLogs()
        {
            try
            {
                if (string.IsNullOrEmpty(_logPath)) return;
                
                string? directory = Path.GetDirectoryName(_logPath);
                if (directory == null || !Directory.Exists(directory)) return;
                
                var files = Directory.GetFiles(directory, "MapaClase_*.log");
                var cutoffDate = DateTime.Now.AddDays(-7);
                
                foreach (var file in files)
                {
                    var fileInfo = new FileInfo(file);
                    if (fileInfo.CreationTime < cutoffDate)
                    {
                        try
                        {
                            File.Delete(file);
                            LogInfo($"Log antiguo eliminado: {fileInfo.Name}");
                        }
                        catch { }
                    }
                }
            }
            catch { }
        }
    }
}