# ExpressionEvaluator
A Simple Math and Pseudo C# Expression Evaluator in One C# File.

It is largely based on and inspired by the following resources [this post on stackoverflow](http://stackoverflow.com/questions/333737/evaluating-string-342-yield-int-18/333749), [NCalc](https://ncalc.codeplex.com/) and [C# Operators](https://msdn.microsoft.com/en-us/library/6a71f45d.aspx)

## Basic usage
```c#
string expression;
...
ExpressionEvaluator evaluator = new ExpressionEvaluator();
Console.Write(expression);
Console.Write(evaluator.Evaluate(expression));
```
Results with some expressions :
```
1+1
2

2 + 3 * 2
8

Pow(2, 4)
16

Sqrt(2) / 3
0.471404520791032

"Hello" + " " + "World"
Hello World

Max(1+1, 2+3, 2*6, Pow(2, 3))
12

Array(2, $"Test { 2 + 2 } U", true).Length
3

Array(2, $"Test { 2 + 2 } U", true)[1]
Test 4 U

if(Array(2, $"Test { 2 + 2 } U", true)[2], "yes" , "no")
yes
```
