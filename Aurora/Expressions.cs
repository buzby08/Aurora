using System.Diagnostics;
namespace Aurora;

internal abstract class Expression
{
    public abstract Token? Evaluate();
}

internal class LiteralExpression(List<Token> value) : Expression
{
    public List<Token> Value { get; } = value;

    public override Token Evaluate()
    {
        return EvaluateTokens(Value);
    }

    private static Token EvaluateTokens(List<Token> tokens)
    {
        switch (tokens.Count)
        {
            case 1:
                return tokens[0];
            case 0:
                return new WordToken().Initialise("Null");
            default:
                (Token result, _) = EvaluateExpression(tokens, firstIteration: true);
                return result;
        }
        }
    
    private static (Token, int) EvaluateExpression(List<Token> tokens, bool firstIteration=false)
    {
        GlobalVariables.ExpressionDepth++;
        if (GlobalVariables.ExpressionDepth > UserConfiguration.MaxExpressionDepth)
        {
            Errors.RaiseError(new MaxExpressionDepthExceededError());
            throw new UnreachableException();
        }
            
        GlobalVariables.LOGGER.Verbose($"Literal expression: {PrettyPrint.TokenList(tokens, output: false)}");
        
        int itemsChecked = 0;
        List<Token> finalExpression = [];
        bool expectsCloseBracket = !firstIteration;
        bool foundCloseBracket = false;

        while (itemsChecked < tokens.Count)
        {
            Token item = tokens[itemsChecked];

            if (item is BracketToken { IsNormal: true, IsOpen: true })
            {
                (Token nextToken, int indexToAdd) = EvaluateExpression(tokens[(itemsChecked + 1)..]);
                finalExpression.Add(nextToken);
                itemsChecked += indexToAdd+1;
                continue;
            }
            
            itemsChecked++;

            if (item is BracketToken { IsNormal: true, IsOpen: false } && expectsCloseBracket)
            {
                foundCloseBracket = true;
                break;
            }

            if (item is BracketToken { IsNormal: true, IsOpen: false })
            {
                foundCloseBracket = true;
                continue;
            }
            
            finalExpression.Add(item);

        }

        switch (expectsCloseBracket, foundCloseBracket)
        {
            case (true, false):
                Errors.RaiseError(new UnclosedDelimiterError("Missing a closing bracket in expression"));
                break;
            case (false, true):
                Errors.RaiseError(new UnexpectedTokenError("Found unmatched closing bracket in expression"));
                break;
            case (_, _):
                break;
        }

        Token? total = null;
        char operation = '+';

        foreach (Token item in finalExpression) 
        {
            if (item is OperatorToken)
            {
                object? newOperation = item.Value;
                if (newOperation is char newCharOperation) operation = newCharOperation;
                continue;
            }

            total = Combine(total, operation, item);
        }

        if (total is not null)
            return (total, itemsChecked);

        Errors.RaiseError(new SystemError("System expected a token, but got null, while evaluating an expression"));
        throw new UnreachableException();
    }

