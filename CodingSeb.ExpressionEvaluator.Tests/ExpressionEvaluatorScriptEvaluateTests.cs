using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Should;

namespace CodingSeb.ExpressionEvaluator.Tests
{
    [TestFixture]
    public class ExpressionEvaluatorScriptEvaluateTests
    {
        private static Regex removeAllWhiteSpacesRegex = new Regex(@"\s+");

        #region Setup TearDown

        [SetUp]
        public static void BeforeTests()
        {
            ClassForTest1.StaticIntProperty = 67;
        }

        [TearDown]
        public static void AfterTests()
        {
            ClassForTest1.StaticIntProperty = 67;
        }

        #endregion

        #region Scripts that must succeed

        public static IEnumerable<TestCaseData> TestCasesForScriptEvaluateTests
        {
            get
            {
                #region while

                yield return new TestCaseData(Resources.Script0001, null)
                    .SetCategory("Script")
                    .SetCategory("while")
                    .SetCategory("variable assignation")
                    .SetCategory("++")
                    .SetCategory("+=")
                    .Returns("0,1,2,3,4");
                yield return new TestCaseData(removeAllWhiteSpacesRegex.Replace(Resources.Script0001, string.Empty), null)
                    .SetCategory("Script")
                    .SetCategory("while")
                    .SetCategory("variable assignation")
                    .SetCategory("++")
                    .SetCategory("+=")
                    .Returns("0,1,2,3,4");

                #endregion

                #region for

                yield return new TestCaseData(Resources.Script0002, null)
                    .SetCategory("Script")
                    .SetCategory("for")
                    .SetCategory("variable assignation")
                    .SetCategory("++")
                    .SetCategory("+=")
                    .Returns("0,1,2,3,4");
                yield return new TestCaseData(removeAllWhiteSpacesRegex.Replace(Resources.Script0002, string.Empty), null)
                    .SetCategory("Script")
                    .SetCategory("for")
                    .SetCategory("variable assignation")
                    .SetCategory("++")
                    .SetCategory("+=")
                    .Returns("0,1,2,3,4");
                yield return new TestCaseData(Resources.Script0003, null)
                    .SetCategory("Script")
                    .SetCategory("for")
                    .SetCategory("variable assignation")
                    .SetCategory("++")
                    .SetCategory("+=")
                    .Returns("0,1,2,3,4");
                yield return new TestCaseData(removeAllWhiteSpacesRegex.Replace(Resources.Script0003, string.Empty), null)
                    .SetCategory("Script")
                    .SetCategory("for")
                    .SetCategory("variable assignation")
                    .SetCategory("++")
                    .SetCategory("+=")
                    .Returns("0,1,2,3,4");

                #endregion

                #region if, else if, else

                yield return new TestCaseData(Resources.Script0004.Replace("[valx]", "0").Replace("[valy]", "1"), null)
                    .SetCategory("Script")
                    .SetCategory("if")
                    .SetCategory("variable assignation")
                    .Returns(1);
                yield return new TestCaseData(removeAllWhiteSpacesRegex.Replace(Resources.Script0004.Replace("[valx]", "0").Replace("[valy]", "1"), string.Empty).Replace("else", "else "), null)
                    .SetCategory("Script")
                    .SetCategory("if")
                    .SetCategory("variable assignation")
                    .Returns(1);
                yield return new TestCaseData(Resources.Script0004.Replace("[valx]", "-1").Replace("[valy]", "1"), null)
                    .SetCategory("Script")
                    .SetCategory("if")
                    .SetCategory("variable assignation")
                    .Returns(1);
                yield return new TestCaseData(removeAllWhiteSpacesRegex.Replace(Resources.Script0004.Replace("[valx]", "-1").Replace("[valy]", "1"), string.Empty).Replace("else", "else "), null)
                    .SetCategory("Script")
                    .SetCategory("if")
                    .SetCategory("variable assignation")
                    .Returns(1);
                yield return new TestCaseData(Resources.Script0004.Replace("[valx]", "1").Replace("[valy]", "1"), null)
                    .SetCategory("Script")
                    .SetCategory("if")
                    .SetCategory("variable assignation")
                    .Returns(1);
                yield return new TestCaseData(removeAllWhiteSpacesRegex.Replace(Resources.Script0004.Replace("[valx]", "1").Replace("[valy]", "1"), string.Empty).Replace("else", "else "), null)
                    .SetCategory("Script")
                    .SetCategory("if")
                    .SetCategory("variable assignation")
                    .Returns(1);

                yield return new TestCaseData(Resources.Script0004.Replace("[valx]", "0").Replace("[valy]", "0"), null)
                    .SetCategory("Script")
                    .SetCategory("if")
                    .SetCategory("variable assignation")
                    .Returns(2);
                yield return new TestCaseData(removeAllWhiteSpacesRegex.Replace(Resources.Script0004.Replace("[valx]", "0").Replace("[valy]", "0"), string.Empty).Replace("else", "else "), null)
                    .SetCategory("Script")
                    .SetCategory("if")
                    .SetCategory("variable assignation")
                    .Returns(2);

                yield return new TestCaseData(Resources.Script0004.Replace("[valx]", "-1").Replace("[valy]", "0"), null)
                    .SetCategory("Script")
                    .SetCategory("if")
                    .SetCategory("variable assignation")
                    .Returns(3);
                yield return new TestCaseData(removeAllWhiteSpacesRegex.Replace(Resources.Script0004.Replace("[valx]", "-1").Replace("[valy]", "0"), string.Empty).Replace("else", "else "), null)
                    .SetCategory("Script")
                    .SetCategory("if")
                    .SetCategory("variable assignation")
                    .Returns(3);

                yield return new TestCaseData(Resources.Script0004.Replace("[valx]", "1").Replace("[valy]", "0"), null)
                    .SetCategory("Script")
                    .SetCategory("if")
                    .SetCategory("variable assignation")
                    .Returns(4);
                yield return new TestCaseData(removeAllWhiteSpacesRegex.Replace(Resources.Script0004.Replace("[valx]", "1").Replace("[valy]", "0"), string.Empty).Replace("else", "else "), null)
                    .SetCategory("Script")
                    .SetCategory("if")
                    .SetCategory("variable assignation")
                    .Returns(4);

                yield return new TestCaseData(Resources.Script0005.Replace("[valx]", "0").Replace("[valy]", "1"), null)
                    .SetCategory("Script")
                    .SetCategory("if")
                    .SetCategory("variable assignation")
                    .Returns(1);
                yield return new TestCaseData(removeAllWhiteSpacesRegex.Replace(Resources.Script0005.Replace("[valx]", "0").Replace("[valy]", "1"), string.Empty).Replace("else", "else "), null)
                    .SetCategory("Script")
                    .SetCategory("if")
                    .SetCategory("variable assignation")
                    .Returns(1);
                yield return new TestCaseData(Resources.Script0005.Replace("[valx]", "-1").Replace("[valy]", "1"), null)
                    .SetCategory("Script")
                    .SetCategory("if")
                    .SetCategory("variable assignation")
                    .Returns(1);
                yield return new TestCaseData(removeAllWhiteSpacesRegex.Replace(Resources.Script0005.Replace("[valx]", "-1").Replace("[valy]", "1"), string.Empty).Replace("else", "else "), null)
                    .SetCategory("Script")
                    .SetCategory("if")
                    .SetCategory("variable assignation")
                    .Returns(1);
                yield return new TestCaseData(Resources.Script0005.Replace("[valx]", "1").Replace("[valy]", "1"), null)
                    .SetCategory("Script")
                    .SetCategory("if")
                    .SetCategory("variable assignation")
                    .Returns(1);
                yield return new TestCaseData(removeAllWhiteSpacesRegex.Replace(Resources.Script0005.Replace("[valx]", "1").Replace("[valy]", "1"), string.Empty).Replace("else", "else "), null)
                    .SetCategory("Script")
                    .SetCategory("if")
                    .SetCategory("variable assignation")
                    .Returns(1);

                yield return new TestCaseData(Resources.Script0005.Replace("[valx]", "0").Replace("[valy]", "0"), null)
                    .SetCategory("Script")
                    .SetCategory("if")
                    .SetCategory("variable assignation")
                    .Returns(2);
                yield return new TestCaseData(removeAllWhiteSpacesRegex.Replace(Resources.Script0005.Replace("[valx]", "0").Replace("[valy]", "0"), string.Empty).Replace("else", "else "), null)
                    .SetCategory("Script")
                    .SetCategory("if")
                    .SetCategory("variable assignation")
                    .Returns(2);

                yield return new TestCaseData(Resources.Script0005.Replace("[valx]", "-1").Replace("[valy]", "0"), null)
                    .SetCategory("Script")
                    .SetCategory("if")
                    .SetCategory("variable assignation")
                    .Returns(3);
                yield return new TestCaseData(removeAllWhiteSpacesRegex.Replace(Resources.Script0005.Replace("[valx]", "-1").Replace("[valy]", "0"), string.Empty).Replace("else", "else "), null)
                    .SetCategory("Script")
                    .SetCategory("if")
                    .SetCategory("variable assignation")
                    .Returns(3);

                yield return new TestCaseData(Resources.Script0005.Replace("[valx]", "1").Replace("[valy]", "0"), null)
                    .SetCategory("Script")
                    .SetCategory("if")
                    .SetCategory("variable assignation")
                    .Returns(4);
                yield return new TestCaseData(removeAllWhiteSpacesRegex.Replace(Resources.Script0005.Replace("[valx]", "1").Replace("[valy]", "0"), string.Empty).Replace("else", "else "), null)
                    .SetCategory("Script")
                    .SetCategory("if")
                    .SetCategory("variable assignation")
                    .Returns(4);

                #endregion

                #region block for lambda body

                yield return new TestCaseData(Resources.Script0006, null)
                    .SetCategory("Script")
                    .SetCategory("lambda")
                    .SetCategory("variable assignation")
                    .SetCategory("block for lambda body")
                    .Returns(3);
                yield return new TestCaseData(removeAllWhiteSpacesRegex.Replace(Resources.Script0006, string.Empty), null)
                    .SetCategory("Script")
                    .SetCategory("lambda")
                    .SetCategory("variable assignation")
                    .SetCategory("block for lambda body")
                    .Returns(3);

                #endregion

                #region return keyword and OptionAutoReturnLastExpressionResultInScriptsActive tests 

                yield return new TestCaseData(Resources.Script0008.Replace("[valx]", "1"), null)
                    .SetCategory("Script")
                    .SetCategory("return")
                    .SetCategory("if")
                    .SetCategory("variable assignation")
                    .Returns(1);
                yield return new TestCaseData(removeAllWhiteSpacesRegex.Replace(Resources.Script0008.Replace("[valx]", "1"), string.Empty).Replace("return", "return "), null)
                    .SetCategory("Script")
                    .SetCategory("return")
                    .SetCategory("if")
                    .SetCategory("variable assignation")
                    .Returns(1);
                yield return new TestCaseData(Resources.Script0008.Replace("[valx]", "2"), null)
                    .SetCategory("Script")
                    .SetCategory("return")
                    .SetCategory("if")
                    .SetCategory("variable assignation")
                    .Returns(2);
                yield return new TestCaseData(removeAllWhiteSpacesRegex.Replace(Resources.Script0008.Replace("[valx]", "2"), string.Empty).Replace("return", "return "), null)
                    .SetCategory("Script")
                    .SetCategory("return")
                    .SetCategory("if")
                    .SetCategory("variable assignation")
                    .Returns(2);
                yield return new TestCaseData(Resources.Script0008.Replace("[valx]", "0"), null)
                    .SetCategory("Script")
                    .SetCategory("return")
                    .SetCategory("if")
                    .SetCategory("variable assignation")
                    .Returns(2);
                yield return new TestCaseData(removeAllWhiteSpacesRegex.Replace(Resources.Script0008.Replace("[valx]", "0"), string.Empty).Replace("return", "return "), null)
                    .SetCategory("Script")
                    .SetCategory("return")
                    .SetCategory("if")
                    .SetCategory("variable assignation")
                    .Returns(2);

                ExpressionEvaluator evaluator = new ExpressionEvaluator()
                {
                    OptionOnNoReturnKeywordFoundInScriptAction = OptionOnNoReturnKeywordFoundInScriptAction.ReturnNull
                };

                yield return new TestCaseData(Resources.Script0008.Replace("[valx]", "1"), evaluator)
                    .SetCategory("Script")
                    .SetCategory("return")
                    .SetCategory("if")
                    .SetCategory("variable assignation")
                    .SetCategory("Options")
                    .SetCategory("OptionOnNoReturnKeywordFoundInScriptAction = ReturnNull")
                    .Returns(null);
                yield return new TestCaseData(removeAllWhiteSpacesRegex.Replace(Resources.Script0008.Replace("[valx]", "1"), string.Empty).Replace("return", "return "), evaluator)
                    .SetCategory("Script")
                    .SetCategory("return")
                    .SetCategory("if")
                    .SetCategory("variable assignation")
                    .SetCategory("Options")
                    .SetCategory("OptionOnNoReturnKeywordFoundInScriptAction = ReturnNull")
                    .Returns(null);
                yield return new TestCaseData(Resources.Script0008.Replace("[valx]", "2"), evaluator)
                    .SetCategory("Script")
                    .SetCategory("return")
                    .SetCategory("if")
                    .SetCategory("variable assignation")
                    .SetCategory("Options")
                    .SetCategory("OptionOnNoReturnKeywordFoundInScriptAction = ReturnNull")
                    .Returns(null);
                yield return new TestCaseData(removeAllWhiteSpacesRegex.Replace(Resources.Script0008.Replace("[valx]", "2"), string.Empty).Replace("return", "return "), evaluator)
                    .SetCategory("Script")
                    .SetCategory("return")
                    .SetCategory("if")
                    .SetCategory("variable assignation")
                    .SetCategory("Options")
                    .SetCategory("OptionOnNoReturnKeywordFoundInScriptAction = ReturnNull")
                    .Returns(null);
                yield return new TestCaseData(Resources.Script0008.Replace("[valx]", "0"), evaluator)
                    .SetCategory("Script")
                    .SetCategory("return")
                    .SetCategory("if")
                    .SetCategory("variable assignation")
                    .SetCategory("Options")
                    .SetCategory("OptionOnNoReturnKeywordFoundInScriptAction = ReturnNull")
                    .Returns(2);
                yield return new TestCaseData(removeAllWhiteSpacesRegex.Replace(Resources.Script0008.Replace("[valx]", "0"), string.Empty).Replace("return", "return "), evaluator)
                    .SetCategory("Script")
                    .SetCategory("return")
                    .SetCategory("if")
                    .SetCategory("variable assignation")
                    .SetCategory("Options")
                    .SetCategory("OptionOnNoReturnKeywordFoundInScriptAction = ReturnNull")
                    .Returns(2);

                evaluator = new ExpressionEvaluator()
                {
                    OptionOnNoReturnKeywordFoundInScriptAction = OptionOnNoReturnKeywordFoundInScriptAction.ThrowSyntaxException
                };

                yield return new TestCaseData(Resources.Script0008.Replace("[valx]", "0"), evaluator)
                    .SetCategory("Script")
                    .SetCategory("return")
                    .SetCategory("if")
                    .SetCategory("variable assignation")
                    .SetCategory("Options")
                    .SetCategory("OptionOnNoReturnKeywordFoundInScriptAction = ReturnNull")
                    .Returns(2);
                yield return new TestCaseData(removeAllWhiteSpacesRegex.Replace(Resources.Script0008.Replace("[valx]", "0"), string.Empty).Replace("return", "return "), evaluator)
                    .SetCategory("Script")
                    .SetCategory("return")
                    .SetCategory("if")
                    .SetCategory("variable assignation")
                    .SetCategory("Options")
                    .SetCategory("OptionOnNoReturnKeywordFoundInScriptAction = ReturnNull")
                    .Returns(2);

                #endregion

                #region More complex script

                yield return new TestCaseData(Resources.Script0007, null)
                    .SetCategory("Script")
                    .SetCategory("lambda")
                    .SetCategory("variable assignation")
                    .SetCategory("if")
                    .SetCategory("block for lambda body")
                    .Returns(13);
                yield return new TestCaseData(removeAllWhiteSpacesRegex.Replace(Resources.Script0007, string.Empty), null)
                    .SetCategory("Script")
                    .SetCategory("lambda")
                    .SetCategory("variable assignation")
                    .SetCategory("if")
                    .SetCategory("block for lambda body")
                    .Returns(13);
                yield return new TestCaseData(Resources.Script0009, null)
                    .SetCategory("Script")
                    .SetCategory("variable assignation")
                    .SetCategory("if")
                    .SetCategory("continue in for")
                    .SetCategory("break in for")
                    .Returns("0,1,2,4,5,6,");
                yield return new TestCaseData(removeAllWhiteSpacesRegex.Replace(Resources.Script0009, string.Empty), null)
                    .SetCategory("Script")
                    .SetCategory("variable assignation")
                    .SetCategory("if")
                    .SetCategory("continue in for")
                    .SetCategory("break in for")
                    .Returns("0,1,2,4,5,6,");
                yield return new TestCaseData(Resources.Script0010, null)
                    .SetCategory("Script")
                    .SetCategory("variable assignation")
                    .SetCategory("if")
                    .SetCategory("continue in while")
                    .SetCategory("break in while")
                    .Returns("0,1,2,4,5,6,");
                yield return new TestCaseData(removeAllWhiteSpacesRegex.Replace(Resources.Script0010, string.Empty), null)
                    .SetCategory("Script")
                    .SetCategory("variable assignation")
                    .SetCategory("if")
                    .SetCategory("continue in while")
                    .SetCategory("break in while")
                    .Returns("0,1,2,4,5,6,");

                #endregion
            }
        }

