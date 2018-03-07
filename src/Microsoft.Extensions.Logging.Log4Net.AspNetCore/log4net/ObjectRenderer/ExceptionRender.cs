// ReSharper disable once CheckNamespace
namespace log4net.ObjectRenderer
{
    using System;
    using System.IO;

    /// <summary>
    /// Renderiza una excepción como JSON.
    /// </summary>
    internal class ExceptionRender : IObjectRenderer
    {
        /// <summary>
        /// Renderiza una excepción como JSON.
        /// </summary>
        /// <param name="rendererMap">Render map log4net.</param>
        /// <param name="obj">Excepción a renderizar como JSON.</param>
        /// <param name="writer">Writer XML.</param>
        public void RenderObject(RendererMap rendererMap, object obj, TextWriter writer)
        {
            writer.Write((obj as Exception).ToJson());
        }
    }
}