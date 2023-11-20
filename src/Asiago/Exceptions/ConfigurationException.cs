namespace Asiago.Exceptions
{
    [Serializable]
    internal class ConfigurationException : SystemException
    {
        public ConfigurationException()
            : base()
        {
        }

        public ConfigurationException(string? message)
            : base(message)
        {
        }

        public ConfigurationException(string? message, Exception? innerException)
            : base(message, innerException)
        {
        }
    }
}