        [TestCaseSource(nameof(TestCasesForScriptEvaluateTests))]
        public object TestCasesForScriptEvaluate(string script, ExpressionEvaluator evaluator)
        {
            evaluator = evaluator ?? new ExpressionEvaluator();

            evaluator.Namespaces.Add("CodingSeb.ExpressionEvaluator.Tests");

            return evaluator.ScriptEvaluate(evaluator.RemoveComments(script));
        }

        #endregion

        #region Exception Throwing Evaluation

        public static IEnumerable<TestCaseData> TestCasesForExceptionThrowingScriptEvaluation
        {
            get
            {
                #region Options

                #region OptionOnNoReturnKeywordFoundInScriptAction = OptionOnNoReturnKeywordFoundInScriptAction.ThrowSyntaxException

                ExpressionEvaluator evaluator = new ExpressionEvaluator()
                {
                    OptionOnNoReturnKeywordFoundInScriptAction = OptionOnNoReturnKeywordFoundInScriptAction.ThrowSyntaxException
                };

                yield return new TestCaseData(evaluator, Resources.Script0008.Replace("[valx]", "1"), typeof(ExpressionEvaluatorSyntaxErrorException))
                    .SetCategory("Script")
                    .SetCategory("return")
                    .SetCategory("if")
                    .SetCategory("variable assignation")
                    .SetCategory("Options")
                    .SetCategory("OptionOnNoReturnKeywordFoundInScriptAction = ThrowSyntaxException");
                yield return new TestCaseData(evaluator, Resources.Script0008.Replace("[valx]", "2"), typeof(ExpressionEvaluatorSyntaxErrorException))
                    .SetCategory("Script")
                    .SetCategory("return")
                    .SetCategory("if")
                    .SetCategory("variable assignation")
                    .SetCategory("Options")
                    .SetCategory("OptionOnNoReturnKeywordFoundInScriptAction = ThrowSyntaxException");

                #endregion

                #endregion
            }
        }

