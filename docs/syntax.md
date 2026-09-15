# Syntax and style

[Documentation index](README.md)

## Calls and attributes

Call a method with parentheses. Read an attribute without parentheses:

```aurora
String.create(name="Liam")
Terminal.writeLine(name.length)
Terminal.writeLine(name.substring(0; 2))
```

A call can target a type object (`String.create`) or an instance (`name.substring`). Results can receive further method calls:

```aurora
Terminal.writeLine("Hello".concat("Liam").add("!"))
```

Names are case-sensitive. Follow the builtin convention: type names use `PascalCase`, and variables, attributes, methods, and parameters use `camelCase`. The current tokenizer accepts letters in identifiers; use names such as `firstName` and `count`, without digits or underscores. `true`, `false`, and `null` have literal meanings; `this` is reserved.

## Arguments

Separate arguments with semicolons:

```aurora
Terminal.writeLine("Hello"; "Aurora"; separator=" / "; end="!")
```

Positional arguments appear first, followed by named arguments written as `name=value`. In a normal call, a named argument selects a parameter. In `.create()`, it supplies the variable name to create.

Methods such as `Terminal.writeLine` and `String.concat` accept multiple positional values. Their named options follow those values. For `Array.new`, supply the element type by name: `Array.new(1; 2; type=Int)`.

## Expressions and blocks

Write separate expressions on separate lines. Semicolons separate arguments; do not add a semicolon at the end of a statement.

```aurora
Int.create(count=2)
Logic.if(count.greaterThan(0); {
    Terminal.writeLine("There are items")
})
.else({
    Terminal.writeLine("There are no items")
})
```

Braces create a block object. Indent its contents consistently, typically with four spaces. Put the closing `})` on its own line for longer blocks. A leading dot on the following line can continue a method chain, as `.else(...)` does above.

Use methods for arithmetic and comparisons: `count.add(1)`, `count.lessThan(5)`, and `count.equals(2)`. Infix arithmetic and comparison operators are not implemented in the current expression evaluator. Use `.set()` to update variables; `=` in the examples is part of a named argument.

## Literals

| Written form | Object type |
| --- | --- |
| `42`, `-3` | `Int` |
| `3.5`, `-0.25` | `Float` |
| `"Hello"`, `'Hello'` | `String` |
| `true`, `false` | `Boolean` |
| `null` | `Null` |
| `{ ... }` | `Block` |

`Int` and `Float` are distinct intended types. A decimal point denotes a Float literal, but the current implementation incorrectly tags these literals as Boolean objects. Decimal calculations are affected; see [Float status](builtins.md#float-and-math).

String escape decoding is not implemented. Do not assume that source text `\n` or `\t` becomes a newline or tab. `Terminal.writeLine` already ends with a newline by default.

## Comments

Use `//` for a comment that continues to the end of the line:

```aurora
// A greeting
Terminal.writeLine("Hello") // This is a comment
Terminal.writeLine("This is // part of a string")
```

Keep a final newline in script files; the current comment scanner has an edge case with comments at end-of-file. Block comments are not implemented.
