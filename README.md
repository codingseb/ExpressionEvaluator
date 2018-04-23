# ExpressionEvaluator
A Simple Math and Pseudo C# Expression Evaluator in One C# File.

It is largely based on and inspired by the following resources [this post on stackoverflow](http://stackoverflow.com/questions/333737/evaluating-string-342-yield-int-18/333749), [NCalc](https://ncalc.codeplex.com/) and [C# Operators](https://msdn.microsoft.com/en-us/library/6a71f45d.aspx)

## Features
* Basic mathematical expression evaluation
* System.Math methods and constants directly available (some like Max, Min, Avg are improved)
* Some useful [functions](#standard-functions) for example to create List and Arrays
* [Custom variables definition](#custom-variables)
* [On the fly variables and functions evaluation](#on-the-fly-variables-and-functions-evaluation) (To easily extend possibilities)
* [A large set of C# operators availables](#operators)
* Instance and static methods and properties access like as in C#
* You can call Methods and/or Properties on your own classes (just pass a object as custom variables)
* [C# primary types](#primary-types)
* Use strings as in C# (@"", $"", $@"" available)
* Linq, generics and lambda expressions
* Classes like File, Directory, Regex, List ... available ([You can extend the list of Namespaces](#namespaces))
* Create instance with [new(TypeOrClass, constructorArgs)](#standard-functions)
* [Call void methods with fluid prefix convention to chain operations](#go-fluid-with-a-simple-methods-prefixing-convention)

## Basic C# usage
```c#
string expression;
//...
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

new(Random).next(1,10)
4 // or a random value between 1 and 9

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

```

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

*Remark : custom variable evaluation is case insensitive*

## Standard functions
The following functions are internally defined. (Most of these are [System.Math Methods](https://msdn.microsoft.com/en-us/library/system.math(v=vs.110).aspx) directly accessible)
The evaluation of functions names is case insensitive.

|Name|Description|Example|Result|
|---|---|---|---|
|**Abs**(double number)|Return a double that is the absolute value of number|`Abs(-3.2d)`|`3.2d`|
|**Acos**(double d)|Return a double value that is the angle in radian whose d is the cosine<br/>d must be betwteen -1 and 1|`Acos(-0.5d)`|`2.0943951023032d`|
|**Array**(object obj1, object obj2 ,...)|Return a array (System.Object[]) of all given arguments|`Array(1, "Hello", true)`|`new object[]{1, "Hello", true}`|
|**Asin**(double d)|Return a double value that is the angle in radian whose d is the sine<br/>d must be betwteen -1 and 1|`Asin(-0.2d)`|`0.304692654015398d`|
|**Atan**(double d)|Return a double value that is the angle in radian whose d is the tangent|`Atan(2.1)`|`1.1263771168938d`|
|**Atan2**(double x, double y)|Return a double value that is the angle in radian whose the tangente is the quotient of x and y<br/>|`Atan2(2.1d, 3.4d)`|`0.553294325322293d`|
|**Avg**(double nb1, double nb2 ,...)|Return a double value that is the average value of all given arguments|`Avg(1, 2.5, -4, 6.2)`|`1.425d`|
|**Ceiling**(double a)|Return a double value that is the smallest integer greater than or equal to the specified number.|`Ceiling(4.23d)`|`5d`|
|**Cos**(double angle)|Return a double value that is the cosine of the specified angle in radian|`Cos(2 * Pi)`|`1d`|
|**Cosh**(double angle)|Return a double value that is the hyperbolic cosine of the specified angle in radian|`Cosh(2d)`|`3.76219569108363d`|
|**Exp**(double d)|Return a double value that is e raised to the specified d power|`Exp(3d)`|`20.0855369231877d`|
|**Floor**(double d)|Return a double value that is the largest integer less than or equal to the specified d argument|`Floor(4.23d)`|`4d`|
|**[IEEERemainder](https://msdn.microsoft.com/en-us/library/system.math.ieeeremainder(v=vs.110).aspx)**(double x, double y)|Return a double value that is the remainder resulting from the division of x by y|`IEEERemainder(9, 8)`|`1d`|
|**if**(bool condition, object yes, object no)|Return the yes object value if condition is true.<br/>Return the no object if condition is false|`if(1>2, "It is true", "It is false")`|`"It is false"`|
|**in**(object valueToFind, object obj1, object obj2...)|Return a boolean value that indicate if the first argument is found in the other arguments|`in(8, 4, 2, 8)`|`true`|
|**List**(object obj1, object obj2 ,...)|Return a List (System.Collections.Generic.List<object>) of all given arguments|`List(1, "Hello", true)`|`new List<object>(){1, "Hello", true}`|
|**Log**(double a, double base)|Return a double value that is the logarithm of a in the specified base|`Log(64d, 2d)`|`6d`|
|**Log10**(double a)|Return a double value that is the base 10 logarithm of a specified a|`Log10(1000d)`|`3d`|
|**Max**(double nb1, double nb2 ,...)|Return a double value that is the maximum value of all given arguments|`Max(1d, 2.5d, -4d)`|`2.5d`|
|**Min**(double nb1, double nb2 ,...)|Return a double value that is the minimum value of all given arguments|`Min(1d, 2.5d, -4d)`|`-4d`|
|**new**(TypeOrClass, constructorArg1, constructorArg2 ...)|Create an instance of the specified class as first argument and return it. A optional list of additional arguments can be passed as constructor arguments|`new(Random).next(0,10)`|`5d // or a random value between 1 and 9`|
|**Pow**(double x, double y)|Return a double value that is x elevate to the power y|`Pow(2,4)`|`16d`|
|**Round**(double d, (optional) int digits)|Rounds d to the nearest integer or specified number of decimal places.|`Round(2.432,1)`|`2.4d`|
|**Sign**(double d)|Return 1,-1 or 0 indicating the sign of d|`Sign(-12)`|`-1d`|
|**Sin**(double angle)|Return a double value that is the sine of the specified angle in radian|`Sin(Pi/2)`|`1d`|
|**Sinh**(double angle)|Return a double value that is the hyperbolic sine of the specified angle in radian|`Sinh(2d)`|`3.62686040784702d`|
|**Sqrt**(double d)|Return a double value that is the square root of the specified d value|`Sqrt(4d)`|`2d`|
|**Tan**(double angle)|Return a double value that is the tangent of the specified angle in radian|`Tan(Pi / 4)`|`1d`|
|**Tanh**(double angle)|Return a double value that is the hyperbolic tangent of the specified angle in radian|`Tanh(2d)`|`0.964027580075817d`|
|**Truncate**(double d)|Return a double value that is the integer part of the specified d value|`Truncate(2.45d)`|`2d`|

## On the fly variables and functions evaluation
In addition to custom variables, you can add variables and/or functions with on the fly evaluation.
2 C# events are provided that are fired when variables or functions are not fund as standard ones in evaluation time.

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
}

private void ExpressionEvaluator_EvaluateFunction(object sender, FunctionEvaluationEventArg e)
{
    if(e.Name.ToLower().Equals("sayhello") && e.Args.Count == 1)
    {
        e.Value = $"Hello {e.EvaluateArg(0)}";
    }
}
```
```
myVar + 2
10

SayHello("Bob")
Hello Bob
```

## Go fluid with a simple methods prefixing convention
Since ExpressionEvaluator evaluate one expression at a time. 
There are cases where we need to use void methods in a fluid syntax manner.

To do so, you only need to prefix the method name with "Fluid" or "Fluent"

```
// Example Add on List
List("hello", "bye").FluidAdd(\"test\").Count
3

List("hello", "bye").Select(x => x.ToUpper()).ToList().FluentAdd("test")[0]
HELLO

List("hello", "bye").Select(x => x.ToUpper()).ToList().FluentAdd("test")[1]
BYE

List("hello", "bye").Select(x => x.ToUpper()).ToList().FluentAdd("test")[2]
test
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
|Primary|[x++](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/operators/increment-operator)|Not Supported|
|Primary|[x--](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/operators/decrement-operator)|Not Supported|
|Primary|[new](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/new-operator)|Not Supported as this use [new() function](#standard-functions) instead|
|Primary|[typeof](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/typeof)|Not Supported|
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

Assignment Operators are not supported in ExpressionEvaluator

## Namespaces
By default the following list of namespaces are available

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
