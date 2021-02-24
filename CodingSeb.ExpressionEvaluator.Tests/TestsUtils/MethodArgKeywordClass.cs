using System.Linq;

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

        public int SumOf(int val1, int val2, params int[] otherVals)
        {
            return val1 + val2 + otherVals.Sum();
        }

        public int SumOf2(params int[] otherVals)
        {
            return otherVals.Sum();
        }

        public void Join(out string result, string separator, params object[] values)
        {
            result = string.Join(separator, values);
        }
    }

    public static class MethodArgKeywordClassExtension
    {
        public static string UseAsSepForJoin(this string separator, params object[] values)
        {
            return string.Join(separator, values);
        }

        public static void UseAsSepForJoin(this string separator, ref string result, params object[] values)
        {
            result =  string.Join(separator, values);
        }
    }
}
