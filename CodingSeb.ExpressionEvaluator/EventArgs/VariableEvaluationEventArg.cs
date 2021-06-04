using System;

namespace CodingSeb.ExpressionEvaluator
{
    /// <summary>
    /// Infos about the variable, attribut or property that is currently evaluate
    /// </summary>
    public partial class VariableEvaluationEventArg : EventArgs
    {
        private readonly Func<string, Type[]> evaluateGenericTypes;
        private readonly string genericTypes;

        /// <summary>
        /// Constructor of the VariableEvaluationEventArg
        /// </summary>
        /// <param name="name">The name of the variable to Evaluate</param>
        public VariableEvaluationEventArg(string name, ExpressionEvaluator evaluator = null, object onInstance = null, string genericTypes = null, Func<string, Type[]> evaluateGenericTypes = null)
        {
            Name = name;
            This = onInstance;
            Evaluator = evaluator;
            this.genericTypes = genericTypes;
            this.evaluateGenericTypes = evaluateGenericTypes;
        }

        /// <summary>
        /// The name of the variable to Evaluate
        /// </summary>
        public string Name { get; }

        private object varValue;

        /// <summary>
        /// To set a value to this variable
        /// </summary>
        public object Value
        {
            get { return varValue; }
            set
            {
                varValue = value;
                HasValue = true;
            }
        }

        /// <summary>
        /// If <c>true</c> the variable is affected, if <c>false</c> it means that the variable does not exist.
        /// </summary>
        public bool HasValue { get; set; }

        /// <summary>
        /// In the case of on the fly instance property definition the instance of the object on which this Property is called.
        /// Otherwise is set to null.
        /// </summary>
        public object This { get; }

        /// <summary>
        /// A reference on the current expression evaluator.
        /// </summary>
        public ExpressionEvaluator Evaluator { get; }

        /// <summary>
        /// Is <c>true</c> if some generic types are specified with &lt;&gt;.
        /// <c>false</c> otherwise
        /// </summary>
        public bool HasGenericTypes
        {
            get
            {
                return !string.IsNullOrEmpty(genericTypes);
            }
        }

        /// <summary>
        /// In the case where generic types are specified with &lt;&gt;
        /// Evaluate all types and return an array of types
        /// </summary>
        public Type[] EvaluateGenericTypes()
        {
            return evaluateGenericTypes?.Invoke(genericTypes) ?? new Type[0];
        }
    }
}
