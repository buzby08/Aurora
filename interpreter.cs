using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Reflection.Metadata;
using OneOf;
using OneOf.Types;

namespace Aurora
{
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

        public string AsString()
        {
            return $"Token ({this.GetType().Name}) - {this.Value}";
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
        public const string TOKEN_TYPE = "INTEGER";
        public override string Type { get; } = TOKEN_TYPE;

        private int value;
        public override object? Value 
        {
            get => value;
            set
            {
                if (value is not int) throw new ArgumentException("Value must be an integer");

                this.value = (int)value;
            }
        }

        public IntegerToken(int value)
        {
            this.Value = value;
        }
    }

    internal class BracketToken : Token
    {
        public const string TOKEN_TYPE = "BRACKET";
        public readonly static Dictionary<string, ImmutableHashSet<char>> TYPES = new() 
        {
            {"curly", ImmutableHashSet.Create('{', '}')},
            {"square", ImmutableHashSet.Create('[', ']')},
            {"normal", ImmutableHashSet.Create('(', ')')}
        };
        public readonly static ImmutableHashSet<char> OPEN_BRACKETS = ImmutableHashSet.Create('(', '[', '{');
        public readonly static ImmutableHashSet<char> CLOSED_BRACKETS = ImmutableHashSet.Create(')', ']', '}');

        public override string Type { get; } = TOKEN_TYPE;

        private char value;
        public override object? Value 
        {
            get => this.value;
            set
            {
                if (value is not char) throw new ArgumentException("Value must be a character");
                if (!(OPEN_BRACKETS.Contains((char)value) || CLOSED_BRACKETS.Contains((char)value)))
                {
                    throw new ArgumentException($"Value is not a valid bracket character - value='{value}'");
                }

                this.value = (char)value;
                this.IsCurly = TYPES["curly"].Contains(this.value);
                this.IsSquare = TYPES["square"].Contains(this.value);
                this.IsNormal = TYPES["normal"].Contains(this.value);
                this.IsOpen = OPEN_BRACKETS.Contains(this.value);
                this.IsClosed = CLOSED_BRACKETS.Contains(this.value);
            }
        }

        public new bool IsCurly { get; private set; }
        public new bool IsSquare { get; private set; }
        public new bool IsNormal { get; private set; }
        public new bool IsOpen { get; private set; }
        public new bool IsClosed { get; private set; }

        public BracketToken(char value)
        {
            this.Value = value;
        }
    }

    internal class StringToken : Token
    {
        public const string TOKEN_TYPE = "STRING";
        public static readonly ImmutableHashSet<char> START_CHARS = ['"', '\''];

        public override string Type { get; } = TOKEN_TYPE;
        
        private string value = string.Empty;
        public override object? Value 
        {
            get => this.value;
            set
            {
                if (value is not string) throw new ArgumentException("Value must be a string");
                string actualValue = (string)value;
                if (actualValue.Length < 2)
                {
                    throw new ArgumentException("Strings must have a start and end quote, thus total length must be greater than 2");
                }

                this.value = actualValue;
                this.StartChar = actualValue[0];
                char endChar = actualValue[^1];

                if (!START_CHARS.Contains(this.StartChar))
                {
                    throw new ArgumentException($"{GlobalVariables.ReprString(actualValue)} does not start with a valid string starter");
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
                {"\\\\", "\\"},
                {"\\t", "\t"}
            };

            return escapeSequences.TryGetValue(escapeSequence, out var result) ? result : escapeSequence;
        }

        public StringToken(string value)
        {
            this.Value = value;
        }
    }

    internal class OperatorToken : Token
    {
        public const string TOKEN_TYPE = "OPERATOR";
        public static readonly ImmutableHashSet<char> OPERATORS = ['+', '-', '*', '/', '^'];

        public override string Type { get; } = TOKEN_TYPE;

        private char value;
        public override object? Value
        {
            get => this.value;
            set
            {
                if (value is not char) throw new ArgumentException("Operators must be a character");
                char actualValue = (char)value;

                if (!OPERATORS.Contains(actualValue))
                {
                    throw new ArgumentException($"'{actualValue}' is not a valid operator character");
                }

                this.value = actualValue;
            }
        }

        public OperatorToken(char value)
        {
            this.Value = value;
        }
    }

