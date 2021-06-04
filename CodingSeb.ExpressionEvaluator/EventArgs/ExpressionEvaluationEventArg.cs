using System;

namespace CodingSeb.ExpressionEvaluator
{
    /// <summary>
    /// Infos about the expression that is currently evaluate
    /// </summary>
    public partial class ExpressionEvaluationEventArg : EventArgs
    {
        private object value;

        public ExpressionEvaluationEventArg(string expression, ExpressionEvaluator evaluator)
        {
            Expression = expression;
            Evaluator = evaluator;
        }

        public ExpressionEvaluationEventArg(string expression, ExpressionEvaluator evaluator, object value)
        {
            Expression = expression;
            Evaluator = evaluator;
            this.value = value;
        }

        public ExpressionEvaluator Evaluator { get; }

        /// <summary>
        /// The Expression that will be evaluated.
        /// Can be modified.
        /// </summary>
        public string Expression { get; set; }

        /// <summary>
        /// To set the return of the evaluation
        /// </summary>
        public object Value
        {
            get { return value; }
            set
            {
                this.value = value;
                HasValue = true;
            }
        }

        /// <summary>
        /// if <c>true</c> the expression evaluation has been done, if <c>false</c> it means that the evaluation must continue.
        /// </summary>
        public bool HasValue { get; set; }
    }
}
