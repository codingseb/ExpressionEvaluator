namespace CodingSeb.ExpressionEvaluator.Tests
{
    public class ClassWithNonPublicMembersAndMethods
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "RCS1169:Make field read-only.", Justification = "<Pending>")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Redundancy", "RCS1213:Remove unused member declaration.", Justification = "<Pending>")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0044:Add readonly modifier", Justification = "<Pending>")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "<Pending>")]
        private int myPrivateField = 5;

        protected int myProtectedField = 10;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "RCS1170:Use read-only auto-implemented property.", Justification = "<Pending>")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Redundancy", "RCS1213:Remove unused member declaration.", Justification = "<Pending>")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "<Pending>")]
        private int MyPrivateProperty { get; set; } = 15;

        protected int MyProtectedProperty { get; set; } = 20;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Redundancy", "RCS1213:Remove unused member declaration.", Justification = "<Pending>")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "<Pending>")]
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
