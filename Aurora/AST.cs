namespace Aurora;


internal class AST
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

            if (value is not null)
                this._customClass = Classes.GetClass(value);
        }
    }
    
    private List<AST>? _classArguments;
    public List<AST>? ClassArguments
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
            
        foreach (List<Token> evaluatedArg in this.ClassArguments!.Select(argumentAst => argumentAst.EvaluateAsArgument()))
        {
            switch (evaluatedArg.Count)
            {
                case 1:
                    positionals.Add(evaluatedArg[0]);
                    break;
                        
                case >2 or  <=0:
                    Errors.AlwaysThrow(
                        new SystemError("The system encountered too many elements when parsing the argument AST"));
                    break;
                
                case 2:
                    string? keyword = evaluatedArg[0].ToString();
    
                    if (keyword is null)
                    {
                        Errors.AlwaysThrow(
                            new InvalidSyntaxError(
                                "Keyword arguments must be a keyword, not any other token, such as string, integer, etc"));
                        break;
                    }
    
                    keywords.Add(keyword, evaluatedArg[1]);
                    break;
            }
        }
            
        return currentMethod(positionals, keywords);
    }
    
    public Token Evaluate()
    {
        return this.ItemType switch
        {
            AstItemTypes.Literal         => this.ItemValue!,
            AstItemTypes.ClassReference  => new WordToken().Initialise(ItemValue!.ValueAsString),
            AstItemTypes.AttributeAccess => this._customClass!.GetAttribute(ClassAttributeName!)(),
            AstItemTypes.MethodCall      => this._callCustomMethod(),
            _ => Errors.AlwaysThrow<Token>(new SystemError("The system tried to parse an invalid expression"))
        };
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