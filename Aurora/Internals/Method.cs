namespace Aurora.Internals;

internal class Method
{
    public string Name { get; }
    public Type DeclaringType { get; }
    public readonly List<ParameterDefinition>? Parameters;
    public Type? UnlimitedPositionalArgsType { get; }
    public Type? UnlimitedKeywordArgumentsType { get; }

    public Method(string name, Type returnType, List<ParameterDefinition>? parameters, MethodBody body)
    {
        this.Name = name;
        this.DeclaringType = returnType;
        this.Parameters = parameters;
        this._builtinBody = body;
    }

    public Method(string name, Type returnType, Type? unlimitedPositionalArgumentsType,
        Type? unlimitedKeywordArgumentsType, MethodBody body)
    {
        this.Name = name;
        this.DeclaringType = returnType;
        this.Parameters = [];
        this.UnlimitedPositionalArgsType = unlimitedPositionalArgumentsType;
        this.UnlimitedKeywordArgumentsType = unlimitedKeywordArgumentsType;
        this._builtinBody = body;
    }
    
    public Method(string name, Type returnType, Type? unlimitedPositionalArgumentsType,
        Type? unlimitedKeywordArgumentsType, List<ParameterDefinition>? parameters, MethodBody body)
    {
        this.Name = name;
        this.DeclaringType = returnType;
        this.Parameters = parameters;
        this.UnlimitedPositionalArgsType = unlimitedPositionalArgumentsType;
        this.UnlimitedKeywordArgumentsType = unlimitedKeywordArgumentsType;
        this._builtinBody = body;
    }

    public Method(string name, Type returnType, List<ParameterDefinition> parameters, List<List<Ast>> body)
    {
        this.Name = name;
        this.DeclaringType = returnType;
        this.Parameters = parameters;
        this._userDefinedBody = body;
    }

    public bool IsBuiltin => this._builtinBody is not null;

    public RuntimeObject Invoke(
        RuntimeObject self,
        List<Argument> args,
        RuntimeContext parentContext)
    {
        RuntimeContext methodContext = new(parentContext);

        Dictionary<string, List<Ast>> matchedArgs = MatchArgumentsToParameter(args);

        bool doNotValidate = this.IsBuiltin && this.Parameters is null;
        if (doNotValidate)
            return this._builtinBody!(self, matchedArgs, methodContext);

        Dictionary<string, RuntimeObject> validatedArgs = [];

        if (this.UnlimitedPositionalArgsType is not null)
            this.ValidateUnlimitedPositionalArguments(validatedArgs, matchedArgs, methodContext);

        if (this.UnlimitedKeywordArgumentsType is not null)
            this.ValidateUnlimitedKeywordArguments(validatedArgs, matchedArgs, methodContext);

        if (this.Parameters is not null)
            this.ValidateArguments(validatedArgs, matchedArgs, methodContext);

        foreach (var (key, value) in validatedArgs)
            methodContext.Create(key, value);

        RuntimeObject returnedObject = null!;

        if (this.IsBuiltin)
            returnedObject = this._builtinBody!(self, matchedArgs, methodContext);

        if (!this.IsBuiltin)
            returnedObject = Evaluator.ExecuteMethodAst(
                this._userDefinedBody!,
                self,
                methodContext);

        if (returnedObject.Type != this.DeclaringType)
            Errors.AlwaysThrow(new TypeMismatchError(
                $"Method is declared to return a value of type {this.DeclaringType.Name}, but a value of " +
                $"type {returnedObject.Type.Name} was returned",
                user: false));

        return returnedObject;
    }

    public void ValidateArguments(Dictionary<string, RuntimeObject> validatedArgs,
        Dictionary<string, List<Ast>> matchedArgs,
        RuntimeContext context)
    {
        if (this.Parameters is null)
            Errors.AlwaysThrow(new SystemError(
                "Cannot validate built-in method arguments when it has been specified not to validate " +
                "arguments"));

        foreach (ParameterDefinition parameter in this.Parameters)
        {
            validatedArgs[parameter.Name] = parameter.DefaultValue;
        }

        foreach (var (key, value) in matchedArgs)
        {
            RuntimeObject argObject = Evaluator.EvaluateAstList(value, context);
            ParameterDefinition? paramDefinition = this.Parameters.FirstOrDefault(x => x.Name == key);

            if (paramDefinition is null && this.UnlimitedKeywordArgumentsType is null && this.UnlimitedPositionalArgsType is null)
                Errors.AlwaysThrow(new SystemError("An unmatched parameter has been found"));

            if (paramDefinition is not null && !argObject.Type.IsSubclassOf(paramDefinition.Type))
                Errors.AlwaysThrow(
                    new TypeMismatchError(
                        $"Cannot assign {argObject.Type.Name} to parameter {paramDefinition.Name} of " +
                        $"type {paramDefinition.Type.Name}"));

            validatedArgs[key] = argObject;
        }

        foreach (var (key, value) in validatedArgs)
            if (value is null)
                Errors.AlwaysThrow(new ArgumentDeficitError($"Parameter `{key}` is required"));
    }

