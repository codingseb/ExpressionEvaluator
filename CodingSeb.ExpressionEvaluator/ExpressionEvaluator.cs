/******************************************************************************************************
    Title : ExpressionEvaluator (https://github.com/codingseb/ExpressionEvaluator)
    Version : 1.3.0.0 
    (if last digit not zero version is an intermediate version and can be unstable)

    Author : Coding Seb
    Licence : MIT (https://github.com/codingseb/ExpressionEvaluator/blob/master/LICENSE.md)
*******************************************************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace CodingSeb.ExpressionEvaluator
{
    /// <summary>
    /// This class allow to evaluate a string math or pseudo C# expression 
    /// </summary>
    public class ExpressionEvaluator
    {
        #region Regex declarations

        private static readonly string diactitics = "áàâãåǎăāąæéèêëěēĕėęěìíîïīĭįĳóôõöōŏőøðœùúûüǔũūŭůűųýþÿŷıćĉċčçďđĝğġģĥħĵķĺļľŀłńņňŋñŕŗřśŝşšţťŧŵźżžÁÀÂÃÅǍĂĀĄÆÉÈÊËĚĒĔĖĘĚÌÍÎÏĪĬĮĲÓÔÕÖŌŎŐØÐŒÙÚÛÜǓŨŪŬŮŰŲÝÞŸŶIĆĈĊČÇĎĐĜĞĠĢĤĦĴĶĹĻĽĿŁŃŅŇŊÑŔŖŘŚŜŞŠŢŤŦŴŹŻŽß";
        private static readonly string diactiticsKeywordsRegexPattern = "a-zA-Z_" + diactitics;

        private static readonly Regex varOrFunctionRegEx = new Regex(@"^((?<sign>[+-])|(?<inObject>(?<nullConditional>[?])?\.)?)(?<name>["+ diactiticsKeywordsRegexPattern + @"][" + diactiticsKeywordsRegexPattern + @"0-9]*)\s*((?<assignationOperator>(?<assignmentPrefix>[+\-*/%&|^]|<<|>>)?=(?![=>]))|(?<postfixOperator>([+][+]|--)(?![" + diactiticsKeywordsRegexPattern + @"0-9]))|((?<isgeneric>[<](?>[^<>]+|(?<gentag>[<])|(?<-gentag>[>]))*(?(gentag)(?!))[>])?(?<isfunction>[(])?))", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex numberRegex = new Regex(@"^(?<sign>[+-])?\d+(?<hasdecimal>\.?\d+(e[+-]?\d+)?)?(?<type>ul|[fdulm])?", RegexOptions.IgnoreCase);
        private static readonly Regex stringBeginningRegex = new Regex("^(?<interpolated>[$])?(?<escaped>[@])?[\"]");
        private static readonly Regex internalCharRegex = new Regex(@"^['](\\[']|[^'])*[']");
        private static readonly Regex castRegex = new Regex(@"^\(\s*(?<typeName>[" + diactiticsKeywordsRegexPattern + @"][" + diactiticsKeywordsRegexPattern + @"0-9\.\[\]<>]*[?]?)\s*\)");
        private static readonly Regex indexingBeginningRegex = new Regex(@"^[?]?\[");
        private static readonly Regex assignationOrPostFixOperatorRegex = new Regex(@"^\s*((?<assignmentPrefix>[+\-*/%&|^]|<<|>>)?=(?![=>])|(?<postfixOperator>([+][+]|--)(?![" + diactiticsKeywordsRegexPattern + @"0-9])))");
        private static readonly Regex endOfStringWithDollar = new Regex("^[^\"{]*[\"{]");
        private static readonly Regex endOfStringWithoutDollar = new Regex("^[^\"]*[\"]");
        private static readonly Regex endOfStringInterpolationRegex = new Regex("^[^}\"]*[}\"]");
        private static readonly Regex stringBeginningForEndBlockRegex = new Regex("[$]?[@]?[\"]$");
        private static readonly Regex lambdaExpressionRegex = new Regex(@"^\s*(?<args>(\s*[(]\s*([" + diactiticsKeywordsRegexPattern + @"][" + diactiticsKeywordsRegexPattern + @"0-9]*\s*([,]\s*[" + diactiticsKeywordsRegexPattern + @"][" + diactiticsKeywordsRegexPattern + @"0-9]*\s*)*)?[)])|[" + diactiticsKeywordsRegexPattern + @"][" + diactiticsKeywordsRegexPattern + @"0-9]*)\s*=>(?<expression>.*)$", RegexOptions.Singleline);
        private static readonly Regex lambdaArgRegex = new Regex(@"[" + diactiticsKeywordsRegexPattern + @"][" + diactiticsKeywordsRegexPattern + @"0-9]*");

        private static readonly string instanceCreationWithNewKeywordRegexPattern = @"^new\s+(?<name>[" + diactiticsKeywordsRegexPattern + @"][" + diactiticsKeywordsRegexPattern + @"0-9.]*)\s*(?<isgeneric>[<](?>[^<>]+|(?<gentag>[<])|(?<-gentag>[>]))*(?(gentag)(?!))[>])?(?<isfunction>[(])?";
        private Regex instanceCreationWithNewKeywordRegex = new Regex(instanceCreationWithNewKeywordRegexPattern);
        private static readonly string primaryTypesRegexPattern = @"(?<=^|[^" + diactiticsKeywordsRegexPattern + @"])(?<primaryType>object|string|bool[?]?|byte[?]?|char[?]?|decimal[?]?|double[?]?|short[?]?|int[?]?|long[?]?|sbyte[?]?|float[?]?|ushort[?]?|uint[?]?|void)(?=[^a-zA-Z_]|$)";
        private Regex primaryTypesRegex = new Regex(primaryTypesRegexPattern);

        // To remove comments in scripts based on https://stackoverflow.com/questions/3524317/regex-to-strip-line-comments-from-c-sharp/3524689#3524689
        private static readonly string blockComments = @"/\*(.*?)\*/";
        private static readonly string lineComments = @"//[^\r\n]*";
        private static readonly string stringsIgnore = @"""((\\[^\n]|[^""\n])*)""";
        private static readonly string verbatimStringsIgnore = @"@(""[^""]*"")+";
        private static readonly Regex removeCommentsRegex = new Regex($"{blockComments}|{lineComments}|{stringsIgnore}|{verbatimStringsIgnore}", RegexOptions.Singleline);
        private static readonly Regex newLineCharsRegex = new Regex(@"\r\n|\r|\n");

        // For script only
        private static readonly Regex blockKeywordsBeginningRegex = new Regex(@"^\s*(?<keyword>while|for|foreach|if|else\s+if|catch)\s*[(]", RegexOptions.IgnoreCase);
        private static readonly Regex foreachParenthisEvaluationRegex = new Regex(@"^\s*(?<variableName>[" + diactiticsKeywordsRegexPattern + @"][" + diactiticsKeywordsRegexPattern + @"0-9]*)\s+(?<in>in)\s+(?<collection>.*)", RegexOptions.IgnoreCase);
        private static readonly Regex blockKeywordsWithoutParenthesesBeginningRegex = new Regex(@"^\s*(?<keyword>else|do|try|finally)(?![" + diactiticsKeywordsRegexPattern + @"0-9])", RegexOptions.IgnoreCase);
        private static readonly Regex blockBeginningRegex = new Regex(@"^\s*[{]");
        private static readonly Regex returnKeywordRegex = new Regex(@"^return(\s+|\()", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        private static readonly Regex nextIsEndOfExpressionRegex = new Regex(@"^\s*[;]");

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

        private static Dictionary<string, Func<string, object>> numberSuffixToParse = new Dictionary<string, Func<string, object>>(StringComparer.OrdinalIgnoreCase) // Always Case insensitive, like in C#
        {
            { "f", number => float.Parse(number, NumberStyles.Any, CultureInfo.InvariantCulture) },
            { "d", number => double.Parse(number, NumberStyles.Any, CultureInfo.InvariantCulture) },
            { "u", number => uint.Parse(number, NumberStyles.Any, CultureInfo.InvariantCulture) },
            { "l", number => long.Parse(number, NumberStyles.Any, CultureInfo.InvariantCulture) },
            { "ul", number => ulong.Parse(number, NumberStyles.Any, CultureInfo.InvariantCulture) },
            { "m", number => decimal.Parse(number, NumberStyles.Any, CultureInfo.InvariantCulture) }
        };

        private static Dictionary<char, string> stringEscapedCharDict = new Dictionary<char, string>()
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

        private static Dictionary<char, char> charEscapedCharDict = new Dictionary<char, char>()
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
            { "&", ExpressionOperator.LogicalAnd },
            { "|", ExpressionOperator.LogicalOr },
            { "^", ExpressionOperator.LogicalXor },
            { "<<", ExpressionOperator.ShiftBitsLeft },
            { ">>", ExpressionOperator.ShiftBitsRight },
            { "??", ExpressionOperator.NullCoalescing },
        };

        private static Dictionary<ExpressionOperator, bool> leftOperandOnlyOperatorsEvaluationDictionary = new Dictionary<ExpressionOperator, bool>()
        {
        };

        private static Dictionary<ExpressionOperator, bool> rightOperandOnlyOperatorsEvaluationDictionary = new Dictionary<ExpressionOperator, bool>()
        {
            {ExpressionOperator.LogicalNegation, true },
            {ExpressionOperator.UnaryPlus, true },
            {ExpressionOperator.UnaryMinus, true }
        };

        private static List<Dictionary<ExpressionOperator, Func<dynamic, dynamic, object>>> operatorsEvaluations =
            new List<Dictionary<ExpressionOperator, Func<dynamic, dynamic, object>>>()
        {
            new Dictionary<ExpressionOperator, Func<dynamic, dynamic, object>>()
            {
                {ExpressionOperator.Indexing, (dynamic left, dynamic right) => left is IDictionary<string,object> dictionaryLeft ? dictionaryLeft[right] : left[right] },
                {ExpressionOperator.IndexingWithNullConditional, (dynamic left, dynamic right) => left is IDictionary<string,object> dictionaryLeft ? dictionaryLeft[right] : left?[right] },
            },
            new Dictionary<ExpressionOperator, Func<dynamic, dynamic, object>>()
            {
                {ExpressionOperator.UnaryPlus, (dynamic left, dynamic right) => +right },
                {ExpressionOperator.UnaryMinus, (dynamic left, dynamic right) => -right },
                {ExpressionOperator.LogicalNegation, (dynamic left, dynamic right) => !right },
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
                {ExpressionOperator.Is, (dynamic left, dynamic right) => left != null && (((ClassOrTypeName)right).Type).IsAssignableFrom(left.GetType()) },
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

                    if (argValue is ClassOrTypeName classOrTypeName)
                        return Activator.CreateInstance(classOrTypeName.Type);
                    else
                        return null;
                }
            },
            //{ "if", (self, args) => (bool)self.Evaluate(args[0]) ? self.Evaluate(args[1]) : self.Evaluate(args[2]) },
            { "in", (self, args) => args.Skip(1).ToList().ConvertAll(arg => self.Evaluate(arg)).Contains(self.Evaluate(args[0])) },
            { "List", (self, args) => args.ConvertAll(arg => self.Evaluate(arg)) },
            { "Max", (self, args) => args.ConvertAll(arg => Convert.ToDouble(self.Evaluate(arg))).Max() },
            { "Min", (self, args) => args.ConvertAll(arg => Convert.ToDouble(self.Evaluate(arg))).Min() },
            { "new", (self, args) =>
                {
                    List<object> cArgs = args.ConvertAll(arg => self.Evaluate(arg));
                    return Activator.CreateInstance((cArgs[0] as ClassOrTypeName).Type, cArgs.Skip(1).ToArray());
                }
            },
            { "Round", (self, args) =>
                {
                    if(args.Count == 3)
                        return Math.Round(Convert.ToDouble(self.Evaluate(args[0])), (int)(self.Evaluate(args[1])), (MidpointRounding)self.Evaluate(args[2]));
                    else if(args.Count == 2)
                    {
                        object arg2 = self.Evaluate(args[1]);

                        if(arg2 is MidpointRounding midpointRounding)
                            return Math.Round(Convert.ToDouble(self.Evaluate(args[0])), midpointRounding);
                        else
                            return Math.Round(Convert.ToDouble(self.Evaluate(args[0])), (int)arg2);
                    }
                    else if(args.Count == 1)
                        return Math.Round(Convert.ToDouble(self.Evaluate(args[0])));
                    else
                        throw new ArgumentException();

                }
            },
            { "Sign", (self, args) => Math.Sign(Convert.ToDouble(self.Evaluate(args[0]))) },
            { "typeof", (self, args) => ((ClassOrTypeName)self.Evaluate(args[0])).Type },
        };

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
                Variables = Variables;
                operatorsDictionary = new Dictionary<string, ExpressionOperator>(operatorsDictionary, StringComparerForCasing);
                defaultVariables = new Dictionary<string, object>(defaultVariables, StringComparerForCasing);
                simpleDoubleMathFuncsDictionary = new Dictionary<string, Func<double, double>>(simpleDoubleMathFuncsDictionary, StringComparerForCasing);
                doubleDoubleMathFuncsDictionary = new Dictionary<string, Func<double, double, double>>(doubleDoubleMathFuncsDictionary, StringComparerForCasing);
                complexStandardFuncsDictionary = new Dictionary<string, Func<ExpressionEvaluator, List<string>, object>>(complexStandardFuncsDictionary, StringComparerForCasing);
                instanceCreationWithNewKeywordRegex = new Regex(instanceCreationWithNewKeywordRegexPattern, (optionCaseSensitiveEvaluationActive ? RegexOptions.None : RegexOptions.IgnoreCase));
                primaryTypesRegex = new Regex(primaryTypesRegexPattern, (optionCaseSensitiveEvaluationActive ? RegexOptions.None : RegexOptions.IgnoreCase));
            }
        }

        private StringComparer StringComparerForCasing
        {
            get
            {
                return OptionCaseSensitiveEvaluationActive ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase;
            }
        }

        /// <summary>
        /// if <c>true</c> allow to add the prefix Fluid or Fluent before void methods names to return back the instance on which the method is call.
        /// if <c>false</c> unactive this functionality.
        /// By default : true
        /// </summary>
        public bool OptionFluidPrefixingActive { get; set; } = true;

        /// <summary>
        /// if <c>true</c> allow to create instance of object with the C# syntax new ClassName(...).
        /// if <c>false</c> unactive this functionality.
        /// By default : true
        /// </summary>
        public bool OptionNewKeywordEvaluationActive { get; set; } = true;

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
                    complexStandardFuncsDictionary["new"] = newMethodMem;
                else if (!value && complexStandardFuncsDictionary.ContainsKey("new"))
                {
                    newMethodMem = complexStandardFuncsDictionary["new"];
                    complexStandardFuncsDictionary.Remove("new");
                }
            }
        }

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

        private Dictionary<string, object> variables = new Dictionary<string, object>(StringComparer.Ordinal);

        /// <summary>
        /// The Values of the variable use in the expressions
        /// </summary>
        public Dictionary<string, object> Variables
        {
            get { return variables; }
            set { variables = value == null ? new Dictionary<string, object>() : new Dictionary<string, object>(value, StringComparerForCasing); }
        }

        /// <summary>
        /// Is Fired when no internal variable is found for a variable name.
        /// Allow to define a variable and the corresponding value on the fly.
        /// </summary>
        public event EventHandler<VariableEvaluationEventArg> EvaluateVariable;

        /// <summary>
        /// Is Fired when no internal function is found for a variable name.
        /// Allow to define a function and the corresponding value on the fly.
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
        }

        /// <summary>
        /// Constructor with variable initialize
        /// </summary>
        /// <param name="variables">The Values of the variable use in the expressions</param>
        public ExpressionEvaluator(Dictionary<string, object> variables) : this()
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
                string baseExpression = expression;
                object result = null;

                expression = expression.Trim();

                string expressionToTest = OptionCaseSensitiveEvaluationActive ? expression : expression.ToLower();

                if (expressionToTest.Equals("break"))
                {
                    isBreak = true;
                    return lastResult;
                }

                if (expressionToTest.Equals("continue"))
                {
                    isContinue = true;
                    return lastResult;
                }

                if(expressionToTest.StartsWith("throw "))
                {
                    throw Evaluate(expressionToTest.Remove(0, 6)) as Exception;
                }

                expression = returnKeywordRegex.Replace(expression, match =>
                {
                    if (OptionCaseSensitiveEvaluationActive && !match.Value.StartsWith("return"))
                        return match.Value;

                    isReturn = true;
                    return match.Value.Contains("(") ? "(" : string.Empty;
                });

                result = Evaluate(expression);

                return result;
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
                    GetExpressionsBetweenParenthis(script, ref index, false);
                }
                else if (script[index] == '{')
                {
                    index++;
                    GetScriptBetweenCurlyBrackets(script, ref index);
                }
                else
                    parsed = false;

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
                if(tryStatementsList.Count > 0)
                {
                    if(tryStatementsList.Count == 1)
                    {
                        throw new ExpressionEvaluatorSyntaxErrorException("a try statement need at least one catch or one finally statement.");
                    }

                    try
                    {
                        lastResult = ScriptEvaluate(tryStatementsList[0][0], ref isReturn, ref isBreak, ref isContinue);
                    }
                    catch(Exception exception)
                    {
                        bool atLeasOneCatch = false;

                        foreach (List<string> catchStatement in tryStatementsList.Skip(1).TakeWhile(e => e[0].Equals("catch")))
                        {
                            if (catchStatement[1] != null)
                            {
                                string[] exceptionVariable = catchStatement[1].ToString().Trim().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                                string exceptionName = exceptionVariable[0];

                                if (exceptionVariable.Length >= 2)
                                {
                                    if (!((ClassOrTypeName)Evaluate(exceptionVariable[0])).Type.IsAssignableFrom(exception.GetType()))
                                        continue;

                                    exceptionName = exceptionVariable[1];
                                }

                                Variables[exceptionName] = exception;
                            }

                            lastResult = ScriptEvaluate(catchStatement[2], ref isReturn, ref isBreak, ref isContinue);
                            atLeasOneCatch = true;
                            break;
                        }

                        if(!atLeasOneCatch)
                        {
                            throw exception;
                        }
                    }
                    finally
                    {
                        if(tryStatementsList.Last()[0].Equals("finally"))
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
                    List<string> keywordAttributes = blockKeywordsBeginingMatch.Success ? GetExpressionsBetweenParenthis(script, ref i, true, ";") : null;

                    if (blockKeywordsBeginingMatch.Success)
                        i++;

                    if (!OptionCaseSensitiveEvaluationActive)
                        keyword = keyword.ToLower();

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

                    if (keyword.Equals("elseif"))
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
                    else if (keyword.Equals("else"))
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
                    else if (keyword.Equals("catch"))
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
                    else if (keyword.Equals("finally"))
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

                        if (keyword.Equals("if"))
                        {
                            ifElseStatementsList.Add(new List<string>() { keywordAttributes[0], subScript });
                            ifBlockEvaluatedState = IfBlockEvaluatedState.If;
                            tryBlockEvaluatedState = TryBlockEvaluatedState.NoBlockEvaluated;
                        }
                        else if(keyword.Equals("try"))
                        {
                            tryStatementsList.Add(new List<string>() { subScript });
                            ifBlockEvaluatedState = IfBlockEvaluatedState.NoBlockEvaluated;
                            tryBlockEvaluatedState = TryBlockEvaluatedState.Try;
                        }
                        else if (keyword.Equals("do"))
                        {
                            if ((blockKeywordsBeginingMatch = blockKeywordsBeginningRegex.Match(script.Substring(i))).Success
                                && blockKeywordsBeginingMatch.Groups["keyword"].Value.ManageCasing(OptionCaseSensitiveEvaluationActive).Equals("while"))
                            {
                                i += blockKeywordsBeginingMatch.Length;
                                keywordAttributes = GetExpressionsBetweenParenthis(script, ref i, true, ";");

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
                                            continue;
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
                        else if (keyword.Equals("while"))
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
                                    continue;
                                }
                            }
                        }
                        else if (keyword.Equals("for"))
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
                        else if (keyword.Equals("foreach"))
                        {
                            Match foreachParenthisEvaluationMatch = foreachParenthisEvaluationRegex.Match(keywordAttributes[0]);

                            if (!foreachParenthisEvaluationMatch.Success)
                            {
                                throw new ExpressionEvaluatorSyntaxErrorException("wrong foreach syntax");
                            }
                            else if (!foreachParenthisEvaluationMatch.Groups["in"].Value.ManageCasing(OptionCaseSensitiveEvaluationActive).Equals("in"))
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
                                        continue;
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
                                GetExpressionsBetweenParenthis(restOfExpression, ref j, false);
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
                        throw new ExpressionEvaluatorSyntaxErrorException($"Invalid character [{((int)s[0])}:{s}]");
                    }
                }
            }

            return ProcessStack(stack);
        }

        #endregion

        #region Sub parts evaluate methods (private)

        private bool EvaluateCast(string restOfExpression, Stack<object> stack, ref int i)
        {
            Match castMatch = castRegex.Match(restOfExpression);

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
            Match numberMatch = numberRegex.Match(restOfExpression);

            if (numberMatch.Success
                && (!numberMatch.Groups["sign"].Success
            || stack.Count == 0
            || stack.Peek() is ExpressionOperator))
            {
                i += numberMatch.Length;
                i--;

                if (numberMatch.Groups["type"].Success)
                {
                    string type = numberMatch.Groups["type"].Value;
                    string numberNoType = numberMatch.Value.Replace(type, string.Empty);

                    if (numberSuffixToParse.TryGetValue(type, out Func<string, object> parseFunc))
                    {
                        stack.Push(parseFunc(numberNoType));
                    }
                }
                else
                {
                    if (numberMatch.Groups["hasdecimal"].Success)
                    {
                        stack.Push(double.Parse(numberMatch.Value, NumberStyles.Any, CultureInfo.InvariantCulture));
                    }
                    else
                    {
                        stack.Push(int.Parse(numberMatch.Value, NumberStyles.Any, CultureInfo.InvariantCulture));
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

            Match instanceCreationMatch = instanceCreationWithNewKeywordRegex.Match(restOfExpression);

            if (instanceCreationMatch.Success &&
                (stack.Count == 0
                || stack.Peek() is ExpressionOperator))
            {
                string completeName = instanceCreationMatch.Groups["name"].Value;

                i += instanceCreationMatch.Length;

                if (!instanceCreationMatch.Groups["isfunction"].Success)
                    throw new ExpressionEvaluatorSyntaxErrorException($"No '(' found after {instanceCreationMatch.Value}");

                List<string> constructorArgs = GetExpressionsBetweenParenthis(expr, ref i, true);

                Type type = GetTypeByFriendlyName(completeName, true);

                if (type == null)
                    throw new ExpressionEvaluatorSyntaxErrorException($"type or class {completeName} is unknown");

                List<object> cArgs = constructorArgs.ConvertAll(arg => Evaluate(arg));
                stack.Push(Activator.CreateInstance(type, cArgs.ToArray()));

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

                i += varFuncMatch.Length;

                if (varFuncMatch.Groups["isfunction"].Success)
                {
                    List<string> funcArgs = GetExpressionsBetweenParenthis(expr, ref i, true);
                    if (varFuncMatch.Groups["inObject"].Success)
                    {
                        if (stack.Count == 0 || stack.Peek() is ExpressionOperator)
                        {
                            throw new ExpressionEvaluatorSyntaxErrorException($"[{varFuncMatch.Value})] must follow an object.");
                        }
                        else
                        {
                            object obj = stack.Pop();
                            Type objType = null;

                            if (obj != null && TypesToBlock.Contains(obj.GetType()))
                                throw new ExpressionEvaluatorSecurityException($"{obj.GetType().FullName} type is blocked");
                            else if (obj is Type staticType && TypesToBlock.Contains(staticType))
                                throw new ExpressionEvaluatorSecurityException($"{staticType.FullName} type is blocked");
                            else if (obj is ClassOrTypeName classOrType && TypesToBlock.Contains(classOrType.Type))
                                throw new ExpressionEvaluatorSecurityException($"{classOrType.Type} type is blocked");

                            try
                            {
                                if (varFuncMatch.Groups["nullConditional"].Success && obj == null)
                                {
                                    stack.Push(null);
                                }
                                else
                                {
                                    FunctionEvaluationEventArg functionEvaluationEventArg = new FunctionEvaluationEventArg(varFuncName, Evaluate, funcArgs, obj);

                                    EvaluateFunction?.Invoke(this, functionEvaluationEventArg);

                                    if (functionEvaluationEventArg.FunctionReturnedValue)
                                    {
                                        stack.Push(functionEvaluationEventArg.Value);
                                    }
                                    else
                                    {
                                        List<object> oArgs = funcArgs.ConvertAll(arg => Evaluate(arg));
                                        BindingFlags flag = DetermineInstanceOrStatic(ref objType, ref obj);

                                        if (!OptionStaticMethodsCallActive && flag.HasFlag(BindingFlags.Static))
                                            throw new ExpressionEvaluatorSyntaxErrorException($"[{objType.ToString()}] object has no Method named \"{varFuncName}\".");
                                        if (!OptionInstanceMethodsCallActive && flag.HasFlag(BindingFlags.Instance))
                                            throw new ExpressionEvaluatorSyntaxErrorException($"[{objType.ToString()}] object has no Method named \"{varFuncName}\".");

                                        // Standard Instance or public method find
                                        MethodInfo methodInfo = GetRealMethod(ref objType, ref obj, varFuncName, flag, oArgs);

                                        // if not found check if obj is an expandoObject or similar
                                        if (obj is IDynamicMetaObjectProvider && obj is IDictionary<string, object> dictionaryObject && (dictionaryObject[varFuncName] is InternalDelegate || dictionaryObject[varFuncName] is Delegate))
                                        {
                                            if (dictionaryObject[varFuncName] is InternalDelegate internalDelegate)
                                                stack.Push(internalDelegate(oArgs.ToArray()));
                                            else
                                                stack.Push((dictionaryObject[varFuncName] as Delegate).DynamicInvoke(oArgs.ToArray()));
                                        }
                                        else if(objType.GetProperty(varFuncName, InstanceBindingFlag) is PropertyInfo instancePropertyInfo && 
                                            (instancePropertyInfo.PropertyType.IsSubclassOf(typeof(Delegate)) || instancePropertyInfo.PropertyType == typeof(Delegate)))
                                        {
                                            stack.Push((instancePropertyInfo.GetValue(obj) as Delegate).DynamicInvoke(oArgs.ToArray()));
                                        }
                                        else
                                        {
                                            // if not found try to Find extension methods.
                                            if (methodInfo == null && obj != null)
                                            {
                                                oArgs.Insert(0, obj);
                                                objType = obj.GetType();
                                                obj = null;
                                                for (int e = 0; e < StaticTypesForExtensionsMethods.Count && methodInfo == null; e++)
                                                {
                                                    Type type = StaticTypesForExtensionsMethods[e];
                                                    methodInfo = GetRealMethod(ref type, ref obj, varFuncName, StaticBindingFlag, oArgs);
                                                }
                                            }

                                            if (methodInfo != null)
                                            {
                                                stack.Push(methodInfo.Invoke(obj, oArgs.ToArray()));
                                            }
                                            else if (objType.GetProperty(varFuncName, StaticBindingFlag) is PropertyInfo staticPropertyInfo &&
                                            (staticPropertyInfo.PropertyType.IsSubclassOf(typeof(Delegate)) || staticPropertyInfo.PropertyType == typeof(Delegate)))
                                            {
                                                stack.Push((staticPropertyInfo.GetValue(obj) as Delegate).DynamicInvoke(oArgs.ToArray()));
                                            }
                                            else
                                            {
                                                throw new ExpressionEvaluatorSyntaxErrorException($"[{objType.ToString()}] object has no Method named \"{varFuncName}\".");
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
                                throw new ExpressionEvaluatorSyntaxErrorException($"The call of the method \"{varFuncName}\" on type [{objType.ToString()}] generate this error : {(ex.InnerException?.Message ?? ex.Message)}", ex);
                            }
                        }
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
                        FunctionEvaluationEventArg functionEvaluationEventArg = new FunctionEvaluationEventArg(varFuncName, Evaluate, funcArgs);

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
                else
                {
                    if (defaultVariables.TryGetValue(varFuncName, out object varValueToPush))
                    {
                        stack.Push(varValueToPush);
                    }
                    else if ((Variables.TryGetValue(varFuncName, out dynamic cusVarValueToPush)
                            || (!varFuncMatch.Groups["inObject"].Success && varFuncMatch.Groups["assignationOperator"].Success))
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
                                cusVarValueToPush = varFuncMatch.Groups["postfixOperator"].Value.Equals("++") ? cusVarValueToPush + 1 : cusVarValueToPush - 1;
                            else
                                assign = false;

                            if (assign)
                                Variables[varFuncName] = cusVarValueToPush;
                        }
                        else if (varFuncMatch.Groups["assignationOperator"].Success)
                            i -= varFuncMatch.Groups["assignationOperator"].Length;
                        else if (varFuncMatch.Groups["postfixOperator"].Success)
                            i -= varFuncMatch.Groups["postfixOperator"].Length;

                    }
                    else
                    {
                        if (varFuncMatch.Groups["inObject"].Success)
                        {
                            if (stack.Count == 0 || stack.Peek() is ExpressionOperator)
                                throw new ExpressionEvaluatorSyntaxErrorException($"[{varFuncMatch.Value}] must follow an object.");

                            object obj = stack.Pop();
                            Type objType = null;

                            if (obj != null && TypesToBlock.Contains(obj.GetType()))
                                throw new ExpressionEvaluatorSecurityException($"{obj.GetType().FullName} type is blocked");
                            else if (obj is Type staticType && TypesToBlock.Contains(staticType))
                                throw new ExpressionEvaluatorSecurityException($"{staticType.FullName} type is blocked");
                            else if (obj is ClassOrTypeName classOrType && TypesToBlock.Contains(classOrType.Type))
                                throw new ExpressionEvaluatorSecurityException($"{classOrType.Type} type is blocked");

                            try
                            {
                                if (varFuncMatch.Groups["nullConditional"].Success && obj == null)
                                {
                                    stack.Push(null);
                                }
                                else
                                {
                                    VariableEvaluationEventArg variableEvaluationEventArg = new VariableEvaluationEventArg(varFuncName, obj);

                                    EvaluateVariable?.Invoke(this, variableEvaluationEventArg);

                                    if (variableEvaluationEventArg.HasValue)
                                    {
                                        stack.Push(variableEvaluationEventArg.Value);
                                    }
                                    else
                                    {
                                        BindingFlags flag = DetermineInstanceOrStatic(ref objType, ref obj);

                                        if (!OptionStaticProperiesGetActive && flag.HasFlag(BindingFlags.Static))
                                            throw new ExpressionEvaluatorSyntaxErrorException($"[{objType.ToString()}] object has no public Property or Field named \"{varFuncName}\".");
                                        if (!OptionInstanceProperiesGetActive && flag.HasFlag(BindingFlags.Instance))
                                            throw new ExpressionEvaluatorSyntaxErrorException($"[{objType.ToString()}] object has no public Property or Field named \"{varFuncName}\".");


                                        bool isDynamic = flag.HasFlag(BindingFlags.Instance) && obj is IDynamicMetaObjectProvider && obj is IDictionary<string, object>;
                                        IDictionary<string, object> dictionaryObject = obj as IDictionary<string, object>;

                                        dynamic member = isDynamic ? null : objType?.GetProperty(varFuncName, flag);
                                        dynamic varValue = null;
                                        bool assign = true;

                                        if (member == null && !isDynamic)
                                            member = objType.GetField(varFuncName, flag);

                                        bool pushVarValue = true;

                                        if (isDynamic)
                                        {
                                            if (!varFuncMatch.Groups["assignationOperator"].Success || varFuncMatch.Groups["assignmentPrefix"].Success)
                                                varValue = dictionaryObject[varFuncName];
                                            else
                                                pushVarValue = false;
                                        }
                                        else
                                            varValue = member.GetValue(obj);

                                        if(pushVarValue)
                                            stack.Push(varValue);

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
                                                varValue = varFuncMatch.Groups["postfixOperator"].Value.Equals("++") ? varValue + 1 : varValue - 1;
                                            else
                                                assign = false;

                                            if (assign)
                                            {
                                                if (isDynamic)
                                                    dictionaryObject[varFuncName] = varValue;
                                                else
                                                    member.SetValue(obj, varValue);
                                            }
                                        }
                                        else if (varFuncMatch.Groups["assignationOperator"].Success)
                                            i -= varFuncMatch.Groups["assignationOperator"].Length;
                                        else if (varFuncMatch.Groups["postfixOperator"].Success)
                                            i -= varFuncMatch.Groups["postfixOperator"].Length;
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
                                throw new ExpressionEvaluatorSyntaxErrorException($"[{objType.ToString()}] object has no public Property or Member named \"{varFuncName}\".", ex);
                            }
                        }
                        else
                        {
                            VariableEvaluationEventArg variableEvaluationEventArg = new VariableEvaluationEventArg(varFuncName);

                            EvaluateVariable?.Invoke(this, variableEvaluationEventArg);

                            if (variableEvaluationEventArg.HasValue)
                            {
                                stack.Push(variableEvaluationEventArg.Value);
                            }
                            else
                            {
                                string typeName = $"{varFuncName}{((i < expr.Length && expr.Substring(i)[0] == '?') ? "?" : "") }";
                                Type staticType = GetTypeByFriendlyName(typeName);

                                if (typeName.EndsWith("?") && staticType != null)
                                    i++;

                                if (staticType != null)
                                {
                                    stack.Push(new ClassOrTypeName() { Type = staticType });
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
                return false;
        }

        private bool EvaluateTwoCharsOperators(string expr, Stack<object> stack, ref int i)
        {
            if (i < expr.Length - 1)
            {
                String op = expr.Substring(i, 2);
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
                    List<string> expressionsInParenthis = GetExpressionsBetweenParenthis(expr, ref i, true);

                    InternalDelegate lambdaDelegate = stack.Pop() as InternalDelegate;

                    stack.Push(lambdaDelegate(expressionsInParenthis.ConvertAll(arg => Evaluate(arg)).ToArray()));
                }
                else
                {
                    CorrectStackWithUnaryPlusOrMinusBeforeParenthisIfNecessary(stack);

                    List<string> expressionsInParenthis = GetExpressionsBetweenParenthis(expr, ref i, false);

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

                Match assignationOrPostFixOperatorMatch = null;

                object valueToPush = null;

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
            List<object> list = stack.ToList();

            operatorsEvaluations.ForEach(delegate (Dictionary<ExpressionOperator, Func<dynamic, dynamic, object>> operatorEvalutationsDict)
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

        private MethodInfo GetRealMethod(ref Type type, ref object obj, string func, BindingFlags flag, List<object> args)
        {
            MethodInfo methodInfo = null;
            List<object> modifiedArgs = new List<object>(args);

            if (OptionFluidPrefixingActive &&
                (func.ManageCasing(OptionCaseSensitiveEvaluationActive).StartsWith("Fluid".ManageCasing(OptionCaseSensitiveEvaluationActive))
                    || func.ManageCasing(OptionCaseSensitiveEvaluationActive).StartsWith("Fluent".ManageCasing(OptionCaseSensitiveEvaluationActive))))
            {
                methodInfo = GetRealMethod(ref type, ref obj, func.ManageCasing(OptionCaseSensitiveEvaluationActive).Substring(func.ManageCasing(OptionCaseSensitiveEvaluationActive).StartsWith("Fluid".ManageCasing(OptionCaseSensitiveEvaluationActive)) ? 5 : 6), flag, modifiedArgs);
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
                methodInfo = type.GetMethod(func.ManageCasing(OptionCaseSensitiveEvaluationActive), flag);
            }
            else
            {
                methodInfo = type.GetMethod(func.ManageCasing(OptionCaseSensitiveEvaluationActive), flag, null, args.ConvertAll(arg => arg.GetType()).ToArray(), null);
            }

            if (methodInfo != null)
            {
                methodInfo = MakeConcreteMethodIfGeneric(methodInfo);
            }
            else
            {
                List<MethodInfo> methodInfos = type.GetMethods(flag)
                .Where(m => m.Name.ManageCasing(OptionCaseSensitiveEvaluationActive).Equals(func.ManageCasing(OptionCaseSensitiveEvaluationActive)) && m.GetParameters().Length == modifiedArgs.Count)
                .ToList();

                for (int m = 0; m < methodInfos.Count && methodInfo == null; m++)
                {
                    methodInfos[m] = MakeConcreteMethodIfGeneric(methodInfos[m]);

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

        private MethodInfo MakeConcreteMethodIfGeneric(MethodInfo methodInfo)
        {
            if (methodInfo.IsGenericMethod)
            {
                return methodInfo.MakeGenericMethod(Enumerable.Repeat(typeof(object), methodInfo.GetGenericArguments().Count()).ToArray());
            }

            return methodInfo;
        }

        private BindingFlags DetermineInstanceOrStatic(ref Type objType, ref object obj)
        {
            if (obj is ClassOrTypeName classOrTypeName)
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

        string GetScriptBetweenCurlyBrackets(string parentScript, ref int index)
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

        private List<string> GetExpressionsBetweenParenthis(string expr, ref int i, bool checkSeparator, string separator = ",")
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

                if (internalStringMatch.Success)
                {
                    string innerString = internalStringMatch.Value + GetCodeUntilEndOfString(expr.Substring(i + internalStringMatch.Length), internalStringMatch);
                    currentExpression += innerString;
                    i += innerString.Length - 1;
                }
                else if (internalCharMatch.Success)
                {
                    currentExpression += internalCharMatch.Value;
                    i += internalCharMatch.Length - 1;
                }
                else
                {
                    s = expr.Substring(i, 1);

                    if (s.Equals("(")) bracketCount++;

                    if (s.Equals(")"))
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
                        currentExpression += s;
                }
            }

            if (bracketCount > 0)
            {
                string beVerb = bracketCount == 1 ? "is" : "are";
                throw new Exception($"{bracketCount} ')' character {beVerb} missing in expression : [{expr}]");
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
            else if (OptionEvaluateFunctionActive && name.ManageCasing(OptionCaseSensitiveEvaluationActive).Equals("Evaluate".ManageCasing(OptionCaseSensitiveEvaluationActive)))
            {
                result = Evaluate((string)Evaluate(args[0]));
            }
            else if (OptionScriptEvaluateFunctionActive && name.ManageCasing(OptionCaseSensitiveEvaluationActive).Equals("ScriptEvaluate".ManageCasing(OptionCaseSensitiveEvaluationActive)))
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

        private Type GetTypeByFriendlyName(string typeName, bool tryWithNamespaceInclude = false)
        {
            Type result = null;
            try
            {
                result = Type.GetType(typeName, false, true);

                if (result == null)
                {
                    typeName = primaryTypesRegex.Replace(typeName, delegate (Match match)
                    {
                        return primaryTypesDict[match.Value.ManageCasing(OptionCaseSensitiveEvaluationActive)].ToString();
                    });

                    result = Type.GetType(typeName, false, true);
                }

                if (result == null)
                {
                    result = Types.Find(type => type.Name.ManageCasing(OptionCaseSensitiveEvaluationActive).Equals(typeName.ManageCasing(OptionCaseSensitiveEvaluationActive)));
                }

                for (int a = 0; a < Assemblies.Count && result == null; a++)
                {
                    if (tryWithNamespaceInclude)
                        result = Type.GetType($"{typeName},{Assemblies[a].FullName}", false, true);

                    for (int i = 0; i < Namespaces.Count && result == null; i++)
                    {
                        result = Type.GetType($"{Namespaces[i]}.{typeName},{Assemblies[a].FullName}", false, true);
                    }
                }
            }
            catch { }

            if (result != null && TypesToBlock.Contains(result))
                result = null;

            return result;
        }

        private static object ChangeType(object value, Type conversionType)
        {
            if (conversionType == null)
            {
                throw new ArgumentNullException("conversionType");
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
            Match codeUntilEndOfStringMatch = stringBeginningMatch.Value.Contains("$") ? endOfStringWithDollar.Match(subExpr) : endOfStringWithoutDollar.Match(subExpr);

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

        private class ClassOrTypeName
        {
            public Type Type { get; set; }
        }

        private class DelegateEncaps
        {
            private readonly InternalDelegate lambda;

            private MethodInfo methodInfo;
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

    #region Internal extentions methods

    internal static class StringCaseManagementForExpressionEvaluatorExtension
    {
        public static string ManageCasing(this string text, bool isCaseSensitive)
        {
            return isCaseSensitive ? text : text.ToLower();
        }
    }

    #endregion

    #region linked enums

    public enum OptionOnNoReturnKeywordFoundInScriptAction
    {
        ReturnAutomaticallyLastEvaluatedExpression,
        ReturnNull,
        ThrowSyntaxException
    }

    #endregion

    #region ExpressionEvaluator linked public classes (specific Exceptions and EventArgs)

    public class ExpressionEvaluatorSyntaxErrorException : Exception
    {
        public ExpressionEvaluatorSyntaxErrorException() : base()
        { }

        public ExpressionEvaluatorSyntaxErrorException(string message) : base(message)
        { }
        public ExpressionEvaluatorSyntaxErrorException(string message, Exception innerException) : base(message, innerException)
        { }
    }

    public class ExpressionEvaluatorSecurityException : Exception
    {
        public ExpressionEvaluatorSecurityException() : base()
        { }

        public ExpressionEvaluatorSecurityException(string message) : base(message)
        { }
        public ExpressionEvaluatorSecurityException(string message, Exception innerException) : base(message, innerException)
        { }
    }

    public class VariableEvaluationEventArg : EventArgs
    {
        /// <summary>
        /// Constructor of the VariableEvaluationEventArg
        /// </summary>
        /// <param name="name">The name of the variable to Evaluate</param>
        public VariableEvaluationEventArg(string name, object onInstance = null)
        {
            Name = name;
            This = onInstance;
        }

        /// <summary>
        /// The name of the variable to Evaluate
        /// </summary>
        public string Name { get; private set; }

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
        /// In the case of on the fly instance property definition the instance of the object on which this Function is called.
        /// Otherwise is set to null.
        /// </summary>
        public object This { get; private set; } = null;
    }

    public class FunctionEvaluationEventArg : EventArgs
    {
        private readonly Func<string, object> evaluateFunc = null;

        public FunctionEvaluationEventArg(string name, Func<string, object> evaluateFunc, List<string> args = null, object onInstance = null)
        {
            Name = name;
            Args = args ?? new List<string>();
            this.evaluateFunc = evaluateFunc;
            This = onInstance;
        }

        /// <summary>
        /// The not evaluated args of the function
        /// </summary>
        public List<string> Args { get; private set; } = new List<string>();

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
        public string Name { get; private set; }

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
        /// In the case of on the fly instance method definition the instance of the object on which this Function is called.
        /// Otherwise is set to null.
        /// </summary>
        public object This { get; private set; } = null;
    }

    #endregion
}
