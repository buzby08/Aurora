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
        Expression,
        Invalid
    }

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
            this._classAttributeName, this._classMethodName, this._classArguments, this._closeBracketFound);

        string paddedBinaryMask = Convert.ToString(mask, 2).PadLeft(7, '0');
        GlobalVariables.LOGGER.Verbose($"Ast mask updated. New mask is {paddedBinaryMask}");

        const int allNull = 0b0000000;
        const int literal = 0b0000001;
        const int onlyClassName = 0b0000101;
        const int classAttributeOne = 0b0000110;
        const int classAttributeTwo = 0b0001010;
        const int classMethod = 0b1110010;

        this.ItemType = mask switch
        {
            allNull => AstItemTypes.None,
            literal => AstItemTypes.Literal,
            onlyClassName => AstItemTypes.ClassReference,
            classAttributeOne or classAttributeTwo => AstItemTypes.AttributeAccess,
            classMethod => AstItemTypes.MethodCall,
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

        foreach (Ast argumentAst in this.ClassArguments!)
        {
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
                    string? keyword = evaluatedArg[0].ValueAsString;

                    if (keyword is null)
                    {
                        Errors.AlwaysThrow(
                            new InvalidSyntaxError(
                                "Keyword arguments must be a keyword, not any other token, such as string, integer, etc"));
                        break;
                    }

                    keywords[keyword] = evaluatedArg[1];
                    continue;
            }
        }

        return currentMethod(positionals, keywords);
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

    public Token Evaluate()
    {
        return this.ItemType switch
        {
            AstItemTypes.Literal => this._evaluateLiteral(),
            AstItemTypes.ClassReference => new WordToken().Initialise(ItemValue!.ValueAsString),
            AstItemTypes.AttributeAccess => this._customClass!.GetAttribute(ClassAttributeName!)(),
            AstItemTypes.MethodCall => this._callCustomMethod(),
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

        if (this.OperatorToken is null)
            Errors.AlwaysThrow(new SystemError("The system tried to parse an invalid expression"));

        this.ItemValue = LiteralExpression.Combine(this.Evaluate(), this.OperatorToken.ValueAsChar, other.Evaluate());
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