    private static Token Combine(Token? left, char operation, Token right)
    {
        switch (left, operation)
        {
            case (null, '+'):
                return right;
            case (null, _):
                Errors.RaiseError(new InvalidSyntaxError("Operators cannot be the first item in an expression"));
                throw new UnreachableException();
            case (_, _):
                break;
        }

        bool validTypes = (left.Type, right.Type) switch
        {
            (StringToken.TokenType, _) => true,
            (IntegerToken.TokenType, FloatToken.TokenType) => true,
            (FloatToken.TokenType, IntegerToken.TokenType) => true,
            _ => false
        } || left.Type == right.Type;

        if (!validTypes)
            Errors.AlwaysThrow(new TypeMismatchError($"Cannot evaluate {left.Type} with {right.Type}"));

        switch (left.Type, right.Type, operation)
        {
            // String operations
            case (StringToken.TokenType, _, '+'):
                return new StringToken().Initialise((string?)left.Value + right.Value, withoutQuotes:true);
            case (StringToken.TokenType, IntegerToken.TokenType, '*'):
                return new StringToken().Initialise(
                    string.Concat(Enumerable.Repeat((string?)left.Value, (int)right.Value)), withoutQuotes:true);

            // Integer operations
            case (IntegerToken.TokenType, IntegerToken.TokenType, '+'):
                return new IntegerToken().Initialise(left.ValueAsInt + right.ValueAsInt);
            case (IntegerToken.TokenType, IntegerToken.TokenType, '-'):
                return new IntegerToken().Initialise(left.ValueAsInt - right.ValueAsInt);
            case (IntegerToken.TokenType, IntegerToken.TokenType, '*'):
                return new IntegerToken().Initialise(left.ValueAsInt * right.ValueAsInt);
            case (IntegerToken.TokenType, IntegerToken.TokenType, '/'):
                try
                {
                    return new FloatToken().Initialise(left.ValueAsFloat / right.ValueAsFloat);
                }
                catch (DivideByZeroException)
                {
                    Errors.RaiseError(new DivisionByZeroError());
                    throw new UnreachableException();
                }

            // Float operations (using Convert.ToDouble for precision)
            case (FloatToken.TokenType, FloatToken.TokenType, '+'):
            case (FloatToken.TokenType, IntegerToken.TokenType, '+'):
            case (IntegerToken.TokenType, FloatToken.TokenType, '+'):
                return new FloatToken().Initialise((left.ValueAsFloat) + (right.ValueAsFloat));

            case (FloatToken.TokenType, FloatToken.TokenType, '-'):
            case (FloatToken.TokenType, IntegerToken.TokenType, '-'):
            case (IntegerToken.TokenType, FloatToken.TokenType, '-'):
                return new FloatToken().Initialise(left.ValueAsFloat - right.ValueAsFloat);

            case (FloatToken.TokenType, FloatToken.TokenType, '*'):
            case (FloatToken.TokenType, IntegerToken.TokenType, '*'):
            case (IntegerToken.TokenType, FloatToken.TokenType, '*'):
                return new FloatToken().Initialise(left.ValueAsFloat * right.ValueAsFloat);

            case (FloatToken.TokenType, FloatToken.TokenType, '/'):
            case (FloatToken.TokenType, IntegerToken.TokenType, '/'):
            case (IntegerToken.TokenType, FloatToken.TokenType, '/'):
                try
                {
                    return new FloatToken().Initialise(
                        left.ValueAsFloat / right.ValueAsFloat);
                }
                catch (DivideByZeroException)
                {
                    Errors.RaiseError(new DivisionByZeroError());
                    throw new UnreachableException();
                }

            // Default case: unsupported operation
            default:
                Errors.AlwaysThrow(
                    new TypeMismatchError($"Unsupported operation: {left.Type} {operation} {right.Type}"));
                throw new UnreachableException();
        }

    }
}

// internal class MethodExpression : Expression
// {
//     public List<Token> Starter { get; }
//     public List<Token> Arguments { get; }
//
//     public MethodExpression(List<Token> starter, List<Token> arguments)
//     {
//         Starter = starter;
//         Arguments = arguments;
//     }
//
//     public override Token Evaluate()
//     {
//         return EvaluateExpression(this.Starter, this.Arguments);
//     }
//
//     public static Token EvaluateExpression(List<Token> starter, List<Token> arguments)
//     {
//         if (starter.Count != 3)
//         {
//             Errors.RaiseError(new InvalidSyntaxError("Method call was invalid. Must follow format 'Class.method'"),
//                 alwaysThrow: true);
//             throw new UnreachableException();
//         }
//
//         if (!Aurora.Evaluate.IsStartSequence(starter))
//         {
//             Errors.RaiseError(
//                 new SystemError("The system tried to evaluate a method call that was not a method call"));
//             throw new UnreachableException();
//         }
//
//         List<Token> positionalArguments = [];
//         Dictionary<string, Token> keywordArguments = new();
//
//         SeparatorToken semicolon = new SeparatorToken().Initialise(';');
//         List<Token> currentArg = [];
//
//         foreach (Token token in arguments)
//         {
//             if (token.Equals(semicolon))
//             {
//                 (positionalArguments, keywordArguments) =
//                     AddArgument(currentArg, positionalArguments, keywordArguments);
//                 continue;
//             }
//             
//             currentArg.Add(token);
//         }
//
//         return Variables.CallMethod(starter[0].ValueAsString, starter[2].ValueAsString, positionalArguments,
//             keywordArguments);
//
//     }
//
//     private static (List<Token>, Dictionary<string, Token>) AddArgument(List<Token> tokens, List<Token> positionalArgs,
//         Dictionary<string, Token> keywordArgs)
//     {
//         (string? key, Token? value) = Aurora.Evaluate.Argument(tokens);
//         switch (key is null, value is null)
//         {
//             case (true, false):
//                 positionalArgs.Add(value);
//                 break;
//             case (false, false):
//                 keywordArgs.Add(key, value);
//                 break;
//         }
//         
//         return (positionalArgs, keywordArgs);
//     }
// }

