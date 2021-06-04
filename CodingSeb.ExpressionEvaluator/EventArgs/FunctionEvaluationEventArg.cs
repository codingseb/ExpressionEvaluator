using System;
using System.Collections.Generic;

namespace CodingSeb.ExpressionEvaluator
{
    /// <summary>
    /// Infos about the function or method that is currently evaluate
    /// </summary>
    public partial class FunctionEvaluationEventArg : EventArgs
    {
        private readonly Func<string, Type[]> evaluateGenericTypes;
        private readonly string genericTypes;

        public FunctionEvaluationEventArg(string name, List<string> args = null, ExpressionEvaluator evaluator = null, object onInstance = null, string genericTypes = null, Func<string, Type[]> evaluateGenericTypes = null)
        {
            Name = name;
            Args = args ?? new List<string>();
            This = onInstance;
            Evaluator = evaluator;
            this.genericTypes = genericTypes;
            this.evaluateGenericTypes = evaluateGenericTypes;
        }

        /// <summary>
        /// The not evaluated args of the function
        /// </summary>
        public List<string> Args { get; } = new List<string>();

        /// <summary>
        /// Get the values of the function's args.
        /// </summary>
        /// <returns></returns>
        public object[] EvaluateArgs()
        {
            return Args.ConvertAll(arg => Evaluator.Evaluate(arg)).ToArray();
        }

        /// <summary>
        /// Get the value of the function's arg at the specified index
        /// </summary>
        /// <param name="index">The index of the function's arg to evaluate</param>
        /// <returns>The evaluated arg</returns>
        public object EvaluateArg(int index)
        {
            return Evaluator.Evaluate(Args[index]);
        }

        /// <summary>
        /// Get the value of the function's arg at the specified index
        /// </summary>
        /// <typeparam name="T">The type of the result to get. (Do a cast)</typeparam>
        /// <param name="index">The index of the function's arg to evaluate</param>
        /// <returns>The evaluated arg casted in the specified type</returns>
        public T EvaluateArg<T>(int index)
        {
            return Evaluator.Evaluate<T>(Args[index]);
        }

        /// <summary>
        /// The name of the variable to Evaluate
        /// </summary>
        public string Name { get; }

        private object returnValue;

        /// <summary>
        /// To set the return value of the function
        /// </summary>
        public object Value
        {
            get { return returnValue; }
            set
            {
                returnValue = value;
                FunctionReturnedValue = true;
            }
        }

        /// <summary>
        /// If <c>true</c> the function evaluation has been done, if <c>false</c> it means that the function does not exist.
        /// </summary>
        public bool FunctionReturnedValue { get; set; }

        /// <summary>
        /// In the case of on the fly instance method definition the instance of the object on which this method (function) is called.
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
