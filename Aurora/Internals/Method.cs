namespace Aurora.Internals;

internal class Method
{
    public string Name { get; }
    public Type DeclaringType { get; }
    public List<ParameterDefinition>? Parameters;

    public Method(string name, Type returnType, List<ParameterDefinition>? parameters, MethodBody body)
    {
        this.Name = name;
        this.DeclaringType = returnType;
        this.Parameters = parameters;
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

        Dictionary<string, RuntimeObject> validatedArgs = ValidateArguments(matchedArgs, parentContext);

        foreach (var (key, value) in validatedArgs)
            methodContext.Create(key, value);

        if (this.IsBuiltin)
            return this._builtinBody!(self, matchedArgs, methodContext);

        return Evaluator.ExecuteMethodAst(
            this._userDefinedBody!,
            self,
            methodContext);
    }

    public Dictionary<string, RuntimeObject> ValidateArguments(Dictionary<string, List<Ast>> matchedArgs,
        RuntimeContext context)
    {
        if (this.Parameters is null)
            Errors.AlwaysThrow(new SystemError(
                "Cannot validate built-in method arguments when it has been specified not to validate " +
                "arguments"));

        return ValidateArguments(matchedArgs, this.Parameters, context);
    }

    public static Dictionary<string, RuntimeObject> ValidateArguments(Dictionary<string, List<Ast>> matchedArgs,
        List<ParameterDefinition> parameters, RuntimeContext context)
    {
        Dictionary<string, RuntimeObject?> validatedArgs = new();

        foreach (ParameterDefinition parameter in parameters)
        {
            validatedArgs.Add(parameter.Name, parameter.DefaultValue);
        }

        // Todo: trace entire program, figure out why .toString is not being hit.

        foreach (var (key, value) in matchedArgs)
        {
            RuntimeObject argObject = Evaluator.EvaluateAstList(value, context);
            ParameterDefinition? param = parameters.FirstOrDefault(x => x.Name == key);

            if (param is null)
                Errors.AlwaysThrow(new SystemError("An unmatched parameter has been found"));

            if (argObject.Type != param.Type)
                Errors.AlwaysThrow(
                    new TypeMismatchError(
                        $"Cannot assign {argObject.Type.Name} to parameter {param.Name} of type {param.Type.Name}"));

            validatedArgs[key] = argObject;
        }

        foreach (var (key, value) in validatedArgs)
            if (value is null)
                Errors.AlwaysThrow(new ArgumentDeficitError($"Parameter `{key}` is required"));

        return validatedArgs!;
    }

    private Dictionary<string, List<Ast>> MatchArgumentsToParameter(List<Argument> arguments)
    {
        Dictionary<string, List<Ast>> matchedArgs = new();

        bool hasReachedKeywordArgument = false;
        int positionalIndex = 0;

        foreach (var arg in arguments)
        {
            bool requiresNoValidation = this.IsBuiltin && this.Parameters is null;
            bool isPositionalArgument = arg.Keyword is null;

            if (requiresNoValidation)
            {
                matchedArgs.Add(arg.Keyword?.AsString ?? Guid.NewGuid().ToString(), arg.ValueAsAsts());
                continue;
            }

            if (this.Parameters is null)
                Errors.AlwaysThrow(new SystemError("Method parameters is unvalidated, for a non-builtin method."));

            if (isPositionalArgument && hasReachedKeywordArgument)
                Errors.AlwaysThrow(new InvalidSyntaxError("Positional arguments cannot exist after keyword arguments"),
                    arg.Value.First().StartCharPosition);


            if (!isPositionalArgument)
            {
                hasReachedKeywordArgument = true;
                matchedArgs[arg.Keyword.Value.AsString] = arg.ValueAsAsts();
                continue;
            }

            ParameterDefinition? param = this.Parameters!.ElementAtOrDefault(positionalIndex);
            if (param is null)
                Errors.RaiseError(new ArgumentSurplusError(
                    $"Method {this.Name} takes {this.Parameters!.Count} parameters, but {arguments.Count} were provided."));

            if (isPositionalArgument)
            {
                matchedArgs.Add(param!.Name, arg.ValueAsAsts());
            }
        }

        return matchedArgs;
    }

    private readonly MethodBody? _builtinBody;
    private readonly List<List<Ast>>? _userDefinedBody;
}