![ExpressionEvaluator Icon](https://github.com/codingseb/ExpressionEvaluator/blob/master/Icon.png?raw=true "ExpressionEvaluator A Simple Math and Pseudo C# Expression Evaluator in One C# File") 
# ExpressionEvaluator

A Simple Math and Pseudo C# Expression Evaluator in One C# File.

And from version 1.2.0 can execute small C# like scripts 

It is largely based on and inspired by the following resources [this post on stackoverflow](http://stackoverflow.com/questions/333737/evaluating-string-342-yield-int-18/333749), [NCalc](https://github.com/pitermarx/NCalc-Edge), [C# Operators](https://msdn.microsoft.com/en-us/library/6a71f45d.aspx) and [C# Statement Keywords](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/statement-keywords)

## Status

|Branch|Status|
|---|---|
|master|[![Build Status](https://coding-seb.visualstudio.com/_apis/public/build/definitions/cbe8d2f2-9c7a-48aa-8366-89ef39381eff/1/badge)](https://coding-seb.visualstudio.com/ExpressionEvaluator/_build/index?definitionId=1)|
|dev|[![Dev Status](https://coding-seb.visualstudio.com/_apis/public/build/definitions/cbe8d2f2-9c7a-48aa-8366-89ef39381eff/2/badge)](https://coding-seb.visualstudio.com/ExpressionEvaluator/_build/index?definitionId=2)|
|nuget|[![NuGet Status](http://img.shields.io/nuget/v/CodingSeb.ExpressionEvaluator.svg?style=flat&max-age=86400)](https://www.nuget.org/packages/CodingSeb.ExpressionEvaluator/)|

## Features
* Basic mathematical expression evaluation
* System.Math methods and constants directly available (some like Max, Min, Avg are improved)
* Some useful [functions](https://github.com/codingseb/ExpressionEvaluator/wiki/Variables-and-functions#standard-functions) for example to create List and Arrays
* [Custom variables definition](https://github.com/codingseb/ExpressionEvaluator/wiki/Variables-and-functions#custom-variables)
* [On the fly variables and functions evaluation](https://github.com/codingseb/ExpressionEvaluator/wiki/Variables-and-functions#on-the-fly-variables-and-functions-evaluation) (To easily extend possibilities, Manage also on instance Property and Methods)
* [A large set of C# operators availables](https://github.com/codingseb/ExpressionEvaluator/wiki/Operators-and-Keywords#standard-operators)
* Instance and static methods and properties access like as in C#
* You can call Methods and/or Properties on your own classes (just pass a object as custom variables)
* [C# primary types](https://github.com/codingseb/ExpressionEvaluator/wiki/C%23-Types-Management#primary-types)
* Use strings as in C# (@"", $"", $@"" available)
* Lambda expressions
* Classes like File, Directory, Regex, List ... available ([You can extend the list of Namespaces](https://github.com/codingseb/ExpressionEvaluator/wiki/C%23-Types-Management#assemblies-namespaces-and-types))
* Create instance with [new(MyClassName, constructorArgs)](https://github.com/codingseb/ExpressionEvaluator/wiki/Variables-and-Functions#standard-functions) or [new MyClassName(constructorArgs)](https://github.com/codingseb/ExpressionEvaluator/wiki/Operators-and-Keywords#standard-operators)
* [Call void methods with fluid prefix convention to chain operations](https://github.com/codingseb/ExpressionEvaluator/wiki/Variables-and-Functions#go-fluid-with-a-simple-methods-prefixing-convention)
* Manage [ExpandoObject](https://github.com/codingseb/ExpressionEvaluator/wiki/ExpandoObject)
* [Create custom Operators or change the parsing process](https://github.com/codingseb/ExpressionEvaluator/wiki/Advanced-Customization-and-Hacking)

## And with [ScriptEvaluate](https://github.com/codingseb/ExpressionEvaluator/wiki/Getting-Started#small-scripts) method
* Small C# like script evaluation (Multi expressions separated by ; )
* Some conditional and loop blocks [keywords](https://github.com/codingseb/ExpressionEvaluator/wiki/Operators-and-Keywords#scripts-keywords) (if, while, for, foreach ...)
* Multi-line (multi expression) Lambda expressions.

## Resources
* [Getting Started](https://github.com/codingseb/ExpressionEvaluator/wiki/Getting-Started)
* [Documentation on the wiki](https://github.com/codingseb/ExpressionEvaluator/wiki)

## And more is coming
* [The Todo List](https://github.com/codingseb/ExpressionEvaluator/wiki/ExpressionEvaluator-Todo-List)

## Similar projects
### Free
* [NCalc](https://github.com/pitermarx/NCalc-Edge)
* [Jint](https://github.com/sebastienros/jint) Support scripting but with Javascript
* [DynamicExpresso](https://github.com/davideicardi/DynamicExpresso/)
* [Flee](https://github.com/mparlak/Flee)
* [CS-Script](https://github.com/oleg-shilo/cs-script) Best alternative (I use it some times) -> Real C# scripts better than ExpressionEvaluator (But everything is compiled. Read the doc. Execution is faster but compilation can make it very slow. And if not done the right way, it can lead to [memory leaks](https://en.wikipedia.org/wiki/Memory_leak))
* [Roslyn](https://github.com/dotnet/roslyn) The Microsoft official solution (For scripting [see](https://github.com/dotnet/roslyn/wiki/Scripting-API-Samples))

### Commercial
* [Eval Expression.NET](http://eval-expression.net/)

I would say every C# evaluation libraries have drawbacks and benefits, ExpressionEvaluator is not an exception so choose wisely (Read docs and licences).

The biggest difference of ExpressionEvaluator is that everything is evaluated on the fly, nothing is compiled or transpile nor in CLR/JIT/IL nor in lambda expressions nor in javascript or other languages stuffs.
So it can be slower in some cases (sometimes not) but it also avoid a lot of memory leaks. 
It already allow to evaluate some small scripts.
If you don't want an another .dll file in your project, you only need to copy one [C# file](./CodingSeb.ExpressionEvaluator/ExpressionEvaluator.cs) in your project. And it's [MIT licence](./LICENSE.md)
