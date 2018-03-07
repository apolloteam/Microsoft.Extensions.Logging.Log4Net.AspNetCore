// ReSharper disable once CheckNamespace
namespace System
{
    // ReSharper disable RedundantNameQualifier
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text;

    // ReSharper restore RedundantNameQualifier
    using Newtonsoft.Json;

    /// <summary>
    /// Extensiones para todas las instancias de Exception.
    /// </summary>
    public static class ExceptionExtensions
    {
        #region Declarations

        /// <summary>Configuración para serializar las excepciones.</summary>
        private static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings
        {
            PreserveReferencesHandling = PreserveReferencesHandling.Objects
        };

        /// <summary>
        /// Separadores para el Split.
        /// </summary>
        private static readonly string[] Separator = new string[] { "\r\n" };

        #endregion

        #region Methods

        /// <summary>Devuelve la serialización de la excepción como Json.</summary>
        /// <param name="exception">Excepción a serializar.</param>
        /// <returns>Cadena Json.</returns>
        internal static string ToJson(this Exception exception)
        {
            if (exception == null)
            {
                throw new ArgumentNullException(nameof(exception));
            }

            StringBuilder sb = new StringBuilder();
            sb.Append("{");

            AggregateException aex = exception as AggregateException;
            string innerexceptions;
            bool includeInnerException = true;
            if (aex != null)
            {
                StringBuilder sbAEx = new StringBuilder();
                sbAEx.Append("[");
                for (int i = 0, l = aex.InnerExceptions.Count; i < l; i++)
                {
                    Exception aux = aex.InnerExceptions[i];
                    string value = ToJson(aux);
                    sbAEx.Append(value);
                    if (i < l - 1)
                    {
                        sbAEx.Append(",");
                    }

                    // Valida que la excepción InnerException no este en la lista de las exepciones.
                    if (includeInnerException && aux == aex.InnerException)
                    {
                        includeInnerException = false;
                    }
                }

                sbAEx.Append("]");
                innerexceptions = sbAEx.ToString();
            }
            else
            {
                innerexceptions = "null";
            }

            Type exceptionType = exception.GetType();

            // ReSharper disable once RedundantAssignment
            string targetsite = null;
#if NET_2_0 || MONO_2_0
            targetsite = (exception.TargetSite != null) ? exception.TargetSite.Name : string.Empty;
#else
            targetsite = string.Empty;
#endif

            string innerexception = "null";
            if (includeInnerException && exception.InnerException != null)
            {
                innerexception = ToJson(exception.InnerException);
            }

            // Serializa el StackTrace. 
            // string stacktrace = !string.IsNullOrWhiteSpace(exception.StackTrace)
            // ? Serializer.Serialize(
            // StringExtension.SplitStatic(
            // exception.StackTrace,
            // Environment.NewLine,
            // StringSplitOptions.RemoveEmptyEntries))
            // : "null";
            string stacktrace;
            if (!string.IsNullOrWhiteSpace(exception.StackTrace))
            {
                stacktrace = JsonConvert.SerializeObject(
                    exception.StackTrace.Split(new[] { Environment.NewLine },
                    StringSplitOptions.RemoveEmptyEntries));
            }
            else
            {
                stacktrace = "null";
            }

            // Toma el nombre de la clase. 
            // ReSharper disable once RedundantAssignment
            string className = null;
#if NET_2_0 || MONO_2_0
            if (exception.TargetSite != null && exception.TargetSite.DeclaringType != null)
            {
                className = exception.TargetSite.DeclaringType.FullName;
            }
            else
            {
                className = string.Empty;
            }
#else
            className = string.Empty;
#endif

            string source = exception.Source ?? string.Empty;

            string data = JsonConvert.SerializeObject(exception.Data);
            string helplink = !string.IsNullOrWhiteSpace(exception.HelpLink) ? exception.HelpLink : string.Empty;
            sb.AppendFormat(
                " \"Message\" : {0}, \"ExceptionType\" : \"{9}\", \"Class\" : \"{5}\", \"Method\" : \"{6}\", \"Source\" : \"{3}\", \"HelpLink\" : \"{8}\", \"InnerException\" : {1}, \"InnerExceptions\" : {2}, \"StackTrace\" : {4}, \"Data\" : {7}",
                JsonConvert.SerializeObject(exception.Message),
                innerexception,
                innerexceptions,
                source,
                stacktrace,
                className,
                targetsite,
                data,
                helplink,
                exceptionType.Name);
            string[] propiedadesYaProcesadas =
            {
                "Message",
                "Data",
                "StackTrace",
                "TargetSite",
                "HelpLink",
                "Source",
                "InnerException",
                "InnerExceptions"
            };
            const BindingFlags Flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy;
            IEnumerable<PropertyInfo> properties = (from p in exceptionType.GetProperties(Flags)
                where p.CanRead && !propiedadesYaProcesadas.Contains(p.Name)
                select p).ToList();
            foreach (PropertyInfo propertyInfo in properties)
            {
                try
                {
                    object value = propertyInfo.GetValue(exception, null);
                    if (value is string)
                    {
                        sb.AppendFormat(", \"{0}\" : {1}", propertyInfo.Name, JsonConvert.SerializeObject(value));
                    }
                    else if (value is Enum)
                    {
                        Type enumType = value.GetType();
                        sb.AppendFormat(
                            ", \"{0}\" : \"{1}.{2}\"",
                            propertyInfo.Name,
                            enumType.FullName,
                            Enum.GetName(enumType, value));
                    }
                    else if (value is Guid
                        /* MODI: 20151126 - DAE - Se Cambio el orden en que se evaluan los tipos.
                    GUID es un ValueType */)
                    {
                        sb.AppendFormat(", \"{0}\" : \"{1}\"", propertyInfo.Name, value);
                    }
                    else if (value is ValueType)
                    {
                        sb.AppendFormat(", \"{0}\" : {1}", propertyInfo.Name, value);
                    }
                    else
                    {
                        try
                        {
                            string json = value.ToJson(SerializerSettings);
                            sb.AppendFormat(", \"{0}\" : {1}", propertyInfo.Name, json);
                        }
                        catch (Exception ex)
                        {
                            sb.AppendFormat(
                                ", \"{0}\" : \"[ERROR No se pudo serializar la propiedad '{0}' ({1}) {2}]\"",
                                propertyInfo.Name,
                                propertyInfo.PropertyType.FullName,
                                ex.Message);
                        }
                    }
                }
                catch (NotImplementedException)
                {
                }
                catch (Exception)
                {
                    // ignored
                }
            }

            sb.Append("}");
            return sb.ToString();
        }

        /// <summary>
        /// Extrae de la excepción especificada los datos más relevantes.
        /// </summary>
        /// <remarks>
        /// Las excepciones completas pasadas a texto pesan como 24MB.
        /// </remarks>
        /// <param name="exception">La excepción sobre la cual se va a hacer el resumen.</param>
        /// <returns>Un objeto con datos resumidos de la excepción.</returns>
        internal static object ToResumeLog(this Exception exception)
        {
            object ret = null;
            if (exception != null)
            {
                Type type = exception.GetType();
                ret = new
                {
                    exception.Message,
                    Class = type.Name,
                    Type = type.FullName,
                    StackTrace = exception.StackTrace.Split(Separator, StringSplitOptions.RemoveEmptyEntries),
                    InnerException = ToResumeLog(exception.InnerException),
                    exception.Data,
                    exception.Source
                };
            }

            return ret;
        }

        #endregion
    }
}