    private void ValidateUnlimitedKeywordArguments(Dictionary<string, RuntimeObject> validatedArgs,
        Dictionary<string, List<Ast>> matchedArgs,
        RuntimeContext context)
    {
        if (this.UnlimitedKeywordArgumentsType is null)
            Errors.AlwaysThrow(
                new SystemError("Variadic (Unlimited) keyword arguments cannot be null after entering the " +
                                "argument validator"));
        
        foreach ((string key, List<Ast> value) in matchedArgs)
        {
            RuntimeObject valueAsObject = Evaluator.EvaluateAstList(value, context);

            if (!valueAsObject.Type.IsSubclassOf(this.UnlimitedKeywordArgumentsType))
                Errors.AlwaysThrow(new ArgumentTypeMismatchError(
                    $"Cannot assign {valueAsObject.Type.Name} to {this.UnlimitedKeywordArgumentsType.Name}"));
            
            validatedArgs[key] = valueAsObject;
        }
    }

    private void ValidateUnlimitedPositionalArguments(Dictionary<string, RuntimeObject> validatedArgs,
        Dictionary<string, List<Ast>> matchedArgs,
        RuntimeContext context)
    {
        if (this.UnlimitedPositionalArgsType is null)
            Errors.AlwaysThrow(
                new SystemError("Variadic (Unlimited) positional arguments cannot be null after entering the " +
                                "argument validator"));
        
        foreach ((string key, List<Ast> value) in matchedArgs)
        {
            RuntimeObject valueAsObject = Evaluator.EvaluateAstList(value, context);

            if (!valueAsObject.Type.IsSubclassOf(this.UnlimitedPositionalArgsType))
                Errors.AlwaysThrow(new ArgumentTypeMismatchError(
                    $"Cannot assign {valueAsObject.Type.Name} to {this.UnlimitedPositionalArgsType.Name}"));

            validatedArgs[key] = valueAsObject;
        }
    }

    private Dictionary<string, List<Ast>> MatchArgumentsToParameter(List<Argument> arguments)
    {
        Dictionary<string, List<Ast>> matchedArgs = new();

        bool hasReachedKeywordArgument = false;
        int positionalIndex = 0;

        bool requiresNoValidation = this.Parameters is null
                                    && this.UnlimitedKeywordArgumentsType is null
                                    && this.UnlimitedPositionalArgsType is null;

        if (requiresNoValidation) return HandleNoValidationArgumentMatching(arguments);

        // Todo: Handle *args and **kwargs

        for (var i = 0; i < arguments.Count; i++)
        {
            var arg = arguments[i];
            bool isPositionalArgument = arg.Keyword is null;

            if (isPositionalArgument && hasReachedKeywordArgument)
                Errors.AlwaysThrow(new InvalidSyntaxError("Positional arguments cannot exist after keyword arguments"),
                    arg.Value.First().StartCharPosition);

            if (!isPositionalArgument)
            {
                hasReachedKeywordArgument = true;
                this.AddKeywordArgument(matchedArgs, arg);
                continue;
            }

            ParameterDefinition? param = this.Parameters!.ElementAtOrDefault(positionalIndex);
            if (param is null && this.UnlimitedPositionalArgsType is null)
            {
                Errors.RaiseError(new ArgumentSurplusError(
                    $"Method {this.Name} takes {this.Parameters!.Count} parameters, but {arguments.Count} were provided."));
                break;
            }

            if (param is null && this.UnlimitedPositionalArgsType is not null)
            {
                matchedArgs[$"__POSITIONAL_ARG_{i}"] = arg.ValueAsAsts();
                continue;
            }

            this.AddPositionalArg(matchedArgs, param?.Name, arg.ValueAsAsts(), i);
        }

        return matchedArgs;
    }

    private void AddPositionalArg(Dictionary<string, List<Ast>> matchedArgs, string? key, List<Ast> value, int index)
    {
        if (this.UnlimitedPositionalArgsType is not null)
        {
            matchedArgs[$"__POSITIONAL_ARG_{index}"] = value;
            return;
        }

        matchedArgs[key!] = value;
    }

    private void AddKeywordArgument(Dictionary<string, List<Ast>> matchedArgs, Argument arg)
    {
        if (this.UnlimitedKeywordArgumentsType is null)
        {
            matchedArgs[arg.Keyword!.Value.AsString] = arg.ValueAsAsts();
            return;
        }
    }

    private Dictionary<string, List<Ast>> HandleNoValidationArgumentMatching(List<Argument> arguments)
    {
        if (!this.IsBuiltin)
            Errors.AlwaysThrow(new SystemError("Method parameters is unvalidated, for a non-builtin method."));

        Dictionary<string, List<Ast>> matchedArgs = new();

        foreach (var arg in arguments)
        {
            matchedArgs.Add(arg.Keyword?.AsString ?? Guid.NewGuid().ToString(), arg.ValueAsAsts());
        }

        return matchedArgs;
    }

    private readonly MethodBody? _builtinBody;
    private readonly List<List<Ast>>? _userDefinedBody;
}