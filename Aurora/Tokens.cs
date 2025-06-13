using System.Collections.Immutable;
namespace Aurora;

internal abstract class Token
{
   public abstract string Type { get; }

    public abstract object? Value { get; set; }

    public int Length { get => ThrowNotImplemented<int>(); private set => ThrowNotImplemented<int>(); }
    public bool IsCurly { get => ThrowNotImplemented<bool>(); private set => ThrowNotImplemented<bool>(); }
    public bool IsSquare { get => ThrowNotImplemented<bool>(); private set => ThrowNotImplemented<bool>(); }
    public bool IsNormal { get => ThrowNotImplemented<bool>(); private set => ThrowNotImplemented<bool>(); }
    public bool IsOpen { get => ThrowNotImplemented<bool>(); private set => ThrowNotImplemented<bool>(); }
    public bool IsClosed { get => ThrowNotImplemented<bool>(); private set => ThrowNotImplemented<bool>(); }
    public char StartChar { get => ThrowNotImplemented<char>(); private set => ThrowNotImplemented<char>(); }
    public bool IsTrue { get => ThrowNotImplemented<bool>(); private set => ThrowNotImplemented<bool>(); }
    public bool IsEqualTo { get => ThrowNotImplemented<bool>(); private set => ThrowNotImplemented<bool>(); }
    public bool IsNotEqualTo { get => ThrowNotImplemented<bool>(); private set => ThrowNotImplemented<bool>(); }
    public bool IsGreaterThan { get => ThrowNotImplemented<bool>(); private set => ThrowNotImplemented<bool>(); }
    public bool IsLessThan { get => ThrowNotImplemented<bool>(); private set => ThrowNotImplemented<bool>(); }
    public bool IsGreaterEqual { get => ThrowNotImplemented<bool>(); private set => ThrowNotImplemented<bool>(); }
    public bool IsLessEqual { get => ThrowNotImplemented<bool>(); private set => ThrowNotImplemented<bool>(); }
    public bool IsOr { get => ThrowNotImplemented<bool>(); private set => ThrowNotImplemented<bool>(); }
    public bool IsAnd { get => ThrowNotImplemented<bool>(); private set => ThrowNotImplemented<bool>(); }
    public bool IsNot { get => ThrowNotImplemented<bool>(); private set => ThrowNotImplemented<bool>(); }
    public bool IsXor { get => ThrowNotImplemented<bool>(); private set => ThrowNotImplemented<bool>(); }

    public virtual CustomFloat ValueAsFloat { get => ThrowNotImplemented<CustomFloat>(); }

    public virtual CustomInt ValueAsInt { get => ThrowNotImplemented<CustomInt>(); }
    
    public virtual string ValueAsString => this.Value?.ToString() ?? "null";


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
        set {} 
    }

    public BaseToken() {}
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

    public WordToken() {}

    public WordToken Initialise(string value)
    {
        this.Value = value;
        return this;
    }
}

internal class IntegerToken : Token
{
    public const string TokenType = "INTEGER";
    public override string Type { get; } = TokenType;

    private CustomInt? _value;
    public override object? Value
    {
        get => this._value?.Value ?? null;
        set
        {
            this._value = value switch
            {
                string v => new CustomInt(v),
                CustomInt i => i,
                _ => throw new ArgumentException("Integer Tokens must have a string or int value")
            };
        }
    }

    public override CustomInt ValueAsInt => this._value ?? new CustomInt(_value);

    public override CustomFloat ValueAsFloat => new (this._value?.Value.ToString()??"null");

    public override string ValueAsString => this._value?.Value.ToString() ?? "null";


    public IntegerToken Initialise(object value)
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
        {"curly", ImmutableHashSet.Create('{', '}')},
        {"square", ImmutableHashSet.Create('[', ']')},
        {"normal", ImmutableHashSet.Create('(', ')')}
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

    public BracketToken(){}

    public BracketToken Initialise(char value)
    {
        this.Value = value;
        return this;
    }
    