    internal class BooleanToken : Token
    {
        public const string TOKEN_TYPE = "BOOLEAN";
        public static readonly ImmutableHashSet<string> VARS = ["true", "false"];

        public override string Type { get; } = TOKEN_TYPE;

        private bool value;
        public override object? Value
        {
            get => this.value;
            set 
            {
                if (value is not string or bool) throw new ArgumentException("Boolean tokens must be strings or boolean objects");
                string actualValue = (string)value;
                if (!VARS.Contains(actualValue))
                {
                    throw new ArgumentException("Value is not a valid boolean token");
                }

                this.value = actualValue == "true";
                this.IsTrue = actualValue == "true";
            }
        }
        
        public new bool IsTrue { get; private set; }

        public BooleanToken(string value)
        {
            this.Value = value;
        }

        public BooleanToken(bool value)
        {
            this.Value = value;
        }
    }

    internal class FloatToken : Token
    {
        public const string TOKEN_TYPE = "FLOAT";

        public override string Type { get; } = TOKEN_TYPE;
        
        private float value;
        public override object? Value
        {
            get => value;
            set
            {
                if (value is not float) throw new ArgumentException("Float Tokens must have a float value");

                this.value = (float)value;
            }
        }

        public FloatToken(float value)
        {
            this.Value = value;
        }
    }

    internal class EqualsToken : Token
    {
        public const string TOKEN_TYPE = "EQUALS";

        public override string Type { get; } = TOKEN_TYPE;

        public override object? Value
        {
            get => '=';
            set => throw new ArgumentException("Cannot set a value to EqualsToken.Value");
        }

        public EqualsToken(){}
    }

    internal class ComparisonToken : Token
    {
        public const string TOKEN_TYPE = "COMPARISON";

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

        public override string Type { get; } = TOKEN_TYPE;