        [TestCaseSource(nameof(TestCasesForExceptionThrowingScriptEvaluation))]
        public void ExceptionThrowingScriptEvaluation(ExpressionEvaluator evaluator, string script, Type exceptionType)
        {
            Assert.Catch(exceptionType, () => evaluator.ScriptEvaluate(script));
        }

        #endregion

        #region Properties set assignation

        [Test]
        [Category("Script")]
        [Category("Instance Property set assignation")]
        [Category("=")]
        public static void InstancePropertySetEqual()
        {
            ClassForTest1 obj = new ClassForTest1();

            ExpressionEvaluator evaluator = new ExpressionEvaluator()
            {
                Variables = new Dictionary<string, object>()
                {
                    {"obj", obj }
                }
            };

            obj.IntProperty.ShouldEqual(25);

            evaluator.ScriptEvaluate("obj.IntProperty = 3;");

            obj.IntProperty.ShouldEqual(3);
        }

        [Test]
        [Category("Script")]
        [Category("Instance Property set assignation")]
        [Category("+=")]
        public static void InstancePropertySetPlusEqual()
        {
            ClassForTest1 obj = new ClassForTest1();

            ExpressionEvaluator evaluator = new ExpressionEvaluator()
            {
                Variables = new Dictionary<string, object>()
                {
                    {"obj", obj }
                }
            };

            obj.IntProperty.ShouldEqual(25);

            evaluator.ScriptEvaluate("obj.IntProperty += 2;");

            obj.IntProperty.ShouldEqual(27);
        }

