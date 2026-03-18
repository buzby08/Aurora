using System.Collections.Immutable;

namespace Aurora;

internal abstract class Token
{
    public abstract string Type { get; }

    public abstract object? Value { get; set; }

    public int Length
    {
        get => ThrowNotImplemented<int>();
        private set => ThrowNotImplemented<int>();
    }

    public bool IsCurly
    {
        get => ThrowNotImplemented<bool>();
        private set => ThrowNotImplemented<bool>();
    }

    public bool IsSquare
    {
        get => ThrowNotImplemented<bool>();
        private set => ThrowNotImplemented<bool>();
    }

    public bool IsNormal
    {
        get => ThrowNotImplemented<bool>();
        private set => ThrowNotImplemented<bool>();
    }

    public bool IsOpen
    {
        get => ThrowNotImplemented<bool>();
        private set => ThrowNotImplemented<bool>();
    }

    public bool IsClosed
    {
        get => ThrowNotImplemented<bool>();
        private set => ThrowNotImplemented<bool>();
    }

    public char StartChar
    {
        get => ThrowNotImplemented<char>();
        private set => ThrowNotImplemented<char>();
    }

    public bool IsTrue
    {
        get => ThrowNotImplemented<bool>();
        private set => ThrowNotImplemented<bool>();
    }

    public bool IsEqualTo
    {
        get => ThrowNotImplemented<bool>();
        private set => ThrowNotImplemented<bool>();
    }

    public bool IsNotEqualTo
    {
        get => ThrowNotImplemented<bool>();
        private set => ThrowNotImplemented<bool>();
    }

    public bool IsGreaterThan
    {
        get => ThrowNotImplemented<bool>();
        private set => ThrowNotImplemented<bool>();
    }

    public bool IsLessThan
    {
        get => ThrowNotImplemented<bool>();
        private set => ThrowNotImplemented<bool>();
    }

    public bool IsGreaterEqual
    {
        get => ThrowNotImplemented<bool>();
        private set => ThrowNotImplemented<bool>();
    }

    public bool IsLessEqual
    {
        get => ThrowNotImplemented<bool>();
        private set => ThrowNotImplemented<bool>();
    }

    public bool IsOr
    {
        get => ThrowNotImplemented<bool>();
        private set => ThrowNotImplemented<bool>();
    }

    public bool IsAnd
    {
        get => ThrowNotImplemented<bool>();
        private set => ThrowNotImplemented<bool>();
    }

    public bool IsNot
    {
        get => ThrowNotImplemented<bool>();
        private set => ThrowNotImplemented<bool>();
    }

    public bool IsXor
    {
        get => ThrowNotImplemented<bool>();
        private set => ThrowNotImplemented<bool>();
    }

    public virtual CustomFloat ValueAsFloat
    {
        get => ThrowNotImplemented<CustomFloat>();
    }

    public virtual CustomInt ValueAsInt
    {
        get => ThrowNotImplemented<CustomInt>();
    }

    public virtual string ValueAsString => this.Value?.ToString() ?? "null";

    public virtual bool ValueAsBool
    {
        get => ThrowNotImplemented<bool>();
    }

    public int ValueLength => ValueAsString?.Length ?? 0;

    public string AsString()
    {
        return $"Token ({this.GetType().Name}) - {this.Value}";
    }

    public bool Equals(Token other)
    {
        return this.ValueAsString == other.ValueAsString && this.Type == other.Type;
    }


    private static T ThrowNotImplemented<T>()
    {
        throw new NotImplementedException();
    }
}

internal class BaseToken : Token
{
    public const string TokenType = "BaseToken";
    public override string Type { get; } = TokenType;

    public override object? Value
    {
        get => null;
        set { }
    }
}

internal class DotToken : Token
{
    public const string TokenType = "DOT";
    public override string Type { get; } = TokenType;
    private char _value = '.';

    public override object? Value
    {
        get => this._value;
        set => Errors.AlwaysThrow(new UnsupportedOperationError("Cannot set a value to a dotToken", user: false));
    }

    public DotToken Initialise()
    {
        return this;
    }
}

internal class WordToken : Token
{
    public const string TokenType = "WORD";
    public override string Type { get; } = TokenType;

    private string _value = string.Empty;

    public override object? Value
    {
        get => this._value;
        set
        {
            if (value is not string s)
                throw new ArgumentException("Value must be a string.");

            this._value = s;
            this.Length = this._value.Length;
        }
    }

    // Public length property (derived from the value)
    public new int Length { get; private set; }

    public WordToken Initialise(string value)
    {
        this.Value = value;
        return this;
    }
}

internal class BracketToken : Token
{
    public const string TokenType = "BRACKET";

    public static readonly Dictionary<string, ImmutableHashSet<char>> TYPES = new()
    {
        { "curly", ImmutableHashSet.Create('{', '}') },
        { "square", ImmutableHashSet.Create('[', ']') },
        { "normal", ImmutableHashSet.Create('(', ')') }
    };

    public static readonly ImmutableHashSet<char> OPEN_BRACKETS = ImmutableHashSet.Create('(', '[', '{');
    public static readonly ImmutableHashSet<char> CLOSED_BRACKETS = ImmutableHashSet.Create(')', ']', '}');

    public override string Type { get; } = TokenType;

    private char _value;

