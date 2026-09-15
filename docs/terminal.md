# Terminal input and output

[Documentation index](README.md)

`Terminal` is a static utility type. Call its methods directly on the type object.

## Output

`Terminal.writeLine(...; separator=" "; end=<newline>) → Unit`

Pass any number of objects. Aurora converts them to strings, inserts the separator between them, and appends the ending. The default ending is an actual newline supplied by the builtin.

```aurora
Terminal.writeLine("Hello"; "Aurora")
Terminal.writeLine("red"; "green"; "blue"; separator=", ")
Terminal.writeLine("Loading"; end="...")
Terminal.writeLine(" done")
```

Use named arguments for `separator` and `end`. Calling `Terminal.writeLine()` prints a blank line. Objects must have a working string conversion; populated Optionals currently have a conversion bug.

## Read a line

`Terminal.readLine(message=""; default=null) → String`

Writes the prompt, reads a line, and returns a String. Empty or whitespace-only input uses the default; when no default is supplied, the result is an empty string.

```aurora
String.create(name=Terminal.readLine("Your name: "; default="Guest"))
Terminal.writeLine("Hello"; name)
```

The current prompt is printed with an internal wrapper, such as `BaseRuntimeValue(Your name: )`, rather than just the prompt text. The returned input is still a String.

The signature's `null` represents the builtin default. Omit `default` instead of explicitly passing `null`; nullable argument validation is incomplete.

## Read an integer

`Terminal.readInt(message=""; min=null; max=null) → Int`

Keeps prompting until input is a valid integer within the supplied inclusive bounds. Omit `min` or `max` for an unbounded side; supply Int objects for bounds you do use.

```aurora
Int.create(age=Terminal.readInt("Your age: "; min=0; max=130))
Terminal.writeLine("Next year:"; age.add(1))
```

## Read a decimal

`Terminal.readFloat(message=""; min=null; max=null)`

The input routine reads decimal numbers with inclusive Float bounds, but the method currently declares an `Int` return type while returning a `Float`. Valid input therefore encounters a return-type error. This API is not ready for use; decimal literals are also affected by a separate Float conversion bug. See [Float status](builtins.md#float-and-math).

Numeric input and formatting can depend on the host locale.

## Read a Boolean

`Terminal.readBoolean(message; outputStyle=BooleanOutputStyles.word; immediate=false) → Boolean`

The prompt is required. Choose a style object to control the accepted true/false choices:

| Style object | True input | False input |
| --- | --- | --- |
| `BooleanOutputStyles.word` | `true` | `false` |
| `BooleanOutputStyles.yesNo` | `yes` | `no` |
| `BooleanOutputStyles.char` | `y` | `n` |
| `BooleanOutputStyles.onOff` | `on` | `off` |
| `BooleanOutputStyles.binary` | `1` | `0` |

```aurora
Boolean.create(ready=Terminal.readBoolean("Continue? "; outputStyle=BooleanOutputStyles.yesNo))
Logic.if(ready; {
    Terminal.writeLine("Continuing")
})
```

Line-based responses are lowercased before comparison. With `immediate=true`, the `char` and `binary` styles read a single key without requiring Enter; character mode expects lowercase `y` or `n`. Other styles remain line-based. The style changes input choices, not the stored Boolean object or its output formatting.

## Read a key and clear the screen

| Method | Behaviour |
| --- | --- |
| `Terminal.readKey(message) → String` | Write a required prompt, read and echo a key, and return its character as a String. |
| `Terminal.clear() → Unit` | Clear the console. |

Single-key input and screen clearing require a suitable interactive terminal. They are unsuitable for redirected-input examples.
