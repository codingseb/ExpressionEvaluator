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
                .FluidAdd(XExpressionOperator2.Sharp)
                .FluidAdd(XExpressionOperator2.Degree);

        //protected new static readonly IList<ExpressionOperator> rightOperandOnlyOperatorsEvaluationDictionary = 
        //    ExpressionEvaluator.rightOperandOnlyOperatorsEvaluationDictionary
        //        .ToList();

        protected new static readonly IList<IDictionary<ExpressionOperator, Func<dynamic, dynamic, object>>> operatorsEvaluations =
            ExpressionEvaluator.operatorsEvaluations
                .Copy()
                .AddOperatorEvaluationAtNewLevelAfter(XExpressionOperator2.Sharp, (left, _) => Math.Pow(left, -left), ExpressionOperator.Cast)
                .AddOperatorEvaluationAtNewLevelAfter(XExpressionOperator2.Degree, (left, _) => Math.Pow(left, left), ExpressionOperator.Cast)
                .AddOperatorEvaluationAtLevelOf(XExpressionOperator2.Love, (left, right) => (left | right) << 1, ExpressionOperator.ShiftBitsLeft);

        protected override IList<ExpressionOperator> LeftOperandOnlyOperatorsEvaluationDictionary => leftOperandOnlyOperatorsEvaluationDictionary;

        // protected override IList<ExpressionOperator> RightOperandOnlyOperatorsEvaluationDictionary => rightOperandOnlyOperatorsEvaluationDictionary;

        protected override IList<IDictionary<ExpressionOperator, Func<dynamic, dynamic, object>>> OperatorsEvaluations => operatorsEvaluations;

        protected override void Init()
        {
            operatorsDictionary.Add("#", XExpressionOperator2.Sharp);
            operatorsDictionary.Add("°", XExpressionOperator2.Degree);
            operatorsDictionary.Add("love", XExpressionOperator2.Love);
            operatorsDictionary.Add("Not", ExpressionOperator.LogicalNegation);
        }
    }

    public class XExpressionOperator2 : ExpressionOperator
    {
        public static readonly ExpressionOperator Sharp = new XExpressionOperator2();
        public static readonly ExpressionOperator Degree = new XExpressionOperator2();
        public static readonly ExpressionOperator Love = new XExpressionOperator2();
    }
}