        [Test]
        [Category("Script")]
        [Category("Instance Property set assignation")]
        [Category("-=")]
        public static void InstancePropertySetMinusEqual()
        {
            ClassForTest1 obj = new ClassForTest1();

            ExpressionEvaluator evaluator = new ExpressionEvaluator()
            {
                Variables = new Dictionary<string, object>()
                {
                    {"obj", obj }
                }
            };

            obj.IntProperty.ShouldEqual(25);

            evaluator.ScriptEvaluate("obj.IntProperty -= 2;");

            obj.IntProperty.ShouldEqual(23);
        }

        [Test]
        [Category("Script")]
        [Category("Instance Property set assignation")]
        [Category("*=")]
        public static void InstancePropertySetMultiplyEqual()
        {
            ClassForTest1 obj = new ClassForTest1();

            ExpressionEvaluator evaluator = new ExpressionEvaluator()
            {
                Variables = new Dictionary<string, object>()
                {
                    {"obj", obj }
                }
            };

            obj.IntProperty.ShouldEqual(25);

            evaluator.ScriptEvaluate("obj.IntProperty *= 2;");

            obj.IntProperty.ShouldEqual(50);
        }

        [Test]
        [Category("Script")]
        [Category("Instance Property set assignation")]
        [Category("/=")]
        public static void InstancePropertySetDividEqual()
        {
            ClassForTest1 obj = new ClassForTest1()
            {
                IntProperty = 50
            };

            ExpressionEvaluator evaluator = new ExpressionEvaluator()
            {
                Variables = new Dictionary<string, object>()
                {
                    {"obj", obj }
                }
            };

            obj.IntProperty.ShouldEqual(50);

            evaluator.ScriptEvaluate("obj.IntProperty /= 5;");

            obj.IntProperty.ShouldEqual(10);
        }

