# Aurora

Aurora is version 1 of a custom programming language and interpreter built with C# and .NET 9.

The language is designed around a strict object-oriented idea: everything is expressed through types and methods, including variable creation. Instead of primitive-style declarations, Aurora code uses calls such as `String.create(...)` to introduce values into scope.

## Current V1 Design

- Fully object-oriented surface syntax
- Types are first-class runtime objects
- Variables are created through type methods such as `String.create`
- Method calls use `Type.method(...)` or `instance.method(...)`
- Arguments are separated with semicolons: `method(a; b; name=value)`
- Scripts are stored in `.aur` files

## Example

[`Aurora/code.aur`](/home/busby08/Documents/Aurora/Aurora/code.aur) contains a minimal example:

```aurora
String.create(a="one"; b="two"; c="three")

Terminal.writeLine(a)
Terminal.writeLine(b)
Terminal.writeLine(c)
```

This shows the core style of Aurora V1:

- `String.create(...)` creates named variables in the current scope
- `a`, `b`, and `c` are string objects, not primitive values
- output is done through the `Terminal` type

## Built-in Types

The interpreter currently attaches these built-ins to the global context:

- `Type`
- `Unit`
- `Null`
- `Int`
- `Float`
- `String`
- `Boolean`
- `Terminal`

## Built-in Methods Implemented Today

The current codebase includes these working methods:

- `Type.create(name=value)` creates variables of a specific type
- `Type.set(name=value)` updates existing variables of a specific type
- `String.add(other)` concatenates strings
- `Int.add(other)`
- `Int.subtract(other)`
- `Int.multiplyBy(other)`
- `Int.divideBy(other)` returns a `Float`
- `Int.toString()`
- `Float.toString()`
- `Terminal.writeLine(value; end="\n")`

## Running Aurora

Prerequisite: .NET 9 SDK

Run a script from the repository root:

```bash
dotnet run --project Aurora -- Aurora/code.aur
```

Build the interpreter:

```bash
dotnet build Aurora.sln
```

Run tests:

```bash
dotnet test Aurora.sln
```

## CLI Notes

The interpreter accepts a `.aur` file path as its main argument. It also currently supports flags including:

- `--version`
- `-v`, `--verbose`
- `-d`, `--debug`
- `-w`, `--warn`
- `-s`, `--strict`
- `--no-console`
- `--logfile <path>`
- `--inline-stack-trace`
- `--disable-easter-eggs`
- `--config-file <path>`

Example:

```bash
dotnet run --project Aurora -- Aurora/code.aur --version
```

## Project Layout

- [`Aurora/`](/home/busby08/Documents/Aurora/Aurora) contains the interpreter
- [`Aurora/code.aur`](/home/busby08/Documents/Aurora/Aurora/code.aur) is the main syntax example
- [`Aurora.Tests/`](/home/busby08/Documents/Aurora/Aurora.Tests) contains tests
- [`Aurora.sln`](/home/busby08/Documents/Aurora/Aurora.sln) is the solution file

## Status

This repository is the first version of Aurora. The language direction is broader than the currently implemented surface area, so this README documents the interpreter as it exists now rather than the full intended roadmap.
