using System;
using System.Collections.Generic;

namespace CodingSeb.ExpressionEvaluator
{
    /// <summary>
    /// Infos about the function or method that is currently evaluate
    /// </summary>
    public partial class FunctionPreEvaluationEventArg : FunctionEvaluationEventArg
    {
        public FunctionPreEvaluationEventArg(string name, List<string> args = null, ExpressionEvaluator evaluator = null, object onInstance = null, string genericTypes = null, Func<string, Type[]> evaluateGenericTypes = null)
            : base(name, args, evaluator, onInstance, genericTypes, evaluateGenericTypes)
        { }

        /// <summary>
        /// If set to true cancel the evaluation of the current function or method and throw an exception that the function does not exists
        /// </summary>
        public bool CancelEvaluation { get; set; }
    }
}
