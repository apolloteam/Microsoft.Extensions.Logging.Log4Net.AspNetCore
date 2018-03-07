// ReSharper disable once CheckNamespace
namespace log4net.Layout
{
    using System;
    using System.Collections;
    using System.Text;
    using System.Xml;

    using Core;
    using log4net.ObjectRenderer;
    using Util;

    /// <summary>
    /// Personalización de la clase XmlLayout para corregir. 
    /// </summary>
    public class CustomXmlLayout : XmlLayout
    {


        #region Private Instance Fields

        /// <summary>
        /// The prefix to use for all generated element names
        /// </summary>
        // private readonly string m_prefix = PREFIX;

        private readonly string m_elmEvent = ELM_EVENT;
        private readonly string m_elmMessage = ELM_MESSAGE;
        private readonly string m_elmData = ELM_DATA;
        private readonly string m_elmProperties = ELM_PROPERTIES;
        private readonly string m_elmException = ELM_EXCEPTION;
        private readonly string m_elmLocation = ELM_LOCATION;

        // private readonly bool m_base64Message = false;
        // private readonly bool m_base64Properties = false;

        #endregion Private Instance Fields

        #region Private Static Fields

        // private const string PREFIX = "log4net";

        private const string ELM_EVENT = "event";
        private const string ELM_MESSAGE = "message";
        private const string ELM_PROPERTIES = "properties";
        // private const string ELM_GLOBAL_PROPERTIES = "global-properties";
        private const string ELM_DATA = "data";
        private const string ELM_EXCEPTION = "exception";
        private const string ELM_LOCATION = "locationInfo";

        private const string ATTR_LOGGER = "logger";
        private const string ATTR_TIMESTAMP = "timestamp";
        private const string ATTR_LEVEL = "level";
        private const string ATTR_THREAD = "thread";
        private const string ATTR_DOMAIN = "domain";
        private const string ATTR_IDENTITY = "identity";
        private const string ATTR_USERNAME = "username";
        private const string ATTR_CLASS = "class";
        private const string ATTR_METHOD = "method";
        private const string ATTR_FILE = "file";
        private const string ATTR_LINE = "line";
        private const string ATTR_NAME = "name";
        private const string ATTR_VALUE = "value";

        #endregion Private Static Fields

        /// <summary>
        /// Constructor de la clase.
        /// </summary>
        public CustomXmlLayout()
        {
        }

        /// <summary>
        /// Constructor de la clase.
        /// </summary>
        /// <param name="locationInfo">Información de la ubicación donde se realiza el log.</param>
        public CustomXmlLayout(bool locationInfo)
            : base(locationInfo)
        {

        }

