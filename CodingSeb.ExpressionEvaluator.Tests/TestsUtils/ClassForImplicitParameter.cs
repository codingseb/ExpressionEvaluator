using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodingSeb.ExpressionEvaluator.Tests.TestsUtils
{
    public readonly struct StringLikeParameter
    {
        public readonly string Value;

        public StringLikeParameter(string s)
        {
            Value = s;
        }

        public static implicit operator StringLikeParameter(string s) => new StringLikeParameter(s);
    }
}
