using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

/// <summary>
/// This class allow to evaluate a string math expression 
/// </summary>
internal class ExpressionEvaluator
{
    private static Regex varOrFunctionRegEx = new Regex(@"^(?<inObject>\.)?(?<name>[a-zA-Z_][a-zA-Z0-9_]*)\s*(?<isfunction>[(])?", RegexOptions.IgnoreCase);
    private static Regex numberRegex = new Regex(@"^(?<sign>[+-])?\d+(?<hasdecimal>\.?\d+(e[+-]?\d+)?)?(?<type>ul|[fdulm])?", RegexOptions.IgnoreCase);
    private static Regex stringBeginningRegex = new Regex("^(?<interpolated>[$])?(?<escaped>[@])?[\"]");
    private static Regex castRegex = new Regex(@"^\(\s*(?<typeName>[a-zA-Z_][a-zA-Z0-9_\.\[\]<>]*)\s*\)");
    private static Regex primaryTypesRegex = new Regex(@"(?<=^|[^a-zA-Z_])(?<primaryType>object|string|bool|byte|char|decimal|double|short|int|long|sbyte|float|ushort|uint|void)(?=[^a-zA-Z_]|$)");

    private static BindingFlags instanceBindingFlag = (BindingFlags.Default | BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
    private static BindingFlags staticBindingFlag = (BindingFlags.Default | BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Static);

    private static string[] namespacesToSearchForTypes = new string[]
    {
        "System",
        "System.Text",
        "System.Text.RegularExpressions",
        "System.ComponentModel",
        "System.Collections",
        "System.Collections.Generic",
        "System.Collections.Specialized",
        "System.Globalization",
    };

    private static Dictionary<string, Type> PrimaryTypesDict = new Dictionary<string, Type>()
    {
        { "object", typeof(object) },
        { "string", typeof(string) },
        { "bool", typeof(bool) },
        { "byte", typeof(byte) },
        { "char", typeof(char) },
        { "decimal", typeof(decimal) },
        { "double", typeof(double) },
        { "short", typeof(short) },
        { "int", typeof(int) },
        { "long", typeof(long) },
        { "sbyte", typeof(sbyte) },
        { "float", typeof(float) },
        { "ushort", typeof(ushort) },
        { "uint", typeof(uint) },
        { "ulong", typeof(ulong) },
        { "void", typeof(void) },
    };

    private enum ExpressionOperator
    {
        Plus,
        Minus,
        Multiply,
        Divide,
        Modulo,
        Lower,
        Greater,
        Equal,
        LowerOrEqual,
        GreaterOrEqual,
        NotEqual,
        LogicalNegation,
        ConditionalAnd,
        ConditionalOr,
        LogicalAnd,
        LogicalOr,
        LogicalXor,
        ShiftBitsLeft,
        ShiftBitsRight,
        Cast,
        Indexing
    }

    private Dictionary<string, ExpressionOperator> operatorsDictionary = new Dictionary<string, ExpressionOperator>(StringComparer.OrdinalIgnoreCase)
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
        { "==", ExpressionOperator.Equal },
        { "<>", ExpressionOperator.NotEqual },
        { "!=", ExpressionOperator.NotEqual },
        { "&&", ExpressionOperator.ConditionalAnd },
        { "||", ExpressionOperator.ConditionalOr },
        { "!", ExpressionOperator.LogicalNegation },
        { "&", ExpressionOperator.LogicalAnd },
        { "|", ExpressionOperator.LogicalOr },
        { "^", ExpressionOperator.LogicalXor },
        { "<<", ExpressionOperator.ShiftBitsLeft },
        { ">>", ExpressionOperator.ShiftBitsRight }
    };

    private Dictionary<ExpressionOperator, bool> leftOperandOnlyOperatorsEvaluationDictionary = new Dictionary<ExpressionOperator, bool>()
    {
    };

    private Dictionary<ExpressionOperator, bool> rightOperandOnlyOperatorsEvaluationDictionary = new Dictionary<ExpressionOperator, bool>()
    {
        {ExpressionOperator.LogicalNegation, true }
    };

