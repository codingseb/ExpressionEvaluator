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
    public double Evaluate(string expr)
    {
        expr = expr.ToLower();
        expr = expr.Replace(" ", "");
        expr = expr.Replace("true", "1");
        expr = expr.Replace("false", "0");

        Stack<String> stack = new Stack<String>();

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
                    int bracketCount = 0;
                    for (; i < expr.Length; i++)
                    {
                        s = expr.Substring(i, 1);

                        if (s.Equals("(")) bracketCount++;

                        if (s.Equals(")"))
                        {
                            if (bracketCount == 0) break;
                            bracketCount--;
                        }

                        if (s.Equals(",") && bracketCount == 0)
                            funcArgs.Add(string.Empty);
                        else
                            funcArgs[funcArgs.Count - 1] += s;
                    }

                    if (bracketCount > 0)
                        throw new Exception($"At least one ')' character is missing in expression : [{expr}]");

                    double funcResult;
                    if (DefaultFunctions(match.Groups["name"].Value.ToLower(), funcArgs, out funcResult))
                    {
                        value = funcResult.ToString();
                    }
                    else
                    {
                        throw new Exception($"Function [{match.Groups["name"].Value}] unknown in expression : [{expr}]");
                    }
                }
                else
                {
                    string var = match.Value.ToLower();

                    if (var.Equals("pi"))
                    {
                        value = Math.PI.ToString();
                    }
                    else if (var.Equals("e"))
                    {
                        value = Math.E.ToString();
                    }
                    else if (Variables.ContainsKey(var))
                    {
                        value = Variables[var].ToString();
                    }
                    else
                    {
                        throw new Exception($"Variable [{var}] unknown in expression : [{expr}]");
                    }

                    i--;
                }

                if (i == (expr.Length - 1))
                    stack.Push(value);
            }
            else
            {
                String s = expr.Substring(i, 1);
                // pick up any doublelogical operators first.
                if (i < expr.Length - 1)
                {
                    String op = expr.Substring(i, 2);
                    if (op == "<=" || op == ">=" || op == "==" || op == "!=" || op == "<>")
                    {
                        stack.Push(value);
                        value = "";
                        stack.Push(op);
                        i++;
                        continue;
                    }
                }

                char chr = s.ToCharArray()[0];

                // For negative values (to not make conflicts with the minus operator)
                if (s.Equals("-") && value.Equals(string.Empty))
                {
                    value += s;

                    if (i == (expr.Length - 1))
                        stack.Push(value);
                }
                else
                {
                    if (!char.IsDigit(chr) && chr != '.' && value != "")
                    {
                        stack.Push(value);
                        value = "";
                    }
                    if (s.Equals("("))
                    {
                        string innerExp = "";
                        i++; //Fetch Next Character
                        int bracketCount = 0;
                        for (; i < expr.Length; i++)
                        {
                            s = expr.Substring(i, 1);

                            if (s.Equals("(")) bracketCount++;

                            if (s.Equals(")"))
                            {
                                if (bracketCount == 0) break;
                                bracketCount--;
                            }
                            innerExp += s;
                        }

                        if (bracketCount > 0)
                            throw new Exception($"At least one ')' character is missing in expression : [{expr}]");

                        stack.Push(Evaluate(innerExp).ToString());
                    }
                    else if (s.Equals("+") ||
                                s.Equals("-") ||
                                s.Equals("*") ||
                                s.Equals("/") ||
                                s.Equals("%") ||
                                s.Equals("^") ||
                                s.Equals("<") ||
                                s.Equals(">"))
                    {
                        stack.Push(s);
                    }
                    else if (char.IsDigit(chr) || chr == '.')
                    {
                        value += s;

                        if (value.Split('.').Length > 2)
                            throw new Exception("Invalid decimal.");

                        if (i == (expr.Length - 1))
                            stack.Push(value);
                    }
                    else
                    {
                        throw new Exception("Invalid character.");
                    }
                }
            }
        }
        double result = 0;
        List<String> list = stack.ToList<String>();
        for (int i = list.Count - 2; i >= 0; i--)
        {
            if (list[i] == "^")
            {
                list[i] = Math.Pow(Convert.ToDouble(list[i + 1]), Convert.ToDouble(list[i - 1])).ToString();
                list.RemoveAt(i + 1);
                list.RemoveAt(i - 1);
                i -= 1;
            }
        }

        for (int i = list.Count - 2; i >= 0; i--)
        {
            if (list[i] == "/")
            {
                list[i] = (Convert.ToDouble(list[i + 1]) / Convert.ToDouble(list[i - 1])).ToString();
                list.RemoveAt(i + 1);
                list.RemoveAt(i - 1);
                i -= 1;
            }
            else if (list[i] == "*")
            {
                list[i] = (Convert.ToDouble(list[i + 1]) * Convert.ToDouble(list[i - 1])).ToString();
                list.RemoveAt(i + 1);
                list.RemoveAt(i - 1);
                i -= 1;
            }
            else if (list[i] == "%")
            {
                list[i] = (Convert.ToDouble(list[i + 1]) % Convert.ToDouble(list[i - 1])).ToString();
                list.RemoveAt(i + 1);
                list.RemoveAt(i - 1);
                i -= 1;
            }
        }

        for (int i = list.Count - 2; i >= 0; i--)
        {
            if (list[i] == "-")
            {
                list[i] = (Convert.ToDouble(list[i + 1]) - Convert.ToDouble(list[i - 1])).ToString();
                list.RemoveAt(i + 1);
                list.RemoveAt(i - 1);
                i -= 1;
            }
            else if (list[i] == "+")
            {
                list[i] = (Convert.ToDouble(list[i + 1]) + Convert.ToDouble(list[i - 1])).ToString();
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
            string op = stack.Pop();
            double left = Convert.ToDouble(stack.Pop());

            if (op == "<") result = (left < right) ? 1 : 0;
            else if (op == ">") result = (left > right) ? 1 : 0;
            else if (op == "<=") result = (left <= right) ? 1 : 0;
            else if (op == ">=") result = (left >= right) ? 1 : 0;
            else if (op == "==") result = (left == right) ? 1 : 0;
            else if (op == "!=") result = (left != right) ? 1 : 0;
            else if (op == "<>") result = (left != right) ? 1 : 0;

            stack.Push(result.ToString());
        }

        return Convert.ToDouble(stack.Pop());
    }

    private bool DefaultFunctions(string name, List<string> args, out double result)
    {
        bool functionExists = true;

        if (name.Equals("abs"))
        {
            result = Math.Abs(Evaluate(args[0]));
        }
        else if (name.Equals("acos"))
        {
            result = Math.Acos(Evaluate(args[0]));
        }
        else if (name.Equals("asin"))
        {
            result = Math.Asin(Evaluate(args[0]));
        }
        else if (name.Equals("atan"))
        {
            result = Math.Atan(Evaluate(args[0]));
        }
        else if (name.Equals("atan2"))
        {
            result = Math.Atan2(Evaluate(args[0]), Evaluate(args[1]));
        }
        else if (name.Equals("ceiling"))
        {
            result = Math.Ceiling(Evaluate(args[0]));
        }
        else if (name.Equals("cos"))
        {
            result = Math.Cos(Evaluate(args[0]));
        }
        else if (name.Equals("cosh"))
        {
            result = Math.Cosh(Evaluate(args[0]));
        }
        else if (name.Equals("exp"))
        {
            result = Math.Exp(Evaluate(args[0]));
        }
        else if (name.Equals("floor"))
        {
            result = Math.Floor(Evaluate(args[0]));
        }
        else if (name.Equals("ieeeremainder"))
        {
            result = Math.IEEERemainder(Evaluate(args[0]), Evaluate(args[1]));
        }
        else if (name.Equals("log"))
        {
            result = Math.Log(Evaluate(args[0]), Evaluate(args[1]));
        }
        else if (name.Equals("log10"))
        {
            result = Math.Log10(Evaluate(args[0]));
        }
        else if (name.Equals("max"))
        {
            result = args.ConvertAll(arg => Evaluate(arg)).Max();
        }
        else if (name.Equals("min"))
        {
            result = args.ConvertAll(arg => Evaluate(arg)).Min();
        }
        else if (name.Equals("pow"))
        {
            result = Math.Pow(Evaluate(args[0]), Evaluate(args[1]));
        }
        else if (name.Equals("round"))
        {
            result = Math.Round(Evaluate(args[0]));
        }
        else if (name.Equals("sign"))
        {
            result = Math.Sign(Evaluate(args[0]));
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
    public double Evaluate(string expr)
    {
        expr = expr.ToLower();
        expr = expr.Replace(" ", "");
        expr = expr.Replace("true", "1");
        expr = expr.Replace("false", "0");

        Stack<String> stack = new Stack<String>();

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
                    int bracketCount = 0;
                    for (; i < expr.Length; i++)
                    {
                        s = expr.Substring(i, 1);

                        if (s.Equals("(")) bracketCount++;

                        if (s.Equals(")"))
                        {
                            if (bracketCount == 0) break;
                            bracketCount--;
                        }

                        if (s.Equals(",") && bracketCount == 0)
                            funcArgs.Add(string.Empty);
                        else
                            funcArgs[funcArgs.Count - 1] += s;
                    }

                    if (bracketCount > 0)
                        throw new Exception($"At least one ')' character is missing in expression : [{expr}]");

                    double funcResult;
                    if (DefaultFunctions(match.Groups["name"].Value.ToLower(), funcArgs, out funcResult))
                    {
                        value = funcResult.ToString();
                    }
                    else
                    {
                        throw new Exception($"Function [{match.Groups["name"].Value}] unknown in expression : [{expr}]");
                    }
                }
                else
                {
                    string var = match.Value.ToLower();

                    if (var.Equals("pi"))
                    {
                        value = Math.PI.ToString();
                    }
                    else if (var.Equals("e"))
                    {
                        value = Math.E.ToString();
                    }
                    else if (Variables.ContainsKey(var))
                    {
                        value = Variables[var].ToString();
                    }
                    else
                    {
                        throw new Exception($"Variable [{var}] unknown in expression : [{expr}]");
                    }

                    i--;
                }

                if (i == (expr.Length - 1))
                    stack.Push(value);
            }
            else
            {
                String s = expr.Substring(i, 1);
                // pick up any doublelogical operators first.
                if (i < expr.Length - 1)
                {
                    String op = expr.Substring(i, 2);
                    if (op == "<=" || op == ">=" || op == "==" || op == "!=" || op == "<>")
                    {
                        stack.Push(value);
                        value = "";
                        stack.Push(op);
                        i++;
                        continue;
                    }
                }

                char chr = s.ToCharArray()[0];

                // For negative values (to not make conflicts with the minus operator)
                if (s.Equals("-") && value.Equals(string.Empty))
                {
                    value += s;

                    if (i == (expr.Length - 1))
                        stack.Push(value);
                }
                else
                {
                    if (!char.IsDigit(chr) && chr != '.' && value != "")
                    {
                        stack.Push(value);
                        value = "";
                    }
                    if (s.Equals("("))
                    {
                        string innerExp = "";
                        i++; //Fetch Next Character
                        int bracketCount = 0;
                        for (; i < expr.Length; i++)
                        {
                            s = expr.Substring(i, 1);

                            if (s.Equals("(")) bracketCount++;

                            if (s.Equals(")"))
                            {
                                if (bracketCount == 0) break;
                                bracketCount--;
                            }
                            innerExp += s;
                        }

                        if (bracketCount > 0)
                            throw new Exception($"At least one ')' character is missing in expression : [{expr}]");

                        stack.Push(Evaluate(innerExp).ToString());
                    }
                    else if (s.Equals("+") ||
                                s.Equals("-") ||
                                s.Equals("*") ||
                                s.Equals("/") ||
                                s.Equals("%") ||
                                s.Equals("^") ||
                                s.Equals("<") ||
                                s.Equals(">"))
                    {
                        stack.Push(s);
                    }
                    else if (char.IsDigit(chr) || chr == '.')
                    {
                        value += s;

                        if (value.Split('.').Length > 2)
                            throw new Exception("Invalid decimal.");

                        if (i == (expr.Length - 1))
                            stack.Push(value);
                    }
                    else
                    {
                        throw new Exception("Invalid character.");
                    }
                }
            }
        }
        double result = 0;
        List<String> list = stack.ToList<String>();
        for (int i = list.Count - 2; i >= 0; i--)
        {
            if (list[i] == "^")
            {
                list[i] = Math.Pow(Convert.ToDouble(list[i + 1]), Convert.ToDouble(list[i - 1])).ToString();
                list.RemoveAt(i + 1);
                list.RemoveAt(i - 1);
                i -= 1;
            }
        }

        for (int i = list.Count - 2; i >= 0; i--)
        {
            if (list[i] == "/")
            {
                list[i] = (Convert.ToDouble(list[i + 1]) / Convert.ToDouble(list[i - 1])).ToString();
                list.RemoveAt(i + 1);
                list.RemoveAt(i - 1);
                i -= 1;
            }
            else if (list[i] == "*")
            {
                list[i] = (Convert.ToDouble(list[i + 1]) * Convert.ToDouble(list[i - 1])).ToString();
                list.RemoveAt(i + 1);
                list.RemoveAt(i - 1);
                i -= 1;
            }
            else if (list[i] == "%")
            {
                list[i] = (Convert.ToDouble(list[i + 1]) % Convert.ToDouble(list[i - 1])).ToString();
                list.RemoveAt(i + 1);
                list.RemoveAt(i - 1);
                i -= 1;
            }
        }

        for (int i = list.Count - 2; i >= 0; i--)
        {
            if (list[i] == "-")
            {
                list[i] = (Convert.ToDouble(list[i + 1]) - Convert.ToDouble(list[i - 1])).ToString();
                list.RemoveAt(i + 1);
                list.RemoveAt(i - 1);
                i -= 1;
            }
            else if (list[i] == "+")
            {
                list[i] = (Convert.ToDouble(list[i + 1]) + Convert.ToDouble(list[i - 1])).ToString();
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
            string op = stack.Pop();
            double left = Convert.ToDouble(stack.Pop());

            if (op == "<") result = (left < right) ? 1 : 0;
            else if (op == ">") result = (left > right) ? 1 : 0;
            else if (op == "<=") result = (left <= right) ? 1 : 0;
            else if (op == ">=") result = (left >= right) ? 1 : 0;
            else if (op == "==") result = (left == right) ? 1 : 0;
            else if (op == "!=") result = (left != right) ? 1 : 0;
            else if (op == "<>") result = (left != right) ? 1 : 0;

            stack.Push(result.ToString());
        }

        return Convert.ToDouble(stack.Pop());
    }

    private bool DefaultFunctions(string name, List<string> args, out double result)
    {
        bool functionExists = true;

        if (name.Equals("abs"))
        {
            result = Math.Abs(Evaluate(args[0]));
        }
        else if (name.Equals("acos"))
        {
            result = Math.Acos(Evaluate(args[0]));
        }
        else if (name.Equals("asin"))
        {
            result = Math.Asin(Evaluate(args[0]));
        }
        else if (name.Equals("atan"))
        {
            result = Math.Atan(Evaluate(args[0]));
        }
        else if (name.Equals("atan2"))
        {
            result = Math.Atan2(Evaluate(args[0]), Evaluate(args[1]));
        }
        else if (name.Equals("avg"))
        {
            result = args.ConvertAll(arg => Evaluate(arg))
                .Sum() / args.Count;
        }
        else if (name.Equals("ceiling"))
        {
            result = Math.Ceiling(Evaluate(args[0]));
        }
        else if (name.Equals("cos"))
        {
            result = Math.Cos(Evaluate(args[0]));
        }
        else if (name.Equals("cosh"))
        {
            result = Math.Cosh(Evaluate(args[0]));
        }
        else if (name.Equals("exp"))
        {
            result = Math.Exp(Evaluate(args[0]));
        }
        else if (name.Equals("floor"))
        {
            result = Math.Floor(Evaluate(args[0]));
        }
        else if (name.Equals("ieeeremainder"))
        {
            result = Math.IEEERemainder(Evaluate(args[0]), Evaluate(args[1]));
        }
        else if (name.Equals("log"))
        {
            result = Math.Log(Evaluate(args[0]), Evaluate(args[1]));
        }
        else if (name.Equals("log10"))
        {
            result = Math.Log10(Evaluate(args[0]));
        }
        else if (name.Equals("max"))
        {
            result = args.ConvertAll(arg => Evaluate(arg)).Max();
        }
        else if (name.Equals("min"))
        {
            result = args.ConvertAll(arg => Evaluate(arg)).Min();
        }
        else if (name.Equals("pow"))
        {
            result = Math.Pow(Evaluate(args[0]), Evaluate(args[1]));
        }
        else if (name.Equals("round"))
        {
            result = Math.Round(Evaluate(args[0]));
        }
        else if (name.Equals("sign"))
        {
            result = Math.Sign(Evaluate(args[0]));
        }
        else if (name.Equals("sin"))
        {
            result = Math.Sin(Evaluate(args[0]));
        }
        else if (name.Equals("sinh"))
        {
            result = Math.Sinh(Evaluate(args[0]));
        }
        else if (name.Equals("sqrt"))
        {
            result = Math.Sqrt(Evaluate(args[0]));
        }
        else if (name.Equals("tan"))
        {
            result = Math.Tan(Evaluate(args[0]));
        }
        else if (name.Equals("tanh"))
        {
            result = Math.Tanh(Evaluate(args[0]));
        }
        else if (name.Equals("truncate"))
        {
            result = Math.Truncate(Evaluate(args[0]));
        }
        else if (name.Equals("if"))
        {
            if (Evaluate(args[0]) == 0)
                result = Evaluate(args[2]);
            else
                result = Evaluate(args[1]);
        }
        else if (name.Equals("in"))
        {
            double valueToFind = Evaluate(args[0]);

            result = args.Skip(1).ToList()
                .ConvertAll(arg => Evaluate(arg))
                .Contains(valueToFind) ? 1d : 0d;
        }
        else
        {
            result = double.NaN;
            functionExists = false;
        }

        return functionExists;
    }
}

