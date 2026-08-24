#define TESTING

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Aurora.Core;

namespace Aurora.Parser;

public class ParserRework
{
    private static ParserOptions _options = new();
    private static int CurrentArgumentRecursionDepth { get; set; }
    private static int CurrentExpressionRecursionDepth { get; set; }

    private int _id = IdGenerator.GenerateId("ParserRework");
    private Logger _logger;
    private InternalCallPoint? CallPoint { get; }
    private ParserMode Mode { get; set; } = ParserMode.Expression;

    private List<List<AstRework>> Expressions { get; } = [];
    public List<AstRework> CurrentExpression { get; private set; } = [];

    private TokenList Tokens { get; }
    private int CurrentIndex { get; set; }
    private ParserState State { get; set; } = ParserState.Empty;
    private AstRework? CurrentAst { get; set; }

    private TokenListItem? Action { get; set; }
    private ICollection<ArgumentRework>? Arguments { get; set; }
    private IEnumerable<IEnumerable<AstRework>>? BlockValue { get; set; }

    public ParserRework(Tokenizer tokenizer, ParserOptions? options = null)
    {
        _options = options ?? _options;

        this.Tokens = tokenizer.GetAllTokens();
        this._logger = new Logger($"ParserRework #{this._id}");
    }

    private ParserRework(IEnumerable<TokenListItem> tokens, InternalCallPoint callPoint)
    {
        this.CallPoint = callPoint;
        TokenList tokenList = new(tokens);
        this.Tokens = tokenList;
        this._logger = new Logger($"ParserRework #{this._id}");
    }

    private void EnsureRecursionDepthValid()
    {
        if (CurrentArgumentRecursionDepth > _options.MaxArgumentLimit)
            ThrowError(new MaxRecursionDepthExceededError("Maximum recursive argument depth exceeded"), null);

        if (CurrentExpressionRecursionDepth > _options.MaxNestingLimit)
            ThrowError(new MaxRecursionDepthExceededError("Maximum recursive expression depth exceeded"), null);
    }

