using Lanka.Common.Domain;

namespace Lanka.Common.Application.Exceptions
{
    public sealed class LankaException : Exception
    {
        public string RequestName { get; }

        public Error? Error { get; }
        
        public LankaException() { }
        
        public LankaException(string message) 
            : base(message) { }
        
        public LankaException(string message, Exception inner)
            : base(message, inner) { }

        public LankaException(string requestName, Error? error, Exception? inner)
            : base("Application exception", inner)
        {
            this.RequestName = requestName;
            this.Error = error;
        }
    }
}