        [Test]
        [Category("Script")]
        [Category("Instance Property set assignation")]
        [Category("%=")]
        public static void InstancePropertySetModuloEqual()
        {
            ClassForTest1 obj = new ClassForTest1();

            ExpressionEvaluator evaluator = new ExpressionEvaluator()
            {
                Variables = new Dictionary<string, object>()
                {
                    {"obj", obj }
                }
            };

            obj.IntProperty.ShouldEqual(25);

            evaluator.ScriptEvaluate("obj.IntProperty %= 5;");

            obj.IntProperty.ShouldEqual(0);

            obj.IntProperty = 10;

            evaluator.ScriptEvaluate("obj.IntProperty %= 3;");

            obj.IntProperty.ShouldEqual(1);
        }

        [Test]
        [Category("Script")]
        [Category("Instance Property set assignation")]
        [Category("&=")]
        public static void InstancePropertySetAndEqual()
        {
            ClassForTest1 obj = new ClassForTest1()
            {
                IntProperty = 15
            };

            ExpressionEvaluator evaluator = new ExpressionEvaluator()
            {
                Variables = new Dictionary<string, object>()
                {
                    {"obj", obj }
                }
            };

            obj.IntProperty.ShouldEqual(15);

            evaluator.ScriptEvaluate("obj.IntProperty &= 19;");

            obj.IntProperty.ShouldEqual(3);
        }

