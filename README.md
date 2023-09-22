![ExpressionEvaluator Icon](https://github.com/codingseb/ExpressionEvaluator/blob/master/Icon.png?raw=true "ExpressionEvaluator A Simple Math and Pseudo C# Expression Evaluator in One C# File") 
# ExpressionEvaluator

| :warning: For now, I don't have time to maintain this repository. So if you have PR to fix some bugs. I'll be happy to review and merge it. Otherwise, I will no longer actively develop ExpressionEvaluator. If the current state of the lib do not suite your needs I suggest you look the [list of great alternative projects](#similar-projects) below.
| --- |

A Simple Math and Pseudo C# Expression Evaluator in One [C# File](./CodingSeb.ExpressionEvaluator/ExpressionEvaluator.cs).

And from version 1.2.0 can execute small C# like scripts 

It is largely based on and inspired by the following resources [this post on stackoverflow](http://stackoverflow.com/questions/333737/evaluating-string-342-yield-int-18/333749), [NCalc](https://github.com/pitermarx/NCalc-Edge), [C# Operators](https://msdn.microsoft.com/en-us/library/6a71f45d.aspx) and [C# Statement Keywords](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/statement-keywords)

## Status

|Branch|Status|
|---|---|
|master|[![Build Status](https://coding-seb.visualstudio.com/_apis/public/build/definitions/cbe8d2f2-9c7a-48aa-8366-89ef39381eff/1/badge)](https://coding-seb.visualstudio.com/ExpressionEvaluator/_build/index?definitionId=1)|
|dev|[![Dev Status](https://coding-seb.visualstudio.com/_apis/public/build/definitions/cbe8d2f2-9c7a-48aa-8366-89ef39381eff/2/badge)](https://coding-seb.visualstudio.com/ExpressionEvaluator/_build/index?definitionId=2)|
|nuget|[![NuGet Status](http://img.shields.io/nuget/v/CodingSeb.ExpressionEvaluator.svg?style=flat&max-age=86400)](https://www.nuget.org/packages/CodingSeb.ExpressionEvaluator/)|

## Features
* Basic mathematical and logical expression evaluation
* System.Math methods and constants directly available (some like Max, Min, Avg are improved)
* Some useful [functions](https://github.com/codingseb/ExpressionEvaluator/wiki/Variables-and-functions#standard-functions) for example to create List and Arrays
* [Custom variables definition](https://github.com/codingseb/ExpressionEvaluator/wiki/Variables-and-functions#custom-variables)
* [On the fly variables and functions evaluation](https://github.com/codingseb/ExpressionEvaluator/wiki/Variables-and-functions#on-the-fly-variables-and-functions-evaluation) (To easily extend possibilities, Manage also on instance Property and Methods)
* [A large set of C# operators availables](https://github.com/codingseb/ExpressionEvaluator/wiki/Operators-and-Keywords#standard-operators)
* Instance and static methods and properties access like as in C#
* You can call Methods and/or Properties on your own classes (just pass a object as custom variables)
* [C# primary types](https://github.com/codingseb/ExpressionEvaluator/wiki/C%23-Types-Management#primary-types)
* Use strings as in C# (`@""`, `$""`, `$@""` available)
* Lambda expressions
* Classes like File, Directory, Regex, List ... available ([You can extend the list of Namespaces](https://github.com/codingseb/ExpressionEvaluator/wiki/C%23-Types-Management#assemblies-namespaces-and-types))
* Create instance with [new(MyClassName, constructorArgs)](https://github.com/codingseb/ExpressionEvaluator/wiki/Variables-and-Functions#standard-functions) or [new MyClassName(constructorArgs)](https://github.com/codingseb/ExpressionEvaluator/wiki/Operators-and-Keywords#standard-operators)
* [Call void methods with fluid prefix convention to chain operations](https://github.com/codingseb/ExpressionEvaluator/wiki/Variables-and-Functions#go-fluid-with-a-simple-methods-prefixing-convention)
* Manage [ExpandoObject](https://github.com/codingseb/ExpressionEvaluator/wiki/ExpandoObject)
* [Create custom Operators or change the parsing process](https://github.com/codingseb/ExpressionEvaluator/wiki/Advanced-Customization-and-Hacking)

## And with [ScriptEvaluate](https://github.com/codingseb/ExpressionEvaluator/wiki/Getting-Started#small-scripts) method
* Small C# like script evaluation (Multi expressions separated by ; )
* Some conditional and loop blocks [keywords](https://github.com/codingseb/ExpressionEvaluator/wiki/Operators-and-Keywords#scripts-keywords) (if, while, for, foreach ...)
* Multi-line (multi expression) Lambda expressions. (Can be use as method [See #72 Declare methods in scripts](https://github.com/codingseb/ExpressionEvaluator/issues/72) and the [doc](https://github.com/codingseb/ExpressionEvaluator/wiki/Variables-and-Functions#simulate-function-and-methods-declaration-with-lambda-and-multiline-lambda))

## Resources
* [Getting Started](https://github.com/codingseb/ExpressionEvaluator/wiki/Getting-Started)
* [Documentation on the wiki](https://github.com/codingseb/ExpressionEvaluator/wiki)
* [Live Demos](https://dotnetfiddle.net/Packages/41132/CodingSeb_ExpressionEvaluator)
* [Try it](https://dotnetfiddle.net/up4x3W)

## Similar projects
### Free
* [NCalc](https://github.com/pitermarx/NCalc-Edge)
* [Jint](https://github.com/sebastienros/jint) Support scripting but with Javascript
* [NLua](https://github.com/NLua/NLua) use Lua language in C#
* [MoonSharp](https://github.com/moonsharp-devs/moonsharp/)
* [DynamicExpresso](https://github.com/davideicardi/DynamicExpresso/)
* [Flee](https://github.com/mparlak/Flee)
* [Jace.Net](https://github.com/pieterderycke/Jace)
* [Calculator](https://github.com/loresoft/Calculator)
* [Westwind.Scripting](https://github.com/RickStrahl/Westwind.Scripting)
* [CS-Script](https://github.com/oleg-shilo/cs-script) Best alternative (I use it some times) -> Real C# scripts better than ExpressionEvaluator (But everything is compiled. Read the doc. Execution is faster but compilation can make it very slow. And if not done the right way, it can lead to [memory leaks](https://en.wikipedia.org/wiki/Memory_leak))
* [Roslyn](https://github.com/dotnet/roslyn) The Microsoft official solution (For scripting [see](https://github.com/dotnet/roslyn/wiki/Scripting-API-Samples))
* [MathParser](https://github.com/KirillOsenkov/MathParser) expression tree compiler and interpreter for math expressions. Heavily inspired by Roslyn.
* [YoowzxCalc](https://github.com/MarkusSecundus/YoowzxCalc)
* [Scriban](https://github.com/scriban/scriban)
* [WattleScript](https://github.com/WattleScript/wattlescript)
* [AngouriMath](https://github.com/asc-community/AngouriMath) For advanced Math in C#
* [Fluid](https://github.com/sebastienros/fluid)
* [ClearScript](https://github.com/microsoft/ClearScript)
* [Expressive](https://github.com/bijington/expressive)
* [IronPython](https://github.com/IronLanguages/ironpython3) to execute python in .Net or .Net in python

### Commercial
* [Eval Expression.NET](http://eval-expression.net/)
* [mXparser](https://mathparser.org) (dual licensing) (Free for open source)

### Projects that could help you build your own expression/script evaluator
* [SuperPower](https://github.com/datalust/superpower)
* [Sprache](https://github.com/sprache/Sprache)
* [CSLY : C# lex and yacc](https://github.com/b3b00/csly)
* [Lexepars](https://github.com/DNemtsov/Lexepars)
* [Parlot](https://github.com/sebastienros/parlot)
* [Irony](https://github.com/IronyProject/Irony)
* [Pidgin](https://github.com/benjamin-hodgson/Pidgin)

### Reading and resources
* [Crafting interpreters](http://www.craftinginterpreters.com)
* [Building a compiler](https://www.youtube.com/playlist?list=PLRAdsfhKI4OWNOSfS7EUu5GRAVmze1t2y) An excellent Youtube tutorial
* Search for LEX, YACC, AST, Syntaxic trees...
  
  
I would say every C# evaluation libraries have drawbacks and benefits, ExpressionEvaluator is not an exception so choose wisely (Read docs and licences).

The biggest difference of ExpressionEvaluator is that everything is evaluated on the fly, nothing is compiled or transpile nor in CLR/JIT/IL nor in lambda expressions nor in javascript or other languages stuffs.
So it can be slower in some cases (sometimes not) but it also avoid a lot of memory leaks. It is clearly not optimized for big reuse of expressions as the expression is reevaluated every time (Filtering on big dataset for example).
It already allow to evaluate some small scripts.
If you don't want an another .dll file in your project, you only need to copy one [C# file](./CodingSeb.ExpressionEvaluator/ExpressionEvaluator.cs) in your project. And it's [MIT licence](./LICENSE.md)

### Donate
Expression Evaluator is free and will always be.  
But if you want to say thanks for this lib with a donation or small sponsoring here you can :  
[Donate](https://www.paypal.com/donate?hosted_button_id=7K467U3H4NVJG)

Thank you anyway for using ExpressionEvaluator
