namespace CodingSeb.ExpressionEvaluator
{
    public enum OptionOnNoReturnKeywordFoundInScriptAction
    {
        ReturnAutomaticallyLastEvaluatedExpression,
        ReturnNull,
        ThrowSyntaxException
    }
}
