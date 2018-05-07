namespace CodingSeb.ExpressionEvaluator.Tests
{
    public class ClassForTest1
    {
        public int IntProperty { get; set; } = 25;

        public static int StaticIntProperty { get; set; } = 67;

        public int Add3To(int value)
        {
            return value + 3;
        }

        public static string StaticStringMethod(string Name)
        {
            return $"Hello {Name}";
        }
    }
}
