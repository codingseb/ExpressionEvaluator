using System;

namespace CodingSeb.ExpressionEvaluator.Exceptions
{
    /// <summary>
    /// An exception thrown when a syntax error is found in an expression or a script
    /// </summary>
    public partial class ExpressionEvaluatorSyntaxErrorException : Exception
    {
        public ExpressionEvaluatorSyntaxErrorException()
        { }

        public ExpressionEvaluatorSyntaxErrorException(string message) : base(message)
        { }

        public ExpressionEvaluatorSyntaxErrorException(string message, Exception innerException) : base(message, innerException)
        { }
    }

}