        /// <summary>
        /// Sobreescritura del metodo para aplicar el Fix del location.
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="loggingEvent"></param>
        protected override void FormatXml(XmlWriter writer, LoggingEvent loggingEvent)
        {
            if (loggingEvent?.LocationInformation != null)
            {

                writer.WriteStartElement(this.m_elmEvent);
                writer.WriteAttributeString(ATTR_LOGGER, loggingEvent.LoggerName);

#if NET_2_0 || NETCF_2_0 || MONO_2_0
            writer.WriteAttributeString(ATTR_TIMESTAMP, XmlConvert.ToString(loggingEvent.TimeStamp, XmlDateTimeSerializationMode.Local));
#else
                writer.WriteAttributeString(ATTR_TIMESTAMP, XmlConvert.ToString(loggingEvent.TimeStamp));
#endif

                writer.WriteAttributeString(ATTR_LEVEL, loggingEvent.Level.DisplayName);
                writer.WriteAttributeString(ATTR_THREAD, loggingEvent.ThreadName);

                if (!string.IsNullOrEmpty(loggingEvent.Domain))
                {
                    writer.WriteAttributeString(ATTR_DOMAIN, loggingEvent.Domain);
                }
                if (!string.IsNullOrEmpty(loggingEvent.Identity))
                {
                    writer.WriteAttributeString(ATTR_IDENTITY, loggingEvent.Identity);
                }
                if (!string.IsNullOrEmpty(loggingEvent.UserName))
                {
                    writer.WriteAttributeString(ATTR_USERNAME, loggingEvent.UserName);
                }

                // Append the message text
                writer.WriteStartElement(this.m_elmMessage);
                if (!this.Base64EncodeMessage)
                {
                    Transform.WriteEscapedXmlString(writer, loggingEvent.RenderedMessage, this.InvalidCharReplacement);
                }
                else
                {
                    byte[] messageBytes = Encoding.UTF8.GetBytes(loggingEvent.RenderedMessage);
                    string base64Message = Convert.ToBase64String(messageBytes, 0, messageBytes.Length);
                    Transform.WriteEscapedXmlString(writer, base64Message, this.InvalidCharReplacement);
                }
                writer.WriteEndElement();

                PropertiesDictionary properties = loggingEvent.GetProperties();

                // Append the properties text
                if (properties.Count > 0)
                {
                    writer.WriteStartElement(this.m_elmProperties);
                    RendererMap rendererMap = loggingEvent.Repository.RendererMap;
                    foreach (DictionaryEntry entry in properties)
                    {
                        writer.WriteStartElement(this.m_elmData);
                        writer.WriteAttributeString(ATTR_NAME, Transform.MaskXmlInvalidCharacters((string)entry.Key, this.InvalidCharReplacement));

                        // Use an ObjectRenderer to convert the object to a string
                        string valueStr;
                        string render = rendererMap.FindAndRender(entry.Value);
                        if (!this.Base64EncodeProperties)
                        {
                            valueStr = Transform.MaskXmlInvalidCharacters(render, this.InvalidCharReplacement);
                        }
                        else
                        {
                            byte[] propertyValueBytes = Encoding.UTF8.GetBytes(render);
                            valueStr = Convert.ToBase64String(propertyValueBytes, 0, propertyValueBytes.Length);
                        }

                        writer.WriteAttributeString(ATTR_VALUE, valueStr);
                        writer.WriteEndElement();
                    }

                    writer.WriteEndElement();
                }

                string exceptionStr = loggingEvent.GetExceptionString();
                if (!string.IsNullOrEmpty(exceptionStr))
                {
                    // Append the stack trace line                    
                    writer.WriteStartElement(this.m_elmException);
                    Transform.WriteEscapedXmlString(writer, exceptionStr, this.InvalidCharReplacement);
                    writer.WriteEndElement();
                }

                if (this.LocationInfo)
                {
                    LocationInfo locationInfo = loggingEvent.LocationInformation;
                    string className = locationInfo.ClassName;
                    string fileName = locationInfo.FileName;

                    //string fullInfo = loggingEvent.LocationInformation.FullInfo;
                    string lineNumber = locationInfo.LineNumber;
                    string methodName = locationInfo.MethodName;
                    writer.WriteStartElement(this.m_elmLocation);
                    //writer.WriteAttributeString(ATTR_CLASS, locationInfo.ClassName);
                    //writer.WriteAttributeString(ATTR_METHOD, locationInfo.MethodName);
                    //writer.WriteAttributeString(ATTR_FILE, locationInfo.FileName);
                    //writer.WriteAttributeString(ATTR_LINE, locationInfo.LineNumber);

                    writer.WriteAttributeString(ATTR_CLASS, className);
                    writer.WriteAttributeString(ATTR_METHOD, methodName);
                    writer.WriteAttributeString(ATTR_FILE, fileName);
                    writer.WriteAttributeString(ATTR_LINE, lineNumber);

                    writer.WriteEndElement();
                }

                writer.WriteEndElement();
            }
            else
            {
                base.FormatXml(writer, loggingEvent);
            }
        }
    }
}