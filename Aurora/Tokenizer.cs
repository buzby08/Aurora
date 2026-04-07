using System.Diagnostics.CodeAnalysis;

namespace Aurora;

internal class Tokenizer
{
    private string _text = string.Empty;

    /// <summary>
    /// The text for the tokenizer.
    /// </summary>
    public string Text
    {
        get => this._text;
        set
        {
            this._text = value;
            this.Position = 0;
            this._cachedTokenList = null;
        }
    }

    private TokenList? _cachedTokenList;

    /// <summary>
    /// The current character position in the <see cref="Text"/> input.
    /// </summary>
    public int Position { get; private set; }

    /// <summary>
    /// Checks if the tokenizer has reached the end of its input.
    /// </summary>
    /// <returns>Boolean to show if EOF is reached.</returns>
    public bool IsEof()
    {
        return this.Position > this._text.Length - 1;
    }

    /// <summary>
    /// Advances the interpreter to the next position.
    /// </summary>
    private void Advance()
    {
        if (this.Position < this._text.Length)
        {
            this.Position += 1;
        }
    }

    /// <summary>
    /// Gets the character at the interpreters current position.
    /// </summary>
    /// <returns>The current character, or if no characters remaining, null.</returns>
    public char? GetCurrentChar()
    {
        if (this.IsEof()) return null;
        return this._text[this.Position];
    }

    /// <summary>
    /// Starting at the current character, this gets the next block of numbers, including negative and decimal symbols,
    /// and returns a <see cref="NumberToken"/>. If the current character does not meet the requirements, the
    /// <see cref="Error"/> method will be called. If the current character is a negative symbol, and no following
    /// characters meet the requirements, an <see cref="SymbolToken"/> containing the negative symbol will be returned.
    /// </summary>
    /// <returns>The generated <see cref="NumberToken"/> or <see cref="SymbolToken"/>.</returns>
    /// <remarks>"Negative symbol" = `-` and "Decimal symbol" = `.`</remarks>
    private Token Number()
    {
        if (this.IsEof()) return new EofToken();

        string fullNum = string.Empty;
        char? currentChar = this.GetCurrentChar();

        bool isFirstItem = true;

        if (currentChar is not null && !Condition((char)currentChar, firstItem: isFirstItem))
        {
            this.Error($"Is not digit - number - '{currentChar}'");
        }

        while (currentChar is not null && Condition((char)currentChar, firstItem: isFirstItem))
        {
            isFirstItem = false;
            fullNum += currentChar;
            this.Advance();
            currentChar = this.GetCurrentChar();
        }

        if (fullNum.EndsWith('.'))
        {
            this.Position -= 1;
            fullNum = fullNum[..^1];
        }

        if (fullNum == "-") return new SymbolToken().Initialise("-");

        return new NumberToken().Initialise(fullNum);

        bool Condition(char x, bool firstItem = false) =>
            char.IsDigit(x) || (x == '.' && !firstItem) || (x == '-' && firstItem);
    }

    /// <summary>
    /// Staring at the current character, this gets the next block of letters, and returns a <see cref="WordToken"/>. If
    /// the current character does not meet the requirements, the <see cref="Error"/> method will be called.
    /// </summary>
    /// <returns>The generated <see cref="WordToken"/>.</returns>
    /// <remarks>
    /// "Letters" means any unicode letter, as defined by the .NET <see cref="char.IsLetter(char)"/> method. More
    /// information can be found at https://learn.microsoft.com/en-us/dotnet/api/System.Char.IsLetter?view=net-9.0
    /// </remarks>
    private Token Word()
    {
        if (this.IsEof()) return new EofToken();

        string fullWord = string.Empty;
        char? currentChar = this.GetCurrentChar();

        if (!(currentChar is not null && char.IsLetter((char)currentChar)))
        {
            this.Error($"Is not letter - word - {currentChar}");
        }

        while (currentChar is not null && char.IsLetter((char)currentChar))
        {
            fullWord += currentChar;
            this.Advance();
            currentChar = this.GetCurrentChar();
        }

        return new WordToken().Initialise(fullWord);
    }

