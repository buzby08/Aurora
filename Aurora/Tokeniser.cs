using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Aurora;

internal class Tokeniser
{
    private string _text = string.Empty;

    public string Text
    {
        get => this._text;
        set
        {
            this._text = value;
            this.Pos = 0;
            this.CurrentToken = null;
        }
    }

    public int Pos { get; private set; } = 0;

    public Token? CurrentToken { get; private set; }
    public static readonly ImmutableHashSet<string> SYMBOLS = ["!", "=", "&", "|", "<", ">"];

    public bool IsEof()
    {
        return this.Pos > this._text.Length - 1;
    }

    private void Advance()
    {
        if (this.Pos < this._text.Length)
        {
            this.Pos += 1;
        }
    }

    public char? GetCurrentChar()
    {
        if (this.IsEof()) return null;
        return this._text[this.Pos];
    }

    private Token Number()
    {
        if (this.IsEof()) return new EofToken();

        string fullNum = string.Empty;
        char? currentChar = this.GetCurrentChar();

        bool isFirstItem = true;

        if (currentChar is not null && !Condition((char)currentChar, firstItem: isFirstItem))
        {
            Error($"Is not digit - number - '{currentChar}'");
        }

        while (currentChar is not null && Condition((char)currentChar, firstItem: isFirstItem))
        {
            isFirstItem = false;
            fullNum += currentChar;
            this.Advance();
            currentChar = this.GetCurrentChar();
        }

        if (fullNum.Contains('.')) return new FloatToken().Initialise(fullNum);

        if (fullNum == "-") return new OperatorToken().Initialise('-');

        return new IntegerToken().Initialise(fullNum);

        bool Condition(char x, bool firstItem = false) =>
            char.IsDigit(x) || (x == '.' && !firstItem) || (x == '-' && firstItem);
    }

    private Token Word()
    {
        if (this.IsEof()) return new EofToken();

        string fullWord = string.Empty;
        char? currentChar = this.GetCurrentChar();

        if (!(currentChar is not null && char.IsLetter((char)currentChar)))
        {
            Error($"Is not letter - word - {currentChar}");
        }

        while (currentChar is not null && char.IsLetter((char)currentChar))
        {
            fullWord += currentChar;
            this.Advance();
            currentChar = this.GetCurrentChar();
        }

        if (BooleanToken.VARS.Contains(fullWord)) return new BooleanToken().Initialise(fullWord);
        if (BinaryOperationToken.VALUES.Contains(fullWord)) return new BinaryOperationToken().Initialise(fullWord);
        if (ComparisonToken.VALUES.Contains(fullWord)) return new ComparisonToken().Initialise(fullWord);

        return new WordToken().Initialise(fullWord);
    }

    private Token String()
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

