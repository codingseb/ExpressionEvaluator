using System;

namespace CodingSeb.ExpressionEvaluator.Containers
{
    /// <summary>
    /// This class is a container for exception that need to be bubble up before to be thrown.
    /// Avoid to assign exception to variables.
    /// </summary>
    public partial class BubbleExceptionContainer
    {
        public Exception Exception { get; set; }
    }
}
