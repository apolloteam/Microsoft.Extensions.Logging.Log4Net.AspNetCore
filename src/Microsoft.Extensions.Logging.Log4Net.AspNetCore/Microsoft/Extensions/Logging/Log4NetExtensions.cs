// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.Logging
{
    using System;

    /// <summary>
    /// The log4net extensions class.
    /// </summary>
    public static class Log4NetExtensions
    {
        /// <summary>
        /// The default log4net config file name.
        /// </summary>
        private const string DefaultLog4NetConfigFile = "log4net.config";

        /// <summary>
        /// Adds the log4net.
        /// </summary>
        /// <param name="factory">The factory.</param>
        /// <param name="log4NetConfigFile">The log4net Config File.</param>
        /// <param name="exceptionFormatter">Exception formatter.</param>
        /// <returns>The <see cref="ILoggerFactory"/>.</returns>
        public static ILoggerFactory AddLog4Net(this ILoggerFactory factory, 
            string log4NetConfigFile, 
            Func<object, Exception, string> exceptionFormatter)
        {
            if (factory == null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            if (log4NetConfigFile == null)
            {
                throw new ArgumentNullException(nameof(log4NetConfigFile));
            }

            ILoggerProvider provider = new Log4NetProvider(log4NetConfigFile, exceptionFormatter);
            factory.AddProvider(provider);
            return factory;
        }

        /// <summary>
        /// Adds the log4net.
        /// </summary>
        /// <param name="factory">The factory.</param>
        /// <param name="log4NetConfigFile">The log4net Config File.</param>
        /// <returns>The <see cref="ILoggerFactory"/>.</returns>
        public static ILoggerFactory AddLog4Net(this ILoggerFactory factory, string log4NetConfigFile)
        {
            if (factory == null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            if (log4NetConfigFile == null)
            {
                throw new ArgumentNullException(nameof(log4NetConfigFile));
            }

            ILoggerProvider provider = new Log4NetProvider(log4NetConfigFile);
            factory.AddProvider(provider);
            return factory;
        }

        /// <summary>
        /// Adds the log4net.
        /// </summary>
        /// <param name="factory">The factory.</param>
        /// <param name="exceptionFormatter">The exception formatter.</param>
        /// <returns>The <see cref="ILoggerFactory"/>.</returns>
        public static ILoggerFactory AddLog4Net(this ILoggerFactory factory, Func<object, Exception, string> exceptionFormatter)
        {
            if (factory == null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            factory.AddLog4Net(DefaultLog4NetConfigFile, exceptionFormatter);
            return factory;
        }

        /// <summary>
        /// Adds the log4net.
        /// </summary>
        /// <param name="factory">The factory.</param>
        /// <returns>The <see cref="ILoggerFactory"/>.</returns>
        public static ILoggerFactory AddLog4Net(this ILoggerFactory factory)
        {
            if (factory == null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            factory.AddLog4Net(DefaultLog4NetConfigFile);
            return factory;
        }
    }
}