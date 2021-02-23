namespace CodingSeb.ExpressionEvaluator.Tests
{
    public class MethodArgKeywordClass
    {
        public void AddOneOnRef(ref int i)
        {
            i++;
        }

        public int AddOneOnIn(in int i)
        {
            return i + 1;
        }
    }
}
