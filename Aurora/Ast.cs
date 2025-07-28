using System.Collections.Immutable;

namespace Aurora;

internal class Ast
{
    public enum AstItemTypes
    {
        None,
        Literal,
        ClassReference,
        AttributeAccess,
        MethodCall,
        ConditionAndBlock,
        Invalid
    }

    public ImmutableArray<string> ValidConditionalMethods = ["if", "while", "else"];

    public AstItemTypes ItemType { get; private set; }

    private Token? _itemValue;

    public Token? ItemValue
    {
        get => _itemValue;
        set
        {
            _itemValue = value;
            this.UpdateItemType();
        }
    }

    private string? _className;

    public string? ClassName
    {
        get => _className;
        set
        {
            _className = value;
            if (value is not null)
                this._customClass = Classes.GetClass(value);
            this.UpdateItemType();
        }
    }

    private bool? _classWaitingForAccess;

    public bool? ClassWaitingForAccess
    {
        get => _classWaitingForAccess;
        set
        {
            _classWaitingForAccess = value;
            this.UpdateItemType();
        }
    }

    private string? _classAttributeName;

    public string? ClassAttributeName
    {
        get => _classAttributeName;
        set
        {
            _classAttributeName = value;
            this.UpdateItemType();
        }
    }

    private CustomClass? _customClass;
    private string? _classMethodName;

    public string? ClassMethodName
    {
        get => _classMethodName;
        set
        {
            _classMethodName = value;
            this.UpdateItemType();
        }
    }

    private List<Ast>? _classArguments;

    public List<Ast>? ClassArguments
    {
        get => _classArguments;
        set
        {
            _classArguments = value;
            this.UpdateItemType();
        }
    }

    private bool? _closeBracketFound;

    public bool? CloseBracketFound
    {
        get => _closeBracketFound;
        set
        {
            _closeBracketFound = value;
            this.UpdateItemType();
        }
    }

    private string? _keywordValue;

    public string? KeywordValue
    {
        get => _keywordValue;
        set
        {
            _keywordValue = value;
            this.UpdateItemType();
        }
    }

    private OperatorToken? _operatorToken;

    public OperatorToken? OperatorToken
    {
        get => _operatorToken;
        set
        {
            _operatorToken = value;
            this.UpdateItemType();
        }
    }

    private ComparisonToken? _comparisonToken;

    public ComparisonToken? ComparisonToken
    {
        get => _comparisonToken;
        set
        {
            _comparisonToken = value;
            this.UpdateItemType();
        }
    }

    private List<Token>? _condition;

    public List<Token>? Condition
    {
        get => _condition;
        set
        {
            _condition = value;
            this.UpdateItemType();
        }
    }

    private List<string>? _codeBlock;

    public List<string>? CodeBlock
    {
        get => _codeBlock;
        set
        {
            _codeBlock = value;
            this.UpdateItemType();
        }
    }

    public int GetNullStates(params object?[] vars)
    {
        int mask = 0;
        for (int i = 0; i < vars.Length; i++)
        {
            if (vars[i] is not null)
                mask |= 1 << i;
        }

        return mask;
    }

    public void UpdateItemType()
    {
        int mask = GetNullStates(this._itemValue, this._className, this._classWaitingForAccess,
            this._classAttributeName, this._classMethodName, this._classArguments, this._closeBracketFound,
            this._condition, this._codeBlock);

        string paddedBinaryMask = Convert.ToString(mask, 2).PadLeft(7, '0');
        GlobalVariables.LOGGER.Verbose($"Ast mask updated. New mask is {paddedBinaryMask}");

        const int allNull = 0b000000000;
        const int literal = 0b000000001;
        const int onlyClassName = 0b000000101;
        const int classAttributeOne = 0b000000110;
        const int classAttributeTwo = 0b000001010;
        const int classMethod = 0b001110010;
        const int conditionAndBlock = 0b110000001;

        this.ItemType = mask switch
        {
            allNull => AstItemTypes.None,
            literal => AstItemTypes.Literal,
            onlyClassName => AstItemTypes.ClassReference,
            classAttributeOne or classAttributeTwo => AstItemTypes.AttributeAccess,
            classMethod => AstItemTypes.MethodCall,
            conditionAndBlock => AstItemTypes.ConditionAndBlock,
            _ => AstItemTypes.Invalid
        };

        GlobalVariables.LOGGER.Verbose($"itemType: {this.ItemType}");
    }

    private Token _callCustomMethod()
    {
        if (ItemType is not AstItemTypes.MethodCall)
        {
            Errors.AlwaysThrow(
                new SystemError(
                    "The system tried to parse a statement as a method call, which was not allowed in this context"));
        }

        CustomClass.CustomMethod currentMethod = this._customClass!.GetMethod(ClassMethodName!);
        List<Token> positionals = [];
        Dictionary<string, Token> keywords = [];
        List<Ast> raw = [];

        foreach (Ast argumentAst in this.ClassArguments!)
        {
            raw.Add(argumentAst);
            List<Token> evaluatedArg = argumentAst.EvaluateAsArgument();
            switch (evaluatedArg.Count)
            {
                case 1:
                    positionals.Add(evaluatedArg[0]);
                    continue;

                case > 2 or <= 0:
                    Errors.AlwaysThrow(
                        new SystemError("The system encountered too many elements when parsing the argument AST"));
                    break;

                case 2:
                    string keyword = evaluatedArg[0].ValueAsString;

                    keywords[keyword] = evaluatedArg[1];
                    continue;
            }
        }

        return currentMethod(positionals, keywords, raw);
    }

