using System.Diagnostics;
using Aurora.Core;
using Aurora.Evaluator.BuiltinObjects;
using Aurora.Evaluator.Internals;
using Type = Aurora.Evaluator.Internals.Type;

namespace Aurora.Evaluator;

public class Evaluator : IDisposable
{
    public static Stack<Evaluator> Evaluators { get; } = new();
    public static Evaluator Current => Evaluators.Peek();
    public readonly int Id;

    private Logger _logger;
    private Ast? CurrentAst { get; set; }
    private RuntimeObject? PreviousResult { get; set; }
    private RuntimeContext Context;
    private Evaluator? Parent { get; set; }
    private EvaluatorState State { get; set; }
    private bool BreakLoop { get; set; }
    private bool ContinueLoop { get; set; }
    private bool Kill { get; set; }


    public Evaluator(RuntimeContext context)
    {
        this.Context = context;
        this.Id = IdGenerator.GenerateId("Evaluator");
        this._logger = new Logger($"Evaluator #{this.Id}");
        this.State = EvaluatorState.NormalEval;
        Evaluators.Push(this);
    }

    public static Evaluator CreateChild(RuntimeContext context, EvaluatorState state = EvaluatorState.NormalEval)
    {
        Evaluator child = new(context)
        {
            Parent = Current,
            State = state,
        };
        return child;
    }

    public void Dispose()
    {
        if (Evaluators.Peek().Id == this.Id)
            Evaluators.Pop();

        this.Kill = true;
    }
    public RuntimeObject? EvaluateMultipleExpressions(IEnumerable<IEnumerable<Ast>> expressions)
    {
        Ast[][] expressionsArr = expressions.Select(x => x.ToArray()).ToArray();

        foreach (Ast[] expression in expressionsArr)
        {
            if (this.Kill) return null;

            RuntimeObject? returnValue = EvaluateExpression(expression);

            if (returnValue is not null)
                return returnValue;
        }

        return null;
    }

    public RuntimeObject? EvaluateExpression(Ast[] expression)
    {
        this.State = EvaluatorState.NormalEval;
        RuntimeObject? previousResult = null;

        foreach (Ast ast in expression) previousResult = EvaluateAst(ast, previousResult);

        this._logger.Debug($"Expression evaluated to {previousResult}");

        return null;
    }

    public RuntimeObject EvaluateExpressionForValue(Ast[] expression)
    {
        this.State = EvaluatorState.NormalEval;
        RuntimeObject? previousResult = null;

        foreach (Ast ast in expression) previousResult = EvaluateAst(ast, previousResult);

        if (previousResult is null)
            Errors.AlwaysThrow(new SystemError("Expression evaluated to null"), null);

        return previousResult;
    }

    private RuntimeObject EvaluateAst(Ast ast, RuntimeObject? previousResult = null)
    {
        if (ast.State == AstState.None)
            Errors.AlwaysThrow(new SystemError("Ast has no state"), null);

        if (ast.State == AstState.Literal && previousResult is null)
            return RuntimeObject.CreateFromToken(ast.GetAction()!, this.Context);

        if (ast.State == AstState.Literal && previousResult is not null)
            return this.EvaluateAttributeAccess(ast.GetAction()!, previousResult);

        if (ast.State == AstState.Method && previousResult is not null)
            return this.EvaluateMethodCall(ast, previousResult);

        if (ast.State == AstState.Block)
            return this.EvaluateBlock(ast.GetBlockValue()!);

        throw new NotImplementedException(
            $"Ast state `{ast.State}`, previous result: {(previousResult is null ? 'n' : 'y')} is not implemented yet");
    }

    private RuntimeObject EvaluateAttributeAccess(Token literal, RuntimeObject previousResult)
    {
        if (literal is not WordToken)
            Errors.AlwaysThrow(new InvalidSyntaxError("Attribute names must be a word"), literal.StartLocation);

        string attributeName = literal.ValueAsString;

        if (previousResult is Type type)
            return type.GetStaticAttribute(attributeName, this.Context, literal.StartLocation)
                .GetValue(previousResult, this.Context, literal.StartLocation);

        return previousResult.Type.GetInstanceAttribute(attributeName, this.Context, literal.StartLocation)
            .GetValue(previousResult, this.Context, literal.StartLocation);
    }

    private RuntimeObject EvaluateMethodCall(Ast ast, RuntimeObject previousResult)
    {
        Token methodName = ast.GetAction()!;
        string methodNameString = methodName.ValueAsString;
        Argument[] args = ast.GetArgs()!.ToArray();

        Method method = null!;

        if (previousResult is Type type)
            method = type.GetStaticMethod(methodNameString, this.Context, methodName.StartLocation);

        if (previousResult is not Type)
            method = previousResult.Type.GetInstanceMethod(methodNameString, this.Context, methodName.StartLocation);

        return method.Invoke(previousResult, args, this.Context, methodName.StartLocation);
    }

    private RuntimeObject EvaluateBlock(IEnumerable<IEnumerable<Ast>> block)
    {
        return new BlockObject(block);
    }

    public void EvaluateWhile(Ast[] condition, BlockObject body)
    {
        this.State = EvaluatorState.WhileLoop;

        loopStart:

        while (!this.BreakLoop && EvaluateCondition(condition))
        {
            using Evaluator evaluator = CreateChild(this.Context, EvaluatorState.Block);
            evaluator.EvaluateMultipleExpressions(body.Value);
        }

        this.BreakLoop = false;

        if (this.ContinueLoop)
        {
            this.ContinueLoop = false;
            goto loopStart;
        }
    }

    public static void ExecuteBreakLoop()
    {
        Evaluator evaluator = RewindBackToWhileLoop();
        evaluator.BreakLoop = true;
    }

