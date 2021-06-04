using System.Reflection;

namespace CodingSeb.ExpressionEvaluator.Containers
{
    /// <summary>
    /// This class is a container used to store a groups of overrides method until the real call of the method to select which override of the method to use.
    /// </summary>
    public partial class MethodsGroupEncaps
    {
        public object ContainerObject { get; set; }

        public MethodInfo[] MethodsGroup { get; set; }
    }
}
