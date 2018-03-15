// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.Logging
{
    using System;
    using System.Collections.Concurrent;
    using System.IO;
    using System.Reflection;
    using System.Text;
    using System.Xml;

    using log4net;
    using log4net.Config;
    using log4net.ObjectRenderer;
    using log4net.Repository;

    /// <summary>
    /// The log4net provider class.
    /// </summary>
    public class Log4NetProvider : ILoggerProvider
    {
        /// <summary>
        /// The log4net repository.
        /// </summary>
        private readonly ILoggerRepository loggerRepository;

        /// <summary>
        /// The exception formatter Func.
        /// </summary>
        private readonly Func<object, Exception, string> exceptionFormatter;

        /// <summary>
        /// The loggers collection.
        /// </summary>
        private readonly ConcurrentDictionary<string, Log4NetLogger> loggers = new ConcurrentDictionary<string, Log4NetLogger>();

        /// <summary>
        /// Initializes a new instance of the <see cref="Log4NetProvider"/> class.
        /// </summary>
        /// <param name="log4NetConfigFile">The log4 net configuration file.</param>
        public Log4NetProvider(string log4NetConfigFile)
            : this(log4NetConfigFile, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Log4NetProvider"/> class.
        /// </summary>
        /// <param name="log4NetConfigFile">The log4NetConfigFile.</param>
        /// <param name="exceptionFormatter">Exception formatter.</param>
        public Log4NetProvider(string log4NetConfigFile, Func<object, Exception, string> exceptionFormatter)
        {
            this.exceptionFormatter = exceptionFormatter ?? FormatExceptionByDefault;
            Type type = typeof(log4net.Repository.Hierarchy.Hierarchy);
            Assembly assembly = Assembly.GetEntryAssembly();
            this.loggerRepository = LogManager.CreateRepository(assembly, type);

            ////XmlConfigurator.Configure(this.loggerRepository, Parselog4NetConfigFile(log4NetConfigFile));

            FileInfo configFile = new FileInfo(log4NetConfigFile);
            XmlConfigurator.Configure(this.loggerRepository, configFile);

            // Agrega un ObjectRender para excepciones para serializarlas como JSON.
            this.loggerRepository.RendererMap.Put(typeof(Exception), new ExceptionRender());
        }

        /// <summary>
        /// Creates the logger.
        /// </summary>
        /// <param name="categoryName">The category name.</param>
        /// <returns>The <see cref="ILogger"/> instance.</returns>
        public ILogger CreateLogger(string categoryName)
        {
            return this.loggers.GetOrAdd(categoryName, this.CreateLoggerImplementation);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
            }

            this.loggers.Clear();
        }

        /// <summary>
        /// Formats an exception by default.
        /// </summary>
        /// <typeparam name="TState">The type of the state.</typeparam>
        /// <param name="state">The state of the logged object.</param>
        /// <param name="exception">The exception to be logged.</param>
        /// <returns>The text with the formatted message.</returns>
        private static string FormatExceptionByDefault<TState>(TState state, Exception exception)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(state);
            builder.Append(" - ");
            if (exception != null)
            {
                builder.Append(exception);
            }

            return builder.ToString();
        }

        /// <summary>
        /// Parses log4net config file.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <returns>The <see cref="XmlElement"/> with the log4net XML element.</returns>
        [Obsolete]
        // ReSharper disable once UnusedMember.Local
        private static XmlElement Parselog4NetConfigFile(string filename)
        {
            using (FileStream fp = File.OpenRead(filename))
            {
                XmlReaderSettings settings = new XmlReaderSettings
                {
                    DtdProcessing = DtdProcessing.Prohibit
                };

                XmlDocument log4NetConfig = new XmlDocument();
                using (XmlReader reader = XmlReader.Create(fp, settings))
                {
                    log4NetConfig.Load(reader);
                }

                return log4NetConfig["log4net"];
            }
        }

        /// <summary>
        /// Creates the logger implementation.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>The <see cref="Log4NetLogger"/> instance.</returns>
        private Log4NetLogger CreateLoggerImplementation(string name)
        {
            Log4NetLogger logger = new Log4NetLogger(this.loggerRepository.Name, name);
            return logger
                .UsingCustomExceptionFormatter(this.exceptionFormatter);
        }
    }
}