    private List<Dictionary<ExpressionOperator, Func<dynamic, dynamic, object>>> operatorsEvaluations =
        new List<Dictionary<ExpressionOperator, Func<dynamic, dynamic, object>>>()
    {
        new Dictionary<ExpressionOperator, Func<dynamic, dynamic, object>>()
        {
            {ExpressionOperator.Indexing, (dynamic left, dynamic right) => left[right] },
        },
        new Dictionary<ExpressionOperator, Func<dynamic, dynamic, object>>()
        {
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
    };

    /// <summary>
    /// The Values of the variable use in the expressions
    /// </summary>
    public Dictionary<string, object> Variables { get; set; } = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

    public ExpressionEvaluator()
    { }

    public ExpressionEvaluator(Dictionary<string, object> variables)
    {
        this.Variables = variables;
    }

    /// <summary>
    /// Evaluate the specified math expression
    /// </summary>
    /// <param name="expr">the math expression to evaluate</param>
    /// <returns>The result of the operation if syntax is correct</returns>
    public object Evaluate(string expr)
    {
        expr = expr.Trim();

        Stack<object> stack = new Stack<object>();

        for (int i = 0; i < expr.Length; i++)
        {
            string restOfExpression = expr.Substring(i, expr.Length - i);

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
                    continue;
                }
            }

            Match numberMatch = numberRegex.Match(restOfExpression);
            Match varFuncMatch = varOrFunctionRegEx.Match(restOfExpression);

            if (numberMatch.Success
                && (!numberMatch.Groups["sign"].Success
                    || stack.Count == 0
                    || stack.Peek() is ExpressionOperator))
            {
                i += numberMatch.Length;
                i--;

                if (numberMatch.Groups["type"].Success)
                {

                    string type = numberMatch.Groups["type"].Value.ToLower();
                    string numberNoType = numberMatch.Value.Replace(type, string.Empty);

                    if (type.Equals("f"))
                    {
                        stack.Push(float.Parse(numberNoType, NumberStyles.Any, CultureInfo.InvariantCulture));
                    }
                    else if (type.Equals("d"))
                    {
                        stack.Push(double.Parse(numberNoType, NumberStyles.Any, CultureInfo.InvariantCulture));
                    }
                    else if (type.Equals("u"))
                    {
                        stack.Push(uint.Parse(numberNoType, NumberStyles.Any, CultureInfo.InvariantCulture));
                    }
                    else if (type.Equals("l"))
                    {
                        stack.Push(long.Parse(numberNoType, NumberStyles.Any, CultureInfo.InvariantCulture));
                    }
                    else if (type.Equals("ul"))
                    {
                        stack.Push(ulong.Parse(numberNoType, NumberStyles.Any, CultureInfo.InvariantCulture));
                    }
                    else if (type.Equals("m"))
                    {
                        stack.Push(decimal.Parse(numberNoType, NumberStyles.Any, CultureInfo.InvariantCulture));
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
            }
            else if (varFuncMatch.Success && !operatorsDictionary.ContainsKey(varFuncMatch.Value))
            {
                i += varFuncMatch.Length;

                if (varFuncMatch.Groups["isfunction"].Success)
                {
                    String s = expr.Substring(i, 1);
                    List<string> funcArgs = new List<string>();
                    string currentArg = string.Empty;
                    int bracketCount = 1;
                    for (; i < expr.Length; i++)
                    {
                        s = expr.Substring(i, 1);

                        if (s.Equals("(")) bracketCount++;

                        if (s.Equals(")"))
                        {
                            if (!currentArg.Trim().Equals(string.Empty))
                                funcArgs.Add(currentArg);
                            bracketCount--;
                            if (bracketCount == 0) break;
                        }

                        if (s.Equals(",") && bracketCount == 1)
                        {
                            funcArgs.Add(currentArg);
                            currentArg = string.Empty;
                        }
                        else
                            currentArg += s;
                    }

                    if (bracketCount > 0)
                    {
                        string beVerb = bracketCount == 1 ? "is" : "are";
                        throw new Exception($"{bracketCount} ')' character {beVerb} missing in expression : [{expr}]");
                    }

                    object funcResult;
                    if (varFuncMatch.Groups["inObject"].Success)
                    {
                        if (stack.Count == 0 || stack.Peek() is ExpressionOperator)
                        {
                            throw new ExpressionEvaluatorSyntaxErrorException($"[{varFuncMatch.Value})] must follow an object.");
                        }
                        else
                        {
                            string func = varFuncMatch.Groups["name"].Value.ToLower();
                            object obj = stack.Pop();
                            Type objType = null;

                            try
                            {
                                List<object> oArgs = funcArgs.ConvertAll(arg => Evaluate(arg));
                                Type[] argsTypes = oArgs.ConvertAll(arg => arg.GetType()).ToArray();
                                BindingFlags flag = instanceBindingFlag;

                                if (obj is Type)
                                {
                                    objType = obj as Type;
                                    obj = null;
                                    flag = staticBindingFlag;
                                }
                                else
                                {
                                    objType = obj.GetType();
                                }
                                    
                                stack.Push(objType
                                    .GetMethod(func, flag, null, argsTypes, null)
                                    .Invoke(obj, oArgs.ToArray()));

                            }
                            catch (Exception ex)
                            {
                                throw new ExpressionEvaluatorSyntaxErrorException($"[{objType.ToString()}] object has no Method named \"{func}\".", ex);
                            }
                        }
                    }
                    else if (DefaultFunctions(varFuncMatch.Groups["name"].Value.ToLower(), funcArgs, out funcResult))
                    {
                        stack.Push(funcResult);
                    }
                    else
                    {
                        throw new ExpressionEvaluatorSyntaxErrorException($"Function [{varFuncMatch.Groups["name"].Value}] unknown in expression : [{expr}]");
                    }
                }
                else
                {
                    string var = varFuncMatch.Groups["name"].Value.ToLower();
                    Type staticType = GetTypeByFriendlyName(var);

                    if (varFuncMatch.Groups["inObject"].Success)
                    {
                        if (stack.Count == 0 || stack.Peek() is ExpressionOperator)
                        {
                            throw new ExpressionEvaluatorSyntaxErrorException($"[{varFuncMatch.Value}] must follow an object.");
                        }
                        else
                        {
                            object obj = stack.Pop();
                            Type objType = null;

                            try
                            {
                                BindingFlags flag = instanceBindingFlag;

                                if (obj is Type)
                                {
                                    objType = obj as Type;
                                    obj = null;
                                    flag = staticBindingFlag;
                                }
                                else
                                {
                                    objType = obj.GetType();
                                }
                                
                                object varValue = objType?.GetProperty(var, flag)?.GetValue(obj);
                                if(varValue == null)
                                    varValue = objType.GetField(var, flag).GetValue(obj);
                                
                                stack.Push(varValue);
                            }
                            catch (Exception ex)
                            {
                                throw new ExpressionEvaluatorSyntaxErrorException($"[{objType.ToString()}] object has no public Property or Member named \"{var}\".", ex);
                            }
                        }
                    }
                    else if(staticType != null)
                    {
                        stack.Push(staticType);
                    }
                    else if (var.Equals("pi"))
                    {
                        stack.Push(Math.PI);
                    }
                    else if (var.Equals("e"))
                    {
                        stack.Push(Math.E);
                    }
                    else if (var.Equals("null"))
                    {
                        stack.Push(null);
                    }
                    else if (var.Equals("true"))
                    {
                        stack.Push(true);
                    }
                    else if (var.Equals("false"))
                    {
                        stack.Push(false);
                    }
                    else if (Variables.ContainsKey(var))
                    {
                        stack.Push(Variables[var]);
                    }
                    else
                    {
                        throw new ExpressionEvaluatorSyntaxErrorException($"Variable [{var}] unknown in expression : [{expr}]");
                    }

                    i--;
                }
            }
            else
            {
                String s = expr.Substring(i, 1);
                if (i < expr.Length - 1)
                {
                    String op = expr.Substring(i, 2);
                    if (operatorsDictionary.ContainsKey(op))
                    {
                        stack.Push(operatorsDictionary[op]);
                        i++;
                        continue;
                    }
                }

                char chr = s.ToCharArray()[0];

                if (s.Equals(")"))
                {
                    throw new Exception($"To much ')' characters are defined in expression : [{expr}] : no corresponding '(' fund.");
                }
                else
                {
                    if (s.Equals("("))
                    {
                        string innerExp = "";
                        i++; //Fetch Next Character
                        int bracketCount = 1;
                        for (; i < expr.Length; i++)
                        {
                            s = expr.Substring(i, 1);

                            if (s.Equals("(")) bracketCount++;

                            if (s.Equals(")"))
                            {
                                bracketCount--;
                                if (bracketCount == 0) break;
                            }
                            innerExp += s;
                        }

                        if (bracketCount > 0)
                        {
                            string beVerb = bracketCount == 1 ? "is" : "are";
                            throw new Exception($"{bracketCount} ')' character {beVerb} missing in expression : [{expr}]");
                        }
                        stack.Push(Evaluate(innerExp));
                    }
                    else if(s.Equals("["))
                    {
                        string innerExp = "";
                        i++; //Fetch Next Character
                        int bracketCount = 1;
                        for (; i < expr.Length; i++)
                        {
                            s = expr.Substring(i, 1);

                            if (s.Equals("[")) bracketCount++;

                            if (s.Equals("]"))
                            {
                                bracketCount--;
                                if (bracketCount == 0) break;
                            }
                            innerExp += s;
                        }

                        if (bracketCount > 0)
                        {
                            string beVerb = bracketCount == 1 ? "is" : "are";
                            throw new Exception($"{bracketCount} ']' character {beVerb} missing in expression : [{expr}]");
                        }
                        stack.Push(ExpressionOperator.Indexing);
                        stack.Push(Evaluate(innerExp));
                    }
                    else if (stringBeginningRegex.IsMatch(restOfExpression))
                    {
                        Match stringBeginningMatch = stringBeginningRegex.Match(restOfExpression);
                        bool isEscaped = stringBeginningMatch.Groups["escaped"].Success;
                        bool isInterpolated = stringBeginningMatch.Groups["interpolated"].Success;

                        i += stringBeginningMatch.Length;

                        Regex stringRegexPattern = new Regex($"^[^{(isEscaped ? "" : @"\\")}{(isInterpolated ? "{}" : "")}\"]*");

                        bool endOfString = false;

                        string resultString = string.Empty;

                        while (!endOfString && i < expr.Length)
                        {
                            Match stringMatch = stringRegexPattern.Match(expr.Substring(i, expr.Length - i));

                            resultString += stringMatch.Value;
                            i += stringMatch.Length;

                            if (expr.Substring(i, expr.Length - i)[0] == '"')
                            {
                                endOfString = true;
                                stack.Push(resultString);
                            }
                            else if (expr.Substring(i, expr.Length - i)[0] == '{')
                            {
                                i++;

                                if (expr.Substring(i, expr.Length - i)[0] == '{')
                                {
                                    resultString += @"{";
                                    i++;
                                }
                                else
                                {
                                    string innerExp = "";
                                    i++; //Fetch Next Character
                                    int bracketCount = 1;
                                    for (; i < expr.Length; i++)
                                    {
                                        s = expr.Substring(i, 1);

                                        if (s.Equals("{")) bracketCount++;

                                        if (s.Equals("}"))
                                        {
                                            bracketCount--;
                                            i++;
                                            if (bracketCount == 0) break;
                                        }
                                        innerExp += s;
                                    }

                                    if (bracketCount > 0)
                                    {
                                        string beVerb = bracketCount == 1 ? "is" : "are";
                                        throw new Exception($"{bracketCount} '}}' character {beVerb} missing in expression : [{expr}]");
                                    }
                                    resultString += Evaluate(innerExp).ToString();
                                }
                            }
                            else if (expr.Substring(i, expr.Length - i)[0] == '}')
                            {
                                i++;

                                if (expr.Substring(i, expr.Length - i)[0] == '}')
                                {
                                    resultString += @"}";
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

                                if (expr.Substring(i, expr.Length - i)[0] == '\\')
                                {
                                    resultString += @"\";
                                    i++;
                                }
                                else if (expr.Substring(i, expr.Length - i)[0] == '"')
                                {
                                    resultString += "\"";
                                    i++;
                                }
                                else if (expr.Substring(i, expr.Length - i)[0] == '0')
                                {
                                    resultString += "\0";
                                    i++;
                                }
                                else if (expr.Substring(i, expr.Length - i)[0] == 'a')
                                {
                                    resultString += "\a";
                                    i++;
                                }
                                else if (expr.Substring(i, expr.Length - i)[0] == 'b')
                                {
                                    resultString += "\b";
                                    i++;
                                }
                                else if (expr.Substring(i, expr.Length - i)[0] == 'f')
                                {
                                    resultString += "\f";
                                    i++;
                                }
                                else if (expr.Substring(i, expr.Length - i)[0] == 'n')
                                {
                                    resultString += "\n";
                                    i++;
                                }
                                else if (expr.Substring(i, expr.Length - i)[0] == 'r')
                                {
                                    resultString += "\r";
                                    i++;
                                }
                                else if (expr.Substring(i, expr.Length - i)[0] == 't')
                                {
                                    resultString += "\t";
                                    i++;
                                }
                                else if (expr.Substring(i, expr.Length - i)[0] == 'v')
                                {
                                    resultString += "\v";
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
                    }
                    else if (operatorsDictionary.ContainsKey(s))
                    {
                        stack.Push(operatorsDictionary[s]);
                    }
                    else if (!s.Trim().Equals(string.Empty))
                    {
                        throw new ExpressionEvaluatorSyntaxErrorException("Invalid character.");
                    }
                }
            }
        }

        List<object> list = stack.ToList<object>();

        operatorsEvaluations.ForEach(delegate (Dictionary<ExpressionOperator, Func<dynamic, dynamic, object>> operatorEvalutationsDict)
        {
            for (int i = list.Count - 1; i >= 0; i--)
            {
                for(int opi = 0; opi < operatorEvalutationsDict.Keys.ToList().Count; opi++)
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

    private bool DefaultFunctions(string name, List<string> args, out object result)
    {
        bool functionExists = true;

        if (name.Equals("abs"))
        {
            result = Math.Abs(Convert.ToDouble(Evaluate(args[0])));
        }
        else if (name.Equals("acos"))
        {
            result = Math.Acos(Convert.ToDouble(Evaluate(args[0])));
        }
        else if (name.Equals("array"))
        {
            result = args.ConvertAll(arg => Evaluate(arg)).ToArray();
        }
        else if (name.Equals("asin"))
        {
            result = Math.Asin(Convert.ToDouble(Evaluate(args[0])));
        }
        else if (name.Equals("atan"))
        {
            result = Math.Atan(Convert.ToDouble(Evaluate(args[0])));
        }
        else if (name.Equals("atan2"))
        {
            result = Math.Atan2(Convert.ToDouble(Evaluate(args[0])), Convert.ToDouble(Evaluate(args[1])));
        }
        else if (name.Equals("avg"))
        {
            result = args.ConvertAll(arg => Convert.ToDouble(Evaluate(arg)))
                .Sum() / args.Count;
        }
        else if (name.Equals("ceiling"))
        {
            result = Math.Ceiling(Convert.ToDouble(Evaluate(args[0])));
        }
        else if (name.Equals("cos"))
        {
            result = Math.Cos(Convert.ToDouble(Evaluate(args[0])));
        }
        else if (name.Equals("cosh"))
        {
            result = Math.Cosh(Convert.ToDouble(Evaluate(args[0])));
        }
        else if (name.Equals("exp"))
        {
            result = Math.Exp(Convert.ToDouble(Evaluate(args[0])));
        }
        else if (name.Equals("floor"))
        {
            result = Math.Floor(Convert.ToDouble(Evaluate(args[0])));
        }
        else if (name.Equals("ieeeremainder"))
        {
            result = Math.IEEERemainder(Convert.ToDouble(Evaluate(args[0])), Convert.ToDouble(Evaluate(args[1])));
        }
        else if (name.Equals("list"))
        {
            result = args.ConvertAll(arg => Evaluate(arg));
        }
        else if (name.Equals("log"))
        {
            result = Math.Log(Convert.ToDouble(Evaluate(args[0])), Convert.ToDouble(Evaluate(args[1])));
        }
        else if (name.Equals("log10"))
        {
            result = Math.Log10(Convert.ToDouble(Evaluate(args[0])));
        }
        else if (name.Equals("max"))
        {
            result = args.ConvertAll(arg => Convert.ToDouble(Evaluate(arg))).Max();
        }
        else if (name.Equals("min"))
        {
            result = args.ConvertAll(arg => Convert.ToDouble(Evaluate(arg))).Min();
        }
        else if (name.Equals("pow"))
        {
            result = Math.Pow(Convert.ToDouble(Evaluate(args[0])), Convert.ToDouble(Evaluate(args[1])));
        }
        else if (name.Equals("round"))
        {
            result = Math.Round(Convert.ToDouble(Evaluate(args[0])));
        }
        else if (name.Equals("sign"))
        {
            result = Math.Sign(Convert.ToDouble(Evaluate(args[0])));
        }
        else if (name.Equals("sin"))
        {
            result = Math.Sin(Convert.ToDouble(Evaluate(args[0])));
        }
        else if (name.Equals("sinh"))
        {
            result = Math.Sinh(Convert.ToDouble(Evaluate(args[0])));
        }
        else if (name.Equals("sqrt"))
        {
            result = Math.Sqrt(Convert.ToDouble(Evaluate(args[0])));
        }
        else if (name.Equals("tan"))
        {
            result = Math.Tan(Convert.ToDouble(Evaluate(args[0])));
        }
        else if (name.Equals("tanh"))
        {
            result = Math.Tanh(Convert.ToDouble(Evaluate(args[0])));
        }
        else if (name.Equals("truncate"))
        {
            result = Math.Truncate(Convert.ToDouble(Evaluate(args[0])));
        }
        else if (name.Equals("if"))
        {
            if ((bool)Evaluate(args[0]))
                result = Evaluate(args[2]);
            else
                result = Evaluate(args[1]);
        }
        else if (name.Equals("in"))
        {
            object valueToFind = Evaluate(args[0]);

            result = args.Skip(1).ToList()
                .ConvertAll(arg => Evaluate(arg))
                .Contains(valueToFind);
        }
        else if (name.Equals("default"))
        {
            Type type = (Evaluate(args[0]) as Type);
            if (type != null && type.IsValueType)
            {
                result = Activator.CreateInstance(type);
            }
            else
            {
                result = null;
            }
        }
        else
        {
            result = null;
            functionExists = false;
        }

        return functionExists;
    }

    private static Type GetTypeByFriendlyName(string typeName)
    {
        Type result = null;
        try
        {
            result = Type.GetType(typeName, false, true);

            if (result == null)
            {
                typeName = primaryTypesRegex.Replace(typeName, delegate (Match match)
                {
                    return PrimaryTypesDict[match.Value].ToString();
                });

                result = Type.GetType(typeName, false, true);
            }

            for (int i = 0; i < namespacesToSearchForTypes.Length && result == null; i++)
            {
                result = Type.GetType($"{namespacesToSearchForTypes[i]}.{typeName}", false, true);
            }
        }
        catch { }

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
}

public class ExpressionEvaluatorSyntaxErrorException : Exception
{
    public ExpressionEvaluatorSyntaxErrorException() : base()
    { }

    public ExpressionEvaluatorSyntaxErrorException(string message) : base(message)
    { }
    public ExpressionEvaluatorSyntaxErrorException(string message, Exception innerException) : base(message, innerException)
    { }
}
