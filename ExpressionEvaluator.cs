using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

/// <summary>
/// This class allow to evaluate a string math expression 
/// </summary>
internal class ExpressionEvaluator
{
    private static Regex varOrFunctionRegEx = new Regex(@"^(?<name>[a-zA-Z_][a-zA-Z0-9_]*)\s*(?<isfunction>[(])?", RegexOptions.IgnoreCase);

    private enum ExpressionOperator
    {
        Plus,
        Minus,
        Multiply,
        Divide,
        Modulo,
        Power,
        Lower,
        Greater,
        Equal,
        LowerOrEqual,
        GreaterOrEqual,
        NotEqual
    }

    private Dictionary<string, ExpressionOperator> operatorsDictionary = new Dictionary<string, ExpressionOperator>()
    {
        { "+", ExpressionOperator.Plus },
        { "-", ExpressionOperator.Minus },
        { "*", ExpressionOperator.Multiply },
        { "/", ExpressionOperator.Divide },
        { "%", ExpressionOperator.Modulo },
        { "^", ExpressionOperator.Power },
        { "<", ExpressionOperator.Lower },
        { ">", ExpressionOperator.Greater },
        { "==", ExpressionOperator.Equal },
        { "<=", ExpressionOperator.LowerOrEqual },
        { ">=", ExpressionOperator.GreaterOrEqual },
        { "<>", ExpressionOperator.NotEqual },
        { "!=", ExpressionOperator.NotEqual },
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

        string value = "";
        for (int i = 0; i < expr.Length; i++)
        {
            Match match = varOrFunctionRegEx.Match(expr.Substring(i, expr.Length - i));

            if (match.Success)
            {
                i += match.Length;

                if (match.Groups["isfunction"].Success)
                {
                    String s = expr.Substring(i, 1);
                    List<string> funcArgs = new List<string>();
                    funcArgs.Add(string.Empty);
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

                        if (s.Equals(",") && bracketCount == 0)
                            funcArgs.Add(string.Empty);
                        else
                            funcArgs[funcArgs.Count - 1] += s;
                    }

                    if (bracketCount > 0)
                    {
                        string beVerb = bracketCount == 1 ? "is" : "are";
                        throw new Exception($"{bracketCount} ')' character {beVerb} missing in expression : [{expr}]");
                    }

                    object funcResult;
                    if (DefaultFunctions(match.Groups["name"].Value.ToLower(), funcArgs, out funcResult))
                    {
                        stack.Push(funcResult);
                    }
                    else
                    {
                        throw new ExpressionEvaluatorSyntaxErrorException($"Function [{match.Groups["name"].Value}] unknown in expression : [{expr}]");
                    }
                }
                else
                {
                    string var = match.Groups["name"].Value.ToLower();

                    if (var.Equals("pi"))
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
                        stack.Push(Evaluate(value));
                        value = string.Empty;
                        stack.Push(operatorsDictionary[op]);
                        i++;
                        continue;
                    }
                }

                char chr = s.ToCharArray()[0];

                if(s.Equals(")"))
                {
                    throw new Exception($"To much ')' characters are defined in expression : [{expr}] : no corresponding '(' fund.");
                }
                // For negative values (to not make conflicts with the minus operator)
                else if (s.Equals("-") && value.Equals(string.Empty) && (stack.Count == 0 || stack.Peek() is ExpressionOperator))
                {
                    value += s;

                    if (i == (expr.Length - 1))
                        stack.Push(value.Trim());
                }
                else
                {
                    if (!char.IsDigit(chr) && chr != '.' && !value.Trim().Equals(string.Empty))
                    {
                        stack.Push(value.Trim());
                        value = string.Empty;
                    }
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
                        stack.Push(Evaluate(innerExp).ToString().Trim());
                    }
                    else if (operatorsDictionary.ContainsKey(s))
                    {
                        stack.Push(operatorsDictionary[s]);
                    }
                    else if (char.IsDigit(chr) || chr == '.')
                    {
                        value += s;

                        if (value.Split('.').Length > 2)
                            throw new ExpressionEvaluatorSyntaxErrorException("Invalid decimal.");

                        if (i == (expr.Length - 1))
                            stack.Push(Convert.ToDouble(value));
                    }
                    else if(!s.Trim().Equals(string.Empty))
                    {
                        throw new ExpressionEvaluatorSyntaxErrorException("Invalid character.");
                    }
                }
            }
        }
        object result = null;
        List<object> list = stack.ToList<object>();
        for (int i = list.Count - 2; i >= 0; i--)
        {
            if ((list[i] as ExpressionOperator?) == ExpressionOperator.Power)
            {
                list[i] = Math.Pow(Convert.ToDouble(list[i + 1]), Convert.ToDouble(list[i - 1]));
                list.RemoveAt(i + 1);
                list.RemoveAt(i - 1);
                i -= 1;
            }
        }

        for (int i = list.Count - 2; i >= 0; i--)
        {
            if ((list[i] as ExpressionOperator?) == ExpressionOperator.Divide)
            {
                list[i] = (Convert.ToDouble(list[i + 1]) / Convert.ToDouble(list[i - 1]));
                list.RemoveAt(i + 1);
                list.RemoveAt(i - 1);
                i -= 1;
            }
            else if ((list[i] as ExpressionOperator?) == ExpressionOperator.Multiply)
            {
                list[i] = (Convert.ToDouble(list[i + 1]) * Convert.ToDouble(list[i - 1]));
                list.RemoveAt(i + 1);
                list.RemoveAt(i - 1);
                i -= 1;
            }
            else if ((list[i] as ExpressionOperator?) == ExpressionOperator.Modulo)
            {
                list[i] = (Convert.ToDouble(list[i + 1]) % Convert.ToDouble(list[i - 1]));
                list.RemoveAt(i + 1);
                list.RemoveAt(i - 1);
                i -= 1;
            }
        }

        for (int i = list.Count - 2; i >= 0; i--)
        {
            if ((list[i] as ExpressionOperator?) == ExpressionOperator.Minus)
            {
                list[i] = (Convert.ToDouble(list[i + 1]) - Convert.ToDouble(list[i - 1]));
                list.RemoveAt(i + 1);
                list.RemoveAt(i - 1);
                i -= 1;
            }
            else if ((list[i] as ExpressionOperator?) == ExpressionOperator.Plus)
            {
                list[i] = (Convert.ToDouble(list[i + 1]) + Convert.ToDouble(list[i - 1]));
                list.RemoveAt(i + 1);
                list.RemoveAt(i - 1);
                i -= 1;
            }
        }
        stack.Clear();
        for (int i = 0; i < list.Count; i++)
        {
            stack.Push(list[i]);
        }
        while (stack.Count >= 3)
        {
            double right = Convert.ToDouble(stack.Pop());
            ExpressionOperator? op = stack.Pop() as ExpressionOperator?;
            double left = Convert.ToDouble(stack.Pop());

            if (op != null)
            {
                if (op == ExpressionOperator.Lower) result = (left < right);
                else if (op == ExpressionOperator.Greater) result = (left > right);
                else if (op == ExpressionOperator.LowerOrEqual) result = (left <= right);
                else if (op == ExpressionOperator.GreaterOrEqual) result = (left >= right);
                else if (op == ExpressionOperator.Equal) result = (left == right);
                else if (op == ExpressionOperator.NotEqual) result = (left != right);
            }

            stack.Push(result.ToString().Trim());
        }

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
        else
        {
            result = null;
            functionExists = false;
        }

        return functionExists;
    }

    public class ExpressionEvaluatorSyntaxErrorException : Exception
    {
        public ExpressionEvaluatorSyntaxErrorException() : base()
        { }

        public ExpressionEvaluatorSyntaxErrorException(string message) : base(message)
        { }
    }
}
