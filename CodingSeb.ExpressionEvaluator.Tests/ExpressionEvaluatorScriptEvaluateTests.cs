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
                #region Assignations

                #region Instance Property Assignation

                ClassForTest1 obj0001 = new ClassForTest1();

                yield return new TestCaseData("obj.IntProperty = 3;",
                        new ExpressionEvaluator()
                        {
                            Variables = new Dictionary<string, object>()
                            {
                                { "obj", obj0001 }
                            }
                        },
                        new Action(() => obj0001.IntProperty.ShouldEqual(25)),
                        new Action(() => obj0001.IntProperty.ShouldEqual(3)))
                    .SetCategory("Script")
                    .SetCategory("Instance Property set assignation")
                    .SetCategory("=")
                    .Returns(3);

                ClassForTest1 obj0002 = new ClassForTest1();

                yield return new TestCaseData("obj.IntProperty += 2;",
                        new ExpressionEvaluator()
                        {
                            Variables = new Dictionary<string, object>()
                            {
                                { "obj", obj0002 }
                            }
                        },
                        new Action(() => obj0002.IntProperty.ShouldEqual(25)),
                        new Action(() => obj0002.IntProperty.ShouldEqual(27)))
                    .SetCategory("Script")
                    .SetCategory("Instance Property set assignation")
                    .SetCategory("+=")
                    .Returns(27);

                ClassForTest1 obj0003 = new ClassForTest1();

                yield return new TestCaseData("obj.IntProperty -= 2;",
                        new ExpressionEvaluator()
                        {
                            Variables = new Dictionary<string, object>()
                            {
                                { "obj", obj0003 }
                            }
                        },
                        new Action(() => obj0003.IntProperty.ShouldEqual(25)),
                        new Action(() => obj0003.IntProperty.ShouldEqual(23)))
                    .SetCategory("Script")
                    .SetCategory("Instance Property set assignation")
                    .SetCategory("-=")
                    .Returns(23);

                ClassForTest1 obj0004 = new ClassForTest1();

                yield return new TestCaseData("obj.IntProperty *= 2;",
                        new ExpressionEvaluator()
                        {
                            Variables = new Dictionary<string, object>()
                            {
                                { "obj", obj0004 }
                            }
                        },
                        new Action(() => obj0004.IntProperty.ShouldEqual(25)),
                        new Action(() => obj0004.IntProperty.ShouldEqual(50)))
                    .SetCategory("Script")
                    .SetCategory("Instance Property set assignation")
                    .SetCategory("*=")
                    .Returns(50);

                ClassForTest1 obj0005 = new ClassForTest1();

                yield return new TestCaseData("obj.IntProperty /= 5;",
                        new ExpressionEvaluator()
                        {
                            Variables = new Dictionary<string, object>()
                            {
                                { "obj", obj0005 }
                            }
                        },
                        new Action(() => obj0005.IntProperty.ShouldEqual(25)),
                        new Action(() => obj0005.IntProperty.ShouldEqual(5)))
                    .SetCategory("Script")
                    .SetCategory("Instance Property set assignation")
                    .SetCategory("/=")
                    .Returns(5);

                ClassForTest1 obj0006 = new ClassForTest1();

                yield return new TestCaseData("obj.IntProperty %= 5;",
                        new ExpressionEvaluator()
                        {
                            Variables = new Dictionary<string, object>()
                            {
                                { "obj", obj0006 }
                            }
                        },
                        new Action(() => obj0006.IntProperty.ShouldEqual(25)),
                        new Action(() => obj0006.IntProperty.ShouldEqual(0)))
                    .SetCategory("Script")
                    .SetCategory("Instance Property set assignation")
                    .SetCategory("%=")
                    .Returns(0);

                ClassForTest1 obj0007 = new ClassForTest1()
                {
                    IntProperty = 10
                };

                yield return new TestCaseData("obj.IntProperty %= 3;",
                        new ExpressionEvaluator()
                        {
                            Variables = new Dictionary<string, object>()
                            {
                                { "obj", obj0007 }
                            }
                        },
                        new Action(() => obj0007.IntProperty.ShouldEqual(10)),
                        new Action(() => obj0007.IntProperty.ShouldEqual(1)))
                    .SetCategory("Script")
                    .SetCategory("Instance Property set assignation")
                    .SetCategory("%=")
                    .Returns(1);

                ClassForTest1 obj0008 = new ClassForTest1()
                {
                    IntProperty = 15
                };

                yield return new TestCaseData("obj.IntProperty &= 19;",
                        new ExpressionEvaluator()
                        {
                            Variables = new Dictionary<string, object>()
                            {
                                { "obj", obj0008 }
                            }
                        },
                        new Action(() => obj0008.IntProperty.ShouldEqual(15)),
                        new Action(() => obj0008.IntProperty.ShouldEqual(3)))
                    .SetCategory("Script")
                    .SetCategory("Instance Property set assignation")
                    .SetCategory("&=")
                    .Returns(3);

                ClassForTest1 obj0009 = new ClassForTest1()
                {
                    IntProperty = 5
                };

                yield return new TestCaseData("obj.IntProperty |= 9;",
                        new ExpressionEvaluator()
                        {
                            Variables = new Dictionary<string, object>()
                            {
                                { "obj", obj0009 }
                            }
                        },
                        new Action(() => obj0009.IntProperty.ShouldEqual(5)),
                        new Action(() => obj0009.IntProperty.ShouldEqual(13)))
                    .SetCategory("Script")
                    .SetCategory("Instance Property set assignation")
                    .SetCategory("|=")
                    .Returns(13);

                #endregion

                #endregion

                #region while

                yield return new TestCaseData(Resources.Script0001, null, null, null)
                    .SetCategory("Script")
                    .SetCategory("while")
                    .SetCategory("variable assignation")
                    .SetCategory("++")
                    .SetCategory("+=")
                    .Returns("0,1,2,3,4");
                yield return new TestCaseData(removeAllWhiteSpacesRegex.Replace(Resources.Script0001, string.Empty), null, null, null)
                    .SetCategory("Script")
                    .SetCategory("while")
                    .SetCategory("variable assignation")
                    .SetCategory("++")
                    .SetCategory("+=")
                    .Returns("0,1,2,3,4");

                #endregion

                #region for

                yield return new TestCaseData(Resources.Script0002, null, null, null)
                    .SetCategory("Script")
                    .SetCategory("for")
                    .SetCategory("variable assignation")
                    .SetCategory("++")
                    .SetCategory("+=")
                    .Returns("0,1,2,3,4");
                yield return new TestCaseData(removeAllWhiteSpacesRegex.Replace(Resources.Script0002, string.Empty), null, null, null)
                    .SetCategory("Script")
                    .SetCategory("for")
                    .SetCategory("variable assignation")
                    .SetCategory("++")
                    .SetCategory("+=")
                    .Returns("0,1,2,3,4");
                yield return new TestCaseData(Resources.Script0003, null, null, null)
                    .SetCategory("Script")
                    .SetCategory("for")
                    .SetCategory("variable assignation")
                    .SetCategory("++")
                    .SetCategory("+=")
                    .Returns("0,1,2,3,4");
                yield return new TestCaseData(removeAllWhiteSpacesRegex.Replace(Resources.Script0003, string.Empty), null, null, null)
                    .SetCategory("Script")
                    .SetCategory("for")
                    .SetCategory("variable assignation")
                    .SetCategory("++")
                    .SetCategory("+=")
                    .Returns("0,1,2,3,4");

                #endregion

                #region if, else if, else

                yield return new TestCaseData(Resources.Script0004.Replace("[valx]", "0").Replace("[valy]", "1"), null, null, null)
                    .SetCategory("Script")
                    .SetCategory("if")
                    .SetCategory("variable assignation")
                    .Returns(1);
                yield return new TestCaseData(removeAllWhiteSpacesRegex.Replace(Resources.Script0004.Replace("[valx]", "0").Replace("[valy]", "1"), string.Empty).Replace("else", "else "), null, null, null)
                    .SetCategory("Script")
                    .SetCategory("if")
                    .SetCategory("variable assignation")
                    .Returns(1);
                yield return new TestCaseData(Resources.Script0004.Replace("[valx]", "-1").Replace("[valy]", "1"), null, null, null)
                    .SetCategory("Script")
                    .SetCategory("if")
                    .SetCategory("variable assignation")
                    .Returns(1);
                yield return new TestCaseData(removeAllWhiteSpacesRegex.Replace(Resources.Script0004.Replace("[valx]", "-1").Replace("[valy]", "1"), string.Empty).Replace("else", "else "), null, null, null)
                    .SetCategory("Script")
                    .SetCategory("if")
                    .SetCategory("variable assignation")
                    .Returns(1);
                yield return new TestCaseData(Resources.Script0004.Replace("[valx]", "1").Replace("[valy]", "1"), null, null, null)
                    .SetCategory("Script")
                    .SetCategory("if")
                    .SetCategory("variable assignation")
                    .Returns(1);
                yield return new TestCaseData(removeAllWhiteSpacesRegex.Replace(Resources.Script0004.Replace("[valx]", "1").Replace("[valy]", "1"), string.Empty).Replace("else", "else "), null, null, null)
                    .SetCategory("Script")
                    .SetCategory("if")
                    .SetCategory("variable assignation")
                    .Returns(1);

                yield return new TestCaseData(Resources.Script0004.Replace("[valx]", "0").Replace("[valy]", "0"), null, null, null)
                    .SetCategory("Script")
                    .SetCategory("if")
                    .SetCategory("variable assignation")
                    .Returns(2);
                yield return new TestCaseData(removeAllWhiteSpacesRegex.Replace(Resources.Script0004.Replace("[valx]", "0").Replace("[valy]", "0"), string.Empty).Replace("else", "else "), null, null, null)
                    .SetCategory("Script")
                    .SetCategory("if")
                    .SetCategory("variable assignation")
                    .Returns(2);

                yield return new TestCaseData(Resources.Script0004.Replace("[valx]", "-1").Replace("[valy]", "0"), null, null, null)
                    .SetCategory("Script")
                    .SetCategory("if")
                    .SetCategory("variable assignation")
                    .Returns(3);
                yield return new TestCaseData(removeAllWhiteSpacesRegex.Replace(Resources.Script0004.Replace("[valx]", "-1").Replace("[valy]", "0"), string.Empty).Replace("else", "else "), null, null, null)
                    .SetCategory("Script")
                    .SetCategory("if")
                    .SetCategory("variable assignation")
                    .Returns(3);

                yield return new TestCaseData(Resources.Script0004.Replace("[valx]", "1").Replace("[valy]", "0"), null, null, null)
                    .SetCategory("Script")
                    .SetCategory("if")
                    .SetCategory("variable assignation")
                    .Returns(4);
                yield return new TestCaseData(removeAllWhiteSpacesRegex.Replace(Resources.Script0004.Replace("[valx]", "1").Replace("[valy]", "0"), string.Empty).Replace("else", "else "), null, null, null)
                    .SetCategory("Script")
                    .SetCategory("if")
                    .SetCategory("variable assignation")
                    .Returns(4);

                yield return new TestCaseData(Resources.Script0005.Replace("[valx]", "0").Replace("[valy]", "1"), null, null, null)
                    .SetCategory("Script")
                    .SetCategory("if")
                    .SetCategory("variable assignation")
                    .Returns(1);
                yield return new TestCaseData(removeAllWhiteSpacesRegex.Replace(Resources.Script0005.Replace("[valx]", "0").Replace("[valy]", "1"), string.Empty).Replace("else", "else "), null, null, null)
                    .SetCategory("Script")
                    .SetCategory("if")
                    .SetCategory("variable assignation")
                    .Returns(1);

                yield return new TestCaseData(Resources.Script0005.Replace("[valx]", "-1").Replace("[valy]", "1"), null, null, null)
                    .SetCategory("Script")
                    .SetCategory("if")
                    .SetCategory("variable assignation")
                    .Returns(1);
                yield return new TestCaseData(removeAllWhiteSpacesRegex.Replace(Resources.Script0005.Replace("[valx]", "-1").Replace("[valy]", "1"), string.Empty).Replace("else", "else "), null, null, null)
                    .SetCategory("Script")
                    .SetCategory("if")
                    .SetCategory("variable assignation")
                    .Returns(1);

                yield return new TestCaseData(Resources.Script0005.Replace("[valx]", "1").Replace("[valy]", "1"), null, null, null)
                    .SetCategory("Script")
                    .SetCategory("if")
                    .SetCategory("variable assignation")
                    .Returns(1);
                yield return new TestCaseData(removeAllWhiteSpacesRegex.Replace(Resources.Script0005.Replace("[valx]", "1").Replace("[valy]", "1"), string.Empty).Replace("else", "else "), null, null, null)
                    .SetCategory("Script")
                    .SetCategory("if")
                    .SetCategory("variable assignation")
                    .Returns(1);

                yield return new TestCaseData(Resources.Script0005.Replace("[valx]", "0").Replace("[valy]", "0"), null, null, null)
                    .SetCategory("Script")
                    .SetCategory("if")
                    .SetCategory("variable assignation")
                    .Returns(2);
                yield return new TestCaseData(removeAllWhiteSpacesRegex.Replace(Resources.Script0005.Replace("[valx]", "0").Replace("[valy]", "0"), string.Empty).Replace("else", "else "), null, null, null)
                    .SetCategory("Script")
                    .SetCategory("if")
                    .SetCategory("variable assignation")
                    .Returns(2);

                yield return new TestCaseData(Resources.Script0005.Replace("[valx]", "-1").Replace("[valy]", "0"), null, null, null)
                    .SetCategory("Script")
                    .SetCategory("if")
                    .SetCategory("variable assignation")
                    .Returns(3);
                yield return new TestCaseData(removeAllWhiteSpacesRegex.Replace(Resources.Script0005.Replace("[valx]", "-1").Replace("[valy]", "0"), string.Empty).Replace("else", "else "), null, null, null)
                    .SetCategory("Script")
                    .SetCategory("if")
                    .SetCategory("variable assignation")
                    .Returns(3);

                yield return new TestCaseData(Resources.Script0005.Replace("[valx]", "1").Replace("[valy]", "0"), null, null, null)
                    .SetCategory("Script")
                    .SetCategory("if")
                    .SetCategory("variable assignation")
                    .Returns(4);
                yield return new TestCaseData(removeAllWhiteSpacesRegex.Replace(Resources.Script0005.Replace("[valx]", "1").Replace("[valy]", "0"), string.Empty).Replace("else", "else "), null, null, null)
                    .SetCategory("Script")
                    .SetCategory("if")
                    .SetCategory("variable assignation")
                    .Returns(4);

                #endregion

                #region block for lambda body

                yield return new TestCaseData(Resources.Script0006, null, null, null)
                    .SetCategory("Script")
                    .SetCategory("lambda")
                    .SetCategory("variable assignation")
                    .SetCategory("block for lambda body")
                    .Returns(3);
                yield return new TestCaseData(removeAllWhiteSpacesRegex.Replace(Resources.Script0006, string.Empty), null, null, null)
                    .SetCategory("Script")
                    .SetCategory("lambda")
                    .SetCategory("variable assignation")
                    .SetCategory("block for lambda body")
                    .Returns(3);

                #endregion

                #region return keyword and OptionAutoReturnLastExpressionResultInScriptsActive tests 

                yield return new TestCaseData(Resources.Script0008.Replace("[valx]", "1"), null, null, null)
                    .SetCategory("Script")
                    .SetCategory("return")
                    .SetCategory("if")
                    .SetCategory("variable assignation")
                    .Returns(1);
                yield return new TestCaseData(removeAllWhiteSpacesRegex.Replace(Resources.Script0008.Replace("[valx]", "1"), string.Empty).Replace("return", "return "), null, null, null)
                    .SetCategory("Script")
                    .SetCategory("return")
                    .SetCategory("if")
                    .SetCategory("variable assignation")
                    .Returns(1);
                yield return new TestCaseData(Resources.Script0008.Replace("[valx]", "2"), null, null, null)
                    .SetCategory("Script")
                    .SetCategory("return")
                    .SetCategory("if")
                    .SetCategory("variable assignation")
                    .Returns(2);
                yield return new TestCaseData(removeAllWhiteSpacesRegex.Replace(Resources.Script0008.Replace("[valx]", "2"), string.Empty).Replace("return", "return "), null, null, null)
                    .SetCategory("Script")
                    .SetCategory("return")
                    .SetCategory("if")
                    .SetCategory("variable assignation")
                    .Returns(2);
                yield return new TestCaseData(Resources.Script0008.Replace("[valx]", "0"), null, null, null)
                    .SetCategory("Script")
                    .SetCategory("return")
                    .SetCategory("if")
                    .SetCategory("variable assignation")
                    .Returns(2);
                yield return new TestCaseData(removeAllWhiteSpacesRegex.Replace(Resources.Script0008.Replace("[valx]", "0"), string.Empty).Replace("return", "return "), null, null, null)
                    .SetCategory("Script")
                    .SetCategory("return")
                    .SetCategory("if")
                    .SetCategory("variable assignation")
                    .Returns(2);

                ExpressionEvaluator evaluator = new ExpressionEvaluator()
                {
                    OptionOnNoReturnKeywordFoundInScriptAction = OptionOnNoReturnKeywordFoundInScriptAction.ReturnNull
                };

                yield return new TestCaseData(Resources.Script0008.Replace("[valx]", "1"), evaluator, null, null)
                    .SetCategory("Script")
                    .SetCategory("return")
                    .SetCategory("if")
                    .SetCategory("variable assignation")
                    .SetCategory("Options")
                    .SetCategory("OptionOnNoReturnKeywordFoundInScriptAction = ReturnNull")
                    .Returns(null);
                yield return new TestCaseData(removeAllWhiteSpacesRegex.Replace(Resources.Script0008.Replace("[valx]", "1"), string.Empty).Replace("return", "return "), evaluator, null, null)
                    .SetCategory("Script")
                    .SetCategory("return")
                    .SetCategory("if")
                    .SetCategory("variable assignation")
                    .SetCategory("Options")
                    .SetCategory("OptionOnNoReturnKeywordFoundInScriptAction = ReturnNull")
                    .Returns(null);
                yield return new TestCaseData(Resources.Script0008.Replace("[valx]", "2"), evaluator, null, null)
                    .SetCategory("Script")
                    .SetCategory("return")
                    .SetCategory("if")
                    .SetCategory("variable assignation")
                    .SetCategory("Options")
                    .SetCategory("OptionOnNoReturnKeywordFoundInScriptAction = ReturnNull")
                    .Returns(null);
                yield return new TestCaseData(removeAllWhiteSpacesRegex.Replace(Resources.Script0008.Replace("[valx]", "2"), string.Empty).Replace("return", "return "), evaluator, null, null)
                    .SetCategory("Script")
                    .SetCategory("return")
                    .SetCategory("if")
                    .SetCategory("variable assignation")
                    .SetCategory("Options")
                    .SetCategory("OptionOnNoReturnKeywordFoundInScriptAction = ReturnNull")
                    .Returns(null);
                yield return new TestCaseData(Resources.Script0008.Replace("[valx]", "0"), evaluator, null, null)
                    .SetCategory("Script")
                    .SetCategory("return")
                    .SetCategory("if")
                    .SetCategory("variable assignation")
                    .SetCategory("Options")
                    .SetCategory("OptionOnNoReturnKeywordFoundInScriptAction = ReturnNull")
                    .Returns(2);
                yield return new TestCaseData(removeAllWhiteSpacesRegex.Replace(Resources.Script0008.Replace("[valx]", "0"), string.Empty).Replace("return", "return "), evaluator, null, null)
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

                yield return new TestCaseData(Resources.Script0008.Replace("[valx]", "0"), evaluator, null, null)
                    .SetCategory("Script")
                    .SetCategory("return")
                    .SetCategory("if")
                    .SetCategory("variable assignation")
                    .SetCategory("Options")
                    .SetCategory("OptionOnNoReturnKeywordFoundInScriptAction = ReturnNull")
                    .Returns(2);
                yield return new TestCaseData(removeAllWhiteSpacesRegex.Replace(Resources.Script0008.Replace("[valx]", "0"), string.Empty).Replace("return", "return "), evaluator, null, null)
                    .SetCategory("Script")
                    .SetCategory("return")
                    .SetCategory("if")
                    .SetCategory("variable assignation")
                    .SetCategory("Options")
                    .SetCategory("OptionOnNoReturnKeywordFoundInScriptAction = ReturnNull")
                    .Returns(2);

                #endregion

                #region More complex script

                yield return new TestCaseData(Resources.Script0007, null, null, null)
                    .SetCategory("Script")
                    .SetCategory("lambda")
                    .SetCategory("variable assignation")
                    .SetCategory("if")
                    .SetCategory("block for lambda body")
                    .Returns(13);
                yield return new TestCaseData(removeAllWhiteSpacesRegex.Replace(Resources.Script0007, string.Empty), null, null, null)
                    .SetCategory("Script")
                    .SetCategory("lambda")
                    .SetCategory("variable assignation")
                    .SetCategory("if")
                    .SetCategory("block for lambda body")
                    .Returns(13);
                yield return new TestCaseData(Resources.Script0009, null, null, null)
                    .SetCategory("Script")
                    .SetCategory("variable assignation")
                    .SetCategory("if")
                    .SetCategory("continue in for")
                    .SetCategory("break in for")
                    .Returns("0,1,2,4,5,6,");
                yield return new TestCaseData(removeAllWhiteSpacesRegex.Replace(Resources.Script0009, string.Empty), null, null, null)
                    .SetCategory("Script")
                    .SetCategory("variable assignation")
                    .SetCategory("if")
                    .SetCategory("continue in for")
                    .SetCategory("break in for")
                    .Returns("0,1,2,4,5,6,");
                yield return new TestCaseData(Resources.Script0010, null, null, null)
                    .SetCategory("Script")
                    .SetCategory("variable assignation")
                    .SetCategory("if")
                    .SetCategory("continue in while")
                    .SetCategory("break in while")
                    .Returns("0,1,2,4,5,6,");
                yield return new TestCaseData(removeAllWhiteSpacesRegex.Replace(Resources.Script0010, string.Empty), null, null, null)
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
        public object TestCasesForScriptEvaluate(string script, ExpressionEvaluator evaluator, Action PreExecuteAssertAction, Action PostExecuteAssertAction)
        {
            evaluator = evaluator ?? new ExpressionEvaluator();

            evaluator.Namespaces.Add("CodingSeb.ExpressionEvaluator.Tests");

            PreExecuteAssertAction?.Invoke();

            object result = evaluator.ScriptEvaluate(evaluator.RemoveComments(script));

            PostExecuteAssertAction?.Invoke();

            return result;
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

        #region Assignations

        #region Instance Property Assignation

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

        #endregion

        #region Static Property Assignation

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
