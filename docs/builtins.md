# Builtin types and methods

[Documentation index](README.md)

All names below are installed in the global context. Signatures describe Aurora calls; `→` identifies the result type, and `...` means multiple positional arguments. Shared methods are inherited where applicable. Limitations are noted beside affected APIs and collected on the [status page](status.md).

## Global objects

| Name | Role |
| --- | --- |
| `Object` | Root object type and shared methods. |
| `Type` | Type objects, such as `Int` and `String`. |
| `Int`, `Float`, `String`, `Boolean`, `Null` | Value object types. |
| `Unit` | Result of methods with no data result. |
| `Array` | Indexed collection of objects. |
| `Optional` | An object that may contain a value. |
| `Block` | Code represented as an object. |
| `Logic`, `LogicIfReturn` | Conditional execution and chained fallback. |
| `Loop` | Repeated execution, break, and continue. |
| `Terminal`, `BooleanOutputStyles` | Terminal input/output and Boolean input styles. |
| `Math` | Numeric utility methods. |
| `Callable` | Runtime foundation for callable behaviour; no dedicated public methods yet. |
| `Interface`, `ICollection` | Interface infrastructure and the builtin collection contract. |

## Shared object methods

| Method | Behaviour |
| --- | --- |
| `SomeType.create(name=value; ...) → Unit` | Create named variables with compatible values. |
| `SomeType.set(name=value; ...) → Unit` | Update existing variables with compatible values. |
| `object.toString() → String` | Return a string representation. Also available on type objects. |
| `object.equals(other) → Boolean` | Compare objects. Also available on type objects. |

`SomeType` stands for a receiving type such as `String`. See [variables and constructors](objects.md) for creation rules and constructor availability.

Equality requires matching runtime types. Basic value objects compare their underlying values; array equality is not element-by-element equality. Conceptually, integer `1` and float `1.0` belong to different types; decimal literals currently have the conversion bug described below.

## `Int`

An integer object stores a signed 32-bit integer, from -2,147,483,648 to 2,147,483,647.

| Method | Result |
| --- | --- |
| `Int.new() → Int` | Zero. |
| `number.add(other) → Int` | Sum. |
| `number.subtract(other) → Int` | Difference. |
| `number.multiplyBy(other) → Int` | Product. |
| `number.divideBy(other) → Float` | Intended decimal quotient; currently fails because its result is tagged Boolean. |
| `number.lessThan(other) → Boolean` | Whether the receiver is smaller. |
| `number.lessThanOrEqual(other) → Boolean` | Whether the receiver is smaller or equal. |
| `number.greaterThan(other) → Boolean` | Whether the receiver is larger. |
| `number.greaterThanOrEqual(other) → Boolean` | Whether the receiver is larger or equal. |
| `number.increment(amount=1) → Unit` | Update a named integer variable by adding `amount`. |
| `number.decrement(amount=1) → Unit` | Update a named integer variable by subtracting `amount`. |
| `number.toString() → String` | Text representation. |

`other` and `amount` must be `Int` objects. Arithmetic methods return results without updating the receiver, except `increment` and `decrement`.

```aurora
Int.create(total=7)
Terminal.writeLine(total.add(3))
total.increment(amount=2)
Terminal.writeLine(total)
```

The outputs are `10` and `9`. Division is registered but currently fails its return-type check.

## `Float` and `Math`

`Float` is intended to represent decimal numbers using .NET's decimal representation. It is a distinct object type from `Int`; implicit numeric conversion is not provided by argument validation.

**Current limitation:** decimal literals, integer division results, and `Math.truncate` results are incorrectly tagged as Boolean objects by the Float conversion helper. They fail normal Float use. `Float.new()` uses a different creation path and correctly creates a zero-valued Float.

| Method | Result |
| --- | --- |
| `Float.new() → Float` | Decimal zero. |
| `number.toString() → String` | Text representation. |
| `Math.truncate(value; places=0) → Float` | Intended truncation to a selected decimal position; affected by the Float conversion bug. |

`places` is an `Int`. Positive values retain that many fractional digits; zero removes the fractional part; negative values truncate integer digits toward tens, hundreds, and so on. This operation truncates rather than rounds. Negative inputs with negative `places` have edge cases in the current implementation.

Intended syntax, currently blocked by the Float conversion bug:

```aurora
Terminal.writeLine(Math.truncate(123.456; places=2))
Terminal.writeLine(Math.truncate(123.456))
Terminal.writeLine(Math.truncate(123.456; places=-1))
```

