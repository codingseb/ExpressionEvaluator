using System;

namespace CodingSeb.ExpressionEvaluator.Exceptions
{
    /// <summary>
    /// An Exception thrown when script or exception try to access type or object that are blocked for security reasons
    /// </summary>
    public partial class ExpressionEvaluatorSecurityException : Exception
    {
        public ExpressionEvaluatorSecurityException()
        { }

        public ExpressionEvaluatorSecurityException(string message) : base(message)
        { }

        public ExpressionEvaluatorSecurityException(string message, Exception innerException) : base(message, innerException)
        { }
    }
}
