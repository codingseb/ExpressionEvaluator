# ExpressionEvaluator

A Simple Math and Pseudo C# Expression Evaluator in One C# File.

And now can execute small C# like scripts 

It is largely based on and inspired by the following resources [this post on stackoverflow](http://stackoverflow.com/questions/333737/evaluating-string-342-yield-int-18/333749), [NCalc](https://ncalc.codeplex.com/) and [C# Operators](https://msdn.microsoft.com/en-us/library/6a71f45d.aspx)

## Status

|Branch|Status|
|---|---|
|master|[![Build Status](https://coding-seb.visualstudio.com/_apis/public/build/definitions/cbe8d2f2-9c7a-48aa-8366-89ef39381eff/1/badge)](https://coding-seb.visualstudio.com/ExpressionEvaluator/_build/index?definitionId=1)|
|dev|[![Dev Status](https://coding-seb.visualstudio.com/_apis/public/build/definitions/cbe8d2f2-9c7a-48aa-8366-89ef39381eff/2/badge)](https://coding-seb.visualstudio.com/ExpressionEvaluator/_build/index?definitionId=2)|
|nuget|[![NuGet Status](http://img.shields.io/nuget/v/CodingSeb.ExpressionEvaluator.svg?style=flat&max-age=86400)](https://www.nuget.org/packages/CodingSeb.ExpressionEvaluator/)|

## Features
* Basic mathematical expression evaluation
* System.Math methods and constants directly available (some like Max, Min, Avg are improved)
* Some useful [functions](#standard-functions) for example to create List and Arrays
* [Custom variables definition](#custom-variables)
* [On the fly variables and functions evaluation](#on-the-fly-variables-and-functions-evaluation) (To easily extend possibilities, Manage also on instance Property and Methods)
* [A large set of C# operators availables](#operators)
* Instance and static methods and properties access like as in C#
* You can call Methods and/or Properties on your own classes (just pass a object as custom variables)
* [C# primary types](#primary-types)
* Use strings as in C# (@"", $"", $@"" available)
* Lambda expressions
* Classes like File, Directory, Regex, List ... available ([You can extend the list of Namespaces](#namespaces))
* Create instance with [new(MyClassName, constructorArgs)](#standard-functions) or [new MyClassName(constructorArgs)](#operators)
* [Call void methods with fluid prefix convention to chain operations](#go-fluid-with-a-simple-methods-prefixing-convention)
* Manage now assignation operators like =, +=, -=, *= ... (On variables and sub properties)
* Manage now postfix operators ++ and -- (On variables and sub properties)

## And with [ScriptEvaluate](#scripts)
* Small C# like script evaluation (Multi expressions separated by ;)
* Some conditional and loop blocks [keywords](#script-keywords) (if, while, for ...)
* Multi-line (multi expression) Lambda expressions.

## Getting started

Install the following nuget package : 

```
Install-Package CodingSeb.ExpressionEvaluator
```
[See on Nuget.org](https://www.nuget.org/packages/CodingSeb.ExpressionEvaluator/)

or copy the [CodingSeb.ExpressionEvaluator/ExpressionEvaluator.cs](./CodingSeb.ExpressionEvaluator/ExpressionEvaluator.cs) in your project :

## Basic C# usage

### Simple expressions

```c#
using CodingSeb.ExpressionEvaluator;
//...
string expression;
//...
ExpressionEvaluator evaluator = new ExpressionEvaluator();
Console.WriteLine(expression);
Console.WriteLine(evaluator.Evaluate(expression));
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

Array(2, $"Test { 2 + 2 } U", true)[2] ? "yes" : "no"
yes

false ? "yes" : "no"
no

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

// use new as a function
new(Random).Next(1,10)
4 // or a random value between 1 and 9

// or as a keyword
new Regex(@"\w*[n]\w*").Match("Which word contains the desired letter ?").Value
contains

List("Test", "Hello", "Bye", "How are you?").Find(t => t.Length < 4)
Bye

Enumerable.Range(1,4).Cast().Sum(x =>(int)x)
10

Enumerable.Repeat(3,6).Cast().ToList().Count
6

Enumerable.Repeat(3,6).Cast().ToList()[4]
3

((x, y) => x * y)(4, 2)
8

"Hello"[2] == 'l'
true
```

### Small scripts

```c#
using CodingSeb.ExpressionEvaluator;
//...
string script;
//...
ExpressionEvaluator evaluator = new ExpressionEvaluator();
Console.WriteLine("--------------------------------------------");
Console.WriteLine(script);
Console.WriteLine("---------------- Result --------------------");
Console.WriteLine(evaluator.ScriptEvaluate(script));
```
Results with some scripts :

```
--------------------------------------------
x = 0;
result = "";

while(x < 5)
{
    result += $"{x},";
    x++;
}

result.Remove(result.Length - 1);
---------------- Result --------------------
0,1,2,3,4

--------------------------------------------
result = "";

for(x = 0; x < 5;x++)
{
    result += $"{x},";
}

result.Remove(result.Length - 1);
---------------- Result --------------------
0,1,2,3,4

--------------------------------------------
x = 0;
y = 1;
result = 0;

if(y != 0)
{
    return 1;
}
else if(x == 0)
{
    return 2;
}
else if(x < 0)
{
    return 3;
}
else
{
    return 4;
}
---------------- Result --------------------
1

--------------------------------------------
x = 0;
y = 0;
result = 0;

if(y != 0)
{
    return 1;
}
else if(x == 0)
{
    return 2;
}
else if(x < 0)
{
    return 3;
}
else
{
    return 4;
}
---------------- Result --------------------
2

--------------------------------------------
x = 5;
y = 0;
result = 0;

if(y != 0)
{
    return 1;
}
else if(x == 0)
{
    return 2;
}
else if(x < 0)
{
    return 3;
}
else
{
    return 4;
}
---------------- Result --------------------
4
```

To see more scripts examples see scripts uses for tests in sub directories [CodingSeb.ExpressionEvaluator.Tests/Resources](./CodingSeb.ExpressionEvaluator.Tests/Resources)

## Standard constants (variables)

The evaluation of variables name is case insensitive so you can write it as you want.

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
  { "myArray", new object[] { 3.5, "Test", false}
};
```
```
x+y
-1.1

myVar + " !!!"
Hello World !!!

myArray.Length
3

myArray[0]
3.5

myArray[1].Length
4

myArray[2] || true
True
```

## Standard functions
The following functions are internally defined. (Most of these are [System.Math Methods](https://msdn.microsoft.com/en-us/library/system.math(v=vs.110).aspx) directly accessible)

|Name|Description|Example|Result|
|---|---|---|---|
|**[Abs](https://msdn.microsoft.com/en-us/library/f2yzeea2(v=vs.110).aspx)**(double number)|Return a double that is the absolute value of number|`Abs(-3.2d)`|`3.2d`|
|**[Acos](https://msdn.microsoft.com/en-us/library/system.math.acos(v=vs.110).aspx)**(double d)|Return a double value that is the angle in radian whose d is the cosine<br/>d must be betwteen -1 and 1|`Acos(-0.5d)`|`2.0943951023032d`|
|**Array**(object obj1, object obj2 ,...)|Return a array (System.Object[]) of all given arguments|`Array(1, "Hello", true)`|`new object[]{1, "Hello", true}`|
|**[Asin](https://msdn.microsoft.com/en-us/library/system.math.asin(v=vs.110).aspx)**(double d)|Return a double value that is the angle in radian whose d is the sine<br/>d must be betwteen -1 and 1|`Asin(-0.2d)`|`0.304692654015398d`|
|**[Atan](https://msdn.microsoft.com/en-us/library/system.math.atan(v=vs.110).aspx)**(double d)|Return a double value that is the angle in radian whose d is the tangent|`Atan(2.1)`|`1.1263771168938d`|
|**[Atan2](https://msdn.microsoft.com/en-us/library/system.math.atan2(v=vs.110).aspx)**(double x, double y)|Return a double value that is the angle in radian whose the tangente is the quotient of x and y<br/>|`Atan2(2.1d, 3.4d)`|`0.553294325322293d`|
|**Avg**(double nb1, double nb2 ,...)|Return a double value that is the average value of all given arguments|`Avg(1, 2.5, -4, 6.2)`|`1.425d`|
|**[Ceiling](https://msdn.microsoft.com/en-us/library/zx4t0t48(v=vs.110).aspx)**(double a)|Return a double value that is the smallest integer greater than or equal to the specified number.|`Ceiling(4.23d)`|`5d`|
|**[Cos](https://msdn.microsoft.com/en-us/library/system.math.cos(v=vs.110).aspx)**(double angle)|Return a double value that is the cosine of the specified angle in radian|`Cos(2 * Pi)`|`1d`|
|**[Cosh](https://msdn.microsoft.com/en-us/library/system.math.cosh(v=vs.110).aspx)**(double angle)|Return a double value that is the hyperbolic cosine of the specified angle in radian|`Cosh(2d)`|`3.76219569108363d`|
|**[Exp](https://msdn.microsoft.com/en-us/library/system.math.exp(v=vs.110).aspx)**(double d)|Return a double value that is e raised to the specified d power|`Exp(3d)`|`20.0855369231877d`|
|**[Floor](https://msdn.microsoft.com/en-us/library/e0b5f0xb(v=vs.110).aspx)**(double d)|Return a double value that is the largest integer less than or equal to the specified d argument|`Floor(4.23d)`|`4d`|
|**[IEEERemainder](https://msdn.microsoft.com/en-us/library/system.math.ieeeremainder(v=vs.110).aspx)**(double x, double y)|Return a double value that is the remainder resulting from the division of x by y|`IEEERemainder(9, 8)`|`1d`|
|**in**(object valueToFind, object obj1, object obj2...)|Return a boolean value that indicate if the first argument is found in the other arguments|`in(8, 4, 2, 8)`|`true`|
|**List**(object obj1, object obj2 ,...)|Return a List (System.Collections.Generic.List<object>) of all given arguments|`List(1, "Hello", true)`|`new List<object>(){1, "Hello", true}`|
|**[Log](https://msdn.microsoft.com/en-us/library/system.math.log(v=vs.110).aspx)**(double a, double base)|Return a double value that is the logarithm of a in the specified base|`Log(64d, 2d)`|`6d`|
|**[Log10](https://msdn.microsoft.com/en-us/library/system.math.log10(v=vs.110).aspx)**(double a)|Return a double value that is the base 10 logarithm of a specified a|`Log10(1000d)`|`3d`|
|**Max**(double nb1, double nb2 ,...)|Return a double value that is the maximum value of all given arguments|`Max(1d, 2.5d, -4d)`|`2.5d`|
|**Min**(double nb1, double nb2 ,...)|Return a double value that is the minimum value of all given arguments|`Min(1d, 2.5d, -4d)`|`-4d`|
|**new**(TypeOrClass, constructorArg1, constructorArg2 ...)|Create an instance of the specified class as first argument and return it. A optional list of additional arguments can be passed as constructor arguments|`new(Random).next(0,10)`|`5d // or a random value between 1 and 9`|
|**[Pow](https://msdn.microsoft.com/en-us/library/system.math.pow(v=vs.110).aspx)**(double x, double y)|Return a double value that is x elevate to the power y|`Pow(2,4)`|`16d`|
|**[Round](https://msdn.microsoft.com/en-us/library/system.math.round(v=vs.110).aspx)**(double d, (optional) int digits, (optional) [MidpointRounding](https://msdn.microsoft.com/en-us/library/system.midpointrounding(v=vs.110).aspx) mode)|Rounds d to the nearest integer or specified number of decimal places.|`Round(2.432,1)`|`2.4d`|
|**[Sign](https://msdn.microsoft.com/en-us/library/system.math.sign(v=vs.110).aspx)**(double d)|Return 1,-1 or 0 indicating the sign of d|`Sign(-12)`|`-1d`|
|**[Sin](https://msdn.microsoft.com/en-us/library/system.math.sin(v=vs.110).aspx)**(double angle)|Return a double value that is the sine of the specified angle in radian|`Sin(Pi/2)`|`1d`|
|**[Sinh](https://msdn.microsoft.com/en-us/library/system.math.sinh(v=vs.110).aspx)**(double angle)|Return a double value that is the hyperbolic sine of the specified angle in radian|`Sinh(2d)`|`3.62686040784702d`|
|**[Sqrt](https://msdn.microsoft.com/en-us/library/system.math.sqrt(v=vs.110).aspx)**(double d)|Return a double value that is the square root of the specified d value|`Sqrt(4d)`|`2d`|
|**[Tan](https://msdn.microsoft.com/en-us/library/system.math.tan(v=vs.110).aspx)**(double angle)|Return a double value that is the tangent of the specified angle in radian|`Tan(Pi / 4)`|`1d`|
|**[Tanh](https://msdn.microsoft.com/en-us/library/system.math.tanh(v=vs.110).aspx)**(double angle)|Return a double value that is the hyperbolic tangent of the specified angle in radian|`Tanh(2d)`|`0.964027580075817d`|
|**[Truncate](https://msdn.microsoft.com/en-us/library/c2eabd70(v=vs.110).aspx)**(double d)|Return a double value that is the integer part of the specified d value|`Truncate(2.45d)`|`2d`|


*Remark : The old if function (NCalc style) has been removed. This to avoid conflicts with the new if, else if, else keywords in script mode. To do something similar on a expression level use the conditional operator [( ? : )](#Operators) instead.*

## On the fly variables and functions evaluation
In addition to custom variables, you can add variables and/or functions with on the fly evaluation.
2 C# events are provided that are fired when variables or functions are not fund as standard ones in evaluation time.
Can be use to define or redefine on object instances methods or properties.

```C#
ExpressionEvaluator evaluator = new ExpressionEvaluator();
evaluator.EvaluateVariable += ExpressionEvaluator_EvaluateVariable;
evaluator.EvaluateFunction += ExpressionEvaluator_EvaluateFunction;
//...

private void ExpressionEvaluator_EvaluateVariable(object sender, VariableEvaluationEventArg e)
{
    if(e.Name.ToLower().Equals("myvar"))
    {
        e.Value = 8;
    }
    else if(e.Name.Equals("MultipliedBy2") && e.This is int intValue)
    {
        e.Value = intValue * 2;
    }
}

private void ExpressionEvaluator_EvaluateFunction(object sender, FunctionEvaluationEventArg e)
{
    if(e.Name.ToLower().Equals("sayhello") && e.Args.Count == 1)
    {
        e.Value = $"Hello {e.EvaluateArg(0)}";
    }
    else if(e.Name.Equals("Add") && e.This is int intValue)
    {
        e.Value = intValue + (int)e.EvaluateArg(0);
    }
}
```
```
myVar + 2
10

SayHello("Bob")
Hello Bob

3.MultipliedBy2
6

3.Add(2)
5
```

## Go fluid with a simple methods prefixing convention
Since ExpressionEvaluator evaluate one expression at a time. 
There are cases where we need to use void methods in a fluid syntax manner.

You only need to prefix the method name with "Fluid" or "Fluent"

```C#
// Example Add on List
List("hello", "bye").FluidAdd("test").Count
3

List("hello", "bye").Select(x => x.ToUpper()).ToList().FluentAdd("test")[0]
HELLO

List("hello", "bye").Select(x => x.ToUpper()).ToList().FluentAdd("test")[1]
BYE

List("hello", "bye").Select(x => x.ToUpper()).ToList().FluentAdd("test")[2]
test
```

If needed this fonctionality can be disabled with :

```
evaluator.OptionFluidPrefixingActive = false;
```

## Primary types
ExpressionEvaluator manage the following list of C# primary types

* object
* string
* bool/bool?
* byte/byte?
* char/char?
* decimal/decimal?
* double/double?
* short/short?
* int/int?
* long/long?
* sbyte/sbyte?
* float/float?
* ushort/ushort?
* uint/uint?
* ulong/ulong?
* void

Add the ? for nullable types

## Operators
ExpressionEvaluator manage a large set of C# operators (See [C# Operators](https://msdn.microsoft.com/en-us/library/6a71f45d.aspx))

ExpressionEvaluator respect the C# precedence rules of operators

Here is a list of which operators are supported in ExpressionEvaluator or not

|Type|Operator|Support|
|---|---|---|
|Primary|[x.y](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/operators/member-access-operator)|Supported|
|Primary|[x?.y](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/operators/null-conditional-operators)|Supported|
|Primary|[x?[y]](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/operators/null-conditional-operators)|Supported|
|Primary|[f(x)](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/operators/invocation-operator)|Supported|
|Primary|[a[x]](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/operators/index-operator)|Supported|
|Primary|[x++](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/operators/increment-operator)|Supported **Warning change the state of the postfixed element**|
|Primary|[x--](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/operators/decrement-operator)|Supported **Warning change the state of the postfixed element**|
|Primary|[new](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/new-operator)|Supported you can also use [new() function](#standard-functions)|
|Primary|[typeof](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/typeof)|Supported|
|Primary|[checked](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/checked)|Not Supported|
|Primary|[unchecked](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/unchecked)|Not Supported|
|Primary|[default(T)](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/statements-expressions-operators/default-value-expressions)|Supported|
|Primary|[delegate](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/statements-expressions-operators/anonymous-methods)|Not Supported|
|Primary|[sizeof](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/sizeof)|Not Supported|
|Primary|[->](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/operators/dereference-operator)|Not Supported|
|Unary|[+x](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/operators/addition-operator)|Supported|
|Unary|[-x](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/operators/subtraction-operator)|Supported|
|Unary|[!x](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/operators/logical-negation-operator)|Supported|
|Unary|[~x](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/operators/bitwise-complement-operator)|Not Supported|
|Unary|[++x](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/operators/increment-operator)|Not Supported|
|Unary|[--x](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/operators/decrement-operator)|Not Supported|
|Unary|[(T)x](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/operators/invocation-operator)|Supported|
|Unary|[await](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/await)|Not Supported|
|Unary|[&x](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/operators/and-operator)|Not Supported|
|Unary|[*x](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/operators/multiplication-operator)|Not Supported|
|Multiplicative|[x * y](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/operators/multiplication-operator)|Supported|
|Multiplicative|[x / y](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/operators/division-operator)|Supported|
|Multiplicative|[x % y](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/operators/modulus-operator)|Supported|
|Additive|[x + y](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/operators/addition-operator)|Supported|
|Additive|[x - y](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/operators/subtraction-operator)|Supported|
|Shift|[x << y](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/operators/left-shift-operator)|Supported|
|Shift|[x >> y](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/operators/right-shift-operator)|Supported|
|Relational|[x < y](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/operators/less-than-operator)|Supported|
|Relational|[x > y](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/operators/greater-than-operator)|Supported|
|Relational|[x <= y](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/operators/less-than-equal-operator)|Supported|
|Relational|[x >= y](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/operators/greater-than-equal-operator)|Supported|
|Type-testing|[is](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/is)|Supported|
|Type-testing|[as](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/is)|Not Supported|
|Equality|[x == y](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/operators/equality-comparison-operator)|Supported|
|Equality|[x != y](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/operators/not-equal-operator)|Supported|
|Logical AND|[x & y](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/operators/and-operator)|Supported|
|Logical XOR|[x ^ y](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/operators/xor-operator)|Supported|
|Logical OR|[x &#124; y](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/operators/or-operator)|Supported|
|Conditional AND|[x && y](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/operators/conditional-and-operator)|Supported|
|Conditional OR|[x &#124;&#124; y](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/operators/conditional-or-operator)|Supported|
|Null-coalescing|[x ?? y](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/operators/null-conditional-operator)|Supported|
|Conditional|[t ? x : y](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/operators/conditional-operator)|Supported equivalent to the [if() function](#standard-functions)|
|Lambda|[=>](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/operators/lambda-operator)|Supported|

### Assignation operators

**Warning all of the following operators change the value of their left element.**

Assignation operators (and also postfix operators (++ and --)) are usable on : 

|Elements|What is changing|Options|
|---|---|---|
|Custom variables|The variable in the Variables dictionary is changed and if the variable doesn't exists, it automatically created with the = operator|Can be disabled with ```evaluator.OptionVariableAssignationActive = false;```|
|Properties or fields on objects|If the property/field is not readonly it is changed|Can be disabled with ```evaluator.OptionPropertyOrFieldSetActive = false;```|
|Indexed object like arrays, list or dictionaries|The value at the specified index is changed|Can be disabled with ```evaluator.OptionIndexingAssignationActive = false;```|

Here is the list of available assignation operator

|Operator|Support|
|---|---|
|[=](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/operators/assignment-operator)|Supported (Can be use to declare a new variable that will be injected in the Variables dictionary)|
|[+=](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/operators/addition-assignment-operator)|Supported|
|[-=](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/operators/subtraction-assignment-operator)|Supported|
|[*=](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/operators/multiplication-assignment-operator)|Supported|
|[/=](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/operators/division-assignment-operator)|Supported|
|[%=](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/operators/remainder-assignment-operator)|Supported|
|[&=](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/operators/and-assignment-operator)|Supported|
|[\|=](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/operators/or-assignment-operator)|Supported|
|[^=](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/operators/xor-assignment-operator)|Supported|
|[<<=](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/operators/left-shift-assignment-operator)|Supported|
|[>>=](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/operators/right-shift-assignment-operator)|Supported|

## Scripts
In addition to simple expression evaluation you can also evaluate small script with the method ```ScriptEvaluate(string script)```.
Scripts are just a serie of expressions to evaluate separated with a ; character and leaded by severals additionals keywords.

To declare a variable types are not yet supported and are for now dynamically deduced.

```C#
// Not supported
int x = 2;
string text = "hello";

for(int i = 0; i < 10; i++)
...

// Write this instead :
x = 2;
text = "hello";

for(i = 0; i < 10; i++)
...
```

### Script keywords

Currently the following script keywords are supported

|Type|Operator|Support|
|---|---|---|
|Selection|[if](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/if-else)|Supported|
|Selection|[else if](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/if-else)|Supported|
|Selection|[else](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/if-else)|Supported|
|Selection|[switch case](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/switch)|Not yet supported|
|Iteration|[do ... while](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/do)|Supported|
|Iteration|[for](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/for)|Supported|
|Iteration|[foreach, in](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/foreach-in)|Not yet supported|
|Iteration|[while](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/while)|Supported|
|Jump|[break](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/break)|Supported in do, for and while blocks|
|Jump|[continue](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/continue)|Supported in do, for and while blocks|
|Jump|[goto](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/goto)|Not supported (But if you looked after it -> Booo !!! Bad code)|
|Jump|[return](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/return)|Supported|

*Remark : The way ScriptEvaluate works is to evaluate expressions one by one. There is no syntax check before the evaluation. So be aware that syntax or naming bugs only appears in execution and some code can already be evaluated at this time. Futhermore a syntaxic/naming bug in an if-else block for example can simply be ignored until the corresponding condition is met to evaluate the specific line of code.*

## Code comments
By default comments are not managed in expressions and scripts evaluations.
But they can be manually removed with the specific method ```string RemoveComments(string scriptWithComments)```

To be sure that your commented script is evaluated correctly you can do : 

```C#
ExpressionEvaluator evaluator = new ExpressionEvaluator();
evaluator.ScriptEvaluate(evaluator.RemoveComments(scriptWithComments));
```
It remove line comments // and blocks comments /* ... */

## Namespaces and types
By default the following list of namespaces are available :

* System
* System.Linq
* System.IO
* System.Text
* System.Text.RegularExpressions
* System.ComponentModel
* System.Collections
* System.Collections.Generic
* System.Collections.Specialized
* System.Globalization

You can extend or reduce this list :

```C#
ExpressionEvaluator evaluator = new ExpressionEvaluator();
evaluator.Namespaces.Add(namespace);
evaluator.Namespaces.Remove(namespaceToRemove);
```

All types define in these namespaces are accessibles.

You can also add a specific type :

```C#
evaluator.Types.Add(typeof(MyClass));
```

## Similar projects
### Free
* [NCalc](https://archive.codeplex.com/?p=ncalc) or [NCalc new home](https://github.com/sheetsync/NCalc) 
* [Jint](https://github.com/sebastienros/jint) Support scripting but with Javascript
* [DynamicExpresso](https://github.com/davideicardi/DynamicExpresso/)
* [Flee](https://github.com/mparlak/Flee)
* [CS-Script](https://github.com/oleg-shilo/cs-script) Best alternative (I use it some times) -> Real C# scripts better than ExpressionEvaluator (But everything is compiled. Read the doc. Execution is faster but compilation can make it very slow. And if not done the right way, it can lead to memory leaks)

### Commercial
* [Eval Expression.NET](http://eval-expression.net/)

I would say every C# evaluation libraries have drawbacks and benefits, ExpressionEvaluator is not an exception so choose wisely.

The biggest difference of ExpressionEvaluator is that everything is evaluated on the fly, nothing is compiled or transpile nor in CLR/JIT nor in lambda expressions nor in javascript or other languages stuffs.
So it can be slower in some cases (sometimes not) but it also avoid a lot of memory leaks. 
It already allow to evaluate some small scripts.
And if you don't want an another .dll file in your project, you only need to copy one [C# file](./CodingSeb.ExpressionEvaluator/ExpressionEvaluator.cs) in your project.