The intended numeric results are `123.45`, `123`, and `120`. Float-specific arithmetic and comparison methods are not registered yet.

## `String`

Use a quoted literal to create a string object. `String.new()` is not implemented.

| Method or attribute | Behaviour |
| --- | --- |
| `text.add(other) → String` | Append another String with no separator. |
| `String.concat(...) → String` | Convert objects to strings and join them with spaces. |
| `text.concat(other) → String` | Append a space and another object's string representation. |
| `text.substring(start; end) → String` | Slice from `start` inclusive to `end` exclusive. |
| `text.elementAt(index) → String` | Read a single indexed string element. |
| `text.find(value) → Optional` | Wrap the first matching index, or return an empty Optional. |
| `text.contains(substring) → Boolean` | Test for a case-sensitive, ordinal substring match. |
| `text.length → Int` | Length of the string. |
| `text.toString() → String` | Return the string itself. |

Indices start at zero. For `substring`, use `0 <= start <= end <= text.length`; for `elementAt`, use `0 <= index < text.length`. String length and indexing use UTF-16 code units, so an emoji can occupy more than one position.

```aurora
String.create(name="Liam")
Terminal.writeLine(name.add("!"))
Terminal.writeLine(name.substring(0; 2))
Terminal.writeLine(name.elementAt(0))
Terminal.writeLine(name.contains("ia"))
Terminal.writeLine(name.find("ia").isEmpty)
```

`String.concat` inserts a separator when the accumulated text is nonempty; leading empty strings therefore do not add leading spaces. `find` returns an empty Optional for an empty receiver. Avoid printing a populated Optional directly until its conversion bug is fixed.

## `Boolean`, `Null`, and `Unit`

`true` and `false` are Boolean objects. `Boolean.new()` returns false; `boolean.toString()` returns its text representation. Integer comparisons and `.equals()` produce Boolean objects. Boolean-specific `and`, `or`, and `not` methods are not registered yet.

`null` is a Null object. `Null.new()` constructs one and `null.toString()` returns `"null"`. Null is distinct from an empty Optional and from Unit.

`Unit` is the result object used by operations such as `.create()`, `.set()`, and `Terminal.writeLine()`. It is not a separately constructible user value.

## `Array` and `ICollection`

| Method or attribute | Behaviour |
| --- | --- |
| `Array.new(...; type=SomeType) → Array` | Construct an array of compatible objects. `type` is required. |
| `array.at(index) → Object` | Return the object at a zero-based Int index. |
| `array.length → Int` | Number of elements. |
| `array.length() → Int` | Method form of the length operation. |
| `array.toString() → String` | Array representation. |

```aurora
Array.create(names=Array.new("Liam"; "Ada"; type=String))
Terminal.writeLine(names.length)
Terminal.writeLine(names.at(0))
Array.create(emptyNames=Array.new(type=String))
Terminal.writeLine(emptyNames.length())
```

Construction checks that each element is compatible with the supplied type. Passing `type=Object` allows objects of different more specific types. The current array value does not retain that declared element type as separate metadata.

Out-of-range indices raise `Aurora.OutOfRange`; incompatible constructor values raise `Aurora.TypeMismatch`. Append, removal, element assignment, and bracket indexing are not exposed as public operations yet.

`Array` implements the builtin `ICollection` contract, which declares `at(index)` and `length()`. The `length` attribute is additionally provided by Array. User-defined interfaces are incomplete; `Interface.new()` is not implemented.

## `Optional`

| Method or attribute | Intended behaviour and current status |
| --- | --- |
| `Optional.new(value) → Optional` | Construct an Optional holding an object. |
| `Optional.empty() → Optional` | Construct one with no contained object. |
| `optional.isEmpty → Boolean` | Whether there is no contained object. |
| `optional.value` | Intended to return the contained object; currently declares the wrong result type for ordinary values. Empty access raises an error. |
| `optional.valueOrDefault(default)` | Intended to return the contained object or fallback; currently declares the wrong result type for ordinary values. |
| `optional.toString() → String` | Empty values print as `Optional(Empty)`; populated values currently recurse during conversion. |

Safe checks with the current implementation:

```aurora
Optional.create(found="Aurora".find("ro"))
Terminal.writeLine(found.isEmpty)
Terminal.writeLine(Optional.empty().isEmpty)
```

`Optional.new(null)` contains a Null object, so it is not empty. Use `Optional.empty()` for absence. The current constructor name is `new`, not the older `of` spelling.

For execution methods see [control flow](control-flow.md). For input/output see [Terminal](terminal.md).
