using System;

namespace CodingSeb.ExpressionEvaluator.Tests
{
    public class XExpressionEvaluator2 : ExpressionEvaluator
    {
        public static void StaticInit()
        {
            leftOperandOnlyOperatorsEvaluationDictionary.Add(XExpressionOperator2.Sharp);

            operatorsEvaluations
                .AddOperatorEvaluationAtNewLevelAfter(XExpressionOperator2.Sharp, (left, _) => Math.Pow(left, -left), ExpressionOperator.UnaryPlus)
                .AddOperatorEvaluationAtLevelOf(XExpressionOperator2.Love, (left, right) => (left | right) << 1, ExpressionOperator.ShiftBitsLeft);
        }

        protected override void Init()
        {
            operatorsDictionary.Add("#", XExpressionOperator2.Sharp);
            operatorsDictionary.Add("love", XExpressionOperator2.Love);
        }
    }

    public class XExpressionOperator2 : ExpressionOperator
    {
        public static readonly ExpressionOperator Sharp = new XExpressionOperator2();
        public static readonly ExpressionOperator Love = new XExpressionOperator2();
    }
}
