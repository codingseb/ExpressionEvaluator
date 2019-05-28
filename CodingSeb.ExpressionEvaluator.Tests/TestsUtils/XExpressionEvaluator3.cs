using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace CodingSeb.ExpressionEvaluator.Tests
{
    public class XExpressionEvaluator3 : ExpressionEvaluator
    {
        protected override void Init()
        {
            ParsingMethods.Insert(0, EvaluateDateTimeSyntax);
            ParsingMethods.Add(EvaluateSpecialTernaryOperator);
        }

        /// <summary>
        /// To evaluate DateTimes objects with #year-month-day syntax (#2019-05-28)
        /// </summary>
        protected virtual bool EvaluateDateTimeSyntax(string expression, Stack<object> stack, ref int i)
        {
            Match match = Regex.Match(expression.Substring(i), @"^\s*#(?<year>\d{4})-(?<month>\d{2})-(?<day>\d{2})");

            if(match.Success)
            {
                int year = int.Parse(match.Groups["year"].Value);
                int month = int.Parse(match.Groups["month"].Value);
                int day = int.Parse(match.Groups["day"].Value);

                DateTime dateTime = new DateTime(year,month, day);

                stack.Push(dateTime);

                i += match.Length - 1;

                return true;
            }

            return false;
        }

        /// <summary>
        /// To evaluate a string replace with custom ternary indicator
        /// </summary>
        protected virtual bool EvaluateSpecialTernaryOperator(string expression, Stack<object> stack, ref int i)
        {
            if (expression.Substring(i, 1).Equals("°"))
            {
                string input = (string)ProcessStack(stack);

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
                    else if (s2.Equals("@"))
                    {
                        stack.Clear();

                        stack.Push(input.Replace((string)Evaluate(restOfExpression.Substring(1, j - 1)), (string)Evaluate(restOfExpression.Substring(j + 1))));

                        i = expression.Length;

                        return true;
                    }
                }
            }

            return false;
        }
    }
}