        [Test]
        [Category("Script")]
        [Category("Instance Property set assignation")]
        [Category("|=")]
        public static void InstancePropertySetOrEqual()
        {
            ClassForTest1 obj = new ClassForTest1()
            {
                IntProperty = 5
            };

            ExpressionEvaluator evaluator = new ExpressionEvaluator()
            {
                Variables = new Dictionary<string, object>()
                {
                    {"obj", obj }
                }
            };

            obj.IntProperty.ShouldEqual(5);

            evaluator.ScriptEvaluate("obj.IntProperty |= 9;");

            obj.IntProperty.ShouldEqual(13);
        }

        [Test]
        [Category("Script")]
        [Category("Instance Property set assignation")]
        [Category("^=")]
        public static void InstancePropertySetXorEqual()
        {
            ClassForTest1 obj = new ClassForTest1()
            {
                IntProperty = 5
            };

            ExpressionEvaluator evaluator = new ExpressionEvaluator()
            {
                Variables = new Dictionary<string, object>()
                {
                    {"obj", obj }
                }
            };

            obj.IntProperty.ShouldEqual(5);

            evaluator.ScriptEvaluate("obj.IntProperty ^= 9;");

            obj.IntProperty.ShouldEqual(12);
        }

        [Test]
        [Category("Script")]
        [Category("Instance Property set assignation")]
        [Category("<<=")]
        public static void InstancePropertySetLeftShiftEqual()
        {
            ClassForTest1 obj = new ClassForTest1()
            {
                IntProperty = 5
            };

            ExpressionEvaluator evaluator = new ExpressionEvaluator()
            {
                Variables = new Dictionary<string, object>()
                {
                    {"obj", obj }
                }
            };

            obj.IntProperty.ShouldEqual(5);

            evaluator.ScriptEvaluate("obj.IntProperty <<= 2;");

            obj.IntProperty.ShouldEqual(20);
        }

