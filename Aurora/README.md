# Aurora

## Introduction

Aurora is designed to be a high-level, high performance, object-oriented programming language.

Aurora challenges traditional syntax norms by eliminating primitives in favour of pure object-oriented constructs — even
variable assignment and control flow are method-driven.

Unlike many current languages, Aurora follows a very strict object-oriented principle. Everything in Aurora, even down
to variable creation and modification, is encapsulated in a class (See [variables](#variables) for more).

## Structure

Aurora follows a strict structure to ensure it is easy for new developers to understand, while being advanced enough for
advanced developers to use.

All classes provided by the language follow a structure of `ClassName.attributeName`, where the class is pascal case (
every word, including the first word, starts with a capitalized letter), and attribute, and method names, use camel
case (every word, excluding the first word, starts with a capitalized letter).

## Classes

There are many classes in Aurora, in fact it is the very principle in which the language was created with.

### Terminal

The terminal class contains all the utilities for interacting with the terminal.

#### Methods

##### clear

Clears the terminal

- positional parameters: None
- keyword parameters: None
- returns: Null
- e.g. `Terminal.clear()`

##### write

Writes a message to the terminal output stream

- All positional parameters, regardless of type, will be outputted. This is an unlimited number
- keyword 'end' (String) [Not required] - The character to output at the end of the stream. Default is `"\n"`
- Returns: Null
- e.g. `Terminal.write("Hello, "; "world"; end="!")` (Outputs `Hello, world!`)

##### read

Reads a string from the user

- keyword 'message' (String) [Not required] - The message to output to the stream before asking the user for input.
  Default is `""`
- keyword 'default' (String) [Not required] - The value to be returned after no input. Default is `""`
- Positional order is `message` then `default`
- Returns: String
- e.g. `Terminal.read("What is your name? "; default="No Name")`

##### readInt

Reads an integer from the user

- keyword 'message' (String) [Not required] - The message to output to the stream before asking the user for input.
  Default is `""`
- keyword 'min' (Integer) [Not required] - The minimum value to allow. Any input below this is rejected and the user is
  prompted again. If not provided this is not utilized.
- keyword 'max' (Integer) [Not required] - The maximum value to allow. Any input above this is rejected and the user is
  prompted again. If not provided this is not utilized.
- Positional order is `message` then `min` then `max`
- Returns: Integer
- e.g. `Terminal.readInt("How old are you? "; min=0; max=100)`

##### readFloat

Reads a float from the user

- keyword 'message' (String) [Not required] - The message to output to the stream before asking the user for input.
  Default is `""`
- keyword 'min' (Float) [Not required] - The minimum value to allow. Any input below this is rejected and the user is
  prompted again. If not provided this is not utilized.
- keyword 'max' (Float) [Not required] - The maximum value to allow. Any input above this is rejected and the user is
  prompted again. If not provided this is not utilized.
- Positional order is `message` then `min` then `max`
- Returns: Float
- e.g. `Terminal.readFloat("What is your height in meters? "; min=0; max=10)`

##### readBoolean

Reads a boolean from the user

- keyword 'message' (String) [Not required] - The message to output to the stream before asking the user for input.
  Default is `""`
- keyword 'optionStyle' (String) [Not required] - The option style to prompt the user with. See below for options and
  examples. Default is `"word"`
- Positional order is `message` then `optionStyle`
- Returns: Boolean
- e.g. `Terminal.readBoolean("Are you a robot? "; optionStyle="char")`

There are four option styles, these are listed below.

- `"word"` - Asks the user to type either `true` or `false` exactly.
- `"number"` - Asks the user to press either the `1` key for true, or the `2` key for false.
- `"char"` - Asks the user to press either the `y` key for true, or the `n` key for false.
- `"binary"` - Asks the user to press either the `0` key for false, or the `1` key for true.

### Variables

This class handles all variables in the codebase

#### Methods

##### create

Creates a variable of the specified type. This class differs from the other methods in Aurora, so please read carefully.

- keyword 'type' (Class) [Required] - The type of the variable. The class must be instantiable.

After the required type, the method expects a keyword argument representing the variable name and its initial value.

Example: `Variables.create(Integer; x=0)` → creates an integer x with value 0

Example: `Variables.create(Boolean; flag)` → creates a boolean flag with default value

Returns: Null

##### edit

Edits the value of a variable.

- There are no keyword arguments for this method.
- Positional value 1 corresponds to the name of the variable you wish to edit, e.g. `x`
- Positional value 2 corresponds to the new value for the variable. This value must match the type that the variable was
  created with.
- Returns: Null
- e.g. `Variables.edit(flag; true)`

### Boolean

#### Methods

##### create

Creates a new Boolean variable, with either a specified value, or the default value if not provided.

- Keyword 'name' (Word) [Required] - The name of the variable.
- Keyword 'value' (Boolean) [Not required] - The value to initialise the variable with. If not provided it uses the
  default value for a Boolean, which is 'False'.
- Positional order is `name` then `value`.
- Returns `Boolean` (The variable that has just been created).
- E.g. `Boolean.create(isRobot; false)`

##### toStyle

Converts a boolean variable to the specified boolean style (listed above in [Terminal.readBoolean](#readboolean)).

- Keyword 'value' (Boolean) [Required] - The boolean value to convert.
- Keyword 'optionStyle' (String) [Required] - The booleanOptionStyle to convert the value into. These can be found
  either in the definition for [Terminal.readBoolean](#readboolean), or in the Boolean class methods section.
- Positional order is `value`, then `optionStyle`.
- Returns: `String` (The provided value in the selected option style)
- E.g. `Boolean.toStyle(false; Boolean.charOptionStyle)` Returns `"n"`
- E.g. `Boolean.toStyle(false; "char")` Returns `"n"`