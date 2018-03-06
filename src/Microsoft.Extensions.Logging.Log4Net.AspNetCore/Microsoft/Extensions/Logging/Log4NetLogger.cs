// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.Logging
{
    using System;
    using log4net;

    /// <summary>
    /// The log4net logger class.
    /// </summary>
    public class Log4NetLogger : ILogger
    {
        /// <summary>
        /// The log.
        /// </summary>
        private readonly ILog log;

        /// <summary>
        /// The formatter when logging an exception.
        /// </summary>
        private Func<object, Exception, string> exceptionDetailsFormatter;

        /// <summary>
        /// Initializes a new instance of the <see cref="Log4NetLogger"/> class.
        /// </summary>
        /// <param name="loggerRepository">The repository name.</param>
        /// <param name="name">The logger's name.</param>
        public Log4NetLogger(string loggerRepository, string name)
        {
            this.log = LogManager.GetLogger(loggerRepository, name);
        }

        /// <summary>
        /// Begins a logical operation scope.
        /// </summary>
        /// <typeparam name="TState">The type of the state.</typeparam>
        /// <param name="state">The identifier for the scope.</param>
        /// <returns>
        /// An IDisposable that ends the logical operation scope on dispose.
        /// </returns>
        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        /// <summary>
        /// Determines whether the logging level is enabled.
        /// </summary>
        /// <param name="logLevel">The log level.</param>
        /// <returns>The <see cref="bool"/> value indicating whether the logging level is enabled.</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public bool IsEnabled(LogLevel logLevel)
        {
            bool ret;
            switch (logLevel)
            {
                case LogLevel.Critical:
                    ret = this.log.IsFatalEnabled;
                    break;

                case LogLevel.Debug:
                case LogLevel.Trace:
                    ret = this.log.IsDebugEnabled;
                    break;

                case LogLevel.Error:
                    ret = this.log.IsErrorEnabled;
                    break;

                case LogLevel.Information:
                    ret = this.log.IsInfoEnabled;
                    break;

                case LogLevel.Warning:
                    ret = this.log.IsWarnEnabled;
                    break;

                case LogLevel.None:
                    ret = false;
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(logLevel));
            }

            return ret;
        }

        /// <summary>
        /// Logs an exception into the log.
        /// </summary>
        /// <param name="logLevel">The log level.</param>
        /// <param name="eventId">The event Id.</param>
        /// <param name="state">The state.</param>
        /// <param name="exception">The exception.</param>
        /// <param name="formatter">The formatter.</param>
        /// <typeparam name="TState">The type of the state.</typeparam>
        /// <exception cref="ArgumentNullException">Throws when the <paramref name="formatter"/> is null.</exception>
        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception exception,
            Func<TState, Exception, string> formatter)
        {
            if (this.IsEnabled(logLevel))
            {
                if (formatter == null)
                {
                    throw new ArgumentNullException(nameof(formatter));
                }

                string message = formatter(state, exception);
                if (exception != null && this.exceptionDetailsFormatter != null)
                {
                    message = this.exceptionDetailsFormatter(message, exception);
                }

                if (message == null)
                {
                    message = "Sin mensaje.";
                }

                switch (logLevel)
                {
                    case LogLevel.Critical:
                        if (exception != null)
                        {
                            this.log.Fatal(message, exception);
                        }
                        else
                        {
                            this.log.Fatal(message);
                        }

                        break;

                    case LogLevel.Debug:
                    case LogLevel.Trace:
                        if (exception != null)
                        {
                            this.log.Debug(message, exception);
                        }
                        else
                        {
                            this.log.Debug(message);
                        }

                        break;

                    case LogLevel.Error:
                        if (exception != null)
                        {
                            this.log.Error(message, exception);
                        }
                        else
                        {
                            this.log.Error(message);
                        }

                        break;

                    case LogLevel.Information:
                        if (exception != null)
                        {
                            this.log.Info(message, exception);
                        }
                        else
                        {
                            this.log.Info(message);
                        }

                        break;

                    case LogLevel.Warning:
                        if (exception != null)
                        {
                            this.log.Warn(message, exception);
                        }
                        else
                        {
                            this.log.Warn(message);
                        }

                        break;

                    case LogLevel.None:
                        break;

                    default:
                        this.log.Warn($"Encountered unknown log level {logLevel}, writing out as Info.");
                        this.log.Info(message, exception);
                        break;
                }
            }
        }

        /// <summary>
        /// Defines custom formatter for logging exceptions.
        /// </summary>
        /// <param name="formatter">The formatting function to be used when formatting exceptions.</param>
        /// <returns>The logger itself for fluent use.</returns>
        public Log4NetLogger UsingCustomExceptionFormatter(Func<object, Exception, string> formatter)
        {
            this.exceptionDetailsFormatter = formatter ?? throw new ArgumentNullException(nameof(formatter));
            return this;
        }
    }
}