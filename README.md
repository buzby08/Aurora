# Aurora

Aurora is a programming language with one central mission: **pure object oriented programming, with zero primitives. Everything is an object or a method.** Numbers, strings, types, and blocks of code are objects. Creating variables, making decisions, and running loops all happen through methods.

```aurora
String.create(name="Liam")
Terminal.writeLine("Hello".concat(name))

Int.create(count=0)
Loop.while(count.lessThan(3); {
    Terminal.writeLine("Count:"; count)
    Int.set(count=count.add(1))
})

Array.create(numbers=Array.new(1; 2; 3; type=Int))
Terminal.writeLine("First number:"; numbers.at(0))
```

`.create()` creates a named variable of the receiving type: `String.create(name="Liam")` creates the string variable `name`. `.new()` is a constructor: `Array.new(type=Int)` constructs an empty array, which can then be stored with `Array.create(...)`.

Aurora is under active development. These docs describe the current interpreter and identify features that are still incomplete.

## Documentation

Start with the [public documentation](docs/README.md), or go directly to:

- [Goals and object model](docs/philosophy.md)
- [Getting started](docs/getting-started.md)
- [Syntax and style](docs/syntax.md)
- [Variables, objects, and constructors](docs/objects.md)
- [Conditionals, blocks, and loops](docs/control-flow.md)
- [Builtin types and methods](docs/builtins.md)
- [Terminal input and output](docs/terminal.md)
- [Examples](docs/examples.md)
- [Current status and limitations](docs/status.md)

## Run Aurora

Install the .NET 9 SDK and run from the repository root:

```sh
dotnet build Aurora/Aurora.csproj
dotnet run --project Aurora -- TestCode/hello-world.aur
```

The interpreter is written in C#. See [getting started](docs/getting-started.md) for commands and [the project map](docs/status.md#project-map) for the solution layout.
