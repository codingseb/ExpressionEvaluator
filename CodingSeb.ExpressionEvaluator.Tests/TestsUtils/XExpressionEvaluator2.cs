using System;
using System.Collections.Generic;
using System.Linq;

namespace CodingSeb.ExpressionEvaluator.Tests
{
    public class XExpressionEvaluator2 : ExpressionEvaluator
    {
        protected new static readonly IList<ExpressionOperator> leftOperandOnlyOperatorsEvaluationDictionary =
            ExpressionEvaluator.leftOperandOnlyOperatorsEvaluationDictionary
                .ToList()
                .FluidAdd(XExpressionOperator2.Sharp);

        //protected new static readonly IList<ExpressionOperator> rightOperandOnlyOperatorsEvaluationDictionary = 
        //    ExpressionEvaluator.rightOperandOnlyOperatorsEvaluationDictionary
        //        .ToList();

        protected new static readonly IList<IDictionary<ExpressionOperator, Func<dynamic, dynamic, object>>> operatorsEvaluations =
            ExpressionEvaluator.operatorsEvaluations
                .Copy()
                .AddOperatorEvaluationAtNewLevelAfter(XExpressionOperator2.Sharp, (left, _) => Math.Pow(left, -left), ExpressionOperator.UnaryPlus)
                .AddOperatorEvaluationAtLevelOf(XExpressionOperator2.Love, (left, right) => (left | right) << 1, ExpressionOperator.ShiftBitsLeft);

        protected override IList<ExpressionOperator> LeftOperandOnlyOperatorsEvaluationDictionary => leftOperandOnlyOperatorsEvaluationDictionary;

        // protected override IList<ExpressionOperator> RightOperandOnlyOperatorsEvaluationDictionary => rightOperandOnlyOperatorsEvaluationDictionary;

        protected override IList<IDictionary<ExpressionOperator, Func<dynamic, dynamic, object>>> OperatorsEvaluations => operatorsEvaluations;

        protected override void Init()
        {
            operatorsDictionary.Add("#", XExpressionOperator2.Sharp);
            operatorsDictionary.Add("love", XExpressionOperator2.Love);
            operatorsDictionary.Add("Not", ExpressionOperator.LogicalNegation);
        }
    }

    public class XExpressionOperator2 : ExpressionOperator
    {
        public static readonly ExpressionOperator Sharp = new XExpressionOperator2();
        public static readonly ExpressionOperator Love = new XExpressionOperator2();
    }
}
