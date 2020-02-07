namespace CodingSeb.ExpressionEvaluator.Tests
{
    public class ContextObject1
    {
        public int AIntValue { get; set; } = 3;

        public string SayHelloTo(string name)
        {
            return "Hello " + name;
        }
    }
}
