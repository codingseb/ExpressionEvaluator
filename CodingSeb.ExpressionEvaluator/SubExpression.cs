namespace CodingSeb.ExpressionEvaluator
{
    /// <summary>
    /// Allow to store sub expression in variables. To compose a bigger expression with variable evaluate as expressions and not string
    /// </summary>
    public partial class SubExpression
    {
        public string Expression { get; set; }

        public SubExpression(string expression)
        {
            Expression = expression;
        }
    }
}