    public List<List<AstRework>> Parse()
    {
        switch (this.CallPoint)
        {
            case InternalCallPoint.ArgumentParsing:
                CurrentArgumentRecursionDepth++;
                break;
            case InternalCallPoint.BlockParsing:
                CurrentExpressionRecursionDepth++;
                break;
            case null:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        this.EnsureRecursionDepthValid();

        this.EvaluateTokens();

        this.HandleNewExpressionStart();

        switch (this.CallPoint)
        {
            case InternalCallPoint.ArgumentParsing:
                CurrentArgumentRecursionDepth--;
                break;
            case InternalCallPoint.BlockParsing:
                CurrentExpressionRecursionDepth--;
                break;
            case null:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        // TODO: Make parser handle any token that comes after a block (such as a dot, which {...}.something i dont know
        //  if should be valid. And make the parser detect the start of a block, and parse it into the block state.
        return this.Expressions;
    }

    private void EvaluateTokens()
    {
        bool reachedInvalidState = false;

        while (this.CurrentIndex < this.Tokens.Count && !reachedInvalidState)
        {
            this.Log($"Current index: {this.CurrentIndex}");
            TokenListItem tokenListItem = this.GetNextToken();

            this.EvaluateToken(tokenListItem, out bool shouldContinue, out bool isInvalid);

            if (isInvalid) reachedInvalidState = true;
            if (!shouldContinue) break;
        }
    }

    private void EvaluateToken(TokenListItem tokenListItem, out bool shouldContinue, out bool isInvalid)
    {
        shouldContinue = true;
        isInvalid = false;

        Token token = tokenListItem.Token;

        bool isDotToken = token is DotToken;

        this.Log($"TokenType: {token.Type} - {token.Value}");

        evaluateTokenRestartPoint:

        if (token is EofToken)
        {
            shouldContinue = false;
            return;
        }

        if (token is EoLToken)
            return;

        if (token is BracketToken { IsCurly: true, IsOpen: true, })
        {
            this.Log("Found block start token");
            this.ReachedBlockStart();
            return;
        }

        switch (this.State)
        {
            case ParserState.Empty:
                this.HandleEmptyState(tokenListItem);
                break;
            case ParserState.Literal when isDotToken:
                this.HandleLiteralState(token);
                break;
            case ParserState.PartialAttributeAccess:
                this.HandlePartialAttributeAccessState(tokenListItem);
                break;
            case ParserState.AttributeAccess:
                this.HandleAttributeAccessState(token);
                break;
            case ParserState.MethodCall when isDotToken:
                this.HandleMethodCallState(token);
                break;
            case ParserState.Invalid:
                this.HandleInvalidState(token);
                isInvalid = true;
                break;

            case ParserState.Block:
            default:
                if (isDotToken) this.HandleDotToken();

                this.HandleNewExpressionStart();
                goto evaluateTokenRestartPoint;
        }
    }

    private void HandleEmptyState(TokenListItem token)
    {
        this.Log("Empty state");
        if (!token.Token.CanBeLiteral)
            ThrowError(new InvalidSyntaxError($"Expected a literal value, found {token.Token.ValueAsString}"),
                token.StartLocation);

        this.Log("Setting action");
        this.Action = token;
        this.Log("Setting state to literal");
        this.State = ParserState.Literal;
    }

    private void HandleLiteralState(Token token)
    {
        this.Log("Literal state");
        if (token is not DotToken)
        {
            ThrowError(new InvalidSyntaxError("Was expecting a dot token after a literal value"), token.StartLocation);
            return;
        }

        this.HandleDotToken();
    }

    private void HandleNewExpressionStart()
    {
        this.HandleNewAstStart();
        this.Expressions.Add(this.CurrentExpression);
        this.CurrentExpression = [];
    }

    private void HandleNewAstStart()
    {
        this.Log("New ast start");
        this.GenerateAst();

        if (this.CurrentAst is null) return;

        this.Log("Adding ast to current expression");
        this.CurrentExpression.Add(this.CurrentAst);
        this.CurrentAst = null;
        this.State = ParserState.Empty;
        this.BlockValue = null;
        this.Arguments = null;
        this.Action = null;
    }

    private void GenerateAst()
    {
        this.Log("Generating ast");

        if (this.Action is null && this.Arguments is null && this.BlockValue is null) return;

        this.CurrentAst = new AstRework();

        if (this.Action is not null)
            this.CurrentAst.AddAction((TokenListItem)this.Action);

        if (this.Arguments is not null)
            this.CurrentAst.AddArgs([.. this.Arguments,]);

        if (this.BlockValue is not null)
            this.CurrentAst.AddBlockValue(this.BlockValue);
    }

    private void HandlePartialAttributeAccessState(TokenListItem token)
    {
        this.Log("Partial attribute access state");
        if (token.Token is not WordToken)
            ThrowError(new InvalidSyntaxError("Expected a word"), token.StartLocation);

        this.Log("Setting action");
        this.Action = token;
        this.Log("Setting state to attribute access");
        this.State = ParserState.AttributeAccess;
    }

    private void HandleAttributeAccessState(Token token)
    {
        this.Log("Attribute access state");
        if (token is not BracketToken bracketToken)
        {
            ThrowError(new InvalidSyntaxError($"Expected a bracket, found {token.ValueAsString}"), token.StartLocation);
            return;
        }

        if (bracketToken is not { IsOpen: true, IsNormal: true, })
        {
            ThrowError(new InvalidSyntaxError("Expected an open bracket"), token.StartLocation);
        }

        this.Log("Setting arguments");
        this.Arguments = this.ParseArguments();
        this.Log("Setting state to method call");
        this.State = ParserState.MethodCall;
    }

    private List<ArgumentRework> ParseArguments()
    {
        this.Log("Parsing arguments");
        List<ArgumentRework> arguments = [];

        int bracketDepth = 1;
        List<TokenListItem> name = [];
        List<TokenListItem> value = [];
        bool isName = true;

        SourceLocation currentLocation = null!;

        while (bracketDepth > 0)
        {
            TokenListItem nextTokenListItem = this.GetNextToken();
            currentLocation = nextTokenListItem.StartLocation;

            if (nextTokenListItem.Token is EofToken)
                ThrowError(new EofError("Unexpected end of file"), nextTokenListItem.StartLocation);

            if (nextTokenListItem.Token is BracketToken { IsOpen: true, IsNormal: true, })
            {
                this.Log($"Increasing bracket depth from {bracketDepth} to {bracketDepth + 1}");
                bracketDepth++;
            }

            if (nextTokenListItem.Token is BracketToken { IsClosed: true, IsNormal: true, })
            {
                this.Log($"Decreasing bracket depth from {bracketDepth} to {bracketDepth - 1}");
                bracketDepth--;
            }

            if (bracketDepth == 0) continue;

            if (bracketDepth <= 1 && nextTokenListItem.Token is SemiColonToken)
            {
                this.Log("Found semi colon");
                arguments.Add(this.ConvertToArgument(name, value, isName, nextTokenListItem.StartLocation));
                name.Clear();
                value.Clear();
                isName = true;
                continue;
            }

            if (nextTokenListItem.Token is EqualsToken)
            {
                this.Log("Found equals");
                isName = false;
                continue;
            }

            if (isName)
            {
                this.Log($"Adding `{nextTokenListItem.Token.AsString()}` to name");
                name.Add(nextTokenListItem);
                continue;
            }

            this.Log("Adding to value");
            value.Add(nextTokenListItem);
        }

        arguments.Add(this.ConvertToArgument(name, value, isName, currentLocation));

        return arguments;
    }

    private ArgumentRework ConvertToArgument(IEnumerable<TokenListItem> name, IEnumerable<TokenListItem> value,
                                             bool isName, SourceLocation semiColonLocation)
    {
        this.Log("Converting to argument");

        TokenListItem[] nameArray = [.. name,];
        TokenListItem[] valueArray = [.. value,];

        bool valueIsEmpty = valueArray.Length == 0;

        int nameCount = nameArray.Length;
        bool nameIsExpression = nameCount > 1;
        bool nameIsEmpty = nameCount == 0;

        bool valid = false;

        if (nameIsEmpty && valueIsEmpty && isName)
            ThrowError(new InvalidSyntaxError("An argument cannot be empty"), semiColonLocation);

        if (valueIsEmpty && !nameIsEmpty && isName)
            valid = true;

        if (nameCount == 1 && nameArray[0].Token is WordToken && !valueIsEmpty && !isName)
            valid = true;

        if (!nameIsEmpty && valueIsEmpty && !isName)
            ThrowError(new InvalidSyntaxError("Expected a value"), semiColonLocation);

        if (!valid)
        {
            ThrowError(new InvalidSyntaxError("Expected an argument name or value"), semiColonLocation);
            throw new UnreachableException();
        }

        // if (valueIsEmpty && !isName)
        //     ThrowError(new InvalidSyntaxError("Expected a value"));
        //
        // if (!valueIsEmpty && nameIsExpression)
        //     ThrowError(new InvalidSyntaxError("Expected an argument name but found an expression"));
        //
        // if (nameIsEmpty && valueIsEmpty)
        //     ThrowError(new InvalidSyntaxError("Expected an argument name"));
        //
        // if (nameArray[0].Token is not WordToken nameAsWord)
        // {
        //     ThrowError(new InvalidSyntaxError("Argument name must be an identifier"));
        //     throw new UnreachableException();
        // }

        if (valueIsEmpty && !nameIsEmpty)
        {
            this.Log("Value is empty but name is not - setting value to name (no equals sign)");
            valueArray = nameArray;
            nameIsEmpty = true;
        }

        if (valueArray.Length == 0)
            ThrowError(new InvalidSyntaxError("Expected a value"), semiColonLocation);

        ParserRework parserRework = new(valueArray, InternalCallPoint.ArgumentParsing);
        this.Log($"Parsing value - count: {valueArray.Length}");
        List<List<AstRework>> valueAst = parserRework.Parse();

        if (valueAst.Count > 1)
            ThrowError(new InvalidSyntaxError("Expected argument value to be a single expression"),
                valueAst[1][0].GetSourceLocation());

        if (nameIsEmpty)
        {
            this.Log("Not a named argument, creating argument with just value");
            return new ArgumentRework(valueAst[0]);
        }

        this.Log("Creating argument with name and value");
        return new ArgumentRework((WordToken)nameArray[0].Token, valueAst[0]);
    }

    private void HandleMethodCallState(Token token)
    {
        this.Log("Method call state");
        if (token is DotToken)
            this.HandleDotToken();
    }

    private void ReachedBlockStart()
    {
        this.Log("Reached block start");

        if (this.State != ParserState.Empty)
        {
            this.HandleNewAstStart();
        }

        List<TokenListItem> block = [];

        int bracketDepth = 1;
        while (bracketDepth > 0)
        {
            TokenListItem nextTokenListItem = this.GetNextToken();
            Token token = nextTokenListItem.Token;

            if (token is BracketToken { IsCurly: true, IsClosed: true, })
            {
                this.Log($"Decreasing bracket depth from {bracketDepth} to {bracketDepth - 1}");
                bracketDepth--;
                continue;
            }

            if (token is BracketToken { IsCurly: true, IsOpen: true, })
            {
                this.Log($"Increasing bracket depth from {bracketDepth} to {bracketDepth + 1}");
                bracketDepth++;
            }

            block.Add(nextTokenListItem);
        }

        ParserRework parser = new(block, InternalCallPoint.BlockParsing);
        List<List<AstRework>> blockAst = parser.Parse();
        this.BlockValue = blockAst;
        this.State = ParserState.Block;
        this.HandleNewAstStart();
    }

    private void HandleDotToken()
    {
        this.Log("Dot token");
        this.HandleNewAstStart();
        this.State = ParserState.PartialAttributeAccess;
    }

    private void HandleInvalidState(Token token)
    {
        this.Log("Invalid state");
        ThrowError(new InvalidSyntaxError($"Unexpected token `{token.Value}`"), token.StartLocation);
    }

    private TokenListItem GetNextToken()
    {
        return this.GetTokenAtIndex(this.CurrentIndex++);
    }

    private TokenListItem GetTokenAtIndex(int index)
    {
        if (index >= 0 && index < this.Tokens.Count) return this.Tokens.ElementAtOrDefault(index);

        SourceLocation location = new()
        {
            LineNumber = 0,
            ColumnNumber = 0,
            Offset = 0,
            FilePath = "",
        };
        return new TokenListItem(new EofToken
        {
            StartLocation = location,
            EndLocation = location,
        });
    }

    [DoesNotReturn]
    private static void ThrowError(ErrorTypes error, SourceLocation location)
    {
        Errors.AlwaysThrow(error, location);
        throw new UnreachableException();
    }

    private void Log(string message) => this._logger.Info(message);

    private enum ParserMode
    {
        Expression,
        Block,
    }

    private enum ParserState
    {
        Literal,
        PartialAttributeAccess,
        AttributeAccess,
        MethodCall,
        Block,
        Empty,
        Invalid,
    }

    private enum InternalCallPoint
    {
        ArgumentParsing,
        BlockParsing,
    }

    public override string ToString() => $"ParserRework(#{this._id})";
}

public struct ParserOptions()
{
    public int MaxNestingLimit { get; } = 1000;
    public int MaxArgumentLimit { get; } = 1000;
}
