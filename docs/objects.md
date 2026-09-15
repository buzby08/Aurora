# Variables, objects, and constructors

[Documentation index](README.md)

## `.create()` creates variables

Call `.create()` on the type of variable you want to introduce:

```aurora
String.create(name="Liam")
Int.create(age=25)
Boolean.create(ready=true)
```

`String.create(name="Liam")` creates a string variable called `name`. The argument name becomes the variable name, and the argument value supplies its initial object.

You can create several variables of the same type in one call:

```aurora
String.create(firstName="Liam"; lastName="Smith")
```

The values must match the receiving type. Creating a variable that already exists in the same scope raises an error. `.create()` returns `Unit`; it does not return the newly stored object.

## `.new()` constructs an object

A constructor returns an object without introducing a variable name:

```aurora
Terminal.writeLine(Array.new(1; 2; 3; type=Int).length)
```

Use `.create()` to keep the constructed object in a variable:

```aurora
Array.create(numbers=Array.new(1; 2; 3; type=Int))
Array.create(emptyNumbers=Array.new(type=Int))
```

The constructor runs first, then `.create()` stores its result. The general meaning of `Array.new(...)` is “construct an array”; the current implementation requires the `type` argument, even for an empty array.

| Constructor | Result |
| --- | --- |
| `Int.new()` | Integer object with value zero. |
| `Float.new()` | Float object with value zero. |
| `Boolean.new()` | Boolean object with value false. |
| `Null.new()` | Null object. |
| `Array.new(...; type=Int)` | Array containing compatible objects. |
| `Optional.new(value)` | Optional wrapping an object. |

`String.new()`, `Object.new()`, `Type.new()`, and `Interface.new()` are registered but unimplemented. Use a string literal to obtain a string object. Static utility types such as `Terminal`, `Logic`, `Loop`, and `Math` do not support instance creation.

## `.set()` updates variables

```aurora
Int.create(count=1)
Int.set(count=count.add(1))
Terminal.writeLine(count)
```

This prints `2`. `.set()` requires an existing variable and a value compatible with both the receiving type and the variable's current type. It returns `Unit`.

`count.add(1)` returns a new integer result. Calling it alone does not update `count`. Integer variables also provide `count.increment()` and `count.decrement()`, with an optional `amount` argument, to update the variable directly. Call these on named integer variables, rather than temporary results.

## Scope

Variables are resolved in the current scope and then in enclosing scopes. `.create()` introduces a variable in its calling scope; `.set()` searches outward to update an existing variable. Branch bodies execute in child scopes, so a name created inside an `if` block is local to that branch. An outer variable can be updated from inside it:

```aurora
Int.create(score=0)
Logic.if(true; {
    Int.set(score=10)
})
Terminal.writeLine(score)
```

Create counters outside `Loop.while` and update them in its body. Loop bodies currently reuse a loop context across iterations, so repeatedly creating the same name directly in the body can produce a duplicate-variable error.
