// ReSharper disable once CheckNamespace
namespace System
{
    /// <summary>
    /// Extensiones para todas las instancias de los objetos.
    /// </summary>
    internal static class ObjectExtensions
    {
        #region Methods

        /// <summary>Serealiza el objeto en formato JSON.</summary>
        /// <param name="obj">Objeto a serializar.</param>
        /// <returns>Un string con el objeto en formato JSON.</returns>
        public static string ToJson(this object obj)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(obj);
        }

        /// <summary>Serealiza el objeto en formato JSON.</summary>
        /// <param name="obj">Objeto a serializar.</param>
        /// <param name="settings">Configuración adicional opcional.</param>
        /// <returns>Un string con el objeto en formato JSON.</returns>
        internal static string ToJson(this object obj, Newtonsoft.Json.JsonSerializerSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            return Newtonsoft.Json.JsonConvert.SerializeObject(obj, settings);
        }

        #endregion
    }
}