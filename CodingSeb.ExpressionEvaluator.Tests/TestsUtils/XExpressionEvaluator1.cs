namespace CodingSeb.ExpressionEvaluator.Tests
{
    public class XExpressionEvaluator1 : ExpressionEvaluator
    {
        protected override void Init()
        {
            operatorsDictionary.Add("and", ExpressionOperator.ConditionalAnd);
            operatorsDictionary.Add("or", ExpressionOperator.ConditionalOr);
            operatorsDictionary.Remove("&&");
            operatorsDictionary.Remove("||");
        }
    }
}