    public override object? Value
    {
        get => this._value;
        set
        {
            if (value is not char c) throw new ArgumentException("Value must be a character");
            if (!(OPEN_BRACKETS.Contains(c) || CLOSED_BRACKETS.Contains(c)))
            {
                throw new ArgumentException($"Value is not a valid bracket character - value='{value}'");
            }

            this._value = c;
            this.IsCurly = TYPES["curly"].Contains(this._value);
            this.IsSquare = TYPES["square"].Contains(this._value);
            this.IsNormal = TYPES["normal"].Contains(this._value);
            this.IsOpen = OPEN_BRACKETS.Contains(this._value);
            this.IsClosed = CLOSED_BRACKETS.Contains(this._value);
        }
    }

    public new bool IsCurly { get; private set; }
    public new bool IsSquare { get; private set; }
    public new bool IsNormal { get; private set; }
    public new bool IsOpen { get; private set; }
    public new bool IsClosed { get; private set; }

    public BracketToken Initialise(char value)
    {
        this.Value = value;
        return this;
    }

    public static readonly BracketToken OpenNormal = new BracketToken().Initialise('(');
    public static readonly BracketToken CloseNormal = new BracketToken().Initialise(')');
    public static readonly BracketToken OpenSquare = new BracketToken().Initialise('[');
    public static readonly BracketToken CloseSquare = new BracketToken().Initialise(']');
    public static readonly BracketToken OpenCurly = new BracketToken().Initialise('{');
    public static readonly BracketToken CloseCurly = new BracketToken().Initialise('}');
}

internal class StringToken : Token
{
    public const string TokenType = "STRING";
    public static readonly ImmutableHashSet<char> START_CHARS = ['"', '\''];

    public override string Type { get; } = TokenType;

    private string _value = string.Empty;

    public override object? Value
    {
        get => this._value;
        set
        {
            if (value is not string s) throw new ArgumentException("Value must be a string");
            if (s.Length < 2)
            {
                throw new ArgumentException(
                    "Strings must have a start and end quote, thus total length must be greater than 2");
            }

            this._value = s[1..^1];
            this.StartChar = s[0];
            char endChar = s[^1];

            if (!START_CHARS.Contains((char)this.StartChar!))
            {
                throw new ArgumentException(
                    $"{null /*GlobalVariables.ReprString(s)*/} does not start with a valid string starter");
            }

            if (this.StartChar != endChar)
            {
                throw new ArgumentException("The start and end characters in a string must be equal");
            }
        }
    }

    public new char? StartChar { get; private set; }

    // public static string ConvertEscapeSequence(string escapeSequence)
    // {
    //     Dictionary<string, string> escapeSequences = new()
    //     {
    //         { "\\\"", "\"" },
    //         { "\\'", "'" },
    //         { "\\n", "\n" },
    //         { @"\\", @"\" },
    //         { "\\t", "\t" }
    //     };
    //
    //     return escapeSequences.GetValueOrDefault(escapeSequence, escapeSequence);
    // }

    public override string ValueAsString => _value;
    public new int ValueLength => StartChar is not null ? 2 + ValueLength : base.ValueLength;

    public StringToken Initialise(string value, bool withoutQuotes = false)
    {
        // if (interpolate) value = InterpolateString(value);

        switch (withoutQuotes)
        {
            case true:
                this._value = value;
                break;
            case false:
                this.Value = value;
                break;
        }

        return this;
    }
}

internal class NumberToken : Token
{
    public const string TokenType = "NUMBER";

    public override string Type { get; } = TokenType;

    private CustomFloat? _value;

    public override object? Value
    {
        get => this._value?.Value ?? new CustomFloat(this._value);
        set
        {
            this._value = value switch
            {
                string v => new CustomFloat(v),
                CustomFloat f => f,
                CustomInt f => new CustomFloat(f),
                _ => throw new ArgumentException("Float Tokens must have a string, int, or float value")
            };
        }
    }

    public override CustomFloat ValueAsFloat => new(this._value?.Value.ToString() ?? "Null");

    public override string ValueAsString => this._value?.Value.ToString() ?? "null";

    public NumberToken Initialise(object value)
    {
        this.Value = value;
        return this;
    }
}

internal class EqualsToken : Token
{
    public const string TokenType = "EQUALS";

    public override string Type { get; } = TokenType;

    public override object? Value
    {
        get => '=';
        set => throw new ArgumentException("Cannot set a value to EqualsToken.Value");
    }

    public static EqualsToken TokenEquals = new EqualsToken();
}

internal class SymbolToken : Token
{
    public const string TokenType = "SYMBOL";

    public static readonly ImmutableHashSet<string> VARS =
        [",", ";", "+", "-", "*", "/", "^", ">", "<", "!", "==", "!=", ">=", "<=", "||", "&&"];

    public override string Type { get; } = TokenType;

    private string _value;

    public override object? Value
    {
        get => this._value;
        set
        {
            if (value is null or not string) throw new ArgumentException("Separator token must have a string value");
            string actualValue = (string)value;

            if (!VARS.Contains(actualValue))
            {
                throw new ArgumentException($"'{actualValue}' is not a valid separator token");
            }

            this._value = actualValue;
        }
    }

    public SymbolToken Initialise(string value)
    {
        this.Value = value;
        return this;
    }
}

internal class EofToken : Token
{
    public const string TokenType = "END_OF_FILE";

    public override string Type { get; } = TokenType;

    public override object? Value
    {
        get => null;
        set => throw new ArgumentException("Cannot set a value to EOF_TOKEN.Value");
    }
}