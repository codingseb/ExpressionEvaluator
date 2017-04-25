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

(2 + 3) * 2
10

Pi
3.14159265358979

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

"Hello\nWorld"
Hello
World

@"Hello\nWorld"
Hello\nWorld

Regex.Match("Test 34 Hello/-World", @"\d+").Value
34

int.Parse(Regex.Match("Test 34 Hello/-World", @"\d+").Value) + 2
36

3 / 2
1

3d / 2d
1.5

(float)3 / (float)2
1.5
```

## Standard constants (variables)

The evaluation of variables name is case insensitive so you can write it like you want to.

|Constant|Value|Type|
|---|---|---|
|null|C# null value|N/A|
|true|C# true value|System.Boolean|
|false|C# false value|System.Boolean|
|Pi|3.14159265358979|System.Double|
|E|2.71828182845905|System.Double|

## Custom variables

You can define your own variables

Examples : 
```C#
ExpressionEvaluator evaluator = new ExpressionEvaluator();
evaluator.Variables = new Dictionary<string, object>()
{
  { "x", 2,5 },
  { "y", -3.6 },
  { "myVar", "Hello World" }
};
```
```
x+y
-1.1

myVar + " !!!"
Hello World !!!
```

*Remark : custom variable evaluation is case insensitive*
