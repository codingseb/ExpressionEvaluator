using System;

namespace CodingSeb.ExpressionEvaluator.Operators
{
    /// <summary>
    /// Inherit from this class to define your own simple operator
    /// </summary>
    public partial class ExpressionOperator : IEquatable<ExpressionOperator>
    {
        protected static uint indexer;

        protected ExpressionOperator()
        {
            indexer++;
            OperatorValue = indexer;
        }

        protected uint OperatorValue { get; }

        public static readonly ExpressionOperator Plus = new ExpressionOperator();
        public static readonly ExpressionOperator Minus = new ExpressionOperator();
        public static readonly ExpressionOperator UnaryPlus = new ExpressionOperator();
        public static readonly ExpressionOperator UnaryMinus = new ExpressionOperator();
        public static readonly ExpressionOperator Multiply = new ExpressionOperator();
        public static readonly ExpressionOperator Divide = new ExpressionOperator();
        public static readonly ExpressionOperator Modulo = new ExpressionOperator();
        public static readonly ExpressionOperator Lower = new ExpressionOperator();
        public static readonly ExpressionOperator Greater = new ExpressionOperator();
        public static readonly ExpressionOperator Equal = new ExpressionOperator();
        public static readonly ExpressionOperator LowerOrEqual = new ExpressionOperator();
        public static readonly ExpressionOperator GreaterOrEqual = new ExpressionOperator();
        public static readonly ExpressionOperator Is = new ExpressionOperator();
        public static readonly ExpressionOperator NotEqual = new ExpressionOperator();
        public static readonly ExpressionOperator LogicalNegation = new ExpressionOperator();
        public static readonly ExpressionOperator BitwiseComplement = new ExpressionOperator();
        public static readonly ExpressionOperator ConditionalAnd = new ExpressionOperator();
        public static readonly ExpressionOperator ConditionalOr = new ExpressionOperator();
        public static readonly ExpressionOperator LogicalAnd = new ExpressionOperator();
        public static readonly ExpressionOperator LogicalOr = new ExpressionOperator();
        public static readonly ExpressionOperator LogicalXor = new ExpressionOperator();
        public static readonly ExpressionOperator ShiftBitsLeft = new ExpressionOperator();
        public static readonly ExpressionOperator ShiftBitsRight = new ExpressionOperator();
        public static readonly ExpressionOperator NullCoalescing = new ExpressionOperator();
        public static readonly ExpressionOperator Cast = new ExpressionOperator();

        public override bool Equals(object obj)
        {
            if (obj is ExpressionOperator otherOperator)
                return Equals(otherOperator);
            else
                return OperatorValue.Equals(obj);
        }

        public override int GetHashCode()
        {
            return OperatorValue.GetHashCode();
        }

        public bool Equals(ExpressionOperator otherOperator)
        {
            return otherOperator != null && OperatorValue == otherOperator.OperatorValue;
        }
    }
}
