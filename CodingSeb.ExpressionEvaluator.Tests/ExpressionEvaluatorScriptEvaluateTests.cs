using NUnit.Framework;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace CodingSeb.ExpressionEvaluator.Tests
{
    [TestFixture]
    public class ExpressionEvaluatorScriptEvaluateTests
    {
        private static Regex removeAllWhiteSpacesRegex = new Regex(@"\s+");

        #region Scripts that must succeed

        public static IEnumerable<TestCaseData> TestCasesForScriptEvaluateTests
        {
            get
            {
                #region while

                yield return new TestCaseData(Resources.Script0001 , null, true).SetCategory("Script").SetCategory("while").SetCategory("variable assignation").SetCategory("++").SetCategory("+=").Returns("0,1,2,3,4");
                yield return new TestCaseData(removeAllWhiteSpacesRegex.Replace(Resources.Script0001, "") , null, true).SetCategory("Script").SetCategory("while").SetCategory("variable assignation").SetCategory("++").SetCategory("+=").Returns("0,1,2,3,4");

                #endregion

                #region for

                yield return new TestCaseData(Resources.Script0002 , null, true).SetCategory("Script").SetCategory("for").SetCategory("variable assignation").SetCategory("++").SetCategory("+=").Returns("0,1,2,3,4");
                yield return new TestCaseData(removeAllWhiteSpacesRegex.Replace(Resources.Script0002, "") , null, true).SetCategory("Script").SetCategory("for").SetCategory("variable assignation").SetCategory("++").SetCategory("+=").Returns("0,1,2,3,4");
                yield return new TestCaseData(Resources.Script0003 , null, true).SetCategory("Script").SetCategory("for").SetCategory("variable assignation").SetCategory("++").SetCategory("+=").Returns("0,1,2,3,4");
                yield return new TestCaseData(removeAllWhiteSpacesRegex.Replace(Resources.Script0003, "") , null, true).SetCategory("Script").SetCategory("for").SetCategory("variable assignation").SetCategory("++").SetCategory("+=").Returns("0,1,2,3,4");

                #endregion

                #region if, else if, else

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

                #endregion
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

        #endregion

        #region Remove Comments

        public static IEnumerable<TestCaseData> TestCasesForRemoveCommentsTests
        {
            get
            {
                yield return new TestCaseData("// simple line comment").SetCategory("RemoveComments").Returns(" ");
                yield return new TestCaseData("/* simple block comment */").SetCategory("RemoveComments").Returns(" ");
                yield return new TestCaseData("/* multi line\r\nblock comment */").SetCategory("RemoveComments").Returns("\r\n");
                yield return new TestCaseData("/* multi line\rblock comment */").SetCategory("RemoveComments").Returns("\r");
                yield return new TestCaseData("/* multi line\nblock comment */").SetCategory("RemoveComments").Returns("\n");
                yield return new TestCaseData(@"a = ""apple""; // test").SetCategory("RemoveComments").Returns(@"a = ""apple"";  ");
                yield return new TestCaseData(@"a = ""apple""; /* test */").SetCategory("RemoveComments").Returns(@"a = ""apple"";  ");
                yield return new TestCaseData(@"// /*comment within comments */").SetCategory("RemoveComments").Returns(@" ");
                yield return new TestCaseData(@"/* //comment within comments */").SetCategory("RemoveComments").Returns(@" ");
                yield return new TestCaseData(@"// bla bla /*comment within comments */  bla bla").SetCategory("RemoveComments").Returns(@" ");
                yield return new TestCaseData(@"/* bla bla //comment within comments */").SetCategory("RemoveComments").Returns(@" ");
                yield return new TestCaseData(@"// ""bla bla"" ").SetCategory("RemoveComments").Returns(@" ");
                yield return new TestCaseData(@"/* ""bla bla"" */").SetCategory("RemoveComments").Returns(@" ");
                yield return new TestCaseData(@"""// test """).SetCategory("RemoveComments").SetCategory("Not a comments").Returns(@"""// test """);
                yield return new TestCaseData(@"""/* test */""").SetCategory("RemoveComments").SetCategory("Not a comments").Returns(@"""/* test */""");
                yield return new TestCaseData(@"""bla bla // test """).SetCategory("RemoveComments").SetCategory("Not a comments").Returns(@"""bla bla // test """);
                yield return new TestCaseData(@"""bla bla /* test */ bla bla""").SetCategory("RemoveComments").SetCategory("Not a comments").Returns(@"""bla bla /* test */ bla bla""");
            }
        }

        [TestCaseSource(nameof(TestCasesForRemoveCommentsTests))]
        public string RemoveCommentsTests(string script)
        {
            ExpressionEvaluator evaluator = new ExpressionEvaluator();

            return evaluator.RemoveComments(script);
        }

        #endregion
    }
}
