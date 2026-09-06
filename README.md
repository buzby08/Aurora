# Aurora

Aurora is a custom programming language and interpreter built with C# and .NET 9. Its current design treats types as first-class runtime objects and expresses most operations through method calls on those types or on instances created from them.

This README reflects the interpreter as it is currently implemented, with the built-in runtime surface based primarily on [`Aurora/Internals/Builtins.cs`](/home/busby08/Documents/Aurora/Aurora/Internals/Builtins.cs) and the example program in [`Aurora/code.aur`](/home/busby08/Documents/Aurora/Aurora/code.aur).

## Language Style

- Variables are introduced through type methods such as `String.create(...)` and `Int.create(...)`.
- Existing variables can be updated through `Type.set(...)`.
- Method calls use `Type.method(...)` and `instance.method(...)`.
- Arguments are separated with semicolons: `method(a; b; name=value)`.
- Scripts are stored in `.aur` files.

## Example

[`Aurora/code.aur`](/home/busby08/Documents/Aurora/Aurora/code.aur) shows Aurora's interactive terminal features:

```aurora
String.create(name=Terminal.readLine("What is your name? "; default="N/A"))
Int.create(age=Terminal.readInt("How old are you? "; min=0))
Boolean.create(robot=Terminal.readBoolean("Are you a robot? "; outputStyle=BooleanOutputStyles.charStyle; immediate=false))

Terminal.writeLine("You are called"; name)
Terminal.writeLine("Your name is"; name.length; "characters long")
Terminal.writeLine("This year you are"; age; "years old, but next year you will be"; age.add(1); "years old.")
Terminal.writeLine("You are a robot:"; robot)
```

This example demonstrates several core ideas:

- input is read through the `Terminal` type
- named variables are created through type methods
- values expose attributes and instance methods, such as `name.length` and `age.add(1)`
- boolean input formatting is configurable through `BooleanOutputStyles`

## Built-in Types And Features

The global runtime currently exposes these built-in types:

- `Type`
- `Unit`
- `Optional`
- `Int`
- `Float`
- `String`
- `Boolean`
- `Null`
- `Terminal`
- `BooleanOutputStyles`
- `Math`

Implemented built-in behavior currently includes:

- `Type.create(name=value)` creates one or more variables of a specific type
- `Type.set(name=value)` updates one or more existing variables of a specific type
- `Type.toString()`
- `Optional.of(value)`
- `Optional.empty()`
- `optional.isEmpty`
- `optional.value`
- `optional.valueOrDefault(default)`
- `optional.toString()`
- `Int.add(other)`
- `Int.subtract(other)`
- `Int.multiplyBy(other)`
- `Int.divideBy(other)` returning a `Float`
- `Int.toString()`
- `Float.toString()`
- `String.add(other)`
- `String.concat(...)`
- `string.concat(other)`
- `string.substring(start; end)`
- `string.elementAt(index)`
- `string.find(value)` returning an `Optional`
- `string.contains(substring)`
- `string.length`
- `string.toString()`
- `Boolean.toString()`
- `Null.toString()`
- `Terminal.writeLine(...; separator=" "; end="\n")`
- `Terminal.readLine(message=""; default=null)`
- `Terminal.readInt(message=""; min=null; max=null)`
- `Terminal.readFloat(message=""; min=null; max=null)`
- `Terminal.readBoolean(message; outputStyle=BooleanOutputStyles.wordStyle; immediate=false)`
- `Terminal.readKey(message)`
- `Terminal.clear()`
- `BooleanOutputStyles.wordStyle`
- `BooleanOutputStyles.yesNoStyle`
- `BooleanOutputStyles.charStyle`
- `BooleanOutputStyles.onOffStyle`
- `BooleanOutputStyles.binaryStyle`
- `Math.truncate(value; places=0)`

## Running Aurora

Aurora can be used either through the prebuilt standalone binaries included in the repository or by compiling the interpreter from source.

### Option 1: Use A Standalone Binary

This repository includes standalone binaries under [`Aurora/dist/`](/home/busby08/Documents/Aurora/Aurora/dist):

- Linux: [`Aurora/dist/linux/Aurora`](/home/busby08/Documents/Aurora/Aurora/dist/linux/Aurora)
- macOS: [`Aurora/dist/macOS/Aurora`](/home/busby08/Documents/Aurora/Aurora/dist/macOS/Aurora)
- Windows: [`Aurora/dist/windows/Aurora.exe`](/home/busby08/Documents/Aurora/Aurora/dist/windows/Aurora.exe)

From the repository root, run the binary for your platform and pass it a `.aur` file:

```bash
./Aurora/dist/linux/Aurora Aurora/code.aur
```

```bash
./Aurora/dist/macOS/Aurora Aurora/code.aur
```

```powershell
.\Aurora\dist\windows\Aurora.exe .\Aurora\code.aur
```

### Option 2: Compile From Source

Prerequisite: .NET 9 SDK

Build the interpreter from the repository root:

```bash
dotnet build Aurora/Aurora.csproj
```

Run a script directly through `dotnet run`:

```bash
dotnet run --project Aurora -- Aurora/code.aur
```

You can also run the built output after compiling:

```bash
dotnet Aurora/bin/Debug/net9.0/Aurora.dll Aurora/code.aur
```

## Command-Line Options

Aurora expects a script path as its main positional argument:

```bash
dotnet run --project Aurora -- Aurora/code.aur
```

Available options include:

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
dotnet run --project Aurora -- Aurora/code.aur --verbose --warn
```

## Repository Layout

- [`Aurora/`](/home/busby08/Documents/Aurora/Aurora) contains the interpreter source
- [`Aurora/code.aur`](/home/busby08/Documents/Aurora/Aurora/code.aur) contains the current example program
- [`Aurora/Internals/Builtins.cs`](/home/busby08/Documents/Aurora/Aurora/Internals/Builtins.cs) defines the built-in runtime types, attributes, and methods
- [`Aurora.sln`](/home/busby08/Documents/Aurora/Aurora.sln) is the solution file

## Status

Aurora is still evolving. The language direction is broader than the implemented runtime today, so this document intentionally describes the current executable behavior rather than the longer-term roadmap.