        [Test]
        [Category("Script")]
        [Category("Instance Property set assignation")]
        [Category(">>=")]
        public static void InstancePropertySetRightShiftEqual()
        {
            ClassForTest1 obj = new ClassForTest1()
            {
                IntProperty = 20
            };

            ExpressionEvaluator evaluator = new ExpressionEvaluator()
            {
                Variables = new Dictionary<string, object>()
                {
                    {"obj", obj }
                }
            };

            obj.IntProperty.ShouldEqual(20);

            evaluator.ScriptEvaluate("obj.IntProperty >>= 2;");

            obj.IntProperty.ShouldEqual(5);
        }

        [Test]
        [Category("Script")]
        [Category("Static Property set assignation")]
        [Category("=")]
        public static void StaticPropertySetEqual()
        {
            ExpressionEvaluator evaluator = new ExpressionEvaluator();

            evaluator.Namespaces.Add("CodingSeb.ExpressionEvaluator.Tests");

            ClassForTest1.StaticIntProperty.ShouldEqual(67);

            evaluator.ScriptEvaluate("ClassForTest1.StaticIntProperty = 18;");

            ClassForTest1.StaticIntProperty.ShouldEqual(18);
        }

        [Test]
        [Category("Script")]
        [Category("Static Property set assignation")]
        [Category("+=")]
        public static void StaticPropertySetPlusEqual()
        {
            ExpressionEvaluator evaluator = new ExpressionEvaluator();

            evaluator.Namespaces.Add("CodingSeb.ExpressionEvaluator.Tests");

            ClassForTest1.StaticIntProperty.ShouldEqual(67);

            evaluator.ScriptEvaluate("ClassForTest1.StaticIntProperty += 3;");

            ClassForTest1.StaticIntProperty.ShouldEqual(70);
        }

        [Test]
        [Category("Script")]
        [Category("Static Property set assignation")]
        [Category("-=")]
        public static void StaticPropertySetMinusEqual()
        {
            ExpressionEvaluator evaluator = new ExpressionEvaluator();

            evaluator.Namespaces.Add("CodingSeb.ExpressionEvaluator.Tests");

            ClassForTest1.StaticIntProperty.ShouldEqual(67);

            evaluator.ScriptEvaluate("ClassForTest1.StaticIntProperty -= 7;");

            ClassForTest1.StaticIntProperty.ShouldEqual(60);
        }

