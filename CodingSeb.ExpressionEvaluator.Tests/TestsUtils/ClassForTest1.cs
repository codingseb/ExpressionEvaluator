using System;

namespace CodingSeb.ExpressionEvaluator.Tests
{
    public class ClassForTest1
    {
        public int intField = 12;

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

        public Func<int, int, int> AddAsDelegate { get; set; } = (nb1, nb2) => nb1 + nb2;
        public static Func<int, int, int> AddAsStaticDelegate { get; set; } = (nb1, nb2) => nb1 + nb2;
    }
}
