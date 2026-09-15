# Conditionals, blocks, and loops

[Documentation index](README.md)

Control flow follows Aurora's central design: methods receive objects and decide what to execute. `{ ... }` produces a `Block` object; its contents run when a control-flow method executes it.

## `Logic.if(condition; block)`

The condition must be a `Boolean` object. A true condition executes the block; a false condition skips it.

```aurora
Int.create(age=20)
Logic.if(age.greaterThanOrEqual(18); {
    Terminal.writeLine("Adult")
})
.else({
    Terminal.writeLine("Under eighteen")
})
```

`Logic.if` returns a `LogicIfReturn` object. Its `.else(block)` method executes the fallback only if a previous branch has not run. `.else()` also returns `LogicIfReturn`, making this ordinary method chaining.

For another condition in a fallback, nest an `if`:

```aurora
Int.create(score=70)
Logic.if(score.greaterThanOrEqual(80); {
    Terminal.writeLine("Excellent")
})
.else({
    Logic.if(score.greaterThanOrEqual(60); {
        Terminal.writeLine("Passed")
    })
    .else({
        Terminal.writeLine("Try again")
    })
})
```

`LogicIfReturn` is produced by control-flow methods. Its constructor is deliberately unavailable.

## `Loop.while(condition; block)`

```aurora
Int.create(count=0)
Loop.while(count.lessThan(3); {
    Terminal.writeLine(count)
    Int.set(count=count.add(1))
})
```

This prints `0`, `1`, and `2` on separate lines. `Loop.while` reevaluates the condition before each iteration. The condition expression must produce a Boolean, and the second argument must produce a block. Supply these two arguments positionally. The method returns `Unit` when the loop finishes.

## Break and continue

`Loop.break()` exits the nearest active loop. `Loop.continue()` skips the remaining statements in the current iteration. Call them only while a loop is executing.

```aurora
Int.create(count=0)
Loop.while(count.lessThan(5); {
    Int.set(count=count.add(1))
    Logic.if(count.equals(3); {
        Loop.continue()
    })
    Terminal.writeLine(count)
})
```

This prints `1`, `2`, `4`, and `5`. Updating the counter before `continue()` allows the condition to keep progressing. See the repository's [break example](../TestCode/whileBreak.aur) and [continue example](../TestCode/whileContinue.aur).

## `Loop.for(initialiser; condition; incrementer; block)`

`Loop.for` is registered with four positional expressions: initialise once, check the condition, run the body, and run the incrementer.

The basic form works, but this method is experimental:

```aurora
Loop.for(Int.create(index=0); index.lessThan(3); index.increment(); {
    Terminal.writeLine(index)
})
```

This example prints `0`, `1`, and `2`. The implementation still has limitations: its initialiser check counts the whole loop context rather than just newly created variables, loop evaluator cleanup is incomplete, and the incrementer runs even after a break from the body. Prefer `Loop.while` when you need the more established control-flow path.

## Block objects

The object model allows a block to be stored and passed to a control-flow method. Intended syntax:

```aurora
Block.create(greeting={
    Terminal.writeLine("Hello from a block")
})
Logic.if(true; greeting)
```

`Block.create(...)` currently fails because the Block type is not marked final during initialization. Pass literal blocks directly to `Logic.if` or `Loop.while`, as in the working examples above.

There is no public `block.run()` method or complete user-defined method declaration facility yet. Blocks execute in the context supplied by the control-flow method; do not assume closure capture semantics.