        private string value = string.Empty;
        public override object? Value
        {
            get => this.value;
            set
            {
                if (value is not string) throw new ArgumentException("Comparison tokens must have a string value");
                string actualValue = (string)value;

                if (!VALUES.Contains(actualValue))
                {
                    throw new ArgumentException($"{GlobalVariables.ReprString(actualValue)} is not a valid comparison token");
                }

                this.value = actualValue;
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

        public ComparisonToken(string value)
        {
            this.Value = value;
        }
    }

    internal class BinaryOperationToken : Token
    {
        public const string TOKEN_TYPE = "BINARY_OPERATION";
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

        public override string Type { get; } = TOKEN_TYPE;

        private string value = string.Empty;
        public override object? Value
        {
            get => this.value;
            set
            {
                if (value is not string) throw new ArgumentException("BinaryOperation Tokens must be strings");
                string actualValue = (string)value;

                if (!VALUES.Contains(actualValue))
                {
                    throw new ArgumentException($"{GlobalVariables.ReprString(actualValue)} is not a valid binary token");
                }

                this.value = actualValue;
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

        public BinaryOperationToken(string value)
        {
            this.Value = value;
        }
    }

    internal class SeparatorToken : Token
    {
        public const string TOKEN_TYPE = "SEPARATOR";
        public readonly static ImmutableHashSet<char> VARS = ['.', ',', ';'];

        public override string Type { get; } = TOKEN_TYPE;

        private char value;
        public override object? Value
        {
            get => this.value;
            set 
            {
                if (value is not char || value is null) throw new ArgumentException("Separator token must have a char value");
                char actualValue = (char)value;

                if (!VARS.Contains(actualValue))
                {
                    throw new ArgumentException($"'{actualValue}' is not a valid separator token");
                }

                this.value = actualValue;

            }
        }

        public SeparatorToken(char value)
        {
            this.Value = value;
        }
    }

    internal class EofToken : Token
    {
        public const string TOKEN_TYPE = "END_OF_FILE";

        public override string Type { get; } = TOKEN_TYPE;

        public override object? Value
        {
            get => null;
            set => throw new ArgumentException("Cannot set a value to EOF_TOKEN.Value");
        }

        public EofToken(){}
    }


    internal class Interpreter
    {
        private string text = string.Empty;
        public string Text 
        {
            get => this.text;
            set
            {
                this.text = value;
                this.pos = 0;
                this.currentToken = null;
            }
        }

        private int pos = 0;
        public int Pos { get => this.pos; }

        private Token? currentToken;
        public Token? CurrentToken { get => this.currentToken; }
        public static ImmutableHashSet<string> Symbols = ["!", "=", "&", "|", "<", ">"];

        public bool IsEof()
        {
            return this.pos > this.text.Length-1;
        }

        public void Advance()
        {
            if (this.pos < this.text.Length)
            {
                this.pos += 1;
            }
        }

        public char? GetCurrentChar()
        {
            if (this.IsEof()) return null;
            return this.text[this.pos];
        }

        private Token Number()
        {
            if (this.IsEof()) return new EofToken();

            string fullNum = string.Empty;
            char? currentChar = this.GetCurrentChar();
            ImmutableHashSet<char> extraChars = ['-', '.'];

            bool Condition(char x) => char.IsDigit(x) || extraChars.Contains(x);

            if (currentChar is not null && !Condition((char)currentChar))
            {
                Error($"Is not digit - number - {currentChar}");
            }

            while (currentChar is not null && Condition((char)currentChar))
            {
                fullNum += currentChar;
                this.Advance();
                currentChar = this.GetCurrentChar();
            }

            if (fullNum.Contains('.'))  return new FloatToken(float.Parse(fullNum));

            if (fullNum == "-") return new OperatorToken('-');

            return new IntegerToken(Convert.ToInt32(fullNum));
        }

        private Token Word()
        {
            if (this.IsEof()) return new EofToken();

            string full_word = string.Empty;
            char? currentChar = this.GetCurrentChar();

            if (!(currentChar is not null && char.IsLetter((char)currentChar)))
            {
                Error($"Is not letter - word - {currentChar}");
            }

            while (currentChar is not null && char.IsLetter((char)currentChar))
            {
                full_word += currentChar;
                this.Advance();
                currentChar = this.GetCurrentChar();
            }

            if (BooleanToken.VARS.Contains(full_word)) return new BooleanToken(full_word);
            if (BinaryOperationToken.VALUES.Contains(full_word)) return new BinaryOperationToken(full_word);
            if (ComparisonToken.VALUES.Contains(full_word)) return new ComparisonToken(full_word);

            return new WordToken().Initialise(full_word);
        }

        Token String()
        {
            if (this.IsEof()) return new EofToken();

            string fullString = string.Empty;
            char? currentChar = this.GetCurrentChar();

            if (currentChar is not null && !StringToken.START_CHARS.Contains((char)currentChar))
            {
                Error($"Is not string start character - string - {currentChar}");
            }

            char? startChar = null;
            string escapeSequence = string.Empty;

            while (currentChar is not null)
            {
                this.Advance();

                if (escapeSequence == "\\")
                {
                    escapeSequence += currentChar;
                    fullString += StringToken.ConvertEscapeSequence(escapeSequence);
                    escapeSequence = string.Empty;
                    currentChar = this.GetCurrentChar();
                    continue;
                }

                if (string.IsNullOrEmpty(escapeSequence) && currentChar == '\\')
                {
                    escapeSequence += currentChar;
                    currentChar = this.GetCurrentChar();
                    continue;
                }

                fullString += currentChar;
                if (startChar is not null & currentChar == startChar) break;

                if (startChar is null && StringToken.START_CHARS.Contains((char)currentChar))
                {
                    startChar = currentChar;
                }

                currentChar = this.GetCurrentChar();
            }

            return new StringToken(fullString);
        }

        Token Symbol()
        {
            if (this.IsEof()) return new EofToken();

            string fullSymbol = string.Empty;
            char? currentChar = this.GetCurrentChar();

            if (currentChar is null || !Symbols.Contains($"{currentChar}"))
            {
                Error($"Error - is not symbol character - '{currentChar}'");
            }

            while (currentChar is not null && Symbols.Contains($"{currentChar}"))
            {
                fullSymbol += currentChar;
                this.Advance();
                currentChar = this.GetCurrentChar();
            }

            if (ComparisonToken.VALUES.Contains(fullSymbol)) return new ComparisonToken(fullSymbol);
            if (BinaryOperationToken.VALUES.Contains(fullSymbol)) return new BinaryOperationToken(fullSymbol);
            if ("=" == fullSymbol) return new EqualsToken();

            Error($"Invalid symbol - '{fullSymbol}'");
            throw new UnreachableException();
        }

        public Token GetNextToken()
        {
            while (true)
            {
                char? currentChar = this.GetCurrentChar();

                if (currentChar is null || this.IsEof()) return new EofToken(); 

                if (char.IsDigit((char)currentChar) || currentChar == '-') return this.Number();

                if (char.IsLetter((char) currentChar)) return this.Word();

                if (OperatorToken.OPERATORS.Contains((char)currentChar))
                {
                    this.Advance();
                    return new OperatorToken((char)currentChar);
                }

                if (BracketToken.OPEN_BRACKETS.Contains((char)currentChar) || BracketToken.CLOSED_BRACKETS.Contains((char)currentChar))
                {
                    this.Advance();
                    return new BracketToken((char)currentChar);
                }

                if (StringToken.START_CHARS.Contains((char)currentChar)) return this.String();

                if (SeparatorToken.VARS.Contains((char)currentChar))
                {
                    this.Advance();
                    return new SeparatorToken((char)currentChar);
                }

                if (Symbols.Contains($"{currentChar}")) return this.Symbol();
                
                if (char.IsWhiteSpace((char)currentChar))
                {
                    this.Advance();
                    continue;
                }

                Error($"Invalid character - {currentChar}");
            }
        }

        public List<Token> GetAllTokens()
        {
            List<Token> allTokens = [];

            Token currentToken = this.GetNextToken();

            while (currentToken.Type != EofToken.TOKEN_TYPE)
            {
                allTokens.Add(currentToken);
            }

            return allTokens;
        } 

        [DoesNotReturn]
        public static void Error(string message)
        {
            Console.WriteLine("\n\nUnhandled exception:");
            Console.WriteLine(message);
            Environment.Exit(1);
        }
    }

    static class Match
    {
        public static List<Token> SplitToTokens(string line)
        {
            Interpreter interpreter = new()
            {
                Text = line
            };
            return interpreter.GetAllTokens();
        }

        public static bool CheckType(Token a, Type b)
        {
            string? otherTokenType = b.GetField("TOKEN_TYPE", BindingFlags.Static | BindingFlags.Public)?.GetValue(null) as string;
            return a.Type == otherTokenType || otherTokenType == BaseToken.TokenType;
        }

        public static bool MatchTokens(List<Token> tokens, List<(List<Type>, int)> sequence)
        {
            int tokenIndex = 0;
            int sequenceIndex = 0;

            while (sequenceIndex < sequence.Count)
            {
                (List<Type> expected, int count) = sequence[sequenceIndex];

                bool isFixedCount = count >= 1;
                bool isGreedy = count == -1;
                bool isOptional = count == 0;

                if (isFixedCount)
                {
                    for (int _ = 0; _ < count; _++)
                    {
                        if (tokenIndex >= tokens.Count) return false;

                        if (!expected.Any(e => CheckType(tokens[tokenIndex], e))) return false;

                        tokenIndex++;
                    }
                }
                else if (isOptional)
                {
                    bool tokenLengthValid = tokenIndex < tokens.Count;
                    bool tokenFound = expected.Any(e => CheckType(tokens[tokenIndex], e));
                    if (tokenLengthValid && tokenFound) tokenIndex++;
                }
                else if (isGreedy)
                {
                    int greedyStart = tokenIndex;
                    bool tokenIndexValid = tokenIndex < tokens.Count;
                    
                    while (
                        tokenIndexValid
                        && expected.Any(e => CheckType(tokens[tokenIndex], e))
                    )
                    {
                        tokenIndex++;
                        tokenIndexValid = tokenIndex < tokens.Count;
                    }

                    for (int backtrack = tokenIndex; backtrack > tokenIndex; backtrack--)
                    {
                        List<Token> remainingTokens = tokens[backtrack..];
                        List<(List<Type>, int)> remainingSequence = sequence[(sequenceIndex+1)..];

                        if (MatchTokens(remainingTokens, remainingSequence)) return true;
                    }

                    return false;
                }

                sequenceIndex++;
            }

            return tokenIndex == tokens.Count;
        }
    }
}