# Current status and limitations

[Documentation index](README.md)

Aurora is an evolving interpreter pursuing pure object oriented programming. This reference follows the active parser, evaluator, builtin registration, and `TestCode` examples. It does not promise that every runtime concept already has a complete public API.

## Available today

The current interpreter supports object literals, named variables through type methods, typed updates, static and instance calls, attributes, integer arithmetic and comparison, strings, arrays, terminal I/O, code blocks, conditional chains, and while loops with break and continue.

## Incomplete features

| Area | Current limitation |
| --- | --- |
| Constructors | `Object.new()`, `Type.new()`, `String.new()`, and `Interface.new()` throw an unimplemented-operation exception. |
| Custom types and methods | Runtime infrastructure exists, but complete public declaration APIs are not implemented. |
| `Loop.for` | Basic iteration works. Its initialiser check counts all context variables, evaluator cleanup is incomplete, and the incrementer runs after a break. Prefer `Loop.while`. |
| Optional extraction | `.value` and `.valueOrDefault(...)` declare `Type` as their result, causing type errors for ordinary contained or fallback objects. |
| Optional printing | A populated Optional's `toString()` converts itself recursively. Avoid printing or concatenating populated Optionals. |
| Float conversion | Decimal literals, division results, and truncation results are incorrectly tagged Boolean. `Float.new()` correctly constructs zero through a separate path. |
| Stored blocks | `Block.create(...)` fails because Block is not marked final during initialization; literal blocks passed directly to control-flow methods work. |
| String input prompt | `Terminal.readLine` displays an internal `BaseRuntimeValue(...)` wrapper around its prompt. |
| Decimal input | `Terminal.readFloat` returns a Float from a method declared to return Int, causing a runtime error. |
| Nullable arguments | Builtin defaults can be Null objects, but explicitly passing `null` to a String/Int/Float parameter can fail validation. Omit optional bounds/defaults instead. |
| String indexing | `elementAt(length)` passes the explicit bounds check but fails in the underlying indexing operation. Valid indices end at `length - 1`. |
| String escapes | Source escape sequences are not decoded. |
| Comments | A final `//` comment without a terminating newline may not be skipped correctly. |
| Arrays | No public append, removal, or element-update methods; declared constructor element type is not retained as metadata. |
| Float and Boolean operations | Float arithmetic/comparison methods and Boolean-specific logical methods are not registered. |
| Configuration | `--config-file` is accepted, but configuration application is not fully connected. |

Use `Loop.break()` and `Loop.continue()` only inside an executing loop. Some invalid or unsupported operations currently report a general system error instead of a dedicated language diagnostic.

Calls accepting multiple positional arguments currently evaluate supplied expressions in more than one validation pass. Keep expressions with side effects, especially terminal reads, in separate variable-creation statements before passing their values to output or collection methods.

## Changes from older examples

| Older spelling or assumption | Current form |
| --- | --- |
| `Optional.of(value)` | `Optional.new(value)` |
| `BooleanOutputStyles.wordStyle` | `BooleanOutputStyles.word` |
| `BooleanOutputStyles.yesNoStyle` | `BooleanOutputStyles.yesNo` |
| `BooleanOutputStyles.charStyle` | `BooleanOutputStyles.char` |
| `BooleanOutputStyles.onOffStyle` | `BooleanOutputStyles.onOff` |
| `BooleanOutputStyles.binaryStyle` | `BooleanOutputStyles.binary` |
| `--logfile` | `--log-file` |
| `--inline-stack-trace` | Not registered by the current command-line parser. |
| `Array.new()` with no arguments | Supply `type`, for example `Array.new(type=Int)`. |

## Project map

| Location | Responsibility |
| --- | --- |
| [Aurora](../Aurora) | Executable entry point and command-line handling. |
| [Aurora.Core](../Aurora.Core) | Tokens, syntax-tree data, argument parsing helpers, errors, and logging. |
| [Aurora.Parser](../Aurora.Parser) | Tokenization and parsing. |
| [Aurora.Evaluator](../Aurora.Evaluator) | Evaluation, runtime objects, types, scopes, and method dispatch. |
| [Builtins.cs](../Aurora.Evaluator/Internals/Builtins.cs) | Global builtin objects and their registered methods and attributes. |
| [InternalMethods](../Aurora.Evaluator/Internals/InternalMethods) | Shared implementations for variables, terminal operations, and control flow. |
| [TestCode](../TestCode) | Example scripts and expected-error cases. |
| [test.py](../test.py) | Script-based regression runner. |
| [Aurora.sln](../Aurora.sln) | .NET solution. |

The active entry point uses `Aurora.Evaluator/Evaluator.cs`; rework files, commented code, and design notes are not evidence that a public feature is available. Build the executable project as shown in [getting started](getting-started.md) to build the interpreter and its dependencies directly.
