using System;

namespace CodingSeb.ExpressionEvaluator.Containers
{
    /// <summary>
    /// This class is a container for typed variable declaration. To force the evaluator to only allow to store variable of this type.
    /// </summary>
    public partial class StronglyTypedVariable
    {
        public Type Type { get; set; }

        public object Value { get; set; }
    }
}
