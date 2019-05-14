namespace CodingSeb.ExpressionEvaluator.Tests
{
    public class XExpressionEvaluator2 : ExpressionEvaluator
    {
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
