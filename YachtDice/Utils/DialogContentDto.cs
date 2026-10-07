using System;

namespace YachtDice.Utils
{
    /// <summary>
    /// Objeto de transferencia de datos que contiene la configuración de texto para un diálogo.
    /// </summary>
    public class DialogContentDto
    {
        /// <summary>
        /// Obtiene o establece el título del diálogo.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Obtiene o establece el mensaje principal del diálogo.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Obtiene o establece el texto del botón principal.
        /// </summary>
        public string PrimaryButtonText { get; set; }

        /// <summary>
        /// Obtiene o establece el texto del botón secundario.
        /// </summary>
        public string SecondaryButtonText { get; set; }
    }
}
