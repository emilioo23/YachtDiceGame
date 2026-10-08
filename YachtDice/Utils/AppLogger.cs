using System;
using System.Linq;
using log4net;
using log4net.Core;

namespace YachtDice.Utils
{
    /// <summary>
    /// Registro de eventos del videojuego con los niveles del estándar del equipo
    /// (Trace, Debug, Info, Warning, Error) sobre log4net.
    /// </summary>
    public class AppLogger
    {
        private const int MaxStackLevels = 3;

        private readonly ILog _log;

        /// <summary>
        /// Crea un logger asociado a la clase que lo usa.
        /// </summary>
        /// <param name="ownerType">Tipo de la clase que registra los eventos.</param>
        public AppLogger(Type ownerType)
        {
            if (ownerType == null)
            {
                throw new ArgumentNullException(nameof(ownerType));
            }

            _log = LogManager.GetLogger(ownerType);
        }

        /// <summary>
        /// Registra el detalle más fino del flujo interno; solo para desarrollo.
        /// </summary>
        /// <param name="message">Mensaje a registrar.</param>
        public void Trace(string message)
        {
            _log.Logger.Log(typeof(AppLogger), Level.Trace, message, null);
        }

        /// <summary>
        /// Registra información útil para depurar durante desarrollo y pruebas.
        /// </summary>
        /// <param name="message">Mensaje a registrar.</param>
        public void Debug(string message)
        {
            _log.Debug(message);
        }

        /// <summary>
        /// Registra un evento relevante del flujo normal.
        /// </summary>
        /// <param name="message">Mensaje a registrar.</param>
        public void Info(string message)
        {
            _log.Info(message);
        }

        /// <summary>
        /// Registra una situación inesperada que no interrumpe la ejecución.
        /// </summary>
        /// <param name="message">Mensaje a registrar.</param>
        public void Warning(string message)
        {
            _log.Warn(message);
        }

        /// <summary>
        /// Registra una falla que impidió completar una operación, con la pila limitada a 3 niveles.
        /// </summary>
        /// <param name="exception">Excepción capturada.</param>
        /// <param name="message">Contexto de la operación que falló.</param>
        public void Error(Exception exception, string message)
        {
            if (exception == null)
            {
                throw new ArgumentNullException(nameof(exception));
            }

            _log.Error(BuildErrorMessage(exception, message));
        }

        private static string BuildErrorMessage(Exception exception, string message)
        {
            string[] stackLines = (exception.StackTrace ?? string.Empty)
                .Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            string limitedStack = string.Join(Environment.NewLine, stackLines.Take(MaxStackLevels));

            return $"{message} {exception.GetType().Name}: {exception.Message}{Environment.NewLine}{limitedStack}";
        }
    }
}