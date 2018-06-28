using NUnit.Framework;
using Should;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace CodingSeb.ExpressionEvaluator.Tests
{
    [TestFixture]
    public class ExpressionEvaluatorScriptEvaluateTests
    {
        private static Regex removeAllWhiteSpacesRegex = new Regex(@"\s+");

        public static IEnumerable<TestCaseData> TestCasesForScriptEvaluateTests
        {
            get
            {
                // while
                yield return new TestCaseData(Resources.Script0001 , null, true).SetCategory("Script").SetCategory("while").SetCategory("variable assignation").SetCategory("++").SetCategory("+=").Returns("0,1,2,3,4");
                yield return new TestCaseData(removeAllWhiteSpacesRegex.Replace(Resources.Script0001, "") , null, true).SetCategory("Script").SetCategory("while").SetCategory("variable assignation").SetCategory("++").SetCategory("+=").Returns("0,1,2,3,4");

                // for
                yield return new TestCaseData(Resources.Script0002 , null, true).SetCategory("Script").SetCategory("for").SetCategory("variable assignation").SetCategory("++").SetCategory("+=").Returns("0,1,2,3,4");
                yield return new TestCaseData(removeAllWhiteSpacesRegex.Replace(Resources.Script0002, "") , null, true).SetCategory("Script").SetCategory("for").SetCategory("variable assignation").SetCategory("++").SetCategory("+=").Returns("0,1,2,3,4");
                yield return new TestCaseData(Resources.Script0003 , null, true).SetCategory("Script").SetCategory("for").SetCategory("variable assignation").SetCategory("++").SetCategory("+=").Returns("0,1,2,3,4");
                yield return new TestCaseData(removeAllWhiteSpacesRegex.Replace(Resources.Script0003, "") , null, true).SetCategory("Script").SetCategory("for").SetCategory("variable assignation").SetCategory("++").SetCategory("+=").Returns("0,1,2,3,4");

                // if else  else
                yield return new TestCaseData(Resources.Script0004.Replace("[valx]", "0").Replace("[valy]", "1") , null, true).SetCategory("Script").SetCategory("if").SetCategory("variable assignation").Returns(1);
                yield return new TestCaseData(removeAllWhiteSpacesRegex.Replace(Resources.Script0004.Replace("[valx]", "0").Replace("[valy]", "1"), "").Replace("else", "else ") , null, true).SetCategory("Script").SetCategory("if").SetCategory("variable assignation").Returns(1);
                yield return new TestCaseData(Resources.Script0004.Replace("[valx]", "-1").Replace("[valy]", "1") , null, true).SetCategory("Script").SetCategory("if").SetCategory("variable assignation").Returns(1);
                yield return new TestCaseData(removeAllWhiteSpacesRegex.Replace(Resources.Script0004.Replace("[valx]", "-1").Replace("[valy]", "1"), "").Replace("else", "else "), null, true).SetCategory("Script").SetCategory("if").SetCategory("variable assignation").Returns(1);
                yield return new TestCaseData(Resources.Script0004.Replace("[valx]", "1").Replace("[valy]", "1") , null, true).SetCategory("Script").SetCategory("if").SetCategory("variable assignation").Returns(1);
                yield return new TestCaseData(removeAllWhiteSpacesRegex.Replace(Resources.Script0004.Replace("[valx]", "1").Replace("[valy]", "1"), "").Replace("else", "else "), null, true).SetCategory("Script").SetCategory("if").SetCategory("variable assignation").Returns(1);

                yield return new TestCaseData(Resources.Script0004.Replace("[valx]", "0").Replace("[valy]", "0") , null, true).SetCategory("Script").SetCategory("if").SetCategory("variable assignation").Returns(2);
                yield return new TestCaseData(removeAllWhiteSpacesRegex.Replace(Resources.Script0004.Replace("[valx]", "0").Replace("[valy]", "0"), "").Replace("else", "else ") , null, true).SetCategory("Script").SetCategory("if").SetCategory("variable assignation").Returns(2);

                yield return new TestCaseData(Resources.Script0004.Replace("[valx]", "-1").Replace("[valy]", "0") , null, true).SetCategory("Script").SetCategory("if").SetCategory("variable assignation").Returns(3);
                yield return new TestCaseData(removeAllWhiteSpacesRegex.Replace(Resources.Script0004.Replace("[valx]", "-1").Replace("[valy]", "0"), "").Replace("else", "else "), null, true).SetCategory("Script").SetCategory("if").SetCategory("variable assignation").Returns(3);

                yield return new TestCaseData(Resources.Script0004.Replace("[valx]", "1").Replace("[valy]", "0") , null, true).SetCategory("Script").SetCategory("if").SetCategory("variable assignation").Returns(4);
                yield return new TestCaseData(removeAllWhiteSpacesRegex.Replace(Resources.Script0004.Replace("[valx]", "1").Replace("[valy]", "0"), "").Replace("else", "else "), null, true).SetCategory("Script").SetCategory("if").SetCategory("variable assignation").Returns(4);

                yield return new TestCaseData(Resources.Script0005.Replace("[valx]", "0").Replace("[valy]", "1"), null, true).SetCategory("Script").SetCategory("if").SetCategory("variable assignation").Returns(1);
                yield return new TestCaseData(removeAllWhiteSpacesRegex.Replace(Resources.Script0005.Replace("[valx]", "0").Replace("[valy]", "1"), "").Replace("else", "else "), null, true).SetCategory("Script").SetCategory("if").SetCategory("variable assignation").Returns(1);
                yield return new TestCaseData(Resources.Script0005.Replace("[valx]", "-1").Replace("[valy]", "1"), null, true).SetCategory("Script").SetCategory("if").SetCategory("variable assignation").Returns(1);
                yield return new TestCaseData(removeAllWhiteSpacesRegex.Replace(Resources.Script0005.Replace("[valx]", "-1").Replace("[valy]", "1"), "").Replace("else", "else "), null, true).SetCategory("Script").SetCategory("if").SetCategory("variable assignation").Returns(1);
                yield return new TestCaseData(Resources.Script0005.Replace("[valx]", "1").Replace("[valy]", "1"), null, true).SetCategory("Script").SetCategory("if").SetCategory("variable assignation").Returns(1);
                yield return new TestCaseData(removeAllWhiteSpacesRegex.Replace(Resources.Script0005.Replace("[valx]", "1").Replace("[valy]", "1"), "").Replace("else", "else "), null, true).SetCategory("Script").SetCategory("if").SetCategory("variable assignation").Returns(1);

                yield return new TestCaseData(Resources.Script0005.Replace("[valx]", "0").Replace("[valy]", "0"), null, true).SetCategory("Script").SetCategory("if").SetCategory("variable assignation").Returns(2);
                yield return new TestCaseData(removeAllWhiteSpacesRegex.Replace(Resources.Script0005.Replace("[valx]", "0").Replace("[valy]", "0"), "").Replace("else", "else "), null, true).SetCategory("Script").SetCategory("if").SetCategory("variable assignation").Returns(2);

                yield return new TestCaseData(Resources.Script0005.Replace("[valx]", "-1").Replace("[valy]", "0"), null, true).SetCategory("Script").SetCategory("if").SetCategory("variable assignation").Returns(3);
                yield return new TestCaseData(removeAllWhiteSpacesRegex.Replace(Resources.Script0005.Replace("[valx]", "-1").Replace("[valy]", "0"), "").Replace("else", "else "), null, true).SetCategory("Script").SetCategory("if").SetCategory("variable assignation").Returns(3);

                yield return new TestCaseData(Resources.Script0005.Replace("[valx]", "1").Replace("[valy]", "0"), null, true).SetCategory("Script").SetCategory("if").SetCategory("variable assignation").Returns(4);
                yield return new TestCaseData(removeAllWhiteSpacesRegex.Replace(Resources.Script0005.Replace("[valx]", "1").Replace("[valy]", "0"), "").Replace("else", "else "), null, true).SetCategory("Script").SetCategory("if").SetCategory("variable assignation").Returns(4);
            }
        }

        [TestCaseSource(nameof(TestCasesForScriptEvaluateTests))]
        public object TestCasesForScriptEvaluate(string script, Dictionary<string, object> variables, bool caseSensitiveEvaluation)
        {
            ExpressionEvaluator evaluator = new ExpressionEvaluator()
            {
                OptionCaseSensitiveEvaluationActive = caseSensitiveEvaluation,
                Variables = variables
            };

            evaluator.Namespaces.Add("CodingSeb.ExpressionEvaluator.Tests");

            return evaluator.ScriptEvaluate(script);
        }
    }
}