    /// <summary>
    /// Starting at the current character, this gets the next string, and returns a <see cref="StringToken"/>. A string
    /// is a block of characters that starts with `"`, or `'`, and ends with the same
    /// character used to start (ie. if the string started with `"`, it must end with `"`).
    /// Any characters in between the starting and ending characters are assumed part of the string, even if they meet
    /// the conditions of other tokens. Escape characters for the starting and ending characters of the string are
    /// handled by this method (I.e. if the string starts with `"`, and while searching through the
    /// characters to find the closing `"`, if ascii character `\` is found, with `"`
    /// following, the method will continue to find the next `"` without a leading ascii character `\`.
    /// This is the same as if `'` is used to start the sequence, and an ascii character `\` is found
    /// preceding an `'`).
    /// </summary>
    /// <returns>
    /// A <see cref="StringToken"/> with the text from the string, not including the start and end characters. Any
    /// escape sequences will be left unescaped, and it is the role of the parser to escape them.
    /// </returns>
    /// <remarks>
    /// `"` is ascii character 34, and `'` is ascii character 39. '\' is ascii character 92. All ascii values are in the
    /// decimal format.
    /// </remarks>
    private Token String()
    {
        if (this.IsEof()) return new EofToken();

        string fullString = string.Empty;
        char? currentChar = this.GetCurrentChar();

        if (currentChar is not null && !StringToken.START_CHARS.Contains((char)currentChar))
        {
            this.Error($"Is not string start character - string - {currentChar}");
        }

        char? startChar = null;
        string escapeSequence = string.Empty;

        while (currentChar is not null)
        {
            this.Advance();

            bool currentCharIsForEscapeSequence =
                escapeSequence.Length > 0 && StringToken.START_CHARS.Contains((char)currentChar);

            if (!currentCharIsForEscapeSequence) escapeSequence = string.Empty;

            if (currentCharIsForEscapeSequence)
            {
                fullString += currentChar;
                currentChar = this.GetCurrentChar();
                continue;
            }

            if (escapeSequence.Length == 0 && currentChar == '\\')
            {
                escapeSequence = "\\";
                fullString += currentChar;
                currentChar = this.GetCurrentChar();
                continue;
            }

            fullString += currentChar;
            if (startChar is not null && currentChar == startChar) break;

            if (startChar is null && StringToken.START_CHARS.Contains((char)currentChar))
            {
                startChar = currentChar;
            }

            currentChar = this.GetCurrentChar();
        }

        return new StringToken().Initialise(fullString);
    }

    /// <summary>
    /// Starting at the current character, this character gets the next block of symbols. A list of the symbols can be
    /// at <see cref="SymbolToken.VARS"/>, and also includes `=` and `.`. If the current character does
    /// not meet the requirements, the <see cref="Error"/> method will be called. When the symbol is created, if the
    /// full generated symbol is not a valid symbol, the <see cref="SymbolToken"/> initialisation will throw an error to
    /// the user.
    /// </summary>
    /// <returns>The generated <see cref="SymbolToken"/>.</returns>
    /// <remarks>
    /// `=` is ascii character 61. `.` is ascii character 46. All ascii values are in the decimal format.
    /// </remarks>
    private Token Symbol()
    {
        if (this.IsEof()) return new EofToken();

        char? currentChar = this.GetCurrentChar();
        this.Advance();

        if (currentChar == '=')
            return new EqualsToken();

        if (currentChar == '.')
            return new DotToken();

        return new SymbolToken().Initialise(currentChar!.Value.ToString());
    }

    /// <summary>
    /// This gets the next token in the input text stored at <see cref="Text"/> and advances the pointer to match.
    /// If the current character doesn't match any of the valid tokens, the <see cref="Error"/> method will be called.
    /// </summary>
    /// <returns>The generated token.</returns>
    public Token GetNextToken()
    {
        while (true)
        {
            char? currentChar = this.GetCurrentChar();

            if (currentChar is null || this.IsEof()) return new EofToken();

            if (char.IsDigit((char)currentChar) || currentChar == '-') return this.Number();

            if (char.IsLetter((char)currentChar)) return this.Word();

            if (BracketToken.OPEN_BRACKETS.Contains((char)currentChar) ||
                BracketToken.CLOSED_BRACKETS.Contains((char)currentChar))
            {
                this.Advance();
                return new BracketToken().Initialise((char)currentChar);
            }

            if (StringToken.START_CHARS.Contains((char)currentChar)) return this.String();

            if (SymbolToken.VARS.Contains(currentChar.ToString()!))
            {
                return this.Symbol();
            }

            if ((char)currentChar == '.')
            {
                this.Advance();
                return new DotToken().Initialise();
            }

            if ((char)currentChar == '=') return this.Symbol();

            if (char.IsWhiteSpace((char)currentChar))
            {
                this.Advance();
                continue;
            }

            this.Error($"Invalid character - {currentChar}");
        }
    }

    /// <summary>
    /// This converts the interpreters text (see <see cref="Text"/>) to a <see cref="TokenList"/>. The result is cached
    /// for speed. This does not include the trailing <see cref="EofToken"/>.
    /// </summary>
    /// <returns>A <see cref="TokenList"/> with all the tokens in the input text.</returns>
    public TokenList GetAllTokens()
    {
        if (this._cachedTokenList is not null) return this._cachedTokenList;

        TokenList allTokens = new();

        Token currentToken = this.GetNextToken();

        while (currentToken.Type != EofToken.TokenType)
        {
            allTokens.Add(currentToken);
            currentToken = this.GetNextToken();
        }

        this._cachedTokenList = allTokens;

        return allTokens;
    }

    /// <summary>
    /// Throws an error to the user that something has gone wrong with the tokeniser. This will be treated as a
    /// <see cref="SystemError"/>. This method exits execution.
    /// </summary>
    /// <param name="message">The error message for the user.</param>
    [DoesNotReturn]
    private void Error(string message)
    {
        Errors.AlwaysThrow(new SystemError($"Tokenizer error: {message}"), position: this.Position);
        Environment.Exit(1);
    }
}