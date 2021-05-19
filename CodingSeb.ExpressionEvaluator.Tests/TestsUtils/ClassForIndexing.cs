namespace CodingSeb.ExpressionEvaluator.Tests
{
    public class ClassForIndexing
    {
        public int this[int i1, int i2]
        {
            get
            {
                return i1 + i2;
            }
            set
            {}
        }

        public string this[string test]
        {
            get
            {
                return test + test;
            }
            set
            {}
        }
    }
}