        [Test]
        [Category("Script")]
        [Category("Static Property set assignation")]
        [Category("*=")]
        public static void StaticPropertySetMultiplyEqual()
        {
            ExpressionEvaluator evaluator = new ExpressionEvaluator();

            evaluator.Namespaces.Add("CodingSeb.ExpressionEvaluator.Tests");

            ClassForTest1.StaticIntProperty.ShouldEqual(67);

            evaluator.ScriptEvaluate("ClassForTest1.StaticIntProperty *= 2;");

            ClassForTest1.StaticIntProperty.ShouldEqual(134);
        }
        [Test]
        [Category("Script")]
        [Category("Static Property set assignation")]
        [Category("/=")]
        public static void StaticPropertySetDivideEqual()
        {
            ExpressionEvaluator evaluator = new ExpressionEvaluator();

            evaluator.Namespaces.Add("CodingSeb.ExpressionEvaluator.Tests");

            ClassForTest1.StaticIntProperty.ShouldEqual(67);

            evaluator.ScriptEvaluate("ClassForTest1.StaticIntProperty /= 2;");

            ClassForTest1.StaticIntProperty.ShouldEqual(33);
        }

        [Test]
        [Category("Script")]
        [Category("Static Property set assignation")]
        [Category("%=")]
        public static void StaticPropertySetModuloEqual()
        {
            ExpressionEvaluator evaluator = new ExpressionEvaluator();

            evaluator.Namespaces.Add("CodingSeb.ExpressionEvaluator.Tests");

            ClassForTest1.StaticIntProperty.ShouldEqual(67);

            evaluator.ScriptEvaluate("ClassForTest1.StaticIntProperty %= 2;");

            ClassForTest1.StaticIntProperty.ShouldEqual(1);
        }

        [Test]
        [Category("Script")]
        [Category("Static Property set assignation")]
        [Category("&=")]
        public static void StaticPropertySetAndEqual()
        {
            ExpressionEvaluator evaluator = new ExpressionEvaluator();

            evaluator.Namespaces.Add("CodingSeb.ExpressionEvaluator.Tests");

            ClassForTest1.StaticIntProperty.ShouldEqual(67);

            evaluator.ScriptEvaluate("ClassForTest1.StaticIntProperty &= 70;");

            ClassForTest1.StaticIntProperty.ShouldEqual(66);
        }

        [Test]
        [Category("Script")]
        [Category("Static Property set assignation")]
        [Category("|=")]
        public static void StaticPropertySetOrEqual()
        {
            ExpressionEvaluator evaluator = new ExpressionEvaluator();

            evaluator.Namespaces.Add("CodingSeb.ExpressionEvaluator.Tests");

            ClassForTest1.StaticIntProperty.ShouldEqual(67);

            evaluator.ScriptEvaluate("ClassForTest1.StaticIntProperty |= 70;");

            ClassForTest1.StaticIntProperty.ShouldEqual(71);
        }

        [Test]
        [Category("Script")]
        [Category("Static Property set assignation")]
        [Category("<<=")]
        public static void StaticPropertySetLeftShiftEqual()
        {
            ExpressionEvaluator evaluator = new ExpressionEvaluator();

            evaluator.Namespaces.Add("CodingSeb.ExpressionEvaluator.Tests");

            ClassForTest1.StaticIntProperty.ShouldEqual(67);

            evaluator.ScriptEvaluate("ClassForTest1.StaticIntProperty <<= 2;");

            ClassForTest1.StaticIntProperty.ShouldEqual(268);
        }

        [Test]
        [Category("Script")]
        [Category("Static Property set assignation")]
        [Category(">>=")]
        public static void StaticPropertySetRightShiftEqual()
        {
            ExpressionEvaluator evaluator = new ExpressionEvaluator();

            evaluator.Namespaces.Add("CodingSeb.ExpressionEvaluator.Tests");

            ClassForTest1.StaticIntProperty.ShouldEqual(67);

            evaluator.ScriptEvaluate("ClassForTest1.StaticIntProperty >>= 2;");

            ClassForTest1.StaticIntProperty.ShouldEqual(16);
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
