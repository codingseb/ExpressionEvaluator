using System;
using System.Reflection;

namespace CodingSeb.ExpressionEvaluator
{
    /// <summary>
    /// Class ParameterCastEvaluationEventArg.
    /// Implements the <see cref="System.EventArgs" /></summary>
    /// <seealso cref="System.EventArgs" />
    public partial class ParameterCastEvaluationEventArg : EventArgs
    {
        /// <summary>
        /// The information of the method that it try to call
        /// </summary>
        public MethodInfo MethodInfo { get; }

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
        /// The required type of the parameter
        /// </summary>
        public Type ParameterType { get; }

        /// <summary>
        /// The original argument
        /// </summary>
        public object OriginalArg { get; }

        /// <summary>
        /// Position of the argument (index from 0)
        /// </summary>
        public int ArgPosition { get; }

        public ParameterCastEvaluationEventArg(MethodInfo methodInfo, Type parameterType, object originalArg, int argPosition, ExpressionEvaluator evaluator = null, object onInstance = null)
        {
            MethodInfo = methodInfo;
            ParameterType = parameterType;
            OriginalArg = originalArg;
            Evaluator = evaluator;
            This = onInstance;
            ArgPosition = argPosition;
        }

        private object modifiedArgument;

        /// <summary>
        /// To set the modified argument
        /// </summary>
        public object Argument
        {
            get { return modifiedArgument; }
            set
            {
                modifiedArgument = value;
                FunctionModifiedArgument = true;
            }
        }

        /// <summary>
        /// if <c>true</c> the argument has been modified, if <c>false</c> it means that the argument can't be of the given type.
        /// </summary>
        public bool FunctionModifiedArgument { get; set; }
    }
}
