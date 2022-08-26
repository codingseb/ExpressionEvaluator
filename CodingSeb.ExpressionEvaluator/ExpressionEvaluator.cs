/******************************************************************************************************
    Title : ExpressionEvaluator (https://github.com/codingseb/ExpressionEvaluator)
    Version : 1.4.39.0 
    (if last digit (the forth) is not a zero, the version is an intermediate version and can be unstable)

    Author : Coding Seb
    Licence : MIT (https://github.com/codingseb/ExpressionEvaluator/blob/master/LICENSE.md)
*******************************************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace CodingSeb.ExpressionEvaluator
{
    /// <summary>
    /// This class allow to evaluate a string math or pseudo C# expression/script
    /// </summary>
    public partial class ExpressionEvaluator
    {
        #region Regex declarations

        protected static readonly Regex varOrFunctionRegEx = new Regex(@"^((?<sign>[+-])|(?<prefixOperator>[+][+]|--)|(?<varKeyword>var)\s+|(?<dynamicKeyword>dynamic)\s+|((?<nullConditional>[?])?(?<inObject>\.))?)(?<name>[\p{L}_](?>[\p{L}_0-9]*))(?>\s*)((?<assignationOperator>(?<assignmentPrefix>[+\-*/%&|^]|<<|>>|\?\?)?=(?![=>]))|(?<postfixOperator>([+][+]|--)(?![\p{L}_0-9]))|((?<isgeneric>[<](?>([\p{L}_](?>[\p{L}_0-9]*)|(?>\s+)|[,\.])+|(?<gentag>[<])|(?<-gentag>[>]))*(?(gentag)(?!))[>])?(?<isfunction>[(])?))", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        //language=regex
        protected const string numberRegexOrigPattern = @"^(?<sign>[+-])?([0-9][0-9_{1}]*[0-9]|\d)(?<hasdecimal>{0}?([0-9][0-9_]*[0-9]|\d)(e[+-]?([0-9][0-9_]*[0-9]|\d))?)?(?<type>ul|[fdulm])?";
        protected string numberRegexPattern;

        protected static readonly Regex otherBasesNumberRegex = new Regex("^(?<sign>[+-])?(?<value>0(?<type>x)([0-9a-f][0-9a-f_]*[0-9a-f]|[0-9a-f])|0(?<type>b)([01][01_]*[01]|[01]))", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        protected static readonly Regex stringBeginningRegex = new Regex("^(?<interpolated>[$])?(?<escaped>[@])?[\"]", RegexOptions.Compiled);
        protected static readonly Regex internalCharRegex = new Regex(@"^['](\\[\\'0abfnrtv]|[^'])[']", RegexOptions.Compiled);
        protected static readonly Regex indexingBeginningRegex = new Regex(@"^(?<nullConditional>[?])?\[", RegexOptions.Compiled);
        protected static readonly Regex arrayTypeDetectionRegex = new Regex(@"^(\s*(\[(?>(?>\s+)|[,])*)\])+", RegexOptions.Compiled);
        protected static readonly Regex assignationOrPostFixOperatorRegex = new Regex(@"^(?>\s*)((?<assignmentPrefix>[+\-*/%&|^]|<<|>>|\?\?)?=(?![=>])|(?<postfixOperator>([+][+]|--)(?![\p{L}_0-9])))");
        protected static readonly Regex genericsDecodeRegex = new Regex("(?<name>[^,<>]+)(?<isgeneric>[<](?>[^<>]+|(?<gentag>[<])|(?<-gentag>[>]))*(?(gentag)(?!))[>])?", RegexOptions.Compiled);
        protected static readonly Regex genericsEndOnlyOneTrim = new Regex(@"(?>\s*)[>](?>\s*)$", RegexOptions.Compiled);

        protected static readonly Regex endOfStringWithDollar = new Regex("^([^\"{\\\\]|\\\\[\\\\\"0abfnrtv])*[\"{]", RegexOptions.Compiled);
        protected static readonly Regex endOfStringWithoutDollar = new Regex("^([^\"\\\\]|\\\\[\\\\\"0abfnrtv])*[\"]", RegexOptions.Compiled);
        protected static readonly Regex endOfStringWithDollarWithAt = new Regex("^[^\"{]*[\"{]", RegexOptions.Compiled);
        protected static readonly Regex endOfStringWithoutDollarWithAt = new Regex("^[^\"]*[\"]", RegexOptions.Compiled);
        protected static readonly Regex endOfStringInterpolationRegex = new Regex("^('\"'|[^}\"])*[}\"]", RegexOptions.Compiled);
        protected static readonly Regex stringBeginningForEndBlockRegex = new Regex("[$]?[@]?[\"]$", RegexOptions.Compiled);
        protected static readonly Regex lambdaExpressionRegex = new Regex(@"^(?>\s*)(?<args>((?>\s*)[(](?>\s*)([\p{L}_](?>[\p{L}_0-9]*)(?>\s*)([,](?>\s*)[\p{L}_][\p{L}_0-9]*(?>\s*))*)?[)])|[\p{L}_](?>[\p{L}_0-9]*))(?>\s*)=>(?<expression>.*)$", RegexOptions.Singleline | RegexOptions.Compiled);
        protected static readonly Regex lambdaArgRegex = new Regex(@"[\p{L}_](?>[\p{L}_0-9]*)", RegexOptions.Compiled);
        protected static readonly Regex initInNewBeginningRegex = new Regex(@"^(?>\s*){", RegexOptions.Compiled);
        protected static readonly Regex functionArgKeywordsRegex = new Regex(@"^\s*(?<keyword>out|ref|in)\s+((?<typeName>[\p{L}_][\p{L}_0-9\.\[\]<>]*[?]?)\s+(?=[\p{L}_]))?(?<toEval>(?<varName>[\p{L}_](?>[\p{L}_0-9]*))\s*(=.*)?)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        protected static readonly Regex instanceCreationWithNewKeywordRegex = new Regex(@"^new(?=[^\w])\s*((?<isAnonymous>[{{])|((?<name>[\p{L}_][\p{L}_0-9\.]*)(?>\s*)(?<isgeneric>[<](?>[^<>]+|(?<gentag>[<])|(?<-gentag>[>]))*(?(gentag)(?!))[>])?(?>\s*)((?<isfunction>[(])|(?<isArray>\[)|(?<isInit>[{{]))?))", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        protected string CastRegexPattern { get { return @"^\((?>\s*)(?<typeName>[\p{L}_][\p{L}_0-9\.\[\]<>]*[?]?)(?>\s*)\)"; } }

        // To remove comments in scripts based on https://stackoverflow.com/questions/3524317/regex-to-strip-line-comments-from-c-sharp/3524689#3524689
        //language=regex
        protected const string blockComments = @"/\*(.*?)\*/";
        //language=regex
        protected const string lineComments = @"//[^\r\n]*";
        //language=regex
        protected const string stringsIgnore = @"""((\\[^\n]|[^""\n])*)""";
        //language=regex
        protected const string verbatimStringsIgnore = @"@(""[^""]*"")+";
        protected static readonly Regex removeCommentsRegex = new Regex($"{blockComments}|{lineComments}|{stringsIgnore}|{verbatimStringsIgnore}", RegexOptions.Singleline | RegexOptions.Compiled);
        protected static readonly Regex newLineCharsRegex = new Regex(@"\r\n|\r|\n", RegexOptions.Compiled);

        // For script only
        protected static readonly Regex blockKeywordsBeginningRegex = new Regex(@"^(?>\s*)(?<keyword>while|for|foreach|if|else(?>\s*)if|catch)(?>\s*)[(]", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        protected static readonly Regex foreachParenthisEvaluationRegex = new Regex(@"^(?>\s*)(?<variableName>[\p{L}_](?>[\p{L}_0-9]*))(?>\s*)(?<in>in)(?>\s*)(?<collection>.*)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        protected static readonly Regex blockKeywordsWithoutParenthesesBeginningRegex = new Regex(@"^(?>\s*)(?<keyword>else|do|try|finally)(?![\p{L}_0-9])", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        protected static readonly Regex blockBeginningRegex = new Regex(@"^(?>\s*)[{]", RegexOptions.Compiled);
        protected static readonly Regex returnKeywordRegex = new Regex(@"^return((?>\s*)|\()", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);
        protected static readonly Regex nextIsEndOfExpressionRegex = new Regex(@"^(?>\s*)[;]", RegexOptions.Compiled);

        #endregion

        #region enums (if else blocks states)

        protected enum IfBlockEvaluatedState
        {
            NoBlockEvaluated,
            If,
            ElseIf
        }

        protected enum TryBlockEvaluatedState
        {
            NoBlockEvaluated,
            Try,
            Catch
        }

        #endregion

        #region Dictionaries declarations (Primary types, number suffix, escaped chars, operators management, default vars and functions)

        protected static readonly IDictionary<string, Type> primaryTypesDict = new Dictionary<string, Type>()
        {
            { "object", typeof(object) },
            { "string", typeof(string) },
            { "bool", typeof(bool) },
            { "bool?", typeof(bool?) },
            { "byte", typeof(byte) },
            { "byte?", typeof(byte?) },
            { "char", typeof(char) },
            { "char?", typeof(char?) },
            { "decimal", typeof(decimal) },
            { "decimal?", typeof(decimal?) },
            { "double", typeof(double) },
            { "double?", typeof(double?) },
            { "short", typeof(short) },
            { "short?", typeof(short?) },
            { "int", typeof(int) },
            { "int?", typeof(int?) },
            { "long", typeof(long) },
            { "long?", typeof(long?) },
            { "sbyte", typeof(sbyte) },
            { "sbyte?", typeof(sbyte?) },
            { "float", typeof(float) },
            { "float?", typeof(float?) },
            { "ushort", typeof(ushort) },
            { "ushort?", typeof(ushort?) },
            { "uint", typeof(uint) },
            { "uint?", typeof(uint?) },
            { "ulong", typeof(ulong) },
            { "ulong?", typeof(ulong?) },
            { "void", typeof(void) }
        };

        // Based on https://docs.microsoft.com/en-us/previous-versions/visualstudio/visual-studio-2012/y5b434w4(v=vs.110)?redirectedfrom=MSDN
        protected static readonly IDictionary<Type, Type[]> implicitCastDict = new Dictionary<Type, Type[]>
        {
            { typeof(sbyte), new Type[] { typeof(short), typeof(int), typeof(long), typeof(float), typeof(double), typeof(decimal) } },
            { typeof(byte), new Type[] { typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(long), typeof(ulong), typeof(float), typeof(double), typeof(decimal) } },
            { typeof(short), new Type[] { typeof(int), typeof(long),  typeof(float), typeof(double), typeof(decimal) } },
            { typeof(ushort), new Type[] { typeof(int), typeof(uint), typeof(long), typeof(ulong), typeof(float), typeof(double), typeof(decimal) } },
            { typeof(int), new Type[] { typeof(long), typeof(float), typeof(double), typeof(decimal) } },
            { typeof(uint), new Type[] {typeof(long), typeof(ulong), typeof(float), typeof(double), typeof(decimal) } },
            { typeof(long), new Type[] { typeof(float), typeof(double), typeof(decimal) } },
            { typeof(char), new Type[] { typeof(ushort), typeof(int), typeof(uint), typeof(long), typeof(ulong), typeof(float), typeof(double), typeof(decimal) } },
            { typeof(float), new Type[] { typeof(double) } },
            { typeof(ulong), new Type[] { typeof(float), typeof(double), typeof(decimal) } },
        };

        protected static readonly IDictionary<string, Func<string, CultureInfo, object>> numberSuffixToParse = new Dictionary<string, Func<string, CultureInfo, object>>(StringComparer.OrdinalIgnoreCase) // Always Case insensitive, like in C#
        {
            { "f", (number, culture) => float.Parse(number, NumberStyles.Any, culture) },
            { "d", (number, culture) => double.Parse(number, NumberStyles.Any, culture) },
            { "u", (number, culture) => uint.Parse(number, NumberStyles.Any, culture) },
            { "l", (number, culture) => long.Parse(number, NumberStyles.Any, culture) },
            { "ul", (number, culture) => ulong.Parse(number, NumberStyles.Any, culture) },
            { "m", (number, culture) => decimal.Parse(number, NumberStyles.Any, culture) }
        };

        protected static readonly IDictionary<char, string> stringEscapedCharDict = new Dictionary<char, string>()
        {
            { '\\', @"\" },
            { '"', "\"" },
            { '0', "\0" },
            { 'a', "\a" },
            { 'b', "\b" },
            { 'f', "\f" },
            { 'n', "\n" },
            { 'r', "\r" },
            { 't', "\t" },
            { 'v', "\v" }
        };

        protected static readonly IDictionary<char, char> charEscapedCharDict = new Dictionary<char, char>()
        {
            { '\\', '\\' },
            { '\'', '\'' },
            { '0', '\0' },
            { 'a', '\a' },
            { 'b', '\b' },
            { 'f', '\f' },
            { 'n', '\n' },
            { 'r', '\r' },
            { 't', '\t' },
            { 'v', '\v' }
        };

        /// <summary>
        /// OperatorsDictionaryInit() for values
        /// </summary>
        protected IDictionary<string, ExpressionOperator> operatorsDictionary = new Dictionary<string, ExpressionOperator>(StringComparer.Ordinal)
        {
            { "+", ExpressionOperator.Plus },
            { "-", ExpressionOperator.Minus },
            { "*", ExpressionOperator.Multiply },
            { "/", ExpressionOperator.Divide },
            { "%", ExpressionOperator.Modulo },
            { "<", ExpressionOperator.Lower },
            { ">", ExpressionOperator.Greater },
            { "<=", ExpressionOperator.LowerOrEqual },
            { ">=", ExpressionOperator.GreaterOrEqual },
            { "is", ExpressionOperator.Is },
            { "==", ExpressionOperator.Equal },
            { "!=", ExpressionOperator.NotEqual },
            { "&&", ExpressionOperator.ConditionalAnd },
            { "||", ExpressionOperator.ConditionalOr },
            { "!", ExpressionOperator.LogicalNegation },
            { "~", ExpressionOperator.BitwiseComplement },
            { "&", ExpressionOperator.LogicalAnd },
            { "|", ExpressionOperator.LogicalOr },
            { "^", ExpressionOperator.LogicalXor },
            { "<<", ExpressionOperator.ShiftBitsLeft },
            { ">>", ExpressionOperator.ShiftBitsRight },
            { "??", ExpressionOperator.NullCoalescing },
        };

        protected static readonly IList<ExpressionOperator> leftOperandOnlyOperatorsEvaluationDictionary = new List<ExpressionOperator>();

        protected static readonly IList<ExpressionOperator> rightOperandOnlyOperatorsEvaluationDictionary = new List<ExpressionOperator>()
        {
            ExpressionOperator.LogicalNegation,
            ExpressionOperator.BitwiseComplement,
            ExpressionOperator.UnaryPlus,
            ExpressionOperator.UnaryMinus
        };

        protected virtual IList<ExpressionOperator> LeftOperandOnlyOperatorsEvaluationDictionary => leftOperandOnlyOperatorsEvaluationDictionary;
        protected virtual IList<ExpressionOperator> RightOperandOnlyOperatorsEvaluationDictionary => rightOperandOnlyOperatorsEvaluationDictionary;
        protected virtual IList<IDictionary<ExpressionOperator, Func<dynamic, dynamic, object>>> OperatorsEvaluations => operatorsEvaluations;

        protected static readonly IList<IDictionary<ExpressionOperator, Func<dynamic, dynamic, object>>> operatorsEvaluations =
            new List<IDictionary<ExpressionOperator, Func<dynamic, dynamic, object>>>()
        {
            new Dictionary<ExpressionOperator, Func<dynamic, dynamic, object>>()
            {
                {ExpressionOperator.UnaryPlus, (dynamic _, dynamic right) => +right },
                {ExpressionOperator.UnaryMinus, (dynamic _, dynamic right) => -right },
                {ExpressionOperator.LogicalNegation, (dynamic _, dynamic right) => !right },
                {ExpressionOperator.BitwiseComplement, (dynamic _, dynamic right) => ~right },
                {ExpressionOperator.Cast, (dynamic left, dynamic right) => ChangeType(right, left) },
            },
            new Dictionary<ExpressionOperator, Func<dynamic, dynamic, object>>()
            {
                {ExpressionOperator.Multiply, (dynamic left, dynamic right) => left * right },
                {ExpressionOperator.Divide, (dynamic left, dynamic right) => left / right },
                {ExpressionOperator.Modulo, (dynamic left, dynamic right) => left % right },
            },
            new Dictionary<ExpressionOperator, Func<dynamic, dynamic, object>>()
            {
                {ExpressionOperator.Plus, (dynamic left, dynamic right) => left + right  },
                {ExpressionOperator.Minus, (dynamic left, dynamic right) => left - right },
            },
            new Dictionary<ExpressionOperator, Func<dynamic, dynamic, object>>()
            {
                {ExpressionOperator.ShiftBitsLeft, (dynamic left, dynamic right) => left << right },
                {ExpressionOperator.ShiftBitsRight, (dynamic left, dynamic right) => left >> right },
            },
            new Dictionary<ExpressionOperator, Func<dynamic, dynamic, object>>()
            {
                {ExpressionOperator.Lower, (dynamic left, dynamic right) => left < right },
                {ExpressionOperator.Greater, (dynamic left, dynamic right) => left > right },
                {ExpressionOperator.LowerOrEqual, (dynamic left, dynamic right) => left <= right },
                {ExpressionOperator.GreaterOrEqual, (dynamic left, dynamic right) => left >= right },
                {ExpressionOperator.Is, (dynamic left, dynamic right) => left != null && (((ClassOrEnumType)right).Type).IsAssignableFrom(left.GetType()) },
            },
            new Dictionary<ExpressionOperator, Func<dynamic, dynamic, object>>()
            {
                {ExpressionOperator.Equal, (dynamic left, dynamic right) => left == right },
                {ExpressionOperator.NotEqual, (dynamic left, dynamic right) => left != right },
            },
            new Dictionary<ExpressionOperator, Func<dynamic, dynamic, object>>()
            {
                {ExpressionOperator.LogicalAnd, (dynamic left, dynamic right) => left & right },
            },
            new Dictionary<ExpressionOperator, Func<dynamic, dynamic, object>>()
            {
                {ExpressionOperator.LogicalXor, (dynamic left, dynamic right) => left ^ right },
            },
            new Dictionary<ExpressionOperator, Func<dynamic, dynamic, object>>()
            {
                {ExpressionOperator.LogicalOr, (dynamic left, dynamic right) => left | right },
            },
            new Dictionary<ExpressionOperator, Func<dynamic, dynamic, object>>()
            {
                {ExpressionOperator.ConditionalAnd, (dynamic left, dynamic right) => {
                    if ( left is BubbleExceptionContainer leftExceptionContainer)
                    {
                        leftExceptionContainer.Throw();
                        return null; // this line is never reached
                    }
                    else if (!left)
                    {
                        return false;
                    }
                    else if (right is BubbleExceptionContainer rightExceptionContainer)
                    {
                        rightExceptionContainer.Throw();
                        return null; // this line is never reached
                    }
                    else
                    {
                        return left && right;
                    }
                } },
            },
            new Dictionary<ExpressionOperator, Func<dynamic, dynamic, object>>()
            {
                {ExpressionOperator.ConditionalOr, (dynamic left, dynamic right) => {
                    if ( left is BubbleExceptionContainer leftExceptionContainer)
                    {
                        leftExceptionContainer.Throw();
                        return null; // this line is never reached
                    }
                    else if (left)
                    {
                        return true;
                    }
                    else if (right is BubbleExceptionContainer rightExceptionContainer)
                    {
                        rightExceptionContainer.Throw();
                        return null; // this line is never reached
                    }
                    else
                    {
                        return left || right;
                    }
                } },
            },
            new Dictionary<ExpressionOperator, Func<dynamic, dynamic, object>>()
            {
                {ExpressionOperator.NullCoalescing, (dynamic left, dynamic right) => left ?? right },
            },
        };

        protected IDictionary<string, object> defaultVariables = new Dictionary<string, object>(StringComparer.Ordinal)
        {
            { "Pi", Math.PI },
            { "E", Math.E },
            { "null", null},
            { "true", true },
            { "false", false },
            { "this", null }
        };

        protected IDictionary<string, Func<double, double>> simpleDoubleMathFuncsDictionary = new Dictionary<string, Func<double, double>>(StringComparer.Ordinal)
        {
            { "Abs", Math.Abs },
            { "Acos", Math.Acos },
            { "Asin", Math.Asin },
            { "Atan", Math.Atan },
            { "Ceiling", Math.Ceiling },
            { "Cos", Math.Cos },
            { "Cosh", Math.Cosh },
            { "Exp", Math.Exp },
            { "Floor", Math.Floor },
            { "Log10", Math.Log10 },
            { "Sin", Math.Sin },
            { "Sinh", Math.Sinh },
            { "Sqrt", Math.Sqrt },
            { "Tan", Math.Tan },
            { "Tanh", Math.Tanh },
            { "Truncate", Math.Truncate },
        };

        protected IDictionary<string, Func<double, double, double>> doubleDoubleMathFuncsDictionary = new Dictionary<string, Func<double, double, double>>(StringComparer.Ordinal)
        {
            { "Atan2", Math.Atan2 },
            { "IEEERemainder", Math.IEEERemainder },
            { "Log", Math.Log },
            { "Pow", Math.Pow },
        };

        protected IDictionary<string, Func<ExpressionEvaluator, List<string>, object>> complexStandardFuncsDictionary = new Dictionary<string, Func<ExpressionEvaluator, List<string>, object>>(StringComparer.Ordinal)
        {
            { "Array", (self, args) => args.ConvertAll(self.Evaluate).ToArray() },
            { "ArrayOfType", (self, args) =>
                {
                    Array sourceArray = args.Skip(1).Select(self.Evaluate).ToArray();
                    Array typedArray = Array.CreateInstance((Type)self.Evaluate(args[0]), sourceArray.Length);
                    Array.Copy(sourceArray, typedArray, sourceArray.Length);

                    return typedArray;
                }
            },
            { "Avg", (self, args) => args.ConvertAll(arg => Convert.ToDouble(self.Evaluate(arg))).Sum() / (double)args.Count },
            { "default", (self, args) =>
                {
                    object argValue = self.Evaluate(args[0]);

                    if (argValue is ClassOrEnumType classOrTypeName)
                        return Activator.CreateInstance(classOrTypeName.Type);
                    else
                        return null;
                }
            },
            { "in", (self, args) => args.Skip(1).ToList().ConvertAll(self.Evaluate).Contains(self.Evaluate(args[0])) },
            { "List", (self, args) => args.ConvertAll(self.Evaluate) },
            { "ListOfType", (self, args) =>
                {
                    Type type = (Type)self.Evaluate(args[0]);
                    Array sourceArray = args.Skip(1).Select(self.Evaluate).ToArray();
                    Array typedArray = Array.CreateInstance(type, sourceArray.Length);
                    Array.Copy(sourceArray, typedArray, sourceArray.Length);

                    Type typeOfList = typeof(List<>).MakeGenericType(type);

                    object list = Activator.CreateInstance(typeOfList);

                    typeOfList.GetMethod("AddRange").Invoke(list, new object[]{ typedArray });

                    return list;
                }
            },
            { "Max", (self, args) => args.ConvertAll(arg => Convert.ToDouble(self.Evaluate(arg))).Max() },
            { "Min", (self, args) => args.ConvertAll(arg => Convert.ToDouble(self.Evaluate(arg))).Min() },
            { "new", (self, args) =>
                {
                    List<object> cArgs = args.ConvertAll(self.Evaluate);
                    return cArgs[0] is ClassOrEnumType classOrEnumType ? Activator.CreateInstance(classOrEnumType.Type, cArgs.Skip(1).ToArray()) : null;
                }
            },
            { "Round", (self, args) =>
                {
                    if(args.Count == 3)
                    {
                        return Math.Round(Convert.ToDouble(self.Evaluate(args[0])), Convert.ToInt32(self.Evaluate(args[1])), (MidpointRounding)self.Evaluate(args[2]));
                    }
                    else if(args.Count == 2)
                    {
                        object arg2 = self.Evaluate(args[1]);

                        if(arg2 is MidpointRounding midpointRounding)
                            return Math.Round(Convert.ToDouble(self.Evaluate(args[0])), midpointRounding);
                        else
                            return Math.Round(Convert.ToDouble(self.Evaluate(args[0])), Convert.ToInt32(arg2));
                    }
                    else if(args.Count == 1) { return Math.Round(Convert.ToDouble(self.Evaluate(args[0]))); }
                    else
                    {
                        throw new ArgumentException();
                    }
                }
            },
            { "Sign", (self, args) => Math.Sign(Convert.ToDouble(self.Evaluate(args[0]))) },
            { "sizeof", (self, args) =>
                {
                    Type type = ((ClassOrEnumType)self.Evaluate(args[0])).Type;

                    if(type == typeof(bool))
                        return 1;
                    else if(type == typeof(char))
                        return 2;
                    else
                        return Marshal.SizeOf(type);
                }
            },
            { "typeof", (self, args) => ((ClassOrEnumType)self.Evaluate(args[0])).Type },
        };

        #endregion

        #region Caching

        /// <summary>
        /// if set to <c>true</c> use a cache for types that were resolved to resolve faster next time.<para/>
        /// if set to <c>false</c> the cache of types resolution is not use for this instance of ExpressionEvaluator.<para/>
        /// Default : false<para/>
        /// The cache is the static Dictionary TypesResolutionCaching (so it is shared by all instances of ExpressionEvaluator that have CacheTypesResolutions enabled)
        /// </summary>
        public bool CacheTypesResolutions { get; set; }

        /// <summary>
        /// A shared cache for types resolution.
        /// </summary>
        public static IDictionary<string, Type> TypesResolutionCaching { get; set; } = new Dictionary<string, Type>();

        /// <summary>
        /// Clear all ExpressionEvaluator caches
        /// </summary>
        public static void ClearAllCaches()
        {
            TypesResolutionCaching.Clear();
        }

        #endregion

        #region Assemblies, Namespaces and types lists

        private static IList<Assembly> staticAssemblies;
        private IList<Assembly> assemblies;

        /// <summary>
        /// All assemblies needed to resolves Types<para/>
        /// By default all assemblies loaded in the current AppDomain
        /// </summary>
        public virtual IList<Assembly> Assemblies
        {
            get { return assemblies ?? (assemblies = staticAssemblies) ?? (assemblies = staticAssemblies = AppDomain.CurrentDomain.GetAssemblies().ToList()); }
            set { assemblies = value; }
        }

        /// <summary>
        /// All Namespaces Where to find types<para/>
        /// Equivalent of a <c>using Namespace;</c>
        /// </summary>
        public virtual IList<string> Namespaces { get; set; } = new List<string>()
        {
            "System",
            "System.Linq",
            "System.IO",
            "System.Text",
            "System.Text.RegularExpressions",
            "System.ComponentModel",
            "System.Dynamic",
            "System.Collections",
            "System.Collections.Generic",
            "System.Collections.Specialized",
            "System.Globalization"
        };

        /// <summary>
        /// To add or remove specific types to manage in expression.
        /// </summary>
        public virtual IList<Type> Types { get; set; } = new List<Type>();

        /// <summary>
        /// A list of type to block an keep un usable in Expression Evaluation for security purpose
        /// </summary>
        public virtual IList<Type> TypesToBlock { get; set; } = new List<Type>();

        /// <summary>
        /// A list of statics types where to find extensions methods
        /// </summary>
        public virtual IList<Type> StaticTypesForExtensionsMethods { get; set; } = new List<Type>()
        {
            typeof(Enumerable) // For Linq extension methods
        };

        #endregion

        #region Options

        private bool optionCaseSensitiveEvaluationActive = true;

        /// <summary>
        /// If <c>true</c> all evaluation are case sensitives.<para/>
        /// If <c>false</c> evaluations are case insensitive.<para/>
        /// By default = true
        /// </summary>
        public bool OptionCaseSensitiveEvaluationActive
        {
            get { return optionCaseSensitiveEvaluationActive; }
            set
            {
                optionCaseSensitiveEvaluationActive = value;
                StringComparisonForCasing = optionCaseSensitiveEvaluationActive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
                Variables = Variables;
                operatorsDictionary = new Dictionary<string, ExpressionOperator>(operatorsDictionary, StringComparerForCasing);
                defaultVariables = new Dictionary<string, object>(defaultVariables, StringComparerForCasing);
                simpleDoubleMathFuncsDictionary = new Dictionary<string, Func<double, double>>(simpleDoubleMathFuncsDictionary, StringComparerForCasing);
                doubleDoubleMathFuncsDictionary = new Dictionary<string, Func<double, double, double>>(doubleDoubleMathFuncsDictionary, StringComparerForCasing);
                complexStandardFuncsDictionary = new Dictionary<string, Func<ExpressionEvaluator, List<string>, object>>(complexStandardFuncsDictionary, StringComparerForCasing);
            }
        }

        /// <summary>
        /// If <c>true</c> Variables dictionary is kept as given so variables are persist outside of the evaluator and the comparer for keys can be defined by the user<para/>
        /// If <c>false</c> Variables dictionary references are copied internally to follow OptionCaseSensitiveEvaluationActive with an internal protected comparer for keys<para/>
        /// By default = false
        /// </summary>
        public bool OptionVariablesPersistenceCustomComparer { get; set; }

        protected StringComparison StringComparisonForCasing { get; set; } = StringComparison.Ordinal;

        protected StringComparer StringComparerForCasing => OptionCaseSensitiveEvaluationActive ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase;

        /// <summary>
        /// If <c>true</c> all numbers without decimal and suffixes evaluations will be done as double<para/>
        /// If <c>false</c> Integers values without decimal and suffixes will be evaluate as int as in C# (Warning some operation can round values)<para/>
        /// By default = false
        /// </summary>
        public bool OptionForceIntegerNumbersEvaluationsAsDoubleByDefault { get; set; }

        private CultureInfo cultureInfoForNumberParsing = CultureInfo.InvariantCulture.Clone() as CultureInfo;

        /// <summary>
        /// The culture used to evaluate numbers.<para/>
        /// Synchronized with OptionNumberParsingDecimalSeparator and OptionNumberParsingThousandSeparator.<para/>
        /// So always set a full CultureInfo object and do not change CultureInfoForNumberParsing.NumberFormat.NumberDecimalSeparator and CultureInfoForNumberParsing.NumberFormat.NumberGroupSeparator properties directly.<para/>
        /// Warning if using comma in separators change also OptionFunctionArgumentsSeparator and OptionInitializersSeparator otherwise it will create conflicts
        /// </summary>
        public CultureInfo CultureInfoForNumberParsing
        {
            get
            {
                return cultureInfoForNumberParsing;
            }

            set
            {
                cultureInfoForNumberParsing = value;

                OptionNumberParsingDecimalSeparator = cultureInfoForNumberParsing.NumberFormat.NumberDecimalSeparator;
                OptionNumberParsingThousandSeparator = cultureInfoForNumberParsing.NumberFormat.NumberGroupSeparator;
            }
        }

        private string optionNumberParsingDecimalSeparator = ".";

        /// <summary>
        /// Allow to change the decimal separator of numbers when parsing expressions.<para/>
        /// By default "."<para/>
        /// Warning if using comma change also OptionFunctionArgumentsSeparator and OptionInitializersSeparator otherwise it will create conflicts.<para/>
        /// Modify CultureInfoForNumberParsing.
        /// </summary>
        public string OptionNumberParsingDecimalSeparator
        {
            get => optionNumberParsingDecimalSeparator;

            set
            {
                optionNumberParsingDecimalSeparator = value ?? ".";
                CultureInfoForNumberParsing.NumberFormat.NumberDecimalSeparator = optionNumberParsingDecimalSeparator;

                numberRegexPattern = string.Format(numberRegexOrigPattern,
                    optionNumberParsingDecimalSeparator != null ? Regex.Escape(optionNumberParsingDecimalSeparator) : ".",
                    optionNumberParsingThousandSeparator != null ? Regex.Escape(optionNumberParsingThousandSeparator) : "");
            }
        }

        private string optionNumberParsingThousandSeparator = string.Empty;

        /// <summary>
        /// Allow to change the thousand separator of numbers when parsing expressions.<para/>
        /// By default string.Empty<para/>
        /// Warning if using comma change also OptionFunctionArgumentsSeparator and OptionInitializersSeparator otherwise it will create conflicts.<para/>
        /// Modify CultureInfoForNumberParsing.
        /// </summary>
        public string OptionNumberParsingThousandSeparator
        {
            get => optionNumberParsingThousandSeparator;

            set
            {
                optionNumberParsingThousandSeparator = value ?? string.Empty;
                CultureInfoForNumberParsing.NumberFormat.NumberGroupSeparator = value;

                numberRegexPattern = string.Format(numberRegexOrigPattern,
                    optionNumberParsingDecimalSeparator != null ? Regex.Escape(optionNumberParsingDecimalSeparator) : ".",
                    optionNumberParsingThousandSeparator != null ? Regex.Escape(optionNumberParsingThousandSeparator) : "");
            }
        }

        /// <summary>
        /// Allow to change the separator of functions arguments.<para/>
        /// By default ","<para/>
        /// Warning must to be changed if OptionNumberParsingDecimalSeparator = "," otherwise it will create conflicts<para/>
        /// </summary>
        public string OptionFunctionArgumentsSeparator { get; set; } = ",";

        /// <summary>
        /// Allow to change the separator of Object and collections Initialization between { and } after the keyword new.<para/>
        /// By default ","<para/>
        /// Warning must to be changed if OptionNumberParsingDecimalSeparator = "," otherwise it will create conflicts<para/>
        /// </summary>
        public string OptionInitializersSeparator { get; set; } = ",";

        /// <summary>
        /// if <c>true</c> allow to add the prefix Fluid or Fluent before void methods names to return back the instance on which the method is call.<para/>
        /// if <c>false</c> unactive this functionality.<para/>
        /// By default : true
        /// </summary>
        public bool OptionFluidPrefixingActive { get; set; } = true;

        /// <summary>
        /// if <c>AllowAll</c> Allow the use of any inline namespace that is available in memory (Can be slow, and is less secure).<para/>
        /// if <c>AllowOnlyInlineNamespacesList</c> Allow only the use of inline namespace defined in <see cref="InlineNamespacesList"/><para/>
        /// if <c>BlockOnlyInlineNamespacesList</c> Allow the use of any inline namespace that is available in memory that is not defined in <see cref="InlineNamespacesList"/><para/>
        /// if <c>BlockAll</c> Unactive the use of inline namespaces<para/>
        /// By default : <c>AllowAll</c>
        /// </summary>
        public InlineNamespacesEvaluationRule OptionInlineNamespacesEvaluationRule { get; set; } = InlineNamespacesEvaluationRule.AllowAll;

        /// <summary>
        /// This list is used to allow or block depending on <see cref="OptionInlineNamespacesEvaluationRule"/> a list of namespaces for inline writing.<para/>
        /// The direct access of type depending on <see cref="Namespaces"/> is not affected by this list.
        /// </summary>
        public virtual IList<string> InlineNamespacesList { get; set; } = new List<string>();

        private Func<ExpressionEvaluator, List<string>, object> newMethodMem;

        /// <summary>
        /// if <c>true</c> allow to create instance of object with the Default function new(ClassNam,...).<para/>
        /// if <c>false</c> unactive this functionality.<para/>
        /// By default : true
        /// </summary>
        public bool OptionNewFunctionEvaluationActive
        {
            get
            {
                return complexStandardFuncsDictionary.ContainsKey("new");
            }
            set
            {
                if (value && !complexStandardFuncsDictionary.ContainsKey("new"))
                {
                    complexStandardFuncsDictionary["new"] = newMethodMem;
                }
                else if (!value && complexStandardFuncsDictionary.ContainsKey("new"))
                {
                    newMethodMem = complexStandardFuncsDictionary["new"];
                    complexStandardFuncsDictionary.Remove("new");
                }
            }
        }

        /// <summary>
        /// if <c>true</c> allow to create instance of object with the C# syntax new ClassName(...).<para/>
        /// if <c>false</c> unactive this functionality.<para/>
        /// By default : true
        /// </summary>
        public bool OptionNewKeywordEvaluationActive { get; set; } = true;

        /// <summary>
        /// if <c>true</c> allow to call static methods on classes.<para/>
        /// if <c>false</c> unactive this functionality.<para/>
        /// By default : true
        /// </summary>
        public bool OptionStaticMethodsCallActive { get; set; } = true;

        /// <summary>
        /// if <c>true</c> allow to get static properties on classes<para/>
        /// if <c>false</c> unactive this functionality.<para/>
        /// By default : true
        /// </summary>
        public bool OptionStaticPropertiesGetActive { get; set; } = true;

        /// <summary>
        /// if <c>true</c> allow to call instance methods on objects.<para/>
        /// if <c>false</c> unactive this functionality.<para/>
        /// By default : true
        /// </summary>
        public bool OptionInstanceMethodsCallActive { get; set; } = true;

        /// <summary>
        /// if <c>true</c> allow to get instance properties on objects<para/>
        /// if <c>false</c> unactive this functionality.<para/>
        /// By default : true
        /// </summary>
        public bool OptionInstancePropertiesGetActive { get; set; } = true;

        /// <summary>
        /// if <c>true</c> allow to get object at index or key like <c>IndexedObject[indexOrKey]</c><para/>
        /// if <c>false</c> unactive this functionality.<para/>
        /// By default : true
        /// </summary>
        public bool OptionIndexingActive { get; set; } = true;

        /// <summary>
        /// if <c>true</c> allow string interpretation with ""<para/>
        /// if <c>false</c> unactive this functionality.<para/>
        /// By default : true
        /// </summary>
        public bool OptionStringEvaluationActive { get; set; } = true;

        /// <summary>
        /// if <c>true</c> allow char interpretation with ''<para/>
        /// if <c>false</c> unactive this functionality.<para/>
        /// By default : true
        /// </summary>
        public bool OptionCharEvaluationActive { get; set; } = true;

        /// <summary>
        /// If <c>true</c> Evaluate function is callables in an expression.<para/>
        /// If <c>false</c> Evaluate is not callable.<para/>
        /// By default : true<para/>
        /// if set to false for security (also ensure that ExpressionEvaluator type is in TypesToBlock list)
        /// </summary>
        public bool OptionEvaluateFunctionActive { get; set; } = true;

        /// <summary>
        /// If <c>true</c> allow to assign a value to a variable in the Variable disctionary with (=, +=, -=, *=, /=, %=, &amp;=, |=, ^=, &lt;&lt;=, &gt;&gt;=, ++ or --)<para/>
        /// If <c>false</c> unactive this functionality<para/>
        /// By default : true
        /// </summary>
        public bool OptionVariableAssignationActive { get; set; } = true;

        /// <summary>
        /// If <c>true</c> allow to set/modify a property or a field value with (=, +=, -=, *=, /=, %=, &amp;=, |=, ^=, &lt;&lt;=, &gt;&gt;=, ++ or --)<para/>
        /// If <c>false</c> unactive this functionality<para/>
        /// By default : true
        /// </summary>
        public bool OptionPropertyOrFieldSetActive { get; set; } = true;

        /// <summary>
        /// If <c>true</c> allow to assign a indexed element like Collections, List, Arrays and Dictionaries with (=, +=, -=, *=, /=, %=, &amp;=, |=, ^=, &lt;&lt;=, &gt;&gt;=, ++ or --)<para/>
        /// If <c>false</c> unactive this functionality<para/>
        /// By default : true
        /// </summary>
        public bool OptionIndexingAssignationActive { get; set; } = true;

        /// <summary>
        /// If <c>true</c> ScriptEvaluate function is callables in an expression.<para/>
        /// If <c>false</c> Evaluate is not callable.<para/>
        /// By default : true<para/>
        /// if set to false for security (also ensure that ExpressionEvaluator type is in TypesToBlock list)
        /// </summary>
        public bool OptionScriptEvaluateFunctionActive { get; set; } = true;

        /// <summary>
        /// Set How to react when the keyword return is not found in a script. when using ScriptEvaluate method<para/>
        /// If <c>ReturnAutomaticallyLastEvaluatedExpression</c> ScriptEvaluate return automatically the last evaluated expression if no return keyword is met.<para/>
        /// If <c>ReturnNull</c> return null if no return keyword is met.<para/>
        /// If <c>ThrowSyntaxException</c> a exception is throw if no return keyword is met.<para/>
        /// By default : ReturnAutomaticallyLastEvaluatedExpression;
        /// </summary>
        public OptionOnNoReturnKeywordFoundInScriptAction OptionOnNoReturnKeywordFoundInScriptAction { get; set; }

        /// <summary>
        /// If <c>true</c> ScriptEvaluate need to have a semicolon [;] after each expression.<para/>
        /// If <c>false</c> Allow to omit the semicolon for the last expression of the script.<para/>
        /// Default : true
        /// </summary>
        public bool OptionScriptNeedSemicolonAtTheEndOfLastExpression { get; set; } = true;

        /// <summary>
        /// If <c>true</c> Allow to access fields, properties and methods that are not declared public. (private, protected and internal)<para/>
        /// If <c>false</c> Allow to access only to public members.<para/>
        /// Default : false<para/>
        /// Warning : This clearly break the encapsulation principle use this only if you know what you do.
        /// </summary>
        public bool OptionAllowNonPublicMembersAccess { get; set; }

        /// <summary>
        /// If <c>true</c> On unsuccessful call to an extension method, all defined overloads of that method are detected to resolve whether method is defined and called with wrong arguments or method is not defined.<para/>
        /// If <c>false</c> Unsucessful call to an extension method will always result in "Method {name} is not defined on type {type}"<para/>
        /// Default : true
        /// </summary>
        public bool OptionDetectExtensionMethodsOverloadsOnExtensionMethodNotFound { get; set; } = true;

        /// <summary>
        /// If <c>true</c> Allow to define multi expression lambda in Expressions (not in script)<para/>
        /// If <c>false</c> Can only define simple expression lambda if not in script<para/>
        /// Default value : <c>true</c>
        /// </summary>
        public bool OptionCanDeclareMultiExpressionsLambdaInSimpleExpressionEvaluate { get; set; } = true;

        #endregion

        #region Reflection flags

        protected virtual BindingFlags InstanceBindingFlag
        {
            get
            {
                BindingFlags flag = BindingFlags.Default | BindingFlags.Public | BindingFlags.Instance;

                if (!OptionCaseSensitiveEvaluationActive)
                    flag |= BindingFlags.IgnoreCase;
                if (OptionAllowNonPublicMembersAccess)
                    flag |= BindingFlags.NonPublic;

                return flag;
            }
        }

        protected virtual BindingFlags StaticBindingFlag
        {
            get
            {
                BindingFlags flag = BindingFlags.Default | BindingFlags.Public | BindingFlags.Static;

                if (!OptionCaseSensitiveEvaluationActive)
                    flag |= BindingFlags.IgnoreCase;
                if (OptionAllowNonPublicMembersAccess)
                    flag |= BindingFlags.NonPublic;

                return flag;
            }
        }

        #endregion

        #region Custom and on the fly evaluation

        private object context;

        /// <summary>
        /// If set, this object is used to use it's fields, properties and methods as global variables and functions
        /// </summary>
        public object Context
        {
            get { return context; }
            set
            {
                context = value;
                defaultVariables["this"] = context;
            }
        }

        private IDictionary<string, object> variables = new Dictionary<string, object>(StringComparer.Ordinal);

        /// <summary>
        /// Counts stack initialisations to determine if the expression entry point was reached. In that case the transported exception should be thrown.
        /// </summary>
        private int evaluationStackCount;

        /// <summary>
        /// A Dictionary of variable name/value that can be use in expressions and scripts evaluate by the current instance of <see cref="ExpressionEvaluator"/><para/>
        /// Warning : Make a copy of the given dictionary to manage casing.
        /// </summary>
        public IDictionary<string, object> Variables
        {
            get { return variables; }
            set
            {
                if(OptionVariablesPersistenceCustomComparer)
                {
                    variables = value;
                }
                else
                {
                    variables = value == null ? new Dictionary<string, object>(StringComparerForCasing) : new Dictionary<string, object>(value, StringComparerForCasing);
                }
            }
        }

        /// <summary>
        /// Is fired just before an expression is evaluate.<para/>
        /// Allow to redefine the expression to evaluate or to force a result value.
        /// </summary>
        public event EventHandler<ExpressionEvaluationEventArg> ExpressionEvaluating;

        /// <summary>
        /// Is fired just before to return the expression evaluation.<para/>
        /// Allow to modify on the fly the result of the evaluation.
        /// </summary>
        public event EventHandler<ExpressionEvaluationEventArg> ExpressionEvaluated;

        /// <summary>
        /// Is fired before a variable, field or property resolution.<para/>
        /// Allow to define a variable and the corresponding value on the fly.<para/>
        /// Allow also to cancel the evaluation of this variable (consider it does'nt exists)
        /// </summary>
        public event EventHandler<VariablePreEvaluationEventArg> PreEvaluateVariable;

        /// <summary>
        /// Is fired before a function or method resolution.<para/>
        /// Allow to define a function or method and the corresponding value on the fly.<para/>
        /// Allow also to cancel the evaluation of this function (consider it does'nt exists)
        /// </summary>
        public event EventHandler<FunctionPreEvaluationEventArg> PreEvaluateFunction;

        /// <summary>
        /// Is fired before a indexing resolution.<para/>
        /// Allow to define an indexing and the corresponding value on the fly.<para/>
        /// Allow also to cancel the evaluation of this indexing (consider it does'nt exists)
        /// </summary>
        public event EventHandler<IndexingPreEvaluationEventArg> PreEvaluateIndexing;

        /// <summary>
        /// Is fired if no variable, field or property were found.<para/>
        /// Allow to define a variable and the corresponding value on the fly.
        /// </summary>
        public event EventHandler<VariableEvaluationEventArg> EvaluateVariable;

        /// <summary>
        /// Is fired if no function or method were found.<para/>
        /// Allow to define a function or method and the corresponding value on the fly.
        /// </summary>
        public event EventHandler<FunctionEvaluationEventArg> EvaluateFunction;

        /// <summary>
        /// Is fired when a parameter is not of the correct type for the function.<para/>
        /// Allow to define a custom parameter cast to make the function call work on the fly.
        /// </summary>
        public event EventHandler<ParameterCastEvaluationEventArg> EvaluateParameterCast;

        #endregion

        #region Constructors and overridable Inits methods

        /// <summary>
        /// Default Constructor
        /// </summary>
        public ExpressionEvaluator()
        {
            DefaultDecimalSeparatorInit();

            Init();
        }

        /// <summary>
        /// Constructor with variables initialize
        /// </summary>
        /// <param name="variables">The Values of variables use in the expressions</param>
        public ExpressionEvaluator(IDictionary<string, object> variables) : this()
        {
            Variables = variables;
        }

        /// <summary>
        /// Constructor with variables initialize
        /// </summary>
        /// <param name="variables">The Values of variables use in the expressions</param>
        /// <param name="optionVariablesPersistenceCustomComparer">To set <see cref="OptionVariablesPersistenceCustomComparer"/> before setting <see cref="Variables"/></param>
        public ExpressionEvaluator(IDictionary<string, object> variables, bool optionVariablesPersistenceCustomComparer) : this()
        {
            OptionVariablesPersistenceCustomComparer = optionVariablesPersistenceCustomComparer;
            Variables = variables;
        }

        /// <summary>
        /// Constructor with context initialize
        /// </summary>
        /// <param name="context">the context that propose it's fields, properties and methods to the evaluation</param>
        public ExpressionEvaluator(object context) : this()
        {
            Context = context;
        }

        /// <summary>
        /// Constructor with variables and context initialize
        /// </summary>
        /// <param name="context">the context that propose it's fields, properties and methods to the evaluation</param>
        /// <param name="variables">The Values of variables use in the expressions</param>
        public ExpressionEvaluator(object context, IDictionary<string, object> variables) : this()
        {
            Context = context;
            Variables = variables;
        }

        /// <summary>
        /// Constructor with variables and context initialize
        /// </summary>
        /// <param name="context">the context that propose it's fields, properties and methods to the evaluation</param>
        /// <param name="variables">The Values of variables use in the expressions</param>
        /// <param name="optionVariablesPersistenceCustomComparer">To set <see cref="OptionVariablesPersistenceCustomComparer"/> before setting <see cref="Variables"/></param>
        public ExpressionEvaluator(object context, IDictionary<string, object> variables, bool optionVariablesPersistenceCustomComparer) : this()
        {
            OptionVariablesPersistenceCustomComparer = optionVariablesPersistenceCustomComparer;
            Context = context;
            Variables = variables;
        }

        protected virtual void DefaultDecimalSeparatorInit()
        {
            numberRegexPattern = string.Format(numberRegexOrigPattern, @"\.", string.Empty);

            CultureInfoForNumberParsing.NumberFormat.NumberDecimalSeparator = ".";
        }

        protected virtual void Init()
        { }

        #endregion

        #region Main evaluate methods (Expressions and scripts ==> public)

        #region Scripts

        protected bool inScript;

        /// <summary>
        /// Evaluate a script (multiple expressions separated by semicolon)<para/>
        /// support some conditional, loop and other C# code flow management keywords
        /// </summary>
        /// <typeparam name="T">The type in which to cast the result of the expression</typeparam>
        /// <param name="script">the script to evaluate</param>
        /// <returns>The result of the last evaluated expression</returns>
        public virtual T ScriptEvaluate<T>(string script)
        {
            return (T)ScriptEvaluate(script);
        }

        /// <summary>
        /// Evaluate a script (multiple expressions separated by semicolon)<para/>
        /// support some conditional, loop and other C# code flow management keywords
        /// </summary>
        /// <param name="script">the script to evaluate</param>
        /// <returns>The result of the last evaluated expression</returns>
        public virtual object ScriptEvaluate(string script)
        {
            inScript = true;
            try
            {
                bool isReturn = false;
                bool isBreak = false;
                bool isContinue = false;

                object result = ScriptEvaluate(script, ref isReturn, ref isBreak, ref isContinue);

                if (isBreak)
                    throw new ExpressionEvaluatorSyntaxErrorException("[break] keyword executed outside a loop");
                else if (isContinue)
                    throw new ExpressionEvaluatorSyntaxErrorException("[continue] keyword executed outside a loop");
                else
                    return result;
            }
            finally
            {
                inScript = false;
            }
        }

        protected virtual object ScriptEvaluate(string script, ref bool valueReturned, ref bool breakCalled, ref bool continueCalled)
        {
            object lastResult = null;
            bool isReturn = valueReturned;
            bool isBreak = breakCalled;
            bool isContinue = continueCalled;
            int startOfExpression = 0;
            IfBlockEvaluatedState ifBlockEvaluatedState = IfBlockEvaluatedState.NoBlockEvaluated;
            TryBlockEvaluatedState tryBlockEvaluatedState = TryBlockEvaluatedState.NoBlockEvaluated;
            List<List<string>> ifElseStatementsList = new List<List<string>>();
            List<List<string>> tryStatementsList = new List<List<string>>();

            script = script.TrimEnd();

            object ManageJumpStatementsOrExpressionEval(string expression)
            {
                expression = expression.Trim();

                if (expression.Equals("break", StringComparisonForCasing))
                {
                    isBreak = true;
                    return lastResult;
                }

                if (expression.Equals("continue", StringComparisonForCasing))
                {
                    isContinue = true;
                    return lastResult;
                }

                if (expression.StartsWith("throw ", StringComparisonForCasing))
                {
                    if (Evaluate(expression.Remove(0, 6)) is Exception exception)
                    {
                        ExceptionDispatchInfo.Capture(exception).Throw();
                    }
                    else
                    {
                        throw new ExpressionEvaluatorSyntaxErrorException("throw keyword must be follow by an Exception instance");
                    }
                }

                expression = returnKeywordRegex.Replace(expression, match =>
                {
                    if (OptionCaseSensitiveEvaluationActive && !match.Value.StartsWith("return"))
                        return match.Value;

                    isReturn = true;
                    return match.Value.Contains("(") ? "(" : string.Empty;
                });

                return Evaluate(expression);
            }

            object ScriptExpressionEvaluate(ref int index)
            {
                string expression = script.Substring(startOfExpression, index - startOfExpression);

                startOfExpression = index + 1;

                return ManageJumpStatementsOrExpressionEval(expression);
            }

            bool TryParseStringAndParenthisAndCurlyBrackets(ref int index)
            {
                bool parsed = true;
                Match internalStringMatch = stringBeginningRegex.Match(script.Substring(index));

                if (internalStringMatch.Success)
                {
                    string innerString = internalStringMatch.Value + GetCodeUntilEndOfString(script.Substring(index + internalStringMatch.Length), internalStringMatch);
                    index += innerString.Length - 1;
                }
                else if (script[index] == '(')
                {
                    index++;
                    GetExpressionsBetweenParenthesesOrOtherImbricableBrackets(script, ref index, false);
                }
                else if (script[index] == '{')
                {
                    index++;
                    GetScriptBetweenCurlyBrackets(script, ref index);
                }
                else
                {
                    Match charMatch = internalCharRegex.Match(script.Substring(index));

                    if (charMatch.Success)
                        index += charMatch.Length - 1;

                    parsed = false;
                }

                return parsed;
            }

            void ExecuteIfList()
            {
                if (ifElseStatementsList.Count > 0)
                {
                    string ifScript = ifElseStatementsList.Find(statement => (bool)ManageJumpStatementsOrExpressionEval(statement[0]))?[1];

                    if (!string.IsNullOrEmpty(ifScript))
                        lastResult = ScriptEvaluate(ifScript, ref isReturn, ref isBreak, ref isContinue);

                    ifElseStatementsList.Clear();
                }
            }

            void ExecuteTryList()
            {
                if (tryStatementsList.Count > 0)
                {
                    if (tryStatementsList.Count == 1)
                    {
                        throw new ExpressionEvaluatorSyntaxErrorException("a try statement need at least one catch or one finally statement.");
                    }

                    try
                    {
                        lastResult = ScriptEvaluate(tryStatementsList[0][0], ref isReturn, ref isBreak, ref isContinue);
                    }
                    catch (Exception exception)
                    {
                        bool atLeasOneCatch = false;

                        foreach (List<string> catchStatement in tryStatementsList.Skip(1).TakeWhile(e => e[0].Equals("catch")))
                        {
                            if (catchStatement[1] != null)
                            {
                                string[] exceptionVariable = catchStatement[1].Trim().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                                string exceptionName = exceptionVariable[0];

                                if (exceptionVariable.Length >= 2)
                                {
                                    if (!((ClassOrEnumType)Evaluate(exceptionVariable[0])).Type.IsAssignableFrom(exception.GetType()))
                                        continue;

                                    exceptionName = exceptionVariable[1];
                                }

                                Variables[exceptionName] = exception;
                            }

                            lastResult = ScriptEvaluate(catchStatement[2], ref isReturn, ref isBreak, ref isContinue);
                            atLeasOneCatch = true;
                            break;
                        }

                        if (!atLeasOneCatch)
                        {
                            throw;
                        }
                    }
                    finally
                    {
                        if (tryStatementsList.Last()[0].Equals("finally"))
                        {
                            lastResult = ScriptEvaluate(tryStatementsList.Last()[1], ref isReturn, ref isBreak, ref isContinue);
                        }
                    }

                    tryStatementsList.Clear();
                }
            }

            void ExecuteBlocksStacks()
            {
                ExecuteTryList();
                ExecuteIfList();
            }

            int i = 0;

            while (!isReturn && !isBreak && !isContinue && i < script.Length)
            {
                Match blockKeywordsBeginingMatch = null;
                Match blockKeywordsWithoutParenthesesBeginningMatch = null;

                if (script.Substring(startOfExpression, i - startOfExpression).Trim().Equals(string.Empty)
                    && ((blockKeywordsBeginingMatch = blockKeywordsBeginningRegex.Match(script.Substring(i))).Success
                        || (blockKeywordsWithoutParenthesesBeginningMatch = blockKeywordsWithoutParenthesesBeginningRegex.Match(script.Substring(i))).Success))
                {
                    i += blockKeywordsBeginingMatch.Success ? blockKeywordsBeginingMatch.Length : blockKeywordsWithoutParenthesesBeginningMatch.Length;
                    string keyword = blockKeywordsBeginingMatch.Success ? blockKeywordsBeginingMatch.Groups["keyword"].Value.Replace(" ", "").Replace("\t", "") : (blockKeywordsWithoutParenthesesBeginningMatch?.Groups["keyword"].Value ?? string.Empty);
                    List<string> keywordAttributes = blockKeywordsBeginingMatch.Success ? GetExpressionsBetweenParenthesesOrOtherImbricableBrackets(script, ref i, true, ";") : null;

                    if (blockKeywordsBeginingMatch.Success)
                        i++;

                    Match blockBeginningMatch = blockBeginningRegex.Match(script.Substring(i));

                    string subScript = string.Empty;

                    if (blockBeginningMatch.Success)
                    {
                        i += blockBeginningMatch.Length;

                        subScript = GetScriptBetweenCurlyBrackets(script, ref i);

                        i++;
                    }
                    else
                    {
                        bool continueExpressionParsing = true;
                        startOfExpression = i;

                        while (i < script.Length && continueExpressionParsing)
                        {
                            if (TryParseStringAndParenthisAndCurlyBrackets(ref i)) { }
                            else if (script.Length - i > 2 && script.Substring(i, 3).Equals("';'"))
                            {
                                i += 2;
                            }
                            else if (script[i] == ';')
                            {
                                subScript = script.Substring(startOfExpression, i + 1 - startOfExpression);
                                continueExpressionParsing = false;
                            }

                            i++;
                        }

                        if (subScript.Trim().Equals(string.Empty))
                            throw new ExpressionEvaluatorSyntaxErrorException($"No instruction after [{keyword}] statement.");
                    }

                    if (keyword.Equals("elseif", StringComparisonForCasing))
                    {
                        if (ifBlockEvaluatedState == IfBlockEvaluatedState.NoBlockEvaluated)
                        {
                            throw new ExpressionEvaluatorSyntaxErrorException("No corresponding [if] for [else if] statement.");
                        }
                        else
                        {
                            ifElseStatementsList.Add(new List<string>() { keywordAttributes[0], subScript });
                            ifBlockEvaluatedState = IfBlockEvaluatedState.ElseIf;
                        }
                    }
                    else if (keyword.Equals("else", StringComparisonForCasing))
                    {
                        if (ifBlockEvaluatedState == IfBlockEvaluatedState.NoBlockEvaluated)
                        {
                            throw new ExpressionEvaluatorSyntaxErrorException("No corresponding [if] for [else] statement.");
                        }
                        else
                        {
                            ifElseStatementsList.Add(new List<string>() { "true", subScript });
                            ifBlockEvaluatedState = IfBlockEvaluatedState.NoBlockEvaluated;
                        }
                    }
                    else if (keyword.Equals("catch", StringComparisonForCasing))
                    {
                        if (tryBlockEvaluatedState == TryBlockEvaluatedState.NoBlockEvaluated)
                        {
                            throw new ExpressionEvaluatorSyntaxErrorException("No corresponding [try] for [catch] statement.");
                        }
                        else
                        {
                            tryStatementsList.Add(new List<string>() { "catch", keywordAttributes.Count > 0 ? keywordAttributes[0] : null, subScript });
                            tryBlockEvaluatedState = TryBlockEvaluatedState.Catch;
                        }
                    }
                    else if (keyword.Equals("finally", StringComparisonForCasing))
                    {
                        if (tryBlockEvaluatedState == TryBlockEvaluatedState.NoBlockEvaluated)
                        {
                            throw new ExpressionEvaluatorSyntaxErrorException("No corresponding [try] for [finally] statement.");
                        }
                        else
                        {
                            tryStatementsList.Add(new List<string>() { "finally", subScript });
                            tryBlockEvaluatedState = TryBlockEvaluatedState.NoBlockEvaluated;
                        }
                    }
                    else
                    {
                        ExecuteBlocksStacks();

                        if (keyword.Equals("if", StringComparisonForCasing))
                        {
                            ifElseStatementsList.Add(new List<string>() { keywordAttributes[0], subScript });
                            ifBlockEvaluatedState = IfBlockEvaluatedState.If;
                            tryBlockEvaluatedState = TryBlockEvaluatedState.NoBlockEvaluated;
                        }
                        else if (keyword.Equals("try", StringComparisonForCasing))
                        {
                            tryStatementsList.Add(new List<string>() { subScript });
                            ifBlockEvaluatedState = IfBlockEvaluatedState.NoBlockEvaluated;
                            tryBlockEvaluatedState = TryBlockEvaluatedState.Try;
                        }
                        else if (keyword.Equals("do", StringComparisonForCasing))
                        {
                            if ((blockKeywordsBeginingMatch = blockKeywordsBeginningRegex.Match(script.Substring(i))).Success
                                && blockKeywordsBeginingMatch.Groups["keyword"].Value.Equals("while", StringComparisonForCasing))
                            {
                                i += blockKeywordsBeginingMatch.Length;
                                keywordAttributes = GetExpressionsBetweenParenthesesOrOtherImbricableBrackets(script, ref i, true, ";");

                                i++;

                                Match nextIsEndOfExpressionMatch = null;

                                if ((nextIsEndOfExpressionMatch = nextIsEndOfExpressionRegex.Match(script.Substring(i))).Success)
                                {
                                    i += nextIsEndOfExpressionMatch.Length;

                                    do
                                    {
                                        lastResult = ScriptEvaluate(subScript, ref isReturn, ref isBreak, ref isContinue);

                                        if (isBreak)
                                        {
                                            isBreak = false;
                                            break;
                                        }
                                        if (isContinue)
                                        {
                                            isContinue = false;
                                        }
                                    }
                                    while (!isReturn && (bool)ManageJumpStatementsOrExpressionEval(keywordAttributes[0]));
                                }
                                else
                                {
                                    throw new ExpressionEvaluatorSyntaxErrorException("A [;] character is missing. (After the do while condition)");
                                }
                            }
                            else
                            {
                                throw new ExpressionEvaluatorSyntaxErrorException("No [while] keyword after the [do] keyword and block");
                            }
                        }
                        else if (keyword.Equals("while", StringComparisonForCasing))
                        {
                            while (!isReturn && (bool)ManageJumpStatementsOrExpressionEval(keywordAttributes[0]))
                            {
                                lastResult = ScriptEvaluate(subScript, ref isReturn, ref isBreak, ref isContinue);

                                if (isBreak)
                                {
                                    isBreak = false;
                                    break;
                                }
                                if (isContinue)
                                {
                                    isContinue = false;
                                }
                            }
                        }
                        else if (keyword.Equals("for", StringComparisonForCasing))
                        {
                            void forAction(int index)
                            {
                                if (keywordAttributes.Count > index && !keywordAttributes[index].Trim().Equals(string.Empty))
                                    ManageJumpStatementsOrExpressionEval(keywordAttributes[index]);
                            }

                            for (forAction(0); !isReturn && (bool)ManageJumpStatementsOrExpressionEval(keywordAttributes[1]); forAction(2))
                            {
                                lastResult = ScriptEvaluate(subScript, ref isReturn, ref isBreak, ref isContinue);

                                if (isBreak)
                                {
                                    isBreak = false;
                                    break;
                                }
                                if (isContinue)
                                {
                                    isContinue = false;
                                }
                            }
                        }
                        else if (keyword.Equals("foreach", StringComparisonForCasing))
                        {
                            Match foreachParenthisEvaluationMatch = foreachParenthisEvaluationRegex.Match(keywordAttributes[0]);

                            if (!foreachParenthisEvaluationMatch.Success)
                            {
                                throw new ExpressionEvaluatorSyntaxErrorException("wrong foreach syntax");
                            }
                            else if (!foreachParenthisEvaluationMatch.Groups["in"].Value.Equals("in", StringComparisonForCasing))
                            {
                                throw new ExpressionEvaluatorSyntaxErrorException("no [in] keyword found in foreach");
                            }
                            else
                            {
                                foreach (dynamic foreachValue in (dynamic)Evaluate(foreachParenthisEvaluationMatch.Groups["collection"].Value))
                                {
                                    Variables[foreachParenthisEvaluationMatch.Groups["variableName"].Value] = foreachValue;

                                    lastResult = ScriptEvaluate(subScript, ref isReturn, ref isBreak, ref isContinue);

                                    if (isBreak || isReturn)
                                    {
                                        isBreak = false;
                                        break;
                                    }
                                    if (isContinue)
                                    {
                                        isContinue = false;
                                    }
                                }
                            }
                        }
                    }

                    startOfExpression = i;
                }
                else
                {
                    ExecuteBlocksStacks();

                    bool executed = false;

                    if (TryParseStringAndParenthisAndCurlyBrackets(ref i)) { }
                    else if (script.Length - i > 2 && script.Substring(i, 3).Equals("';'"))
                    {
                        i += 2;
                    }
                    else if (script[i] == ';')
                    {
                        lastResult = ScriptExpressionEvaluate(ref i);
                        executed = true;
                    }

                    if (!OptionScriptNeedSemicolonAtTheEndOfLastExpression && i == script.Length - 1 && !executed)
                    {
                        i++;
                        lastResult = ScriptExpressionEvaluate(ref i);
                        startOfExpression--;
                    }

                    ifBlockEvaluatedState = IfBlockEvaluatedState.NoBlockEvaluated;
                    tryBlockEvaluatedState = TryBlockEvaluatedState.NoBlockEvaluated;

                    if (OptionScriptNeedSemicolonAtTheEndOfLastExpression || i < script.Length)
                        i++;
                }
            }

            if (!script.Substring(startOfExpression).Trim().Equals(string.Empty) && !isReturn && !isBreak && !isContinue && OptionScriptNeedSemicolonAtTheEndOfLastExpression)
                throw new ExpressionEvaluatorSyntaxErrorException("A [;] character is missing.");

            ExecuteBlocksStacks();

            valueReturned = isReturn;
            breakCalled = isBreak;
            continueCalled = isContinue;

            if (isReturn || OptionOnNoReturnKeywordFoundInScriptAction == OptionOnNoReturnKeywordFoundInScriptAction.ReturnAutomaticallyLastEvaluatedExpression)
                return lastResult;
            else if (OptionOnNoReturnKeywordFoundInScriptAction == OptionOnNoReturnKeywordFoundInScriptAction.ReturnNull)
                return null;
            else
                throw new ExpressionEvaluatorSyntaxErrorException("No [return] keyword found");
        }

        #endregion

        #region Expressions

        /// <summary>
        /// Evaluate the specified math or pseudo C# expression
        /// </summary>
        /// <typeparam name="T">The type in which to cast the result of the expression</typeparam>
        /// <param name="expression">the math or pseudo C# expression to evaluate</param>
        /// <returns>The result of the operation if syntax is correct casted in the specified type</returns>
        public T Evaluate<T>(string expression)
        {
            return (T)Evaluate(expression);
        }

        private IList<ParsingMethodDelegate> parsingMethods;

        protected virtual IList<ParsingMethodDelegate> ParsingMethods => parsingMethods ?? (parsingMethods = new List<ParsingMethodDelegate>()
        {
            EvaluateCast,
            EvaluateNumber,
            EvaluateInstanceCreationWithNewKeyword,
            EvaluateVarOrFunc,
            EvaluateOperators,
            EvaluateChar,
            EvaluateParenthis,
            EvaluateIndexing,
            EvaluateString,
            EvaluateTernaryConditionalOperator,
        });

        /// <summary>
        /// Evaluate the specified math or pseudo C# expression
        /// </summary>
        /// <param name="expression">the math or pseudo C# expression to evaluate</param>
        /// <returns>The result of the operation if syntax is correct</returns>
        public object Evaluate(string expression)
        {
            expression = expression.Trim();

            Stack<object> stack = new Stack<object>();
            evaluationStackCount++;
            object result;

            try
            {
                ExpressionEvaluationEventArg expressionEvaluationEventArg =
                    new ExpressionEvaluationEventArg(expression, this);

                ExpressionEvaluating?.Invoke(this, expressionEvaluationEventArg);

                expression = expressionEvaluationEventArg.Expression;

                if (expressionEvaluationEventArg.HasValue)
                {
                    result = expressionEvaluationEventArg.Value;
                }
                else
                {
                    if (GetLambdaExpression(expression, stack))
                        return stack.Pop();

                    for (int i = 0; i < expression.Length; i++)
                    {
                        if (!ParsingMethods.Any(parsingMethod => parsingMethod(expression, stack, ref i)))
                        {
                            string s = expression.Substring(i, 1);

                            if (!s.Trim().Equals(string.Empty))
                            {
                                throw new ExpressionEvaluatorSyntaxErrorException(
                                    $"Invalid character [{(int)s[0]}:{s}]");
                            }
                        }
                    }

                    result = ProcessStack(stack);

                    expressionEvaluationEventArg = new ExpressionEvaluationEventArg(expression, this, result);

                    ExpressionEvaluated?.Invoke(this, expressionEvaluationEventArg);

                    if (expressionEvaluationEventArg.HasValue)
                    {
                        result = expressionEvaluationEventArg.Value;
                    }
                }

                return result;
            }
            catch (TargetInvocationException exception) when (exception.InnerException != null)
            {
                Exception exceptionToThrow = exception.InnerException;

                while (exceptionToThrow is TargetInvocationException && exceptionToThrow.InnerException != null)
                    exceptionToThrow = exceptionToThrow.InnerException;

                ExceptionDispatchInfo.Capture(exceptionToThrow).Throw();
                // Will not go here but need to return something to avoid compilation errors.
                return null;
            }
            finally
            {
                evaluationStackCount--;
            }
        }

        #endregion

        #endregion

        #region Sub parts evaluate methods (protected virtual)

        protected virtual bool EvaluateCast(string expression, Stack<object> stack, ref int i)
        {
            Match castMatch = Regex.Match(expression.Substring(i), CastRegexPattern, optionCaseSensitiveEvaluationActive ? RegexOptions.None : RegexOptions.IgnoreCase);

            if (castMatch.Success)
            {
                string typeName = castMatch.Groups["typeName"].Value;

                int typeIndex = 0;
                Type type = EvaluateType(typeName, ref typeIndex);

                if (type != null)
                {
                    i += castMatch.Length - 1;
                    stack.Push(type);
                    stack.Push(ExpressionOperator.Cast);
                    return true;
                }
            }

            return false;
        }

        protected virtual bool EvaluateNumber(string expression, Stack<object> stack, ref int i)
        {
            string restOfExpression = expression.Substring(i);
            Match numberMatch = Regex.Match(restOfExpression, numberRegexPattern, RegexOptions.IgnoreCase);
            Match otherBaseMatch = otherBasesNumberRegex.Match(restOfExpression);

            if (otherBaseMatch.Success
                && (!otherBaseMatch.Groups["sign"].Success
                || stack.Count == 0
                || stack.Peek() is ExpressionOperator))
            {
                i += otherBaseMatch.Length;
                i--;

                int baseValue = otherBaseMatch.Groups["type"].Value.Equals("b") ? 2 : 16;

                if (otherBaseMatch.Groups["sign"].Success)
                {
                    string value = otherBaseMatch.Groups["value"].Value.Replace("_", "").Substring(2);
                    stack.Push(otherBaseMatch.Groups["sign"].Value.Equals("-") ? -Convert.ToInt32(value, baseValue) : Convert.ToInt32(value, baseValue));
                }
                else
                {
                    stack.Push(Convert.ToInt32(otherBaseMatch.Value.Replace("_", "").Substring(2), baseValue));
                }

                return true;
            }
            else if (numberMatch.Success
                && (!numberMatch.Groups["sign"].Success
                || stack.Count == 0
                || stack.Peek() is ExpressionOperator))
            {
                i += numberMatch.Length;
                i--;

                if (numberMatch.Groups["type"].Success)
                {
                    string type = numberMatch.Groups["type"].Value;
                    string numberNoType = numberMatch.Value.Replace(type, string.Empty).Replace("_", "");

                    if (numberSuffixToParse.TryGetValue(type, out Func<string, CultureInfo, object> parseFunc))
                    {
                        stack.Push(parseFunc(numberNoType, CultureInfoForNumberParsing));
                    }
                }
                else if (OptionForceIntegerNumbersEvaluationsAsDoubleByDefault || numberMatch.Groups["hasdecimal"].Success)
                {
                    stack.Push(double.Parse(numberMatch.Value.Replace("_", ""), NumberStyles.Any, CultureInfoForNumberParsing));
                }
                else
                {
                    stack.Push(int.Parse(numberMatch.Value.Replace("_", ""), NumberStyles.Any, CultureInfoForNumberParsing));
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        protected virtual bool EvaluateInstanceCreationWithNewKeyword(string expression, Stack<object> stack, ref int i)
        {
            if (!OptionNewKeywordEvaluationActive)
                return false;

            Match instanceCreationMatch = instanceCreationWithNewKeywordRegex.Match(expression.Substring(i));

            if (instanceCreationMatch.Success
                && (stack.Count == 0
                || stack.Peek() is ExpressionOperator))
            {
                void InitSimpleObjet(object element, List<string> initArgs)
                {
                    string variable = "V" + Guid.NewGuid().ToString().Replace("-", "");

                    Variables[variable] = element;

                    initArgs.ForEach(subExpr =>
                    {
                        if (subExpr.Contains("="))
                        {
                            string trimmedSubExpr = subExpr.TrimStart();

                            Evaluate($"{variable}{(trimmedSubExpr.StartsWith("[") ? string.Empty : ".")}{trimmedSubExpr}");
                        }
                        else
                        {
                            throw new ExpressionEvaluatorSyntaxErrorException($"A '=' char is missing in [{subExpr}]. It is in a object initializer. It must contains one.");
                        }
                    });

                    Variables.Remove(variable);
                }

                i += instanceCreationMatch.Length;

                if (instanceCreationMatch.Groups["isAnonymous"].Success)
                {
                    object element = new ExpandoObject();

                    List<string> initArgs = GetExpressionsBetweenParenthesesOrOtherImbricableBrackets(expression, ref i, true, OptionInitializersSeparator, "{", "}");

                    InitSimpleObjet(element, initArgs);

                    stack.Push(element);
                }
                else
                {
                    string completeName = instanceCreationMatch.Groups["name"].Value;
                    string genericTypes = instanceCreationMatch.Groups["isgeneric"].Value;

                    int typeIndex = 0;
                    Type type = EvaluateType(completeName + genericTypes, ref typeIndex);

                    if (type == null || (typeIndex > 0 && typeIndex < completeName.Length))
                        throw new ExpressionEvaluatorSyntaxErrorException($"Type or class {completeName}{genericTypes} is unknown");

                    void Init(object element, List<string> initArgs)
                    {
                        if (typeof(IEnumerable).IsAssignableFrom(type)
                            && !typeof(IDictionary).IsAssignableFrom(type)
                            && !typeof(ExpandoObject).IsAssignableFrom(type))
                        {
                            MethodInfo methodInfo = type.GetMethod("Add", BindingFlags.Public | BindingFlags.Instance);

                            initArgs.ForEach(subExpr => methodInfo.Invoke(element, new object[] { Evaluate(subExpr) }));
                        }
                        else if (typeof(IDictionary).IsAssignableFrom(type)
                            && initArgs.All(subExpr => subExpr.TrimStart().StartsWith("{"))
                            && !typeof(ExpandoObject).IsAssignableFrom(type))
                        {
                            initArgs.ForEach(subExpr =>
                            {
                                int subIndex = subExpr.IndexOf("{") + 1;

                                List<string> subArgs = GetExpressionsBetweenParenthesesOrOtherImbricableBrackets(subExpr, ref subIndex, true, OptionInitializersSeparator, "{", "}");

                                if (subArgs.Count == 2)
                                {
                                    dynamic indexedObject = element;
                                    dynamic index = Evaluate(subArgs[0]);
                                    indexedObject[index] = (dynamic)Evaluate(subArgs[1]);
                                }
                                else
                                {
                                    throw new ExpressionEvaluatorSyntaxErrorException($"Bad Number of args in initialization of [{subExpr}]");
                                }
                            });
                        }
                        else
                        {
                            InitSimpleObjet(element, initArgs);
                        }
                    }

                    if (instanceCreationMatch.Groups["isfunction"].Success)
                    {
                        List<string> constructorArgs = GetExpressionsBetweenParenthesesOrOtherImbricableBrackets(expression, ref i, true, OptionFunctionArgumentsSeparator);
                        i++;

                        List<object> cArgs = constructorArgs.ConvertAll(Evaluate);

                        object element = Activator.CreateInstance(type, cArgs.ToArray());

                        Match blockBeginningMatch = blockBeginningRegex.Match(expression.Substring(i));

                        if (blockBeginningMatch.Success)
                        {
                            i += blockBeginningMatch.Length;

                            List<string> initArgs = GetExpressionsBetweenParenthesesOrOtherImbricableBrackets(expression, ref i, true, OptionInitializersSeparator, "{", "}");

                            Init(element, initArgs);
                        }
                        else
                        {
                            i--;
                        }

                        stack.Push(element);
                    }
                    else if (instanceCreationMatch.Groups["isInit"].Success)
                    {
                        object element = Activator.CreateInstance(type, new object[0]);

                        List<string> initArgs = GetExpressionsBetweenParenthesesOrOtherImbricableBrackets(expression, ref i, true, OptionInitializersSeparator, "{", "}");

                        Init(element, initArgs);

                        stack.Push(element);
                    }
                    else if (instanceCreationMatch.Groups["isArray"].Success)
                    {
                        List<string> arrayArgs = GetExpressionsBetweenParenthesesOrOtherImbricableBrackets(expression, ref i, true, OptionInitializersSeparator, "[", "]");
                        i++;
                        Array array = null;

                        if (arrayArgs.Count > 0)
                        {
                            array = Array.CreateInstance(type, arrayArgs.ConvertAll(subExpression => Convert.ToInt32(Evaluate(subExpression))).ToArray());
                        }

                        Match initInNewBeginningMatch = initInNewBeginningRegex.Match(expression.Substring(i));

                        if (initInNewBeginningMatch.Success)
                        {
                            i += initInNewBeginningMatch.Length;

                            List<string> arrayElements = GetExpressionsBetweenParenthesesOrOtherImbricableBrackets(expression, ref i, true, OptionInitializersSeparator, "{", "}");

                            if (array == null)
                                array = Array.CreateInstance(type, arrayElements.Count);

                            Array.Copy(arrayElements.ConvertAll(Evaluate).ToArray(), array, arrayElements.Count);
                        }

                        stack.Push(array);
                    }
                    else
                    {
                        throw new ExpressionEvaluatorSyntaxErrorException($"A new expression requires that type be followed by (), [] or {{}}(Check : {instanceCreationMatch.Value})");
                    }
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        protected virtual bool EvaluateVarOrFunc(string expression, Stack<object> stack, ref int i)
        {
            Match varFuncMatch = varOrFunctionRegEx.Match(expression.Substring(i));

            if (varFuncMatch.Groups["varKeyword"].Success
                && !varFuncMatch.Groups["assignationOperator"].Success)
            {
                throw new ExpressionEvaluatorSyntaxErrorException("Implicit variables must be initialized. [var " + varFuncMatch.Groups["name"].Value + "]");
            }

            if (varFuncMatch.Success
            && (!varFuncMatch.Groups["sign"].Success
                || stack.Count == 0
                || stack.Peek() is ExpressionOperator)
            && !operatorsDictionary.ContainsKey(varFuncMatch.Value.Trim())
            && (!operatorsDictionary.ContainsKey(varFuncMatch.Groups["name"].Value) || varFuncMatch.Groups["inObject"].Success))
            {
                string varFuncName = varFuncMatch.Groups["name"].Value;
                string genericsTypes = varFuncMatch.Groups["isgeneric"].Value;
                bool inObject = varFuncMatch.Groups["inObject"].Success;

                i += varFuncMatch.Length;

                if (varFuncMatch.Groups["isfunction"].Success)
                {
                    List<string> funcArgs = GetExpressionsBetweenParenthesesOrOtherImbricableBrackets(expression, ref i, true, OptionFunctionArgumentsSeparator);

                    if (inObject
                        || Context?.GetType()
                            .GetMethods(InstanceBindingFlag)
                            .Any(methodInfo => methodInfo.Name.Equals(varFuncName, StringComparisonForCasing)) == true)
                    {
                        if (inObject && (stack.Count == 0 || stack.Peek() is ExpressionOperator))
                            throw new ExpressionEvaluatorSyntaxErrorException($"[{varFuncMatch.Value})] must follow an object.");

                        object obj = inObject ? stack.Pop() : Context;
                        object keepObj = obj;
                        Type objType = null;
                        Type[] inferedGenericsTypes = obj?.GetType().GenericTypeArguments;
                        ValueTypeNestingTrace valueTypeNestingTrace = null;

                        if (obj != null && TypesToBlock.Contains(obj.GetType()))
                            throw new ExpressionEvaluatorSecurityException($"{obj.GetType().FullName} type is blocked");
                        else if (obj is Type staticType && TypesToBlock.Contains(staticType))
                            throw new ExpressionEvaluatorSecurityException($"{staticType.FullName} type is blocked");
                        else if (obj is ClassOrEnumType classOrType && TypesToBlock.Contains(classOrType.Type))
                            throw new ExpressionEvaluatorSecurityException($"{classOrType.Type} type is blocked");

                        try
                        {
                            if (obj is NullConditionalNullValue)
                            {
                                stack.Push(obj);
                            }
                            else if (varFuncMatch.Groups["nullConditional"].Success && obj == null)
                            {
                                stack.Push(new NullConditionalNullValue());
                            }
                            else if (obj is BubbleExceptionContainer)
                            {
                                stack.Push(obj);
                                return true;
                            }
                            else
                            {
                                FunctionPreEvaluationEventArg functionPreEvaluationEventArg = new FunctionPreEvaluationEventArg(varFuncName, funcArgs, this, obj, genericsTypes, GetConcreteTypes);

                                PreEvaluateFunction?.Invoke(this, functionPreEvaluationEventArg);

                                if (functionPreEvaluationEventArg.CancelEvaluation)
                                {
                                    throw new ExpressionEvaluatorSyntaxErrorException($"[{objType}] object has no Method named \"{varFuncName}\".");
                                }
                                else if (functionPreEvaluationEventArg.FunctionReturnedValue)
                                {
                                    stack.Push(functionPreEvaluationEventArg.Value);
                                }
                                else
                                {
                                    int argIndex = 0;
                                    List<ArgKeywordsEncaps> argsWithKeywords = new List<ArgKeywordsEncaps>();

                                    List<object> oArgs = funcArgs.ConvertAll(arg =>
                                    {
                                        Match functionArgKeywordsMatch = functionArgKeywordsRegex.Match(arg);
                                        object argValue;

                                        if (functionArgKeywordsMatch.Success)
                                        {
                                            ArgKeywordsEncaps argKeywordEncaps = new ArgKeywordsEncaps()
                                            {
                                                Index = argIndex,
                                                Keyword = functionArgKeywordsMatch.Groups["keyword"].Value,
                                                VariableName = functionArgKeywordsMatch.Groups["varName"].Value
                                            };

                                            argsWithKeywords.Add(argKeywordEncaps);

                                            if (functionArgKeywordsMatch.Groups["typeName"].Success)
                                            {
                                                Type fixedType = ((ClassOrEnumType)Evaluate(functionArgKeywordsMatch.Groups["typeName"].Value)).Type;

                                                variables[argKeywordEncaps.VariableName] = new StronglyTypedVariable() { Type = fixedType, Value = GetDefaultValueOfType(fixedType) };
                                            }
                                            else if (!variables.ContainsKey(argKeywordEncaps.VariableName))
                                            {
                                                variables[argKeywordEncaps.VariableName] = null;
                                            }

                                            argValue = Evaluate(functionArgKeywordsMatch.Groups["toEval"].Value);
                                        }
                                        else
                                        {
                                            argValue = Evaluate(arg);
                                        }

                                        argIndex++;
                                        return argValue;
                                    });

                                    BindingFlags flag = DetermineInstanceOrStatic(ref objType, ref obj, ref valueTypeNestingTrace);

                                    if (!OptionStaticMethodsCallActive && (flag & BindingFlags.Static) != 0)
                                        throw new ExpressionEvaluatorSyntaxErrorException($"[{objType}] object has no Method named \"{varFuncName}\".");
                                    if (!OptionInstanceMethodsCallActive && (flag & BindingFlags.Instance) != 0)
                                        throw new ExpressionEvaluatorSyntaxErrorException($"[{objType}] object has no Method named \"{varFuncName}\".");

                                    // Standard Instance or public method find
                                    MethodInfo methodInfo = GetRealMethod(ref objType, ref obj, varFuncName, flag, oArgs, genericsTypes, inferedGenericsTypes, argsWithKeywords);

                                    // if not found check if obj is an expandoObject or similar
                                    if (obj is IDynamicMetaObjectProvider
                                        && obj is IDictionary<string, object> dictionaryObject
                                        && (dictionaryObject[varFuncName] is InternalDelegate || dictionaryObject[varFuncName] is Delegate))
                                    {
                                        if (dictionaryObject[varFuncName] is InternalDelegate internalDelegate)
                                            stack.Push(internalDelegate(oArgs.ToArray()));
                                        else if (dictionaryObject[varFuncName] is Delegate del)
                                            stack.Push(del.DynamicInvoke(oArgs.ToArray()));
                                    }
                                    else if (objType.GetProperty(varFuncName, InstanceBindingFlag) is PropertyInfo instancePropertyInfo
                                        && (instancePropertyInfo.PropertyType.IsSubclassOf(typeof(Delegate)) || instancePropertyInfo.PropertyType == typeof(Delegate))
                                        && instancePropertyInfo.GetValue(obj) is Delegate del)
                                    {
                                        stack.Push(del.DynamicInvoke(oArgs.ToArray()));
                                    }
                                    else
                                    {
                                        bool isExtention = false;

                                        // if not found try to Find extension methods.
                                        if (methodInfo == null && obj != null)
                                        {
                                            oArgs.Insert(0, obj);
                                            objType = obj.GetType();

                                            object extentionObj = null;
                                            for (int e = 0; e < StaticTypesForExtensionsMethods.Count && methodInfo == null; e++)
                                            {
                                                Type type = StaticTypesForExtensionsMethods[e];
                                                methodInfo = GetRealMethod(ref type, ref extentionObj, varFuncName, StaticBindingFlag, oArgs, genericsTypes, inferedGenericsTypes, argsWithKeywords, true);
                                                isExtention = methodInfo != null;
                                            }
                                        }

                                        if (methodInfo != null)
                                        {
                                            object[] argsArray = oArgs.ToArray();
                                            stack.Push(methodInfo.Invoke(isExtention ? null : obj, argsArray));
                                            argsWithKeywords
                                                .FindAll(argWithKeyword => argWithKeyword.Keyword.Equals("out", StringComparisonForCasing) || argWithKeyword.Keyword.Equals("ref", StringComparisonForCasing))
                                                .ForEach(outOrRefArg => AssignVariable(outOrRefArg.VariableName, argsArray[outOrRefArg.Index + (isExtention ? 1 : 0)]));
                                        }
                                        else if (objType.GetProperty(varFuncName, StaticBindingFlag) is PropertyInfo staticPropertyInfo
                                            && (staticPropertyInfo.PropertyType.IsSubclassOf(typeof(Delegate)) || staticPropertyInfo.PropertyType == typeof(Delegate))
                                            && staticPropertyInfo.GetValue(obj) is Delegate del2)
                                        {
                                            stack.Push(del2.DynamicInvoke(oArgs.ToArray()));
                                        }
                                        else
                                        {
                                            FunctionEvaluationEventArg functionEvaluationEventArg = new FunctionEvaluationEventArg(varFuncName, funcArgs, this, obj ?? keepObj, genericsTypes, GetConcreteTypes);

                                            EvaluateFunction?.Invoke(this, functionEvaluationEventArg);

                                            if (functionEvaluationEventArg.FunctionReturnedValue)
                                            {
                                                stack.Push(functionEvaluationEventArg.Value);
                                            }
                                            else
                                            {
                                                if (OptionDetectExtensionMethodsOverloadsOnExtensionMethodNotFound)
                                                {
                                                    IEnumerable<MethodInfo> query = from type in StaticTypesForExtensionsMethods
                                                                                    where
                                                                                          !type.IsGenericType &&
                                                                                          type.IsSealed &&
                                                                                          !type.IsNested
                                                                                    from method in type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                                                                                    where method.IsDefined(typeof(ExtensionAttribute), false)
                                                                                    where method.GetParameters()[0].ParameterType == objType // static extMethod(this outType, ...)
                                                                                    select method;

                                                    if (query.Any())
                                                    {
                                                        string fnArgsPrint = string.Join(",", funcArgs);
                                                        string fnOverloadsPrint = "";

                                                        foreach (MethodInfo mi in query)
                                                        {
                                                            ParameterInfo[] parInfo = mi.GetParameters();
                                                            fnOverloadsPrint += string.Join(",", parInfo.Select(x => x.ParameterType.FullName ?? x.ParameterType.Name)) + "\n";
                                                        }

                                                        throw new ExpressionEvaluatorSyntaxErrorException($"[{objType}] extension method \"{varFuncName}\" has no overload for arguments: {fnArgsPrint}. Candidates: {fnOverloadsPrint}");
                                                    }
                                                }

                                                throw new ExpressionEvaluatorSyntaxErrorException($"[{objType}] object has no Method named \"{varFuncName}\".");
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        catch (ExpressionEvaluatorSecurityException)
                        {
                            throw;
                        }
                        catch (ExpressionEvaluatorSyntaxErrorException)
                        {
                            throw;
                        }
                        catch (NullReferenceException nullException)
                        {
                            stack.Push(new BubbleExceptionContainer(nullException));

                            return true;
                        }
                        catch (Exception ex)
                        {
                            //Transport the exception in stack.
                            var nestedException = new ExpressionEvaluatorSyntaxErrorException($"The call of the method \"{varFuncName}\" on type [{objType}] generate this error : {ex.InnerException?.Message ?? ex.Message}", ex);
                            stack.Push(new BubbleExceptionContainer(nestedException));
                            return true;  //Signals an error to the parsing method array call                          
                        }
                    }
                    else
                    {
                        FunctionPreEvaluationEventArg functionPreEvaluationEventArg = new FunctionPreEvaluationEventArg(varFuncName, funcArgs, this, null, genericsTypes, GetConcreteTypes);

                        PreEvaluateFunction?.Invoke(this, functionPreEvaluationEventArg);

                        if (functionPreEvaluationEventArg.CancelEvaluation)
                        {
                            throw new ExpressionEvaluatorSyntaxErrorException($"Function [{varFuncName}] unknown in expression : [{expression.Replace("\r", "").Replace("\n", "")}]");
                        }
                        else if (functionPreEvaluationEventArg.FunctionReturnedValue)
                        {
                            stack.Push(functionPreEvaluationEventArg.Value);
                        }
                        else if (DefaultFunctions(varFuncName, funcArgs, out object funcResult))
                        {
                            stack.Push(funcResult);
                        }
                        else if (Variables.TryGetValue(varFuncName, out object o) && o is InternalDelegate lambdaExpression)
                        {
                            stack.Push(lambdaExpression.Invoke(funcArgs.ConvertAll(Evaluate).ToArray()));
                        }
                        else if (Variables.TryGetValue(varFuncName, out o) && o is Delegate delegateVar)
                        {
                            stack.Push(delegateVar.DynamicInvoke(funcArgs.ConvertAll(Evaluate).ToArray()));
                        }
                        else if (Variables.TryGetValue(varFuncName, out o) && o is MethodsGroupEncaps methodsGroupEncaps)
                        {
                            List<object> args = funcArgs.ConvertAll(Evaluate);
                            List<object> modifiedArgs= null;
                            MethodInfo methodInfo = null;

                            for (int m = 0; methodInfo == null && m < methodsGroupEncaps.MethodsGroup.Length; m++)
                            {
                                modifiedArgs = new List<object>(args);

                                methodInfo = TryToCastMethodParametersToMakeItCallable(methodsGroupEncaps.MethodsGroup[m], modifiedArgs, genericsTypes, new Type[0], methodsGroupEncaps.ContainerObject);
                            }

                            if(methodInfo != null)
                                stack.Push(methodInfo.Invoke(methodsGroupEncaps.ContainerObject, modifiedArgs?.ToArray()));
                        }
                        else
                        {
                            FunctionEvaluationEventArg functionEvaluationEventArg = new FunctionEvaluationEventArg(varFuncName, funcArgs, this, genericTypes: genericsTypes, evaluateGenericTypes: GetConcreteTypes);

                            EvaluateFunction?.Invoke(this, functionEvaluationEventArg);

                            if (functionEvaluationEventArg.FunctionReturnedValue)
                            {
                                stack.Push(functionEvaluationEventArg.Value);
                            }
                            else
                            {
                                throw new ExpressionEvaluatorSyntaxErrorException($"Function [{varFuncName}] unknown in expression : [{expression.Replace("\r", "").Replace("\n", "")}]");
                            }
                        }
                    }
                }
                else
                {
                    if (inObject
                        || Context?.GetType()
                            .GetProperties(InstanceBindingFlag)
                            .Any(propInfo => propInfo.Name.Equals(varFuncName, StringComparisonForCasing)) == true
                        || Context?.GetType()
                            .GetFields(InstanceBindingFlag)
                            .Any(fieldInfo => fieldInfo.Name.Equals(varFuncName, StringComparisonForCasing)) == true)
                    {
                        if (inObject && (stack.Count == 0 || stack.Peek() is ExpressionOperator))
                            throw new ExpressionEvaluatorSyntaxErrorException($"[{varFuncMatch.Value}] must follow an object.");

                        object obj = inObject ? stack.Pop() : Context;
                        object keepObj = obj;
                        Type objType = null;
                        ValueTypeNestingTrace valueTypeNestingTrace = null;

                        if (obj != null && TypesToBlock.Contains(obj.GetType()))
                            throw new ExpressionEvaluatorSecurityException($"{obj.GetType().FullName} type is blocked");
                        else if (obj is Type staticType && TypesToBlock.Contains(staticType))
                            throw new ExpressionEvaluatorSecurityException($"{staticType.FullName} type is blocked");
                        else if (obj is ClassOrEnumType classOrType && TypesToBlock.Contains(classOrType.Type))
                            throw new ExpressionEvaluatorSecurityException($"{classOrType.Type} type is blocked");

                        try
                        {
                            if (obj is NullConditionalNullValue)
                            {
                                stack.Push(obj);
                            }
                            else if (varFuncMatch.Groups["nullConditional"].Success && obj == null)
                            {
                                stack.Push(new NullConditionalNullValue());
                            }
                            else if (obj is BubbleExceptionContainer)
                            {
                                stack.Push(obj);
                                return true;
                            }
                            else
                            {
                                VariablePreEvaluationEventArg variablePreEvaluationEventArg = new VariablePreEvaluationEventArg(varFuncName, this, obj, genericsTypes, GetConcreteTypes);

                                PreEvaluateVariable?.Invoke(this, variablePreEvaluationEventArg);

                                if (variablePreEvaluationEventArg.CancelEvaluation)
                                {
                                    throw new ExpressionEvaluatorSyntaxErrorException($"[{objType}] object has no public Property or Member named \"{varFuncName}\".", new Exception("Variable evaluation canceled"));
                                }
                                else if (variablePreEvaluationEventArg.HasValue)
                                {
                                    stack.Push(variablePreEvaluationEventArg.Value);
                                }
                                else
                                {
                                    BindingFlags flag = DetermineInstanceOrStatic(ref objType, ref obj, ref valueTypeNestingTrace);

                                    if (!OptionStaticPropertiesGetActive && (flag & BindingFlags.Static) != 0)
                                        throw new ExpressionEvaluatorSyntaxErrorException($"[{objType}] object has no public Property or Field named \"{varFuncName}\".");
                                    if (!OptionInstancePropertiesGetActive && (flag & BindingFlags.Instance) != 0)
                                        throw new ExpressionEvaluatorSyntaxErrorException($"[{objType}] object has no public Property or Field named \"{varFuncName}\".");

                                    bool isDynamic = (flag & BindingFlags.Instance) != 0 && obj is IDynamicMetaObjectProvider && obj is IDictionary<string, object>;
                                    IDictionary<string, object> dictionaryObject = obj as IDictionary<string, object>;

                                    MemberInfo member = isDynamic ? null : objType?.GetProperty(varFuncName, flag);
                                    dynamic varValue = null;
                                    bool assign = true;

                                    if (member == null && !isDynamic)
                                        member = objType.GetField(varFuncName, flag);

                                    if (member == null && !isDynamic)
                                    {
                                        MethodInfo[] methodsGroup = objType.GetMember(varFuncName, flag).OfType<MethodInfo>().ToArray();

                                        if(methodsGroup.Length > 0)
                                        {
                                            varValue = new MethodsGroupEncaps()
                                            {
                                                ContainerObject = obj,
                                                MethodsGroup = methodsGroup
                                            };
                                        }
                                    }

                                    bool pushVarValue = true;

                                    if (isDynamic)
                                    {
                                        if (!varFuncMatch.Groups["assignationOperator"].Success || varFuncMatch.Groups["assignmentPrefix"].Success)
                                            varValue = dictionaryObject.ContainsKey(varFuncName) ? dictionaryObject[varFuncName] : null;
                                        else
                                            pushVarValue = false;
                                    }

									bool isVarValueSet = false;
                                    if (member == null && pushVarValue)
                                    {
                                        VariableEvaluationEventArg variableEvaluationEventArg = new VariableEvaluationEventArg(varFuncName, this, obj ?? keepObj, genericsTypes, GetConcreteTypes);

                                        EvaluateVariable?.Invoke(this, variableEvaluationEventArg);

                                        if (variableEvaluationEventArg.HasValue)
                                        {
                                            varValue = variableEvaluationEventArg.Value;
											isVarValueSet = true;
                                        }
                                    }

                                    if (!isVarValueSet && !isDynamic && varValue == null && pushVarValue)
                                    {
                                        varValue = ((dynamic)member).GetValue(obj);

                                        if (varValue is ValueType)
                                        {
                                            stack.Push(valueTypeNestingTrace = new ValueTypeNestingTrace
                                            {
                                                Container = valueTypeNestingTrace ?? obj,
                                                Member = member,
                                                Value = varValue
                                            });

                                            pushVarValue = false;
                                        }
                                    }

                                    if (pushVarValue)
                                    {
                                        stack.Push(varValue);
                                    }

                                    if (OptionPropertyOrFieldSetActive)
                                    {
                                        if (varFuncMatch.Groups["assignationOperator"].Success)
                                        {
                                            varValue = ManageKindOfAssignation(expression, ref i, varFuncMatch, () => varValue, stack);
                                        }
                                        else if (varFuncMatch.Groups["postfixOperator"].Success)
                                        {
                                            varValue = varFuncMatch.Groups["postfixOperator"].Value.Equals("++") ? varValue + 1 : varValue - 1;
                                        }
                                        else
                                        {
                                            assign = false;
                                        }

                                        if (assign)
                                        {
                                            if (isDynamic)
                                            {
                                                dictionaryObject[varFuncName] = varValue;
                                            }
                                            else if (valueTypeNestingTrace != null)
                                            {
                                                valueTypeNestingTrace.Value = varValue;
                                                valueTypeNestingTrace.AssignValue();
                                            }
                                            else
                                            {
                                                ((dynamic)member).SetValue(obj, varValue);
                                            }
                                        }
                                    }
                                    else if (varFuncMatch.Groups["assignationOperator"].Success)
                                    {
                                        i -= varFuncMatch.Groups["assignationOperator"].Length;
                                    }
                                    else if (varFuncMatch.Groups["postfixOperator"].Success)
                                    {
                                        i -= varFuncMatch.Groups["postfixOperator"].Length;
                                    }
                                }
                            }
                        }
                        catch (ExpressionEvaluatorSecurityException)
                        {
                            throw;
                        }
                        catch (ExpressionEvaluatorSyntaxErrorException)
                        {
                            throw;
                        }
                        catch (Exception ex)
                        {
                            //Transport the exception in stack.
                            var nestedException = new ExpressionEvaluatorSyntaxErrorException($"[{objType}] object has no public Property or Member named \"{varFuncName}\".", ex);
                            stack.Push(new BubbleExceptionContainer(nestedException));
                            i--;
                            return true;  //Signals an error to the parsing method array call
                        }
                    }
                    else
                    {
                        VariablePreEvaluationEventArg variablePreEvaluationEventArg = new VariablePreEvaluationEventArg(varFuncName, this, genericTypes: genericsTypes, evaluateGenericTypes: GetConcreteTypes);

                        PreEvaluateVariable?.Invoke(this, variablePreEvaluationEventArg);

                        if (variablePreEvaluationEventArg.CancelEvaluation)
                        {
                            throw new ExpressionEvaluatorSyntaxErrorException($"Variable [{varFuncName}] unknown in expression : [{expression}]");
                        }
                        else if (variablePreEvaluationEventArg.HasValue)
                        {
                            stack.Push(variablePreEvaluationEventArg.Value);
                        }
                        else if (defaultVariables.TryGetValue(varFuncName, out object varValueToPush))
                        {
                            stack.Push(varValueToPush);
                        }
                        else if ((Variables.TryGetValue(varFuncName, out object cusVarValueToPush)
                                || varFuncMatch.Groups["assignationOperator"].Success
                                || (stack.Count == 1 && stack.Peek() is ClassOrEnumType && string.IsNullOrWhiteSpace(expression.Substring(i))))
                            && (cusVarValueToPush == null || !TypesToBlock.Contains(cusVarValueToPush.GetType())))
                        {
                            if (stack.Count == 1 && stack.Peek() is ClassOrEnumType classOrEnum)
                            {
                                if (Variables.ContainsKey(varFuncName))
                                    throw new ExpressionEvaluatorSyntaxErrorException($"Can not declare a new variable named [{varFuncName}]. A variable with this name already exists");
                                else if (varFuncMatch.Groups["varKeyword"].Success)
                                    throw new ExpressionEvaluatorSyntaxErrorException("Can not declare a variable with type and var keyword.");
                                else if (varFuncMatch.Groups["dynamicKeyword"].Success)
                                    throw new ExpressionEvaluatorSyntaxErrorException("Can not declare a variable with type and dynamic keyword.");

                                stack.Pop();

                                Variables[varFuncName] = new StronglyTypedVariable
                                {
                                    Type = classOrEnum.Type,
                                    Value = GetDefaultValueOfType(classOrEnum.Type),
                                };
                            }

                            if (cusVarValueToPush is StronglyTypedVariable typedVariable)
                                cusVarValueToPush = typedVariable.Value;

                            stack.Push(cusVarValueToPush);

                            if (OptionVariableAssignationActive)
                            {
                                bool assign = true;

                                if (varFuncMatch.Groups["assignationOperator"].Success)
                                {
                                    cusVarValueToPush = ManageKindOfAssignation(expression, ref i, varFuncMatch, () => cusVarValueToPush, stack);
                                }
                                else if (varFuncMatch.Groups["postfixOperator"].Success)
                                {
                                    cusVarValueToPush = varFuncMatch.Groups["postfixOperator"].Value.Equals("++") ? (dynamic)cusVarValueToPush + 1 : (dynamic)cusVarValueToPush - 1;
                                }
                                else if (varFuncMatch.Groups["prefixOperator"].Success)
                                {
                                    stack.Pop();
                                    cusVarValueToPush = varFuncMatch.Groups["prefixOperator"].Value.Equals("++") ? (dynamic)cusVarValueToPush + 1 : (dynamic)cusVarValueToPush - 1;
                                    stack.Push(cusVarValueToPush);
                                }
                                else
                                {
                                    assign = false;
                                }

                                if (assign)
                                {
                                    AssignVariable(varFuncName, cusVarValueToPush);
                                }
                            }
                            else if (varFuncMatch.Groups["assignationOperator"].Success)
                            {
                                i -= varFuncMatch.Groups["assignationOperator"].Length;
                            }
                            else if (varFuncMatch.Groups["postfixOperator"].Success)
                            {
                                i -= varFuncMatch.Groups["postfixOperator"].Length;
                            }
                        }
                        else
                        {
                            Type staticType = EvaluateType(expression, ref i, varFuncName, genericsTypes);

                            if (staticType != null)
                            {
                                stack.Push(new ClassOrEnumType() { Type = staticType });
                            }
                            else
                            {
                                VariableEvaluationEventArg variableEvaluationEventArg = new VariableEvaluationEventArg(varFuncName, this, genericTypes: genericsTypes, evaluateGenericTypes: GetConcreteTypes);

                                EvaluateVariable?.Invoke(this, variableEvaluationEventArg);

                                if (variableEvaluationEventArg.HasValue)
                                {
                                    stack.Push(variableEvaluationEventArg.Value);
                                }
                                else
                                {
                                    throw new ExpressionEvaluatorSyntaxErrorException($"Variable [{varFuncName}] unknown in expression : [{expression}]");
                                }
                            }
                        }
                    }

                    i--;
                }

                if (varFuncMatch.Groups["sign"].Success)
                {
                    object temp = stack.Pop();
                    stack.Push(varFuncMatch.Groups["sign"].Value.Equals("+") ? ExpressionOperator.UnaryPlus : ExpressionOperator.UnaryMinus);
                    stack.Push(temp);
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        protected virtual Type EvaluateType(string expression,ref int i, string currentName = "", string genericsTypes = "")
        {
            string typeName = $"{currentName}{((i < expression.Length && expression.Substring(i)[0] == '?') ? "?" : "") }";
            Type staticType = GetTypeByFriendlyName(typeName, genericsTypes);

            // For inline namespace parsing
            if (staticType == null)
            {
                if (OptionInlineNamespacesEvaluationRule != InlineNamespacesEvaluationRule.BlockAll)
                {
                    int subIndex = 0;
                    Match namespaceMatch = varOrFunctionRegEx.Match(expression.Substring(i + subIndex));

                    while (staticType == null
                        && namespaceMatch.Success
                        && !namespaceMatch.Groups["sign"].Success
                        && !namespaceMatch.Groups["assignationOperator"].Success
                        && !namespaceMatch.Groups["postfixOperator"].Success
                        && !namespaceMatch.Groups["isfunction"].Success
                        && i + subIndex < expression.Length
                        && !typeName.EndsWith("?"))
                    {
                        subIndex += namespaceMatch.Length;
                        typeName += $"{namespaceMatch.Groups["inObject"].Value}{namespaceMatch.Groups["name"].Value}{((i + subIndex < expression.Length && expression.Substring(i + subIndex)[0] == '?') ? "?" : "") }";

                        staticType = GetTypeByFriendlyName(typeName, namespaceMatch.Groups["isgeneric"].Value);

                        if (staticType != null)
                        {
                            if((OptionInlineNamespacesEvaluationRule == InlineNamespacesEvaluationRule.BlockOnlyInlineNamespacesList && InlineNamespacesList.Contains(staticType.Namespace))
                                || (OptionInlineNamespacesEvaluationRule == InlineNamespacesEvaluationRule.AllowOnlyInlineNamespacesList && !InlineNamespacesList.Contains(staticType.Namespace)))
                            {
                                staticType = null;
                            }
                            else
                            {
                                i += subIndex;
                            }

                            break;
                        }

                        namespaceMatch = varOrFunctionRegEx.Match(expression.Substring(i + subIndex));
                    }
                }
                else
                {
                    int subIndex = 0;
                    Match typeMatch = varOrFunctionRegEx.Match(expression.Substring(i + subIndex));

                    if (staticType == null
                        && typeMatch.Success
                        && !typeMatch.Groups["sign"].Success
                        && !typeMatch.Groups["assignationOperator"].Success
                        && !typeMatch.Groups["postfixOperator"].Success
                        && !typeMatch.Groups["isfunction"].Success
                        && !typeMatch.Groups["inObject"].Success
                        && i + subIndex < expression.Length
                        && !typeName.EndsWith("?"))
                    {
                        subIndex += typeMatch.Length;
                        typeName += $"{typeMatch.Groups["name"].Value}{((i + subIndex < expression.Length && expression.Substring(i + subIndex)[0] == '?') ? "?" : "") }";

                        staticType = GetTypeByFriendlyName(typeName, typeMatch.Groups["isgeneric"].Value);

                        if (staticType != null)
                        {
                            i += subIndex;
                        }
                    }
                }
            }

            if (typeName.EndsWith("?") && staticType != null)
                i++;

            // For nested type parsing
            if (staticType != null)
            {
                int subIndex = 0;
                Match nestedTypeMatch = varOrFunctionRegEx.Match(expression.Substring(i + subIndex));
                while (nestedTypeMatch.Success
                    && !nestedTypeMatch.Groups["sign"].Success
                    && !nestedTypeMatch.Groups["assignationOperator"].Success
                    && !nestedTypeMatch.Groups["postfixOperator"].Success
                    && !nestedTypeMatch.Groups["isfunction"].Success)
                {
                    subIndex = nestedTypeMatch.Length;
                    typeName += $"+{nestedTypeMatch.Groups["name"].Value}{((i + subIndex < expression.Length && expression.Substring(i + subIndex)[0] == '?') ? "?" : "") }";

                    Type nestedType = GetTypeByFriendlyName(typeName, nestedTypeMatch.Groups["isgeneric"].Value);
                    if (nestedType != null)
                    {
                        i += subIndex;
                        staticType = nestedType;

                        if (typeName.EndsWith("?"))
                            i++;
                    }
                    else
                    {
                        break;
                    }

                    nestedTypeMatch = varOrFunctionRegEx.Match(expression.Substring(i));
                }
            }

            Match arrayTypeMatch;

            if(i < expression.Length && (arrayTypeMatch = arrayTypeDetectionRegex.Match(expression.Substring(i))).Success)
            {
                Type arrayType = GetTypeByFriendlyName(staticType + arrayTypeMatch.Value);
                if(arrayType != null)
                {
                    i += arrayTypeMatch.Length;
                    staticType = arrayType;
                }
            }

            return staticType;
        }

        protected virtual bool EvaluateChar(string expression, Stack<object> stack, ref int i)
        {
            if (!OptionCharEvaluationActive)
                return false;

            string s = expression.Substring(i, 1);

            if (s.Equals("'"))
            {
                i++;

                if (expression.Substring(i, 1).Equals(@"\"))
                {
                    i++;
                    char escapedChar = expression[i];

                    if (charEscapedCharDict.ContainsKey(escapedChar))
                    {
                        stack.Push(charEscapedCharDict[escapedChar]);
                        i++;
                    }
                    else
                    {
                        throw new ExpressionEvaluatorSyntaxErrorException("Not known escape sequence in literal character");
                    }
                }
                else if (expression.Substring(i, 1).Equals("'"))
                {
                    throw new ExpressionEvaluatorSyntaxErrorException("Empty literal character is not valid");
                }
                else
                {
                    stack.Push(expression[i]);
                    i++;
                }

                if (expression.Substring(i, 1).Equals("'"))
                {
                    return true;
                }
                else
                {
                    throw new ExpressionEvaluatorSyntaxErrorException("Too much characters in the literal character");
                }
            }
            else
            {
                return false;
            }
        }

        protected virtual bool EvaluateOperators(string expression, Stack<object> stack, ref int i)
        {
            string regexPattern = "^(" + string.Join("|", operatorsDictionary
                .Keys
                .OrderByDescending(key => key.Length)
                .Select(Regex.Escape)) + ")";

            Match match = Regex.Match(expression.Substring(i), regexPattern, optionCaseSensitiveEvaluationActive ? RegexOptions.None : RegexOptions.IgnoreCase);

            if (match.Success)
            {
                string op = match.Value;

                if (op.Equals("+") && (stack.Count == 0 || (stack.Peek() is ExpressionOperator previousOp && !LeftOperandOnlyOperatorsEvaluationDictionary.Contains(previousOp))))
                    stack.Push(ExpressionOperator.UnaryPlus);
                else if (op.Equals("-") && (stack.Count == 0 || (stack.Peek() is ExpressionOperator previousOp2 && !LeftOperandOnlyOperatorsEvaluationDictionary.Contains(previousOp2))))
                    stack.Push(ExpressionOperator.UnaryMinus);
                else
                    stack.Push(operatorsDictionary[op]);
                i += op.Length - 1;
                return true;
            }

            return false;
        }

        protected virtual bool EvaluateTernaryConditionalOperator(string expression, Stack<object> stack, ref int i)
        {
            if (expression.Substring(i, 1).Equals("?"))
            {
                bool condition = (bool)ProcessStack(stack);

                string restOfExpression = expression.Substring(i + 1);

                for (int j = 0; j < restOfExpression.Length; j++)
                {
                    string s2 = restOfExpression.Substring(j, 1);

                    Match internalStringMatch = stringBeginningRegex.Match(restOfExpression.Substring(j));

                    if (internalStringMatch.Success)
                    {
                        string innerString = internalStringMatch.Value + GetCodeUntilEndOfString(restOfExpression.Substring(j + internalStringMatch.Length), internalStringMatch);
                        j += innerString.Length - 1;
                    }
                    else if (s2.Equals("("))
                    {
                        j++;
                        GetExpressionsBetweenParenthesesOrOtherImbricableBrackets(restOfExpression, ref j, false);
                    }
                    else if (s2.Equals(":"))
                    {
                        stack.Clear();

                        stack.Push(condition ? Evaluate(restOfExpression.Substring(0, j)) : Evaluate(restOfExpression.Substring(j + 1)));

                        i = expression.Length;

                        return true;
                    }
                }
            }

            return false;
        }

        protected virtual bool EvaluateParenthis(string expression, Stack<object> stack, ref int i)
        {
            string s = expression.Substring(i, 1);

            if (s.Equals(")"))
                throw new Exception($"To much ')' characters are defined in expression : [{expression}] : no corresponding '(' fund.");

            if (s.Equals("("))
            {
                i++;

                if (stack.Count > 0 && stack.Peek() is InternalDelegate)
                {
                    List<string> expressionsInParenthis = GetExpressionsBetweenParenthesesOrOtherImbricableBrackets(expression, ref i, true);

                    InternalDelegate lambdaDelegate = stack.Pop() as InternalDelegate;

                    stack.Push(lambdaDelegate(expressionsInParenthis.ConvertAll(Evaluate).ToArray()));
                }
                else
                {
                    CorrectStackWithUnaryPlusOrMinusBeforeParenthisIfNecessary(stack);

                    List<string> expressionsInParenthis = GetExpressionsBetweenParenthesesOrOtherImbricableBrackets(expression, ref i, false);

                    stack.Push(Evaluate(expressionsInParenthis[0]));
                }

                return true;
            }

            return false;
        }

        protected virtual void CorrectStackWithUnaryPlusOrMinusBeforeParenthisIfNecessary(Stack<object> stack)
        {
            if (stack.Count > 0 && stack.Peek() is ExpressionOperator op && (op == ExpressionOperator.Plus || op == ExpressionOperator.Minus))
            {
                stack.Pop();

                if (stack.Count == 0 || stack.Peek() is ExpressionOperator)
                {
                    stack.Push(op == ExpressionOperator.Plus ? ExpressionOperator.UnaryPlus : ExpressionOperator.UnaryMinus);
                }
                else
                {
                    stack.Push(op);
                }
            }
        }

        protected virtual bool EvaluateIndexing(string expression, Stack<object> stack, ref int i)
        {
            if (!OptionIndexingActive)
                return false;

            Match indexingBeginningMatch = indexingBeginningRegex.Match(expression.Substring(i));

            if (indexingBeginningMatch.Success)
            {
                i += indexingBeginningMatch.Length;

                dynamic left = stack.Pop();

                List<string> indexingArgs = GetExpressionsBetweenParenthesesOrOtherImbricableBrackets(expression, ref i, true, startChar: "[", endChar: "]");

                if (left is NullConditionalNullValue || left is BubbleExceptionContainer)
                {
                    stack.Push(left);
                    return true;
                }
                else if(indexingBeginningMatch.Groups["nullConditional"].Success && left == null)
                {
                    stack.Push(new NullConditionalNullValue());
                    return true;
                }

                IndexingPreEvaluationEventArg indexingPreEvaluationEventArg = new IndexingPreEvaluationEventArg(indexingArgs, this, left);

                PreEvaluateIndexing?.Invoke(this, indexingPreEvaluationEventArg);

                if (indexingPreEvaluationEventArg.CancelEvaluation)
                {
                    throw new ExpressionEvaluatorSyntaxErrorException($"[{left.GetType()}] can not be indexed.");
                }
                else if (indexingPreEvaluationEventArg.HasValue)
                {
                    stack.Push(indexingPreEvaluationEventArg.Value);
                }
                else
                {
                    Match assignationOrPostFixOperatorMatch = null;
                    dynamic valueToPush = null;
                    List<dynamic> oIndexingArgs = indexingArgs.ConvertAll(Evaluate);
                    PropertyInfo[] itemProperties = null;

                    if (!(left is IDictionary<string, object>))
                    {
                        Type type = ((object)left).GetType();

                        if(type.IsArray && OptionForceIntegerNumbersEvaluationsAsDoubleByDefault)
                        {
                            oIndexingArgs = oIndexingArgs.ConvertAll(o => o is double ? (int)o : o);
                        }
                        else
                        {
                            itemProperties = type.GetProperties()
                                .Where(property => property.GetIndexParameters().Length > 0
                                    && property.GetIndexParameters().Length == oIndexingArgs.Count
                                    && property.GetIndexParameters().All(parameter => parameter.ParameterType.IsAssignableFrom(oIndexingArgs[parameter.Position].GetType())))
                                .ToArray();

                            if (itemProperties.Length == 0 && OptionForceIntegerNumbersEvaluationsAsDoubleByDefault)
                            {
                                List<dynamic> backupIndexedArgs = oIndexingArgs.ToList();

                                itemProperties = type.GetProperties()
                                    .Where(property => property.Name.Equals("Item")
                                        && property.GetIndexParameters().Length == oIndexingArgs.Count
                                        && property.GetIndexParameters().All(parameter =>
                                        {
                                            if (parameter.ParameterType.IsAssignableFrom(((object)oIndexingArgs[parameter.Position]).GetType()))
                                            {
                                                return true;
                                            }
                                            else if (parameter.ParameterType == typeof(int) && oIndexingArgs[parameter.Position] is double)
                                            {
                                                oIndexingArgs[parameter.Position] = (int)oIndexingArgs[parameter.Position];
                                                return true;
                                            }
                                            else
                                            {
                                                return false;
                                            }
                                        }))
                                    .ToArray();

                                if (itemProperties.Length == 0)
                                    oIndexingArgs = backupIndexedArgs;
                            }
                        }
                    }

                    object GetIndexedObject()
                    {
                        if (left is IDictionary<string, object> dictionaryLeft)
                            return dictionaryLeft[oIndexingArgs[0]];
                        else if (itemProperties?.Length > 0)
                            return itemProperties[0].GetValue(left, oIndexingArgs.Cast<object>().ToArray());
                        else
                            return left[oIndexingArgs[0]];
                    }

                    if (OptionIndexingAssignationActive && (assignationOrPostFixOperatorMatch = assignationOrPostFixOperatorRegex.Match(expression.Substring(i + 1))).Success)
                    {
                        i += assignationOrPostFixOperatorMatch.Length + 1;

                        bool postFixOperator = assignationOrPostFixOperatorMatch.Groups["postfixOperator"].Success;
                        string exceptionContext = postFixOperator ? "++ or -- operator" : "an assignation";

                        if (stack.Count > 1)
                            throw new ExpressionEvaluatorSyntaxErrorException($"The left part of {exceptionContext} must be a variable, a property or an indexer.");

                        if (indexingBeginningMatch.Groups["nullConditional"].Success)
                            throw new ExpressionEvaluatorSyntaxErrorException($"Null conditional is not usable left to {exceptionContext}");

                        if (postFixOperator)
                        {
                            if (left is IDictionary<string, object> dictionaryLeft)
                            {
                                valueToPush = assignationOrPostFixOperatorMatch.Groups["postfixOperator"].Value.Equals("++") ? dictionaryLeft[oIndexingArgs[0]]++ : dictionaryLeft[oIndexingArgs[0]]--;
                            }
                            else if (itemProperties?.Length > 0)
                            {
                                valueToPush = itemProperties[0].GetValue(left, oIndexingArgs.Cast<object>().ToArray());
                                itemProperties[0].SetValue(left, assignationOrPostFixOperatorMatch.Groups["postfixOperator"].Value.Equals("++") ? valueToPush + 1 : valueToPush - 1, oIndexingArgs.Cast<object>().ToArray());
                            }
                            else
                            {
                                valueToPush = assignationOrPostFixOperatorMatch.Groups["postfixOperator"].Value.Equals("++") ? left[oIndexingArgs[0]]++ : left[oIndexingArgs[0]]--;
                            }
                        }
                        else
                        {
                            valueToPush = ManageKindOfAssignation(expression, ref i, assignationOrPostFixOperatorMatch, GetIndexedObject);

                            if (left is IDictionary<string, object> dictionaryLeft)
                                dictionaryLeft[oIndexingArgs[0]] = valueToPush;
                            else if (itemProperties?.Length > 0)
                                itemProperties[0].SetValue(left, valueToPush, oIndexingArgs.Cast<object>().ToArray());
                            else
                                left[oIndexingArgs[0]] = valueToPush;

                            stack.Clear();
                        }
                    }
                    else
                    {
                        valueToPush = GetIndexedObject();
                    }

                    stack.Push(valueToPush);
                }

                return true;
            }

            return false;
        }

        protected virtual bool EvaluateString(string expression, Stack<object> stack, ref int i)
        {
            if (!OptionStringEvaluationActive)
                return false;

            Match stringBeginningMatch = stringBeginningRegex.Match(expression.Substring(i));

            if (stringBeginningMatch.Success)
            {
                bool isEscaped = stringBeginningMatch.Groups["escaped"].Success;
                bool isInterpolated = stringBeginningMatch.Groups["interpolated"].Success;

                i += stringBeginningMatch.Length;

                Regex stringRegexPattern = new Regex($"^[^{(isEscaped ? "" : @"\\")}{(isInterpolated ? "{}" : "")}\"]*");

                bool endOfString = false;

                StringBuilder resultString = new StringBuilder();

                while (!endOfString && i < expression.Length)
                {
                    Match stringMatch = stringRegexPattern.Match(expression.Substring(i, expression.Length - i));

                    resultString.Append(stringMatch.Value);
                    i += stringMatch.Length;

                    if (expression.Substring(i)[0] == '"')
                    {
                        if (expression.Substring(i).Length > 1 && expression.Substring(i)[1] == '"')
                        {
                            i += 2;
                            resultString.Append(@"""");
                        }
                        else
                        {
                            endOfString = true;
                            stack.Push(resultString.ToString());
                        }
                    }
                    else if (expression.Substring(i)[0] == '{')
                    {
                        i++;

                        if (expression.Substring(i)[0] == '{')
                        {
                            resultString.Append("{");
                            i++;
                        }
                        else
                        {
                            StringBuilder innerExp = new StringBuilder();
                            int bracketCount = 1;
                            for (; i < expression.Length; i++)
                            {
                                if (i + 3 <= expression.Length && expression.Substring(i, 3).Equals("'\"'"))
                                {
                                    innerExp.Append("'\"'");
                                    i += 2;
                                }
                                else
                                {
                                    Match internalStringMatch = stringBeginningRegex.Match(expression.Substring(i));

                                    if (internalStringMatch.Success)
                                    {
                                        string innerString = internalStringMatch.Value + GetCodeUntilEndOfString(expression.Substring(i + internalStringMatch.Length), internalStringMatch);
                                        innerExp.Append(innerString);
                                        i += innerString.Length - 1;
                                    }
                                    else
                                    {
                                        string s = expression.Substring(i, 1);

                                        if (s.Equals("{")) bracketCount++;

                                        if (s.Equals("}"))
                                        {
                                            bracketCount--;
                                            i++;
                                            if (bracketCount == 0) break;
                                        }
                                        innerExp.Append(s);
                                    }
                                }
                            }

                            if (bracketCount > 0)
                            {
                                string beVerb = bracketCount == 1 ? "is" : "are";
                                throw new Exception($"{bracketCount} '}}' character {beVerb} missing in expression : [{expression}]");
                            }

                            object obj = Evaluate(innerExp.ToString());

                            if (obj is BubbleExceptionContainer bubbleExceptionContainer)
                                bubbleExceptionContainer.Throw();

                            resultString.Append(obj);
                        }
                    }
                    else if (expression.Substring(i, expression.Length - i)[0] == '}')
                    {
                        i++;

                        if (expression.Substring(i, expression.Length - i)[0] == '}')
                        {
                            resultString.Append("}");
                            i++;
                        }
                        else
                        {
                            throw new ExpressionEvaluatorSyntaxErrorException("A character '}' must be escaped in a interpolated string.");
                        }
                    }
                    else if (expression.Substring(i, expression.Length - i)[0] == '\\')
                    {
                        i++;

                        if (stringEscapedCharDict.TryGetValue(expression.Substring(i, expression.Length - i)[0], out string escapedString))
                        {
                            resultString.Append(escapedString);
                            i++;
                        }
                        else
                        {
                            throw new ExpressionEvaluatorSyntaxErrorException("There is no corresponding escaped character for \\" + expression.Substring(i, 1));
                        }
                    }
                }

                if (!endOfString)
                    throw new ExpressionEvaluatorSyntaxErrorException("A \" character is missing.");

                return true;
            }

            return false;
        }

        #endregion

        #region ProcessStack

        protected virtual object ProcessStack(Stack<object> stack)
        {
            if (stack.Count == 0)
                throw new ExpressionEvaluatorSyntaxErrorException("Empty expression or no token found");

            List<object> list = stack
                .Select(e => e is ValueTypeNestingTrace valueTypeNestingTrace ? valueTypeNestingTrace.Value : e)
                .Select(e => e is SubExpression subExpression ? Evaluate(subExpression.Expression) : e)
                .Select(e => e is NullConditionalNullValue ? null : e)
                .ToList();

            OperatorsEvaluations.ToList().ForEach((IDictionary<ExpressionOperator, Func<dynamic, dynamic, object>> operatorEvalutationsDict) =>
            {
                for (int i = list.Count - 1; i >= 0; i--)
                {
                    for (int opi = 0; opi < operatorEvalutationsDict.Keys.ToList().Count; opi++)
                    {
                        ExpressionOperator eOp = operatorEvalutationsDict.Keys.ToList()[opi];

                        if ((list[i] as ExpressionOperator) == eOp)
                        {
                            if (RightOperandOnlyOperatorsEvaluationDictionary.Contains(eOp))
                            {
                                try
                                {
                                    void EvaluateFirstNextUnaryOp(int j, ref int parentIndex)
                                    {
                                        if (j > 0 && list[j] is ExpressionOperator nextOp && RightOperandOnlyOperatorsEvaluationDictionary.Contains(nextOp))
                                        {
                                            EvaluateFirstNextUnaryOp(j - 1, ref j);

                                            list[j] = OperatorsEvaluations.FirstOrDefault(od => od.ContainsKey(nextOp))?[nextOp](null, (dynamic)list[j - 1]);

                                            list.RemoveAt(j - 1);
                                            parentIndex=j;
                                        }
                                    }

                                    EvaluateFirstNextUnaryOp(i - 1, ref i);

                                    list[i] = operatorEvalutationsDict[eOp](null, (dynamic)list[i - 1]);
                                }
                                catch (Exception ex)
                                {
                                    var right = (dynamic)list[i - 1];
                                    if (right is BubbleExceptionContainer)
                                    {
                                        list[i] = right;//Bubble up the causing error
                                    }
                                    else
                                    {
                                        list[i] = new BubbleExceptionContainer(ex); //Transport the processing error
                                    }
                                }
                                list.RemoveAt(i - 1);
                                break;
                            }
                            else if (LeftOperandOnlyOperatorsEvaluationDictionary.Contains(eOp))
                            {
                                try
                                {
                                    void EvaluateFirstPreviousUnaryOp(int j)
                                    {
                                        if (j < list.Count - 1 && list[j] is ExpressionOperator previousOp && LeftOperandOnlyOperatorsEvaluationDictionary.Contains(previousOp))
                                        {
                                            EvaluateFirstPreviousUnaryOp(j + 1);

                                            list[j] = OperatorsEvaluations.FirstOrDefault(od => od.ContainsKey(previousOp))?[previousOp]((dynamic)list[j + 1], null);

                                            list.RemoveAt(j + 1);
                                        }
                                    }

                                    EvaluateFirstPreviousUnaryOp(i + 1);

                                    list[i] = operatorEvalutationsDict[eOp]((dynamic)list[i + 1], null);
                                }
                                catch (Exception ex)
                                {
                                    var left = (dynamic)list[i + 1];
                                    if (left is BubbleExceptionContainer)
                                    {
                                        list[i] = left; //Bubble up the causing error
                                    }
                                    else
                                    {
                                        list[i] = new BubbleExceptionContainer(ex); //Transport the processing error
                                    }
                                }
                                list.RemoveAt(i + 1);
                                break;
                            }
                            else
                            {
                                var left = (dynamic)list[i + 1];
                                var right = (dynamic)list[i - 1];

                                try
                                {
                                    list[i] = operatorEvalutationsDict[eOp](left, right);

                                    if (left is BubbleExceptionContainer && right is string)
                                    {
                                        list[i] = left; //Bubble up the causing error
                                    }
                                    else if (right is BubbleExceptionContainer && left is string)
                                    {
                                        list[i] = right; //Bubble up the causing error
                                    }
                                }
                                catch (Exception ex)
                                {
                                    if (left is BubbleExceptionContainer)
                                    {
                                        list[i] = left; //Bubble up the causing error
                                    }
                                    else if (right is BubbleExceptionContainer)
                                    {
                                        list[i] = right; //Bubble up the causing error
                                    }
                                    else
                                    {
                                        list[i] = new BubbleExceptionContainer(ex); //Transport the processing error
                                    }
                                }
                                list.RemoveAt(i + 1);
                                list.RemoveAt(i - 1);
                                i--;
                                break;
                            }
                        }
                    }
                }
            });

            stack.Clear();
            for (int i = 0; i < list.Count; i++)
            {
                stack.Push(list[i]);
            }

            if (stack.Count > 1)
            {
                foreach (var item in stack)
                {
                    if (item is BubbleExceptionContainer bubbleExceptionContainer)
                    {
                        bubbleExceptionContainer.Throw(); //Throw the first occuring error
                    }
                }
                throw new ExpressionEvaluatorSyntaxErrorException("Syntax error. Check that no operator is missing");
            }
            else if (evaluationStackCount == 1 && stack.Peek() is BubbleExceptionContainer bubbleExceptionContainer)
            {
                //We reached the top level of the evaluation. So we want to throw the resulting exception.
                bubbleExceptionContainer.Throw();
            }

            return stack.Pop();
        }

        #endregion

        #region Remove comments

        /// <summary>
        /// remove all line and blocks comments of the specified C# script. (Manage in strings comment syntax ignore)
        /// based on https://stackoverflow.com/questions/3524317/regex-to-strip-line-comments-from-c-sharp/3524689#3524689
        /// </summary>
        /// <param name="scriptWithComments">The C# code with comments to remove</param>
        /// <returns>The same C# code without comments</returns>
        public string RemoveComments(string scriptWithComments)
        {
            return removeCommentsRegex.Replace(scriptWithComments,
                match =>
                {
                    if (match.Value.StartsWith("/"))
                    {
                        Match newLineCharsMatch = newLineCharsRegex.Match(match.Value);

                        if (match.Value.StartsWith("/*") && newLineCharsMatch.Success)
                        {
                            return newLineCharsMatch.Value;
                        }
                        else
                        {
                            return " ";
                        }
                    }
                    else
                    {
                        return match.Value;
                    }
                });
        }

        #endregion

        #region Utils methods for parsing and interpretation

        protected delegate bool ParsingMethodDelegate(string expression, Stack<object> stack, ref int i);

        protected delegate dynamic InternalDelegate(params dynamic[] args);

        protected virtual object ManageKindOfAssignation(string expression, ref int index, Match match, Func<object> getCurrentValue, Stack<object> stack = null)
        {
            if (stack?.Count > 1)
                throw new ExpressionEvaluatorSyntaxErrorException("The left part of an assignation must be a variable, a property or an indexer.");

            object result;
            string rightExpression = expression.Substring(index);
            index = expression.Length;

            if (rightExpression.Trim().Equals(string.Empty))
                throw new ExpressionEvaluatorSyntaxErrorException("Right part is missing in assignation");

            if (match.Groups["assignmentPrefix"].Success)
            {
                ExpressionOperator prefixOp = operatorsDictionary[match.Groups["assignmentPrefix"].Value];

                result = OperatorsEvaluations.ToList().Find(dict => dict.ContainsKey(prefixOp))[prefixOp](getCurrentValue(), Evaluate(rightExpression));
            }
            else
            {
                result = Evaluate(rightExpression);
            }

            if (result is BubbleExceptionContainer exceptionContainer)
                exceptionContainer.Throw();

            if (stack != null)
            {
                stack.Clear();
                stack.Push(result);
            }

            return result;
        }

        protected virtual void AssignVariable(string varName, object value)
        {
            if (Variables.ContainsKey(varName) && Variables[varName] is StronglyTypedVariable stronglyTypedVariable)
            {
                if (value == null && stronglyTypedVariable.Type.IsValueType && Nullable.GetUnderlyingType(stronglyTypedVariable.Type) == null)
                {
                    throw new ExpressionEvaluatorSyntaxErrorException($"Can not cast null to {stronglyTypedVariable.Type} because it's not a nullable valueType");
                }

                Type typeToAssign = value?.GetType();
                if (typeToAssign == null || stronglyTypedVariable.Type.IsAssignableFrom(typeToAssign))
                {
                    stronglyTypedVariable.Value = value;
                }
                else
                {
                    try
                    {
                        Variables[varName] = Convert.ChangeType(value, stronglyTypedVariable.Type);
                    }
                    catch(Exception exception)
                    {
                        throw new InvalidCastException($"A object of type {typeToAssign} can not be cast implicitely in {stronglyTypedVariable.Type}", exception);
                    }
                }
            }
            else
            {
                Variables[varName] = value;
            }
        }

        protected virtual object GetDefaultValueOfType(Type type) => type.IsValueType ? Activator.CreateInstance(type) : null;

        protected virtual bool GetLambdaExpression(string expression, Stack<object> stack)
        {
            Match lambdaExpressionMatch = lambdaExpressionRegex.Match(expression);

            if (lambdaExpressionMatch.Success)
            {
                List<string> argsNames = lambdaArgRegex
                    .Matches(lambdaExpressionMatch.Groups["args"].Value)
                    .Cast<Match>().ToList()
                    .ConvertAll(argMatch => argMatch.Value);

                bool inScriptAtDeclaration = inScript;

                stack.Push(new InternalDelegate((object[] args) =>
                {
                    var vars = new Dictionary<string, object>(variables);

                    for (int a = 0; a < argsNames.Count || a < args.Length; a++)
                    {
                        vars[argsNames[a]] = args[a];
                    }

                    var savedVars = variables;
                    Variables = vars;

                    string lambdaBody = lambdaExpressionMatch.Groups["expression"].Value.Trim();

                    object result = null;

                    if ((OptionCanDeclareMultiExpressionsLambdaInSimpleExpressionEvaluate || inScriptAtDeclaration)
                        && lambdaBody.StartsWith("{") && lambdaBody.EndsWith("}"))
                    {
                        result = ScriptEvaluate(lambdaBody.Substring(1, lambdaBody.Length - 2));
                    }
                    else
                    {
                        result = Evaluate(lambdaExpressionMatch.Groups["expression"].Value);
                    }

                    variables = savedVars;

                    return result;
                }));

                return true;
            }
            else
            {
                return false;
            }
        }

        protected virtual MethodInfo GetRealMethod(ref Type type, ref object obj, string func, BindingFlags flag, List<object> args, string genericsTypes, Type[] inferedGenericsTypes, List<ArgKeywordsEncaps> argsWithKeywords, bool testForExtention = false)
        {
            MethodInfo methodInfo = null;
            List<object> modifiedArgs = new List<object>(args);
            Type typeCopy = type;

            if (OptionFluidPrefixingActive
                && (func.StartsWith("Fluid", StringComparisonForCasing)
                    || func.StartsWith("Fluent", StringComparisonForCasing)))
            {
                methodInfo = GetRealMethod(ref type, ref obj, func.Substring(func.StartsWith("Fluid", StringComparisonForCasing) ? 5 : 6), flag, modifiedArgs, genericsTypes, inferedGenericsTypes, argsWithKeywords, testForExtention);
                if (methodInfo != null)
                {
                    if (methodInfo.ReturnType == typeof(void))
                    {
                        obj = new DelegateEncaps(obj, methodInfo);

                        methodInfo = typeof(DelegateEncaps).GetMethod("CallFluidMethod");

                        args.Clear();
                        args.Add(modifiedArgs.ToArray());
                    }

                    return methodInfo;
                }
            }

            bool parameterValidate(ParameterInfo p) => p.Position >= modifiedArgs.Count
                || (testForExtention && p.Position == 0)
                || modifiedArgs[p.Position] == null
                || IsCastable(modifiedArgs[p.Position].GetType(), p.ParameterType)
                || typeof(Delegate).IsAssignableFrom(p.ParameterType)
                || p.IsDefined(typeof(ParamArrayAttribute))
                || (p.ParameterType.IsByRef && argsWithKeywords.Any(a => a.Index == p.Position + (testForExtention ? 1 : 0)));

            bool methodByNameFilter(MethodInfo m) => m.Name.Equals(func, StringComparisonForCasing)
                        && (m.GetParameters().Length == modifiedArgs.Count
                            || (m.GetParameters().Length > modifiedArgs.Count && m.GetParameters().Take(modifiedArgs.Count).All(p => modifiedArgs[p.Position] == null || IsCastable(modifiedArgs[p.Position].GetType(), p.ParameterType)) && m.GetParameters().Skip(modifiedArgs.Count).All(p => p.HasDefaultValue))
                            || (m.GetParameters().Length > 0 && m.GetParameters().Last().IsDefined(typeof(ParamArrayAttribute), false)
                                && m.GetParameters().All(parameterValidate)));

            List<MethodInfo> methodInfos = type.GetMethods(flag)
                .Where(methodByNameFilter)
                .OrderByDescending(m => m.GetParameters().Length)
                .ToList();

            // For Linq methods that are overloaded and implement possibly lambda arguments
            try
            {
                if (methodInfos.Count > 1
                    && type == typeof(Enumerable)
                    && args.Count == 2
                    && args[1] is InternalDelegate internalDelegate
                    && args[0] is IEnumerable enumerable
                    && enumerable.GetEnumerator() is IEnumerator enumerator
                    && enumerator.MoveNext()
                    && methodInfos.Any(m => m.GetParameters().Any(p => p.ParameterType.Name.StartsWith("Func"))))
                {
                    Type lambdaResultType = internalDelegate.Invoke(enumerator.Current).GetType();

                    methodInfo = methodInfos.Find(m =>
                    {
                        ParameterInfo[] parameterInfos = m.GetParameters();

                        return parameterInfos.Length == 2
                            && parameterInfos[1].ParameterType.Name.StartsWith("Func")
                            && parameterInfos[1].ParameterType.GenericTypeArguments is Type[] genericTypesArgs
                            && genericTypesArgs.Length == 2
                            && genericTypesArgs[1] == lambdaResultType;
                    });

                    if (methodInfo != null)
                    {
                        methodInfo = TryToCastMethodParametersToMakeItCallable(methodInfo, modifiedArgs, genericsTypes, inferedGenericsTypes, obj);
                    }
                }
            }
            catch { }

            for (int m = 0; methodInfo == null && m < methodInfos.Count; m++)
            {
                modifiedArgs = new List<object>(args);

                methodInfo = TryToCastMethodParametersToMakeItCallable(methodInfos[m], modifiedArgs, genericsTypes, inferedGenericsTypes, obj);
            }

            if (methodInfo != null)
            {
                args.Clear();
                args.AddRange(modifiedArgs);
            }

            return methodInfo;
        }

        protected virtual bool IsCastable(Type fromType, Type toType)
        {
            return toType.IsAssignableFrom(fromType)
                || (implicitCastDict.ContainsKey(fromType) && implicitCastDict[fromType].Contains(toType));
        }

        protected virtual MethodInfo TryToCastMethodParametersToMakeItCallable(MethodInfo methodInfoToCast, List<object> modifiedArgs, string genericsTypes, Type[] inferedGenericsTypes, object onInstance = null)
        {
            MethodInfo methodInfo = null;

            MethodInfo oldMethodInfo = methodInfoToCast;

            if (!string.IsNullOrEmpty(genericsTypes))
            {
                methodInfoToCast = MakeConcreteMethodIfGeneric(methodInfoToCast, genericsTypes, inferedGenericsTypes);
            }
            else if (oldMethodInfo.IsGenericMethod
                && oldMethodInfo.ContainsGenericParameters)
            {
                Type[] genericArgsTypes = oldMethodInfo.GetGenericArguments();
                List<Type> inferedTypes = new List<Type>();

                for (int t = 0; t<genericArgsTypes.Length; t++)
                {
                    if (genericArgsTypes[t].IsGenericParameter)
                    {
                        string name = genericArgsTypes[t].Name;
                        ParameterInfo[] parameterInfos = oldMethodInfo.GetParameters();

                        ParameterInfo paramsForInference = Array.Find(parameterInfos, p => p.ParameterType.IsGenericParameter
                                && p.ParameterType.Name.Equals(name)
                                && modifiedArgs.Count > p.Position
                                && !modifiedArgs[p.Position].GetType().IsGenericParameter);

                        if (paramsForInference != null)
                        {
                            inferedTypes.Add(modifiedArgs[paramsForInference.Position].GetType());
                        }
                        else
                        {
                            paramsForInference = Array.Find(parameterInfos, p => p.ParameterType.IsGenericType
                                && p.ParameterType.ContainsGenericParameters
                                && p.ParameterType.GetGenericArguments().Any(subP => subP.Name.Equals(name))
                                && modifiedArgs.Count > p.Position
                                && !modifiedArgs[p.Position].GetType().IsGenericType);

                            if (paramsForInference != null)
                            {
                                if (modifiedArgs[paramsForInference.Position] is MethodsGroupEncaps methodsGroupEncaps)
                                {
                                    if(paramsForInference.ParameterType.Name.StartsWith("Converter"))
                                    {
                                        Type specificType = Array.Find(paramsForInference.ParameterType.GetGenericArguments(), pType => pType.Name.Equals(name));
                                        MethodInfo paraMethodInfo = Array.Find(methodsGroupEncaps.MethodsGroup, mi => mi.GetParameters().Length == 1);
                                        if(specificType?.GenericParameterPosition == 0)
                                        {
                                            inferedTypes.Add(paraMethodInfo.GetParameters()[0].ParameterType);
                                        }
                                        else if(specificType?.GenericParameterPosition == 1)
                                        {
                                            inferedTypes.Add(paraMethodInfo.ReturnType);
                                        }
                                    }
                                    else if(paramsForInference.ParameterType.Name.StartsWith("Action"))
                                    {
                                        Type specificType = Array.Find(paramsForInference.ParameterType.GetGenericArguments(), pType => pType.Name.Equals(name));
                                        MethodInfo paraMethodInfo = Array.Find(methodsGroupEncaps.MethodsGroup, mi => mi.GetParameters().Length == paramsForInference.ParameterType.GetGenericArguments().Length);
                                        if(specificType != null)
                                        {
                                            inferedTypes.Add(paraMethodInfo.GetParameters()[specificType.GenericParameterPosition].ParameterType);
                                        }
                                    }
                                    else if(paramsForInference.ParameterType.Name.StartsWith("Func"))
                                    {
                                        Type specificType = Array.Find(paramsForInference.ParameterType.GetGenericArguments(), pType => pType.Name.Equals(name));
                                        MethodInfo paraMethodInfo = Array.Find(methodsGroupEncaps.MethodsGroup, mi => mi.GetParameters().Length == paramsForInference.ParameterType.GetGenericArguments().Length - 1);
                                        if(specificType?.GenericParameterPosition == paraMethodInfo.GetParameters().Length)
                                        {
                                            inferedTypes.Add(paraMethodInfo.ReturnType);
                                        }
                                        else
                                        {
                                            inferedTypes.Add(paraMethodInfo.GetParameters()[specificType.GenericParameterPosition].ParameterType);
                                        }
                                    }
                                }
                                else if (modifiedArgs[paramsForInference.Position].GetType().HasElementType)
                                {
                                    inferedTypes.Add(modifiedArgs[paramsForInference.Position].GetType().GetElementType());
                                }
                            }
                        }
                    }
                    else
                    {
                        inferedTypes.Add(genericArgsTypes[t]);
                    }
                }

                if(inferedTypes.Count > 0 && inferedTypes.Count == genericArgsTypes.Length)
                    methodInfoToCast = MakeConcreteMethodIfGeneric(oldMethodInfo, genericsTypes, inferedTypes.ToArray());
                else
                    methodInfoToCast = MakeConcreteMethodIfGeneric(methodInfoToCast, genericsTypes, inferedGenericsTypes);
            }

            bool parametersCastOK = true;

            // To manage empty params argument
            if ((methodInfoToCast.GetParameters().LastOrDefault()?.IsDefined(typeof(ParamArrayAttribute), false) ?? false)
                && methodInfoToCast.GetParameters().Length == modifiedArgs.Count + 1)
            {
                modifiedArgs.Add(Activator.CreateInstance(methodInfoToCast.GetParameters().Last().ParameterType, new object[] { 0 }));
            }
            else if (methodInfoToCast.GetParameters().Length > modifiedArgs.Count)
            {
                modifiedArgs.AddRange(methodInfoToCast.GetParameters().Skip(modifiedArgs.Count).Select(p => p.DefaultValue));
            }

            for (int a = 0; a < modifiedArgs.Count && parametersCastOK; a++)
            {
                Type parameterType = methodInfoToCast.GetParameters()[a].ParameterType;
                string paramTypeName = parameterType.Name;

                if (modifiedArgs[a] is InternalDelegate internalDelegate)
                {
                    if (paramTypeName.StartsWith("Predicate"))
                    {
                        DelegateEncaps de = new DelegateEncaps(internalDelegate);
                        MethodInfo encapsMethod = de.GetType()
                            .GetMethod("Predicate")
                            .MakeGenericMethod(parameterType.GetGenericArguments());
                        modifiedArgs[a] = Delegate.CreateDelegate(parameterType, de, encapsMethod);
                    }
                    else if (paramTypeName.StartsWith("Func"))
                    {
                        DelegateEncaps de = new DelegateEncaps(internalDelegate);
                        MethodInfo encapsMethod = de.GetType()
                            .GetMethod($"Func{parameterType.GetGenericArguments().Length - 1}")
                            .MakeGenericMethod(parameterType.GetGenericArguments());
                        modifiedArgs[a] = Delegate.CreateDelegate(parameterType, de, encapsMethod);
                    }
                    else if (paramTypeName.StartsWith("Action"))
                    {
                        DelegateEncaps de = new DelegateEncaps(internalDelegate);
                        MethodInfo encapsMethod = de.GetType()
                            .GetMethod($"Action{parameterType.GetGenericArguments().Length}")
                            .MakeGenericMethod(parameterType.GetGenericArguments());
                        modifiedArgs[a] = Delegate.CreateDelegate(parameterType, de, encapsMethod);
                    }
                    else if (paramTypeName.StartsWith("Converter"))
                    {
                        DelegateEncaps de = new DelegateEncaps(internalDelegate);
                        MethodInfo encapsMethod = de.GetType()
                            .GetMethod("Func1")
                            .MakeGenericMethod(parameterType.GetGenericArguments());
                        modifiedArgs[a] = Delegate.CreateDelegate(parameterType, de, encapsMethod);
                    }
                }
                else if (typeof(Delegate).IsAssignableFrom(parameterType)
                    && modifiedArgs[a] is MethodsGroupEncaps methodsGroupEncaps)
                {
                    MethodInfo invokeMethod = parameterType.GetMethod("Invoke");
                    MethodInfo methodForDelegate = Array.Find(methodsGroupEncaps.MethodsGroup, m => invokeMethod.GetParameters().Length == m.GetParameters().Length && invokeMethod.ReturnType.IsAssignableFrom(m.ReturnType));
                    if (methodForDelegate != null)
                    {
                        Type[] parametersTypes = methodForDelegate.GetParameters().Select(p => p.ParameterType).ToArray();
                        Type delegateType;

                        if (methodForDelegate.ReturnType == typeof(void))
                        {
                            delegateType = Type.GetType($"System.Action`{parametersTypes.Length}");
                        }
                        else if (paramTypeName.StartsWith("Predicate"))
                        {
                            delegateType = typeof(Predicate<>);
                            methodInfoToCast = MakeConcreteMethodIfGeneric(oldMethodInfo, genericsTypes, parametersTypes);
                        }
                        else if (paramTypeName.StartsWith("Converter"))
                        {
                            delegateType = typeof(Converter<,>);
                            parametersTypes = parametersTypes.Concat(new Type[] { methodForDelegate.ReturnType }).ToArray();
                        }
                        else
                        {
                            delegateType = Type.GetType($"System.Func`{parametersTypes.Length + 1}");
                            parametersTypes = parametersTypes.Concat(new Type[] { methodForDelegate.ReturnType }).ToArray();
                        }

                        delegateType = delegateType.MakeGenericType(parametersTypes);

                        modifiedArgs[a] = Delegate.CreateDelegate(delegateType, methodsGroupEncaps.ContainerObject, methodForDelegate);

                        if (oldMethodInfo.IsGenericMethod &&
                            methodInfoToCast.GetGenericArguments().Length == parametersTypes.Length &&
                            !methodInfoToCast.GetGenericArguments().SequenceEqual(parametersTypes) &&
                            string.IsNullOrWhiteSpace(genericsTypes))
                        {
                            methodInfoToCast = MakeConcreteMethodIfGeneric(oldMethodInfo, genericsTypes, parametersTypes);
                        }
                    }
                }
                else
                {
                    try
                    {
                        // To manage params argument
                        if (methodInfoToCast.GetParameters().Length == a + 1
                            && methodInfoToCast.GetParameters()[a].IsDefined(typeof(ParamArrayAttribute), false)
                            && parameterType != modifiedArgs[a]?.GetType()
                            && parameterType.GetElementType() is Type elementType
                            && modifiedArgs.Skip(a).All(arg => arg == null || elementType.IsAssignableFrom(arg.GetType())))
                        {
                            int numberOfElements = modifiedArgs.Count - a;
                            var paramsArray = Array.CreateInstance(elementType, numberOfElements);
                            modifiedArgs.Skip(a).ToArray().CopyTo(paramsArray, 0);
                            modifiedArgs.RemoveRange(a, numberOfElements);
                            modifiedArgs.Add(paramsArray);
                        }
                        else if (modifiedArgs[a] != null && !parameterType.IsAssignableFrom(modifiedArgs[a].GetType()))
                        {
                            if (parameterType.IsByRef)
                            {
                                if (!parameterType.GetElementType().IsAssignableFrom(modifiedArgs[a].GetType()))
                                    modifiedArgs[a] = Convert.ChangeType(modifiedArgs[a], parameterType.GetElementType());
                            }
                            else if (modifiedArgs[a].GetType().IsArray
                                && typeof(IEnumerable).IsAssignableFrom(parameterType)
                                && oldMethodInfo.IsGenericMethod
                                && string.IsNullOrWhiteSpace(genericsTypes)
                                && methodInfoToCast.GetGenericArguments().Length == 1
                                && !methodInfoToCast.GetGenericArguments()[0].Equals(modifiedArgs[a].GetType().GetElementType()))
                            {
                                methodInfoToCast = MakeConcreteMethodIfGeneric(oldMethodInfo, genericsTypes, new Type[] { modifiedArgs[a].GetType().GetElementType() });
                            }
                            else
                            {
                                if (parameterType.IsArray && modifiedArgs[a] is Array sourceArray)
                                {
                                    modifiedArgs[a] = typeof(Enumerable)
                                        .GetMethod("Cast")
                                        .MakeGenericMethod(parameterType.GetElementType())
                                        .Invoke(null, new object[] { modifiedArgs[a] });

                                    modifiedArgs[a] = typeof(Enumerable)
                                        .GetMethod("ToArray")
                                        .MakeGenericMethod(parameterType.GetElementType())
                                        .Invoke(null, new object[] { modifiedArgs[a] });
                                }
                                else if (IsCastable(modifiedArgs[a].GetType(), parameterType))
                                {
                                    modifiedArgs[a] = Convert.ChangeType(modifiedArgs[a], parameterType);
                                }
                                else
                                {
                                    var converter = parameterType.GetMethod("op_Implicit", new[] { modifiedArgs[a].GetType() });
                                    if(converter != null)
                                    {
                                        modifiedArgs[a] = converter.Invoke(null, new[] { modifiedArgs[a] });
                                    }
                                    else
                                    {
                                        parametersCastOK = false;
                                    }
                                }
                            }
                        }
                    }
                    catch
                    {
                        parametersCastOK = false;
                    }

                    if (!parametersCastOK)
                    {
                        try
                        {
                            ParameterCastEvaluationEventArg parameterCastEvaluationEventArg = new ParameterCastEvaluationEventArg(methodInfoToCast, parameterType, modifiedArgs[a], a, this, onInstance);

                            EvaluateParameterCast?.Invoke(this, parameterCastEvaluationEventArg);

                            if (parameterCastEvaluationEventArg.FunctionModifiedArgument)
                            {
                                modifiedArgs[a] = parameterCastEvaluationEventArg.Argument;
                                parametersCastOK = true;
                            }
                        }
                        catch {}
                    }
                }
            }

            if (parametersCastOK)
                methodInfo = methodInfoToCast;

            return methodInfo;
        }

        protected virtual MethodInfo MakeConcreteMethodIfGeneric(MethodInfo methodInfo, string genericsTypes, Type[] inferedGenericsTypes)
        {
            if (methodInfo.IsGenericMethod)
            {
                if (genericsTypes.Equals(string.Empty))
                {
                    if (inferedGenericsTypes != null && inferedGenericsTypes.Length == methodInfo.GetGenericArguments().Length)
                    {
                        return methodInfo.MakeGenericMethod(inferedGenericsTypes);
                    }
                    else
                    {
                        return methodInfo.MakeGenericMethod(Enumerable.Repeat(typeof(object), methodInfo.GetGenericArguments().Length).ToArray());
                    }
                }
                else
                {
                    return methodInfo.MakeGenericMethod(GetConcreteTypes(genericsTypes));
                }
            }

            return methodInfo;
        }

        protected virtual Type[] GetConcreteTypes(string genericsTypes)
        {
            return genericsDecodeRegex
                .Matches(genericsEndOnlyOneTrim.Replace(genericsTypes.TrimStart(' ', '<'), ""))
                .Cast<Match>()
                .Select(match => GetTypeByFriendlyName(match.Groups["name"].Value, match.Groups["isgeneric"].Value, true))
                .ToArray();
        }

        protected virtual BindingFlags DetermineInstanceOrStatic(ref Type objType, ref object obj, ref ValueTypeNestingTrace valueTypeNestingTrace)
        {
            valueTypeNestingTrace = obj as ValueTypeNestingTrace;

            if (valueTypeNestingTrace != null)
            {
                obj = valueTypeNestingTrace.Value;
            }

            if (obj is ClassOrEnumType classOrTypeName)
            {
                objType = classOrTypeName.Type;
                obj = null;
                return StaticBindingFlag;
            }
            else
            {
                objType = obj.GetType();
                return InstanceBindingFlag;
            }
        }

        protected virtual string GetScriptBetweenCurlyBrackets(string parentScript, ref int index)
        {
            string s;
            string currentScript = string.Empty;
            int bracketCount = 1;
            for (; index < parentScript.Length; index++)
            {
                Match internalStringMatch = stringBeginningRegex.Match(parentScript.Substring(index));
                Match internalCharMatch = internalCharRegex.Match(parentScript.Substring(index));

                if (internalStringMatch.Success)
                {
                    string innerString = internalStringMatch.Value + GetCodeUntilEndOfString(parentScript.Substring(index + internalStringMatch.Length), internalStringMatch);
                    currentScript += innerString;
                    index += innerString.Length - 1;
                }
                else if (internalCharMatch.Success)
                {
                    currentScript += internalCharMatch.Value;
                    index += internalCharMatch.Length - 1;
                }
                else
                {
                    s = parentScript.Substring(index, 1);

                    if (s.Equals("{")) bracketCount++;

                    if (s.Equals("}"))
                    {
                        bracketCount--;
                        if (bracketCount == 0)
                            break;
                    }

                    currentScript += s;
                }
            }

            if (bracketCount > 0)
            {
                string beVerb = bracketCount == 1 ? "is" : "are";
                throw new Exception($"{bracketCount} '" + "}" + $"' character {beVerb} missing in script at : [{index}]");
            }

            return currentScript;
        }

        protected virtual List<string> GetExpressionsBetweenParenthesesOrOtherImbricableBrackets(string expression, ref int i, bool checkSeparator, string separator = ",", string startChar = "(", string endChar = ")")
        {
            List<string> expressionsList = new List<string>();

            string s;
            string currentExpression = string.Empty;
            int bracketCount = 1;
            for (; i < expression.Length; i++)
            {
                string subExpr = expression.Substring(i);
                Match internalStringMatch = stringBeginningRegex.Match(subExpr);
                Match internalCharMatch = internalCharRegex.Match(subExpr);

                if (OptionStringEvaluationActive && internalStringMatch.Success)
                {
                    string innerString = internalStringMatch.Value + GetCodeUntilEndOfString(expression.Substring(i + internalStringMatch.Length), internalStringMatch);
                    currentExpression += innerString;
                    i += innerString.Length - 1;
                }
                else if (OptionCharEvaluationActive && internalCharMatch.Success)
                {
                    currentExpression += internalCharMatch.Value;
                    i += internalCharMatch.Length - 1;
                }
                else
                {
                    s = expression.Substring(i, 1);

                    if (s.Equals(startChar))
                    {
                        bracketCount++;
                    }
                    else if (s.Equals("("))
                    {
                        i++;
                        currentExpression += "(" + GetExpressionsBetweenParenthesesOrOtherImbricableBrackets(expression, ref i, false, ",", "(", ")").SingleOrDefault() + ")";
                        continue;
                    }
                    else if (s.Equals("{"))
                    {
                        i++;
                        currentExpression += "{" + GetExpressionsBetweenParenthesesOrOtherImbricableBrackets(expression, ref i, false, ",", "{", "}").SingleOrDefault() + "}";
                        continue;
                    }
                    else if (s.Equals("["))
                    {
                        i++;
                        currentExpression += "[" + GetExpressionsBetweenParenthesesOrOtherImbricableBrackets(expression, ref i, false, ",", "[", "]").SingleOrDefault() + "]";
                        continue;
                    }

                    if (s.Equals(endChar))
                    {
                        bracketCount--;
                        if (bracketCount == 0)
                        {
                            if (!currentExpression.Trim().Equals(string.Empty))
                                expressionsList.Add(currentExpression);
                            break;
                        }
                    }

                    if (checkSeparator && s.Equals(separator) && bracketCount == 1)
                    {
                        expressionsList.Add(currentExpression);
                        currentExpression = string.Empty;
                    }
                    else
                    {
                        currentExpression += s;
                    }
                }
            }

            if (bracketCount > 0)
            {
                string beVerb = bracketCount == 1 ? "is" : "are";
                throw new Exception($"{bracketCount} '{endChar}' character {beVerb} missing in expression : [{expression}]");
            }

            return expressionsList;
        }

        protected virtual bool DefaultFunctions(string name, List<string> args, out object result)
        {
            bool functionExists = true;

            if (simpleDoubleMathFuncsDictionary.TryGetValue(name, out Func<double, double> func))
            {
                result = func(Convert.ToDouble(Evaluate(args[0])));
            }
            else if (doubleDoubleMathFuncsDictionary.TryGetValue(name, out Func<double, double, double> func2))
            {
                result = func2(Convert.ToDouble(Evaluate(args[0])), Convert.ToDouble(Evaluate(args[1])));
            }
            else if (complexStandardFuncsDictionary.TryGetValue(name, out Func<ExpressionEvaluator, List<string>, object> complexFunc))
            {
                result = complexFunc(this, args);
            }
            else if (OptionEvaluateFunctionActive && name.Equals("Evaluate", StringComparisonForCasing))
            {
                result = Evaluate((string)Evaluate(args[0]));
            }
            else if (OptionScriptEvaluateFunctionActive && name.Equals("ScriptEvaluate", StringComparisonForCasing))
            {
                bool oldInScript = inScript;
                result = ScriptEvaluate((string)Evaluate(args[0]));
                inScript = oldInScript;
            }
            else
            {
                result = null;
                functionExists = false;
            }

            return functionExists;
        }

        protected virtual Type GetTypeByFriendlyName(string typeName, string genericTypes = "", bool throwExceptionIfNotFound = false)
        {
            Type result = null;
            string formatedGenericTypes = string.Empty;
            bool isCached = false;
            try
            {
                typeName = typeName.Replace(" ", "").Replace("\t", "").Replace("\r", "").Replace("\n", "");
                genericTypes = genericTypes.Replace(" ", "").Replace("\t", "").Replace("\r", "").Replace("\n", "");

                if (primaryTypesDict.ContainsKey(OptionCaseSensitiveEvaluationActive ? typeName : typeName.ToLower()))
                {
                    result = primaryTypesDict[OptionCaseSensitiveEvaluationActive ? typeName : typeName.ToLower()];
                }

                if (CacheTypesResolutions && (TypesResolutionCaching?.ContainsKey(typeName + genericTypes) ?? false))
                {
                    result = TypesResolutionCaching[typeName + genericTypes];
                    isCached = true;
                }

                if (result == null)
                {
                    if (!genericTypes.Equals(string.Empty))
                    {
                        Type[] types = GetConcreteTypes(genericTypes);
                        formatedGenericTypes = $"`{types.Length}[{ string.Join(", ", types.Select(type => "[" + type.AssemblyQualifiedName + "]"))}]";
                    }

                    result = Type.GetType(typeName + formatedGenericTypes, false, !OptionCaseSensitiveEvaluationActive);
                }

                if (result == null)
                {
                    result = Types.ToList().Find(type => type.Name.Equals(typeName, StringComparisonForCasing) || type.FullName.StartsWith(typeName + ","));
                }

                for (int a = 0; a < Assemblies.Count && result == null; a++)
                {
                    if (typeName.Contains("."))
                    {
                        result = Type.GetType($"{typeName}{formatedGenericTypes},{Assemblies[a].FullName}", false, !OptionCaseSensitiveEvaluationActive);
                    }
                    else
                    {
                        for (int i = 0; i < Namespaces.Count && result == null; i++)
                        {
                            result = Type.GetType($"{Namespaces[i]}.{typeName}{formatedGenericTypes},{Assemblies[a].FullName}", false, !OptionCaseSensitiveEvaluationActive);
                        }
                    }
                }
            }
            catch (ExpressionEvaluatorSyntaxErrorException exception)
            {
                ExceptionDispatchInfo.Capture(exception).Throw();
                // Will not go here but need to return something to avoid compilation errors.
                return null;
            }
            catch { }

            if (result != null && TypesToBlock.Contains(result))
                result = null;

            if (result == null && throwExceptionIfNotFound)
                throw new ExpressionEvaluatorSyntaxErrorException($"Type or class {typeName}{genericTypes} is unknown");

            if (CacheTypesResolutions && (result != null) && !isCached)
                TypesResolutionCaching[typeName + genericTypes] = result;

            return result;
        }

        protected static object ChangeType(object value, Type conversionType)
        {
            if (conversionType == null)
            {
                throw new ArgumentNullException(nameof(conversionType));
            }
            if (conversionType.IsGenericType && conversionType.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
            {
                if (value == null)
                {
                    return null;
                }
                NullableConverter nullableConverter = new NullableConverter(conversionType);
                conversionType = nullableConverter.UnderlyingType;
            }
            if(conversionType.IsEnum)
            {
                return Enum.ToObject(conversionType, value);
            }

            if (value.GetType().IsPrimitive && conversionType.IsPrimitive)
            {
                return primitiveExplicitCastMethodInfo
                    .MakeGenericMethod(conversionType)
                    .Invoke(null, new object[] {value});
            }

            if (DynamicCast(value, conversionType, out object ret))
            {
                return ret;
            }

            return Convert.ChangeType(value, conversionType);
        }

        protected static MethodInfo primitiveExplicitCastMethodInfo = typeof(ExpressionEvaluator).GetMethod(nameof(PrimitiveExplicitCast), BindingFlags.Static | BindingFlags.NonPublic);

        protected static object PrimitiveExplicitCast<T>(dynamic value)
        {
            return (T)value;
        }

        protected static bool DynamicCast(object source, Type destType, out object result)
        {
            Type srcType = source.GetType();
            if (srcType == destType) { result = source; return true; }
            result = null;

            const BindingFlags bindingFlags = BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy;
            MethodInfo castOperator = destType.GetMethods(bindingFlags)
                                        .Union(srcType.GetMethods(bindingFlags))
                                        .Where(methodInfo => methodInfo.Name == "op_Explicit" || methodInfo.Name == "op_Implicit")
                                        .Where(methodInfo =>
                                        {
                                            var pars = methodInfo.GetParameters();
                                            return pars.Length == 1 && pars[0].ParameterType == srcType;
                                        })
                                        .FirstOrDefault(mi => mi.ReturnType == destType);

            if (castOperator != null)
                result = castOperator.Invoke(null, new object[] { source });
            else
                return false;

            return true;
        }

        protected virtual string GetCodeUntilEndOfString(string subExpr, Match stringBeginningMatch)
        {
            StringBuilder stringBuilder = new StringBuilder();

            GetCodeUntilEndOfString(subExpr, stringBeginningMatch, ref stringBuilder);

            return stringBuilder.ToString();
        }

        protected virtual void GetCodeUntilEndOfString(string subExpr, Match stringBeginningMatch, ref StringBuilder stringBuilder)
        {
            Match codeUntilEndOfStringMatch = stringBeginningMatch.Value.Contains("$") ?
                (stringBeginningMatch.Value.Contains("@") ? endOfStringWithDollarWithAt.Match(subExpr) : endOfStringWithDollar.Match(subExpr)) :
                (stringBeginningMatch.Value.Contains("@") ? endOfStringWithoutDollarWithAt.Match(subExpr) : endOfStringWithoutDollar.Match(subExpr));

            if (codeUntilEndOfStringMatch.Success)
            {
                if (codeUntilEndOfStringMatch.Value.EndsWith("\""))
                {
                    stringBuilder.Append(codeUntilEndOfStringMatch.Value);
                }
                else if (codeUntilEndOfStringMatch.Value.EndsWith("{") && codeUntilEndOfStringMatch.Length < subExpr.Length)
                {
                    if (subExpr[codeUntilEndOfStringMatch.Length] == '{')
                    {
                        stringBuilder.Append(codeUntilEndOfStringMatch.Value);
                        stringBuilder.Append("{");
                        GetCodeUntilEndOfString(subExpr.Substring(codeUntilEndOfStringMatch.Length + 1), stringBeginningMatch, ref stringBuilder);
                    }
                    else
                    {
                        string interpolation = GetCodeUntilEndOfStringInterpolation(subExpr.Substring(codeUntilEndOfStringMatch.Length));
                        stringBuilder.Append(codeUntilEndOfStringMatch.Value);
                        stringBuilder.Append(interpolation);
                        GetCodeUntilEndOfString(subExpr.Substring(codeUntilEndOfStringMatch.Length + interpolation.Length), stringBeginningMatch, ref stringBuilder);
                    }
                }
                else
                {
                    stringBuilder.Append(subExpr);
                }
            }
            else
            {
                stringBuilder.Append(subExpr);
            }
        }

        protected virtual string GetCodeUntilEndOfStringInterpolation(string subExpr)
        {
            Match endOfStringInterpolationMatch = endOfStringInterpolationRegex.Match(subExpr);
            string result = subExpr;

            if (endOfStringInterpolationMatch.Success)
            {
                if (endOfStringInterpolationMatch.Value.EndsWith("}"))
                {
                    result = endOfStringInterpolationMatch.Value;
                }
                else
                {
                    Match stringBeginningForEndBlockMatch = stringBeginningForEndBlockRegex.Match(endOfStringInterpolationMatch.Value);

                    string subString = GetCodeUntilEndOfString(subExpr.Substring(endOfStringInterpolationMatch.Length), stringBeginningForEndBlockMatch);

                    result = endOfStringInterpolationMatch.Value + subString
                        + GetCodeUntilEndOfStringInterpolation(subExpr.Substring(endOfStringInterpolationMatch.Length + subString.Length));
                }
            }

            return result;
        }

        #endregion

        #region Utils protected sub classes for parsing and interpretation

        protected class ValueTypeNestingTrace
        {
            public object Container { get; set; }

            public MemberInfo Member { get; set; }

            public object Value { get; set; }

            public void AssignValue()
            {
                if (Container is ValueTypeNestingTrace valueTypeNestingTrace)
                {
                    ((dynamic)Member).SetValue(valueTypeNestingTrace.Value, Value);
                    valueTypeNestingTrace.AssignValue();
                }
                else
                {
                    ((dynamic)Member).SetValue(Container, Value);
                }
            }
        }

        protected class NullConditionalNullValue
        { }

        protected class ArgKeywordsEncaps
        {
            public int Index { get; set; }
            public string Keyword { get; set; }
            public string VariableName { get; set; }
        }

        protected class DelegateEncaps
        {
            private readonly InternalDelegate lambda;

            private readonly MethodInfo methodInfo;
            private readonly object target;

            public DelegateEncaps(InternalDelegate lambda)
            {
                this.lambda = lambda;
            }

            public DelegateEncaps(object target, MethodInfo methodInfo)
            {
                this.target = target;
                this.methodInfo = methodInfo;
            }

            public object CallFluidMethod(params object[] args)
            {
                methodInfo.Invoke(target, args);
                return target;
            }

            public void Action0()
            {
                lambda();
            }

            public bool Predicate<T1>(T1 arg1)
            {
                return (bool)lambda(arg1);
            }

            public void Action1<T1>(T1 arg1)
            {
                lambda(arg1);
            }

            public void Action2<T1, T2>(T1 arg1, T2 arg2)
            {
                lambda(arg1, arg2);
            }

            public void Action3<T1, T2, T3>(T1 arg1, T2 arg2, T3 arg3)
            {
                lambda(arg1, arg2, arg3);
            }

            public void Action4<T1, T2, T3, T4>(T1 arg1, T2 arg2, T3 arg3, T4 arg4)
            {
                lambda(arg1, arg2, arg3, arg4);
            }

            public void Action5<T1, T2, T3, T4, T5>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
            {
                lambda(arg1, arg2, arg3, arg4, arg5);
            }

            public void Action6<T1, T2, T3, T4, T5, T6>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
            {
                lambda(arg1, arg2, arg3, arg4, arg5, arg6);
            }

            public void Action7<T1, T2, T3, T4, T5, T6, T7>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7)
            {
                lambda(arg1, arg2, arg3, arg4, arg5, arg6, arg7);
            }

            public void Action8<T1, T2, T3, T4, T5, T6, T7, T8>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8)
            {
                lambda(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
            }

            public void Action9<T1, T2, T3, T4, T5, T6, T7, T8, T9>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9)
            {
                lambda(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9);
            }

            public void Action10<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10)
            {
                lambda(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10);
            }

            public void Action11<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11)
            {
                lambda(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11);
            }

            public void Action12<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12)
            {
                lambda(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12);
            }

            public void Action13<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13)
            {
                lambda(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13);
            }

            public void Action14<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14)
            {
                lambda(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14);
            }

            public void Action15<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15)
            {
                lambda(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15);
            }

            public void Action16<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15, T16 arg16)
            {
                lambda(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15, arg16);
            }

            public TResult Func0<TResult>()
            {
                return (TResult)lambda();
            }

            public TResult Func1<T, TResult>(T arg)
            {
                return (TResult)lambda(arg);
            }

            public TResult Func2<T1, T2, TResult>(T1 arg1, T2 arg2)
            {
                return (TResult)lambda(arg1, arg2);
            }

            public TResult Func3<T1, T2, T3, TResult>(T1 arg1, T2 arg2, T3 arg3)
            {
                return (TResult)lambda(arg1, arg2, arg3);
            }

            public TResult Func4<T1, T2, T3, T4, TResult>(T1 arg1, T2 arg2, T3 arg3, T4 arg4)
            {
                return (TResult)lambda(arg1, arg2, arg3, arg4);
            }

            public TResult Func5<T1, T2, T3, T4, T5, TResult>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
            {
                return (TResult)lambda(arg1, arg2, arg3, arg4, arg5);
            }

            public TResult Func6<T1, T2, T3, T4, T5, T6, TResult>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
            {
                return (TResult)lambda(arg1, arg2, arg3, arg4, arg5, arg6);
            }

            public TResult Func7<T1, T2, T3, T4, T5, T6, T7, TResult>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7)
            {
                return (TResult)lambda(arg1, arg2, arg3, arg4, arg5, arg6, arg7);
            }

            public TResult Func8<T1, T2, T3, T4, T5, T6, T7, T8, TResult>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8)
            {
                return (TResult)lambda(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
            }

            public TResult Func9<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9)
            {
                return (TResult)lambda(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9);
            }

            public TResult Func10<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10)
            {
                return (TResult)lambda(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10);
            }

            public TResult Func11<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11)
            {
                return (TResult)lambda(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11);
            }

            public TResult Func12<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12)
            {
                return (TResult)lambda(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12);
            }

            public TResult Func13<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13)
            {
                return (TResult)lambda(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13);
            }

            public TResult Func14<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14)
            {
                return (TResult)lambda(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14);
            }

            public TResult Func15<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15)
            {
                return (TResult)lambda(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15);
            }

            public TResult Func16<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15, T16 arg16)
            {
                return (TResult)lambda(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15, arg16);
            }
        }

        #endregion
    }

    #region linked enums

    /// <summary>
    /// This enum represent the way <see cref="ExpressionEvaluator.ScriptEvaluate(string)"/> behave when no <c>return</c> keyword is found.<para/>
    /// Used for the option <see cref="ExpressionEvaluator.OptionOnNoReturnKeywordFoundInScriptAction"/>
    /// </summary>
    public enum OptionOnNoReturnKeywordFoundInScriptAction
    {
        /// <summary>
        /// ScriptEvaluate return automatically the last evaluated expression if no return keyword is met.
        /// </summary>
        ReturnAutomaticallyLastEvaluatedExpression,

        /// <summary>
        ///  ScriptEvaluate Return null if no return keyword is met.
        /// </summary>
        ReturnNull,

        /// <summary>
        /// A exception is throw if no return keyword is met.
        /// </summary>
        ThrowSyntaxException
    }

    /// <summary>
    /// This enum define the rules to use to allow or block inline namespaces.<para />
    /// Used for the option <see cref="ExpressionEvaluator.OptionInlineNamespacesEvaluationRule"/>
    /// </summary>
    public enum InlineNamespacesEvaluationRule
    {
        /// <summary>
        /// Allow the use of any inline namespace that is available in memory
        /// </summary>
        AllowAll,

        /// <summary>
        /// Allow only the use of inline namespace defined in <see cref="ExpressionEvaluator.InlineNamespacesList"/>
        /// </summary>
        AllowOnlyInlineNamespacesList,

        /// <summary>
        ///  Allow the use of any inline namespace that is available in memory that is not defined in <see cref="ExpressionEvaluator.InlineNamespacesList"/>
        /// </summary>
        BlockOnlyInlineNamespacesList,

        /// <summary>
        /// Unactive the use of inline namespaces
        /// </summary>
        BlockAll
    }

    #endregion

    #region ExpressionEvaluator linked public classes (specific Exceptions, EventArgs and Operators)

    #region Operators Management

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

    public static partial class OperatorsEvaluationsExtensions
    {
        public static IList<IDictionary<ExpressionOperator, Func<dynamic, dynamic, object>>> Copy(this IList<IDictionary<ExpressionOperator, Func<dynamic, dynamic, object>>> operatorsEvaluations)
        {
            return operatorsEvaluations
                .Select(dic => (IDictionary<ExpressionOperator, Func<dynamic, dynamic, object>>)new Dictionary<ExpressionOperator, Func<dynamic, dynamic, object>>(dic))
                .ToList();
        }

        public static IList<IDictionary<ExpressionOperator, Func<dynamic, dynamic, object>>> AddOperatorEvaluationAtLevelOf(this IList<IDictionary<ExpressionOperator, Func<dynamic, dynamic, object>>> operatorsEvaluations, ExpressionOperator operatorToAdd, Func<dynamic, dynamic, object> evaluation, ExpressionOperator levelOfThisOperator)
        {
            operatorsEvaluations
                .First(dict => dict.ContainsKey(levelOfThisOperator))
                .Add(operatorToAdd, evaluation);

            return operatorsEvaluations;
        }

        public static IList<IDictionary<ExpressionOperator, Func<dynamic, dynamic, object>>> AddOperatorEvaluationAtLevel(this IList<IDictionary<ExpressionOperator, Func<dynamic, dynamic, object>>> operatorsEvaluations, ExpressionOperator operatorToAdd, Func<dynamic, dynamic, object> evaluation, int level)
        {
            operatorsEvaluations[level]
                .Add(operatorToAdd, evaluation);

            return operatorsEvaluations;
        }

        public static IList<IDictionary<ExpressionOperator, Func<dynamic, dynamic, object>>> AddOperatorEvaluationAtNewLevelAfter(this IList<IDictionary<ExpressionOperator, Func<dynamic, dynamic, object>>> operatorsEvaluations, ExpressionOperator operatorToAdd, Func<dynamic, dynamic, object> evaluation, ExpressionOperator levelOfThisOperator)
        {
            int level = operatorsEvaluations
                .IndexOf(operatorsEvaluations.First(dict => dict.ContainsKey(levelOfThisOperator)));

            operatorsEvaluations
                .Insert(level + 1, new Dictionary<ExpressionOperator, Func<dynamic, dynamic, object>> { { operatorToAdd, evaluation } });

            return operatorsEvaluations;
        }

        public static IList<IDictionary<ExpressionOperator, Func<dynamic, dynamic, object>>> AddOperatorEvaluationAtNewLevelBefore(this IList<IDictionary<ExpressionOperator, Func<dynamic, dynamic, object>>> operatorsEvaluations, ExpressionOperator operatorToAdd, Func<dynamic, dynamic, object> evaluation, ExpressionOperator levelOfThisOperator)
        {
            int level = operatorsEvaluations
                .IndexOf(operatorsEvaluations.First(dict => dict.ContainsKey(levelOfThisOperator)));

            operatorsEvaluations
                .Insert(level, new Dictionary<ExpressionOperator, Func<dynamic, dynamic, object>> { { operatorToAdd, evaluation } });

            return operatorsEvaluations;
        }

        public static IList<IDictionary<ExpressionOperator, Func<dynamic, dynamic, object>>> AddOperatorEvaluationAtNewLevel(this IList<IDictionary<ExpressionOperator, Func<dynamic, dynamic, object>>> operatorsEvaluations, ExpressionOperator operatorToAdd, Func<dynamic, dynamic, object> evaluation, int level)
        {
            operatorsEvaluations
                .Insert(level, new Dictionary<ExpressionOperator, Func<dynamic, dynamic, object>> { { operatorToAdd, evaluation } });

            return operatorsEvaluations;
        }

        public static IList<IDictionary<ExpressionOperator, Func<dynamic, dynamic, object>>> RemoveOperatorEvaluation(this IList<IDictionary<ExpressionOperator, Func<dynamic, dynamic, object>>> operatorsEvaluations, ExpressionOperator operatorToRemove)
        {
            operatorsEvaluations.First(dict => dict.ContainsKey(operatorToRemove)).Remove(operatorToRemove);

            return operatorsEvaluations;
        }

        public static IList<ExpressionOperator> FluidAdd(this IList<ExpressionOperator> listOfOperator, ExpressionOperator operatorToAdd)
        {
            listOfOperator.Add(operatorToAdd);

            return listOfOperator;
        }

        public static IList<ExpressionOperator> FluidRemove(this IList<ExpressionOperator> listOfOperator, ExpressionOperator operatorToRemove)
        {
            listOfOperator.Remove(operatorToRemove);

            return listOfOperator;
        }
    }

    #endregion

    public partial class ClassOrEnumType

    {
        public Type Type { get; set; }
    }

    public partial class StronglyTypedVariable
    {
        public Type Type { get; set; }

        public object Value { get; set; }
    }

    public partial class SubExpression
    {
        public string Expression { get; set; }

        public SubExpression(string expression)
        {
            Expression = expression;
        }
    }

    /// <summary>
    /// Represent a group of method on which the override to call is not yet known.<para/>
    /// Simulate delegates
    /// </summary>
    public partial class MethodsGroupEncaps
    {
        /// <summary>
        /// The instance of the object on which the method group is define
        /// </summary>
        public object ContainerObject { get; set; }

        /// <summary>
        /// An array of methods overrides infos that could be potentially use for call
        /// </summary>
        public MethodInfo[] MethodsGroup { get; set; }
    }

    /// <summary>
    /// Encapsulate an exception that happend in a sub part of the expression when the evaluation need to continue before throwing the exception.
    /// </summary>
    public partial class BubbleExceptionContainer
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="exception">the exception to encapsulate</param>
        public BubbleExceptionContainer(Exception exception)
        {
            _dispatchInfo = ExceptionDispatchInfo.Capture(exception);
        }

        private readonly ExceptionDispatchInfo _dispatchInfo;

        /// <summary>
        /// Rethrow the exception with proper context.
        /// </summary>
        public void Throw() => _dispatchInfo.Throw();
    }

    public partial class ExpressionEvaluatorSyntaxErrorException : Exception
    {
        public ExpressionEvaluatorSyntaxErrorException()
        { }

        public ExpressionEvaluatorSyntaxErrorException(string message) : base(message)
        { }

        public ExpressionEvaluatorSyntaxErrorException(string message, Exception innerException) : base(message, innerException)
        { }
    }

    public partial class ExpressionEvaluatorSecurityException : Exception
    {
        public ExpressionEvaluatorSecurityException()
        { }

        public ExpressionEvaluatorSecurityException(string message) : base(message)
        { }

        public ExpressionEvaluatorSecurityException(string message, Exception innerException) : base(message, innerException)
        { }
    }

    /// <summary>
    /// Infos about the variable, attribut or property that is currently evaluate
    /// </summary>
    public partial class VariableEvaluationEventArg : EventArgs
    {
        private readonly Func<string, Type[]> evaluateGenericTypes;
        private readonly string genericTypes;

        /// <summary>
        /// Constructor of the VariableEvaluationEventArg
        /// </summary>
        /// <param name="name">The name of the variable to Evaluate</param>
        /// <param name="evaluator">The <see cref="ExpressionEvaluator"/> that detected the variable, field or property to evaluate</param>
        /// <param name="onInstance">The object instance on which to evaluate the field or property (will be <see cref="This"/>)</param>
        /// <param name="genericTypes">The generics types specified when at property access</param>
        /// <param name="evaluateGenericTypes">A func to evaluate the list of specific types given between &lt; and &gt;</param>
        public VariableEvaluationEventArg(string name, ExpressionEvaluator evaluator = null, object onInstance = null, string genericTypes = null, Func<string, Type[]> evaluateGenericTypes = null)
        {
            Name = name;
            This = onInstance;
            Evaluator = evaluator;
            this.genericTypes = genericTypes;
            this.evaluateGenericTypes = evaluateGenericTypes;
        }

        /// <summary>
        /// The name of the variable to Evaluate
        /// </summary>
        public string Name { get; }

        private object varValue;

        /// <summary>
        /// To set a value to this variable
        /// </summary>
        public object Value
        {
            get { return varValue; }
            set
            {
                varValue = value;
                HasValue = true;
            }
        }

        /// <summary>
        /// if <c>true</c> the variable is affected<para/>
        /// if <c>false</c> it means that the variable does not exist.
        /// </summary>
        public bool HasValue { get; set; }

        /// <summary>
        /// In the case of on the fly instance property definition the instance of the object on which this Property is called.<para/>
        /// Otherwise is set to null.
        /// </summary>
        public object This { get; }

        /// <summary>
        /// A reference on the current expression evaluator.
        /// </summary>
        public ExpressionEvaluator Evaluator { get; }

        /// <summary>
        /// Is <c>true</c> if some generic types are specified with &lt;&gt;.<para/>
        /// <c>false</c> otherwise
        /// </summary>
        public bool HasGenericTypes
        {
            get
            {
                return !string.IsNullOrEmpty(genericTypes);
            }
        }

        /// <summary>
        /// In the case where generic types are specified with &lt;&gt;<para/>
        /// Evaluate all types and return an array of types
        /// </summary>
        public Type[] EvaluateGenericTypes()
        {
            return evaluateGenericTypes?.Invoke(genericTypes) ?? new Type[0];
        }
    }

    /// <summary>
    /// Infos about the expression that has been/will be evaluated
    /// </summary>
    public partial class ExpressionEvaluationEventArg : EventArgs
    {
        private object value;

        /// <summary>
        /// Constructor of ExpressionEvaluationEventArg (for <see cref="ExpressionEvaluator.ExpressionEvaluating"/> event)
        /// </summary>
        /// <param name="expression">The expression that will be evaluated</param>
        /// <param name="evaluator">The evaluator that will be use to evaluate the expression</param>
        public ExpressionEvaluationEventArg(string expression, ExpressionEvaluator evaluator)
        {
            Expression = expression;
            Evaluator = evaluator;
        }

        /// <summary>
        /// Constructor of ExpressionEvaluationEventArg (for <see cref="ExpressionEvaluator.ExpressionEvaluated"/> event)
        /// </summary>
        /// <param name="expression">The expression that will be evaluated</param>
        /// <param name="evaluator"></param>
        /// <param name="value"></param>
        public ExpressionEvaluationEventArg(string expression, ExpressionEvaluator evaluator, object value)
        {
            Expression = expression;
            Evaluator = evaluator;
            this.value = value;
        }

        /// <summary>
        /// The evaluator that has been/will be use to evaluate the expression
        /// </summary>
        public ExpressionEvaluator Evaluator { get; }

        /// <summary>
        /// The Expression that wil be evaluated.
        /// Can be modified.
        /// </summary>
        public string Expression { get; set; }

        /// <summary>
        /// To set the return of the evaluation<para/>
        /// In the case of <see cref="ExpressionEvaluator.ExpressionEvaluated"/> event, store the result of the evaluation
        /// </summary>
        public object Value
        {
            get { return value; }
            set
            {
                this.value = value;
                HasValue = true;
            }
        }

        /// <summary>
        /// if <c>true</c> the expression evaluation has been done<para/>
        /// if <c>false</c> it means that the evaluation must continue.
        /// </summary>
        public bool HasValue { get; set; }
    }

    /// <summary>
    /// Infos about the variable, attribut or property that is currently evaluate
    /// </summary>
    public partial class VariablePreEvaluationEventArg : VariableEvaluationEventArg
    {
        public VariablePreEvaluationEventArg(string name, ExpressionEvaluator evaluator = null, object onInstance = null, string genericTypes = null, Func<string, Type[]> evaluateGenericTypes = null)
            : base(name, evaluator, onInstance, genericTypes, evaluateGenericTypes)
        { }

        /// <summary>
        /// If set to true cancel the evaluation of the current variable, field or property and throw an exception it does not exists
        /// </summary>
        public bool CancelEvaluation { get; set; }
    }

    /// <summary>
    /// Infos about the function or method that is currently evaluate
    /// </summary>
    public partial class FunctionEvaluationEventArg : EventArgs
    {
        private readonly Func<string, Type[]> evaluateGenericTypes;
        private readonly string genericTypes;

        /// <summary>
        /// Constructor of the <see cref="FunctionEvaluationEventArg"/>
        /// </summary>
        /// <param name="name">The name of the function or method</param>
        /// <param name="args">Arguments passed to fthe function or method</param>
        /// <param name="evaluator">The <see cref="ExpressionEvaluator"/> that detected the function or method to evaluate</param>
        /// <param name="onInstance">The object instance on which to evaluate the method (will be <see cref="This"/>)</param>
        /// <param name="genericTypes">The generics types specified when at function calling</param>
        /// <param name="evaluateGenericTypes">A func to evaluate the list of specific types given between &lt; and &gt;</param>
        public FunctionEvaluationEventArg(string name, List<string> args = null, ExpressionEvaluator evaluator = null, object onInstance = null, string genericTypes = null, Func<string, Type[]> evaluateGenericTypes = null)
        {
            Name = name;
            Args = args ?? new List<string>();
            This = onInstance;
            Evaluator = evaluator;
            this.genericTypes = genericTypes;
            this.evaluateGenericTypes = evaluateGenericTypes;
        }

        /// <summary>
        /// The not evaluated args of the function
        /// </summary>
        public List<string> Args { get; } = new List<string>();

        /// <summary>
        /// Get the values of the function's args.
        /// </summary>
        /// <returns></returns>
        public object[] EvaluateArgs()
        {
            return Args.ConvertAll(arg => Evaluator.Evaluate(arg)).ToArray();
        }

        /// <summary>
        /// Get the value of the function's arg at the specified index
        /// </summary>
        /// <param name="index">The index of the function's arg to evaluate</param>
        /// <returns>The evaluated arg</returns>
        public object EvaluateArg(int index)
        {
            return Evaluator.Evaluate(Args[index]);
        }

        /// <summary>
        /// Get the value of the function's arg at the specified index
        /// </summary>
        /// <typeparam name="T">The type of the result to get. (Do a cast)</typeparam>
        /// <param name="index">The index of the function's arg to evaluate</param>
        /// <returns>The evaluated arg casted in the specified type</returns>
        public T EvaluateArg<T>(int index)
        {
            return Evaluator.Evaluate<T>(Args[index]);
        }

        /// <summary>
        /// The name of the variable to Evaluate
        /// </summary>
        public string Name { get; }

        private object returnValue;

        /// <summary>
        /// To set the return value of the function
        /// </summary>
        public object Value
        {
            get { return returnValue; }
            set
            {
                returnValue = value;
                FunctionReturnedValue = true;
            }
        }

        /// <summary>
        /// if <c>true</c> the function evaluation has been done,<para/>
        /// if <c>false</c> it means that the function does not exist.
        /// </summary>
        public bool FunctionReturnedValue { get; set; }

        /// <summary>
        /// In the case of on the fly instance method definition the instance of the object on which this method (function) is called.<para/>
        /// Otherwise is set to null.
        /// </summary>
        public object This { get; }

        /// <summary>
        /// A reference on the current expression evaluator.
        /// </summary>
        public ExpressionEvaluator Evaluator { get; }

        /// <summary>
        /// Is <c>true</c> if some generic types are specified with &lt;&gt;.<para/>
        /// <c>false</c> otherwise
        /// </summary>
        public bool HasGenericTypes
        {
            get
            {
                return !string.IsNullOrEmpty(genericTypes);
            }
        }

        /// <summary>
        /// In the case where generic types are specified with &lt;&gt;<para/>
        /// Evaluate all types and return an array of types
        /// </summary>
        public Type[] EvaluateGenericTypes()
        {
            return evaluateGenericTypes?.Invoke(genericTypes) ?? new Type[0];
        }
    }

    /// <summary>
    /// Infos about the function or method that is currently evaluate
    /// </summary>
    public partial class FunctionPreEvaluationEventArg : FunctionEvaluationEventArg
    {
        public FunctionPreEvaluationEventArg(string name, List<string> args = null, ExpressionEvaluator evaluator = null, object onInstance = null, string genericTypes = null, Func<string, Type[]> evaluateGenericTypes = null)
            : base(name, args, evaluator, onInstance, genericTypes, evaluateGenericTypes)
        { }

        /// <summary>
        /// If set to true cancel the evaluation of the current function or method and throw an exception that the function does not exists
        /// </summary>
        public bool CancelEvaluation { get; set; }
    }

    /// <summary>
    /// Infos about the indexing that is currently evaluate
    /// </summary>
    public partial class IndexingPreEvaluationEventArg : EventArgs
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="args">The not evaluated args of the indexing</param>
        /// <param name="evaluator">A reference on the current expression evaluator.</param>
        /// <param name="onInstance">The instance of the object on which the indexing is called.<para/>Will be the <see cref="This"/> property</param>
        public IndexingPreEvaluationEventArg(List<string> args, ExpressionEvaluator evaluator, object onInstance)
        {
            Args = args;
            This = onInstance;
            Evaluator = evaluator;
        }

        /// <summary>
        /// The not evaluated args of the indexing
        /// </summary>
        public List<string> Args { get; } = new List<string>();

        /// <summary>
        /// The instance of the object on which the indexing is called.
        /// </summary>
        public object This { get; }

        private object returnValue;

        /// <summary>
        /// To set the result value of the indexing
        /// </summary>
        public object Value
        {
            get { return returnValue; }
            set
            {
                returnValue = value;
                HasValue = true;
            }
        }

        /// <summary>
        /// if <c>true</c> the indexing evaluation has been done<para/>
        /// if <c>false</c> it means that the indexing does not exist.
        /// </summary>
        public bool HasValue { get; set; }

        /// <summary>
        /// A reference on the current expression evaluator.
        /// </summary>
        public ExpressionEvaluator Evaluator { get; }

        /// <summary>
        /// Get the values of the indexing's args.
        /// </summary>
        /// <returns></returns>
        public object[] EvaluateArgs()
        {
            return Args.ConvertAll(arg => Evaluator.Evaluate(arg)).ToArray();
        }

        /// <summary>
        /// Get the value of the indexing's arg at the specified index
        /// </summary>
        /// <param name="index">The index of the indexing's arg to evaluate</param>
        /// <returns>The evaluated arg</returns>
        public object EvaluateArg(int index)
        {
            return Evaluator.Evaluate(Args[index]);
        }

        /// <summary>
        /// Get the value of the indexing's arg at the specified index
        /// </summary>
        /// <typeparam name="T">The type of the result to get. (Do a cast)</typeparam>
        /// <param name="index">The index of the indexing's arg to evaluate</param>
        /// <returns>The evaluated arg casted in the specified type</returns>
        public T EvaluateArg<T>(int index)
        {
            return Evaluator.Evaluate<T>(Args[index]);
        }

        /// <summary>
        /// If set to <c>true</c> cancel the evaluation of the current function or method and throw an exception that the function does not exists
        /// </summary>
        public bool CancelEvaluation { get; set; }
    }

    /// <summary>
    /// Class ParameterCastEvaluationEventArg
    /// </summary>
    public partial class ParameterCastEvaluationEventArg : EventArgs
    {
        /// <summary>
        /// The information of the method that it try to call
        /// </summary>
        public MethodInfo MethodInfo { get; }

        /// <summary>
        /// In the case of on the fly instance method definition the instance of the object on which this method (function) is called.<para/>
        /// Otherwise is set to null.
        /// </summary>
        public object This { get; }

        /// <summary>
        /// A reference on the current expression evaluator.
        /// </summary>
        public ExpressionEvaluator Evaluator { get; }

        /// <summary>
        /// The required type of the parameter
        /// </summary>
        public Type ParameterType { get; }

        /// <summary>
        /// The original argument
        /// </summary>
        public object OriginalArg { get; }

        /// <summary>
        /// Position of the argument (index from 0)
        /// </summary>
        public int ArgPosition { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="methodInfo">The information of the method that it try to call</param>
        /// <param name="parameterType">The required type of the parameter</param>
        /// <param name="originalArg">The original argument</param>
        /// <param name="argPosition">Position of the argument (index from 0)</param>
        /// <param name="evaluator">A reference on the current expression evaluator.</param>
        /// <param name="onInstance">In the case of on the fly instance method definition the instance of the object on which this method (function) is called.<para/>Otherwise is set to null.<para/>Will be the <see cref="This"/> property</param>
        public ParameterCastEvaluationEventArg(MethodInfo methodInfo, Type parameterType, object originalArg, int argPosition, ExpressionEvaluator evaluator = null, object onInstance = null)
        {
            MethodInfo = methodInfo;
            ParameterType = parameterType;
            OriginalArg = originalArg;
            Evaluator = evaluator;
            This = onInstance;
            ArgPosition = argPosition;
        }

        private object modifiedArgument;

        /// <summary>
        /// To set the modified argument
        /// </summary>
        public object Argument
        {
            get { return modifiedArgument; }
            set
            {
                modifiedArgument = value;
                FunctionModifiedArgument = true;
            }
        }

        /// <summary>
        /// if <c>true</c> the argument has been modified, if <c>false</c> it means that the argument can't be of the given type.
        /// </summary>
        public bool FunctionModifiedArgument { get; set; }
    }

    #endregion
}
