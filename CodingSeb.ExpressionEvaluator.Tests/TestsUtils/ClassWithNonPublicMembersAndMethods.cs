#pragma warning disable 414
namespace CodingSeb.ExpressionEvaluator.Tests
{
    public class ClassWithNonPublicMembersAndMethods
    {
        private int myPrivateField = 5;

        protected int myProtectedField = 10;

        private int MyPrivateProperty { get; set; } = 15;

        protected int MyProtectedProperty { get; set; } = 20;

        private string MyPrivateMethod(string name)
        {
            return "Hello " + name;
        }

        protected string MyProtectedMethod(string name)
        {
            return "GoodBye " + name;
        }
    }
}
