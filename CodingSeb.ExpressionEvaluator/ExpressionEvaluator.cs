/******************************************************************************************************
    Title : ExpressionEvaluator (https://github.com/codingseb/ExpressionEvaluator)
    Version : 1.3.7.2 
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
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace CodingSeb.ExpressionEvaluator
{
    /// <summary>
    /// This class allow to evaluate a string math or pseudo C# expression 
    /// </summary>
    public partial class ExpressionEvaluator
    {
        #region Regex declarations

        private const string diactitics = "áàâãåǎăāąæéèêëěēĕėęěìíîïīĭįĳóôõöōŏőøðœùúûüǔũūŭůűųýþÿŷıćĉċčçďđĝğġģĥħĵķĺļľŀłńņňŋñŕŗřśŝşšţťŧŵźżžÁÀÂÃÅǍĂĀĄÆÉÈÊËĚĒĔĖĘĚÌÍÎÏĪĬĮĲÓÔÕÖŌŎŐØÐŒÙÚÛÜǓŨŪŬŮŰŲÝÞŸŶIĆĈĊČÇĎĐĜĞĠĢĤĦĴĶĹĻĽĿŁŃŅŇŊÑŔŖŘŚŜŞŠŢŤŦŴŹŻŽß";
        private const string diactiticsKeywordsRegexPattern = "a-zA-Z_" + diactitics;

        private static readonly Regex varOrFunctionRegEx = new Regex($@"^((?<sign>[+-])|(?<prefixOperator>[+][+]|--)|(?<inObject>(?<nullConditional>[?])?\.)?)(?<name>[{ diactiticsKeywordsRegexPattern }](?>[{ diactiticsKeywordsRegexPattern }0-9]*))(?>\s*)((?<assignationOperator>(?<assignmentPrefix>[+\-*/%&|^]|<<|>>)?=(?![=>]))|(?<postfixOperator>([+][+]|--)(?![{ diactiticsKeywordsRegexPattern}0-9]))|((?<isgeneric>[<](?>([{ diactiticsKeywordsRegexPattern }](?>[{ diactiticsKeywordsRegexPattern }0-9]*)|(?>\s+)|[,\.])+|(?<gentag>[<])|(?<-gentag>[>]))*(?(gentag)(?!))[>])?(?<isfunction>[(])?))", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private const string numberRegexOrigPattern = @"^(?<sign>[+-])?([0-9][0-9_{1}]*[0-9]|\d)(?<hasdecimal>{0}?([0-9][0-9_]*[0-9]|\d)(e[+-]?([0-9][0-9_]*[0-9]|\d))?)?(?<type>ul|[fdulm])?";
        private string numberRegexPattern = null;

        private static readonly Regex otherBasesNumberRegex = new Regex("^(?<sign>[+-])?(?<value>0(?<type>x)([0-9a-f][0-9a-f_]*[0-9a-f]|[0-9a-f])|0(?<type>b)([01][01_]*[01]|[01]))", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex stringBeginningRegex = new Regex("^(?<interpolated>[$])?(?<escaped>[@])?[\"]", RegexOptions.Compiled);
        private static readonly Regex internalCharRegex = new Regex(@"^['](\\[\\'0abfnrtv]|[^'])[']", RegexOptions.Compiled);
        private static readonly Regex indexingBeginningRegex = new Regex(@"^[?]?\[", RegexOptions.Compiled);
        private static readonly Regex assignationOrPostFixOperatorRegex = new Regex(@"^(?>\s*)((?<assignmentPrefix>[+\-*/%&|^]|<<|>>)?=(?![=>])|(?<postfixOperator>([+][+]|--)(?![" + diactiticsKeywordsRegexPattern + "0-9])))");
        private static readonly Regex genericsDecodeRegex = new Regex("(?<name>[^,<>]+)(?<isgeneric>[<](?>[^<>]+|(?<gentag>[<])|(?<-gentag>[>]))*(?(gentag)(?!))[>])?", RegexOptions.Compiled);
        private static readonly Regex genericsEndOnlyOneTrim = new Regex(@"(?>\s*)[>](?>\s*)$", RegexOptions.Compiled);

        private static readonly Regex endOfStringWithDollar = new Regex("^([^\"{\\\\]|\\\\[\\\\\"0abfnrtv])*[\"{]", RegexOptions.Compiled);
        private static readonly Regex endOfStringWithoutDollar = new Regex("^([^\"\\\\]|\\\\[\\\\\"0abfnrtv])*[\"]", RegexOptions.Compiled);
        private static readonly Regex endOfStringWithDollarWithAt = new Regex("^[^\"{]*[\"{]", RegexOptions.Compiled);
        private static readonly Regex endOfStringWithoutDollarWithAt = new Regex("^[^\"]*[\"]", RegexOptions.Compiled);
        private static readonly Regex endOfStringInterpolationRegex = new Regex("^('\"'|[^}\"])*[}\"]", RegexOptions.Compiled);
        private static readonly Regex stringBeginningForEndBlockRegex = new Regex("[$]?[@]?[\"]$", RegexOptions.Compiled);
        private static readonly Regex lambdaExpressionRegex = new Regex($@"^(?>\s*)(?<args>((?>\s*)[(](?>\s*)([{ diactiticsKeywordsRegexPattern }](?>[{ diactiticsKeywordsRegexPattern }0-9]*)(?>\s*)([,](?>\s*)[{diactiticsKeywordsRegexPattern}][{ diactiticsKeywordsRegexPattern}0-9]*(?>\s*))*)?[)])|[{ diactiticsKeywordsRegexPattern}](?>[{ diactiticsKeywordsRegexPattern }0-9]*))(?>\s*)=>(?<expression>.*)$", RegexOptions.Singleline | RegexOptions.Compiled);
        private static readonly Regex lambdaArgRegex = new Regex($"[{ diactiticsKeywordsRegexPattern }](?>[{ diactiticsKeywordsRegexPattern }0-9]*)", RegexOptions.Compiled);
        private static readonly Regex initInNewBeginningRegex = new Regex(@"^(?>\s*){", RegexOptions.Compiled);

        // Depending on OptionInlineNamespacesEvaluationActive. Initialized in constructor
        private string InstanceCreationWithNewKeywordRegexPattern { get { return $@"^new(?>\s*)((?<isAnonymous>[{{])|((?<name>[{ diactiticsKeywordsRegexPattern }][{ diactiticsKeywordsRegexPattern}0-9{ (OptionInlineNamespacesEvaluationActive ? @"\." : string.Empty) }]*)(?>\s*)(?<isgeneric>[<](?>[^<>]+|(?<gentag>[<])|(?<-gentag>[>]))*(?(gentag)(?!))[>])?(?>\s*)((?<isfunction>[(])|(?<isArray>\[)|(?<isInit>[{{]))?))"; } }
        private string CastRegexPattern { get { return $@"^\((?>\s*)(?<typeName>[{ diactiticsKeywordsRegexPattern }][{ diactiticsKeywordsRegexPattern }0-9{ (OptionInlineNamespacesEvaluationActive ? @"\." : string.Empty) }\[\]<>]*[?]?)(?>\s*)\)"; } }

        private const string primaryTypesRegexPattern = "(?<=^|[^" + diactiticsKeywordsRegexPattern + "])(?<primaryType>object|string|bool[?]?|byte[?]?|char[?]?|decimal[?]?|double[?]?|short[?]?|int[?]?|long[?]?|sbyte[?]?|float[?]?|ushort[?]?|uint[?]?|ulong[?]?|void)(?=[^a-zA-Z_]|$)";

        // To remove comments in scripts based on https://stackoverflow.com/questions/3524317/regex-to-strip-line-comments-from-c-sharp/3524689#3524689
        private const string blockComments = @"/\*(.*?)\*/";
        private const string lineComments = @"//[^\r\n]*";
        private const string stringsIgnore = @"""((\\[^\n]|[^""\n])*)""";
        private const string verbatimStringsIgnore = @"@(""[^""]*"")+";
        private static readonly Regex removeCommentsRegex = new Regex($"{blockComments}|{lineComments}|{stringsIgnore}|{verbatimStringsIgnore}", RegexOptions.Singleline | RegexOptions.Compiled);
        private static readonly Regex newLineCharsRegex = new Regex(@"\r\n|\r|\n", RegexOptions.Compiled);

        // For script only
        private static readonly Regex blockKeywordsBeginningRegex = new Regex(@"^(?>\s*)(?<keyword>while|for|foreach|if|else(?>\s*)if|catch)(?>\s*)[(]", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex foreachParenthisEvaluationRegex = new Regex(@"^(?>\s*)(?<variableName>[" + diactiticsKeywordsRegexPattern + "](?>[" + diactiticsKeywordsRegexPattern + @"0-9]*))(?>\s*)(?<in>in)(?>\s*)(?<collection>.*)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex blockKeywordsWithoutParenthesesBeginningRegex = new Regex(@"^(?>\s*)(?<keyword>else|do|try|finally)(?![" + diactiticsKeywordsRegexPattern + "0-9])", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex blockBeginningRegex = new Regex(@"^(?>\s*)[{]", RegexOptions.Compiled);
        private static readonly Regex returnKeywordRegex = new Regex(@"^return((?>\s*)|\()", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);
        private static readonly Regex nextIsEndOfExpressionRegex = new Regex(@"^(?>\s*)[;]", RegexOptions.Compiled);

        #endregion

        #region enums (Operators, if else blocks states)

        private enum ExpressionOperator
        {
            Plus,
            Minus,
            UnaryPlus,
            UnaryMinus,
            Multiply,
            Divide,
            Modulo,
            Lower,
            Greater,
            Equal,
            LowerOrEqual,
            GreaterOrEqual,
            Is,
            NotEqual,
            LogicalNegation,
            BitwiseComplement,
            ConditionalAnd,
            ConditionalOr,
            LogicalAnd,
            LogicalOr,
            LogicalXor,
            ShiftBitsLeft,
            ShiftBitsRight,
            NullCoalescing,
            Cast,
            Indexing,
            IndexingWithNullConditional,
        }

        private enum IfBlockEvaluatedState
        {
            NoBlockEvaluated,
            If,
            ElseIf
        }

        private enum TryBlockEvaluatedState
        {
            NoBlockEvaluated,
            Try,
            Catch
        }

        #endregion

        #region Dictionaries declarations (Primary types, number suffix, escaped chars, operators management, default vars and functions)

        private static readonly Dictionary<string, Type> primaryTypesDict = new Dictionary<string, Type>()
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

        private static readonly Dictionary<string, Func<string, CultureInfo, object>> numberSuffixToParse = new Dictionary<string, Func<string, CultureInfo, object>>(StringComparer.OrdinalIgnoreCase) // Always Case insensitive, like in C#
        {
            { "f", (number, culture) => float.Parse(number, NumberStyles.Any, culture) },
            { "d", (number, culture) => double.Parse(number, NumberStyles.Any, culture) },
            { "u", (number, culture) => uint.Parse(number, NumberStyles.Any, culture) },
            { "l", (number, culture) => long.Parse(number, NumberStyles.Any, culture) },
            { "ul", (number, culture) => ulong.Parse(number, NumberStyles.Any, culture) },
            { "m", (number, culture) => decimal.Parse(number, NumberStyles.Any, culture) }
        };

        private static readonly Dictionary<char, string> stringEscapedCharDict = new Dictionary<char, string>()
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

        private static readonly Dictionary<char, char> charEscapedCharDict = new Dictionary<char, char>()
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

        private Dictionary<string, ExpressionOperator> operatorsDictionary = new Dictionary<string, ExpressionOperator>(StringComparer.Ordinal)
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

        private static readonly Dictionary<ExpressionOperator, bool> leftOperandOnlyOperatorsEvaluationDictionary = new Dictionary<ExpressionOperator, bool>();

        private static readonly Dictionary<ExpressionOperator, bool> rightOperandOnlyOperatorsEvaluationDictionary = new Dictionary<ExpressionOperator, bool>()
        {
            {ExpressionOperator.LogicalNegation, true },
            {ExpressionOperator.BitwiseComplement, true },
            {ExpressionOperator.UnaryPlus, true },
            {ExpressionOperator.UnaryMinus, true }
        };

        private static readonly List<Dictionary<ExpressionOperator, Func<dynamic, dynamic, object>>> operatorsEvaluations =
            new List<Dictionary<ExpressionOperator, Func<dynamic, dynamic, object>>>()
        {
            new Dictionary<ExpressionOperator, Func<dynamic, dynamic, object>>()
            {
                {ExpressionOperator.Indexing, (dynamic left, dynamic right) =>
                    {
                        Type type = ((object)left).GetType();

                        if(left is IDictionary<string, object> dictionaryLeft)
                        {
                            return dictionaryLeft[right];
                        }
                        else if(type.GetMethod("Item", new Type[] { ((object)right).GetType() }) is MethodInfo methodInfo)
                        {
                            return methodInfo.Invoke(left, new object[] { right });
                        }

                        return left[right];
                    }
                },
                {ExpressionOperator.IndexingWithNullConditional, (dynamic left, dynamic right) => left is IDictionary<string,object> dictionaryLeft ? dictionaryLeft[right] : left?[right] },
            },
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
                {ExpressionOperator.ConditionalAnd, (dynamic left, dynamic right) => left && right },
            },
            new Dictionary<ExpressionOperator, Func<dynamic, dynamic, object>>()
            {
                {ExpressionOperator.ConditionalOr, (dynamic left, dynamic right) => left || right },
            },
            new Dictionary<ExpressionOperator, Func<dynamic, dynamic, object>>()
            {
                {ExpressionOperator.NullCoalescing, (dynamic left, dynamic right) => left ?? right },
            },
        };

        private Dictionary<string, object> defaultVariables = new Dictionary<string, object>(StringComparer.Ordinal)
        {
            { "Pi", Math.PI },
            { "E", Math.E },
            { "null", null},
            { "true", true },
            { "false", false },
        };

        private Dictionary<string, Func<double, double>> simpleDoubleMathFuncsDictionary = new Dictionary<string, Func<double, double>>(StringComparer.Ordinal)
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

        private Dictionary<string, Func<double, double, double>> doubleDoubleMathFuncsDictionary = new Dictionary<string, Func<double, double, double>>(StringComparer.Ordinal)
        {
            { "Atan2", Math.Atan2 },
            { "IEEERemainder", Math.IEEERemainder },
            { "Log", Math.Log },
            { "Pow", Math.Pow },
        };

        private Dictionary<string, Func<ExpressionEvaluator, List<string>, object>> complexStandardFuncsDictionary = new Dictionary<string, Func<ExpressionEvaluator, List<string>, object>>(StringComparer.Ordinal)
        {
            { "Array", (self, args) => args.ConvertAll(arg => self.Evaluate(arg)).ToArray() },
            { "ArrayOfType", (self, args) =>
                {
                    Array sourceArray = args.Skip(1).Select(arg => self.Evaluate(arg)).ToArray();
                    Array typedArray = Array.CreateInstance((Type)self.Evaluate(args[0]), sourceArray.Length);
                    Array.Copy(sourceArray, typedArray, sourceArray.Length);

                    return typedArray;
                }
            },
            { "Avg", (self, args) => args.ConvertAll(arg => Convert.ToDouble(self.Evaluate(arg))).Sum() / args.Count },
            { "default", (self, args) =>
                {
                    object argValue = self.Evaluate(args[0]);

                    if (argValue is ClassOrEnumType classOrTypeName)
                        return Activator.CreateInstance(classOrTypeName.Type);
                    else
                        return null;
                }
            },
            //{ "if", (self, args) => (bool)self.Evaluate(args[0]) ? self.Evaluate(args[1]) : self.Evaluate(args[2]) },
            { "in", (self, args) => args.Skip(1).ToList().ConvertAll(arg => self.Evaluate(arg)).Contains(self.Evaluate(args[0])) },
            { "List", (self, args) => args.ConvertAll(arg => self.Evaluate(arg)) },
            { "ListOfType", (self, args) =>
                {
                    Type type = (Type)self.Evaluate(args[0]);
                    Array sourceArray = args.Skip(1).Select(arg => self.Evaluate(arg)).ToArray();
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
                    List<object> cArgs = args.ConvertAll(arg => self.Evaluate(arg));
                    return Activator.CreateInstance((cArgs[0] as ClassOrEnumType).Type, cArgs.Skip(1).ToArray());
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
        /// if set to <c>true</c> use a cache for types that were resolved to resolve faster next time.
        /// if set to <c>false</c> the cach of types resolution is not use for this instance of ExpressionEvaluator.
        /// Default : false
        /// the cache is the static Dictionary TypesResolutionCaching (so it is shared by all instances of ExpressionEvaluator that have CacheTypesResolutions enabled)
        /// </summary>
        public bool CacheTypesResolutions { get; set; } = false;

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

        /// <summary>
        /// All assemblies needed to resolves Types
        /// by default all Assemblies loaded in the current AppDomain
        /// </summary>
        public List<Assembly> Assemblies { get; set; } = new List<Assembly>();

        /// <summary>
        /// All Namespaces Where to find types
        /// </summary>
        public List<string> Namespaces { get; set; } = new List<string>()
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
        public List<Type> Types { get; set; } = new List<Type>();

        /// <summary>
        /// A list of type to block an keep un usable in Expression Evaluation for security purpose
        /// </summary>
        public List<Type> TypesToBlock { get; set; } = new List<Type>();

        /// <summary>
        /// A list of statics types where to find extensions methods
        /// </summary>
        public List<Type> StaticTypesForExtensionsMethods { get; set; } = new List<Type>()
        {
            typeof(Enumerable) // For Linq extension methods
        };

        #endregion

        #region Options

        private bool optionCaseSensitiveEvaluationActive = true;

        /// <summary>
        /// If <c>true</c> all evaluation are case sensitives.
        /// If <c>false</c> evaluations are case insensitive.
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

        private StringComparison StringComparisonForCasing { get; set; } = StringComparison.Ordinal;

        private StringComparer StringComparerForCasing
        {
            get
            {
                return OptionCaseSensitiveEvaluationActive ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase;
            }
        }

        /// <summary>
        /// If <c>true</c> all numbers without decimal and suffixes evaluations will be done as double
        /// If <c>false</c> Integers values without decimal and suffixes will be evaluate as int as in C# (Warning some operation can round values)
        /// By default = false
        /// </summary>
        public bool OptionForceIntegerNumbersEvaluationsAsDoubleByDefault { get; set; } = false;

        private CultureInfo cultureInfoForNumberParsing = CultureInfo.InvariantCulture.Clone() as CultureInfo;

        /// <summary>
        /// The culture used to evaluate numbers
        /// Synchronized with OptionNumberParsingDecimalSeparator and OptionNumberParsingThousandSeparator.
        /// So always set a full CultureInfo object and do not change CultureInfoForNumberParsing.NumberFormat.NumberDecimalSeparator and CultureInfoForNumberParsing.NumberFormat.NumberGroupSeparator properties directly.
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
        /// Allow to change the decimal separator of numbers when parsing expressions.
        /// By default "."
        /// Warning if using comma change also OptionFunctionArgumentsSeparator and OptionInitializersSeparator otherwise it will create conflicts.
        /// Modify CultureInfoForNumberParsing.
        /// </summary>
        public string OptionNumberParsingDecimalSeparator
        {
            get
            {
                return optionNumberParsingDecimalSeparator;
            }

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
        /// Allow to change the thousand separator of numbers when parsing expressions.
        /// By default string.Empty
        /// Warning if using comma change also OptionFunctionArgumentsSeparator and OptionInitializersSeparator otherwise it will create conflicts.
        /// Modify CultureInfoForNumberParsing.
        /// </summary>
        public string OptionNumberParsingThousandSeparator
        {
            get
            {
                return optionNumberParsingThousandSeparator;
            }

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
        /// Allow to change the separator of functions arguments.
        /// By default ","
        /// Warning must to be changed if OptionNumberParsingDecimalSeparator = "," otherwise it will create conflicts
        /// </summary>
        public string OptionFunctionArgumentsSeparator { get; set; } = ",";

        /// <summary>
        /// Allow to change the separator of Object and collections Initialization between { and } after the keyword new.
        /// By default ","
        /// Warning must to be changed if OptionNumberParsingDecimalSeparator = "," otherwise it will create conflicts
        /// </summary>
        public string OptionInitializersSeparator { get; set; } = ",";

        /// <summary>
        /// if <c>true</c> allow to add the prefix Fluid or Fluent before void methods names to return back the instance on which the method is call.
        /// if <c>false</c> unactive this functionality.
        /// By default : true
        /// </summary>
        public bool OptionFluidPrefixingActive { get; set; } = true;

        /// <summary>
        /// if <c>true</c> allow the use of inline namespace (Can be slow, and is less secure). 
        /// if <c>false</c> unactive inline namespace (only namespaces in Namespaces list are available). 
        /// By default : true
        /// </summary>
        public bool OptionInlineNamespacesEvaluationActive { get; set; } = true;

        private Func<ExpressionEvaluator, List<string>, object> newMethodMem;

        /// <summary>
        /// if <c>true</c> allow to create instance of object with the Default function new(ClassNam,...).
        /// if <c>false</c> unactive this functionality.
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
        /// if <c>true</c> allow to create instance of object with the C# syntax new ClassName(...).
        /// if <c>false</c> unactive this functionality.
        /// By default : true
        /// </summary>
        public bool OptionNewKeywordEvaluationActive { get; set; } = true;

        /// <summary>
        /// if <c>true</c> allow to call static methods on classes.
        /// if <c>false</c> unactive this functionality.
        /// By default : true
        /// </summary>
        public bool OptionStaticMethodsCallActive { get; set; } = true;

        /// <summary>
        /// if <c>true</c> allow to get static properties on classes
        /// if <c>false</c> unactive this functionality.
        /// By default : true
        /// </summary>
        public bool OptionStaticProperiesGetActive { get; set; } = true;

        /// <summary>
        /// if <c>true</c> allow to call instance methods on objects.
        /// if <c>false</c> unactive this functionality.
        /// By default : true
        /// </summary>
        public bool OptionInstanceMethodsCallActive { get; set; } = true;

        /// <summary>
        /// if <c>true</c> allow to get instance properties on objects
        /// if <c>false</c> unactive this functionality.
        /// By default : true
        /// </summary>
        public bool OptionInstanceProperiesGetActive { get; set; } = true;

        /// <summary>
        /// if <c>true</c> allow to get object at index or key like IndexedObject[indexOrKey]
        /// if <c>false</c> unactive this functionality.
        /// By default : true
        /// </summary>
        public bool OptionIndexingActive { get; set; } = true;

        /// <summary>
        /// if <c>true</c> allow string interpretation with ""
        /// if <c>false</c> unactive this functionality.
        /// By default : true
        /// </summary>
        public bool OptionStringEvaluationActive { get; set; } = true;

        /// <summary>
        /// if <c>true</c> allow char interpretation with ''
        /// if <c>false</c> unactive this functionality.
        /// By default : true
        /// </summary>
        public bool OptionCharEvaluationActive { get; set; } = true;

        /// <summary>
        /// If <c>true</c> Evaluate function is callables in an expression. If <c>false</c> Evaluate is not callable.
        /// By default : true 
        /// if set to false for security (also ensure that ExpressionEvaluator type is in TypesToBlock list)
        /// </summary>
        public bool OptionEvaluateFunctionActive { get; set; } = true;

        /// <summary>
        /// If <c>true</c> allow to assign a value to a variable in the Variable disctionary with (=, +=, -=, *=, /=, %=, &=, |=, ^=, <<=, >>=, ++ or --)
        /// If <c>false</c> unactive this functionality
        /// By default : true
        /// </summary>
        public bool OptionVariableAssignationActive { get; set; } = true;

        /// <summary>
        /// If <c>true</c> allow to set/modify a property or a field value with (=, +=, -=, *=, /=, %=, &=, |=, ^=, <<=, >>=, ++ or --)
        /// If <c>false</c> unactive this functionality
        /// By default : true
        /// </summary>
        public bool OptionPropertyOrFieldSetActive { get; set; } = true;

        /// <summary>
        /// If <c>true</c> allow to assign a indexed element like Collections, List, Arrays and Dictionaries with (=, +=, -=, *=, /=, %=, &=, |=, ^=, <<=, >>=, ++ or --)
        /// If <c>false</c> unactive this functionality
        /// By default : true
        /// </summary>
        public bool OptionIndexingAssignationActive { get; set; } = true;

        /// <summary>
        /// If <c>true</c> ScriptEvaluate function is callables in an expression. If <c>false</c> Evaluate is not callable.
        /// By default : true 
        /// if set to false for security (also ensure that ExpressionEvaluator type is in TypesToBlock list)
        /// </summary>
        public bool OptionScriptEvaluateFunctionActive { get; set; } = true;

        /// <summary>
        /// If <c>ReturnAutomaticallyLastEvaluatedExpression</c> ScriptEvaluate return automatically the last evaluated expression if no return keyword is met.
        /// If <c>ReturnNull</c> return null if no return keyword is met.
        /// If <c>ThrowSyntaxException</c> a exception is throw if no return keyword is met.
        /// By default : ReturnAutomaticallyLastEvaluatedExpression;
        /// </summary>
        public OptionOnNoReturnKeywordFoundInScriptAction OptionOnNoReturnKeywordFoundInScriptAction { get; set; } = OptionOnNoReturnKeywordFoundInScriptAction.ReturnAutomaticallyLastEvaluatedExpression;

        #endregion

        #region Reflection flags

        private BindingFlags InstanceBindingFlag
        {
            get
            {
                BindingFlags flag = BindingFlags.Default | BindingFlags.Public | BindingFlags.Instance;

                if (!OptionCaseSensitiveEvaluationActive)
                    flag |= BindingFlags.IgnoreCase;

                return flag;
            }
        }

        public BindingFlags StaticBindingFlag
        {
            get
            {
                BindingFlags flag = BindingFlags.Default | BindingFlags.Public | BindingFlags.Static;

                if (!OptionCaseSensitiveEvaluationActive)
                    flag |= BindingFlags.IgnoreCase;

                return flag;
            }
        }

        #endregion

        #region Custom and on the fly variables and methods

        private IDictionary<string, object> variables = new Dictionary<string, object>(StringComparer.Ordinal);

        /// <summary>
        /// The Values of the variable use in the expressions
        /// </summary>
        public IDictionary<string, object> Variables
        {
            get { return variables; }
            set { variables = value == null ? new Dictionary<string, object>(StringComparerForCasing) : new Dictionary<string, object>(value, StringComparerForCasing); }
        }

        /// <summary>
        /// Is Fired before a variable, field or property resolution.
        /// Allow to define a variable and the corresponding value on the fly.
        /// Allow also to cancel the evaluation of this variable (consider it does'nt exists)
        /// </summary>
        public event EventHandler<VariablePreEvaluationEventArg> PreEvaluateVariable;

        /// <summary>
        /// Is Fired before a function or method resolution.
        /// Allow to define a function or method and the corresponding value on the fly.
        /// Allow also to cancel the evaluation of this function (consider it does'nt exists)
        /// </summary>
        public event EventHandler<FunctionPreEvaluationEventArg> PreEvaluateFunction;

        /// <summary>
        /// Is Fired if no variable, field or property were found
        /// Allow to define a variable and the corresponding value on the fly.
        /// </summary>
        public event EventHandler<VariableEvaluationEventArg> EvaluateVariable;

        /// <summary>
        /// Is Fired if no function or method when were found.
        /// Allow to define a function or method and the corresponding value on the fly.
        /// </summary>
        public event EventHandler<FunctionEvaluationEventArg> EvaluateFunction;

        #endregion

        #region Constructors

        /// <summary>
        /// Default Constructor
        /// </summary>
        public ExpressionEvaluator()
        {
            Assemblies.AddRange(AppDomain.CurrentDomain.GetAssemblies());

            numberRegexPattern = string.Format(numberRegexOrigPattern, @"\.", string.Empty);

            CultureInfoForNumberParsing.NumberFormat.NumberDecimalSeparator = ".";
        }

        /// <summary>
        /// Constructor with variable initialize
        /// </summary>
        /// <param name="variables">The Values of the variable use in the expressions</param>
        public ExpressionEvaluator(IDictionary<string, object> variables) : this()
        {
            Variables = variables;
        }

        #endregion

        #region Main evaluate methods (Expressions and scripts ==> public)

        private bool inScript = false;

        /// <summary>
        /// Evaluate a script (multiple expressions separated by semicolon)
        /// Support Assignation with [=] (for simple variable write in the Variables dictionary)
        /// support also if, else if, else while and for keywords
        /// </summary>
        /// <typeparam name="T">The type in which to cast the result of the expression</typeparam>
        /// <param name="script">the script to evaluate</param>
        /// <returns>The result of the last evaluated expression</returns>
        public T ScriptEvaluate<T>(string script)
        {
            return (T)ScriptEvaluate(script);
        }

        /// <summary>
        /// Evaluate a script (multiple expressions separated by semicolon)
        /// Support Assignation with [=] (for simple variable write in the Variables dictionary)
        /// support also if, else if, else while and for keywords
        /// </summary>
        /// <param name="script">the script to evaluate</param>
        /// <returns>The result of the last evaluated expression</returns>
        public object ScriptEvaluate(string script)
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

        private object ScriptEvaluate(string script, ref bool valueReturned, ref bool breakCalled, ref bool continueCalled)
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
                    throw Evaluate(expression.Remove(0, 6)) as Exception;
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
                        index += charMatch.Length;

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
                                throw new ExpressionEvaluatorSyntaxErrorException("No [while] keyword afte the [do] keyword and block");
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
                                    continue;
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
                                dynamic collection = Evaluate(foreachParenthisEvaluationMatch.Groups["collection"].Value);

                                foreach (dynamic foreachValue in collection)
                                {
                                    Variables[foreachParenthisEvaluationMatch.Groups["variableName"].Value] = foreachValue;

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
                        }
                    }

                    startOfExpression = i;
                }
                else
                {
                    ExecuteBlocksStacks();

                    if (TryParseStringAndParenthisAndCurlyBrackets(ref i)) { }
                    else if (script.Length - i > 2 && script.Substring(i, 3).Equals("';'"))
                    {
                        i += 2;
                    }
                    else if (script[i] == ';')
                    {
                        lastResult = ScriptExpressionEvaluate(ref i);
                    }

                    ifBlockEvaluatedState = IfBlockEvaluatedState.NoBlockEvaluated;
                    tryBlockEvaluatedState = TryBlockEvaluatedState.NoBlockEvaluated;

                    i++;
                }
            }

            if (!script.Substring(startOfExpression).Trim().Equals(string.Empty) && !isReturn && !isBreak && !isContinue)
                throw new ExpressionEvaluatorSyntaxErrorException("A [;] character is missing.");

            ExecuteBlocksStacks();

            valueReturned = isReturn;
            breakCalled = isBreak;
            continueCalled = isContinue;

            inScript = false;

            if (isReturn || OptionOnNoReturnKeywordFoundInScriptAction == OptionOnNoReturnKeywordFoundInScriptAction.ReturnAutomaticallyLastEvaluatedExpression)
                return lastResult;
            else if (OptionOnNoReturnKeywordFoundInScriptAction == OptionOnNoReturnKeywordFoundInScriptAction.ReturnNull)
                return null;
            else
                throw new ExpressionEvaluatorSyntaxErrorException("No [return] keyword found");
        }

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

        /// <summary>
        /// Evaluate the specified math or pseudo C# expression
        /// </summary>
        /// <param name="expression">the math or pseudo C# expression to evaluate</param>
        /// <returns>The result of the operation if syntax is correct</returns>
        public object Evaluate(string expression)
        {
            bool continueEvaluation = true;

            expression = expression.Trim();

            Stack<object> stack = new Stack<object>();

            if (GetLambdaExpression(expression, stack))
                return stack.Pop();

            for (int i = 0; i < expression.Length && continueEvaluation; i++)
            {
                string restOfExpression = expression.Substring(i, expression.Length - i);

                if (!(EvaluateCast(restOfExpression, stack, ref i)
                    || EvaluateNumber(restOfExpression, stack, ref i)
                    || EvaluateInstanceCreationWithNewKeyword(expression, restOfExpression, stack, ref i)
                    || EvaluateVarOrFunc(expression, restOfExpression, stack, ref i)
                    || EvaluateTwoCharsOperators(expression, stack, ref i)))
                {
                    string s = expression.Substring(i, 1);

                    if (EvaluateChar(expression, s, stack, ref i)
                        || EvaluateParenthis(expression, s, stack, ref i)
                        || EvaluateIndexing(expression, s, stack, ref i)
                        || EvaluateString(expression, s, restOfExpression, stack, ref i))
                    { }
                    else if (operatorsDictionary.ContainsKey(s))
                    {
                        stack.Push(operatorsDictionary[s]);
                    }
                    else if (s.Equals("?"))
                    {
                        bool condition = (bool)ProcessStack(stack);

                        for (int j = 1; j < restOfExpression.Length; j++)
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

                                stack.Push(condition ? Evaluate(restOfExpression.Substring(1, j - 1)) : Evaluate(restOfExpression.Substring(j + 1)));

                                continueEvaluation = false;

                                break;
                            }
                        }
                    }
                    else if (!s.Trim().Equals(string.Empty))
                    {
                        throw new ExpressionEvaluatorSyntaxErrorException($"Invalid character [{(int)s[0]}:{s}]");
                    }
                }
            }

            return ProcessStack(stack);
        }

        #endregion

        #region Sub parts evaluate methods (private)

        private bool EvaluateCast(string restOfExpression, Stack<object> stack, ref int i)
        {
            Match castMatch = Regex.Match(restOfExpression, CastRegexPattern, optionCaseSensitiveEvaluationActive ? RegexOptions.None : RegexOptions.IgnoreCase);

            if (castMatch.Success)
            {
                string typeName = castMatch.Groups["typeName"].Value;

                Type type = GetTypeByFriendlyName(typeName);

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

        private bool EvaluateNumber(string restOfExpression, Stack<object> stack, ref int i)
        {
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
                else
                {
                    if (OptionForceIntegerNumbersEvaluationsAsDoubleByDefault || numberMatch.Groups["hasdecimal"].Success)
                    {
                        stack.Push(double.Parse(numberMatch.Value.Replace("_", ""), NumberStyles.Any, CultureInfoForNumberParsing));
                    }
                    else
                    {
                        stack.Push(int.Parse(numberMatch.Value.Replace("_", ""), NumberStyles.Any, CultureInfoForNumberParsing));
                    }
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        private bool EvaluateInstanceCreationWithNewKeyword(string expr, string restOfExpression, Stack<object> stack, ref int i)
        {
            if (!OptionNewKeywordEvaluationActive)
                return false;

            Match instanceCreationMatch = Regex.Match(restOfExpression, InstanceCreationWithNewKeywordRegexPattern, optionCaseSensitiveEvaluationActive ? RegexOptions.None : RegexOptions.IgnoreCase);

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

                    List<string> initArgs = GetExpressionsBetweenParenthesesOrOtherImbricableBrackets(expr, ref i, true, OptionInitializersSeparator, "{", "}");

                    InitSimpleObjet(element, initArgs);

                    stack.Push(element);
                }
                else
                {
                    string completeName = instanceCreationMatch.Groups["name"].Value;
                    string genericTypes = instanceCreationMatch.Groups["isgeneric"].Value;
                    Type type = GetTypeByFriendlyName(completeName, genericTypes);

                    if (type == null)
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
                                    dynamic value = Evaluate(subArgs[1]);

                                    indexedObject[index] = value;
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
                        List<string> constructorArgs = GetExpressionsBetweenParenthesesOrOtherImbricableBrackets(expr, ref i, true, OptionFunctionArgumentsSeparator);
                        i++;

                        List<object> cArgs = constructorArgs.ConvertAll(arg => Evaluate(arg));

                        object element = Activator.CreateInstance(type, cArgs.ToArray());

                        Match blockBeginningMatch = blockBeginningRegex.Match(expr.Substring(i));

                        if (blockBeginningMatch.Success)
                        {
                            i += blockBeginningMatch.Length;

                            List<string> initArgs = GetExpressionsBetweenParenthesesOrOtherImbricableBrackets(expr, ref i, true, OptionInitializersSeparator, "{", "}");

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

                        List<string> initArgs = GetExpressionsBetweenParenthesesOrOtherImbricableBrackets(expr, ref i, true, OptionInitializersSeparator, "{", "}");

                        Init(element, initArgs);

                        stack.Push(element);
                    }
                    else if (instanceCreationMatch.Groups["isArray"].Success)
                    {
                        List<string> arrayArgs = GetExpressionsBetweenParenthesesOrOtherImbricableBrackets(expr, ref i, true, OptionInitializersSeparator, "[", "]");
                        i++;
                        Array array = null;

                        if (arrayArgs.Count > 0)
                        {
                            array = Array.CreateInstance(type, arrayArgs.ConvertAll(subExpression => Convert.ToInt32(Evaluate(subExpression))).ToArray());
                        }

                        Match initInNewBeginningMatch = initInNewBeginningRegex.Match(expr.Substring(i));

                        if (initInNewBeginningMatch.Success)
                        {
                            i += initInNewBeginningMatch.Length;

                            List<string> arrayElements = GetExpressionsBetweenParenthesesOrOtherImbricableBrackets(expr, ref i, true, OptionInitializersSeparator, "{", "}");

                            if (array == null)
                                array = Array.CreateInstance(type, arrayElements.Count);

                            Array.Copy(arrayElements.ConvertAll(subExpression => Evaluate(subExpression)).ToArray(), array, arrayElements.Count);
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

        private bool EvaluateVarOrFunc(string expr, string restOfExpression, Stack<object> stack, ref int i)
        {
            Match varFuncMatch = varOrFunctionRegEx.Match(restOfExpression);

            if (varFuncMatch.Success
            && (!varFuncMatch.Groups["sign"].Success
                || stack.Count == 0
                || stack.Peek() is ExpressionOperator)
            && !operatorsDictionary.ContainsKey(varFuncMatch.Value.Trim()))
            {
                string varFuncName = varFuncMatch.Groups["name"].Value;
                string genericsTypes = varFuncMatch.Groups["isgeneric"].Value;

                i += varFuncMatch.Length;

                if (varFuncMatch.Groups["isfunction"].Success)
                {
                    List<string> funcArgs = GetExpressionsBetweenParenthesesOrOtherImbricableBrackets(expr, ref i, true, OptionFunctionArgumentsSeparator);
                    if (varFuncMatch.Groups["inObject"].Success)
                    {
                        if (stack.Count == 0 || stack.Peek() is ExpressionOperator)
                        {
                            throw new ExpressionEvaluatorSyntaxErrorException($"[{varFuncMatch.Value})] must follow an object.");
                        }
                        else
                        {
                            object obj = stack.Pop();
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
                                if (varFuncMatch.Groups["nullConditional"].Success && obj == null)
                                {
                                    stack.Push(null);
                                }
                                else
                                {
                                    FunctionPreEvaluationEventArg functionPreEvaluationEventArg = new FunctionPreEvaluationEventArg(varFuncName, Evaluate, funcArgs, this, obj, genericsTypes, GetConcreteTypes);

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
                                        List<object> oArgs = funcArgs.ConvertAll(arg => Evaluate(arg));
                                        BindingFlags flag = DetermineInstanceOrStatic(ref objType, ref obj, ref valueTypeNestingTrace);

                                        if (!OptionStaticMethodsCallActive && (flag & BindingFlags.Static) != 0)
                                            throw new ExpressionEvaluatorSyntaxErrorException($"[{objType}] object has no Method named \"{varFuncName}\".");
                                        if (!OptionInstanceMethodsCallActive && (flag & BindingFlags.Instance) != 0)
                                            throw new ExpressionEvaluatorSyntaxErrorException($"[{objType}] object has no Method named \"{varFuncName}\".");

                                        // Standard Instance or public method find
                                        MethodInfo methodInfo = GetRealMethod(ref objType, ref obj, varFuncName, flag, oArgs, genericsTypes);

                                        // if not found check if obj is an expandoObject or similar
                                        if (obj is IDynamicMetaObjectProvider && obj is IDictionary<string, object> dictionaryObject && (dictionaryObject[varFuncName] is InternalDelegate || dictionaryObject[varFuncName] is Delegate))
                                        {
                                            if (dictionaryObject[varFuncName] is InternalDelegate internalDelegate)
                                                stack.Push(internalDelegate(oArgs.ToArray()));
                                            else
                                                stack.Push((dictionaryObject[varFuncName] as Delegate).DynamicInvoke(oArgs.ToArray()));
                                        }
                                        else if (objType.GetProperty(varFuncName, InstanceBindingFlag) is PropertyInfo instancePropertyInfo
                                            && (instancePropertyInfo.PropertyType.IsSubclassOf(typeof(Delegate)) || instancePropertyInfo.PropertyType == typeof(Delegate)))
                                        {
                                            stack.Push((instancePropertyInfo.GetValue(obj) as Delegate).DynamicInvoke(oArgs.ToArray()));
                                        }
                                        else
                                        {
                                            bool isExtention = false;

                                            // if not found try to Find extension methods.
                                            if (methodInfo == null && obj != null)
                                            {
                                                oArgs.Insert(0, obj);
                                                objType = obj.GetType();
                                                //obj = null;
                                                object extentionObj = null;
                                                for (int e = 0; e < StaticTypesForExtensionsMethods.Count && methodInfo == null; e++)
                                                {
                                                    Type type = StaticTypesForExtensionsMethods[e];
                                                    methodInfo = GetRealMethod(ref type, ref extentionObj, varFuncName, StaticBindingFlag, oArgs, genericsTypes);
                                                    isExtention = methodInfo != null;
                                                }
                                            }

                                            if (methodInfo != null)
                                            {
                                                stack.Push(methodInfo.Invoke(isExtention ? null : obj, oArgs.ToArray()));
                                            }
                                            else if (objType.GetProperty(varFuncName, StaticBindingFlag) is PropertyInfo staticPropertyInfo
                                            && (staticPropertyInfo.PropertyType.IsSubclassOf(typeof(Delegate)) || staticPropertyInfo.PropertyType == typeof(Delegate)))
                                            {
                                                stack.Push((staticPropertyInfo.GetValue(obj) as Delegate).DynamicInvoke(oArgs.ToArray()));
                                            }
                                            else
                                            {
                                                FunctionEvaluationEventArg functionEvaluationEventArg = new FunctionEvaluationEventArg(varFuncName, Evaluate, funcArgs, this, obj ?? keepObj, genericsTypes, GetConcreteTypes);

                                                EvaluateFunction?.Invoke(this, functionEvaluationEventArg);

                                                if (functionEvaluationEventArg.FunctionReturnedValue)
                                                {
                                                    stack.Push(functionEvaluationEventArg.Value);
                                                }
                                                else
                                                    throw new ExpressionEvaluatorSyntaxErrorException($"[{objType}] object has no Method named \"{varFuncName}\".");
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
                            catch (Exception ex)
                            {
                                throw new ExpressionEvaluatorSyntaxErrorException($"The call of the method \"{varFuncName}\" on type [{objType}] generate this error : {ex.InnerException?.Message ?? ex.Message}", ex);
                            }
                        }
                    }
                    else
                    {
                        FunctionPreEvaluationEventArg functionPreEvaluationEventArg = new FunctionPreEvaluationEventArg(varFuncName, Evaluate, funcArgs, this, null, genericsTypes, GetConcreteTypes);

                        PreEvaluateFunction?.Invoke(this, functionPreEvaluationEventArg);

                        if (functionPreEvaluationEventArg.CancelEvaluation)
                        {
                            throw new ExpressionEvaluatorSyntaxErrorException($"Function [{varFuncName}] unknown in expression : [{expr.Replace("\r", "").Replace("\n", "")}]");
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
                            stack.Push(lambdaExpression.Invoke(funcArgs.ConvertAll(e => Evaluate(e)).ToArray()));
                        }
                        else if (Variables.TryGetValue(varFuncName, out o) && o is Delegate delegateVar)
                        {
                            stack.Push(delegateVar.DynamicInvoke(funcArgs.ConvertAll(e => Evaluate(e)).ToArray()));
                        }
                        else
                        {
                            FunctionEvaluationEventArg functionEvaluationEventArg = new FunctionEvaluationEventArg(varFuncName, Evaluate, funcArgs, this, genericTypes: genericsTypes, evaluateGenericTypes: GetConcreteTypes);

                            EvaluateFunction?.Invoke(this, functionEvaluationEventArg);

                            if (functionEvaluationEventArg.FunctionReturnedValue)
                            {
                                stack.Push(functionEvaluationEventArg.Value);
                            }
                            else
                            {
                                throw new ExpressionEvaluatorSyntaxErrorException($"Function [{varFuncName}] unknown in expression : [{expr.Replace("\r", "").Replace("\n", "")}]");
                            }
                        }
                    }
                }
                else
                {
                    if (varFuncMatch.Groups["inObject"].Success)
                    {
                        if (stack.Count == 0 || stack.Peek() is ExpressionOperator)
                            throw new ExpressionEvaluatorSyntaxErrorException($"[{varFuncMatch.Value}] must follow an object.");

                        object obj = stack.Pop();
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
                            if (varFuncMatch.Groups["nullConditional"].Success && obj == null)
                            {
                                stack.Push(null);
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

                                    if (!OptionStaticProperiesGetActive && (flag & BindingFlags.Static) != 0)
                                        throw new ExpressionEvaluatorSyntaxErrorException($"[{objType}] object has no public Property or Field named \"{varFuncName}\".");
                                    if (!OptionInstanceProperiesGetActive && (flag & BindingFlags.Instance) != 0)
                                        throw new ExpressionEvaluatorSyntaxErrorException($"[{objType}] object has no public Property or Field named \"{varFuncName}\".");

                                    bool isDynamic = (flag & BindingFlags.Instance) != 0 && obj is IDynamicMetaObjectProvider && obj is IDictionary<string, object>;
                                    IDictionary<string, object> dictionaryObject = obj as IDictionary<string, object>;

                                    MemberInfo member = isDynamic ? null : objType?.GetProperty(varFuncName, flag);
                                    dynamic varValue = null;
                                    bool assign = true;

                                    if (member == null && !isDynamic)
                                        member = objType.GetField(varFuncName, flag);

                                    bool pushVarValue = true;

                                    if (isDynamic)
                                    {
                                        if (!varFuncMatch.Groups["assignationOperator"].Success || varFuncMatch.Groups["assignmentPrefix"].Success)
                                            varValue = dictionaryObject.ContainsKey(varFuncName) ? dictionaryObject[varFuncName] : null;
                                        else
                                            pushVarValue = false;
                                    }

                                    if (member == null && pushVarValue)
                                    {
                                        VariableEvaluationEventArg variableEvaluationEventArg = new VariableEvaluationEventArg(varFuncName, this, obj ?? keepObj, genericsTypes, GetConcreteTypes);

                                        EvaluateVariable?.Invoke(this, variableEvaluationEventArg);

                                        if (variableEvaluationEventArg.HasValue)
                                        {
                                            varValue = variableEvaluationEventArg.Value;
                                        }
                                    }

                                    if (varValue == null && pushVarValue)
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
                                            if (stack.Count > 1)
                                                throw new ExpressionEvaluatorSyntaxErrorException("The left part of an assignation must be a variable, a property or an indexer.");

                                            string rightExpression = expr.Substring(i);
                                            i = expr.Length;

                                            if (rightExpression.Trim().Equals(string.Empty))
                                                throw new ExpressionEvaluatorSyntaxErrorException("Right part is missing in assignation");

                                            if (varFuncMatch.Groups["assignmentPrefix"].Success)
                                            {
                                                ExpressionOperator op = operatorsDictionary[varFuncMatch.Groups["assignmentPrefix"].Value];

                                                varValue = operatorsEvaluations.Find(dict => dict.ContainsKey(op))[op](varValue, Evaluate(rightExpression));
                                            }
                                            else
                                            {
                                                varValue = Evaluate(rightExpression);
                                            }

                                            stack.Clear();
                                            stack.Push(varValue);
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
                                            else
                                            {
                                                if (valueTypeNestingTrace != null)
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
                            throw new ExpressionEvaluatorSyntaxErrorException($"[{objType}] object has no public Property or Member named \"{varFuncName}\".", ex);
                        }
                    }
                    else
                    {
                        VariablePreEvaluationEventArg variablePreEvaluationEventArg = new VariablePreEvaluationEventArg(varFuncName, this, genericTypes: genericsTypes, evaluateGenericTypes: GetConcreteTypes);

                        PreEvaluateVariable?.Invoke(this, variablePreEvaluationEventArg);

                        if (variablePreEvaluationEventArg.CancelEvaluation)
                        {
                            throw new ExpressionEvaluatorSyntaxErrorException($"Variable [{varFuncName}] unknown in expression : [{expr}]");
                        }
                        else if (variablePreEvaluationEventArg.HasValue)
                        {
                            stack.Push(variablePreEvaluationEventArg.Value);
                        }
                        else if (defaultVariables.TryGetValue(varFuncName, out object varValueToPush))
                        {
                            stack.Push(varValueToPush);
                        }
                        else if ((Variables.TryGetValue(varFuncName, out object cusVarValueToPush) || varFuncMatch.Groups["assignationOperator"].Success)
                            && (cusVarValueToPush == null || !TypesToBlock.Contains(cusVarValueToPush.GetType())))
                        {
                            stack.Push(cusVarValueToPush);

                            if (OptionVariableAssignationActive)
                            {
                                bool assign = true;

                                if (varFuncMatch.Groups["assignationOperator"].Success)
                                {
                                    if (stack.Count > 1)
                                        throw new ExpressionEvaluatorSyntaxErrorException("The left part of an assignation must be a variable, a property or an indexer.");

                                    string rightExpression = expr.Substring(i);
                                    i = expr.Length;

                                    if (rightExpression.Trim().Equals(string.Empty))
                                        throw new ExpressionEvaluatorSyntaxErrorException("Right part is missing in assignation");

                                    if (varFuncMatch.Groups["assignmentPrefix"].Success)
                                    {
                                        if (!Variables.ContainsKey(varFuncName))
                                            throw new ExpressionEvaluatorSyntaxErrorException($"The variable[{varFuncName}] do not exists.");

                                        ExpressionOperator op = operatorsDictionary[varFuncMatch.Groups["assignmentPrefix"].Value];

                                        cusVarValueToPush = operatorsEvaluations.Find(dict => dict.ContainsKey(op))[op](cusVarValueToPush, Evaluate(rightExpression));
                                    }
                                    else
                                    {
                                        cusVarValueToPush = Evaluate(rightExpression);
                                    }

                                    stack.Clear();
                                    stack.Push(cusVarValueToPush);
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
                                    Variables[varFuncName] = cusVarValueToPush;
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
                            string typeName = $"{varFuncName}{((i < expr.Length && expr.Substring(i)[0] == '?') ? "?" : "") }";
                            Type staticType = GetTypeByFriendlyName(typeName, genericsTypes);

                            if (staticType == null && OptionInlineNamespacesEvaluationActive)
                            {
                                int subIndex = 0;
                                Match namespaceMatch = varOrFunctionRegEx.Match(expr.Substring(i + subIndex));

                                while (staticType == null
                                    && namespaceMatch.Success
                                    && !namespaceMatch.Groups["sign"].Success
                                    && !namespaceMatch.Groups["assignationOperator"].Success
                                    && !namespaceMatch.Groups["postfixOperator"].Success
                                    && !namespaceMatch.Groups["isfunction"].Success
                                    && i + subIndex < expr.Length
                                    && !typeName.EndsWith("?"))
                                {
                                    subIndex += namespaceMatch.Length;
                                    typeName += $".{namespaceMatch.Groups["name"].Value}{((i + subIndex < expr.Length && expr.Substring(i + subIndex)[0] == '?') ? "?" : "") }";

                                    staticType = GetTypeByFriendlyName(typeName, namespaceMatch.Groups["isgeneric"].Value);

                                    if (staticType != null)
                                    {
                                        i += subIndex;
                                        break;
                                    }

                                    namespaceMatch = varOrFunctionRegEx.Match(expr.Substring(i + subIndex));
                                }
                            }

                            if (typeName.EndsWith("?") && staticType != null)
                                i++;

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
                                    throw new ExpressionEvaluatorSyntaxErrorException($"Variable [{varFuncName}] unknown in expression : [{expr}]");
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

        private bool EvaluateChar(string expr, string s, Stack<object> stack, ref int i)
        {
            if (!OptionCharEvaluationActive)
                return false;

            if (s.Equals("'"))
            {
                i++;

                if (expr.Substring(i, 1).Equals(@"\"))
                {
                    i++;
                    char escapedChar = expr[i];

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
                else if (expr.Substring(i, 1).Equals("'"))
                {
                    throw new ExpressionEvaluatorSyntaxErrorException("Empty literal character is not valid");
                }
                else
                {
                    stack.Push(expr[i]);
                    i++;
                }

                if (expr.Substring(i, 1).Equals("'"))
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

        private bool EvaluateTwoCharsOperators(string expr, Stack<object> stack, ref int i)
        {
            if (i < expr.Length - 1)
            {
                string op = expr.Substring(i, 2);
                if (operatorsDictionary.ContainsKey(op))
                {
                    stack.Push(operatorsDictionary[op]);
                    i++;
                    return true;
                }
            }

            return false;
        }

        private bool EvaluateParenthis(string expr, string s, Stack<object> stack, ref int i)
        {
            if (s.Equals(")"))
                throw new Exception($"To much ')' characters are defined in expression : [{expr}] : no corresponding '(' fund.");

            if (s.Equals("("))
            {
                i++;

                if (stack.Count > 0 && stack.Peek() is InternalDelegate)
                {
                    List<string> expressionsInParenthis = GetExpressionsBetweenParenthesesOrOtherImbricableBrackets(expr, ref i, true);

                    InternalDelegate lambdaDelegate = stack.Pop() as InternalDelegate;

                    stack.Push(lambdaDelegate(expressionsInParenthis.ConvertAll(arg => Evaluate(arg)).ToArray()));
                }
                else
                {
                    CorrectStackWithUnaryPlusOrMinusBeforeParenthisIfNecessary(stack);

                    List<string> expressionsInParenthis = GetExpressionsBetweenParenthesesOrOtherImbricableBrackets(expr, ref i, false);

                    stack.Push(Evaluate(expressionsInParenthis[0]));
                }

                return true;
            }

            return false;
        }

        private void CorrectStackWithUnaryPlusOrMinusBeforeParenthisIfNecessary(Stack<object> stack)
        {
            if (stack.Count > 0 && stack.Peek() is ExpressionOperator op && (op == ExpressionOperator.Plus || stack.Peek() is ExpressionOperator.Minus))
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

        private bool EvaluateIndexing(string expr, string s, Stack<object> stack, ref int i)
        {
            if (!OptionIndexingActive)
                return false;

            Match indexingBeginningMatch = indexingBeginningRegex.Match(expr.Substring(i));

            if (indexingBeginningMatch.Success)
            {
                StringBuilder innerExp = new StringBuilder();
                i += indexingBeginningMatch.Length;
                int bracketCount = 1;
                for (; i < expr.Length; i++)
                {
                    Match internalStringMatch = stringBeginningRegex.Match(expr.Substring(i));

                    if (internalStringMatch.Success)
                    {
                        string innerString = internalStringMatch.Value + GetCodeUntilEndOfString(expr.Substring(i + internalStringMatch.Length), internalStringMatch);
                        innerExp.Append(innerString);
                        i += innerString.Length - 1;
                    }
                    else
                    {
                        s = expr.Substring(i, 1);

                        if (s.Equals("[")) bracketCount++;

                        if (s.Equals("]"))
                        {
                            bracketCount--;
                            if (bracketCount == 0) break;
                        }
                        innerExp.Append(s);
                    }
                }

                if (bracketCount > 0)
                {
                    string beVerb = bracketCount == 1 ? "is" : "are";
                    throw new Exception($"{bracketCount} ']' character {beVerb} missing in expression : [{expr}]");
                }

                dynamic right = Evaluate(innerExp.ToString());
                ExpressionOperator op = indexingBeginningMatch.Length == 2 ? ExpressionOperator.IndexingWithNullConditional : ExpressionOperator.Indexing;
                dynamic left = stack.Pop();

                if (OptionForceIntegerNumbersEvaluationsAsDoubleByDefault && right is double && Regex.IsMatch(innerExp.ToString(), @"^\d+$"))
                    right = (int)right;

                Match assignationOrPostFixOperatorMatch = null;

                dynamic valueToPush = null;

                if (OptionIndexingAssignationActive && (assignationOrPostFixOperatorMatch = assignationOrPostFixOperatorRegex.Match(expr.Substring(i + 1))).Success)
                {
                    i += assignationOrPostFixOperatorMatch.Length + 1;

                    bool postFixOperator = assignationOrPostFixOperatorMatch.Groups["postfixOperator"].Success;
                    string exceptionContext = postFixOperator ? "++ or -- operator" : "an assignation";

                    if (stack.Count > 1)
                        throw new ExpressionEvaluatorSyntaxErrorException($"The left part of {exceptionContext} must be a variable, a property or an indexer.");

                    if (op == ExpressionOperator.IndexingWithNullConditional)
                        throw new ExpressionEvaluatorSyntaxErrorException($"Null coalescing is not usable left to {exceptionContext}");

                    if (postFixOperator)
                    {
                        if (left is IDictionary<string, object> dictionaryLeft)
                            valueToPush = assignationOrPostFixOperatorMatch.Groups["postfixOperator"].Value.Equals("++") ? dictionaryLeft[right]++ : dictionaryLeft[right]--;
                        else
                            valueToPush = assignationOrPostFixOperatorMatch.Groups["postfixOperator"].Value.Equals("++") ? left[right]++ : left[right]--;
                    }
                    else
                    {
                        string rightExpression = expr.Substring(i);
                        i = expr.Length;

                        if (rightExpression.Trim().Equals(string.Empty))
                            throw new ExpressionEvaluatorSyntaxErrorException("Right part is missing in assignation");

                        if (assignationOrPostFixOperatorMatch.Groups["assignmentPrefix"].Success)
                        {
                            ExpressionOperator prefixOp = operatorsDictionary[assignationOrPostFixOperatorMatch.Groups["assignmentPrefix"].Value];

                            valueToPush = operatorsEvaluations[0][op](left, right);

                            valueToPush = operatorsEvaluations.Find(dict => dict.ContainsKey(prefixOp))[prefixOp](valueToPush, Evaluate(rightExpression));
                        }
                        else
                        {
                            valueToPush = Evaluate(rightExpression);
                        }

                        if (left is IDictionary<string, object> dictionaryLeft)
                            dictionaryLeft[right] = valueToPush;
                        else
                            left[right] = valueToPush;

                        stack.Clear();
                    }
                }
                else
                {
                    valueToPush = operatorsEvaluations[0][op](left, right);
                }

                stack.Push(valueToPush);

                return true;
            }

            return false;
        }

        private bool EvaluateString(string expr, string s, string restOfExpression, Stack<object> stack, ref int i)
        {
            if (!OptionStringEvaluationActive)
                return false;

            Match stringBeginningMatch = stringBeginningRegex.Match(restOfExpression);

            if (stringBeginningMatch.Success)
            {
                bool isEscaped = stringBeginningMatch.Groups["escaped"].Success;
                bool isInterpolated = stringBeginningMatch.Groups["interpolated"].Success;

                i += stringBeginningMatch.Length;

                Regex stringRegexPattern = new Regex($"^[^{(isEscaped ? "" : @"\\")}{(isInterpolated ? "{}" : "")}\"]*");

                bool endOfString = false;

                StringBuilder resultString = new StringBuilder();

                while (!endOfString && i < expr.Length)
                {
                    Match stringMatch = stringRegexPattern.Match(expr.Substring(i, expr.Length - i));

                    resultString.Append(stringMatch.Value);
                    i += stringMatch.Length;

                    if (expr.Substring(i)[0] == '"')
                    {
                        endOfString = true;
                        stack.Push(resultString.ToString());
                    }
                    else if (expr.Substring(i)[0] == '{')
                    {
                        i++;

                        if (expr.Substring(i)[0] == '{')
                        {
                            resultString.Append("{");
                            i++;
                        }
                        else
                        {
                            StringBuilder innerExp = new StringBuilder();
                            int bracketCount = 1;
                            for (; i < expr.Length; i++)
                            {
                                if (i + 3 <= expr.Length && expr.Substring(i, 3).Equals("'\"'"))
                                {
                                    innerExp.Append("'\"'");
                                    i += 2;
                                }
                                else
                                {
                                    Match internalStringMatch = stringBeginningRegex.Match(expr.Substring(i));

                                    if (internalStringMatch.Success)
                                    {
                                        string innerString = internalStringMatch.Value + GetCodeUntilEndOfString(expr.Substring(i + internalStringMatch.Length), internalStringMatch);
                                        innerExp.Append(innerString);
                                        i += innerString.Length - 1;
                                    }
                                    else
                                    {
                                        s = expr.Substring(i, 1);

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
                                throw new Exception($"{bracketCount} '}}' character {beVerb} missing in expression : [{expr}]");
                            }
                            resultString.Append(Evaluate(innerExp.ToString()));
                        }
                    }
                    else if (expr.Substring(i, expr.Length - i)[0] == '}')
                    {
                        i++;

                        if (expr.Substring(i, expr.Length - i)[0] == '}')
                        {
                            resultString.Append("}");
                            i++;
                        }
                        else
                        {
                            throw new ExpressionEvaluatorSyntaxErrorException("A character '}' must be escaped in a interpolated string.");
                        }
                    }
                    else if (expr.Substring(i, expr.Length - i)[0] == '\\')
                    {
                        i++;

                        if (stringEscapedCharDict.TryGetValue(expr.Substring(i, expr.Length - i)[0], out string escapedString))
                        {
                            resultString.Append(escapedString);
                            i++;
                        }
                        else
                        {
                            throw new ExpressionEvaluatorSyntaxErrorException("There is no corresponding escaped character for \\" + expr.Substring(i, 1));
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

        private object ProcessStack(Stack<object> stack)
        {
            List<object> list = stack
                .Select(e => e is ValueTypeNestingTrace valueTypeNestingTrace ? valueTypeNestingTrace.Value : e)
                .ToList();

            operatorsEvaluations.ForEach((Dictionary<ExpressionOperator, Func<dynamic, dynamic, object>> operatorEvalutationsDict) =>
            {
                for (int i = list.Count - 1; i >= 0; i--)
                {
                    for (int opi = 0; opi < operatorEvalutationsDict.Keys.ToList().Count; opi++)
                    {
                        ExpressionOperator eOp = operatorEvalutationsDict.Keys.ToList()[opi];

                        if ((list[i] as ExpressionOperator?) == eOp)
                        {
                            if (rightOperandOnlyOperatorsEvaluationDictionary.ContainsKey(eOp))
                            {
                                list[i] = operatorEvalutationsDict[eOp](null, (dynamic)list[i - 1]);
                                list.RemoveAt(i - 1);
                                break;
                            }
                            else if (leftOperandOnlyOperatorsEvaluationDictionary.ContainsKey(eOp))
                            {
                                list[i] = operatorEvalutationsDict[eOp]((dynamic)list[i + 1], null);
                                list.RemoveAt(i + 1);
                                break;
                            }
                            else
                            {
                                list[i] = operatorEvalutationsDict[eOp]((dynamic)list[i + 1], (dynamic)list[i - 1]);
                                list.RemoveAt(i + 1);
                                list.RemoveAt(i - 1);
                                i -= 1;
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
                throw new ExpressionEvaluatorSyntaxErrorException("Syntax error. Check that no operator is missing");

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

        private delegate dynamic InternalDelegate(params dynamic[] args);
        private bool GetLambdaExpression(string expr, Stack<object> stack)
        {
            Match lambdaExpressionMatch = lambdaExpressionRegex.Match(expr);

            if (lambdaExpressionMatch.Success)
            {
                List<string> argsNames = lambdaArgRegex
                    .Matches(lambdaExpressionMatch.Groups["args"].Value)
                    .Cast<Match>().ToList()
                    .ConvertAll(argMatch => argMatch.Value);

                stack.Push(new InternalDelegate((object[] args) =>
                {
                    Dictionary<string, object> vars = new Dictionary<string, object>(Variables);

                    for (int a = 0; a < argsNames.Count || a < args.Length; a++)
                    {
                        vars[argsNames[a]] = args[a];
                    }

                    ExpressionEvaluator expressionEvaluator = new ExpressionEvaluator(vars);

                    string lambdaBody = lambdaExpressionMatch.Groups["expression"].Value.Trim();

                    if (inScript && lambdaBody.StartsWith("{") && lambdaBody.EndsWith("}"))
                        return expressionEvaluator.ScriptEvaluate(lambdaBody.Substring(1, lambdaBody.Length - 2));
                    else
                        return expressionEvaluator.Evaluate(lambdaExpressionMatch.Groups["expression"].Value);
                }));

                return true;
            }
            else
            {
                return false;
            }
        }

        private MethodInfo GetRealMethod(ref Type type, ref object obj, string func, BindingFlags flag, List<object> args, string genericsTypes = "")
        {
            MethodInfo methodInfo = null;
            List<object> modifiedArgs = new List<object>(args);

            if (OptionFluidPrefixingActive
                && (func.StartsWith("Fluid", StringComparisonForCasing)
                    || func.StartsWith("Fluent", StringComparisonForCasing)))
            {
                methodInfo = GetRealMethod(ref type, ref obj, func.Substring(func.StartsWith("Fluid", StringComparisonForCasing) ? 5 : 6), flag, modifiedArgs, genericsTypes);
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

            if (args.Contains(null))
            {
                methodInfo = type.GetMethod(func, flag);
            }
            else
            {
                methodInfo = type.GetMethod(func, flag, null, args.ConvertAll(arg => arg.GetType()).ToArray(), null);
            }

            if (methodInfo != null)
            {
                methodInfo = MakeConcreteMethodIfGeneric(methodInfo, genericsTypes);
            }
            else
            {
                List<MethodInfo> methodInfos = type.GetMethods(flag)
                .Where(m => m.Name.Equals(func, StringComparisonForCasing) && m.GetParameters().Length == modifiedArgs.Count)
                .ToList();

                for (int m = 0; m < methodInfos.Count && methodInfo == null; m++)
                {
                    methodInfos[m] = MakeConcreteMethodIfGeneric(methodInfos[m], genericsTypes);

                    bool parametersCastOK = true;

                    modifiedArgs = new List<object>(args);

                    for (int a = 0; a < modifiedArgs.Count; a++)
                    {
                        Type parameterType = methodInfos[m].GetParameters()[a].ParameterType;
                        string paramTypeName = parameterType.Name;

                        if (paramTypeName.StartsWith("Predicate")
                            && modifiedArgs[a] is InternalDelegate)
                        {
                            InternalDelegate led = modifiedArgs[a] as InternalDelegate;
                            modifiedArgs[a] = new Predicate<object>(o => (bool)(led(new object[] { o })));
                        }
                        else if (paramTypeName.StartsWith("Func")
                            && modifiedArgs[a] is InternalDelegate)
                        {
                            InternalDelegate led = modifiedArgs[a] as InternalDelegate;
                            DelegateEncaps de = new DelegateEncaps(led);
                            MethodInfo encapsMethod = de.GetType()
                                .GetMethod($"Func{parameterType.GetGenericArguments().Length - 1}")
                                .MakeGenericMethod(parameterType.GetGenericArguments());
                            modifiedArgs[a] = Delegate.CreateDelegate(parameterType, de, encapsMethod);
                        }
                        else if (paramTypeName.StartsWith("Converter")
                            && modifiedArgs[a] is InternalDelegate)
                        {
                            InternalDelegate led = modifiedArgs[a] as InternalDelegate;
                            modifiedArgs[a] = new Converter<object, object>(o => (led(new object[] { o })));
                        }
                        else
                        {
                            try
                            {
                                if (!methodInfos[m].GetParameters()[a].ParameterType.IsAssignableFrom(modifiedArgs[a].GetType()))
                                {
                                    modifiedArgs[a] = Convert.ChangeType(modifiedArgs[a], methodInfos[m].GetParameters()[a].ParameterType);
                                }
                            }
                            catch
                            {
                                parametersCastOK = false;
                            }
                        }
                    }

                    if (parametersCastOK)
                        methodInfo = methodInfos[m];
                }

                if (methodInfo != null)
                {
                    args.Clear();
                    args.AddRange(modifiedArgs);
                }
            }

            return methodInfo;
        }

        private MethodInfo MakeConcreteMethodIfGeneric(MethodInfo methodInfo, string genericsTypes = "")
        {
            if (methodInfo.IsGenericMethod)
            {
                if (genericsTypes.Equals(string.Empty))
                    return methodInfo.MakeGenericMethod(Enumerable.Repeat(typeof(object), methodInfo.GetGenericArguments().Length).ToArray());
                else
                    return methodInfo.MakeGenericMethod(GetConcreteTypes(genericsTypes));
            }

            return methodInfo;
        }

        private Type[] GetConcreteTypes(string genericsTypes)
        {
            return genericsDecodeRegex
                .Matches(genericsEndOnlyOneTrim.Replace(genericsTypes.TrimStart(' ', '<'), ""))
                .Cast<Match>()
                .Select(match => GetTypeByFriendlyName(match.Groups["name"].Value, match.Groups["isgeneric"].Value, true))
                .ToArray();
        }

        private BindingFlags DetermineInstanceOrStatic(ref Type objType, ref object obj, ref ValueTypeNestingTrace valueTypeNestingTrace)
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

        private string GetScriptBetweenCurlyBrackets(string parentScript, ref int index)
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

        private List<string> GetExpressionsBetweenParenthesesOrOtherImbricableBrackets(string expr, ref int i, bool checkSeparator, string separator = ",", string startChar = "(", string endChar = ")")
        {
            List<string> expressionsList = new List<string>();

            string s;
            string currentExpression = string.Empty;
            int bracketCount = 1;
            for (; i < expr.Length; i++)
            {
                string subExpr = expr.Substring(i);
                Match internalStringMatch = stringBeginningRegex.Match(subExpr);
                Match internalCharMatch = internalCharRegex.Match(subExpr);

                if (OptionStringEvaluationActive && internalStringMatch.Success)
                {
                    string innerString = internalStringMatch.Value + GetCodeUntilEndOfString(expr.Substring(i + internalStringMatch.Length), internalStringMatch);
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
                    s = expr.Substring(i, 1);

                    if (s.Equals(startChar))
                    {
                        bracketCount++;
                    }
                    else if (s.Equals("("))
                    {
                        i++;
                        currentExpression += "(" + GetExpressionsBetweenParenthesesOrOtherImbricableBrackets(expr, ref i, false, ",", "(", ")").SingleOrDefault() + ")";
                        continue;
                    }
                    else if (s.Equals("{"))
                    {
                        i++;
                        currentExpression += "{" + GetExpressionsBetweenParenthesesOrOtherImbricableBrackets(expr, ref i, false, ",", "{", "}").SingleOrDefault() + "}";
                        continue;
                    }
                    else if (s.Equals("["))
                    {
                        i++;
                        currentExpression += "[" + GetExpressionsBetweenParenthesesOrOtherImbricableBrackets(expr, ref i, false, ",", "[", "]").SingleOrDefault() + "]";
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
                throw new Exception($"{bracketCount} '{endChar}' character {beVerb} missing in expression : [{expr}]");
            }

            return expressionsList;
        }

        private bool DefaultFunctions(string name, List<string> args, out object result)
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
                result = ScriptEvaluate((string)Evaluate(args[0]));
            }
            else
            {
                result = null;
                functionExists = false;
            }

            return functionExists;
        }

        private Type GetTypeByFriendlyName(string typeName, string genericTypes = "", bool throwExceptionIfNotFound = false)
        {
            Type result = null;
            string formatedGenericTypes = string.Empty;
            bool isCached = false;
            try
            {
                typeName = typeName.Replace(" ", "").Replace("\t", "").Replace("\r", "").Replace("\n", "");
                genericTypes = genericTypes.Replace(" ", "").Replace("\t", "").Replace("\r", "").Replace("\n", "");

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
                    typeName = Regex.Replace(typeName, primaryTypesRegexPattern,
                        (Match match) => primaryTypesDict[OptionCaseSensitiveEvaluationActive ? match.Value : match.Value.ToLower()].ToString(), optionCaseSensitiveEvaluationActive ? RegexOptions.None : RegexOptions.IgnoreCase);

                    result = Type.GetType(typeName, false, !OptionCaseSensitiveEvaluationActive);
                }

                if (result == null)
                {
                    result = Types.Find(type => type.Name.Equals(typeName, StringComparisonForCasing));
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
            catch (ExpressionEvaluatorSyntaxErrorException)
            {
                throw;
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

        private static object ChangeType(object value, Type conversionType)
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
            return Convert.ChangeType(value, conversionType);
        }

        private string GetCodeUntilEndOfString(string subExpr, Match stringBeginningMatch)
        {
            StringBuilder stringBuilder = new StringBuilder();

            GetCodeUntilEndOfString(subExpr, stringBeginningMatch, ref stringBuilder);

            return stringBuilder.ToString();
        }

        private void GetCodeUntilEndOfString(string subExpr, Match stringBeginningMatch, ref StringBuilder stringBuilder)
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

        private string GetCodeUntilEndOfStringInterpolation(string subExpr)
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

        #region Utils private sub classes for parsing and interpretation

        private class ValueTypeNestingTrace
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

        private class DelegateEncaps
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

    public enum OptionOnNoReturnKeywordFoundInScriptAction
    {
        ReturnAutomaticallyLastEvaluatedExpression,
        ReturnNull,
        ThrowSyntaxException
    }

    #endregion

    #region ExpressionEvaluator linked public classes (specific Exceptions and EventArgs)

    public partial class ClassOrEnumType
    {
        public Type Type { get; set; }
    }

    public partial class ExpressionEvaluatorSyntaxErrorException : Exception
    {
        public ExpressionEvaluatorSyntaxErrorException() : base()
        { }

        public ExpressionEvaluatorSyntaxErrorException(string message) : base(message)
        { }
        public ExpressionEvaluatorSyntaxErrorException(string message, Exception innerException) : base(message, innerException)
        { }
    }

    public partial class ExpressionEvaluatorSecurityException : Exception
    {
        public ExpressionEvaluatorSecurityException() : base()
        { }

        public ExpressionEvaluatorSecurityException(string message) : base(message)
        { }
        public ExpressionEvaluatorSecurityException(string message, Exception innerException) : base(message, innerException)
        { }
    }

    public partial class VariableEvaluationEventArg : EventArgs
    {
        private readonly Func<string, Type[]> evaluateGenericTypes = null;
        private readonly string genericTypes = null;

        /// <summary>
        /// Constructor of the VariableEvaluationEventArg
        /// </summary>
        /// <param name="name">The name of the variable to Evaluate</param>
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
        /// if <c>true</c> the variable is affected, if <c>false</c> it means that the variable does not exist.
        /// </summary>
        public bool HasValue { get; set; } = false;

        /// <summary>
        /// In the case of on the fly instance property definition the instance of the object on which this Property is called.
        /// Otherwise is set to null.
        /// </summary>
        public object This { get; } = null;

        /// <summary>
        /// A reference on the current expression evaluator.
        /// </summary>
        public ExpressionEvaluator Evaluator { get; }

        /// <summary>
        /// Is <c>true</c> if some generic types are specified with &lt;&gt;.
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
        /// In the case where generic types are specified with &lt;&gt;
        /// Evaluate all types and return an array of types
        /// </summary>
        public Type[] EvaluateGenericTypes()
        {
            return evaluateGenericTypes?.Invoke(genericTypes) ?? new Type[0];
        }
    }

    public class VariablePreEvaluationEventArg : VariableEvaluationEventArg
    {
        public VariablePreEvaluationEventArg(string name, ExpressionEvaluator evaluator = null, object onInstance = null, string genericTypes = null, Func<string, Type[]> evaluateGenericTypes = null)
            : base(name, evaluator, onInstance, genericTypes, evaluateGenericTypes)
        { }

        /// <summary>
        /// If set to true cancel the evaluation of the current variable, field or property and throw an exception it does not exists
        /// </summary>
        public bool CancelEvaluation { get; set; } = false;
    }

    public partial class FunctionEvaluationEventArg : EventArgs
    {
        private readonly Func<string, object> evaluateFunc = null;
        private readonly Func<string, Type[]> evaluateGenericTypes = null;
        private readonly string genericTypes = null;

        public FunctionEvaluationEventArg(string name, Func<string, object> evaluateFunc, List<string> args = null, ExpressionEvaluator evaluator = null, object onInstance = null, string genericTypes = null, Func<string, Type[]> evaluateGenericTypes = null)
        {
            Name = name;
            Args = args ?? new List<string>();
            this.evaluateFunc = evaluateFunc;
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
            return Args.ConvertAll(arg => evaluateFunc(arg)).ToArray();
        }

        /// <summary>
        /// Get the value of the function's arg at the specified index
        /// </summary>
        /// <param name="index">The index of the function's arg to evaluate</param>
        /// <returns>The evaluated arg</returns>
        public object EvaluateArg(int index)
        {
            return evaluateFunc(Args[index]);
        }

        /// <summary>
        /// Get the value of the function's arg at the specified index
        /// </summary>
        /// <typeparam name="T">The type of the result to get. (Do a cast)</typeparam>
        /// <param name="index">The index of the function's arg to evaluate</param>
        /// <returns>The evaluated arg casted in the specified type</returns>
        public T EvaluateArg<T>(int index)
        {
            return (T)evaluateFunc(Args[index]);
        }

        /// <summary>
        /// The name of the variable to Evaluate
        /// </summary>
        public string Name { get; }

        private object returnValue = null;

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
        /// if <c>true</c> the function evaluation has been done, if <c>false</c> it means that the function does not exist.
        /// </summary>
        public bool FunctionReturnedValue { get; set; } = false;

        /// <summary>
        /// In the case of on the fly instance method definition the instance of the object on which this method (function) is called.
        /// Otherwise is set to null.
        /// </summary>
        public object This { get; } = null;

        /// <summary>
        /// A reference on the current expression evaluator.
        /// </summary>
        public ExpressionEvaluator Evaluator { get; }

        /// <summary>
        /// Is <c>true</c> if some generic types are specified with &lt;&gt;.
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
        /// In the case where generic types are specified with &lt;&gt;
        /// Evaluate all types and return an array of types
        /// </summary>
        public Type[] EvaluateGenericTypes()
        {
            return evaluateGenericTypes?.Invoke(genericTypes) ?? new Type[0];
        }
    }

    public class FunctionPreEvaluationEventArg : FunctionEvaluationEventArg
    {
        public FunctionPreEvaluationEventArg(string name, Func<string, object> evaluateFunc, List<string> args = null, ExpressionEvaluator evaluator = null, object onInstance = null, string genericTypes = null, Func<string, Type[]> evaluateGenericTypes = null)
            : base(name, evaluateFunc, args, evaluator, onInstance, genericTypes, evaluateGenericTypes)
        { }

        /// <summary>
        /// If set to true cancel the evaluation of the current function or method and throw an exception that the function does not exists
        /// </summary>
        public bool CancelEvaluation { get; set; } = false;
    }

    #endregion
}
