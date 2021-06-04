using System;
using System.Collections.Generic;
using System.Linq;

namespace CodingSeb.ExpressionEvaluator.Operators
{
    /// <summary>
    /// This class provides extensions methods to easily customize operators list.
    /// </summary>
    public static partial class OperatorsEvaluationsExtensions
    {
        public static IList<IDictionary<ExpressionOperator, Func<dynamic, dynamic, object>>> Copy(this IList<IDictionary<ExpressionOperator, Func<dynamic, dynamic, object>>> operatorsEvaluations)
        {
            return operatorsEvaluations
                .Select(dic => (IDictionary<ExpressionOperator, Func<dynamic, dynamic, object>>)new Dictionary<ExpressionOperator, Func<dynamic, dynamic, object>>(dic))
                .ToList();
        }

        public static IList<IDictionary<ExpressionOperator, Func<dynamic, dynamic, object>>> AddOperatorEvaluationAtLevelOf(this IList<IDictionary<ExpressionOperator, Func<dynamic, dynamic, object>>> operatorsEvaluations, ExpressionOperator operatorToAdd, Func<dynamic, dynamic, object> evaluation, ExpressionOperator levelOfThisOperator)
        {
            operatorsEvaluations
                .First(dict => dict.ContainsKey(levelOfThisOperator))
                .Add(operatorToAdd, evaluation);

            return operatorsEvaluations;
        }

        public static IList<IDictionary<ExpressionOperator, Func<dynamic, dynamic, object>>> AddOperatorEvaluationAtLevel(this IList<IDictionary<ExpressionOperator, Func<dynamic, dynamic, object>>> operatorsEvaluations, ExpressionOperator operatorToAdd, Func<dynamic, dynamic, object> evaluation, int level)
        {
            operatorsEvaluations[level]
                .Add(operatorToAdd, evaluation);

            return operatorsEvaluations;
        }

        public static IList<IDictionary<ExpressionOperator, Func<dynamic, dynamic, object>>> AddOperatorEvaluationAtNewLevelAfter(this IList<IDictionary<ExpressionOperator, Func<dynamic, dynamic, object>>> operatorsEvaluations, ExpressionOperator operatorToAdd, Func<dynamic, dynamic, object> evaluation, ExpressionOperator levelOfThisOperator)
        {
            int level = operatorsEvaluations
                .IndexOf(operatorsEvaluations.First(dict => dict.ContainsKey(levelOfThisOperator)));

            operatorsEvaluations
                .Insert(level + 1, new Dictionary<ExpressionOperator, Func<dynamic, dynamic, object>> { { operatorToAdd, evaluation } });

            return operatorsEvaluations;
        }

        public static IList<IDictionary<ExpressionOperator, Func<dynamic, dynamic, object>>> AddOperatorEvaluationAtNewLevelBefore(this IList<IDictionary<ExpressionOperator, Func<dynamic, dynamic, object>>> operatorsEvaluations, ExpressionOperator operatorToAdd, Func<dynamic, dynamic, object> evaluation, ExpressionOperator levelOfThisOperator)
        {
            int level = operatorsEvaluations
                .IndexOf(operatorsEvaluations.First(dict => dict.ContainsKey(levelOfThisOperator)));

            operatorsEvaluations
                .Insert(level, new Dictionary<ExpressionOperator, Func<dynamic, dynamic, object>> { { operatorToAdd, evaluation } });

            return operatorsEvaluations;
        }

        public static IList<IDictionary<ExpressionOperator, Func<dynamic, dynamic, object>>> AddOperatorEvaluationAtNewLevel(this IList<IDictionary<ExpressionOperator, Func<dynamic, dynamic, object>>> operatorsEvaluations, ExpressionOperator operatorToAdd, Func<dynamic, dynamic, object> evaluation, int level)
        {
            operatorsEvaluations
                .Insert(level, new Dictionary<ExpressionOperator, Func<dynamic, dynamic, object>> { { operatorToAdd, evaluation } });

            return operatorsEvaluations;
        }

        public static IList<IDictionary<ExpressionOperator, Func<dynamic, dynamic, object>>> RemoveOperatorEvaluation(this IList<IDictionary<ExpressionOperator, Func<dynamic, dynamic, object>>> operatorsEvaluations, ExpressionOperator operatorToRemove)
        {
            operatorsEvaluations.First(dict => dict.ContainsKey(operatorToRemove)).Remove(operatorToRemove);

            return operatorsEvaluations;
        }

        public static IList<ExpressionOperator> FluidAdd(this IList<ExpressionOperator> listOfOperator, ExpressionOperator operatorToAdd)
        {
            listOfOperator.Add(operatorToAdd);

            return listOfOperator;
        }

        public static IList<ExpressionOperator> FluidRemove(this IList<ExpressionOperator> listOfOperator, ExpressionOperator operatorToRemove)
        {
            listOfOperator.Remove(operatorToRemove);

            return listOfOperator;
        }
    }
}