    public static BracketToken OpenNormal = new BracketToken().Initialise('(');
    public static BracketToken CloseNormal = new BracketToken().Initialise(')');
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
                throw new ArgumentException("Strings must have a start and end quote, thus total length must be greater than 2");
            }

            this._value = s[1..^1];
            this.StartChar = s[0];
            char endChar = s[^1];

            if (!START_CHARS.Contains(this.StartChar))
            {
                throw new ArgumentException($"{GlobalVariables.ReprString(s)} does not start with a valid string starter");
            }

            if (this.StartChar != endChar)
            {
                throw new ArgumentException("The start and end characters in a string must be equal");
            }
        }
    }

    public new char StartChar { get; private set; }

    public static string ConvertEscapeSequence(string escapeSequence)
    {
        Dictionary<string, string> escapeSequences = new() {
            {"\\\"", "\""},
            {"\\'", "'"},
            {"\\n", "\n"},
            {@"\\", @"\"},
            {"\\t", "\t"}
        };

        return escapeSequences.GetValueOrDefault(escapeSequence, escapeSequence);
    }

    private static string GetVariableTextValue(string name)
    {
        if (Variables.IsVariable(name))
        {
            string variableValue = Variables.GetVariable(name).ValueAsString;
            GlobalVariables.LOGGER.Verbose($"Interpolating '{name}' as value '{variableValue}'");
            return variableValue;
        }

        Errors.RaiseError(
            new VarNotDefinedError($"The variable '{name}' is not defined. Using literal value instead"));
        return $"{{{name}}}";
    }

    public static string InterpolateString(string value)
    {
        string result = string.Empty;
        bool isBackslash = false;
        string variableName = string.Empty;
        bool trackVariableName = false;
        
        foreach (char character in value)
        {
            if (character == '\\' && !isBackslash)
            {
                isBackslash = true;
                continue;
            }

            if (!isBackslash && character == '{')
            {
                trackVariableName = true;
                continue;
            }

            if (trackVariableName && character == '}')
            {
                trackVariableName = false;
                result += GetVariableTextValue(variableName);
                continue;
            }

            if (trackVariableName)
            {
                variableName += character;
                continue;
            }

            result += character;
        }
        
        return result;
    }

    public override string ValueAsString => $"{this.StartChar}{this._value}{this.StartChar}";

    public StringToken Initialise(string value, bool withoutQuotes = false, bool interpolate = true)
    {
        if (interpolate) value = InterpolateString(value);
        
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

internal class OperatorToken : Token
{
    public const string TokenType = "OPERATOR";
    public static readonly ImmutableHashSet<char> OPERATORS = ['+', '-', '*', '/', '^'];

    public override string Type { get; } = TokenType;

    private char _value;
    public override object? Value
    {
        get => this._value;
        set
        {
            if (value is not char actualValue) throw new ArgumentException("Operators must be a character");
            if (!OPERATORS.Contains(actualValue))
            {
                throw new ArgumentException($"'{actualValue}' is not a valid operator character");
            }

            this._value = actualValue;
        }
    }


    public OperatorToken Initialise(char value)
    {
        this.Value = value;
        return this;
    }
}

internal class BooleanToken : Token
{
    public const string TokenType = "BOOLEAN";
    public static readonly ImmutableHashSet<string> VARS = ["true", "false"];

    public override string Type { get; } = TokenType;

    private bool _value;
    public override object? Value
    {
        get => this._value;
        set 
        {
            if (value is not string or bool) throw new ArgumentException("Boolean tokens must be strings or boolean objects");
            string actualValue = (string)value;
            if (!VARS.Contains(actualValue))
            {
                throw new ArgumentException("Value is not a valid boolean token");
            }

            this._value = actualValue == "true";
            this.IsTrue = actualValue == "true";
        }
    }
    
    public new bool IsTrue { get; private set; }

    public BooleanToken(){}

    public BooleanToken Initialise(string value)
    {
        this.Value = value;
        return this;
    }
}

internal class FloatToken : Token
{
    public const string TokenType = "FLOAT";

    public override string Type { get; } = TokenType;
    
    private CustomFloat? _value;
    public override object? Value
    {
        get => this._value?.Value??new CustomFloat(this._value);
        set
        {
            this._value = value switch
            {
                string v => new CustomFloat(v),
                CustomFloat f => f,
                _ => throw new ArgumentException("Float Tokens must have a string or float value")
            };
        }
    }
    
    public override CustomFloat ValueAsFloat => new (this._value?.Value.ToString()??"Null");

    public override string ValueAsString => this._value?.Value.ToString() ?? "null";

    public FloatToken Initialise(object value)
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

    public EqualsToken(){}
    
    public static EqualsToken TokenEquals = new EqualsToken();
}

internal class ComparisonToken : Token
{
    public const string TokenType = "COMPARISON";

    public static readonly ImmutableHashSet<string> EQUAL = ["==", "Equal"];
    public static readonly ImmutableHashSet<string> NOT_EQUAL = ["!=", "NotEqual"];
    public static readonly ImmutableHashSet<string> GREATER = [">", "Greater"];
    public static readonly ImmutableHashSet<string> LESS = ["<", "Less"];
    public static readonly ImmutableHashSet<string> GREATER_OR_EQUAL = [">=", "GreaterOrEqual"];
    public static readonly ImmutableHashSet<string> LESS_OR_EQUAL = ["<=", "LessOrEqual"];
    public static readonly ImmutableHashSet<string> VALUES = 
        EQUAL
        .Union(NOT_EQUAL)
        .Union(GREATER)
        .Union(LESS)
        .Union(GREATER_OR_EQUAL)
        .Union(LESS_OR_EQUAL)
    ;

    public override string Type { get; } = TokenType;

    private string _value = string.Empty;
    public override object? Value
    {
        get => this._value;
        set
        {
            if (value is not string actualValue) throw new ArgumentException("Comparison tokens must have a string value");
            if (!VALUES.Contains(actualValue))
            {
                throw new ArgumentException($"{GlobalVariables.ReprString(actualValue)} is not a valid comparison token");
            }

            this._value = actualValue;
            this.IsEqualTo = EQUAL.Contains(actualValue);
            this.IsNotEqualTo = NOT_EQUAL.Contains(actualValue);
            this.IsGreaterThan = GREATER.Contains(actualValue);
            this.IsLessThan = LESS.Contains(actualValue);
            this.IsGreaterThan = GREATER_OR_EQUAL.Contains(actualValue);
            this.IsLessEqual = LESS_OR_EQUAL.Contains(actualValue);
        }
    }

    public new bool IsEqualTo { get; private set; }
    public new bool IsNotEqualTo { get; private set; }
    public new bool IsGreaterThan { get; private set; }
    public new bool IsLessThan { get; private set; }
    public new bool IsGreaterEqual { get; private set; }
    public new bool IsLessEqual { get; private set; }

    public ComparisonToken(){}

    public ComparisonToken Initialise(string value)
    {
        this.Value = value;
        return this;
    }
}

internal class BinaryOperationToken : Token
{
    public const string TokenType = "BINARY_OPERATION";
    public static readonly ImmutableHashSet<string> OR = ["||", "Or"];
    public static readonly ImmutableHashSet<string> AND = ["&&", "And"];
    public static readonly ImmutableHashSet<string> NOT = ["!", "Not"];
    public static readonly ImmutableHashSet<string> XOR = ["Xor"];
    public static readonly ImmutableHashSet<string> VALUES = 
        OR
        .Union(AND)
        .Union(NOT)
        .Union(XOR)
    ;

    public override string Type { get; } = TokenType;

    private string _value = string.Empty;
    public override object? Value
    {
        get => this._value;
        set
        {
            if (value is not string actualValue) throw new ArgumentException("BinaryOperation Tokens must be strings");
            if (!VALUES.Contains(actualValue))
            {
                throw new ArgumentException($"{GlobalVariables.ReprString(actualValue)} is not a valid binary token");
            }

            this._value = actualValue;
            this.IsOr = OR.Contains(actualValue);
            this.IsAnd = AND.Contains(actualValue);
            this.IsNot = NOT.Contains(actualValue);
            this.IsXor = XOR.Contains(actualValue);
        }
    }

    public new bool IsOr { get; private set; }
    public new bool IsAnd { get; private set; }
    public new bool IsNot { get; private set; }
    public new bool IsXor { get; private set; }

    public BinaryOperationToken(){}

    public BinaryOperationToken Initialise(string value)
    {
        this.Value = value;
        return this;
    }
}

internal class SeparatorToken : Token
{
    public const string TokenType = "SEPARATOR";
    public readonly static ImmutableHashSet<char> VARS = ['.', ',', ';'];

    public override string Type { get; } = TokenType;

    private char _value;
    public override object? Value
    {
        get => this._value;
        set 
        {
            if (value is null or not char) throw new ArgumentException("Separator token must have a char value");
            char actualValue = (char)value;

            if (!VARS.Contains(actualValue))
            {
                throw new ArgumentException($"'{actualValue}' is not a valid separator token");
            }

            this._value = actualValue;

        }
    }

    public SeparatorToken(){}

    public SeparatorToken Initialise(char value)
    {
        this.Value = value;
        return this;
    }
    
    public static SeparatorToken DotToken = new SeparatorToken().Initialise('.');
    public static SeparatorToken SemiColonToken = new SeparatorToken().Initialise(';');
}

internal class NullToken : Token
{
    public const string TokenType = "NULL";
    
    public override string Type { get; } = TokenType;

    public override object? Value
    {
        get => null;
        set => throw new ArgumentException($"Cannot set a value to NullToken.Value");
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
