using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Should;
using Newtonsoft.Json;

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

                ClassForTest1 obj0010 = new ClassForTest1()
                {
                    IntProperty = 5
                };

                yield return new TestCaseData("obj.IntProperty ^= 9;",
                        new ExpressionEvaluator()
                        {
                            Variables = new Dictionary<string, object>()
                            {
                                { "obj", obj0010 }
                            }
                        },
                        new Action(() => obj0010.IntProperty.ShouldEqual(5)),
                        new Action(() => obj0010.IntProperty.ShouldEqual(12)))
                    .SetCategory("Script")
                    .SetCategory("Instance Property set assignation")
                    .SetCategory("^=")
                    .Returns(12);

                ClassForTest1 obj0011 = new ClassForTest1()
                {
                    IntProperty = 5
                };

                yield return new TestCaseData("obj.IntProperty <<= 2;",
                        new ExpressionEvaluator()
                        {
                            Variables = new Dictionary<string, object>()
                            {
                                { "obj", obj0011 }
                            }
                        },
                        new Action(() => obj0011.IntProperty.ShouldEqual(5)),
                        new Action(() => obj0011.IntProperty.ShouldEqual(20)))
                    .SetCategory("Script")
                    .SetCategory("Instance Property set assignation")
                    .SetCategory("<<=")
                    .Returns(20);

                ClassForTest1 obj0012 = new ClassForTest1()
                {
                    IntProperty = 20
                };

                yield return new TestCaseData("obj.IntProperty >>= 2;",
                        new ExpressionEvaluator()
                        {
                            Variables = new Dictionary<string, object>()
                            {
                                { "obj", obj0012 }
                            }
                        },
                        new Action(() => obj0012.IntProperty.ShouldEqual(20)),
                        new Action(() => obj0012.IntProperty.ShouldEqual(5)))
                    .SetCategory("Script")
                    .SetCategory("Instance Property set assignation")
                    .SetCategory(">>=")
                    .Returns(5);

                #endregion

                #region Static Property Assignation

                yield return new TestCaseData("ClassForTest1.StaticIntProperty = 18;",
                        null,
                        new Action(() => ClassForTest1.StaticIntProperty.ShouldEqual(67)),
                        new Action(() => ClassForTest1.StaticIntProperty.ShouldEqual(18)))
                    .SetCategory("Script")
                    .SetCategory("Static Property set assignation")
                    .SetCategory("=")
                    .Returns(18);

                yield return new TestCaseData("ClassForTest1.StaticIntProperty += 3;",
                        null,
                        new Action(() => ClassForTest1.StaticIntProperty.ShouldEqual(67)),
                        new Action(() => ClassForTest1.StaticIntProperty.ShouldEqual(70)))
                    .SetCategory("Script")
                    .SetCategory("Static Property set assignation")
                    .SetCategory("+=")
                    .Returns(70);

                yield return new TestCaseData("ClassForTest1.StaticIntProperty -= 7;",
                        null,
                        new Action(() => ClassForTest1.StaticIntProperty.ShouldEqual(67)),
                        new Action(() => ClassForTest1.StaticIntProperty.ShouldEqual(60)))
                    .SetCategory("Script")
                    .SetCategory("Static Property set assignation")
                    .SetCategory("-=")
                    .Returns(60);

                yield return new TestCaseData("ClassForTest1.StaticIntProperty *= 2;",
                        null,
                        new Action(() => ClassForTest1.StaticIntProperty.ShouldEqual(67)),
                        new Action(() => ClassForTest1.StaticIntProperty.ShouldEqual(134)))
                    .SetCategory("Script")
                    .SetCategory("Static Property set assignation")
                    .SetCategory("*=")
                    .Returns(134);

                yield return new TestCaseData("ClassForTest1.StaticIntProperty /= 2;",
                        null,
                        new Action(() => ClassForTest1.StaticIntProperty.ShouldEqual(67)),
                        new Action(() => ClassForTest1.StaticIntProperty.ShouldEqual(33)))
                    .SetCategory("Script")
                    .SetCategory("Static Property set assignation")
                    .SetCategory("/=")
                    .Returns(33);

                yield return new TestCaseData("ClassForTest1.StaticIntProperty %= 2;",
                        null,
                        new Action(() => ClassForTest1.StaticIntProperty.ShouldEqual(67)),
                        new Action(() => ClassForTest1.StaticIntProperty.ShouldEqual(1)))
                    .SetCategory("Script")
                    .SetCategory("Static Property set assignation")
                    .SetCategory("%=")
                    .Returns(1);

                yield return new TestCaseData("ClassForTest1.StaticIntProperty &= 70;",
                        null,
                        new Action(() => ClassForTest1.StaticIntProperty.ShouldEqual(67)),
                        new Action(() => ClassForTest1.StaticIntProperty.ShouldEqual(66)))
                    .SetCategory("Script")
                    .SetCategory("Static Property set assignation")
                    .SetCategory("&=")
                    .Returns(66);

                yield return new TestCaseData("ClassForTest1.StaticIntProperty |= 70;",
                        null,
                        new Action(() => ClassForTest1.StaticIntProperty.ShouldEqual(67)),
                        new Action(() => ClassForTest1.StaticIntProperty.ShouldEqual(71)))
                    .SetCategory("Script")
                    .SetCategory("Static Property set assignation")
                    .SetCategory("|=")
                    .Returns(71);

               yield return new TestCaseData("ClassForTest1.StaticIntProperty <<= 2;",
                        null,
                        new Action(() => ClassForTest1.StaticIntProperty.ShouldEqual(67)),
                        new Action(() => ClassForTest1.StaticIntProperty.ShouldEqual(268)))
                    .SetCategory("Script")
                    .SetCategory("Static Property set assignation")
                    .SetCategory("<<=")
                    .Returns(268);

               yield return new TestCaseData("ClassForTest1.StaticIntProperty >>= 2;",
                        null,
                        new Action(() => ClassForTest1.StaticIntProperty.ShouldEqual(67)),
                        new Action(() => ClassForTest1.StaticIntProperty.ShouldEqual(16)))
                    .SetCategory("Script")
                    .SetCategory("Static Property set assignation")
                    .SetCategory(">>=")
                    .Returns(16);

                #endregion

                #region Variable Assignation

                ExpressionEvaluator evaluator0001 = new ExpressionEvaluator()
                {
                    Variables = new Dictionary<string, object>()
                    {
                        { "x", 5 }
                    }
                };

                yield return new TestCaseData("x = 3;",
                    evaluator0001,
                    new Action(() => evaluator0001.Variables["x"].ShouldEqual(5)),
                    new Action(() => evaluator0001.Variables["x"].ShouldEqual(3)))
                .SetCategory("Script")
                .SetCategory("Variable assignation")
                .SetCategory("=")
                .Returns(3);

                yield return new TestCaseData("y = 20;",
                    evaluator0001,
                    new Action(() => evaluator0001.Variables.ContainsKey("y").ShouldBeFalse()),
                    new Action(() => evaluator0001.Variables["y"].ShouldEqual(20)))
                .SetCategory("Script")
                .SetCategory("Variable assignation")
                .SetCategory("=")
                .Returns(20);

                ExpressionEvaluator evaluator0002 = new ExpressionEvaluator()
                {
                    Variables = new Dictionary<string, object>()
                    {
                        { "x", 5 },
                        { "text", "Test" }
                    }
                };

                yield return new TestCaseData("x += 4;",
                   evaluator0002,
                    new Action(() => evaluator0002.Variables["x"].ShouldEqual(5)),
                    new Action(() => evaluator0002.Variables["x"].ShouldEqual(9)))
                .SetCategory("Script")
                .SetCategory("Variable assignation")
                .SetCategory("+=")
                .Returns(9);

                yield return new TestCaseData("text += \" Try\";",
                    evaluator0002,
                    new Action(() => evaluator0002.Variables["text"].ShouldEqual("Test")),
                    new Action(() => evaluator0002.Variables["text"].ShouldEqual("Test Try")))
                .SetCategory("Script")
                .SetCategory("Variable assignation")
                .SetCategory("+=")
                .Returns("Test Try");

                ExpressionEvaluator evaluator0003 = new ExpressionEvaluator()
                {
                    Variables = new Dictionary<string, object>()
                    {
                        { "x", 5 },
                    }
                };

                yield return new TestCaseData("x -= 4;",
                   evaluator0003,
                    new Action(() => evaluator0003.Variables["x"].ShouldEqual(5)),
                    new Action(() => evaluator0003.Variables["x"].ShouldEqual(1)))
                .SetCategory("Script")
                .SetCategory("Variable assignation")
                .SetCategory("-=")
                .Returns(1);

                ExpressionEvaluator evaluator0004 = new ExpressionEvaluator()
                {
                    Variables = new Dictionary<string, object>()
                    {
                        { "x", 5 },
                    }
                };

                yield return new TestCaseData("x *= 2;",
                   evaluator0004,
                    new Action(() => evaluator0004.Variables["x"].ShouldEqual(5)),
                    new Action(() => evaluator0004.Variables["x"].ShouldEqual(10)))
                .SetCategory("Script")
                .SetCategory("Variable assignation")
                .SetCategory("*=")
                .Returns(10);

                ExpressionEvaluator evaluator0005 = new ExpressionEvaluator()
                {
                    Variables = new Dictionary<string, object>()
                    {
                        { "x", 5 },
                    }
                };

                yield return new TestCaseData("x /= 2;",
                   evaluator0005,
                    new Action(() => evaluator0005.Variables["x"].ShouldEqual(5)),
                    new Action(() => evaluator0005.Variables["x"].ShouldEqual(2)))
                .SetCategory("Script")
                .SetCategory("Variable assignation")
                .SetCategory("/=")
                .Returns(2);

                ExpressionEvaluator evaluator0006 = new ExpressionEvaluator()
                {
                    Variables = new Dictionary<string, object>()
                    {
                        { "x", 5 },
                    }
                };

                yield return new TestCaseData("x %= 2;",
                   evaluator0006,
                    new Action(() => evaluator0006.Variables["x"].ShouldEqual(5)),
                    new Action(() => evaluator0006.Variables["x"].ShouldEqual(1)))
                .SetCategory("Script")
                .SetCategory("Variable assignation")
                .SetCategory("%=")
                .Returns(1);

                ExpressionEvaluator evaluator0007 = new ExpressionEvaluator()
                {
                    Variables = new Dictionary<string, object>()
                    {
                        { "x", 5 },
                    }
                };

                yield return new TestCaseData("x ^= 3;",
                   evaluator0007,
                    new Action(() => evaluator0007.Variables["x"].ShouldEqual(5)),
                    new Action(() => evaluator0007.Variables["x"].ShouldEqual(6)))
                .SetCategory("Script")
                .SetCategory("Variable assignation")
                .SetCategory("^=")
                .Returns(6);

                ExpressionEvaluator evaluator0008 = new ExpressionEvaluator()
                {
                    Variables = new Dictionary<string, object>()
                    {
                        { "x", 5 },
                    }
                };

                yield return new TestCaseData("x &= 3;",
                   evaluator0008,
                    new Action(() => evaluator0008.Variables["x"].ShouldEqual(5)),
                    new Action(() => evaluator0008.Variables["x"].ShouldEqual(1)))
                .SetCategory("Script")
                .SetCategory("Variable assignation")
                .SetCategory("&=")
                .Returns(1);

                ExpressionEvaluator evaluator0009 = new ExpressionEvaluator()
                {
                    Variables = new Dictionary<string, object>()
                    {
                        { "x", 5 },
                    }
                };

                yield return new TestCaseData("x |= 3;",
                   evaluator0009,
                    new Action(() => evaluator0009.Variables["x"].ShouldEqual(5)),
                    new Action(() => evaluator0009.Variables["x"].ShouldEqual(7)))
                .SetCategory("Script")
                .SetCategory("Variable assignation")
                .SetCategory("|=")
                .Returns(7);

                ExpressionEvaluator evaluator0010 = new ExpressionEvaluator()
                {
                    Variables = new Dictionary<string, object>()
                    {
                        { "x", 5 },
                    }
                };

                yield return new TestCaseData("x <<= 2;",
                   evaluator0010,
                    new Action(() => evaluator0010.Variables["x"].ShouldEqual(5)),
                    new Action(() => evaluator0010.Variables["x"].ShouldEqual(20)))
                .SetCategory("Script")
                .SetCategory("Variable assignation")
                .SetCategory("<<=")
                .Returns(20);

                ExpressionEvaluator evaluator0011 = new ExpressionEvaluator()
                {
                    Variables = new Dictionary<string, object>()
                    {
                        { "x", 5 },
                    }
                };

                yield return new TestCaseData("x >>= 2;",
                   evaluator0011,
                    new Action(() => evaluator0011.Variables["x"].ShouldEqual(5)),
                    new Action(() => evaluator0011.Variables["x"].ShouldEqual(1)))
                .SetCategory("Script")
                .SetCategory("Variable assignation")
                .SetCategory(">>=")
                .Returns(1);

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
                yield return new TestCaseData(Resources.Script0015, null, null, null)
                    .SetCategory("Script")
                    .SetCategory("do while")
                    .SetCategory("variable assignation")
                    .SetCategory("++")
                    .SetCategory("+=")
                    .Returns(string.Empty);
                yield return new TestCaseData(removeAllWhiteSpacesRegex.Replace(Resources.Script0015, string.Empty), null, null, null)
                    .SetCategory("Script")
                    .SetCategory("do while")
                    .SetCategory("variable assignation")
                    .SetCategory("++")
                    .SetCategory("+=")
                    .Returns(string.Empty);

                #endregion

                #region do while

                yield return new TestCaseData(Resources.Script0013, null, null, null)
                    .SetCategory("Script")
                    .SetCategory("do while")
                    .SetCategory("variable assignation")
                    .SetCategory("++")
                    .SetCategory("+=")
                    .Returns("0,1,2,3,4");
                yield return new TestCaseData(removeAllWhiteSpacesRegex.Replace(Resources.Script0013, string.Empty), null, null, null)
                    .SetCategory("Script")
                    .SetCategory("do while")
                    .SetCategory("variable assignation")
                    .SetCategory("++")
                    .SetCategory("+=")
                    .Returns("0,1,2,3,4");
                yield return new TestCaseData(Resources.Script0014, null, null, null)
                    .SetCategory("Script")
                    .SetCategory("do while")
                    .SetCategory("variable assignation")
                    .SetCategory("++")
                    .SetCategory("+=")
                    .Returns("0");
                yield return new TestCaseData(removeAllWhiteSpacesRegex.Replace(Resources.Script0014, string.Empty), null, null, null)
                    .SetCategory("Script")
                    .SetCategory("do while")
                    .SetCategory("variable assignation")
                    .SetCategory("++")
                    .SetCategory("+=")
                    .Returns("0");

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

                #region foreach

                yield return new TestCaseData(Resources.Script0018, null, null, null)
                    .SetCategory("Script")
                    .SetCategory("foreach")
                    .SetCategory("variable assignation")
                    .SetCategory("+=")
                    .Returns("This is a splitted text");

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

                #region Lambda assignation and call

                yield return new TestCaseData(Resources.Script0016, null, null, null)
                    .SetCategory("Script")
                    .SetCategory("lambda")
                    .SetCategory("lambda call")
                    .SetCategory("lambda assignation")
                    .SetCategory("return")
                    .SetCategory("variable assignation")
                    .Returns(7);

                yield return new TestCaseData(Resources.Script0017, null, null, null)
                    .SetCategory("Script")
                    .SetCategory("lambda")
                    .SetCategory("lambda call")
                    .SetCategory("lambda call imbrication")
                    .SetCategory("lambda assignation")
                    .SetCategory("return")
                    .SetCategory("variable assignation")
                    .Returns(6);

                #endregion

                #region ExpandoObject

                yield return new TestCaseData(Resources.Script0019, null, null, null)
                    .SetCategory("Script")
                    .SetCategory("ExpandoObject")
                    .SetCategory("return")
                    .Returns(58.3);
                yield return new TestCaseData(Resources.Script0020, null, null, null)
                    .SetCategory("Script")
                    .SetCategory("ExpandoObject")
                    .SetCategory("Indexing")
                    .SetCategory("return")
                    .Returns(58.3);
                yield return new TestCaseData(Resources.Script0021, null, null, null)
                    .SetCategory("Script")
                    .SetCategory("ExpandoObject")
                    .SetCategory("Indexing")
                    .SetCategory("return")
                    .Returns(58.3);
                yield return new TestCaseData(Resources.Script0022, null, null, null)
                    .SetCategory("Script")
                    .SetCategory("ExpandoObject")
                    .SetCategory("Indexing")
                    .SetCategory("return")
                    .Returns(58.3);
                yield return new TestCaseData(Resources.Script0023, null, null, null)
                    .SetCategory("Script")
                    .SetCategory("ExpandoObject")
                    .SetCategory("Indexing")
                    .SetCategory("Postfix operator")
                    .SetCategory("++")
                    .SetCategory("return")
                    .Returns(5);

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

                yield return new TestCaseData(Resources.Script0011, null, null, null)
                    .SetCategory("Script")
                    .SetCategory("Indexing assignation")
                    .SetCategory("=")
                    .SetCategory("+=")
                    .SetCategory("-=")
                    .SetCategory("*=")
                    .SetCategory("/=")
                    .SetCategory("/=")
                    .SetCategory("%=")
                    .SetCategory("^=")
                    .SetCategory("&=")
                    .SetCategory("|=")
                    .SetCategory("<<=")
                    .SetCategory("<<=")
                    .SetCategory("List function")
                    .Returns("[8,11,3,15,2,1,6,1,7,20,1]");

                yield return new TestCaseData(Resources.Script0012, null, null, null)
                    .SetCategory("Script")
                    .SetCategory("Indexing postfix operators")
                    .SetCategory("=")
                    .SetCategory("+=")
                    .SetCategory("++")
                    .SetCategory("--")
                    .SetCategory("List function")
                    .Returns("[6,4,10,6,10,4]");

                #endregion
            }
        }

        [TestCaseSource(nameof(TestCasesForScriptEvaluateTests))]
        public object TestCasesForScriptEvaluate(string script, ExpressionEvaluator evaluator, Action PreExecuteAssertAction, Action PostExecuteAssertAction)
        {
            evaluator = evaluator ?? new ExpressionEvaluator();

            evaluator.EvaluateVariable += Evaluator_EvaluateVariable;

            evaluator.Namespaces.Add("CodingSeb.ExpressionEvaluator.Tests");

            PreExecuteAssertAction?.Invoke();

            object result = evaluator.ScriptEvaluate(evaluator.RemoveComments(script));

            PostExecuteAssertAction?.Invoke();

            evaluator.EvaluateVariable -= Evaluator_EvaluateVariable;

            return result;
        }

        private void Evaluator_EvaluateVariable(object sender, VariableEvaluationEventArg e)
        {
            if (e.This != null && e.Name.Equals("Json"))
            {
                e.Value = JsonConvert.SerializeObject(e.This);
            }
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
