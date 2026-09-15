# Examples

[Documentation index](README.md)

Save each example in its own `.aur` file and run it with `dotnet run --project Aurora -- yourFile.aur` from the repository root.

## A greeting

```aurora
String.create(name="Liam")
Terminal.writeLine("Hello".concat(name).add("!"))
```

Output:

```text
Hello Liam!
```

## Walk through an array

This combines construction, variable creation, a loop, object comparison, and indexed access:

```aurora
Array.create(names=Array.new("Liam"; "Ada"; "Grace"; type=String))
Int.create(index=0)
Loop.while(index.lessThan(names.length); {
    Terminal.writeLine(index; names.at(index); separator=": ")
    Int.set(index=index.add(1))
})
```

Output:

```text
0: Liam
1: Ada
2: Grace
```

## Search a string

```aurora
String.create(message="Welcome to Aurora")
Logic.if(message.contains("Aurora"); {
    Terminal.writeLine("Aurora was found")
})
.else({
    Terminal.writeLine("No match")
})
```

`String.find()` also returns an Optional when you need to test for a matching position. See the [Optional reference](builtins.md#optional) for the current limitations on extracting its value.

## Ask for a name and age

This example requires interactive input:

```aurora
String.create(name=Terminal.readLine("What is your name? "; default="Guest"))
Int.create(age=Terminal.readInt("How old are you? "; min=0))
Terminal.writeLine("Hello"; name)
Terminal.writeLine("Your name has"; name.length; "characters")
Terminal.writeLine("Next year you will be"; age.add(1))
```

## Repository examples

The [TestCode directory](../TestCode) contains runnable examples and intentional error cases.

| Topic | File |
| --- | --- |
| First output | [hello-world.aur](../TestCode/hello-world.aur) |
| Variable creation | [variables.aur](../TestCode/variables.aur) |
| Comments | [comments.aur](../TestCode/comments.aur) |
| Named arguments | [keywordArguments.aur](../TestCode/keywordArguments.aur) |
| Multiple output arguments | [unlimitedKwargs.aur](../TestCode/unlimitedKwargs.aur) |
| Basic conditionals | [if-statements-basic.aur](../TestCode/if-statements-basic.aur) |
| Nested fallback branches | [if-else-statements.aur](../TestCode/if-else-statements.aur) |
| Arrays | [Array.aur](../TestCode/Array.aur) |
| Break and continue | [whileBreak.aur](../TestCode/whileBreak.aur), [whileContinue.aur](../TestCode/whileContinue.aur) |

Despite its filename, `unlimitedKwargs.aur` demonstrates multiple positional arguments.

These cases deliberately fail:

| File | Expected error |
| --- | --- |
| [ArrayIndexTooBig.aur](../TestCode/ArrayIndexTooBig.aur) | `Aurora.OutOfRange` |
| [ArrayIndexTooSmall.aur](../TestCode/ArrayIndexTooSmall.aur) | `Aurora.OutOfRange` |
| [ArrayTypeIncorrect.aur](../TestCode/ArrayTypeIncorrect.aur) | `Aurora.TypeMismatch` |
| [unexpectedSemiColon](../TestCode/unexpectedSemiColon) | `Aurora.UnclosedDelimiter` for a statement-ending semicolon. |

The repository's [Python runner](../test.py) checks successful exits and expected diagnostics. It does not assert the output of successful examples. The scratch file `Aurora/code.aur` currently calls an unregistered `Array.test` method; start with the examples above.
