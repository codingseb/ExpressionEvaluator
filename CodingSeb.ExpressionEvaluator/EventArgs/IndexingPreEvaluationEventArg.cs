using System;
using System.Collections.Generic;

namespace CodingSeb.ExpressionEvaluator
{
    /// <summary>
    /// Infos about the indexing that is currently evaluate
    /// </summary>
    public partial class IndexingPreEvaluationEventArg : EventArgs
    {
        public IndexingPreEvaluationEventArg(List<string> args, ExpressionEvaluator evaluator, object onInstance)
        {
            Args = args;
            This = onInstance;
            Evaluator = evaluator;
        }

        /// <summary>
        /// The not evaluated args of the indexing
        /// </summary>
        public List<string> Args { get; } = new List<string>();

        /// <summary>
        /// The instance of the object on which the indexing is called.
        /// </summary>
        public object This { get; }

        private object returnValue;

        /// <summary>
        /// To set the result value of the indexing
        /// </summary>
        public object Value
        {
            get { return returnValue; }
            set
            {
                returnValue = value;
                HasValue = true;
            }
        }

        /// <summary>
        /// if <c>true</c> the indexing evaluation has been done, if <c>false</c> it means that the indexing does not exist.
        /// </summary>
        public bool HasValue { get; set; }

        /// <summary>
        /// A reference on the current expression evaluator.
        /// </summary>
        public ExpressionEvaluator Evaluator { get; }

        /// <summary>
        /// Get the values of the indexing's args.
        /// </summary>
        /// <returns></returns>
        public object[] EvaluateArgs()
        {
            return Args.ConvertAll(arg => Evaluator.Evaluate(arg)).ToArray();
        }

        /// <summary>
        /// Get the value of the indexing's arg at the specified index
        /// </summary>
        /// <param name="index">The index of the indexing's arg to evaluate</param>
        /// <returns>The evaluated arg</returns>
        public object EvaluateArg(int index)
        {
            return Evaluator.Evaluate(Args[index]);
        }

        /// <summary>
        /// Get the value of the indexing's arg at the specified index
        /// </summary>
        /// <typeparam name="T">The type of the result to get. (Do a cast)</typeparam>
        /// <param name="index">The index of the indexing's arg to evaluate</param>
        /// <returns>The evaluated arg casted in the specified type</returns>
        public T EvaluateArg<T>(int index)
        {
            return Evaluator.Evaluate<T>(Args[index]);
        }

        /// <summary>
        /// If set to true cancel the evaluation of the current function or method and throw an exception that the function does not exists
        /// </summary>
        public bool CancelEvaluation { get; set; }
    }
}
