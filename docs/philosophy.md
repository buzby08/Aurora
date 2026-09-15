# Goals and object model

[Documentation index](README.md)

Aurora's main mission is **pure object oriented programming**. There are zero language-level primitives. Everything is an object or a method, including the operations that create variables and control execution.

## Values are objects

`"Liam"`, `42`, `3.5`, `true`, and `null` are literal spellings for objects. Their types are `String`, `Int`, `Float`, `Boolean`, and `Null`. Literal syntax gives you a convenient way to write an object directly. The current interpreter has a [Float conversion bug](builtins.md#float-and-math); the types above describe the language model.

Objects expose behaviour through methods and information through attributes:

```aurora
String.create(name="Liam")
Terminal.writeLine(name.length)
Terminal.writeLine(name.add("!"))
Terminal.writeLine(42.add(1))
```

The interpreter uses C# and .NET internally to represent these values. Those implementation details do not introduce a separate primitive category into Aurora programs.

## Types are objects too

Names such as `String`, `Int`, and `Array` refer to type objects. You can call methods on them and pass them as arguments:

```aurora
Array.create(names=Array.new("Liam"; "Ada"; type=String))
```

Here, `String` is an object supplied to the array constructor to describe which values it accepts. `Object` is the root of the builtin type hierarchy, and `Type` describes type objects. `Type` is itself a type object.

## Operations are methods

Aurora carries the same approach into variable creation, arithmetic, conditions, and repetition:

| Operation | Aurora form |
| --- | --- |
| Create a string variable | `String.create(name="Liam")` |
| Construct an array object | `Array.new(type=String)` |
| Update a variable | `Int.set(count=count.add(1))` |
| Add two integer objects | `count.add(1)` |
| Compare integer objects | `count.lessThan(5)` |
| Choose whether to execute a block | `Logic.if(condition; block)` |
| Repeat a block | `Loop.while(condition; block)` |

A block written with `{ ... }` is a `Block` object containing code. `Logic` and `Loop` methods decide when that code runs. A method that completes without a data result returns a `Unit` object, preserving the same model for operations such as terminal output.

## Readable, explicit code

Aurora favours descriptive method names such as `multiplyBy`, `lessThan`, and `valueOrDefault`. Semicolons separate arguments, and dots connect an object to its behaviour. The aim is a consistent language in which the same object-and-method approach applies throughout a program.

The implementation is still growing toward this mission. User-defined types and methods are not yet available as a complete public language feature. See [current status](status.md) for the distinction between the design and what runs today.
