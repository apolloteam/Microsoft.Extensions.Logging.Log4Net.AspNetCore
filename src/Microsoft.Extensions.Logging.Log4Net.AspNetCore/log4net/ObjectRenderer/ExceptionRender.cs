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
        /// <param name="rendererMap"></param>
        /// <param name="obj">excepción a renderizar como JSON.</param>
        /// <param name="writer"></param>
        public void RenderObject(RendererMap rendererMap, object obj, TextWriter writer)
        {
            writer.Write((obj as Exception).ToJson());
        }
    }
}