        return new StringToken().Initialise(fullString);
    }

    private Token Symbol()
    {
        if (this.IsEof()) return new EofToken();

        string fullSymbol = string.Empty;
        char? currentChar = this.GetCurrentChar();

        if (currentChar is null || !SYMBOLS.Contains($"{currentChar}"))
        {
            Error($"Error - is not symbol character - '{currentChar}'");
        }

        while (currentChar is not null && SYMBOLS.Contains($"{currentChar}"))
        {
            fullSymbol += currentChar;
            this.Advance();
            currentChar = this.GetCurrentChar();
        }

        if (ComparisonToken.VALUES.Contains(fullSymbol)) return new ComparisonToken().Initialise(fullSymbol);
        if (BinaryOperationToken.VALUES.Contains(fullSymbol)) return new BinaryOperationToken().Initialise(fullSymbol);
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

            if (char.IsLetter((char)currentChar)) return this.Word();

            if (OperatorToken.OPERATORS.Contains((char)currentChar))
            {
                this.Advance();
                return new OperatorToken().Initialise((char)currentChar);
            }

            if (BracketToken.OPEN_BRACKETS.Contains((char)currentChar) ||
                BracketToken.CLOSED_BRACKETS.Contains((char)currentChar))
            {
                this.Advance();
                return new BracketToken().Initialise((char)currentChar);
            }

            if (StringToken.START_CHARS.Contains((char)currentChar)) return this.String();

            if (SeparatorToken.VARS.Contains((char)currentChar))
            {
                this.Advance();
                return new SeparatorToken().Initialise((char)currentChar);
            }

            if ((char)currentChar == '.')
            {
                this.Advance();
                return new DotToken().Initialise();
            }

            if (SYMBOLS.Contains($"{currentChar}")) return this.Symbol();

            if (char.IsWhiteSpace((char)currentChar))
            {
                this.Advance();
                continue;
            }

            Error($"Invalid character - {currentChar}");
        }
    }

    public TokenList GetAllTokens()
    {
        TokenList allTokens = new();

        Token currentToken = this.GetNextToken();

        while (currentToken.Type != EofToken.TokenType)
        {
            allTokens.Add(currentToken);
            currentToken = this.GetNextToken();
        }

        return allTokens;
    }

    // Todo: Loop over all tokens, generate ast's as needed (method call ast, attribute ast). This should be done by the
    //      evaluator
    // Todo: Evaluate all Ast's
    // Todo: Build all required classes (Variables, Int, Float, String, Boolean, Terminal, Logic)


    private GetPatternResult GetMethodCall(List<Token> tokens)
    {
        return new GetPatternResult { Tokens = tokens, TokensSkipped = tokens.Count };
        // List<Token> allTokens = this.GetAllTokens();
        // List<Token> methodCallTokens = [];
        //
        // List<string> pattern = [DotToken.TokenType, WordToken.TokenType, BracketToken.TokenType];
        //
        // int patternIndex = 0;
        // int bracketDepth = 0;
        //
        // foreach (Token token in allTokens)
        // {
        //     bool isInsideParenthesis = bracketDepth > 0;
        //     bool isBracketToken = token is BracketToken;
        //     bool matchesNextItemInPattern = pattern[patternIndex] == token.Type;
        //     
        //     if (isInsideParenthesis && !isBracketToken)
        //     {
        //         methodCallTokens.Add(token);
        //         continue;
        //     }
        //     
        //     if (!isBracketToken && matchesNextItemInPattern)
        //     {
        //         patternIndex++;
        //         methodCallTokens.Add(token);
        //         continue;
        //     }
        //     
        //     if (patternIndex == 0) continue;
        //
        //     if (!isInsideParenthesis && token is BracketToken { IsClosed: true, IsNormal: true })
        //     {
        //         Logs.Warning($"Tokeniser.GetMethodCall - bracket depth is 0, and token is a close token, and is normal");
        //         return null;
        //     }
        //
        //     if (token is BracketToken { IsOpen: true, IsNormal: true })
        //     {
        //         bracketDepth++;
        //         methodCallTokens.Add(token);
        //     }
        //     
        //     if (bracketDepth > 1 && token is BracketToken { IsClosed: true, IsNormal: true })
        //     {
        //         bracketDepth--;
        //         methodCallTokens.Add(token);
        //     }
        //     
        //     if (bracketDepth == 1 && token is BracketToken { IsClosed: true, IsNormal: true })
        //     {
        //         methodCallTokens.Add(token);
        //         return methodCallTokens;
        //     }
        //     
        //     methodCallTokens.Add(token);
        // }
        //
        // return null;
    }

    [DoesNotReturn]
    public static void Error(string message)
    {
        Console.WriteLine("\n\nUnhandled exception:");
        Console.WriteLine(message);
        Environment.Exit(1);
    }

    private struct GetPatternResult(List<Token>? tokens, int tokensSkipped)
    {
        public required List<Token>? Tokens = tokens;
        public required int TokensSkipped = tokensSkipped;
    }
}