// internal static class Evaluate
// {
//     public static readonly BracketToken OPEN_BRACKET = new BracketToken().Initialise('(');
//     public static readonly BracketToken CLOSE_BRACKET = new BracketToken().Initialise(')');
//     public static readonly SeparatorToken SEMICOLON = new SeparatorToken().Initialise(';');
//     public static readonly SeparatorToken Dot = new SeparatorToken().Initialise('.');
//
//     private static List<(List<Type>, int)> _methodStartSequence =
//     [
//         ([typeof(WordToken)], 1), ([typeof(SeparatorToken)], 1), ([typeof(WordToken)], 1)
//     ];
//
//     public static bool IsStartSequence(List<Token> tokens)
//     {
//         GlobalVariables.LOGGER.Verbose(
//             $"(Checking if start sequence) {PrettyPrint.TokenList(tokens, output: false)}");
//         if (tokens.Count < 3) return false;
//         if (!Match.MatchTokens(tokens[..3], _methodStartSequence)) return false;
//         if (tokens[1].Equals(new SeparatorToken().Initialise('.')))
//         {
//             GlobalVariables.LOGGER.Verbose("Is start sequence");
//             return true;
//         }
//
//         return false;
//     }
//
//     public static (Token result, int itemsChecked) MethodExpressionHelper(WordToken className, WordToken methodName,
//         List<Token> arguments)
//     {
//         List<Token> positionals = [];
//         Dictionary<string, Token> keywordArguments = new();
//
//         int index = 0;
//
//         while (index < arguments.Count)
//         {
//             
//         }
//     }
//     
//     public static (List<Token> tokenList, int itemsChecked) SingleLineHelper(List<Token> tokens, bool endOnBracket=true)
//     {
//         List<Token> expression = [];
//
//         GlobalVariables.LOGGER.Verbose($"Evaluating tokens {PrettyPrint.TokenList(tokens, output: false)}");
//
//         bool item1IsWord = false;
//         bool item2IsDot = false;
//         bool item3IsWord = false;
//         bool item4IsOpenBracket = false;
//         
//         int index = 0;
//         while (index < tokens.Count)
//         {
//             Token currentToken = tokens[index];
//             index++;
//
//             if (currentToken.Type == WordToken.TokenType) item1IsWord = true;
//             if (currentToken.Equals(Dot) && item1IsWord) item2IsDot = true;
//             if (currentToken.Type == WordToken.TokenType && item2IsDot) item3IsWord = true;
//             if (currentToken.Equals(OPEN_BRACKET) && item3IsWord) item4IsOpenBracket = true;
//
//             if (item1IsWord && item2IsDot && item3IsWord && item4IsOpenBracket)
//             {
//                 (item1IsWord, item2IsDot, item3IsWord, item4IsOpenBracket) = (false, false, false, false);
//                 var result = SingleLineHelper(tokens[(index + 1)..]);
//                 index += result.itemsChecked;
//                 
//                 continue;
//             }
//
//             if (currentToken.Equals(CLOSE_BRACKET))
//             {
//                 
//             }
//                 
//             expression.Add(currentToken);
//         }
//         
//         GlobalVariables.LOGGER.Verbose("Is literal expression");
//         LiteralExpression literalExpression2 = new(expression);
//         Token result2 = literalExpression2.Evaluate();
//         GlobalVariables.LOGGER.Verbose($"Literal expression returned {result2.ValueAsString}");
//         return ([result2], index+1);
//     }
//     
//     public static Token SingleLine(List<Token> line)
//     {
//         GlobalVariables.ExpressionDepth++;
//         if (GlobalVariables.ExpressionDepth > UserConfiguration.MaxExpressionDepth)
//         {
//             Errors.RaiseError(new MaxExpressionDepthExceededError(), alwaysThrow: true);
//             throw new UnreachableException();
//         }
//
//         var result = SingleLineHelper(line);
//         
//         return result.tokenList.ElementAtOrDefault(0)?? new NullToken();
//     }
//
//     public static (string? key, Token? value) Argument(List<Token> arg)
//     {
//         EqualsToken equalsToken = new();
//         string? key = null;
//         Token? value = null;
//
//         if (arg.Count == 1 && arg[0].Equals(equalsToken))
//         {
//             Errors.RaiseError(new UnexpectedTokenError("Expected an argument, or a parameter name, not 'equals'"),
//                 alwaysThrow: true);
//             throw new UnreachableException();
//         }
//
//         if (arg.Count > 2 && arg[0] is WordToken wordKey && arg[1].Equals(equalsToken))
//         {
//             key = wordKey.ValueAsString;
//             value = arg[2];
//         }
//
//         value ??= arg.ElementAtOrDefault(0);
//         
//         return (key, value);
//     }
//
//     public static void AllCode(string[] code)
//     {
//         Interpreter interpreter = new();
//         GlobalVariables.LineNumber = 0;
//         Variables.SYSTEM_DEFINED.Add("__EXEC_START__", new IntegerToken().Initialise(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString()));
//         
//         foreach (var line in code)
//         {
//             GlobalVariables.ExpressionDepth = 0;
//             GlobalVariables.LineNumber++;
//             Variables.SYSTEM_DEFINED["__LINE_NUMBER__"] =
//                 new IntegerToken().Initialise(GlobalVariables.LineNumber.ToString());
//             interpreter.Text = line;
//             GlobalVariables.LOGGER.Verbose($"(Evaluating line {GlobalVariables.LineNumber}) {line}");
//             List<Token> tokens = interpreter.GetAllTokens();
//             
//             SingleLine(tokens);
//         }
//
//         GlobalVariables.LineNumber = 0;
//     }
// }