using System;

namespace ForzaRadioModTool.Core.Models
{
    /// <summary>
    /// Custom exceptions for the Forza Radio Mod Tool
    /// </summary>
    public class RadioToolException : Exception
    {
        public RadioToolException(string message) : base(message) { }
        public RadioToolException(string message, Exception innerException) : base(message, innerException) { }
    }

    public class ConfigurationException : RadioToolException
    {
        public ConfigurationException(string message) : base(message) { }
        public ConfigurationException(string message, Exception innerException) : base(message, innerException) { }
    }

    public class AudioProcessingException : RadioToolException
    {
        public AudioProcessingException(string message) : base(message) { }
        public AudioProcessingException(string message, Exception innerException) : base(message, innerException) { }
    }

    public class XmlProcessingException : RadioToolException
    {
        public XmlProcessingException(string message) : base(message) { }
        public XmlProcessingException(string message, Exception innerException) : base(message, innerException) { }
    }

    public class FileOperationException : RadioToolException
    {
        public FileOperationException(string message) : base(message) { }
        public FileOperationException(string message, Exception innerException) : base(message, innerException) { }
    }
}