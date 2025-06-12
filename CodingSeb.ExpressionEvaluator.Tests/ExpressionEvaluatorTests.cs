﻿using Newtonsoft.Json;
using NUnit.Framework;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace CodingSeb.ExpressionEvaluator.Tests
{
    [TestFixture]
    [DefaultFloatingPointTolerance(0.00001)]
    public class ExpressionEvaluatorTests
    {
        #region Type testing

        #region Test cases for TypeTesting

        #region IntFormats
        [TestCase("45", typeof(int), Category = "IntFormats")]
        [TestCase("0", typeof(int), Category = "IntFormats")]
        [TestCase("-72346", typeof(int), Category = "IntFormats")]
        #endregion

        #region DoubleFormats
        [TestCase("123.54", typeof(double), Category = "DoubleFormats")]
        [TestCase("0.0", typeof(double), Category = "DoubleFormats")]
        [TestCase("0d", typeof(double), Category = "DoubleFormats")]
        [TestCase("-146.678", typeof(double), Category = "DoubleFormats")]
        [TestCase("123.54e-12", typeof(double), Category = "DoubleFormats")]
        [TestCase("-146.678e-12", typeof(double), Category = "DoubleFormats")]
        [TestCase("45d", typeof(double), Category = "DoubleFormats")]
        [TestCase("123.54e-12d", typeof(double), Category = "DoubleFormats")]
        [TestCase("-146.678e-12d", typeof(double), Category = "DoubleFormats")]
        #endregion

        #region FloatFormats
        [TestCase("45f", typeof(float), Category = "FloatFormats")]
        [TestCase("0f", typeof(float), Category = "FloatFormats")]
        [TestCase("-63f", typeof(float), Category = "FloatFormats")]
        [TestCase("123.54f", typeof(float), Category = "FloatFormats")]
        [TestCase("-146.678f", typeof(float), Category = "FloatFormats")]
        [TestCase("123.54e-12f", typeof(float), Category = "FloatFormats")]
        [TestCase("-146.678e-12f", typeof(float), Category = "FloatFormats")]
        #endregion

        #region UIntFormats
        [TestCase("45u", typeof(uint), Category = "UIntFormats")]
        [TestCase("0u", typeof(uint), Category = "UIntFormats")]
        [TestCase("123.54e6u", typeof(uint), Category = "UIntFormats")]
        #endregion

        #region LongFormats
        [TestCase("45l", typeof(long), Category = "LongFormats")]
        [TestCase("0l", typeof(long), Category = "LongFormats")]
        [TestCase("-63l", typeof(long), Category = "LongFormats")]
        [TestCase("123.54e6l", typeof(long), Category = "LongFormats")]
        #endregion

        #region ULongFormats
        [TestCase("45ul", typeof(ulong), Category = "ULongFormats")]
        [TestCase("0ul", typeof(ulong), Category = "ULongFormats")]
        [TestCase("123.54e6ul", typeof(ulong), Category = "ULongFormats")]
        #endregion

        #region DecimalFormats
        [TestCase("45m", typeof(decimal), Category = "DecimalFormats")]
        [TestCase("0m", typeof(decimal), Category = "DecimalFormats")]
        [TestCase("-63m", typeof(decimal), Category = "DecimalFormats")]
        [TestCase("123.54m", typeof(decimal), Category = "DecimalFormats")]
        [TestCase("-146.678m", typeof(decimal), Category = "DecimalFormats")]
        [TestCase("123.54e-12m", typeof(decimal), Category = "DecimalFormats")]
        [TestCase("-146.678e-12m", typeof(decimal), Category = "DecimalFormats")]
        #endregion

        #region Lists & Arrays
        [TestCase("Array(14, \"A text for test\", 2.5, true)", typeof(object[]), Category = "Standard Functions,Array Function")]
        [TestCase("List(14, \"A text for test\", 2.5, true)", typeof(List<object>), Category = "Standard Functions,List Function")]
        #endregion

        #endregion
        public void TypeTesting(string expression, Type type)
        {
            ExpressionEvaluator evaluator = new ExpressionEvaluator();

            evaluator.Evaluate(expression)
                .ShouldBeOfType(type);
        }

        #endregion

        #region Direct Expression Evaluation

        #region Test cases for DirectExpressionEvaluation

        #region Other bases numbers

        [TestCase("0xab", ExpectedResult = 0xab, Category = "HexNumber")]
        [TestCase("0xAB", ExpectedResult = 0xab, Category = "HexNumber")]
        [TestCase("0x1", ExpectedResult = 0x1, Category = "HexNumber")]
        [TestCase("0xf", ExpectedResult = 0xf, Category = "HexNumber")]
        [TestCase("-0xf", ExpectedResult = -0xf, Category = "HexNumber")]
        [TestCase("0xff_2a", ExpectedResult = 0xff_2a, Category = "HexNumber")]

        [TestCase("0b01100111", ExpectedResult = 0b01100111, Category = "BinaryNumber")]
        [TestCase("0b0100", ExpectedResult = 0b0100, Category = "BinaryNumber")]
        [TestCase("0b1010", ExpectedResult = 0b1010, Category = "BinaryNumber")]
        [TestCase("0b10_10", ExpectedResult = 0b10_10, Category = "BinaryNumber")]
        [TestCase("-0b10_10", ExpectedResult = -0b10_10, Category = "BinaryNumber")]

        #endregion

        #region Null Expression
        [TestCase("null", ExpectedResult = null, Category = "Null Expression")]
        #endregion

        #region Booleans Constants
        [TestCase("true", TestOf = typeof(bool), ExpectedResult = true, Category = "Boolean Constants")]
        [TestCase("false", TestOf = typeof(bool), ExpectedResult = false, Category = "Boolean Constants")]
        #endregion

        #region String Operations
        [TestCase("\"Hello World\"", TestOf = typeof(string), ExpectedResult = "Hello World", Category = "SimpleString")]
        [TestCase("\"Hello\" + \"World\"", TestOf = typeof(string), ExpectedResult = "HelloWorld", Category = "SimpleString")]

        [TestCase("\"\\\"\"", TestOf = typeof(string), ExpectedResult = "\"", Category = "StringEscape")]
        [TestCase("\"\\n\"", TestOf = typeof(string), ExpectedResult = "\n", Category = "StringEscape")]
        [TestCase("\"\\r\"", TestOf = typeof(string), ExpectedResult = "\r", Category = "StringEscape")]
        [TestCase("\"\\t\"", TestOf = typeof(string), ExpectedResult = "\t", Category = "StringEscape")]
        [TestCase("\"" + @"\\" + "\"", TestOf = typeof(string), ExpectedResult = @"\", Category = "StringEscape")]
        [TestCase("\"" + @"\\\n" + "\"", TestOf = typeof(string), ExpectedResult = "\\\n", Category = "StringEscape")]
        [TestCase("@\"" + @"\\n" + "\"", TestOf = typeof(string), ExpectedResult = @"\\n", Category = "StringEscape")]

        [TestCase("$\"Hello {1 + 2}\"", TestOf = typeof(string), ExpectedResult = "Hello 3", Category = "StringInterpolation")]
        [TestCase("$\"{'\"'}\"", TestOf = typeof(string), ExpectedResult = "\"", Category = "StringInterpolation")]
        [TestCase("$\"{ '\"' }\"", TestOf = typeof(string), ExpectedResult = "\"", Category = "StringInterpolation")]
        [TestCase("$\"{{\"", TestOf = typeof(string), ExpectedResult = "{", Category = "StringInterpolation")]
        [TestCase("$\"{ \"{\" }\"", TestOf = typeof(string), ExpectedResult = "{", Category = "StringInterpolation")]
        [TestCase("$\"Test { 5+5 } Test\"", TestOf = typeof(string), ExpectedResult = "Test 10 Test", Category = "StringInterpolation")]
        [TestCase("$\"Test { 5+5 + \" Test\" } Test\"", TestOf = typeof(string), ExpectedResult = "Test 10 Test Test", Category = "StringInterpolation")]
        [TestCase("$\"Test { 5+5 + \" Test{\" } Test\"", TestOf = typeof(string), ExpectedResult = "Test 10 Test{ Test", Category = "StringInterpolation")]
        [TestCase("$\"Test { 5+5 + \" Test{{ }\" } Test\"", TestOf = typeof(string), ExpectedResult = "Test 10 Test{{ } Test", Category = "StringInterpolation")]

        [TestCase("$\"Hello { $\"TS\"}\"", TestOf = typeof(string), ExpectedResult = "Hello TS", Category = "StringInterpolationInCascade")]
        [TestCase("$\"Hello { $\"T{{S\"}\"", TestOf = typeof(string), ExpectedResult = "Hello T{S", Category = "StringInterpolationInCascade")]
        [TestCase("$\"Hello { $\"T}}S\"}\"", TestOf = typeof(string), ExpectedResult = "Hello T}S", Category = "StringInterpolationInCascade")]
        [TestCase("$\"Hello { $\"T{1 + 2}\"}\"", TestOf = typeof(string), ExpectedResult = "Hello T3", Category = "StringInterpolationInCascade")]
        [TestCase("$\"Hello { $\"T{1 + 2 + \"S\"}\"}\"", TestOf = typeof(string), ExpectedResult = "Hello T3S", Category = "StringInterpolationInCascade")]
        [TestCase("$\"Hello { $\"T{1 + 2 + $\"S\"}\"}\"", TestOf = typeof(string), ExpectedResult = "Hello T3S", Category = "StringInterpolationInCascade")]
        [TestCase("$\"Hello { $\"T{1 + 2 + $\"S{ 2 + 2 }\"}\"}\"", TestOf = typeof(string), ExpectedResult = "Hello T3S4", Category = "StringInterpolationInCascade")]
        [TestCase("$\"Hello { $\"T{1 + 2 + $\"S{ 2 + 2 } Test\"}\"}\"", TestOf = typeof(string), ExpectedResult = "Hello T3S4 Test", Category = "StringInterpolationInCascade")]
        [TestCase("$\"Hello { $\"T{1 + 2 + $\"S{ 2 + \" Test\" }\"}\"}\"", TestOf = typeof(string), ExpectedResult = "Hello T3S2 Test", Category = "StringInterpolationInCascade")]
        [TestCase("$\"Hello { $\"T{1 + 2 + $\"S{ 2 + $\" Test\" }\"}\"}\"", TestOf = typeof(string), ExpectedResult = "Hello T3S2 Test", Category = "StringInterpolationInCascade")]
        [TestCase("$\"Hello { $\"T{1 + 2 + $\"S{ 2 + $\" Test{ 2 + 2 }\" }\"}\"}\"", TestOf = typeof(string), ExpectedResult = "Hello T3S2 Test4", Category = "StringInterpolationInCascade")]

        [TestCase("\"Hello\" + (\"Test\" + \"(\")", TestOf = typeof(string), ExpectedResult = "HelloTest(", Category = "StringBetweenParenthis")]
        [TestCase("\"Hello\" + (\"Test\" + \")\")", TestOf = typeof(string), ExpectedResult = "HelloTest)", Category = "StringBetweenParenthis")]

        [TestCase("\"Hello\" + (\"Test\" + $\"{ Abs(int.Parse(\"-4\"))}\")", TestOf = typeof(string), ExpectedResult = "HelloTest4", Category = "StringWithParenthisOrComaInFunctionsArgs")]
        [TestCase("\"Text()\".Replace(\"(\", \"x\")", TestOf = typeof(string), ExpectedResult = "Textx)", Category = "StringWithParenthisOrComaInFunctionsArgs")]
        [TestCase("\"Text()\".Replace(\"x\", \",\")", TestOf = typeof(string), ExpectedResult = "Te,t()", Category = "StringWithParenthisOrComaInFunctionsArgs")]
        [TestCase("\"Text()\".Replace(\"(\", \",\")", TestOf = typeof(string), ExpectedResult = "Text,)", Category = "StringWithParenthisOrComaInFunctionsArgs")]

        [TestCase("\"Hello,Test,What\".Split(ArrayOfType(typeof(char), ',')).Length", ExpectedResult = 3, Category = "StringSplit,ArrayOfType")]
        [TestCase("\"Hello,Test,What\".Split(ArrayOfType(typeof(char), ',')).Json", ExpectedResult = "[\"Hello\",\"Test\",\"What\"]", Category = "StringSplit,ArrayOfType")]
        [TestCase("\"Hello,Test,What\".Split(new char[]{','}).Length", ExpectedResult = 3, Category = "StringSplit,Array instanciation")]
        [TestCase("\"Hello,Test,What\".Split(new char[]{','}).Json", ExpectedResult = "[\"Hello\",\"Test\",\"What\"]", Category = "StringSplit,Array instanciation")]
        #endregion

        #region char
        [TestCase("'a'", TestOf = typeof(char), ExpectedResult = 'a', Category = "char")]
        [TestCase("'g'", TestOf = typeof(char), ExpectedResult = 'g', Category = "char")]
        [TestCase("'z'", TestOf = typeof(char), ExpectedResult = 'z', Category = "char")]
        [TestCase("'A'", TestOf = typeof(char), ExpectedResult = 'A', Category = "char")]
        [TestCase("'Q'", TestOf = typeof(char), ExpectedResult = 'Q', Category = "char")]
        [TestCase("'Z'", TestOf = typeof(char), ExpectedResult = 'Z', Category = "char")]
        [TestCase("'é'", TestOf = typeof(char), ExpectedResult = 'é', Category = "char")]
        [TestCase("'è'", TestOf = typeof(char), ExpectedResult = 'è', Category = "char")]
        [TestCase("'ô'", TestOf = typeof(char), ExpectedResult = 'ô', Category = "char")]
        [TestCase("'ç'", TestOf = typeof(char), ExpectedResult = 'ç', Category = "char")]
        [TestCase("'%'", TestOf = typeof(char), ExpectedResult = '%', Category = "char")]
        [TestCase("'('", TestOf = typeof(char), ExpectedResult = '(', Category = "char")]
        [TestCase("'\"'", TestOf = typeof(char), ExpectedResult = '"', Category = "char")]
        [TestCase(@"'\\'", TestOf = typeof(char), ExpectedResult = '\\', Category = "char")]
        [TestCase(@"'\''", TestOf = typeof(char), ExpectedResult = '\'', Category = "char")]
        [TestCase(@"'\0'", TestOf = typeof(char), ExpectedResult = '\0', Category = "char")]
        [TestCase(@"'\a'", TestOf = typeof(char), ExpectedResult = '\a', Category = "char")]
        [TestCase(@"'\b'", TestOf = typeof(char), ExpectedResult = '\b', Category = "char")]
        [TestCase(@"'\f'", TestOf = typeof(char), ExpectedResult = '\f', Category = "char")]
        [TestCase(@"'\n'", TestOf = typeof(char), ExpectedResult = '\n', Category = "char")]
        [TestCase(@"'\r'", TestOf = typeof(char), ExpectedResult = '\r', Category = "char")]
        [TestCase(@"'\t'", TestOf = typeof(char), ExpectedResult = '\t', Category = "char")]
        [TestCase(@"'\v'", TestOf = typeof(char), ExpectedResult = '\v', Category = "char")]
        [TestCase("'\"'", TestOf = typeof(char), ExpectedResult = '"', Category = "char")]
        [TestCase("\"hello\" + ' ' + '!'", ExpectedResult = "hello !", Category = "char")]
        [TestCase("(int)'a'", ExpectedResult = 97, Category = "char")]
        [TestCase("'a'.CompareTo('b')", ExpectedResult = -1, Category = "char")]
        [TestCase("'a'.Equals('b')", ExpectedResult = false, Category = "char")]
        [TestCase("'b'.Equals('b')", ExpectedResult = true, Category = "char")]
        [TestCase("char.GetNumericValue('1')", ExpectedResult = 1, Category = "char")]
        [TestCase("char.IsControl('\t')", ExpectedResult = true, Category = "char")]
        [TestCase("char.IsDigit('f')", ExpectedResult = false, Category = "char")]
        [TestCase("char.IsDigit('3')", ExpectedResult = true, Category = "char")]
        [TestCase("char.IsLetter(',')", ExpectedResult = false, Category = "char")]
        [TestCase("char.IsLetter('R')", ExpectedResult = true, Category = "char")]
        [TestCase("char.IsLetter('h')", ExpectedResult = true, Category = "char")]
        [TestCase("char.IsLower('u')", ExpectedResult = true, Category = "char")]
        [TestCase("char.IsLower('U')", ExpectedResult = false, Category = "char")]
        [TestCase("char.IsPunctuation('.')", ExpectedResult = true, Category = "char")]
        [TestCase("char.IsSeparator(\"test string\", 4)", ExpectedResult = true, Category = "char")]
        [TestCase("char.IsSymbol('+')", ExpectedResult = true, Category = "char")]
        [TestCase("char.IsWhiteSpace(\"test string\", 4)", ExpectedResult = true, Category = "char")]
        [TestCase("char.Parse(\"S\")", ExpectedResult = 'S', Category = "char")]
        [TestCase("char.ToLower('M')", ExpectedResult = 'm', Category = "char")]
        [TestCase("'x'.ToString()", ExpectedResult = "x", Category = "char")]

        #endregion

        #region SimpleAddition
        [TestCase("-60 + -10", TestOf = typeof(int), ExpectedResult = -70, Category = "SimpleAddition")]
        [TestCase("-1 + -10", TestOf = typeof(int), ExpectedResult = -11, Category = "SimpleAddition")]
        [TestCase("1 + -10", TestOf = typeof(int), ExpectedResult = -9, Category = "SimpleAddition")]
        [TestCase("0 + -10", TestOf = typeof(int), ExpectedResult = -10, Category = "SimpleAddition")]
        [TestCase("10 + -10", TestOf = typeof(int), ExpectedResult = 0, Category = "SimpleAddition")]
        [TestCase("467 + -10", TestOf = typeof(int), ExpectedResult = 457, Category = "SimpleAddition")]

        [TestCase("-60 + -1", TestOf = typeof(int), ExpectedResult = -61, Category = "SimpleAddition")]
        [TestCase("-1 + -1", TestOf = typeof(int), ExpectedResult = -2, Category = "SimpleAddition")]
        [TestCase("1 + -1", TestOf = typeof(int), ExpectedResult = 0, Category = "SimpleAddition")]
        [TestCase("0 + -1", TestOf = typeof(int), ExpectedResult = -1, Category = "SimpleAddition")]
        [TestCase("467 + -1", TestOf = typeof(int), ExpectedResult = 466, Category = "SimpleAddition")]

        [TestCase("-1232 + 0", TestOf = typeof(int), ExpectedResult = -1232, Category = "SimpleAddition")]
        [TestCase("-1 + 0", TestOf = typeof(int), ExpectedResult = -1, Category = "SimpleAddition")]
        [TestCase("1 + 0", TestOf = typeof(int), ExpectedResult = 1, Category = "SimpleAddition")]
        [TestCase("0 + 0", TestOf = typeof(int), ExpectedResult = 0, Category = "SimpleAddition")]
        [TestCase("467 + 0", TestOf = typeof(int), ExpectedResult = 467, Category = "SimpleAddition")]

        [TestCase("-60 + 1", TestOf = typeof(int), ExpectedResult = -59, Category = "SimpleAddition")]
        [TestCase("-1 + 1", TestOf = typeof(int), ExpectedResult = 0, Category = "SimpleAddition")]
        [TestCase("1 + 1", TestOf = typeof(int), ExpectedResult = 2, Category = "SimpleAddition")]
        [TestCase("0 + 1", TestOf = typeof(int), ExpectedResult = 1, Category = "SimpleAddition")]
        [TestCase("467 + 1", TestOf = typeof(int), ExpectedResult = 468, Category = "SimpleAddition")]

        [TestCase("-60 + 10", TestOf = typeof(int), ExpectedResult = -50, Category = "SimpleAddition")]
        [TestCase("-1 + 10", TestOf = typeof(int), ExpectedResult = 9, Category = "SimpleAddition")]
        [TestCase("1 + 10", TestOf = typeof(int), ExpectedResult = 11, Category = "SimpleAddition")]
        [TestCase("0 + 10", TestOf = typeof(int), ExpectedResult = 10, Category = "SimpleAddition")]
        [TestCase("10 + 10", TestOf = typeof(int), ExpectedResult = 20, Category = "SimpleAddition")]
        [TestCase("467 + 10", TestOf = typeof(int), ExpectedResult = 477, Category = "SimpleAddition")]

        [TestCase("-60+-10", TestOf = typeof(int), ExpectedResult = -70, Category = "SimpleAddition")]
        [TestCase("-1+-10", TestOf = typeof(int), ExpectedResult = -11, Category = "SimpleAddition")]
        [TestCase("1+-10", TestOf = typeof(int), ExpectedResult = -9, Category = "SimpleAddition")]
        [TestCase("0+-10", TestOf = typeof(int), ExpectedResult = -10, Category = "SimpleAddition")]
        [TestCase("10+-10", TestOf = typeof(int), ExpectedResult = 0, Category = "SimpleAddition")]
        [TestCase("467+-10", TestOf = typeof(int), ExpectedResult = 457, Category = "SimpleAddition")]

        [TestCase("-60+-1", TestOf = typeof(int), ExpectedResult = -61, Category = "SimpleAddition")]
        [TestCase("-1+-1", TestOf = typeof(int), ExpectedResult = -2, Category = "SimpleAddition")]
        [TestCase("1+-1", TestOf = typeof(int), ExpectedResult = 0, Category = "SimpleAddition")]
        [TestCase("0+-1", TestOf = typeof(int), ExpectedResult = -1, Category = "SimpleAddition")]
        [TestCase("467+-1", TestOf = typeof(int), ExpectedResult = 466, Category = "SimpleAddition")]

        [TestCase("-1232++0", TestOf = typeof(int), ExpectedResult = -1232, Category = "SimpleAddition")]
        [TestCase("-1++0", TestOf = typeof(int), ExpectedResult = -1, Category = "SimpleAddition")]
        [TestCase("1 ++0", TestOf = typeof(int), ExpectedResult = 1, Category = "SimpleAddition")]
        [TestCase("0 + +0", TestOf = typeof(int), ExpectedResult = 0, Category = "SimpleAddition")]
        [TestCase("467 + +0", TestOf = typeof(int), ExpectedResult = 467, Category = "SimpleAddition")]

        [TestCase("-60 + +1", TestOf = typeof(int), ExpectedResult = -59, Category = "SimpleAddition")]
        [TestCase("-1 + +1", TestOf = typeof(int), ExpectedResult = 0, Category = "SimpleAddition")]
        [TestCase("1 ++1", TestOf = typeof(int), ExpectedResult = 2, Category = "SimpleAddition")]
        [TestCase("0 ++1", TestOf = typeof(int), ExpectedResult = 1, Category = "SimpleAddition")]
        [TestCase("467 ++1", TestOf = typeof(int), ExpectedResult = 468, Category = "SimpleAddition")]

        [TestCase("-60 ++10", TestOf = typeof(int), ExpectedResult = -50, Category = "SimpleAddition")]
        [TestCase("-1 ++10", TestOf = typeof(int), ExpectedResult = 9, Category = "SimpleAddition")]
        [TestCase("1 ++10", TestOf = typeof(int), ExpectedResult = 11, Category = "SimpleAddition")]
        [TestCase("0 + +10", TestOf = typeof(int), ExpectedResult = 10, Category = "SimpleAddition")]
        [TestCase("10 + +10", TestOf = typeof(int), ExpectedResult = 20, Category = "SimpleAddition")]
        [TestCase("467 + +10", TestOf = typeof(int), ExpectedResult = 477, Category = "SimpleAddition")]
        #endregion

        #region SimpleSubstraction
        [TestCase("-60 - -10", TestOf = typeof(int), ExpectedResult = -50, Category = "SimpleSubstraction")]
        [TestCase("-1 - -10", TestOf = typeof(int), ExpectedResult = 9, Category = "SimpleSubstraction")]
        [TestCase("1 - -10", TestOf = typeof(int), ExpectedResult = 11, Category = "SimpleSubstraction")]
        [TestCase("0 - -10", TestOf = typeof(int), ExpectedResult = 10, Category = "SimpleSubstraction")]
        [TestCase("10 - -10", TestOf = typeof(int), ExpectedResult = 20, Category = "SimpleSubstraction")]
        [TestCase("467 - -10", TestOf = typeof(int), ExpectedResult = 477, Category = "SimpleSubstraction")]

        [TestCase("-60 - -1", TestOf = typeof(int), ExpectedResult = -59, Category = "SimpleSubstraction")]
        [TestCase("-1 - -1", TestOf = typeof(int), ExpectedResult = 0, Category = "SimpleSubstraction")]
        [TestCase("1 - -1", TestOf = typeof(int), ExpectedResult = 2, Category = "SimpleSubstraction")]
        [TestCase("0 - -1", TestOf = typeof(int), ExpectedResult = 1, Category = "SimpleSubstraction")]
        [TestCase("467 - -1", TestOf = typeof(int), ExpectedResult = 468, Category = "SimpleSubstraction")]

        [TestCase("-1232 - 0", TestOf = typeof(int), ExpectedResult = -1232, Category = "SimpleSubstraction")]
        [TestCase("-1 - 0", TestOf = typeof(int), ExpectedResult = -1, Category = "SimpleSubstraction")]
        [TestCase("1 - 0", TestOf = typeof(int), ExpectedResult = 1, Category = "SimpleSubstraction")]
        [TestCase("0 - 0", TestOf = typeof(int), ExpectedResult = 0, Category = "SimpleSubstraction")]
        [TestCase("467 - 0", TestOf = typeof(int), ExpectedResult = 467, Category = "SimpleSubstraction")]

        [TestCase("-60 - 1", TestOf = typeof(int), ExpectedResult = -61, Category = "SimpleSubstraction")]
        [TestCase("-1 - 1", TestOf = typeof(int), ExpectedResult = -2, Category = "SimpleSubstraction")]
        [TestCase("1 - 1", TestOf = typeof(int), ExpectedResult = 0, Category = "SimpleSubstraction")]
        [TestCase("0 - 1", TestOf = typeof(int), ExpectedResult = -1, Category = "SimpleSubstraction")]
        [TestCase("467 - 1", TestOf = typeof(int), ExpectedResult = 466, Category = "SimpleSubstraction")]

        [TestCase("-60 - 10", TestOf = typeof(int), ExpectedResult = -70, Category = "SimpleSubstraction")]
        [TestCase("-1 - 10", TestOf = typeof(int), ExpectedResult = -11, Category = "SimpleSubstraction")]
        [TestCase("1 - 10", TestOf = typeof(int), ExpectedResult = -9, Category = "SimpleSubstraction")]
        [TestCase("0 - 10", TestOf = typeof(int), ExpectedResult = -10, Category = "SimpleSubstraction")]
        [TestCase("10 - 10", TestOf = typeof(int), ExpectedResult = 0, Category = "SimpleSubstraction")]
        [TestCase("467 - 10", TestOf = typeof(int), ExpectedResult = 457, Category = "SimpleSubstraction")]

        [TestCase("-60--10", TestOf = typeof(int), ExpectedResult = -50, Category = "SimpleSubstraction")]
        [TestCase("-1--10", TestOf = typeof(int), ExpectedResult = 9, Category = "SimpleSubstraction")]
        [TestCase("1--10", TestOf = typeof(int), ExpectedResult = 11, Category = "SimpleSubstraction")]
        [TestCase("+0- -10", TestOf = typeof(int), ExpectedResult = 10, Category = "SimpleSubstraction")]
        [TestCase("+10 --10", TestOf = typeof(int), ExpectedResult = 20, Category = "SimpleSubstraction")]
        [TestCase("+467--10", TestOf = typeof(int), ExpectedResult = 477, Category = "SimpleSubstraction")]

        [TestCase("-60 --1", TestOf = typeof(int), ExpectedResult = -59, Category = "SimpleSubstraction")]
        [TestCase("-1 --1", TestOf = typeof(int), ExpectedResult = 0, Category = "SimpleSubstraction")]
        [TestCase("+1 --1", TestOf = typeof(int), ExpectedResult = 2, Category = "SimpleSubstraction")]
        [TestCase("+0 --1", TestOf = typeof(int), ExpectedResult = 1, Category = "SimpleSubstraction")]
        [TestCase("467 --1", TestOf = typeof(int), ExpectedResult = 468, Category = "SimpleSubstraction")]

        [TestCase("-1232 -0", TestOf = typeof(int), ExpectedResult = -1232, Category = "SimpleSubstraction")]
        [TestCase("-1 - -0", TestOf = typeof(int), ExpectedResult = -1, Category = "SimpleSubstraction")]
        [TestCase("+1 -+0", TestOf = typeof(int), ExpectedResult = 1, Category = "SimpleSubstraction")]
        [TestCase("+0 --0", TestOf = typeof(int), ExpectedResult = 0, Category = "SimpleSubstraction")]
        [TestCase("467 -+0", TestOf = typeof(int), ExpectedResult = 467, Category = "SimpleSubstraction")]

        [TestCase("-60 -+1", TestOf = typeof(int), ExpectedResult = -61, Category = "SimpleSubstraction")]
        [TestCase("-1 -+1", TestOf = typeof(int), ExpectedResult = -2, Category = "SimpleSubstraction")]
        [TestCase("1 -+1", TestOf = typeof(int), ExpectedResult = 0, Category = "SimpleSubstraction")]
        [TestCase("0 - +1", TestOf = typeof(int), ExpectedResult = -1, Category = "SimpleSubstraction")]
        [TestCase("467 - +1", TestOf = typeof(int), ExpectedResult = 466, Category = "SimpleSubstraction")]

        [TestCase("-60 - +10", TestOf = typeof(int), ExpectedResult = -70, Category = "SimpleSubstraction")]
        [TestCase("-1 - +10", TestOf = typeof(int), ExpectedResult = -11, Category = "SimpleSubstraction")]
        [TestCase("1 -+10", TestOf = typeof(int), ExpectedResult = -9, Category = "SimpleSubstraction")]
        [TestCase("0 -+10", TestOf = typeof(int), ExpectedResult = -10, Category = "SimpleSubstraction")]
        [TestCase("+10 -+10", TestOf = typeof(int), ExpectedResult = 0, Category = "SimpleSubstraction")]
        [TestCase("+467-+10", TestOf = typeof(int), ExpectedResult = 457, Category = "SimpleSubstraction")]
        #endregion

        #region SimpleMultiplication
        [TestCase("-12 * 5", TestOf = typeof(int), ExpectedResult = -60, Category = "SimpleMultiplication")]
        [TestCase("-1 * 5", TestOf = typeof(int), ExpectedResult = -5, Category = "SimpleMultiplication")]
        [TestCase("-1 * -23", TestOf = typeof(int), ExpectedResult = 23, Category = "SimpleMultiplication")]
        [TestCase("-1 * 1", TestOf = typeof(int), ExpectedResult = -1, Category = "SimpleMultiplication")]
        [TestCase("-1 * -1", TestOf = typeof(int), ExpectedResult = 1, Category = "SimpleMultiplication")]
        [TestCase("0 * 440", TestOf = typeof(int), ExpectedResult = 0, Category = "SimpleMultiplication")]
        [TestCase("0 * 0", TestOf = typeof(int), ExpectedResult = 0, Category = "SimpleMultiplication")]
        [TestCase("67326 * 0", TestOf = typeof(int), ExpectedResult = 0, Category = "SimpleMultiplication")]
        [TestCase("1 * 0", TestOf = typeof(int), ExpectedResult = 0, Category = "SimpleMultiplication")]
        [TestCase("1 * 1", TestOf = typeof(int), ExpectedResult = 1, Category = "SimpleMultiplication")]
        [TestCase("1 * 672", TestOf = typeof(int), ExpectedResult = 672, Category = "SimpleMultiplication")]
        [TestCase("3 * 4", TestOf = typeof(int), ExpectedResult = 12, Category = "SimpleMultiplication")]
        [TestCase("5.5 * 10", TestOf = typeof(int), ExpectedResult = 55, Category = "SimpleMultiplication")]
        [TestCase("-6.12 * 10", TestOf = typeof(double), ExpectedResult = -61.2, Category = "SimpleMultiplication")]
        #endregion

        #region SimpleDivision
        [TestCase("-10 / 5", TestOf = typeof(int), ExpectedResult = -2, Category = "SimpleMultiplication")]
        [TestCase("-6 / 6", TestOf = typeof(int), ExpectedResult = -1, Category = "SimpleMultiplication")]
        [TestCase("-1 / 2", TestOf = typeof(int), ExpectedResult = 0, Category = "SimpleMultiplication")]
        [TestCase("-1 / 2d", TestOf = typeof(double), ExpectedResult = -0.5, Category = "SimpleMultiplication")]
        [TestCase("0 / 2", TestOf = typeof(int), ExpectedResult = 0, Category = "SimpleMultiplication")]
        [TestCase("2 / 1", TestOf = typeof(int), ExpectedResult = 2, Category = "SimpleMultiplication")]
        [TestCase("1 / 1", TestOf = typeof(int), ExpectedResult = 1, Category = "SimpleMultiplication")]
        [TestCase("500 / 10", TestOf = typeof(int), ExpectedResult = 50, Category = "SimpleMultiplication")]
        [TestCase("6 / 4", TestOf = typeof(int), ExpectedResult = 1, Category = "SimpleMultiplication")]
        [TestCase("6d / 4d", TestOf = typeof(double), ExpectedResult = 1.5, Category = "SimpleMultiplication")]
        [TestCase("5.5 / 2", TestOf = typeof(double), ExpectedResult = 2.75, Category = "SimpleMultiplication")]
        #endregion

        #region DivAndMultiplyPriorityOverSubAndAdd
        [TestCase("5 - 10 / 2", TestOf = typeof(int), ExpectedResult = 0, Category = "DivAndMultiplyPriorityOverSubAndAdd")]
        [TestCase("5 + 10 / 2", TestOf = typeof(int), ExpectedResult = 10, Category = "DivAndMultiplyPriorityOverSubAndAdd")]
        [TestCase("5 - 10 * 2", TestOf = typeof(int), ExpectedResult = -15, Category = "DivAndMultiplyPriorityOverSubAndAdd")]
        [TestCase("5 + 10 * 2", TestOf = typeof(int), ExpectedResult = 25, Category = "DivAndMultiplyPriorityOverSubAndAdd")]
        #endregion

        #region Parenthesis Priority
        [TestCase("(5d - 10) / 2", TestOf = typeof(double), ExpectedResult = -2.5, Category = "ParenthesisPriority")]
        [TestCase("(5d + 10) / 2", TestOf = typeof(double), ExpectedResult = 7.5, Category = "ParenthesisPriority")]
        [TestCase("(5 - 10) * 2", TestOf = typeof(double), ExpectedResult = -10, Category = "ParenthesisPriority")]
        [TestCase("(5 + 10) * 2", TestOf = typeof(double), ExpectedResult = 30, Category = "ParenthesisPriority")]
        [TestCase("5 - (10 / 2)", TestOf = typeof(double), ExpectedResult = 0, Category = "ParenthesisPriority")]
        [TestCase("5 + (10 / 2)", TestOf = typeof(double), ExpectedResult = 10, Category = "ParenthesisPriority")]
        [TestCase("5 - (10 * 2)", TestOf = typeof(double), ExpectedResult = -15, Category = "ParenthesisPriority")]
        [TestCase("5 + (10 * 2)", TestOf = typeof(double), ExpectedResult = 25, Category = "ParenthesisPriority")]
        [TestCase("3 + (2 * (5 - 3 - (Abs(-5) - 6)))", TestOf = typeof(double), ExpectedResult = 9, Category = "ParenthesisPriority")]
        [TestCase("!(!false || false)", TestOf = typeof(double), ExpectedResult = false, Category = "ParenthesisPriority")]
        [TestCase("!(!false || false) || !(!true && true)", TestOf = typeof(double), ExpectedResult = true, Category = "ParenthesisPriority")]

        [TestCase("+(5d - 10) / 2", TestOf = typeof(double), ExpectedResult = -2.5, Category = "ParenthesisPriority")]
        [TestCase("+(5d + 10) / 2", TestOf = typeof(double), ExpectedResult = 7.5, Category = "ParenthesisPriority")]
        [TestCase("-(5 - 10) * -2", TestOf = typeof(double), ExpectedResult = -10, Category = "ParenthesisPriority")]
        [TestCase("-(5 + 10) * 2", TestOf = typeof(double), ExpectedResult = -30, Category = "ParenthesisPriority")]
        [TestCase("5 - +(10 / 2)", TestOf = typeof(double), ExpectedResult = 0, Category = "ParenthesisPriority")]
        [TestCase("5 + +(10 / 2)", TestOf = typeof(double), ExpectedResult = 10, Category = "ParenthesisPriority")]
        [TestCase("5 -+(10 * 2)", TestOf = typeof(double), ExpectedResult = -15, Category = "ParenthesisPriority")]
        [TestCase("5 - -(10 * 2)", TestOf = typeof(double), ExpectedResult = 25, Category = "ParenthesisPriority")]
        [TestCase("3 --(2 *+(5 - 3 - +(-Abs(-5) - 6)))", TestOf = typeof(double), ExpectedResult = 29, Category = "ParenthesisPriority")]
        #endregion

        #region BitwiseComplement
        [TestCase("~-10", TestOf = typeof(int), ExpectedResult = 9, Category = "BitwiseComplement")]
        [TestCase("~-2", TestOf = typeof(int), ExpectedResult = 1, Category = "BitwiseComplement")]
        [TestCase("~-1", TestOf = typeof(int), ExpectedResult = 0, Category = "BitwiseComplement")]
        [TestCase("~0", TestOf = typeof(int), ExpectedResult = -1, Category = "BitwiseComplement")]
        [TestCase("~1", TestOf = typeof(int), ExpectedResult = -2, Category = "BitwiseComplement")]
        [TestCase("~2", TestOf = typeof(int), ExpectedResult = -3, Category = "BitwiseComplement")]
        [TestCase("~10", TestOf = typeof(int), ExpectedResult = -11, Category = "BitwiseComplement")]
        #endregion

        #region SimpleModulo
        [TestCase("-4 % 2", TestOf = typeof(int), ExpectedResult = 0, Category = "SimpleModulo")]
        [TestCase("-3 % 2", TestOf = typeof(int), ExpectedResult = -1, Category = "SimpleModulo")]
        [TestCase("-2 % 2", TestOf = typeof(int), ExpectedResult = 0, Category = "SimpleModulo")]
        [TestCase("-1 % 2", TestOf = typeof(int), ExpectedResult = -1, Category = "SimpleModulo")]
        [TestCase("0 % 2", TestOf = typeof(int), ExpectedResult = 0, Category = "SimpleModulo")]
        [TestCase("1 % 2", TestOf = typeof(int), ExpectedResult = 1, Category = "SimpleModulo")]
        [TestCase("2 % 2", TestOf = typeof(int), ExpectedResult = 0, Category = "SimpleModulo")]
        [TestCase("3 % 2", TestOf = typeof(int), ExpectedResult = 1, Category = "SimpleModulo")]
        [TestCase("4 % 2", TestOf = typeof(int), ExpectedResult = 0, Category = "SimpleModulo")]
        [TestCase("5 % 2", TestOf = typeof(int), ExpectedResult = 1, Category = "SimpleModulo")]
        [TestCase("6 % 2", TestOf = typeof(int), ExpectedResult = 0, Category = "SimpleModulo")]
        [TestCase("7 % 2", TestOf = typeof(int), ExpectedResult = 1, Category = "SimpleModulo")]
        [TestCase("8 % 2", TestOf = typeof(int), ExpectedResult = 0, Category = "SimpleModulo")]

        [TestCase("-6 % 3", TestOf = typeof(int), ExpectedResult = 0, Category = "SimpleModulo")]
        [TestCase("-5 % 3", TestOf = typeof(int), ExpectedResult = -2, Category = "SimpleModulo")]
        [TestCase("-4 % 3", TestOf = typeof(int), ExpectedResult = -1, Category = "SimpleModulo")]
        [TestCase("-3 % 3", TestOf = typeof(int), ExpectedResult = 0, Category = "SimpleModulo")]
        [TestCase("-2 % 3", TestOf = typeof(int), ExpectedResult = -2, Category = "SimpleModulo")]
        [TestCase("-1 % 3", TestOf = typeof(int), ExpectedResult = -1, Category = "SimpleModulo")]
        [TestCase("0 % 3", TestOf = typeof(int), ExpectedResult = 0, Category = "SimpleModulo")]
        [TestCase("1 % 3", TestOf = typeof(int), ExpectedResult = 1, Category = "SimpleModulo")]
        [TestCase("2 % 3", TestOf = typeof(int), ExpectedResult = 2, Category = "SimpleModulo")]
        [TestCase("3 % 3", TestOf = typeof(int), ExpectedResult = 0, Category = "SimpleModulo")]
        [TestCase("4 % 3", TestOf = typeof(int), ExpectedResult = 1, Category = "SimpleModulo")]
        [TestCase("5 % 3", TestOf = typeof(int), ExpectedResult = 2, Category = "SimpleModulo")]
        [TestCase("6 % 3", TestOf = typeof(int), ExpectedResult = 0, Category = "SimpleModulo")]
        #endregion

        #region Boolean Tests Operators
        [TestCase("1 < 5", TestOf = typeof(bool), ExpectedResult = true, Category = "LowerThanBooleanOperator")]
        [TestCase("5 < 5", TestOf = typeof(bool), ExpectedResult = false, Category = "LowerThanBooleanOperator")]
        [TestCase("7 < 5", TestOf = typeof(bool), ExpectedResult = false, Category = "LowerThanBooleanOperator")]

        [TestCase("1 > 5", TestOf = typeof(bool), ExpectedResult = false, Category = "GreaterThanBooleanOperator")]
        [TestCase("5 > 5", TestOf = typeof(bool), ExpectedResult = false, Category = "GreaterThanBooleanOperator")]
        [TestCase("7 > 5", TestOf = typeof(bool), ExpectedResult = true, Category = "GreaterThanBooleanOperator")]

        [TestCase("1 <= 5", TestOf = typeof(bool), ExpectedResult = true, Category = "LowerThanOrEqualBooleanOperator")]
        [TestCase("5 <= 5", TestOf = typeof(bool), ExpectedResult = true, Category = "LowerThanOrEqualBooleanOperator")]
        [TestCase("7 <= 5", TestOf = typeof(bool), ExpectedResult = false, Category = "LowerThanOrEqualBooleanOperator")]

        [TestCase("1 >= 5", TestOf = typeof(bool), ExpectedResult = false, Category = "GreaterThanOrEqualBooleanOperator")]
        [TestCase("5 >= 5", TestOf = typeof(bool), ExpectedResult = true, Category = "GreaterThanOrEqualBooleanOperator")]
        [TestCase("7 >= 5", TestOf = typeof(bool), ExpectedResult = true, Category = "GreaterThanOrEqualBooleanOperator")]

        [TestCase("1 == 5", TestOf = typeof(bool), ExpectedResult = false, Category = "EqualBooleanOperator")]
        [TestCase("5 == 5", TestOf = typeof(bool), ExpectedResult = true, Category = "EqualBooleanOperator")]
        [TestCase("7 == 5", TestOf = typeof(bool), ExpectedResult = false, Category = "EqualBooleanOperator")]

        [TestCase("1 != 5", TestOf = typeof(bool), ExpectedResult = true, Category = "NotEqualBooleanOperator")]
        [TestCase("5 != 5", TestOf = typeof(bool), ExpectedResult = false, Category = "NotEqualBooleanOperator")]
        [TestCase("7 != 5", TestOf = typeof(bool), ExpectedResult = true, Category = "NotEqualBooleanOperator")]

        [TestCase("1 is string", TestOf = typeof(bool), ExpectedResult = false, Category = "IsOperatorBooleanOperator")]
        [TestCase("\"Test\" is string", TestOf = typeof(bool), ExpectedResult = true, Category = "IsOperatorBooleanOperator")]
        [TestCase("true is string", TestOf = typeof(bool), ExpectedResult = false, Category = "IsOperatorBooleanOperator")]
        [TestCase("null is string", TestOf = typeof(bool), ExpectedResult = false, Category = "IsOperatorBooleanOperator")]

        [TestCase("true && true", TestOf = typeof(bool), ExpectedResult = true, Category = "ConditionalAndBooleanOperator")]
        [TestCase("false && true", TestOf = typeof(bool), ExpectedResult = false, Category = "ConditionalAndBooleanOperator")]
        [TestCase("true && false", TestOf = typeof(bool), ExpectedResult = false, Category = "ConditionalAndBooleanOperator")]
        [TestCase("false && false", TestOf = typeof(bool), ExpectedResult = false, Category = "ConditionalAndBooleanOperator")]

        [TestCase("true || true", TestOf = typeof(bool), ExpectedResult = true, Category = "ConditionalOrBooleanOperator")]
        [TestCase("false || true", TestOf = typeof(bool), ExpectedResult = true, Category = "ConditionalOrBooleanOperator")]
        [TestCase("true || false", TestOf = typeof(bool), ExpectedResult = true, Category = "ConditionalOrBooleanOperator")]
        [TestCase("false || false", TestOf = typeof(bool), ExpectedResult = false, Category = "ConditionalOrBooleanOperator")]

        [TestCase("!true", TestOf = typeof(bool), ExpectedResult = false, Category = "NegationBooleanOperator")]
        [TestCase("!false", TestOf = typeof(bool), ExpectedResult = true, Category = "ConditionalOrBooleanOperator")]
        [TestCase("!(5 > 2)", TestOf = typeof(bool), ExpectedResult = false, Category = "ConditionalOrBooleanOperator")]
        [TestCase("!(5 < 2)", TestOf = typeof(bool), ExpectedResult = true, Category = "ConditionalOrBooleanOperator")]

        #endregion

        #region Null Coalescing Operator
        [TestCase("\"Option1\" ?? \"Option2\"", TestOf = typeof(string), ExpectedResult = "Option1", Category = "Null Coalescing Operator")]
        [TestCase("null ?? \"Option2\"", TestOf = typeof(string), ExpectedResult = "Option2", Category = "Null Coalescing Operator")]
        #endregion

        #region Null conditional Operator
        [TestCase("null?.Trim()", ExpectedResult = null, Category = "Null conditional Operator")]
        #endregion

        #region default values
        [TestCase("default(int)", TestOf = typeof(int), ExpectedResult = 0, Category = "default values")]
        [TestCase("default(bool)", TestOf = typeof(bool), ExpectedResult = false, Category = "default values")]
        [TestCase("default(System.Boolean)", TestOf = typeof(bool), ExpectedResult = false, Category = "default values, Inline namespaces")]
        #endregion

        #region typeof keyword
        [TestCase("typeof(int)", ExpectedResult = typeof(int), Category = "typeof keyword")]
        [TestCase("typeof(float)", ExpectedResult = typeof(float), Category = "typeof keyword")]
        [TestCase("typeof(string)", ExpectedResult = typeof(string), Category = "typeof keyword")]
        [TestCase("typeof(Regex)", ExpectedResult = typeof(Regex), Category = "typeof keyword")]
        [TestCase("typeof(System.Text.RegularExpressions.Regex)", ExpectedResult = typeof(Regex), Category = "typeof keyword,inline namespace")]
        [TestCase("typeof(string) == \"Hello\".GetType()", ExpectedResult = true, Category = "typeof keyword")]
        [TestCase("typeof(int) == 12.GetType()", ExpectedResult = true, Category = "typeof keyword")]
        [TestCase("typeof(string) == 12.GetType()", ExpectedResult = false, Category = "typeof keyword")]
        #endregion

        #region sizeof keyword

        [TestCase("sizeof(sbyte)", ExpectedResult = sizeof(sbyte), Category = "sizeof keyword")]
        [TestCase("sizeof(byte)", ExpectedResult = sizeof(byte), Category = "sizeof keyword")]
        [TestCase("sizeof(short)", ExpectedResult = sizeof(short), Category = "sizeof keyword")]
        [TestCase("sizeof(ushort)", ExpectedResult = sizeof(ushort), Category = "sizeof keyword")]
        [TestCase("sizeof(int)", ExpectedResult = sizeof(int), Category = "sizeof keyword")]
        [TestCase("sizeof(uint)", ExpectedResult = sizeof(uint), Category = "sizeof keyword")]
        [TestCase("sizeof(long)", ExpectedResult = sizeof(long), Category = "sizeof keyword")]
        [TestCase("sizeof(ulong)", ExpectedResult = sizeof(ulong), Category = "sizeof keyword")]
        [TestCase("sizeof(char)", ExpectedResult = sizeof(char), Category = "sizeof keyword")]
        [TestCase("sizeof(float)", ExpectedResult = sizeof(float), Category = "sizeof keyword")]
        [TestCase("sizeof(double)", ExpectedResult = sizeof(double), Category = "sizeof keyword")]
        [TestCase("sizeof(decimal)", ExpectedResult = sizeof(decimal), Category = "sizeof keyword")]
        [TestCase("sizeof(bool)", ExpectedResult = sizeof(bool), Category = "sizeof keyword")]

        #endregion

        #region Create instance with new Keyword
        [TestCase("new ClassForTest1().GetType()", ExpectedResult = typeof(ClassForTest1), Category = "Create instance with new Keyword")]
        [TestCase("new ClassForTest2(15).GetType()", ExpectedResult = typeof(ClassForTest2), Category = "Create instance with new Keyword")]
        [TestCase("new ClassForTest2(15).Value1", ExpectedResult = 15, Category = "Create instance with new Keyword")]
        [TestCase("new CodingSeb.ExpressionEvaluator.Tests.OtherNamespace.ClassInOtherNameSpace1().Value1", ExpectedResult = 26, Category = "Create instance with new Keyword,Inline namespace")]
        [TestCase("new Regex(@\"\\w*[n]\\w*\").Match(\"Which word contains the desired letter ?\").Value", ExpectedResult = "contains", Category = "Create instance with new Keyword")]
        [TestCase("new List<string>(){ \"Hello\", \"Test\" }.GetType()", ExpectedResult = typeof(List<string>), Category = "Create instance with new Keyword, Collection Initializer")]
        [TestCase("new List<string>(){ \"Hello\", \"Test\" }.Count", ExpectedResult = 2, Category = "Create instance with new Keyword, Collection Initializer")]
        [TestCase("new List<string>(){ \"Hello\", \"Test\" }[0]", ExpectedResult = "Hello", Category = "Create instance with new Keyword, Collection Initializer")]
        [TestCase("new List<string>(){ \"Hello\", \"Test\" }[1]", ExpectedResult = "Test", Category = "Create instance with new Keyword, Collection Initializer")]
        [TestCase("new List<string>{ \"Hello\", \"Test\" }.GetType()", ExpectedResult = typeof(List<string>), Category = "Create instance with new Keyword, Collection Initializer")]
        [TestCase("new List<string>{ \"Hello\", \"Test\" }.Count", ExpectedResult = 2, Category = "Create instance with new Keyword, Collection Initializer")]
        [TestCase("new List<string>{ \"Hello\", \"Test\" }[0]", ExpectedResult = "Hello", Category = "Create instance with new Keyword, Collection Initializer")]
        [TestCase("new List<string>{ \"Hello\", \"Test\" }[1]", ExpectedResult = "Test", Category = "Create instance with new Keyword, Collection Initializer")]
        [TestCase("new ClassForTest1(){ IntProperty = 100, StringProperty = \"A Text\" }.GetType()", ExpectedResult = typeof(ClassForTest1), Category = "Create instance with new Keyword, Object Initializer")]
        [TestCase("new ClassForTest1(){ IntProperty = 100, StringProperty = \"A Text\" }.IntProperty", ExpectedResult = 100, Category = "Create instance with new Keyword, Object Initializer")]
        [TestCase("new ClassForTest1(){ IntProperty = 100, StringProperty = \"A Text\" }.StringProperty", ExpectedResult = "A Text", Category = "Create instance with new Keyword, Object Initializer")]
        [TestCase("new ClassForTest1{ IntProperty = 100, StringProperty = \"A Text\" }.GetType()", ExpectedResult = typeof(ClassForTest1), Category = "Create instance with new Keyword, Object Initializer")]
        [TestCase("new ClassForTest1{ IntProperty = 100, StringProperty = \"A Text\" }.IntProperty", ExpectedResult = 100, Category = "Create instance with new Keyword, Object Initializer")]
        [TestCase("new ClassForTest1{ IntProperty = 100, StringProperty = \"A Text\" }.StringProperty", ExpectedResult = "A Text", Category = "Create instance with new Keyword, Object Initializer")]
        [TestCase("new ClassForTest2(10){ Value2 = 100 }.GetType()", ExpectedResult = typeof(ClassForTest2), Category = "Create instance with new Keyword, Object Initializer")]
        [TestCase("new ClassForTest2(10){ Value2 = 100 }.Value1", ExpectedResult = 10, Category = "Create instance with new Keyword, Object Initializer")]
        [TestCase("new ClassForTest2(10){ Value2 = 100 }.Value2", ExpectedResult = 100, Category = "Create instance with new Keyword, Object Initializer")]
        [TestCase("new Dictionary<int, string>(){ [7] = \"seven\", [7+2] = \"nine\" }.GetType()", ExpectedResult = typeof(Dictionary<int, string>), Category = "Create instance with new Keyword, Dictionary Initializer")]
        [TestCase("new Dictionary<int, string>(){ [7] = \"seven\", [7+2] = \"nine\" }[7]", ExpectedResult = "seven", Category = "Create instance with new Keyword, Dictionary Initializer")]
        [TestCase("new Dictionary<int, string>(){ [7] = \"seven\", [7+2] = \"nine\" }[9]", ExpectedResult = "nine", Category = "Create instance with new Keyword, Dictionary Initializer")]
        [TestCase("new Dictionary<int, string>{ [7] = \"seven\", [7+2] = \"nine\" }.GetType()", ExpectedResult = typeof(Dictionary<int, string>), Category = "Create instance with new Keyword, Dictionary Initializer")]
        [TestCase("new Dictionary<int, string>{ [7] = \"seven\", [7+2] = \"nine\" }[7]", ExpectedResult = "seven", Category = "Create instance with new Keyword, Dictionary Initializer")]
        [TestCase("new Dictionary<int, string>{ [7] = \"seven\", [7+2] = \"nine\" }[9]", ExpectedResult = "nine", Category = "Create instance with new Keyword, Dictionary Initializer")]
        [TestCase("new Dictionary<string, int>(){ [\"seven\"] = 7, [\"nine\"] = 9 }.GetType()", ExpectedResult = typeof(Dictionary<string, int>), Category = "Create instance with new Keyword, Dictionary Initializer")]
        [TestCase("new Dictionary<string, int>(){ [\"seven\"] = 7, [\"nine\"] = 9 }[\"seven\"]", ExpectedResult = 7, Category = "Create instance with new Keyword, Dictionary Initializer")]
        [TestCase("new Dictionary<string, int>(){ [\"seven\"] = 7, [\"nine\"] = 9 }[\"nine\"]", ExpectedResult = 9, Category = "Create instance with new Keyword, Dictionary Initializer")]
        [TestCase("new Dictionary<string, int>{ [\"seven\"] = 7, [\"nine\"] = 9 }.GetType()", ExpectedResult = typeof(Dictionary<string, int>), Category = "Create instance with new Keyword, Dictionary Initializer")]
        [TestCase("new Dictionary<string, int>{ [\"seven\"] = 7, [\"nine\"] = 9 }[\"seven\"]", ExpectedResult = 7, Category = "Create instance with new Keyword, Dictionary Initializer")]
        [TestCase("new Dictionary<string, int>{ [\"seven\"] = 7, [\"nine\"] = 9 }[\"nine\"]", ExpectedResult = 9, Category = "Create instance with new Keyword, Dictionary Initializer")]
        [TestCase("new Dictionary<int, string>(){ {7 ,\"seven\"}, {7+2, \"nine\"} }.GetType()", ExpectedResult = typeof(Dictionary<int, string>), Category = "Create instance with new Keyword, Dictionary Initializer")]
        [TestCase("new Dictionary<int, string>(){ {7 ,\"seven\"}, {7+2, \"nine\"} }[7]", ExpectedResult = "seven", Category = "Create instance with new Keyword, Dictionary Initializer")]
        [TestCase("new Dictionary<int, string>(){ {7 ,\"seven\"}, {7+2, \"nine\"}  }[9]", ExpectedResult = "nine", Category = "Create instance with new Keyword, Dictionary Initializer")]
        [TestCase("new Dictionary<int, string>{ {7 ,\"seven\"}, {7+2, \"nine\"}  }.GetType()", ExpectedResult = typeof(Dictionary<int, string>), Category = "Create instance with new Keyword, Dictionary Initializer")]
        [TestCase("new Dictionary<int, string>{ {7 ,\"seven\"}, {7+2, \"nine\"}  }[7]", ExpectedResult = "seven", Category = "Create instance with new Keyword, Dictionary Initializer")]
        [TestCase("new Dictionary<int, string>{ {7 ,\"seven\"}, {7+2, \"nine\"} }[9]", ExpectedResult = "nine", Category = "Create instance with new Keyword, Dictionary Initializer")]
        [TestCase("new Dictionary<string, int>(){ {\"seven\", 7} , {\"nine\", 9 } }.GetType()", ExpectedResult = typeof(Dictionary<string, int>), Category = "Create instance with new Keyword, Dictionary Initializer")]
        [TestCase("new Dictionary<string, int>(){ {\"seven\", 7} , {\"nine\", 9 } }[\"seven\"]", ExpectedResult = 7, Category = "Create instance with new Keyword, Dictionary Initializer")]
        [TestCase("new Dictionary<string, int>(){ {\"seven\", 7} , {\"nine\", 9 } }[\"nine\"]", ExpectedResult = 9, Category = "Create instance with new Keyword, Dictionary Initializer")]
        [TestCase("new Dictionary<string, int>{ {\"seven\", 7} , {\"nine\", 9 }  }.GetType()", ExpectedResult = typeof(Dictionary<string, int>), Category = "Create instance with new Keyword, Dictionary Initializer")]
        [TestCase("new Dictionary<string, int>{ {\"seven\", 7} , {\"nine\", 9 }  }[\"seven\"]", ExpectedResult = 7, Category = "Create instance with new Keyword, Dictionary Initializer")]
        [TestCase("new Dictionary<string, int>{ {\"seven\", 7} , {\"nine\", 9 }  }[\"nine\"]", ExpectedResult = 9, Category = "Create instance with new Keyword, Dictionary Initializer")]

        #endregion

        #region Logical And Shift Operators
        [TestCase("2 & 8", TestOf = typeof(int), ExpectedResult = 0, Category = "LogicalAndOperator")]
        [TestCase("10 & 8", TestOf = typeof(int), ExpectedResult = 8, Category = "LogicalAndOperator")]

        [TestCase("2 ^ 8", TestOf = typeof(int), ExpectedResult = 10, Category = "LogicalXorOperator")]
        [TestCase("10 ^ 8", TestOf = typeof(int), ExpectedResult = 2, Category = "LogicalXorOperator")]

        [TestCase("2 | 8", TestOf = typeof(int), ExpectedResult = 10, Category = "LogicalOrOperator")]
        [TestCase("10 | 8", TestOf = typeof(int), ExpectedResult = 10, Category = "LogicalOrOperator")]

        [TestCase("1 << 2", TestOf = typeof(int), ExpectedResult = 4, Category = "ShiftLeftOperator")]
        [TestCase("2 << 2", TestOf = typeof(int), ExpectedResult = 8, Category = "ShiftLeftOperator")]

        [TestCase("4 >> 2", TestOf = typeof(int), ExpectedResult = 1, Category = "ShiftRightOperator")]
        [TestCase("8 >> 2", TestOf = typeof(int), ExpectedResult = 2, Category = "ShiftRightOperator")]
        #endregion

        #region Conditional Operator t ? x : y
        [TestCase("true ? \"Test gives yes\" : \"Test gives no\"", ExpectedResult = "Test gives yes", Category = "Conditional Operator t ? x : y")]
        [TestCase("false ? \"Test gives yes\" : \"Test gives no\"", ExpectedResult = "Test gives no", Category = "Conditional Operator t ? x : y")]
        [TestCase("4 < 5 ? \"Test gives yes\" : \"Test gives no\"", ExpectedResult = "Test gives yes", Category = "Conditional Operator t ? x : y")]
        [TestCase("4 > 5 ? \"Test gives yes\" : \"Test gives no\"", ExpectedResult = "Test gives no", Category = "Conditional Operator t ? x : y")]
        [TestCase("Abs(-4) < 10 / 2 ? \"Test gives yes\" : \"Test gives no\"", ExpectedResult = "Test gives yes", Category = "Conditional Operator t ? x : y")]
        [TestCase("Abs(-4) > 10 / 2 ? \"Test gives yes\" : \"Test gives no\"", ExpectedResult = "Test gives no", Category = "Conditional Operator t ? x : y")]
        [TestCase("Abs(-4) < 10 / 2 ? Abs(-3) : (Abs(-4) + 4) / 2", ExpectedResult = 3, Category = "Conditional Operator t ? x : y")]
        [TestCase("Abs(-4) > 10 / 2 ? Abs(-3) : (Abs(-4) + 4) / 2", ExpectedResult = 4, Category = "Conditional Operator t ? x : y")]
        [TestCase("Abs(-4) < 10 / 2 ? (true ? 6 : 3+2) : (false ? Abs(-18) : 100 / 2)", ExpectedResult = 6, Category = "Conditional Operator t ? x : y")]
        [TestCase("Abs(-4) > 10 / 2 ? (true ? 6 : 3+2) : (false ? Abs(-18) : 100 / 2)", ExpectedResult = 50, Category = "Conditional Operator t ? x : y")]
        [TestCase("Abs(-4) > 10 / 2?(true ? 6 : 3+2):(false?Abs(-18):100 / 2)", ExpectedResult = 50, Category = "Conditional Operator t ? x : y")]
        [TestCase("1==1?true:false", ExpectedResult = true, Category = "Conditional Operator t ? x : y")]
        [TestCase("1 == 2 ? 3 == 4 ? 1 : 0 : 0", ExpectedResult = 0, Category = "Conditional Operator t ? x : y")]
        [TestCase("1 == 1 ? 3 == 3 ? 1 : 0 : 0", ExpectedResult = 1, Category = "Conditional Operator t ? x : y")]
        [TestCase("false ? true ? 10 : 20 : 30", ExpectedResult = 30, Category = "Conditional Operator t ? x : y")]
        [TestCase("true ? false ? 10 : 20 : 30", ExpectedResult = 20, Category = "Conditional Operator t ? x : y")]
        #endregion

        #region Math Constants
        [TestCase("Pi", TestOf = typeof(double), ExpectedResult = Math.PI, Category = "Math Constants")]
        [TestCase("E", TestOf = typeof(double), ExpectedResult = Math.E, Category = "Math Constants")]
        [TestCase("+Pi", TestOf = typeof(double), ExpectedResult = +Math.PI, Category = "Math Constants,Unary +")]
        [TestCase("+E", TestOf = typeof(double), ExpectedResult = +Math.E, Category = "Math Constants,Unary +")]
        [TestCase("-Pi", TestOf = typeof(double), ExpectedResult = -Math.PI, Category = "Math Constants,Unary -")]
        [TestCase("-E", TestOf = typeof(double), ExpectedResult = -Math.E, Category = "Math Constants,Unary -")]
        [TestCase("-Pi + +Pi", TestOf = typeof(double), ExpectedResult = 0, Category = "Math Constants,Unary -")]
        [TestCase("-E - -E", TestOf = typeof(double), ExpectedResult = 0, Category = "Math Constants,Unary -")]
        #endregion

        #region Lambda functions
        [TestCase("((x, y) => x * y)(4, 2)", ExpectedResult = 8, Category = "Lambda Functions")]
        #endregion

        #region Standard Functions

        #region Abs Function
        [TestCase("Abs(-50)", ExpectedResult = 50, Category = "Standard Functions,Abs Function")]
        [TestCase("Abs(-19)", ExpectedResult = 19, Category = "Standard Functions,Abs Function")]
        [TestCase("Abs(-3.5)", ExpectedResult = 3.5, Category = "Standard Functions,Abs Function")]
        [TestCase("Abs(0)", ExpectedResult = 0, Category = "Standard Functions,Abs Function")]
        [TestCase("Abs(1)", ExpectedResult = 1, Category = "Standard Functions,Abs Function")]
        [TestCase("Abs(4.2)", ExpectedResult = 4.2, Category = "Standard Functions,Abs Function")]
        [TestCase("Abs(10)", ExpectedResult = 10, Category = "Standard Functions,Abs Function")]
        [TestCase("Abs(60)", ExpectedResult = 60, Category = "Standard Functions,Abs Function")]

        [TestCase("-30 + Abs(-30)", ExpectedResult = 0, Category = "Standard Functions,Abs Function")]
        [TestCase("-5.5 + Abs(-5.5)", ExpectedResult = 0, Category = "Standard Functions,Abs Function")]
        [TestCase("-1 + Abs(-1)", ExpectedResult = 0, Category = "Standard Functions,Abs Function")]
        [TestCase("0 + Abs(0)", ExpectedResult = 0, Category = "Standard Functions,Abs Function")]
        [TestCase("1 + Abs(1)", ExpectedResult = 2, Category = "Standard Functions,Abs Function")]
        [TestCase("5 + Abs(5)", ExpectedResult = 10, Category = "Standard Functions,Abs Function")]
        [TestCase("2.5 + Abs(2.5)", ExpectedResult = 5, Category = "Standard Functions,Abs Function")]

        [TestCase("Abs(-10 - 5)", ExpectedResult = 15, Category = "Standard Functions,Abs Function")]
        #endregion

        #region Acos Function
        [TestCase("Acos(-1)", ExpectedResult = Math.PI, Category = "Standard Functions,Acos Function")]
        [TestCase("Acos(0)", ExpectedResult = 1.5707963267948966d, Category = "Standard Functions,Acos Function")]
        [TestCase("Acos(0.5)", ExpectedResult = 1.0471975511965979d, Category = "Standard Functions,Acos Function")]
        [TestCase("Acos(1)", ExpectedResult = 0, Category = "Standard Functions,Acos Function")]
        [TestCase("Acos(2)", ExpectedResult = Double.NaN, Category = "Standard Functions,Acos Function")]
        #endregion

        #region Array Function
        [TestCase("Array(14, \"A text for test\", 2.5, true).Length", ExpectedResult = 4, Category = "Standard Functions,Array Function,Instance Property")]
        [TestCase("Array(14, \"A text for test\", 2.5, true)[0]", ExpectedResult = 14, Category = "Standard Functions,Array Function,Indexing")]
        [TestCase("Array(14, \"A text for test\", 2.5, true)[1]", ExpectedResult = "A text for test", Category = "Standard Functions,Array Function,Indexing")]
        [TestCase("Array(14, \"A text for test\", 2.5, true)[2]", ExpectedResult = 2.5, Category = "Standard Functions,Array Function,Indexing")]
        [TestCase("Array(14, \"A text for test\", 2.5, true)[3]", ExpectedResult = true, Category = "Standard Functions,Array Function,Indexing")]
        #endregion

        #region Asin Function
        [TestCase("Asin(-1)", ExpectedResult = -1.5707963267948966d, Category = "Standard Functions,Asin Function")]
        [TestCase("Asin(0)", ExpectedResult = 0, Category = "Standard Functions,Asin Function")]
        [TestCase("Asin(0.5)", ExpectedResult = 0.52359877559829893d, Category = "Standard Functions,Asin Function")]
        [TestCase("Asin(1)", ExpectedResult = 1.5707963267948966d, Category = "Standard Functions,Asin Function")]
        [TestCase("Asin(2)", ExpectedResult = Double.NaN, Category = "Standard Functions,Asin Function")]
        #endregion

        #region Atan Function
        [TestCase("Atan(-Pi)", ExpectedResult = -1.2626272556789118d, Category = "Standard Functions,Atan Function")]
        [TestCase("Atan(-1)", ExpectedResult = -0.78539816339744828d, Category = "Standard Functions,Atan Function")]
        [TestCase("Atan(0)", ExpectedResult = 0, Category = "Standard Functions,Atan Function")]
        [TestCase("Atan(0.5)", ExpectedResult = 0.46364760900080609d, Category = "Standard Functions,Atan Function")]
        [TestCase("Atan(1)", ExpectedResult = 0.78539816339744828d, Category = "Standard Functions,Atan Function")]
        [TestCase("Atan(2)", ExpectedResult = 1.1071487177940904d, Category = "Standard Functions,Atan Function")]
        [TestCase("Atan(Pi)", ExpectedResult = 1.2626272556789118d, Category = "Standard Functions,Atan Function")]
        #endregion

        #region Atan2 Function
        [TestCase("Atan2(2d, 3d)", ExpectedResult = 0.5880026035475675d, Category = "Standard Functions,Atan2 Function")]
        [TestCase("Atan2(-1d, 2d)", ExpectedResult = -0.46364760900080609d, Category = "Standard Functions,Atan2 Function")]
        [TestCase("Atan2(0d, 0.5)", ExpectedResult = 0, Category = "Standard Functions,Atan2 Function")]
        [TestCase("Atan2(0.5, 2d)", ExpectedResult = 0.24497866312686414d, Category = "Standard Functions,Atan2 Function")]
        [TestCase("Atan2(1, 1)", ExpectedResult = 0.78539816339744828d, Category = "Standard Functions,Atan2 Function")]
        [TestCase("Atan2(Pi, 1d)", ExpectedResult = 1.2626272556789118d, Category = "Standard Functions,Atan2 Function")]
        #endregion

        #region Avg Function
        [TestCase("Avg(2d)", ExpectedResult = 2d, Category = "Standard Functions,Avg Function")]
        [TestCase("Avg(2d,3d)", ExpectedResult = 2.5, Category = "Standard Functions,Avg Function")]
        [TestCase("Avg(2d,3d, 6.5)", ExpectedResult = 3.8333333333333335d, Category = "Standard Functions,Avg Function")]
        [TestCase("Avg(10d,-10d)", ExpectedResult = 0d, Category = "Standard Functions,Avg Function")]
        [TestCase("Avg(10d,-10d, 10d, -10d, 10d)", ExpectedResult = 2d, Category = "Standard Functions,Avg Function")]
        #endregion

        #region Ceiling Function
        [TestCase("Ceiling(2d)", ExpectedResult = 2d, Category = "Standard Functions,Ceiling Function")]
        [TestCase("Ceiling(2.5d)", ExpectedResult = 3d, Category = "Standard Functions,Ceiling Function")]
        [TestCase("Ceiling(35.432638d)", ExpectedResult = 36d, Category = "Standard Functions,Ceiling Function")]
        [TestCase("Ceiling(-2d)", ExpectedResult = -2d, Category = "Standard Functions,Ceiling Function")]
        [TestCase("Ceiling(-2.5d)", ExpectedResult = -2d, Category = "Standard Functions,Ceiling Function")]
        [TestCase("Ceiling(-35.432638d)", ExpectedResult = -35d, Category = "Standard Functions,Ceiling Function")]
        #endregion

        #region Cos Function
        [TestCase("Cos(0d)", ExpectedResult = 1d, Category = "Standard Functions,Cos Function")]
        [TestCase("Cos(Pi)", ExpectedResult = -1d, Category = "Standard Functions,Cos Function")]
        [TestCase("Cos(2 * Pi)", ExpectedResult = 1d, Category = "Standard Functions,Cos Function")]
        [TestCase("Cos(3 * Pi)", ExpectedResult = -1d, Category = "Standard Functions,Cos Function")]
        [TestCase("Round(Cos(Pi / 3d), 1)", ExpectedResult = 0.5d, Category = "Standard Functions,Cos Function,Round Function")]
        [TestCase("Round(Cos(Pi / 2d), 2)", ExpectedResult = 0d, Category = "Standard Functions,Cos Function,Round Function")]
        [TestCase("Cos(4.8)", ExpectedResult = 0.087498983439446398d, Category = "Standard Functions,Cos Function")]
        #endregion

        #region Cosh Function
        [TestCase("Cosh(0d)", ExpectedResult = 1d, Category = "Standard Functions,Cosh Function")]
        [TestCase("Cosh(1d)", ExpectedResult = 1.5430806348152437d, Category = "Standard Functions,Cosh Function")]
        [TestCase("Cosh(Pi)", ExpectedResult = 11.591953275521519d, Category = "Standard Functions,Cosh Function")]
        #endregion

        #region Exp Function
        [TestCase("Exp(-10d)", ExpectedResult = 4.5399929762484854E-05d, Category = "Standard Functions,Exp Function")]
        [TestCase("Exp(0d)", ExpectedResult = 1d, Category = "Standard Functions,Exp Function")]
        [TestCase("Exp(1d)", ExpectedResult = 2.7182818284590451d, Category = "Standard Functions,Exp Function")]
        [TestCase("Exp(20d)", ExpectedResult = 485165195.40979028d, Category = "Standard Functions,Exp Function")]
        #endregion

        #region Floor Function
        [TestCase("Floor(2d)", ExpectedResult = 2d, Category = "Standard Functions,Floor Function")]
        [TestCase("Floor(2.5d)", ExpectedResult = 2d, Category = "Standard Functions,Floor Function")]
        [TestCase("Floor(35.432638d)", ExpectedResult = 35d, Category = "Standard Functions,Floor Function")]
        [TestCase("Floor(-2d)", ExpectedResult = -2d, Category = "Standard Functions,Floor Function")]
        [TestCase("Floor(-2.5d)", ExpectedResult = -3d, Category = "Standard Functions,Floor Function")]
        [TestCase("Floor(-35.432638d)", ExpectedResult = -36d, Category = "Standard Functions,Floor Function")]
        #endregion

        #region IEEERemainder Function
        [TestCase("IEEERemainder(-4, 2)", ExpectedResult = 0, Category = "Standard Functions,IEEERemainder Function")]
        [TestCase("IEEERemainder(-3, 2)", ExpectedResult = 1, Category = "Standard Functions,IEEERemainder Function")]
        [TestCase("IEEERemainder(-2, 2)", ExpectedResult = 0, Category = "Standard Functions,IEEERemainder Function")]
        [TestCase("IEEERemainder(-1, 2)", ExpectedResult = -1, Category = "Standard Functions,IEEERemainder Function")]
        [TestCase("IEEERemainder(0, 2)", ExpectedResult = 0, Category = "Standard Functions,IEEERemainder Function")]
        [TestCase("IEEERemainder(1, 2)", ExpectedResult = 1, Category = "Standard Functions,IEEERemainder Function")]
        [TestCase("IEEERemainder(2, 2)", ExpectedResult = 0, Category = "Standard Functions,IEEERemainder Function")]
        [TestCase("IEEERemainder(3, 2)", ExpectedResult = -1, Category = "Standard Functions,IEEERemainder Function")]
        [TestCase("IEEERemainder(4, 2)", ExpectedResult = 0, Category = "Standard Functions,IEEERemainder Function")]
        [TestCase("IEEERemainder(5, 2)", ExpectedResult = 1, Category = "Standard Functions,IEEERemainder Function")]
        [TestCase("IEEERemainder(6, 2)", ExpectedResult = 0, Category = "Standard Functions,IEEERemainder Function")]
        [TestCase("IEEERemainder(7, 2)", ExpectedResult = -1, Category = "Standard Functions,IEEERemainder Function")]
        [TestCase("IEEERemainder(8, 2)", ExpectedResult = 0, Category = "Standard Functions,IEEERemainder Function")]

        [TestCase("IEEERemainder(-6, 3)", ExpectedResult = 0, Category = "Standard Functions,IEEERemainder Function")]
        [TestCase("IEEERemainder(-5, 3)", ExpectedResult = 1, Category = "Standard Functions,IEEERemainder Function")]
        [TestCase("IEEERemainder(-4, 3)", ExpectedResult = -1, Category = "Standard Functions,IEEERemainder Function")]
        [TestCase("IEEERemainder(-3, 3)", ExpectedResult = 0, Category = "Standard Functions,IEEERemainder Function")]
        [TestCase("IEEERemainder(-2, 3)", ExpectedResult = 1, Category = "Standard Functions,IEEERemainder Function")]
        [TestCase("IEEERemainder(-1, 3)", ExpectedResult = -1, Category = "Standard Functions,IEEERemainder Function")]
        [TestCase("IEEERemainder(0, 3)", ExpectedResult = 0, Category = "Standard Functions,IEEERemainder Function")]
        [TestCase("IEEERemainder(1, 3)", ExpectedResult = 1, Category = "Standard Functions,IEEERemainder Function")]
        [TestCase("IEEERemainder(2, 3)", ExpectedResult = -1, Category = "Standard Functions,IEEERemainder Function")]
        [TestCase("IEEERemainder(3, 3)", ExpectedResult = 0, Category = "Standard Functions,IEEERemainder Function")]
        [TestCase("IEEERemainder(4, 3)", ExpectedResult = 1, Category = "Standard Functions,IEEERemainder Function")]
        [TestCase("IEEERemainder(5, 3)", ExpectedResult = -1, Category = "Standard Functions,IEEERemainder Function")]
        [TestCase("IEEERemainder(6, 3)", ExpectedResult = 0, Category = "Standard Functions,IEEERemainder Function")]
        #endregion

        #region in Function
        [TestCase("in(8, 4, 2, 8)", ExpectedResult = true, Category = "Standard Functions,in Function")]
        [TestCase("in(20, 4, 2, 8)", ExpectedResult = false, Category = "Standard Functions,in Function")]
        #endregion

        #region List Function
        [TestCase("List(14, \"A text for test\", 2.5, true).Count", ExpectedResult = 4, Category = "Standard Functions,List Function,Instance Property")]
        [TestCase("List(14, \"A text for test\", 2.5, true)[0]", ExpectedResult = 14, Category = "Standard Functions,List Function,Indexing")]
        [TestCase("List(14, \"A text for test\", 2.5, true)[1]", ExpectedResult = "A text for test", Category = "Standard Functions,List Function,Indexing")]
        [TestCase("List(14, \"A text for test\", 2.5, true)[2]", ExpectedResult = 2.5, Category = "Standard Functions,List Function,Indexing")]
        [TestCase("List(14, \"A text for test\", 2.5, true)[3]", ExpectedResult = true, Category = "Standard Functions,List Function,Indexing")]
        #endregion

        #region ListOfType Function
        [TestCase("ListOfType(typeof(int), 1,2,3 ).GetType()", ExpectedResult = typeof(List<int>), Category = "Standard Functions,ListOfType Function,Instance Property")]
        [TestCase("ListOfType(typeof(int), 1,2,3 ).Count", ExpectedResult = 3, Category = "Standard Functions,ListOfType Function,Instance Property")]
        [TestCase("ListOfType(typeof(int), 1,2,3 )[0]", ExpectedResult = 1, Category = "Standard Functions,ListOfType Function,Indexing")]
        [TestCase("ListOfType(typeof(int), 1,2,3 )[1]", ExpectedResult = 2, Category = "Standard Functions,ListOfType Function,Indexing")]
        [TestCase("ListOfType(typeof(int), 1,2,3 )[2]", ExpectedResult = 3, Category = "Standard Functions,ListOfType Function,Indexing")]
        [TestCase("ListOfType(typeof(string), \"hello\",\"Test\").GetType()", ExpectedResult = typeof(List<string>), Category = "Standard Functions,ListOfType Function,Instance Property")]
        [TestCase("ListOfType(typeof(string), \"hello\",\"Test\").Count", ExpectedResult = 2, Category = "Standard Functions,ListOfType Function,Instance Property")]
        [TestCase("ListOfType(typeof(string), \"hello\",\"Test\")[0]", ExpectedResult = "hello", Category = "Standard Functions,ListOfType Function,Indexing")]
        [TestCase("ListOfType(typeof(string), \"hello\",\"Test\")[1]", ExpectedResult = "Test", Category = "Standard Functions,ListOfType Function,Indexing")]
        #endregion

        #region Log Function
        [TestCase("Log(64d, 2d)", ExpectedResult = 6, Category = "Standard Functions,Log Function")]
        [TestCase("Log(100d, 10d)", ExpectedResult = 2, Category = "Standard Functions,Log Function")]
        #endregion

        #region Log10 Function
        [TestCase("Log10(64d)", ExpectedResult = 1.8061799739838871d, Category = "Standard Functions,Log10 Function")]
        [TestCase("Log10(100d)", ExpectedResult = 2, Category = "Standard Functions,Log10 Function")]
        [TestCase("Log10(1000d)", ExpectedResult = 3, Category = "Standard Functions,Log10 Function")]
        #endregion

        #region Max Function
        [TestCase("Max(-2)", ExpectedResult = -2, Category = "Standard Functions,Max Function")]
        [TestCase("Max(0)", ExpectedResult = 0, Category = "Standard Functions,Max Function")]
        [TestCase("Max(4)", ExpectedResult = 4, Category = "Standard Functions,Max Function")]
        [TestCase("Max(5.5)", ExpectedResult = 5.5, Category = "Standard Functions,Max Function")]

        [TestCase("Max(-2, 2)", ExpectedResult = 2, Category = "Standard Functions,Max Function")]
        [TestCase("Max(0, 2)", ExpectedResult = 2, Category = "Standard Functions,Max Function")]
        [TestCase("Max(1, 2)", ExpectedResult = 2, Category = "Standard Functions,Max Function")]
        [TestCase("Max(2, 2)", ExpectedResult = 2, Category = "Standard Functions,Max Function")]
        [TestCase("Max(3, 2)", ExpectedResult = 3, Category = "Standard Functions,Max Function")]

        [TestCase("Max(-7, 2, 4, 6)", ExpectedResult = 6, Category = "Standard Functions,Max Function")]
        [TestCase("Max(-6, 2, 4, 6)", ExpectedResult = 6, Category = "Standard Functions,Max Function")]
        [TestCase("Max(-0, 2, 4, 6)", ExpectedResult = 6, Category = "Standard Functions,Max Function")]
        [TestCase("Max(4, 2, 8, 6)", ExpectedResult = 8, Category = "Standard Functions,Max Function")]
        [TestCase("Max(6.2, 10.6, 4.1, 6)", ExpectedResult = 10.6, Category = "Standard Functions,Max Function")]
        #endregion

        #region Min Function
        [TestCase("Min(-2)", ExpectedResult = -2, Category = "Standard Functions,Min Function")]
        [TestCase("Min(0)", ExpectedResult = 0, Category = "Standard Functions,Min Function")]
        [TestCase("Min(4)", ExpectedResult = 4, Category = "Standard Functions,Min Function")]
        [TestCase("Min(5.5)", ExpectedResult = 5.5, Category = "Standard Functions,Min Function")]

        [TestCase("Min(-2, 2)", ExpectedResult = -2, Category = "Standard Functions,Min Function")]
        [TestCase("Min(0, 2)", ExpectedResult = 0, Category = "Standard Functions,Min Function")]
        [TestCase("Min(1, 2)", ExpectedResult = 1, Category = "Standard Functions,Min Function")]
        [TestCase("Min(2, 2)", ExpectedResult = 2, Category = "Standard Functions,Min Function")]
        [TestCase("Min(3, 2)", ExpectedResult = 2, Category = "Standard Functions,Min Function")]

        [TestCase("Min(-7, 2, 4, 6)", ExpectedResult = -7, Category = "Standard Functions,Min Function")]
        [TestCase("Min(-6, 2, 4, 6)", ExpectedResult = -6, Category = "Standard Functions,Min Function")]
        [TestCase("Min(0, 2, 4, 6)", ExpectedResult = 0, Category = "Standard Functions,Min Function")]
        [TestCase("Min(4, 2, 8, 6)", ExpectedResult = 2, Category = "Standard Functions,Min Function")]
        [TestCase("Min(6.2, 10.6, 4.1, 6)", ExpectedResult = 4.1, Category = "Standard Functions,Min Function")]
        #endregion

        #region new Function
        [TestCase("new(ClassForTest1).GetType()", ExpectedResult = typeof(ClassForTest1), Category = "Standard Functions,new Function")]
        [TestCase("new(ClassForTest2, 15).GetType()", ExpectedResult = typeof(ClassForTest2), Category = "Standard Functions,new Function")]
        [TestCase("new(ClassForTest2, 15).Value1", ExpectedResult = 15, Category = "Standard Functions,new Function")]
        #endregion

        #region Pow Function
        [TestCase("Pow(2, 4)", ExpectedResult = 16, Category = "Standard Functions,Pow Function")]
        [TestCase("Pow(10, 2)", ExpectedResult = 100, Category = "Standard Functions,Pow Function")]
        [TestCase("Pow(2, -2)", ExpectedResult = 0.25, Category = "Standard Functions,Pow Function")]
        [TestCase("Pow(-2, 2)", ExpectedResult = 4, Category = "Standard Functions,Pow Function")]
        [TestCase("Pow(-2, 3)", ExpectedResult = -8, Category = "Standard Functions,Pow Function")]
        [TestCase("Pow(0, 3)", ExpectedResult = 0, Category = "Standard Functions,Pow Function")]
        [TestCase("Pow(3, 0)", ExpectedResult = 1, Category = "Standard Functions,Pow Function")]
        [TestCase("Pow(4, 0.5)", ExpectedResult = 2, Category = "Standard Functions,Pow Function")]
        [TestCase("Pow(1.5, 4)", ExpectedResult = 5.0625d, Category = "Standard Functions,Pow Function")]
        #endregion

        #region Round Function
        [TestCase("Round(1.5)", ExpectedResult = 2, Category = "Standard Functions,Round Function")]
        [TestCase("Round(1.6)", ExpectedResult = 2, Category = "Standard Functions,Round Function")]
        [TestCase("Round(1.4)", ExpectedResult = 1, Category = "Standard Functions,Round Function")]
        [TestCase("Round(-0.3)", ExpectedResult = 0, Category = "Standard Functions,Round Function")]
        [TestCase("Round(-1.5)", ExpectedResult = -2, Category = "Standard Functions,Round Function")]
        [TestCase("Round(4)", ExpectedResult = 4, Category = "Standard Functions,Round Function")]
        [TestCase("Round(Pi, 2)", ExpectedResult = 3.14, Category = "Standard Functions,Round Function")]
        [TestCase("Round(2.5, MidpointRounding.AwayFromZero)", ExpectedResult = 3, Category = "Standard Functions,Round Function")]
        [TestCase("Round(2.5, MidpointRounding.ToEven)", ExpectedResult = 2, Category = "Standard Functions,Round Function")]
        [TestCase("Round(2.25, 1, MidpointRounding.AwayFromZero)", ExpectedResult = 2.3, Category = "Standard Functions,Round Function")]
        [TestCase("Round(2.25, 1, MidpointRounding.ToEven)", ExpectedResult = 2.2, Category = "Standard Functions,Round Function")]
        #endregion

        #region Sign Function
        [TestCase("Sign(-12)", ExpectedResult = -1, Category = "Standard Functions,Sign Function")]
        [TestCase("Sign(-3.7)", ExpectedResult = -1, Category = "Standard Functions,Sign Function")]
        [TestCase("Sign(0)", ExpectedResult = 0, Category = "Standard Functions,Sign Function")]
        [TestCase("Sign(2.7)", ExpectedResult = 1, Category = "Standard Functions,Sign Function")]
        [TestCase("Sign(60)", ExpectedResult = 1, Category = "Standard Functions,Sign Function")]
        #endregion

        #region Sin Function
        [TestCase("Sin(0d)", ExpectedResult = 0, Category = "Standard Functions,Sin Function")]
        [TestCase("Round(Sin(Pi), 2)", ExpectedResult = 0, Category = "Standard Functions,Sin Function,Round Function")]
        [TestCase("Round(Sin(Pi / 2),2)", ExpectedResult = 1, Category = "Standard Functions,Sin Function,Round Function")]
        [TestCase("Round(Sin(Pi / 6),2)", ExpectedResult = 0.5, Category = "Standard Functions,Sin Function,Round Function")]
        [TestCase("Sin(2)", ExpectedResult = 0.90929742682568171d, Category = "Standard Functions,Sin Function")]
        #endregion

        #region Sinh Function
        [TestCase("Sinh(0d)", ExpectedResult = 0, Category = "Standard Functions,Sinh Function")]
        [TestCase("Round(Sinh(Pi), 2)", ExpectedResult = 11.55, Category = "Standard Functions,Sinh Function,Round Function")]
        [TestCase("Round(Sinh(Pi / 2),2)", ExpectedResult = 2.3, Category = "Standard Functions,Sinh Function,Round Function")]
        [TestCase("Round(Sinh(Pi / 6),2)", ExpectedResult = 0.55, Category = "Standard Functions,Sinh Function,Round Function")]
        [TestCase("Sinh(2)", ExpectedResult = 3.6268604078470186d, Category = "Standard Functions,Sinh Function")]
        #endregion

        #region Sqrt Function
        [TestCase("Sqrt(0d)", ExpectedResult = 0, Category = "Standard Functions,Sqrt Function")]
        [TestCase("Sqrt(-2)", ExpectedResult = Double.NaN, Category = "Standard Functions,Sqrt Function")]
        [TestCase("Sqrt(4)", ExpectedResult = 2, Category = "Standard Functions,Sqrt Function")]
        [TestCase("Sqrt(9)", ExpectedResult = 3, Category = "Standard Functions,Sqrt Function")]
        [TestCase("Sqrt(18)", ExpectedResult = 4.2426406871192848d, Category = "Standard Functions,Sqrt Function")]
        [TestCase("Sqrt(0.25)", ExpectedResult = 0.5, Category = "Standard Functions,Sqrt Function")]
        [TestCase("Sqrt(100)", ExpectedResult = 10, Category = "Standard Functions,Sqrt Function")]
        #endregion

        #region Tan Function
        [TestCase("Tan(0d)", ExpectedResult = 0, Category = "Standard Functions,Tan Function")]
        [TestCase("Round(Tan(Pi / 4), 2)", ExpectedResult = 1, Category = "Standard Functions,Tan Function")]
        [TestCase("Round(Tan(Pi / 3) ,2)", ExpectedResult = 1.73, Category = "Standard Functions,Tan Function,Round Function")]
        [TestCase("Round(Tan(Pi / 6) ,2)", ExpectedResult = 0.58, Category = "Standard Functions,Tan Function,Round Function")]
        [TestCase("Round(Tan(2),2)", ExpectedResult = -2.19, Category = "Standard Functions,Tan Function,Round Function")]
        #endregion

        #region Tanh Function
        [TestCase("Tanh(0d)", ExpectedResult = 0, Category = "Standard Functions,Tanh Function")]
        [TestCase("Round(Tanh(Pi / 4), 2)", ExpectedResult = 0.66, Category = "Standard Functions,Tanh Function")]
        [TestCase("Round(Tanh(Pi / 3) ,2)", ExpectedResult = 0.78, Category = "Standard Functions,Tanh Function,Round Function")]
        [TestCase("Round(Tanh(Pi / 6) ,2)", ExpectedResult = 0.48, Category = "Standard Functions,Tanh Function,Round Function")]
        [TestCase("Round(Tanh(2),2)", ExpectedResult = 0.96, Category = "Standard Functions,Tanh Function,Round Function")]
        #endregion

        #region Truncate Function
        [TestCase("Truncate(0d)", ExpectedResult = 0, Category = "Standard Functions,Truncate Function")]
        [TestCase("Truncate(-1)", ExpectedResult = -1, Category = "Standard Functions,Truncate Function")]
        [TestCase("Truncate(-2)", ExpectedResult = -2, Category = "Standard Functions,Truncate Function")]
        [TestCase("Truncate(-23)", ExpectedResult = -23, Category = "Standard Functions,Truncate Function")]
        [TestCase("Truncate(-0.5)", ExpectedResult = 0, Category = "Standard Functions,Truncate Function")]
        [TestCase("Truncate(-0.6)", ExpectedResult = 0, Category = "Standard Functions,Truncate Function")]
        [TestCase("Truncate(-0.4)", ExpectedResult = 0, Category = "Standard Functions,Truncate Function")]
        [TestCase("Truncate(0.5)", ExpectedResult = 0, Category = "Standard Functions,Truncate Function")]
        [TestCase("Truncate(0.6)", ExpectedResult = 0, Category = "Standard Functions,Truncate Function")]
        [TestCase("Truncate(0.4)", ExpectedResult = 0, Category = "Standard Functions,Truncate Function")]
        [TestCase("Truncate(1)", ExpectedResult = 1, Category = "Standard Functions,Truncate Function")]
        [TestCase("Truncate(2)", ExpectedResult = 2, Category = "Standard Functions,Truncate Function")]
        [TestCase("Truncate(23)", ExpectedResult = 23, Category = "Standard Functions,Truncate Function")]
        [TestCase("Truncate(213.4719468971)", ExpectedResult = 213, Category = "Standard Functions,Truncate Function")]

        #endregion

        #endregion

        #region Generic types Management

        [TestCase("List(\"Hello\", \"Test\").Cast<string>().ToList<string>().GetType()", ExpectedResult = typeof(List<string>), Category = "List function, Generics")]
        [TestCase("new List<string>().GetType()", ExpectedResult = typeof(List<string>), Category = "new Keyword, Generics")]
        [TestCase("new Dictionary<string,List<int>>().GetType()", ExpectedResult = typeof(Dictionary<string, List<int>>), Category = "new Keyword, Generics")]

        // Linq and Types inference
        [TestCase("new List<int>() { 1, 2, 3, 4 }.Where<int>(x => x > 2).Json", ExpectedResult = "[3,4]", Category = "Linq, Lambda, new Keyword, Generics")]
        [TestCase("new List<int>() { 1, 2, 3, 4 }.Where(x => x > 2).Json", ExpectedResult = "[3,4]", Category = "Linq, Type inference, Lambda, new Keyword, Generics")]

        #endregion

        #region Complex expressions
        [TestCase("Enumerable.Range(1,4).Cast().Sum(x =>(int)x)", ExpectedResult = 10, Category = "Complex expression,Static method,Instance method,Lambda function,Cast")]
        [TestCase("System.Linq.Enumerable.Range(1,4).Cast().Sum(x =>(int)x)", ExpectedResult = 10, Category = "Complex expression,Static method,Instance method,Lambda function,Cast")]
        [TestCase("List(1,2,3,4,5,6).ConvertAll(x => (float)x)[2].GetType()", ExpectedResult = typeof(float), Category = "Complex expression,Type Manage,Instance method,Lambda function,Cast,Indexing")]
        [TestCase("List(\"hello\", \"bye\").Select(x => x.ToUpper()).ToList().FluidAdd(\"test\").Count", ExpectedResult = 3, Category = "Complex expression,Fluid Functions")]
        [TestCase("List(\"hello\", \"bye\").Select(x => x.ToUpper()).ToList().FluidAdd(\"test\")[0]", ExpectedResult = "HELLO", Category = "Complex expression,Fluid Functions")]
        [TestCase("List(\"hello\", \"bye\").Select(x => x.ToUpper()).ToList().FluidAdd(\"test\")[1]", ExpectedResult = "BYE", Category = "Complex expression,Fluid Functions")]
        [TestCase("List(\"hello\", \"bye\").Select(x => x.ToUpper()).ToList().FluidAdd(\"test\")[2]", ExpectedResult = "test", Category = "Complex expression,Fluid Functions")]
        [TestCase("List(\"hello\", \"bye\").Select(x => x.ToUpper()).ToList().FluidAdd(\"test\")[2]", ExpectedResult = "test", Category = "Complex expression,Fluid Functions")]
        [TestCase("$\"https://www.google.com/search?q={System.Net.WebUtility.UrlEncode(\"test of request with url encode() ?\")}\"", ExpectedResult = "https://www.google.com/search?q=test+of+request+with+url+encode()+%3F", Category = "Complex expression,Inline namespace")]
        [TestCase("new System.Xml.XmlDocument().FluidLoadXml(\"<root><element id='MyElement'>Xml Content</element></root>\").SelectSingleNode(\"//element[@id='MyElement']\").InnerXml", ExpectedResult = "Xml Content", Category = "Complex expression,Inline namespace,Fluid")]
        [TestCase("new System.Xml.XmlDocument().FluidLoadXml(\"<root><element id='MyElement'>Xml Content</element></root>\").ChildNodes[0].Name", ExpectedResult = "root", Category = "Complex expression,Inline namespace,Fluid,Custom Indexer")]
        [TestCase("string.Join(\" - \", new string[]{\"Hello\", \"Bye\", \"Other\"})", ExpectedResult = "Hello - Bye - Other", Category = "Complex expression, Different brackets imbrication")]
        [TestCase("new{Hello = \"Hye\"}.Hello", ExpectedResult = "Hye", Category = "Complex expression, new without space but not a word")]

        #endregion

        #region Bugs correction

        [TestCase("new DateTime(1985,9,11).ToString(\"dd.MM.yyyy\")", ExpectedResult = "11.09.1985", Category = "Complex expression,Static method,Instance method,Lambda function,Cast")]
        
        [TestCase("(int)3.6", ExpectedResult = (int)3.6, Category = "Complex expression,Cast,double to int,#130")]
        [TestCase("(int)-3.6", ExpectedResult = (int)-3.6, Category = "Complex expression,Cast,double to int,#130")]
        [TestCase("(uint)3.6", ExpectedResult = (uint)3.6, Category = "Complex expression,Cast,double to uint,#130")]
        [TestCase("(long)3.6", ExpectedResult = (long)3.6, Category = "Complex expression,Cast,double to long,#130")]
        [TestCase("(short)3.6", ExpectedResult = (short)3.6, Category = "Complex expression,Cast,double to short,#130")]

        [TestCase("(int)3.6d", ExpectedResult = (int)3.6d, Category = "Complex expression,Cast,double to int,#130")]
        [TestCase("(int)-3.6d", ExpectedResult = (int)-3.6d, Category = "Complex expression,Cast,double to int,#130")]
        [TestCase("(uint)3.6d", ExpectedResult = (uint)3.6d, Category = "Complex expression,Cast,double to uint,#130")]
        [TestCase("(long)3.6d", ExpectedResult = (long)3.6d, Category = "Complex expression,Cast,double to long,#130")]
        [TestCase("(short)3.6d", ExpectedResult = (short)3.6d, Category = "Complex expression,Cast,double to short,#130")]

        [TestCase("(int)3.6f", ExpectedResult = (int)3.6f, Category = "Complex expression,Cast,float to int,#130")]
        [TestCase("(int)-3.6f", ExpectedResult = (int)-3.6f, Category = "Complex expression,Cast,float to int,#130")]
        [TestCase("(uint)3.6f", ExpectedResult = (uint)3.6f, Category = "Complex expression,Cast,float to uint,#130")]
        [TestCase("(long)3.6f", ExpectedResult = (long)3.6f, Category = "Complex expression,Cast,float to long,#130")]
        [TestCase("(short)3.6f", ExpectedResult = (short)3.6f, Category = "Complex expression,Cast,float to short,#130")]

        [TestCase("(int)3.6m", ExpectedResult = (int)3.6m, Category = "Complex expression,Cast,decimal to int,#130")]
        [TestCase("(int)-3.6m", ExpectedResult = (int)-3.6m, Category = "Complex expression,Cast,decimal to int,#130")]
        [TestCase("(uint)3.6m", ExpectedResult = (uint)3.6m, Category = "Complex expression,Cast,decimal to uint,#130")]
        [TestCase("(long)3.6m", ExpectedResult = (long)3.6m, Category = "Complex expression,Cast,decimal to long,#130")]
        [TestCase("(short)3.6m", ExpectedResult = (short)3.6m, Category = "Complex expression,Cast,decimal to short,#130")]
        
        #endregion

        #endregion
        public object DirectExpressionEvaluation(string expression)
        {
            ExpressionEvaluator evaluator = new ExpressionEvaluator();

            evaluator.EvaluateVariable += Evaluator_EvaluateVariable;

            evaluator.Namespaces.Add("CodingSeb.ExpressionEvaluator.Tests");

            object result = evaluator.Evaluate(expression);

            evaluator.EvaluateVariable -= Evaluator_EvaluateVariable;

            return result;
        }

        #endregion

        #region With Custom Variables Expression Evaluation

        #region Test cases source for With Custom Variables Expression Evaluation
        public static IEnumerable<TestCaseData> TestCasesForWithCustomVariablesExpressionEvaluation
        {
            get
            {
                #region SimpleVariablesInjection

                Dictionary<string, object> variablesForSimpleVariablesInjection = new Dictionary<string, object>()
                {
                    { "hello", "Test" },
                    { "a", 0 },
                    { "x", 5 },
                    { "y", 20 },
                    { "isThisReal", true },
                };

                yield return new TestCaseData("hello", variablesForSimpleVariablesInjection, true).SetCategory("SimpleVariablesInjection").Returns("Test");
                yield return new TestCaseData("x", variablesForSimpleVariablesInjection, true).SetCategory("SimpleVariablesInjection").Returns(5);
                yield return new TestCaseData("isThisReal", variablesForSimpleVariablesInjection, true).SetCategory("SimpleVariablesInjection").Returns(true);

                yield return new TestCaseData("+x", variablesForSimpleVariablesInjection, true).SetCategory("SimpleVariablesInjection,Unary +").Returns(5);
                yield return new TestCaseData("-5 + +x", variablesForSimpleVariablesInjection, true).SetCategory("SimpleVariablesInjection,Unary +").Returns(0);
                yield return new TestCaseData("5 + +x", variablesForSimpleVariablesInjection, true).SetCategory("SimpleVariablesInjection,Unary +").Returns(10);
                yield return new TestCaseData("5 - +x", variablesForSimpleVariablesInjection, true).SetCategory("SimpleVariablesInjection,Unary +").Returns(0);
                yield return new TestCaseData("5-+x", variablesForSimpleVariablesInjection, true).SetCategory("SimpleVariablesInjection,Unary +").Returns(0);
                yield return new TestCaseData("-5 - +x", variablesForSimpleVariablesInjection, true).SetCategory("SimpleVariablesInjection,Unary +").Returns(-10);
                yield return new TestCaseData("-5-+x", variablesForSimpleVariablesInjection, true).SetCategory("SimpleVariablesInjection,Unary +").Returns(-10);
                yield return new TestCaseData("+y - +x", variablesForSimpleVariablesInjection, true).SetCategory("SimpleVariablesInjection,Unary +").Returns(15);
                yield return new TestCaseData("+y-+x", variablesForSimpleVariablesInjection, true).SetCategory("SimpleVariablesInjection,Unary +").Returns(15);
                yield return new TestCaseData("-x", variablesForSimpleVariablesInjection, true).SetCategory("SimpleVariablesInjection,Unary -").Returns(-5);
                yield return new TestCaseData("-5 + -x", variablesForSimpleVariablesInjection, true).SetCategory("SimpleVariablesInjection,Unary -").Returns(-10);
                yield return new TestCaseData("-5+-x", variablesForSimpleVariablesInjection, true).SetCategory("SimpleVariablesInjection,Unary -").Returns(-10);
                yield return new TestCaseData("5 + -x", variablesForSimpleVariablesInjection, true).SetCategory("SimpleVariablesInjection,Unary -").Returns(0);
                yield return new TestCaseData("5+-x", variablesForSimpleVariablesInjection, true).SetCategory("SimpleVariablesInjection,Unary -").Returns(0);
                yield return new TestCaseData("-5 - -x", variablesForSimpleVariablesInjection, true).SetCategory("SimpleVariablesInjection,Unary -").Returns(0);
                yield return new TestCaseData("5 - -x", variablesForSimpleVariablesInjection, true).SetCategory("SimpleVariablesInjection,Unary -").Returns(10);
                yield return new TestCaseData("-x - -y", variablesForSimpleVariablesInjection, true).SetCategory("SimpleVariablesInjection,Unary -").Returns(15);
                yield return new TestCaseData("+x - -y", variablesForSimpleVariablesInjection, true).SetCategory("SimpleVariablesInjection,Unary both +-").Returns(25);
                yield return new TestCaseData("+x + -y", variablesForSimpleVariablesInjection, true).SetCategory("SimpleVariablesInjection,Unary both +-").Returns(-15);
                yield return new TestCaseData("+x+-y", variablesForSimpleVariablesInjection, true).SetCategory("SimpleVariablesInjection,Unary both +-").Returns(-15);
                yield return new TestCaseData("-x - +y", variablesForSimpleVariablesInjection, true).SetCategory("SimpleVariablesInjection,Unary both +-").Returns(-25);
                yield return new TestCaseData("-x-+y", variablesForSimpleVariablesInjection, true).SetCategory("SimpleVariablesInjection,Unary both +-").Returns(-25);
                yield return new TestCaseData("-x + +y", variablesForSimpleVariablesInjection, true).SetCategory("SimpleVariablesInjection,Unary both +-").Returns(15);
                yield return new TestCaseData("(-x + +y)", variablesForSimpleVariablesInjection, true).SetCategory("SimpleVariablesInjection,Unary both +-,Parenthis").Returns(15);

                yield return new TestCaseData("-~a", variablesForSimpleVariablesInjection, true).SetCategory("SimpleVariablesInjection,MultipleUnary").Returns(1);
                yield return new TestCaseData("+-+-+-+-+a", variablesForSimpleVariablesInjection, true).SetCategory("SimpleVariablesInjection,MultipleUnary").Returns(0);
                yield return new TestCaseData("a >> +-+-+-+2 << +-+-+-+-2 >> +-+-+-+-+2 << +-+-+-+-+2", variablesForSimpleVariablesInjection, true).SetCategory("SimpleVariablesInjection,MultipleUnary").Returns(0);

                yield return new TestCaseData("ISTHISREAL", variablesForSimpleVariablesInjection, false).SetCategory("SimpleVariablesInjection,IgnoreCase").Returns(true).SetCategory("Options, OptionCaseSensitiveEvaluationActive");
                yield return new TestCaseData("isthisreal", variablesForSimpleVariablesInjection, false).SetCategory("SimpleVariablesInjection,IgnoreCase").Returns(true).SetCategory("Options, OptionCaseSensitiveEvaluationActive");
                yield return new TestCaseData("iStHISrEAL", variablesForSimpleVariablesInjection, false).SetCategory("SimpleVariablesInjection,IgnoreCase").Returns(true).SetCategory("Options, OptionCaseSensitiveEvaluationActive");
                yield return new TestCaseData("isThisReal.tostring()", variablesForSimpleVariablesInjection, false).SetCategory("SimpleVariablesInjection,IgnoreCase").Returns("True").SetCategory("Options, OptionCaseSensitiveEvaluationActive");
                yield return new TestCaseData("abs(-1)", variablesForSimpleVariablesInjection, false).SetCategory("SimpleVariablesInjection,IgnoreCase").Returns(1).SetCategory("Options, OptionCaseSensitiveEvaluationActive");

                #endregion

                #region StringWithSquareBracketInIndexing

                Dictionary<string, object> variablesForStringWithSquareBracketInIndexing = new Dictionary<string, object>()
                {
                    { "dictionary", new Dictionary<string, string>()
                        {
                            { "Test[", "Test1" },
                            { "Test]", "Test2" },
                            { "Test[]", "Test3" },
                        }
                    }
                };

                yield return new TestCaseData("dictionary[\"Test[\"]", variablesForStringWithSquareBracketInIndexing, true).SetCategory("StringWithSquareBracketInIndexing").Returns("Test1");
                yield return new TestCaseData("dictionary[\"Test]\"]", variablesForStringWithSquareBracketInIndexing, true).SetCategory("StringWithSquareBracketInIndexing").Returns("Test2");
                yield return new TestCaseData("dictionary[\"Test[]\"]", variablesForStringWithSquareBracketInIndexing, true).SetCategory("StringWithSquareBracketInIndexing").Returns("Test3");

                #endregion

                #region On Instance and static + complex expressions

                Dictionary<string, object> onInstanceVariables = new Dictionary<string, object>()
                {
                    { "simpleArray", new object[] {2 , "Hello", true} },
                    { "simpleArray2", new object[] {2 , " Hello  ", true, null } },
                    { "otherArray", new object[] {2 , "Hello", true, new ClassForTest1() { IntProperty = 18 } } },
                    { "simpleList", new List<object>() {"Test" ,false, -15, 123.8f} },
                    { "nullVar", null },
                    { "notTrimmedString", " Hello  " },
                    { "simpleInt", 42 },
                    { "simpleChar", 'n' },
                    { "simpleLineFeed", '\n' },
                    { "customObject", new ClassForTest1() }
                };

                yield return new TestCaseData("simpleArray.Length", onInstanceVariables, true).SetCategory("Instance Property").Returns(3);
                yield return new TestCaseData("simpleList.Count", onInstanceVariables, true).SetCategory("Instance Property").Returns(4);
                yield return new TestCaseData("simpleArray?.Length", onInstanceVariables, true).SetCategory("Instance Property,Null Conditional Property").Returns(3);
                yield return new TestCaseData("simpleList?.Count", onInstanceVariables, true).SetCategory("Instance Property,Null Conditional Property").Returns(4);
                yield return new TestCaseData("nullVar?.Length", onInstanceVariables, true).SetCategory("Instance Property,Null Conditional Property").Returns(null);
                yield return new TestCaseData("nullVar?.Count", onInstanceVariables, true).SetCategory("Instance Property,Null Conditional Property").Returns(null);
                yield return new TestCaseData("nullVar?.Trim()", onInstanceVariables, true).SetCategory("Instance Property,Null Conditional Method").Returns(null);
                yield return new TestCaseData("nullVar?.Trim().Length", onInstanceVariables, true).SetCategory("Instance Property,Null Conditional Method").Returns(null);
                yield return new TestCaseData("notTrimmedString?.Trim()", onInstanceVariables, true).SetCategory("Instance Property,Null Conditional Method").Returns("Hello");
                yield return new TestCaseData("notTrimmedString?.Trim().Length", onInstanceVariables, true).SetCategory("Instance Property,Null Conditional Method").Returns(5);
                yield return new TestCaseData("simpleArray?[2]", onInstanceVariables, true).SetCategory("Instance Property,Null Conditional indexing").Returns(true);
                yield return new TestCaseData("simpleList?[2]", onInstanceVariables, true).SetCategory("Instance Property,Null Conditional indexing").Returns(-15);
                yield return new TestCaseData("simpleArray2[1].Trim()", onInstanceVariables, true).SetCategory("Instance Property,Null Conditional indexing").Returns("Hello");
                yield return new TestCaseData("nullVar?[2]", onInstanceVariables, true).SetCategory("Instance Property,Null Conditional indexing").Returns(null);
                yield return new TestCaseData("nullVar?[1].Trim()", onInstanceVariables, true).SetCategory("Instance Property,Null Conditional indexing").Returns(null);
                yield return new TestCaseData("nullVar?[1]?.Trim()", onInstanceVariables, true).SetCategory("Instance Property,Null Conditional indexing").Returns(null);
                yield return new TestCaseData("nullVar?[1][3]", onInstanceVariables, true).SetCategory("Instance Property,Null Conditional indexing").Returns(null);
                yield return new TestCaseData("simpleArray2?[3]?.Trim()", onInstanceVariables, true).SetCategory("Instance Property,Null Conditional indexing").Returns(null);

                yield return new TestCaseData("customObject.IntProperty", onInstanceVariables, true).SetCategory("Instance Property").Returns(25);
                yield return new TestCaseData("customObject?.IntProperty", onInstanceVariables, true).SetCategory("Instance Property").Returns(25);
                yield return new TestCaseData("customObject.intField", onInstanceVariables, true).SetCategory("Instance Field").Returns(12);
                yield return new TestCaseData("customObject?.intField", onInstanceVariables, true).SetCategory("Instance Field").Returns(12);
                yield return new TestCaseData("customObject.Add3To(9)", onInstanceVariables, true).SetCategory("Instance Method").Returns(12);
                yield return new TestCaseData("customObject?.Add3To(5)", onInstanceVariables, true).SetCategory("Instance Method").Returns(8);

                yield return new TestCaseData("simpleInt.ToString()", onInstanceVariables, true).SetCategory("Instance Method").Returns("42");
                yield return new TestCaseData("simpleInt.ToString().Length", onInstanceVariables, true).SetCategory("Instance Method,Instance Property").Returns(2);

                yield return new TestCaseData("ClassForTest1.StaticIntProperty", onInstanceVariables, true).SetCategory("Static Property").Returns(67);
                yield return new TestCaseData("ClassForTest1.StaticStringMethod(\"Bob\")", onInstanceVariables, true).SetCategory("Static Method").Returns("Hello Bob");

                yield return new TestCaseData("simpleInt.GetType()", onInstanceVariables, true).SetCategory("Instance Method,Instance Property,Type Manage").Returns(typeof(int));
                yield return new TestCaseData("simpleInt.GetType().Name", onInstanceVariables, true).SetCategory("Instance Method,Instance Property,Type Manage").Returns("Int32");

                yield return new TestCaseData("simpleInt.GetType().Name", onInstanceVariables, true).SetCategory("Instance Method,Instance Property,Type Manage").Returns("Int32");

                yield return new TestCaseData("simpleList.Find(o => o is int)", onInstanceVariables, true).SetCategory("Lambda function,Instance method,is Operator").Returns(-15);

                yield return new TestCaseData("List(simpleArray[1], simpleList?[0], nullVar?[4] ?? \"Bye\", \"How are you ?\").Find(t => t.Length < 4)", onInstanceVariables, true).SetCategory("Complex expression,Lambda function,Instance method,Instance Property,Null Conditional indexing, Null Coalescing Operator").Returns("Bye");
                yield return new TestCaseData("int.Parse(Regex.Match(\"Test 34 Hello / -World\", @\"\\d+\").Value) + simpleArray.ToList().Find(val => val is int)", onInstanceVariables, true).SetCategory("Complex expression,Static Method,Lambda function").Returns(36);
                yield return new TestCaseData("otherArray[3].IntProperty", onInstanceVariables, true).SetCategory("Indexing,Instance Property").Returns(18);
                yield return new TestCaseData("otherArray[3].intField", onInstanceVariables, true).SetCategory("Indexing,Instance Field").Returns(12);
                yield return new TestCaseData("(() => simpleInt + 1)()", onInstanceVariables, true).SetCategory("Complex expression").Returns(43);

                yield return new TestCaseData("simpleInt++", onInstanceVariables, true).SetCategory("Postfix operator, ++").Returns(42);
                yield return new TestCaseData("simpleInt++ - simpleInt", onInstanceVariables, true).SetCategory("Postfix operator, ++").Returns(-1);
                yield return new TestCaseData("simpleInt--", onInstanceVariables, true).SetCategory("Postfix operator, --").Returns(42);
                yield return new TestCaseData("simpleInt-- - simpleInt", onInstanceVariables, true).SetCategory("Postfix operator, --").Returns(1);

                yield return new TestCaseData("false && 1/0>0", onInstanceVariables, true).SetCategory("Conditional And, negative left operand (should respect left associativity)").Returns(false);
                yield return new TestCaseData("true || 1/0>0", onInstanceVariables, true).SetCategory("Conditional Or, positive left operand (should respect left associativity)").Returns(true);
                yield return new TestCaseData("false && (true && 1/0>0)", onInstanceVariables, true).SetCategory("Conditional And, negative left operand (should respect left associativity)").Returns(false);
                yield return new TestCaseData("true || (false || 1/0>0)", onInstanceVariables, true).SetCategory("Conditional Or, positive left operand (should respect left associativity)").Returns(true);
                yield return new TestCaseData("false && 1/0 == 0", onInstanceVariables, true).SetCategory("Instance Property,And Conditional").Returns(false);
                yield return new TestCaseData("!string.IsNullOrEmpty(nullVar) && nullVar.StartsWith(\"ABC\")", onInstanceVariables, true).SetCategory("Instance Property,And Conditional").Returns(false);
                yield return new TestCaseData("string.IsNullOrEmpty(nullVar) || nullVar.StartsWith(\"ABC\")", onInstanceVariables, true).SetCategory("Instance Property,Or Conditional").Returns(true);
                yield return new TestCaseData("!string.IsNullOrEmpty(nullVar) && nullVar.StartsWith(\"ABC\") == false", onInstanceVariables, true).SetCategory("Instance Property,And Conditional").Returns(false);
                yield return new TestCaseData("string.IsNullOrEmpty(nullVar) || nullVar.StartsWith(\"ABC\") == false", onInstanceVariables, true).SetCategory("Instance Property,Or Conditional").Returns(true);
                yield return new TestCaseData("!string.IsNullOrEmpty(nullVar) && nullVar.Length < 2", onInstanceVariables, true).SetCategory("Instance Property,And Conditional").Returns(false);
                yield return new TestCaseData("string.IsNullOrEmpty(nullVar) || nullVar.Length < 2", onInstanceVariables, true).SetCategory("Instance Property,Or Conditional").Returns(true);
                yield return new TestCaseData("true || 1/0 == 0", onInstanceVariables, true).SetCategory("Instance Property,Or Conditional").Returns(true);
                yield return new TestCaseData("false && true || true", onInstanceVariables, true).SetCategory("Instance Property,Or Conditional,And Conditional,Precedence check").Returns(true);
                yield return new TestCaseData("true || true && false", onInstanceVariables, true).SetCategory("Instance Property,Or Conditional,And Conditional,Precedence check").Returns(true);
                yield return new TestCaseData("false && nullVar.What ? nullVar.Text : \"Hello\"", onInstanceVariables, true).SetCategory("Instance Property,Ternary operator, And Conditional").Returns("Hello");
                yield return new TestCaseData("false && (false && nullVar.What ? nullVar.boolValue : true) ? nullVar.Text : \"Hello\"", onInstanceVariables, true).SetCategory("Instance Property,Ternary operator, And Conditional").Returns("Hello");

                #endregion

                #region ExpandoObject

                dynamic MyDynamic = new System.Dynamic.ExpandoObject();
                MyDynamic.NullValue = null;
                MyDynamic.Number = 11;

                Dictionary<string, object> ExpandoObjectVariables = new Dictionary<string, object>()
                {
                    { "expObj", MyDynamic },
                };

                yield return new TestCaseData("expObj.Number", ExpandoObjectVariables, true).SetCategory("ExpandoObject").SetCategory("Instance Property").Returns(11);

                #region bug #67
                yield return new TestCaseData("expObj.NullValue", ExpandoObjectVariables, true).SetCategory("ExpandoObject").SetCategory("Instance Property").Returns(null);
                yield return new TestCaseData("expObj.NullValue ?? \"A\"", ExpandoObjectVariables, true).SetCategory("ExpandoObject").SetCategory("Instance Property").Returns("A");
                #endregion

                #endregion

                #region Delegates as a variable

                Dictionary<string, object> delegatesInVariable = new Dictionary<string, object>()
                {
                    { "Add", new Func<int,int,int>((x, y) => x + y)},
                    { "Test", new Action<int>(x => x.ShouldBe(5))},
                };

                yield return new TestCaseData("Add(3, 4)", delegatesInVariable, true).SetCategory("Delegate as a variable").Returns(7);
                yield return new TestCaseData("Test(5)", delegatesInVariable, true).SetCategory("Delegate as a variable").Returns(null);

                #endregion

                #region Delegates as Property of object

                yield return new TestCaseData("customObject.AddAsDelegate(6, 10)", onInstanceVariables, true).SetCategory("Delegate as a instance Property").Returns(16);
                yield return new TestCaseData("ClassForTest1.AddAsStaticDelegate(6, 10)", onInstanceVariables, true).SetCategory("Delegate as a static Property").Returns(16);

                #endregion
            }
        }

        #endregion

        [TestCaseSource(nameof(TestCasesForWithCustomVariablesExpressionEvaluation))]
        public object WithCustomVariablesExpressionEvaluation(string expression, Dictionary<string, object> variables, bool caseSensitiveEvaluation)
        {
            ExpressionEvaluator evaluator = new ExpressionEvaluator(variables)
            {
                OptionCaseSensitiveEvaluationActive = caseSensitiveEvaluation
            };

            evaluator.Namespaces.Add("CodingSeb.ExpressionEvaluator.Tests");

            return evaluator.Evaluate(expression);
        }

        #endregion

        #region On the fly evaluation tests

        #region Test cases for On the fly evaluation
        [TestCase("3.Add(2)", ExpectedResult = 5, Category = "On the fly method")]
        [TestCase("3.MultipliedBy2", ExpectedResult = 6, Category = "On the fly property")]
        [TestCase("myVar + 2", ExpectedResult = 10, Category = "On the fly variable")]
        [TestCase("SayHello(\"Bob\")", ExpectedResult = "Hello Bob", Category = "On the fly func")]
        #endregion
        public object OnTheFlyEvaluation(string expression)
        {
            ExpressionEvaluator evaluator = new ExpressionEvaluator();

            evaluator.EvaluateFunction += Evaluator_EvaluateFunction;

            evaluator.EvaluateVariable += Evaluator_EvaluateVariable;

            return evaluator.Evaluate(expression);
        }

        #region On the fly evaluation events handlers

        private void Evaluator_EvaluateFunction(object sender, FunctionEvaluationEventArg e)
        {
            if (e.Name.Equals("Add") && e.This is int intValue)
            {
                e.Value = intValue + (int)e.EvaluateArg(0);
            }
            else if (e.Name.Equals("SayHello") && e.Args.Count == 1)
            {
                e.Value = $"Hello {e.EvaluateArg(0)}";
            }
            else if (e.Name.Equals("GetSpecifiedGenericTypesFunc"))
            {
                e.Value = e.EvaluateGenericTypes();
            }
        }

        private void Evaluator_EvaluateVariable(object sender, VariableEvaluationEventArg e)
        {
            if (e.Name.Equals("MultipliedBy2") && e.This is int intValue)
            {
                e.Value = intValue * 2;
            }
            else if (e.Name.Equals("myVar"))
            {
                e.Value = 8;
            }
            else if (e.This != null && e.Name.Equals("Json"))
            {
                e.Value = JsonConvert.SerializeObject(e.This);
            }
            else if (e.Name.Equals("GetSpecifiedGenericTypesProp"))
            {
                e.Value = e.EvaluateGenericTypes();
            }
        }

        [TestCase("ClassForTest1.Add(1, 5)", ExpectedResult = 6)]
        [TestCase("ClassForTest1.Add(1, 5.0)", ExpectedResult = 6)]
        public object OnTheFlyCastEvaluation(string expression)
        {
            ExpressionEvaluator evaluator = new ExpressionEvaluator(new ContextObject1());

            evaluator.EvaluateParameterCast += Evaluator_EvaluateParameterCast;

            evaluator.Namespaces.Add("CodingSeb.ExpressionEvaluator.Tests");

            return evaluator.Evaluate(expression);
        }

        private void Evaluator_EvaluateParameterCast(object sender, ParameterCastEvaluationEventArg e)
        {
            if (e.ParameterType == typeof(ClassForTest2) && e.OriginalArg is int originalArgInt)
            {
                e.Argument = new ClassForTest2(originalArgInt);
            }

            if (e.ParameterType == typeof(ClassForTest2) && e.OriginalArg is double originalArgDouble)
            {
                e.Argument = new ClassForTest2((int) originalArgDouble);
            }
        }

        [TestCase("2[\"Test\"]", ExpectedResult = "Test,Test")]
        [TestCase("3[\"Hello\"]", ExpectedResult = "Hello,Hello,Hello")]
        public object OnTheFlyIndexingEvaluation(string expression)
        {
            ExpressionEvaluator evaluator = new ExpressionEvaluator(new ContextObject1());

            evaluator.PreEvaluateIndexing += Evaluator_PreEvaluateIndexing;

            evaluator.Namespaces.Add("CodingSeb.ExpressionEvaluator.Tests");

            return evaluator.Evaluate(expression);
        }

        private void Evaluator_PreEvaluateIndexing(object sender, IndexingPreEvaluationEventArg e)
        {
            if(e.This is int intValue && e.EvaluateArg(0) is string text)
            {
                e.Value = string.Join(",", Enumerable.Repeat(text, intValue));
            }
        }

        #endregion

        #endregion

        #region Exception Throwing Evaluation

        #region Test cases source for With Custom Variables Expression Evaluation

        public static IEnumerable<TestCaseData> TestCasesForExceptionThrowingEvaluation
        {
            get
            {
                #region Options

                #region OptionCaseSensitiveEvaluationActive = true

                ExpressionEvaluator evaluator = new ExpressionEvaluator()
                {
                    OptionCaseSensitiveEvaluationActive = true
                };

                evaluator.Variables["isThisReal"] = true;

                yield return new TestCaseData(evaluator, "ISTHISREAL", typeof(ExpressionEvaluatorSyntaxErrorException)).SetCategory("Options").SetCategory("OptionCaseSensitiveEvaluationActive");
                yield return new TestCaseData(evaluator, "isthisreal", typeof(ExpressionEvaluatorSyntaxErrorException)).SetCategory("Options").SetCategory("OptionCaseSensitiveEvaluationActive");
                yield return new TestCaseData(evaluator, "iStHISrEAL", typeof(ExpressionEvaluatorSyntaxErrorException)).SetCategory("Options").SetCategory("OptionCaseSensitiveEvaluationActive");
                yield return new TestCaseData(evaluator, "isThisReal.tostring()", typeof(ExpressionEvaluatorSyntaxErrorException)).SetCategory("Options").SetCategory("OptionCaseSensitiveEvaluationActive");
                yield return new TestCaseData(evaluator, "abs(-1)", typeof(ExpressionEvaluatorSyntaxErrorException)).SetCategory("Options").SetCategory("OptionCaseSensitiveEvaluationActive");

                #endregion

                #region OptionFluidPrefixingActive = false

                evaluator = new ExpressionEvaluator()
                {
                    OptionFluidPrefixingActive = false
                };

                yield return new TestCaseData(evaluator, "List(1,2,3).FluidAdd(4).Count", typeof(ExpressionEvaluatorSyntaxErrorException)).SetCategory("Options").SetCategory("OptionFluidPrefixingActive");

                #endregion

                #region OptionCharEvaluationActive = false

                evaluator = new ExpressionEvaluator()
                {
                    OptionCharEvaluationActive = false
                };

                yield return new TestCaseData(evaluator, "'e'", typeof(ExpressionEvaluatorSyntaxErrorException)).SetCategory("Options").SetCategory("OptionCharEvaluationActive");
                yield return new TestCaseData(evaluator, "\"hell\" + 'o'", typeof(ExpressionEvaluatorSyntaxErrorException)).SetCategory("Options").SetCategory("OptionCharEvaluationActive");
                yield return new TestCaseData(evaluator, "\"Test\" + '\\n'", typeof(ExpressionEvaluatorSyntaxErrorException)).SetCategory("Options").SetCategory("OptionCharEvaluationActive");

                #endregion

                #region OptionStringEvaluationActive = false

                evaluator = new ExpressionEvaluator()
                {
                    OptionStringEvaluationActive = false
                };

                yield return new TestCaseData(evaluator, "\"hello\"", typeof(ExpressionEvaluatorSyntaxErrorException)).SetCategory("Options").SetCategory("OptionStringEvaluationActive");
                yield return new TestCaseData(evaluator, "3 + @\"xyz\".Length", typeof(ExpressionEvaluatorSyntaxErrorException)).SetCategory("Options").SetCategory("OptionStringEvaluationActive");
                yield return new TestCaseData(evaluator, "$\"Test { 5+5 } Test\"", typeof(ExpressionEvaluatorSyntaxErrorException)).SetCategory("Options").SetCategory("OptionStringEvaluationActive");

                #endregion

                #region OptionNewFunctionEvaluationActive = false

                evaluator = new ExpressionEvaluator()
                {
                    OptionNewFunctionEvaluationActive = false
                };

                evaluator.Namespaces.Add(typeof(ClassForTest1).Namespace);

                yield return new TestCaseData(evaluator, "new(ClassForTest1).GetType()", typeof(ExpressionEvaluatorSyntaxErrorException)).SetCategory("Options").SetCategory("OptionNewFunctionEvaluationActive");
                yield return new TestCaseData(evaluator, "new(ClassForTest2, 15).GetType()", typeof(ExpressionEvaluatorSyntaxErrorException)).SetCategory("Options").SetCategory("OptionNewFunctionEvaluationActive");
                yield return new TestCaseData(evaluator, "new(ClassForTest2, 15).Value1", typeof(ExpressionEvaluatorSyntaxErrorException)).SetCategory("Options").SetCategory("OptionNewFunctionEvaluationActive");

                #endregion

                #region OptionNewKeywordEvaluationActive = false

                evaluator = new ExpressionEvaluator()
                {
                    OptionNewKeywordEvaluationActive = false
                };

                evaluator.Namespaces.Add(typeof(ClassForTest1).Namespace);

                yield return new TestCaseData(evaluator, "new ClassForTest1().GetType()", typeof(ExpressionEvaluatorSyntaxErrorException)).SetCategory("Options").SetCategory("OptionNewKeywordEvaluationActive");
                yield return new TestCaseData(evaluator, "new ClassForTest2(15).GetType()", typeof(ExpressionEvaluatorSyntaxErrorException)).SetCategory("Options").SetCategory("OptionNewKeywordEvaluationActive");
                yield return new TestCaseData(evaluator, "new CodingSeb.ExpressionEvaluator.Tests.OtherNamespace.ClassInOtherNameSpace1().Value1", typeof(ExpressionEvaluatorSyntaxErrorException)).SetCategory("Options").SetCategory("OptionNewKeywordEvaluationActive");
                yield return new TestCaseData(evaluator, "new Regex(@\"\\w*[n]\\w*\").Match(\"Which word contains the desired letter ?\").Value", typeof(ExpressionEvaluatorSyntaxErrorException)).SetCategory("Options").SetCategory("OptionNewKeywordEvaluationActive");

                #endregion

                #region OptionStaticMethodsCallActive = false

                evaluator = new ExpressionEvaluator()
                {
                    OptionStaticMethodsCallActive = false
                };

                evaluator.Namespaces.Add(typeof(ClassForTest1).Namespace);

                yield return new TestCaseData(evaluator, "ClassForTest1.StaticStringMethod(\"Bob\")", typeof(ExpressionEvaluatorSyntaxErrorException)).SetCategory("Options").SetCategory("OptionStaticMethodsCallActive");

                #endregion

                #region OptionStaticMethodsCallActive = false

                evaluator = new ExpressionEvaluator()
                {
                    OptionStaticPropertiesGetActive = false
                };

                evaluator.Namespaces.Add(typeof(ClassForTest1).Namespace);

                yield return new TestCaseData(evaluator, "ClassForTest1.StaticIntProperty", typeof(ExpressionEvaluatorSyntaxErrorException)).SetCategory("Options").SetCategory("OptionStaticProperiesGetActive");

                #endregion

                #region OptionInstanceMethodsCallActive = false

                evaluator = new ExpressionEvaluator()
                {
                    OptionInstanceMethodsCallActive = false
                };

                evaluator.Variables["customObject"] = new ClassForTest1();

                evaluator.Namespaces.Add(typeof(ClassForTest1).Namespace);

                yield return new TestCaseData(evaluator, "customObject.Add3To(9)", typeof(ExpressionEvaluatorSyntaxErrorException)).SetCategory("Options").SetCategory("OptionInstanceMethodsCallActive");

                #endregion

                #region OptionInstanceProperiesGetActive = false

                evaluator = new ExpressionEvaluator()
                {
                    OptionInstancePropertiesGetActive = false
                };

                evaluator.Variables["customObject"] = new ClassForTest1();

                evaluator.Namespaces.Add(typeof(ClassForTest1).Namespace);

                yield return new TestCaseData(evaluator, "customObject.IntProperty", typeof(ExpressionEvaluatorSyntaxErrorException)).SetCategory("Options").SetCategory("OptionInstanceProperiesGetActive");

                #endregion

                #region OptionIndexingActive = false

                evaluator = new ExpressionEvaluator()
                {
                    OptionIndexingActive = false
                };

                evaluator.Variables["dict"] = new Dictionary<string, int>() { { "intValue", 5 } };

                yield return new TestCaseData(evaluator, "List(1,2,3)[1]", typeof(ExpressionEvaluatorSyntaxErrorException)).SetCategory("Options").SetCategory("OptionIndexingActive");
                yield return new TestCaseData(evaluator, "dict[\"intValue\"]", typeof(ExpressionEvaluatorSyntaxErrorException)).SetCategory("Options").SetCategory("OptionIndexingActive");

                #endregion

                #region OptionInlineNamespacesEvaluationRule

                evaluator = new ExpressionEvaluator()
                {
                    OptionInlineNamespacesEvaluationRule = InlineNamespacesEvaluationRule.BlockAll
                };

                yield return new TestCaseData(evaluator, "new System.Collections.Generic.List<string>(){\"-\"}", typeof(ExpressionEvaluatorSyntaxErrorException))
                    .SetCategory("Options")
                    .SetCategory("InlineNamespacesEvaluationRule.BlockAll")
                    .SetCategory("OptionInlineNamespacesEvaluationRule");
                
                evaluator = new ExpressionEvaluator()
                {
                    OptionInlineNamespacesEvaluationRule = InlineNamespacesEvaluationRule.AllowOnlyInlineNamespacesList
                };

                evaluator.InlineNamespacesList = evaluator.Namespaces.ToList();

                evaluator.InlineNamespacesList.Remove("System.Collections.Generic");

                yield return new TestCaseData(evaluator, "new System.Collections.Generic.List<string>()", typeof(ExpressionEvaluatorSyntaxErrorException))
                    .SetCategory("Options")
                    .SetCategory("InlineNamespacesEvaluationRule.AllowOnlyInlineNamespacesList")
                    .SetCategory("OptionInlineNamespacesEvaluationRule");
                
                evaluator = new ExpressionEvaluator()
                {
                    OptionInlineNamespacesEvaluationRule = InlineNamespacesEvaluationRule.BlockOnlyInlineNamespacesList
                };

                evaluator.InlineNamespacesList = evaluator.Namespaces.ToList();
                
                yield return new TestCaseData(evaluator, "new System.Collections.Generic.List<string>(){ \"\" }", typeof(ExpressionEvaluatorSyntaxErrorException))
                    .SetCategory("Options")
                    .SetCategory("InlineNamespacesEvaluationRule.BlockOnlyInlineNamespacesList")
                    .SetCategory("OptionInlineNamespacesEvaluationRule");

                #endregion

                #endregion

                #region TypesToBlock

                evaluator = new ExpressionEvaluator();

                evaluator.TypesToBlock.Add(typeof(Regex));

                yield return new TestCaseData(evaluator, "new ExpressionEvaluation()", typeof(ExpressionEvaluatorSyntaxErrorException)).SetCategory("Options").SetCategory("TypesToBlock");
                yield return new TestCaseData(evaluator, "new Regex(@\"\\d+\").IsMatch(\"sdajflk32748safd\")", typeof(ExpressionEvaluatorSyntaxErrorException)).SetCategory("Options").SetCategory("TypesToBlock");

                #endregion

                #region On the fly pre events cancel

                evaluator = new ExpressionEvaluator(new Dictionary<string, object>
                {
                    { "P1var", "P1" },
                    { "myObj", new ClassForTest1() },
                    { "nullVar", null },
                    { "myArray", new int[] {1, 2, 3} },
                });

                evaluator.PreEvaluateVariable += (sender, e) =>
                {
                    if (e.Name.StartsWith("P"))
                        e.CancelEvaluation = true;
                };

                evaluator.PreEvaluateFunction += (sender, e) =>
                {
                    if (e.Name.StartsWith("A"))
                        e.CancelEvaluation = true;
                };

                evaluator.PreEvaluateIndexing += (sender, e) =>
                {
                    if (e.This is int[])
                        e.CancelEvaluation = true;
                };

                yield return new TestCaseData(evaluator, "Pi", typeof(ExpressionEvaluatorSyntaxErrorException)).SetCategory("OnTheFly canceled Var");
                yield return new TestCaseData(evaluator, "P1var", typeof(ExpressionEvaluatorSyntaxErrorException)).SetCategory("OnTheFly canceled Var");
                yield return new TestCaseData(evaluator, "myObj.PropertyThatWillFailed", typeof(ExpressionEvaluatorSyntaxErrorException)).SetCategory("OnTheFly canceled Var");
                yield return new TestCaseData(evaluator, "myObj.Add3To(5)", typeof(ExpressionEvaluatorSyntaxErrorException)).SetCategory("OnTheFly canceled Func");
                yield return new TestCaseData(evaluator, "Abs(-5)", typeof(ExpressionEvaluatorSyntaxErrorException)).SetCategory("OnTheFly canceled Func");
                yield return new TestCaseData(evaluator, "myArray[1]", typeof(ExpressionEvaluatorSyntaxErrorException)).SetCategory("OnTheFlyCanceledIndexing");
                #endregion

                #region Bugs corrections

                yield return new TestCaseData(evaluator, "true && 1/0>0", typeof(DivideByZeroException)).SetCategory("Conditional And, positive left operand (should lead to exception)");
                yield return new TestCaseData(evaluator, "false || 1/0>0", typeof(DivideByZeroException)).SetCategory("Conditional Or, positive left operand (should lead to exception associativity)");
                yield return new TestCaseData(evaluator, "true && (true && 1/0>0)", typeof(DivideByZeroException)).SetCategory("Conditional And, positive left operand (should lead to exception)");
                yield return new TestCaseData(evaluator, "false || (false || 1/0>0)", typeof(DivideByZeroException)).SetCategory("Conditional Or, positive left operand (should lead to exception associativity)");

                #endregion
            }
        }

        #endregion

        [TestCaseSource(nameof(TestCasesForExceptionThrowingEvaluation))]
        public void ExceptionThrowingEvaluation(ExpressionEvaluator evaluator, string expression, Type exceptionType)
        {
            Exception e = null;
            object result = null;

            try
            {
                result = evaluator.Evaluate(expression);
            }
            catch(Exception exception)
            {
                e = exception;
            }

            result.ShouldBeNull();
            e.ShouldNotBeNull().ShouldBeOfType(exceptionType);
        }

        #endregion

        #region Bug corrections

        /// <summary>
        /// To correct #127 Evaluating "new DateTime(2022,1,20)" does not work
        /// unless OptionInlineNamespacesEvaluationActive is turned on
        /// </summary>
        [Test]
        [Category("Bug")]
        [Category("#127")]
        public void Evaluate_NewDateTime_When_OptionInlineNamespacesEvaluationActive_is_off()
        {
            ExpressionEvaluator evaluator = new ExpressionEvaluator()
            {
                OptionInlineNamespacesEvaluationRule = InlineNamespacesEvaluationRule.BlockAll,
            };

            DateTime? dateTime = evaluator.Evaluate<DateTime>("new DateTime(2022,1,20)");

            dateTime.HasValue.ShouldBeTrue();

            dateTime.Value.Year.ShouldBe(2022);
            dateTime.Value.Month.ShouldBe(1);
            dateTime.Value.Day.ShouldBe(20);
        }

        /// <summary>
        /// To correct #127 Evaluating "new DateTime(2022,1,20)" does not work
        /// unless OptionInlineNamespacesEvaluationActive is turned on
        /// </summary>
        [Test]
        [Category("Bug")]
        [Category("#127")]
        public void Evaluate_NewDateTime_When_OptionInlineNamespacesEvaluationActive_is_on()
        {
            ExpressionEvaluator evaluator = new ExpressionEvaluator()
            {
                OptionInlineNamespacesEvaluationRule = InlineNamespacesEvaluationRule.AllowAll,
            };

            DateTime? dateTime = evaluator.Evaluate<DateTime>("new DateTime(2022,1,20)");

            dateTime.HasValue.ShouldBeTrue();

            dateTime.Value.Year.ShouldBe(2022);
            dateTime.Value.Month.ShouldBe(1);
            dateTime.Value.Day.ShouldBe(20);
        }

        /// <summary>
        /// To correct #81 Exception is assigned to variable
        /// With simple variable
        /// </summary>
        [Test]
        [Category("Bug")]
        [Category("#81")]
        public void Evaluate_WithException_ThrowsExceptionAndDoesNotAssignItSimpleVariable()
        {
            ExpressionEvaluator evaluator = new ExpressionEvaluator();

            evaluator.Variables.Add("exceptionGenerator", new ExceptionGenerator());
            Assert.Throws<ExpressionEvaluatorSyntaxErrorException>(() => evaluator.Evaluate("result = exceptionGenerator.ThrowAnException()"));

            evaluator.Variables.ContainsKey("result").ShouldBeFalse();
        }

        /// <summary>
        /// To correct #81 Exception is assigned to variable
        /// With InObject
        /// </summary>
        [Test]
        [Category("Bug")]
        [Category("#81")]
        public void Evaluate_WithException_ThrowsExceptionAndDoesNotAssignItInObject()
        {
            ExpressionEvaluator evaluator = new ExpressionEvaluator();

            ObjectContainer objectContainer = new ObjectContainer();

            evaluator.Variables.Add("exceptionGenerator", new ExceptionGenerator());
            evaluator.Variables.Add("objectContainer", objectContainer);
            Assert.Throws<ExpressionEvaluatorSyntaxErrorException>(() => evaluator.Evaluate("objectContainer.AnObjectProperty = exceptionGenerator.ThrowAnException()"));

            objectContainer.AnObjectProperty.ShouldBeOfType(typeof(int));
            objectContainer.AnObjectProperty.ShouldBe(10);
        }

        /// <summary>
        /// To correct #87 Parameter type mismatch is causing missing methods
        /// </summary>
        [Test]
        [Category("Bug")]
        [Category("#87")]
        public void Evaluate_ParameterTypeMismatchAutoAdapt()
        {
            var evaluator = new ExpressionEvaluator();
            evaluator.Evaluate("DateTime.Now.AddDays(1)")
                .ShouldBeOfType<DateTime>();
        }

        /// <summary>
        /// To correct #104 Extension methods do not work on context
        /// work now with this keyword like in C#
        /// </summary>
        [Test]
        [Category("Bug")]
        [Category("#104")]
        public void Evaluate_ExtensionMethodOnContext()
        {
            var evaluator = new ExpressionEvaluator
            {
                Context = new List<int>()
                {
                    1,2,3,4,5
                }
            };

            evaluator.Evaluate("this.Sum()")
                .ShouldBe(15);
        }

        /// <summary>
        /// To correct #104 Indexing do not work on context
        /// work now with this keyword like in C#
        /// </summary>
        [Test]
        [Category("Bug")]
        [Category("#104")]
        public void Evaluate_IndexingOnContext()
        {
            var evaluator = new ExpressionEvaluator
            {
                Context = new ClassForIndexing()
            };

            evaluator.Evaluate("this[\"Test\"]")
                .ShouldBe("TestTest");
        }

        /// <summary>
        /// To correct #105 Exception were not thrown when concat with string
        /// </summary>
        [Test]
        [Category("Bug")]
        [Category("#105")]
        public void Evaluate_ExceptionsNotThrownWhenConcatWithString()
        {
            var evaluator = new ExpressionEvaluator
            {
                Context = new { Person = new Person2 { Name = null, Number = 1.11m } }
            };

            Should.Throw<NullReferenceException>(() => Console.WriteLine(evaluator.Evaluate("Person.Name.Trim() + \"Test one \"")));
            Should.Throw<NullReferenceException>(() => Console.WriteLine(evaluator.Evaluate("\"Test one \" + Person.Name.Trim()")));
            Should.Throw<ExpressionEvaluatorSyntaxErrorException>(() => Console.WriteLine(evaluator.Evaluate("\"Test two \" + Person.AnotherName.Trim()")));
        }

        /// <summary>
        /// #105 Exception were not thrown when in string interpolation
        /// </summary>
        [Test]
        [Category("Bug")]
        [Category("#105")]
        public void Evaluate_ExceptionsNotThrownWhenInStringInterpolation()
        {
            var evaluator = new ExpressionEvaluator
            {
                Context = new { Person = new Person2 { Name = null, Number = 1.11m } }
            };

            Should.Throw<NullReferenceException>(() => Console.WriteLine(evaluator.Evaluate("$\"Test one {Person.Name.Trim()}\"")));
            Should.Throw<ExpressionEvaluatorSyntaxErrorException>(() => Console.WriteLine(evaluator.Evaluate("$\"Test two {Person.AnotherName.Trim()}\"")));
        }

        [Test]
        [Category("Bug")]
        [Category("MergeRequest")]
        [Category("#107")]
        public void Evaluate_DoubleDoubleQuotesInEscapedStringThrowException()
        {
            var evaluator = new ExpressionEvaluator();

            evaluator.Evaluate("@\"Hello \"\" Joe\"").ShouldBe(@"Hello "" Joe");
        }

        //[Test]
        //[Category("Bug")]
        //[Category("#135")]
        //public void Evaluate_MultiLevelPropertyAccessAfterConditionalAndBeforeDoubleEquals()
        //{
        //    ExpressionEvaluator ee = new ExpressionEvaluator();

        //    ee.Variables["A"] = new Dictionary<string, FileStream>();

        //    ee.Evaluate<bool>("A.TryGetValue(\"NotExist\", out FileStream fs) && fs.SafeFileHandle.IsClosed == true").ShouldBe<bool>(false);
        //}

        #endregion

        #region EvaluateWithSpecificEvaluator

        #region TestCasesEvaluateWithSpecificEvaluator

        public static IEnumerable<TestCaseData> TestCasesEvaluateWithSpecificEvaluator
        {
            get
            {
                #region Different culture for numbers

                yield return new TestCaseData(new ExpressionEvaluator
                {
                    OptionNumberParsingDecimalSeparator = ",",
                }
                , "0,5"
                , null)
                .Returns(0.5)
                .SetCategory("Options")
                .SetCategory("Numbers Culture");

                yield return new TestCaseData(new ExpressionEvaluator
                {
                    OptionNumberParsingDecimalSeparator = "'",
                }
                , "0'5"
                , null)
                .Returns(0.5)
                .SetCategory("Options")
                .SetCategory("Numbers Culture");

                yield return new TestCaseData(new ExpressionEvaluator
                {
                    OptionNumberParsingDecimalSeparator = ".",
                }
                , "0.5"
                , null)
                .Returns(0.5)
                .SetCategory("Options")
                .SetCategory("Numbers Culture");

                yield return new TestCaseData(new ExpressionEvaluator
                {
                    OptionNumberParsingDecimalSeparator = ",",
                    OptionFunctionArgumentsSeparator = ";"
                }
                , "Max(0,5; 0,7)"
                , null)
                .Returns(0.7)
                .SetCategory("Options")
                .SetCategory("Numbers Culture");

                yield return new TestCaseData(new ExpressionEvaluator
                {
                    OptionNumberParsingDecimalSeparator = ",",
                    OptionNumberParsingThousandSeparator = "'",
                    OptionFunctionArgumentsSeparator = ";"
                }
                , "Max(1'200,5; 1'111'000,7)"
                , null)
                .Returns(1111000.7)
                .SetCategory("Options")
                .SetCategory("Numbers Culture");

                yield return new TestCaseData(new ExpressionEvaluator
                {
                    OptionNumberParsingDecimalSeparator = ",",
                    OptionNumberParsingThousandSeparator = "'",
                    OptionInitializersSeparator = ";"
                }
                 , "new double[]{1'200,5; 1'111'000,7}.Max()"
                 , null)
                 .Returns(1111000.7)
                 .SetCategory("Options")
                 .SetCategory("Numbers Culture");

                #endregion

                #region Force Integer numbers default type

                yield return new TestCaseData(new ExpressionEvaluator()
                    , "(130-120)/(2*250)"
                    , null)
                    .Returns(0)
                    .SetCategory("Options")
                    .SetCategory("Integer Numbers default types");

                yield return new TestCaseData(new ExpressionEvaluator
                {
                    OptionForceIntegerNumbersEvaluationsAsDoubleByDefault = false
                }
                    , "(130-120)/(2*250)"
                    , null)
                    .Returns(0)
                    .SetCategory("Options")
                    .SetCategory("Integer Numbers default types");

                ExpressionEvaluator evaluatorWithIntForceToDouble = new ExpressionEvaluator
                {
                    OptionForceIntegerNumbersEvaluationsAsDoubleByDefault = true
                };

                yield return new TestCaseData(evaluatorWithIntForceToDouble
                    , "(130-120)/(2*250)"
                    , null)
                    .Returns(0.02)
                    .SetCategory("Options")
                    .SetCategory("Integer Numbers default types");

                yield return new TestCaseData(evaluatorWithIntForceToDouble
                    , "Round(5.54,1)"
                    , null)
                    .Returns(5.5)
                    .SetCategory("Bug")
                    .SetCategory("Options")
                    .SetCategory("Integer Numbers default types");

                yield return new TestCaseData(evaluatorWithIntForceToDouble
                    , "Round(5.54,1, MidpointRounding.ToEven)"
                    , null)
                    .Returns(5.5)
                    .SetCategory("Bug")
                    .SetCategory("Options")
                    .SetCategory("Integer Numbers default types");

                yield return new TestCaseData(evaluatorWithIntForceToDouble
                    , "Round(5.54,1, MidpointRounding.AwayFromZero)"
                    , null)
                    .Returns(5.5)
                    .SetCategory("Bug")
                    .SetCategory("Options")
                    .SetCategory("Integer Numbers default types");

                yield return new TestCaseData(evaluatorWithIntForceToDouble
                    , "(new string[2]).Length"
                    , null)
                    .Returns(2)
                    .SetCategory("Bug")
                    .SetCategory("Options")
                    .SetCategory("Integer Numbers default types");

                yield return new TestCaseData(evaluatorWithIntForceToDouble
                    , "(new string[] { \"Test\", \"Other\", \"Youhouhou\" })[1]"
                    , null)
                    .Returns("Other")
                    .SetCategory("Bug")
                    .SetCategory("Options")
                    .SetCategory("Integer Numbers default types");

                #endregion

                #region Onthefly events (Pre events and/or Generics and/or Static)

                ExpressionEvaluator evaluatorOnTheFlies = new ExpressionEvaluator(new Dictionary<string, object>
                {
                    { "myvar1", 10 },
                    { "myvar2", 3 },
                });

                evaluatorOnTheFlies.Namespaces.Add("CodingSeb.ExpressionEvaluator.Tests");

                void VariableEval(object sender, VariableEvaluationEventArg e)
                {
                    if (e.Name.Equals("GetSpecifiedGenericTypesVar"))
                    {
                        e.Value = e.EvaluateGenericTypes();
                    }
                    else if (e.Name.Equals("myvar2"))
                    {
                        e.Value = 50;
                    }
                    else if (e.This is ClassOrEnumType classOrTypeName && classOrTypeName.Type == typeof(ClassForTest1) && e.Name.Equals("OnTheFlyStaticVar"))
                    {
                        e.Value = 10;
                    }
                }

                void FunctionEval(object sender, FunctionEvaluationEventArg e)
                {
                    if (e.Name.Equals("GetSpecifiedGenericTypesFunc"))
                    {
                        e.Value = e.EvaluateGenericTypes();
                    }
                    else if (e.This is ClassOrEnumType classOrTypeName && classOrTypeName.Type == typeof(ClassForTest1) && e.Name.Equals("OnTheFlyStaticFunc"))
                    {
                        e.Value = 8;
                    }
                }

                void Evaluator_PreEvaluateFunction(object sender, FunctionPreEvaluationEventArg e)
                {
                    if (e.Name.Equals("Test"))
                    {
                        e.Value = $"It is a test for {e.EvaluateArg(0)}";
                    }
                    else if (e.Name.Equals("GenericNamespace") && e.HasGenericTypes)
                    {
                        // e.EvaluateGenericTypes() return a Type[]
                        e.Value = e.EvaluateGenericTypes()[0].Namespace;
                    }
                    else if (e.This is ClassOrEnumType classOrTypeName && classOrTypeName.Type == typeof(ClassForTest1) && e.Name.Equals("OnTheFlyStaticPreFunc"))
                    {
                        e.Value = 15;
                    }
                }

                void Evaluator_PreEvaluateVariable(object sender, VariablePreEvaluationEventArg e)
                {
                    if (e.Name.Equals("myvar1"))
                    {
                        e.Value = 5;
                    }
                    else if (e.Name.Equals("GenericAssembly") && e.HasGenericTypes)
                    {
                        // e.EvaluateGenericTypes() return a Type[]
                        e.Value = e.EvaluateGenericTypes()[0].Assembly.GetName().Name;
                    }
                    else if (e.This is ClassOrEnumType classOrTypeName && classOrTypeName.Type == typeof(ClassForTest1) && e.Name.Equals("OnTheFlyStaticPreVar"))
                    {
                        e.Value = 3;
                    }
                }

                evaluatorOnTheFlies.EvaluateVariable += VariableEval;
                evaluatorOnTheFlies.EvaluateFunction += FunctionEval;

                evaluatorOnTheFlies.PreEvaluateVariable += Evaluator_PreEvaluateVariable;
                evaluatorOnTheFlies.PreEvaluateFunction += Evaluator_PreEvaluateFunction;

                yield return new TestCaseData(evaluatorOnTheFlies
                        , "GetSpecifiedGenericTypesFunc<string>()[0]"
                        , null)
                        .Returns(typeof(string))
                        .SetCategory("On the fly func")
                        .SetCategory("GenericTypes");

                yield return new TestCaseData(evaluatorOnTheFlies
                        , "GetSpecifiedGenericTypesVar<string>[0]"
                        , null)
                        .Returns(typeof(string))
                        .SetCategory("On the fly var")
                        .SetCategory("GenericTypes");

                yield return new TestCaseData(evaluatorOnTheFlies
                        , "GetSpecifiedGenericTypesFunc<string, List<int>>()[1]"
                        , null)
                        .Returns(typeof(List<int>))
                        .SetCategory("On the fly func")
                        .SetCategory("GenericTypes");

                yield return new TestCaseData(evaluatorOnTheFlies
                        , "GetSpecifiedGenericTypesVar<string, List<int>>[1]"
                        , null)
                        .Returns(typeof(List<int>))
                        .SetCategory("On the fly var")
                        .SetCategory("GenericTypes");

                yield return new TestCaseData(evaluatorOnTheFlies
                        , "myvar1"
                        , null)
                        .Returns(5)
                        .SetCategory("var evaluation priority");

                yield return new TestCaseData(evaluatorOnTheFlies
                        , "myvar2"
                        , null)
                        .Returns(3)
                        .SetCategory("var evaluation priority");

                yield return new TestCaseData(evaluatorOnTheFlies
                        , "ClassForTest1.OnTheFlyStaticFunc()"
                        , null)
                        .Returns(8)
                        .SetCategory("On the fly func")
                        .SetCategory("Static Onthefly");

                yield return new TestCaseData(evaluatorOnTheFlies
                        , "ClassForTest1.OnTheFlyStaticVar"
                        , null)
                        .Returns(10)
                        .SetCategory("On the fly var")
                        .SetCategory("Static Onthefly");

                yield return new TestCaseData(evaluatorOnTheFlies
                        , "ClassForTest1.OnTheFlyStaticPreFunc()"
                        , null)
                        .Returns(15)
                        .SetCategory("On the fly func")
                        .SetCategory("Static Onthefly");

                yield return new TestCaseData(evaluatorOnTheFlies
                        , "ClassForTest1.OnTheFlyStaticPreVar"
                        , null)
                        .Returns(3)
                        .SetCategory("On the fly var")
                        .SetCategory("Static Onthefly");

                // MR #106
                ExpressionEvaluator nullForceEvaluator = new ExpressionEvaluator
                {
                    Variables = new Dictionary<string, object>()
                {
                    { "obj", new { }}
                }
                };

                nullForceEvaluator.EvaluateVariable += (sender, e) =>
                {
                    e.HasValue = true;
                    e.Value = null;
                };

                yield return new TestCaseData(nullForceEvaluator
                    , "obj.x"
                    , null)
                    .Returns(null)
                    .SetCategory("On the fly var");

                #endregion

                #region With Context object

                ExpressionEvaluator evaluatorWithContext = new ExpressionEvaluator(new ContextObject1());

                yield return new TestCaseData(evaluatorWithContext
                    , "AIntValue"
                    , null)
                    .Returns(3)
                    .SetCategory("Context object");

                yield return new TestCaseData(evaluatorWithContext
                    , "SayHelloTo(\"Bob\")"
                    , null)
                    .Returns("Hello Bob")
                    .SetCategory("Context object");

                ExpressionEvaluator evaluatorWithPersonContext = new ExpressionEvaluator
                {
                    Context = new Person1()
                    {
                        name = "John",
                        LastName = "Smith"
                    }
                };

                yield return new TestCaseData(evaluatorWithPersonContext
                    , "name + \" \" + LastName"
                    , null)
                    .Returns("John Smith")
                    .SetCategory("Context object");

                yield return new TestCaseData(evaluatorWithPersonContext
                    , "APersonMethod() + 10"
                    , null)
                    .Returns(20)
                    .SetCategory("Context object");

                #endregion

                #region NonPublic members

                ExpressionEvaluator nonPublicEvaluator = new ExpressionEvaluator(new Dictionary<string, object>()
                {
                    { "obj", new ClassWithNonPublicMembersAndMethods() }
                })
                {
                    OptionAllowNonPublicMembersAccess = true
                };

                yield return new TestCaseData(nonPublicEvaluator
                    , "obj.myPrivateField"
                    , null)
                    .Returns(5)
                    .SetCategory("NonPublic Members and methods");

                yield return new TestCaseData(nonPublicEvaluator
                    , "obj.myProtectedField"
                    , null)
                    .Returns(10)
                    .SetCategory("NonPublic Members and methods");

                yield return new TestCaseData(nonPublicEvaluator
                    , "obj.MyPrivateProperty"
                    , null)
                    .Returns(15)
                    .SetCategory("NonPublic Members and methods");

                yield return new TestCaseData(nonPublicEvaluator
                    , "obj.MyProtectedProperty"
                    , null)
                    .Returns(20)
                    .SetCategory("NonPublic Members and methods");

                yield return new TestCaseData(nonPublicEvaluator
                    , "obj.MyPrivateMethod(\"Bob\")"
                    , null)
                    .Returns("Hello Bob")
                    .SetCategory("NonPublic Members and methods");

                yield return new TestCaseData(nonPublicEvaluator
                    , "obj.MyProtectedMethod(\"Bob\")"
                    , null)
                    .Returns("GoodBye Bob")
                    .SetCategory("NonPublic Members and methods");

                #endregion

                #region Inherits ExpressionEvaluator

                #region Redefine existing operators

                ExpressionEvaluator xExpressionEvaluator1 = new XExpressionEvaluator1();

                yield return new TestCaseData(xExpressionEvaluator1
                    , "true and true"
                    , null)
                    .Returns(true)
                    .SetCategory("ExpressionEvaluator extend")
                    .SetCategory("inherits ExpressionEvaluator")
                    .SetCategory("Custom operators");

                yield return new TestCaseData(xExpressionEvaluator1
                    , "false and true"
                    , null)
                    .Returns(false)
                    .SetCategory("ExpressionEvaluator extend")
                    .SetCategory("inherits ExpressionEvaluator")
                    .SetCategory("Custom operators");

                yield return new TestCaseData(xExpressionEvaluator1
                    , "false and false"
                    , null)
                    .Returns(false)
                    .SetCategory("ExpressionEvaluator extend")
                    .SetCategory("inherits ExpressionEvaluator")
                    .SetCategory("Custom operators");

                yield return new TestCaseData(xExpressionEvaluator1
                    , "true and false"
                    , null)
                    .Returns(false)
                    .SetCategory("ExpressionEvaluator extend")
                    .SetCategory("inherits ExpressionEvaluator")
                    .SetCategory("Custom operators");

                yield return new TestCaseData(xExpressionEvaluator1
                    , "true or true"
                    , null)
                    .Returns(true)
                    .SetCategory("ExpressionEvaluator extend")
                    .SetCategory("inherits ExpressionEvaluator")
                    .SetCategory("Custom operators");

                yield return new TestCaseData(xExpressionEvaluator1
                    , "false or true"
                    , null)
                    .Returns(true)
                    .SetCategory("ExpressionEvaluator extend")
                    .SetCategory("inherits ExpressionEvaluator")
                    .SetCategory("Custom operators");

                yield return new TestCaseData(xExpressionEvaluator1
                    , "false or false"
                    , null)
                    .Returns(false)
                    .SetCategory("ExpressionEvaluator extend")
                    .SetCategory("inherits ExpressionEvaluator")
                    .SetCategory("Custom operators");

                yield return new TestCaseData(xExpressionEvaluator1
                    , "true or false"
                    , null)
                    .Returns(true)
                    .SetCategory("ExpressionEvaluator extend")
                    .SetCategory("inherits ExpressionEvaluator")
                    .SetCategory("Custom operators");

                yield return new TestCaseData(xExpressionEvaluator1
                    , "true && true"
                    , new Func<Exception, object>(exception =>
                    {
                        exception.ShouldNotBeOfType<ExpressionEvaluatorSyntaxErrorException>();

                        return true;
                    }))
                    .Returns(true)
                    .SetCategory("ExpressionEvaluator extend")
                    .SetCategory("inherits ExpressionEvaluator")
                    .SetCategory("Custom operators");

                yield return new TestCaseData(xExpressionEvaluator1
                    , "true || true"
                    , new Func<Exception, object>(exception =>
                   {
                       exception.ShouldNotBeOfType<ExpressionEvaluatorSyntaxErrorException>();

                       return true;
                   }))
                    .Returns(true)
                    .SetCategory("ExpressionEvaluator extend")
                    .SetCategory("inherits ExpressionEvaluator")
                    .SetCategory("Custom operators");

                #endregion

                #region Add your own simple operators

                ExpressionEvaluator xExpressionEvaluator2 = new XExpressionEvaluator2();

                yield return new TestCaseData(xExpressionEvaluator2
                    , "1#", null)
                    .Returns(1)
                    .SetCategory("ExpressionEvaluator extend")
                    .SetCategory("inherits ExpressionEvaluator")
                    .SetCategory("Custom operators");

                yield return new TestCaseData(xExpressionEvaluator2
                    , "2#", null)
                    .Returns(0.25)
                    .SetCategory("ExpressionEvaluator extend")
                    .SetCategory("inherits ExpressionEvaluator")
                    .SetCategory("Custom operators");

                yield return new TestCaseData(xExpressionEvaluator2
                    , "-4# - 6", null)
                    .Returns(250)
                    .SetCategory("ExpressionEvaluator extend")
                    .SetCategory("inherits ExpressionEvaluator")
                    .SetCategory("Custom operators");

                yield return new TestCaseData(xExpressionEvaluator2
                    , "2##", null)
                    .Returns(1.4142135623730952d)
                    .SetCategory("ExpressionEvaluator extend")
                    .SetCategory("inherits ExpressionEvaluator")
                    .SetCategory("Custom operators");

                yield return new TestCaseData(xExpressionEvaluator2
                    , "2## + 1", null)
                    .Returns(2.4142135623730949d)
                    .SetCategory("ExpressionEvaluator extend")
                    .SetCategory("inherits ExpressionEvaluator")
                    .SetCategory("Custom operators");

                yield return new TestCaseData(xExpressionEvaluator2
                    , "2## + +-+-~+1", null)
                    .Returns(-0.58578643762690485d)
                    .SetCategory("ExpressionEvaluator extend")
                    .SetCategory("inherits ExpressionEvaluator")
                    .SetCategory("Custom operators");

                yield return new TestCaseData(xExpressionEvaluator2
                    , "2#°", null)
                    .Returns(0.70710678118654757d)
                    .SetCategory("ExpressionEvaluator extend")
                    .SetCategory("inherits ExpressionEvaluator")
                    .SetCategory("Custom operators");

                yield return new TestCaseData(xExpressionEvaluator2
                    , "2°#", null)
                    .Returns(0.00390625d)
                    .SetCategory("ExpressionEvaluator extend")
                    .SetCategory("inherits ExpressionEvaluator")
                    .SetCategory("Custom operators");

                yield return new TestCaseData(xExpressionEvaluator2
                     , "2#° + +-+-~+1", null)
                     .Returns(-1.2928932188134525d)
                     .SetCategory("ExpressionEvaluator extend")
                     .SetCategory("inherits ExpressionEvaluator")
                     .SetCategory("Custom operators");

                yield return new TestCaseData(xExpressionEvaluator2
                     , "2°# + +-+-~+1", null)
                     .Returns(-1.99609375d)
                     .SetCategory("ExpressionEvaluator extend")
                     .SetCategory("inherits ExpressionEvaluator")
                     .SetCategory("Custom operators");

                yield return new TestCaseData(xExpressionEvaluator2
                    , "1 love 2", null)
                    .Returns(6)
                    .SetCategory("ExpressionEvaluator extend")
                    .SetCategory("inherits ExpressionEvaluator")
                    .SetCategory("Custom operators");

                yield return new TestCaseData(xExpressionEvaluator2
                    , "1 love 2 >> 1", null)
                    .Returns(3)
                    .SetCategory("ExpressionEvaluator extend")
                    .SetCategory("inherits ExpressionEvaluator")
                    .SetCategory("Custom operators");

                yield return new TestCaseData(xExpressionEvaluator2
                    , "Not true", null)
                    .Returns(false)
                    .SetCategory("ExpressionEvaluator extend")
                    .SetCategory("inherits ExpressionEvaluator")
                    .SetCategory("Custom operators");

                yield return new TestCaseData(xExpressionEvaluator2
                    , "Not(true)", null)
                    .Returns(false)
                    .SetCategory("ExpressionEvaluator extend")
                    .SetCategory("inherits ExpressionEvaluator")
                    .SetCategory("Custom operators");

                yield return new TestCaseData(xExpressionEvaluator2
                    , "Not(1 == 1)", null)
                    .Returns(false)
                    .SetCategory("ExpressionEvaluator extend")
                    .SetCategory("inherits ExpressionEvaluator")
                    .SetCategory("Custom operators");

                #endregion

                #region Add a complex operator or change the parsing process

                ExpressionEvaluator xExpressionEvaluator3 = new XExpressionEvaluator3();

                yield return new TestCaseData(xExpressionEvaluator3
                    , "\"A sentence where a word must be replaced where it is\" ° \"replaced\" @ \"kept\"", null)
                    .Returns("A sentence where a word must be kept where it is")
                    .SetCategory("ExpressionEvaluator extend")
                    .SetCategory("inherits ExpressionEvaluator")
                    .SetCategory("Custom operators")
                    .SetCategory("Custom parsing");

                yield return new TestCaseData(xExpressionEvaluator3
                    , "#1985-09-11.Year", null)
                    .Returns(1985)
                    .SetCategory("ExpressionEvaluator extend")
                    .SetCategory("inherits ExpressionEvaluator")
                    .SetCategory("Custom syntax")
                    .SetCategory("Custom parsing");

                yield return new TestCaseData(xExpressionEvaluator3
                    , "#1985-09-11.Month", null)
                    .Returns(9)
                    .SetCategory("ExpressionEvaluator extend")
                    .SetCategory("inherits ExpressionEvaluator")
                    .SetCategory("Custom syntax")
                    .SetCategory("Custom parsing");

                yield return new TestCaseData(xExpressionEvaluator3
                    , "#1985-09-11.Day", null)
                    .Returns(11)
                    .SetCategory("ExpressionEvaluator extend")
                    .SetCategory("inherits ExpressionEvaluator")
                    .SetCategory("Custom syntax")
                    .SetCategory("Custom parsing");

                yield return new TestCaseData(xExpressionEvaluator3
                    , "#1985-09-11.Equals(new DateTime(1985,9,11))", null)
                    .Returns(true)
                    .SetCategory("ExpressionEvaluator extend")
                    .SetCategory("inherits ExpressionEvaluator")
                    .SetCategory("Custom syntax")
                    .SetCategory("Custom parsing");

                #endregion

                #endregion

                #region Method with params parameter

                ExpressionEvaluator evaluatorForMethodArgs()
                {
                    ExpressionEvaluator ee = new ExpressionEvaluator(new Dictionary<string, object>()
                    {
                        { "paramsObj", new MethodArgKeywordClass() }
                    });

                    ee.StaticTypesForExtensionsMethods.Add(typeof(MethodArgKeywordClassExtension));
                    return ee;
                }

                yield return new TestCaseData(evaluatorForMethodArgs()
                    , "paramsObj.SumOf(1, 2)"
                    , null)
                    .Returns(3)
                    .SetCategory("ParamsKeywordMethod");

                yield return new TestCaseData(evaluatorForMethodArgs()
                    , "paramsObj.SumOf(1, 2, 3)"
                    , null)
                    .Returns(6)
                    .SetCategory("ParamsKeywordMethod");

                yield return new TestCaseData(evaluatorForMethodArgs()
                    , "paramsObj.SumOf(1,2,3,4)"
                    , null)
                    .Returns(10)
                    .SetCategory("ParamsKeywordMethod");

                yield return new TestCaseData(evaluatorForMethodArgs()
                    , "paramsObj.SumOf2()"
                    , null)
                    .Returns(0)
                    .SetCategory("ParamsKeywordMethod");

                yield return new TestCaseData(evaluatorForMethodArgs()
                    , "paramsObj.SumOf2(1)"
                    , null)
                    .Returns(1)
                    .SetCategory("ParamsKeywordMethod");

                yield return new TestCaseData(evaluatorForMethodArgs()
                    , "paramsObj.SumOf2(1,2,3,4)"
                    , null)
                    .Returns(10)
                    .SetCategory("ParamsKeywordMethod");

                yield return new TestCaseData(evaluatorForMethodArgs()
                    , "string.Join(\",\", 2, 3.5, \"Hello\", true)"
                    , null)
                    .Returns("2,3.5,Hello,True")
                    .SetCategory("ParamsKeywordMethod");

                yield return new TestCaseData(evaluatorForMethodArgs()
                    , "string.Join(\",\", 2, 3.5, null, \"Hello\", true)"
                    , null)
                    .Returns("2,3.5,,Hello,True")
                    .SetCategory("ParamsKeywordMethod");

                yield return new TestCaseData(evaluatorForMethodArgs()
                    , "\",\".UseAsSepForJoin(2, 3.5, \"Hello\", true)"
                    , null)
                    .Returns("2,3.5,Hello,True")
                    .SetCategory("ParamsKeywordMethod");

                yield return new TestCaseData(evaluatorForMethodArgs()
                    , "\",\".UseAsSepForJoin(2, 3.5, null, \"Hello\", true)"
                    , null)
                    .Returns("2,3.5,,Hello,True")
                    .SetCategory("ParamsKeywordMethod");

                yield return new TestCaseData(evaluatorForMethodArgs()
                    , "paramsObj.Join(out x, \",\", 2, 3.5, \"Hello\", true) ?? x"
                    , null)
                    .Returns("2,3.5,Hello,True")
                    .SetCategory("ParamsKeywordMethod");

                yield return new TestCaseData(evaluatorForMethodArgs()
                    , "paramsObj.Join(out x, \",\", 2, 3.5, null, \"Hello\", true) ?? x"
                    , null)
                    .Returns("2,3.5,,Hello,True")
                    .SetCategory("ParamsKeywordMethod");

                yield return new TestCaseData(evaluatorForMethodArgs()
                    , "\",\".UseAsSepForJoin(ref string x, 2, 3.5, \"Hello\", true) ?? x"
                    , null)
                    .Returns("2,3.5,Hello,True")
                    .SetCategory("ParamsKeywordMethod");

                yield return new TestCaseData(evaluatorForMethodArgs()
                    , "\",\".UseAsSepForJoin(ref x, 2, 3.5, null, \"Hello\", true) ?? x"
                    , null)
                    .Returns("2,3.5,,Hello,True")
                    .SetCategory("ParamsKeywordMethod");

                yield return new TestCaseData(evaluatorForMethodArgs()
                    , "paramsObj.ReturnTrue(2)"
                    , null)
                    .Returns(true)
                    .SetCategory("ParamsKeywordMethod");

                yield return new TestCaseData(evaluatorForMethodArgs()
                    , "paramsObj.ReturnTrue(2, \"Hello\")"
                    , null)
                    .Returns(true)
                    .SetCategory("ParamsKeywordMethod");

                yield return new TestCaseData(evaluatorForMethodArgs()
                    , "paramsObj.ReturnTrue(2, \"Hello\", \"Test\")"
                    , null)
                    .Returns(true)
                    .SetCategory("ParamsKeywordMethod");

                #endregion

                #region Method parameters with default value

                yield return new TestCaseData(evaluatorForMethodArgs()
                    , "paramsObj.GetTheDefaultValue()"
                    , null)
                    .Returns(10)
                    .SetCategory("DefaultValueMethod");

                yield return new TestCaseData(evaluatorForMethodArgs()
                    , "paramsObj.GetTheDefaultValue(15)"
                    , null)
                    .Returns(15)
                    .SetCategory("DefaultValueMethod");

                yield return new TestCaseData(evaluatorForMethodArgs()
                    , "paramsObj.GetTheDefaultValue(\"default value is \")"
                    , null)
                    .Returns("default value is 20")
                    .SetCategory("DefaultValueMethod");

                yield return new TestCaseData(evaluatorForMethodArgs()
                    , "paramsObj.GetTheDefaultValue(\"given value is \", 25)"
                    , null)
                    .Returns("given value is 25")
                    .SetCategory("DefaultValueMethod");

                yield return new TestCaseData(evaluatorForMethodArgs()
                    , "paramsObj.SumOf(out r) ?? r"
                    , null)
                    .Returns("default value is 30")
                    .SetCategory("DefaultValueMethod");

                yield return new TestCaseData(evaluatorForMethodArgs()
                    , "paramsObj.SumOf(out r, \"half default value is \") ?? r"
                    , null)
                    .Returns("half default value is 30")
                    .SetCategory("DefaultValueMethod");

                yield return new TestCaseData(evaluatorForMethodArgs()
                    , "paramsObj.SumOf(out r, \"given value is \", 35) ?? r"
                    , null)
                    .Returns("given value is 35")
                    .SetCategory("DefaultValueMethod");

                yield return new TestCaseData(evaluatorForMethodArgs()
                    , "paramsObj.SumOf()"
                    , null)
                    .Returns(40)
                    .SetCategory("DefaultValueMethod");

               yield return new TestCaseData(evaluatorForMethodArgs()
                    , "paramsObj.SumOf(22)"
                    , null)
                    .Returns(43)
                    .SetCategory("DefaultValueMethod");

              yield return new TestCaseData(evaluatorForMethodArgs()
                    , "paramsObj.SumOf(22, 23)"
                    , null)
                    .Returns(45)
                    .SetCategory("DefaultValueMethod");

                #endregion

                #region Issues/Bug resolution

                yield return new TestCaseData(new ExpressionEvaluator()
                    , "(new List<Regex>()).GetType()"
                    , null)
                    .Returns(typeof(List<Regex>))
                    .SetCategory("Bug resolution");

                yield return new TestCaseData(new ExpressionEvaluator()
                    {
                        OptionCaseSensitiveEvaluationActive = false
                    }
                    , "Int32.Parse(\"2\")"
                    , null)
                    .Returns(2)
                    .SetCategory("Bug resolution");

                #region For bug #65
                var Persons = new List<Person2>() { new Person2() { Code = "QT00010", Name = "Pedrito", Number = 11.11m },
                    new Person2() { Code = "QT00011", Name = "Pablito", Number = 12.11m }};

                yield return new TestCaseData(new ExpressionEvaluator() { Context = new { Persons } }
                    , "Persons.Sum(x=>x.Number)"
                    , null)
                    .Returns(23.22m)
                    .SetCategory("Bug resolution")
                    .SetCategory("#65");

                yield return new TestCaseData(new ExpressionEvaluator() { Context = new { Persons } }
                    , "Persons.Average(x=>x.Number)"
                    , null)
                    .Returns(11.61m)
                    .SetCategory("Bug resolution")
                    .SetCategory("#65");

                yield return new TestCaseData(new ExpressionEvaluator() { Context = new { Persons } }
                    , "Persons.Max(x=>x.Number)"
                    , null)
                    .Returns(12.11m)
                    .SetCategory("Bug resolution")
                    .SetCategory("#65");

                yield return new TestCaseData(new ExpressionEvaluator() { Context = new { Persons } }
                    , "Persons.Min(x=>x.Number)"
                    , null)
                    .Returns(11.11m)
                    .SetCategory("Bug resolution")
                    .SetCategory("#65");

                #endregion

                #region For Issue Manage nested class and enum #95

                yield return new TestCaseData(new ExpressionEvaluator()
                    , "Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData)"
                    , null)
                    .Returns(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData))
                    .SetCategory("Bug resolution")
                    .SetCategory("#95")
                    .SetCategory("NestedType");

                yield return new TestCaseData(new ExpressionEvaluator()
                    , "Environment.GetFolderPath((Environment.SpecialFolder)5)"
                    , null)
                    .Returns(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments))
                    .SetCategory("Bug resolution")
                    .SetCategory("#95")
                    .SetCategory("NestedType");

                yield return new TestCaseData(new ExpressionEvaluator()
                    , "new CodingSeb.ExpressionEvaluator.Tests.OtherNamespace.ClassInOtherNameSpace1.ANestedClass().Value1"
                    , null)
                    .Returns(45)
                    .SetCategory("Bug resolution")
                    .SetCategory("#95")
                    .SetCategory("NestedType");

                #endregion

                #region For issue #98 Evaluate methods names as delegates

                yield return new TestCaseData(new ExpressionEvaluator()
                    , "Array.ConvertAll(\"1,2,3,4,5,6,-1\".Split(','), Int32.Parse).Min()"
                    , null)
                    .Returns(-1)
                    .SetCategory("Bug resolution")
                    .SetCategory("#95")
                    .SetCategory("MethodNameAsDelegates");

                yield return new TestCaseData(new ExpressionEvaluator()
                    , "Array.ConvertAll<string,int>(\"1,2,3,4,5,6,-1\".Split(','), Int32.Parse).Min()"
                    , null)
                    .Returns(-1)
                    .SetCategory("Bug resolution")
                    .SetCategory("#95")
                    .SetCategory("MethodNameAsDelegates");

                yield return new TestCaseData(new ExpressionEvaluator()
                    , "Array.ConvertAll(\"1,2,3,4,5,6,-1\".Split(','), s => Int32.Parse(s)).Min()"
                    , null)
                    .Returns(-1)
                    .SetCategory("Bug resolution")
                    .SetCategory("#95")
                    .SetCategory("MethodNameAsDelegates");

                yield return new TestCaseData(new ExpressionEvaluator()
                    , "Array.ConvertAll<string, int>(\"1,2,3,4,5,6,-1\".Split(','), s => Int32.Parse(s)).Min()"
                    , null)
                    .Returns(-1)
                    .SetCategory("Bug resolution")
                    .SetCategory("#95")
                    .SetCategory("MethodNameAsDelegates");

                yield return new TestCaseData(new ExpressionEvaluator()
                    , "Array.ConvertAll(\"test for Upper\".ToCharArray(), Char.IsUpper)"
                    , null)
                    .Returns(Array.ConvertAll("test for Upper".ToCharArray(), Char.IsUpper))
                    .SetCategory("Bug resolution")
                    .SetCategory("#95")
                    .SetCategory("MethodNameAsDelegates");

                yield return new TestCaseData(new ExpressionEvaluator()
                    , "(() => { var m = int.Parse; return m(\"5\"); })()"
                    , null)
                    .Returns(5)
                    .SetCategory("Bug resolution")
                    .SetCategory("#95")
                    .SetCategory("MethodNameAsDelegates");

              yield return new TestCaseData(new ExpressionEvaluator()
                    , "\"test for Upper\".ToCharArray().First(Char.IsUpper)"
                    , null)
                    .Returns('U')
                    .SetCategory("Bug resolution")
                    .SetCategory("#95")
                    .SetCategory("MethodNameAsDelegates");

              yield return new TestCaseData(new ExpressionEvaluator()
                    , "\"test for Upper\".ToCharArray().First(Char.IsUpper)"
                    , null)
                    .Returns('U')
                    .SetCategory("Bug resolution")
                    .SetCategory("#95")
                    .SetCategory("MethodNameAsDelegates");

              yield return new TestCaseData(new ExpressionEvaluator()
                    , "\"test for Upper\".ToCharArray().First(c => Char.IsUpper(c))"
                    , null)
                    .Returns('U')
                    .SetCategory("Bug resolution")
                    .SetCategory("#95")
                    .SetCategory("MethodNameAsDelegates");

              yield return new TestCaseData(new ExpressionEvaluator()
                    , "Array.Find(\"test for Upper\".ToCharArray(), Char.IsUpper)"
                    , null)
                    .Returns('U')
                    .SetCategory("Bug resolution")
                    .SetCategory("#95")
                    .SetCategory("MethodNameAsDelegates");

              yield return new TestCaseData(new ExpressionEvaluator()
                    , "Array.Find(\"test for Upper\".ToCharArray(), c => Char.IsUpper(c))"
                    , null)
                    .Returns('U')
                    .SetCategory("Bug resolution")
                    .SetCategory("#95")
                    .SetCategory("MethodNameAsDelegates");

              yield return new TestCaseData(new ExpressionEvaluator()
                    , "\"test for Upper\".ToCharArray().Any(Char.IsUpper)"
                    , null)
                    .Returns(true)
                    .SetCategory("Bug resolution")
                    .SetCategory("#95")
                    .SetCategory("MethodNameAsDelegates");

              yield return new TestCaseData(new ExpressionEvaluator()
                    , "\"test for Upper\".ToCharArray().ToList().First(Char.IsUpper)"
                    , null)
                    .Returns('U')
                    .SetCategory("Bug resolution")
                    .SetCategory("#95")
                    .SetCategory("MethodNameAsDelegates");

                #endregion

                #region for issue #100 Array types in cast or typeof generate an exception

                yield return new TestCaseData(new ExpressionEvaluator()
                    , "typeof(double[])"
                    , null)
                    .Returns(typeof(double[]))
                    .SetCategory("#100")
                    .SetCategory("Bug resolution");

                yield return new TestCaseData(new ExpressionEvaluator()
                    , "typeof(double[ ])"
                    , null)
                    .Returns(typeof(double[]))
                    .SetCategory("#100")
                    .SetCategory("Bug resolution");

                yield return new TestCaseData(new ExpressionEvaluator()
                    , "typeof(double[][])"
                    , null)
                    .Returns(typeof(double[][]))
                    .SetCategory("#100")
                    .SetCategory("Bug resolution");

                yield return new TestCaseData(new ExpressionEvaluator()
                    , "typeof(double[,])"
                    , null)
                    .Returns(typeof(double[,]))
                    .SetCategory("#100")
                    .SetCategory("Bug resolution");

                yield return new TestCaseData(new ExpressionEvaluator()
                    , "typeof(int[])"
                    , null)
                    .Returns(typeof(int[]))
                    .SetCategory("#100")
                    .SetCategory("Bug resolution");

                yield return new TestCaseData(new ExpressionEvaluator()
                    , "typeof(Int32[])"
                    , null)
                    .Returns(typeof(Int32[]))
                    .SetCategory("#100")
                    .SetCategory("Bug resolution");

                yield return new TestCaseData(new ExpressionEvaluator()
                    , "typeof(string[])"
                    , null)
                    .Returns(typeof(string[]))
                    .SetCategory("#100")
                    .SetCategory("Bug resolution");

                yield return new TestCaseData(new ExpressionEvaluator()
                    , "typeof(Regex[])"
                    , null)
                    .Returns(typeof(Regex[]))
                    .SetCategory("#100")
                    .SetCategory("Bug resolution");

                yield return new TestCaseData(new ExpressionEvaluator(new ObjectContainer()
                    {
                        AnObjectProperty = new double[] {1.1, 2.3, 4.3}
                    })
                    , "(double[])AnObjectProperty"
                    , null)
                    .Returns(new double[] { 1.1, 2.3, 4.3 })
                    .SetCategory("#100")
                    .SetCategory("Bug resolution");

                #endregion

                #region for issue #110 OptionForceIntegerNumbersEvaluationsAsDoubleByDefault leads to exception when dividing

                yield return new TestCaseData(new ExpressionEvaluator()
                    {
                        OptionForceIntegerNumbersEvaluationsAsDoubleByDefault = true,
                    }
                    , "3/Math.Round(Avg(1,2),MidpointRounding.AwayFromZero)"
                    , null)
                    .Returns(1.5d)
                    .SetCategory("#110")
                    .SetCategory("Bug resolution");

                yield return new TestCaseData(new ExpressionEvaluator()
                    {
                        OptionForceIntegerNumbersEvaluationsAsDoubleByDefault = true,
                    }
                    , "Math.Round(1.5,MidpointRounding.AwayFromZero)"
                    , null)
                    .Returns(2d)
                    .SetCategory("#110")
                    .SetCategory("Bug resolution");

                yield return new TestCaseData(new ExpressionEvaluator()
                    {
                        OptionForceIntegerNumbersEvaluationsAsDoubleByDefault = true,
                    }
                    , "Avg(1,2)"
                    , null)
                    .Returns(1.5d)
                    .SetCategory("#110")
                    .SetCategory("Bug resolution");

                #endregion

                #region For multidimensional indexing

                yield return new TestCaseData(new ExpressionEvaluator(new Dictionary<string, object> { { "doubleIndexObject", new ClassForIndexing() } })
                    , "doubleIndexObject[2,3]"
                    , null)
                    .Returns(5)
                    .SetCategory("Multidimensional indexing");

                #endregion

                #endregion
            }
        }

        #endregion

        [TestCaseSource(nameof(TestCasesEvaluateWithSpecificEvaluator))]
        public object EvaluateWithSpecificEvaluator(ExpressionEvaluator evaluator, string expression, Func<Exception, object> inCaseOfException)
        {
            try
            {
                return evaluator.Evaluate(expression);
            }
            catch (Exception exception) when (inCaseOfException != null)
            {
                return inCaseOfException(exception);
            }
        }

        #endregion

        #region With SubExpressions variables

        [Test]
        public void EvaluateWithSubExpressionsVariables()
        {
            ExpressionEvaluator evaluator = new ExpressionEvaluator();

            evaluator.Variables["a"] = 1;
            evaluator.Variables["b"] = 2;
            evaluator.Variables["c"] = new SubExpression("a+b");
            evaluator.Variables["d"] = new SubExpression("c+3");

            evaluator.Evaluate("a").ShouldBe(1);
            evaluator.Evaluate("b").ShouldBe(2);
            evaluator.Evaluate("c").ShouldBe(3);
            evaluator.Evaluate("d").ShouldBe(6);
            evaluator.Evaluate("d-a").ShouldBe(5);
        }

        #endregion

        #region Method with out parameter

        [Test]
        [Category("OutKeywordMethod")]
        public void MethodWithOutParameter()
        {
            ExpressionEvaluator evaluator = new ExpressionEvaluator();

            evaluator.Variables["myDict"] = new Dictionary<string, object>
            {
                { "ANumber", 10 },
            };

            evaluator.Variables["x"] = null;

            evaluator.Evaluate<bool>("myDict.TryGetValue(\"ANumber\",out x)").ShouldBeTrue();

            evaluator.Variables.ShouldContainKey("x");
            evaluator.Variables["x"]
                .ShouldBeOfType<int>()
                .ShouldBe(10);
        }

        [Test]
        [Category("OutKeywordMethod")]
        public void MethodWithOutParameterWithoutExistingVariable()
        {
            ExpressionEvaluator evaluator = new ExpressionEvaluator();

            evaluator.Variables["myDict"] = new Dictionary<string, object>
            {
                { "ANumber", 10 },
            };

            evaluator.Evaluate<bool>("myDict.TryGetValue(\"ANumber\",out x)").ShouldBeTrue();

            evaluator.Variables.ShouldContainKey("x");
            evaluator.Variables["x"]
                .ShouldBeOfType<int>()
                .ShouldBe(10);
        }

        [Test]
        [Category("OutKeywordMethod")]
        public void MethodWithOutParameterInlineVarDeclaration()
        {
            ExpressionEvaluator evaluator = new ExpressionEvaluator();

            evaluator.Variables["myDict"] = new Dictionary<string, object>
            {
                { "ANumber", 10 },
            };

            evaluator.Evaluate<bool>("myDict.TryGetValue(\"ANumber\",out int x)").ShouldBeTrue();

            evaluator.Variables.ShouldContainKey("x");
            evaluator.Variables["x"]
                .ShouldBeOfType<StronglyTypedVariable>()
                .Value.ShouldBe(10);
        }

        #endregion

        #region Method with ref parameter

        [Test]
        [Category("RefKeywordMethod")]
        public void MethodWithRefParameter()
        {
            ExpressionEvaluator evaluator = new ExpressionEvaluator();

            evaluator.Variables["refObj"] = new MethodArgKeywordClass();

            evaluator.Variables["x"] = 3;

            evaluator.Evaluate("refObj.AddOneOnRef(ref x)");

            evaluator.Variables.ShouldContainKey("x");
            evaluator.Variables["x"]
                .ShouldBeOfType<int>()
                .ShouldBe(4);
        }

        [Test]
        [Category("RefKeywordMethod")]
        public void MethodWithRefParameterWithoutExistingVariable()
        {
            ExpressionEvaluator evaluator = new ExpressionEvaluator();

            evaluator.Variables["refObj"] = new MethodArgKeywordClass();

            evaluator.Evaluate("refObj.AddOneOnRef(ref x)");

            evaluator.Variables.ShouldContainKey("x");
            evaluator.Variables["x"]
                .ShouldBeOfType<int>()
                .ShouldBe(1);
        }

        [Test]
        [Category("RefKeywordMethod")]
        public void MethodWithRefParameterInlineVarDeclaration()
        {
            ExpressionEvaluator evaluator = new ExpressionEvaluator();

            evaluator.Variables["refObj"] = new MethodArgKeywordClass();

            evaluator.Evaluate("refObj.AddOneOnRef(ref int x)");

            evaluator.Variables.ShouldContainKey("x");
            evaluator.Variables["x"]
                .ShouldBeOfType<StronglyTypedVariable>()
                .Value.ShouldBe(1);
        }

        [Test]
        [Category("RefKeywordMethod")]
        public void MethodWithRefParameterWithoutExistingVariableAndInit()
        {
            ExpressionEvaluator evaluator = new ExpressionEvaluator();

            evaluator.Variables["refObj"] = new MethodArgKeywordClass();

            evaluator.Evaluate("refObj.AddOneOnRef(ref x = 4)");

            evaluator.Variables.ShouldContainKey("x");
            evaluator.Variables["x"]
                .ShouldBeOfType<int>()
                .ShouldBe(5);
        }

        [Test]
        [Category("RefKeywordMethod")]
        public void MethodWithRefParameterInlineVarDeclarationAndInit()
        {
            ExpressionEvaluator evaluator = new ExpressionEvaluator();

            evaluator.Variables["refObj"] = new MethodArgKeywordClass();

            evaluator.Evaluate("refObj.AddOneOnRef(ref int x = 6)");

            evaluator.Variables.ShouldContainKey("x");
            evaluator.Variables["x"]
                .ShouldBeOfType<StronglyTypedVariable>()
                .Value.ShouldBe(7);
        }

        #endregion

        #region Method with in parameter

        [Test]
        [Category("InKeywordMethod")]
        public void MethodWithInParameter()
        {
            ExpressionEvaluator evaluator = new ExpressionEvaluator();

            evaluator.Variables["inObj"] = new MethodArgKeywordClass();

            evaluator.Variables["x"] = 3;

            evaluator.Evaluate("inObj.AddOneOnIn(in x)")
                .ShouldBe(4);

            evaluator.Variables.ShouldContainKey("x");
            evaluator.Variables["x"]
                .ShouldBeOfType<int>()
                .ShouldBe(3);
        }

        [Test]
        [Category("InKeywordMethod")]
        public void MethodWithInParameterWithoutExistingVariable()
        {
            ExpressionEvaluator evaluator = new ExpressionEvaluator();

            evaluator.Variables["inObj"] = new MethodArgKeywordClass();

            evaluator.Evaluate("inObj.AddOneOnIn(in x)")
                .ShouldBe(1);

            evaluator.Variables.ShouldContainKey("x");
            evaluator.Variables["x"]
                .ShouldBeNull();
        }

        [Test]
        [Category("InKeywordMethod")]
        public void MethodWithInParameterInlineVarDeclaration()
        {
            ExpressionEvaluator evaluator = new ExpressionEvaluator();

            evaluator.Variables["inObj"] = new MethodArgKeywordClass();

            evaluator.Evaluate("inObj.AddOneOnIn(in int x)")
                .ShouldBe(1);

            evaluator.Variables.ShouldContainKey("x");
            evaluator.Variables["x"]
                .ShouldBeOfType<StronglyTypedVariable>()
                .Value.ShouldBe(0);
        }

        [Test]
        [Category("InKeywordMethod")]
        public void MethodWithInParameterWithoutExistingVariableAndInit()
        {
            ExpressionEvaluator evaluator = new ExpressionEvaluator();

            evaluator.Variables["inObj"] = new MethodArgKeywordClass();

            evaluator.Evaluate("inObj.AddOneOnIn(in x = 4)")
                .ShouldBe(5);

            evaluator.Variables.ShouldContainKey("x");
            evaluator.Variables["x"]
                .ShouldBeOfType<int>()
                .ShouldBe(4);
        }

        [Test]
        [Category("InKeywordMethod")]
        public void MethodWithInParameterInlineVarDeclarationAndInit()
        {
            ExpressionEvaluator evaluator = new ExpressionEvaluator();

            evaluator.Variables["inObj"] = new MethodArgKeywordClass();

            evaluator.Evaluate("inObj.AddOneOnIn(in int x = 6)")
                .ShouldBe(7);

            evaluator.Variables.ShouldContainKey("x");
            evaluator.Variables["x"]
                .ShouldBeOfType<StronglyTypedVariable>()
                .Value.ShouldBe(6);
        }

        #endregion

        #region Method with implicit parameter

        [Test]
        [Category("ImplicitParameterMethod")]
        public void MethodWithImplicitParameter()
        {
            ExpressionEvaluator evaluator = new ExpressionEvaluator();

            evaluator.Variables["inObj"] = new MethodArgKeywordClass();

            evaluator.Variables["x"] = "string";

            evaluator.Evaluate("inObj.AcceptStringLike(x)")
                .ShouldBe(true);
        }

        #endregion
    }
}