    public static void ExecuteContinueLoop()
    {
        Evaluator evaluator = RewindBackToWhileLoop();
        evaluator.ContinueLoop = true;
    }

    private static Evaluator RewindBackToWhileLoop()
    {
        while (Evaluators.Peek().State != EvaluatorState.WhileLoop)
            Evaluators.Pop().Dispose();

        return Evaluators.Peek();
    }

    private bool EvaluateCondition(Ast[] condition)
    {
        using Evaluator evaluator = CreateChild(this.Context);
        RuntimeObject evaluatedObject = evaluator.EvaluateExpressionForValue(condition);

        if (evaluatedObject is BooleanObject booleanObject) return booleanObject.Value;

        Errors.AlwaysThrow(
            new UnsupportedOperationError($"Argument 1 to while must evaluate be a boolean"),
            this.Context.CallSiteLocation);
        throw new UnreachableException();
    }

    public override string ToString()
    {
        return $"{nameof(Evaluator)}(#{this.Id}, {this.State})";
    }

    public enum EvaluatorState
    {
        NormalEval,
        Block,
        Function,
        WhileLoop,
        ForLoop,
    }

    private enum EvaluatorMode
    {
        ConditionEval,
        ExpressionEval,
        AstEval,
        MultipleExpressionEval,
        WhileEval,
    }
}

// internal class Evaluator
// {
//     public Evaluator(string text)
//     {
//         this.Tokenizer.Text = text;
//     }
//
//     public static void EvaluateAllCode(string[] code, RuntimeContext context)
//     {
//         foreach (string s in code)
//         {
//             InternalVariables.LineNumber += 1;
//
//             if (InternalVariables.LinesToDebug.Contains<int>((int)InternalVariables.LineNumber!))
//                 Debugger.Break();
//
//             int commentStart = s.IndexOf("//", StringComparison.Ordinal);
//
//             string line = s;
//
//             if (commentStart != -1)
//                 line = s[0..commentStart];
//
//             if (string.IsNullOrWhiteSpace(line))
//                 continue;
//
//             Evaluator evaluator = new(line);
//             AstList astList = evaluator.ParseTokenList(context);
//             EvaluateAstList(astList, context);
//         }
//     }
//
//     private AstList ParseTokenList(RuntimeContext context)
//     {
//         TokenList tokens = this.Tokenizer.GetAllTokens();
//
//         return ParseTokenList(tokens, context);
//     }
//
//     public static AstList ParseTokenList(TokenList tokens, RuntimeContext context)
//     {
//         if (tokens.Count == 0) return [];
//
//         AstList asts = [];
//
//         Ast currentAst = new()
//         {
//             IsALiteral = true,
//         };
//         bool isTarget = true;
//
//         int count = 0;
//
//         while (count < tokens.Count)
//         {
//             // Todo: Set Ast as literal when dotToken has not been found as name is a token that can become a runtime
//             //  object.
//             TokenListItem tokenItem = tokens[count++];
//
//             if (isTarget && tokenItem.Token is DotToken)
//             {
//                 isTarget = false;
//                 currentAst.IsALiteral = false;
//                 continue;
//             }
//
//             if (!isTarget && tokenItem.Token is DotToken)
//             {
//                 asts.Add(currentAst);
//                 currentAst = new Ast
//                 {
//                     IsALiteral = false,
//                 };
//                 isTarget = false;
//                 continue;
//             }
//
//             if (tokenItem.Token is not WordToken && tokenItem.Token is not StringToken &&
//                 tokenItem.Token is not DotToken &&
//                 tokenItem.Token is not BracketToken
//                 && tokenItem.Token is not NumberToken)
//                 Errors.AlwaysThrow(new UnexpectedTokenError($"`{tokenItem.AsString}` was not expected"), context,
//                     position: tokenItem.StartCharPosition);
//
//             if (isTarget && tokenItem.Token is WordToken or StringToken or NumberToken)
//             {
//                 currentAst.Target = tokenItem;
//                 continue;
//             }
//
//             if (!isTarget && tokenItem.Token is WordToken)
//             {
//                 currentAst.Name = tokenItem;
//                 continue;
//             }
//
//             if (!isTarget && tokenItem.AsString == "(")
//             {
//                 // This should parse the args, then set the count to exactly where the argument parser finished
//                 //      The count should then be the index after the arguments closing bracket
//                 ArgumentParsingReturnResult argumentParsingReturnResult = Argument.Parse(tokens[count..], context);
//                 List<Argument> arguments = argumentParsingReturnResult.Arguments;
//                 currentAst.Arguments = arguments;
//
//                 count += argumentParsingReturnResult.NumberOfTokensChecked;
//             }
//         }
//
//         if (currentAst.IsALiteral && currentAst.Target is not null)
//         {
//             currentAst.Name = currentAst.Target.Value;
//             currentAst.Target = null;
//         }
//
//         asts.Add(currentAst);
//
//         return asts;
//     }
//
//     public static RuntimeObject EvaluateAstList(AstList asts, RuntimeContext context)
//     {
//         RuntimeObject? result = null;
//         foreach (Ast currentAst in asts)
//         {
//             result = currentAst.Evaluate(context, result);
//         }
//
//         if (result is null) /* Todo: This is being hit when current code in code.aur is being run. Figure out why, as passing 3 values should
//                                 be allowed*/
//             Errors.AlwaysThrow(new SystemError($"Could not parse ast list where ast list is empty."), context);
//
//         return result;
//     }
//
//     public static RuntimeObject ExecuteMethodAst(List<List<Ast>> body, RuntimeObject self,
//         RuntimeContext context)
//     {
//         // Todo: Implement
//         return new UnitObject();
//     }
//
//
//     private Tokenizer Tokenizer { get; } = new Tokenizer();
// }
