using System;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace YachtDice.Utils
{
    /// <summary>
    /// Registra eventos de la aplicación en un archivo de texto.
    /// </summary>
    public static class AppLogger
    {
        private const string LogFolderName = "logs";
        private const string LogFileName = "yachtdice.log";
        private const string TimestampFormat = "yyyy-MM-dd HH:mm:ss";
        private const string InfoLevel = "INFO";
        private const string ErrorLevel = "ERROR";
        private const string LineFormat = "{0} [{1}] {2}";
        private const string ExceptionFormat = "{0} | {1}: {2}";
        private const int SingleAccessCount = 1;
        private const bool AppendMode = true;

        private static readonly SemaphoreSlim _writeLock = new SemaphoreSlim(SingleAccessCount);

        /// <summary>
        /// Registra un mensaje informativo.
        /// </summary>
        /// <param name="message">Texto del evento.</param>
        public static Task InfoAsync(string message) => WriteAsync(InfoLevel, message);

        /// <summary>
        /// Registra un error junto con la excepción que lo causó.
        /// </summary>
        /// <param name="message">Texto descriptivo del error.</param>
        /// <param name="exception">Excepción capturada.</param>
        public static Task ErrorAsync(string message, Exception exception)
        {
            string fullMessage = string.Format(
                CultureInfo.InvariantCulture,
                ExceptionFormat,
                message,
                exception.GetType().Name,
                exception.Message);

            return WriteAsync(ErrorLevel, fullMessage);
        }

        private static async Task WriteAsync(string level, string message)
        {
            string timestamp = DateTime.Now.ToString(TimestampFormat, CultureInfo.InvariantCulture);
            string line = string.Format(CultureInfo.InvariantCulture, LineFormat, timestamp, level, message);
            string folderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, LogFolderName);

            await _writeLock.WaitAsync();

            try
            {
                Directory.CreateDirectory(folderPath);

                using (var writer = new StreamWriter(Path.Combine(folderPath, LogFileName), AppendMode))
                {
                    await writer.WriteLineAsync(line);
                }
            }
            catch (IOException)
            {
                // Un fallo al escribir el log no debe detener la aplicación
            }
            catch (UnauthorizedAccessException)
            {
                // Sin permisos de escritura se omite el registro
            }
            finally
            {
                _writeLock.Release();
            }
        }
    }
}