    private Token _evaluateLiteral()
    {
        if (this.ItemValue is null)
            Errors.AlwaysThrow(new SystemError("The system tried to parse an invalid expression"));

        if (Variables.IsVariable(this.ItemValue.ValueAsString))
            return Variables.GetVariable(this.ItemValue.ValueAsString);

        if (Classes.ClassExists(this.ItemValue.ValueAsString))
        {
            string className = Classes.GetClass(this.ItemValue.ValueAsString).Name;
            string message = $"CLASS<{className}>";
            return new StringToken().Initialise(message, withoutQuotes: true, interpolate: false);
        }

        return this.ItemValue;
    }

    private Token? _evaluateIf()
    {
        Token? evaluatedCondition = this.Condition!.Count != 0
            ? Aurora.Evaluate.SingleLine(this._condition!)
            : new BooleanToken().Initialise(true);

        if (evaluatedCondition is null || evaluatedCondition.Type != BooleanToken.TokenType)
            Errors.AlwaysThrow(new InvalidSyntaxError("Conditional expressions must evaluate to a boolean!"));

        if (this.ItemValue!.ValueAsString == "else" && GlobalVariables.PreviousIfIsTrue is null)
        {
            Errors.RaiseError(new InvalidSyntaxError("else statement not tied to an if statement is not allowed"));
            return null;
        }

        if (this.ItemValue!.ValueAsString == "else" && GlobalVariables.PreviousIfIsTrue != false)
        {
            GlobalVariables.PreviousIfIsTrue = this._condition!.Count == 0
                ? null
                : GlobalVariables.PreviousIfIsTrue;
            return null;
        }

        if (!evaluatedCondition.ValueAsBool)
        {
            GlobalVariables.PreviousIfIsTrue = false;
            return null;
        }

        Aurora.Evaluate.AllCode(this._codeBlock!.ToArray());

        if (this._condition!.Count == 0)
        {
            GlobalVariables.PreviousIfIsTrue = null;
            return null;
        }

        GlobalVariables.PreviousIfIsTrue = true;
        return null;
    }

    private Token? _evaluateWhile()
    {
        Token? evaluatedCondition = Aurora.Evaluate.SingleLine(this._condition!);

        if (evaluatedCondition is null || evaluatedCondition.Type != BooleanToken.TokenType)
            Errors.AlwaysThrow(new InvalidSyntaxError("Conditional expressions must evaluate to a boolean!"));

        bool conditionIsTrue = evaluatedCondition.ValueAsBool;
        int lineStartNumber = (int)GlobalVariables.LineNumber!;
        while (conditionIsTrue)
        {
            GlobalVariables.LineNumber = lineStartNumber;
            Aurora.Evaluate.AllCode(this._codeBlock!.ToArray());

            Token? reEvaluatedCondition = Aurora.Evaluate.SingleLine(this._condition!);

            if (reEvaluatedCondition is null || reEvaluatedCondition.Type != BooleanToken.TokenType)
                Errors.AlwaysThrow(new InvalidSyntaxError("Conditional expressions must evaluate to a boolean!"));

            conditionIsTrue = reEvaluatedCondition.ValueAsBool;
        }

        return null;
    }

    private Token? _evaluateConditionAndBlock()
    {
        if (!this.ValidConditionalMethods.Contains(this._itemValue!.ValueAsString))
            Errors.AlwaysThrow(
                new InvalidSyntaxError($"{this._itemValue!.ValueAsString} is not a valid conditional expression"));

        return this._itemValue!.ValueAsString switch
        {
            "while" => this._evaluateWhile(),
            "if" => this._evaluateIf(),
            "else" => this._evaluateIf(),
            _ => null
        };
    }


    public Token? Evaluate()
    {
        return this.ItemType switch
        {
            AstItemTypes.Literal => this._evaluateLiteral(),
            AstItemTypes.ClassReference => new WordToken().Initialise(ItemValue!.ValueAsString),
            AstItemTypes.AttributeAccess => this._customClass!.GetAttribute(ClassAttributeName!)(),
            AstItemTypes.MethodCall => this._callCustomMethod(),
            AstItemTypes.ConditionAndBlock => this._evaluateConditionAndBlock(),
            _ => Errors.AlwaysThrow<Token>(new SystemError("The system tried to parse an invalid expression"))
        };
    }

    public void Combine(Ast other)
    {
        if (this.ItemType is AstItemTypes.None)
        {
            this.ItemValue = other.Evaluate();
            return;
        }

        if (this.OperatorToken is null && this.ComparisonToken is null)
            Errors.AlwaysThrow(new SystemError("The system tried to parse an invalid expression"));

        Token? currentEvaluated = this.Evaluate();
        Token? otherEvaluated = other.Evaluate();
        if (currentEvaluated is null || otherEvaluated is null)
            Errors.AlwaysThrow(new SystemError("The system tried to parse an invalid expression"));
        if (this.ComparisonToken is not null)
            this.ItemValue =
                LiteralExpression.CombineComparison(currentEvaluated, this.ComparisonToken, otherEvaluated);

        if (this.OperatorToken is not null)
            this.ItemValue =
                LiteralExpression.CombineOperator(currentEvaluated, this.OperatorToken.ValueAsChar, otherEvaluated);

        this.ComparisonToken = other.ComparisonToken;
        this.OperatorToken = other.OperatorToken;
    }

    public List<Token> EvaluateAsArgument()
    {
        if (KeywordValue is null) return [this.Evaluate()];

        return [new WordToken().Initialise(KeywordValue), this.Evaluate()];
    }

    public void ResetValues()
    {
        this._customClass = null;
        this._className = null;
        this._classWaitingForAccess = null;
        this._classAttributeName = null;
        this._classMethodName = null;
        this._classArguments = null;
        this._closeBracketFound = null;
        this._itemValue = null;
        this._keywordValue = null;
    }
}