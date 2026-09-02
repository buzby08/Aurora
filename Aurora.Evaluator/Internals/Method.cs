using Aurora.Core;
using Aurora.Evaluator.BuiltinObjects;

namespace Aurora.Evaluator.Internals;

public class Method
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
        Argument[] args,
        RuntimeContext parentContext,
        SourceLocation callSite)
    {
        RuntimeContext methodContext = parentContext.CreateChild(callSite);

        methodContext.SetThis(self, callSite); // Todo: Figure out what 'this' should resolve to.

        Dictionary<string, RawMethodArgument> matchedArgs = this.MatchArgumentsToParameter(args, parentContext, callSite);

        bool doNotValidate = this.IsBuiltin && this.Parameters is null;
        if (doNotValidate)
            return this._builtinBody!(self, matchedArgs, methodContext);

        Dictionary<string, RuntimeObject> validatedArgs = [];

        if (this.UnlimitedPositionalArgsType is not null)
            this.ValidateUnlimitedPositionalArguments(validatedArgs, matchedArgs, methodContext, callSite);

        if (this.UnlimitedKeywordArgumentsType is not null)
            this.ValidateUnlimitedKeywordArguments(validatedArgs, matchedArgs, methodContext, callSite);

        if (this.Parameters is not null)
            this.ValidateArguments(validatedArgs, matchedArgs, methodContext, callSite);

        foreach (var (key, value) in validatedArgs)
            methodContext.Create(key, value, callSite);

        RuntimeObject? returnedObject = null;

        if (this.IsBuiltin)
            returnedObject = this._builtinBody!(self, matchedArgs, methodContext);

        if (!this.IsBuiltin)
        {
            using Evaluator evaluator = Evaluator.CreateChild(methodContext);
            returnedObject = evaluator.EvaluateMultipleExpressions(this._userDefinedBody!);
        }

        if (returnedObject is null)
            Errors.AlwaysThrow(new InvalidReturnTypeError($"Method {this.Name} did not return a value"), callSite);

        if (returnedObject.Type != this.DeclaringType)
            Errors.AlwaysThrow(new TypeMismatchError(
                    $"Callable is declared to return a value of type {this.DeclaringType.Name}, but a value of " +
                    $"type {returnedObject.Type.Name} was returned",
                    user: false),
                callSite);

        return returnedObject;
    }

    public void ValidateArguments(Dictionary<string, RuntimeObject> validatedArgs,
                                  Dictionary<string, RawMethodArgument> matchedArgs,
                                  RuntimeContext context,
                                  SourceLocation location)
    {
        if (this.Parameters is null)
            Errors.AlwaysThrow(new SystemError(
                "Cannot validate built-in method arguments when it has been specified not to validate " +
                "arguments"), location);

        foreach (ParameterDefinition parameter in this.Parameters)
        {
            validatedArgs[parameter.Name] = parameter.DefaultValue!;
        }

        foreach (var (key, rawArg) in matchedArgs)
        {
            using Evaluator evaluator = Evaluator.CreateChild(context);
            RuntimeObject argObject = evaluator.EvaluateExpressionForValue(rawArg.Value);
            ParameterDefinition? paramDefinition = this.Parameters.FirstOrDefault(x => x.Name == key);

            if (paramDefinition is null && this.UnlimitedKeywordArgumentsType is null &&
                this.UnlimitedPositionalArgsType is null)
                Errors.AlwaysThrow(new ArgumentDeficitError($"Callable {this.Name} has no attribute `{key}`"), location);

            if (paramDefinition is not null && !argObject.Type.IsSubclassOf(paramDefinition.Type))
                Errors.AlwaysThrow(
                    new TypeMismatchError(
                        $"Cannot assign {argObject.Type.Name} to parameter {paramDefinition.Name} of " +
                        $"type {paramDefinition.Type.Name}"), location);

            validatedArgs[key] = argObject;
        }

        foreach (var (key, value) in validatedArgs)
            if (value is null)
                Errors.AlwaysThrow(new ArgumentDeficitError($"Parameter `{key}` is required"), location);
    }

    private void ValidateUnlimitedKeywordArguments(Dictionary<string, RuntimeObject> validatedArgs,
                                                   Dictionary<string, RawMethodArgument> matchedArgs,
                                                   RuntimeContext context,
                                                   SourceLocation location)
    {
        if (this.UnlimitedKeywordArgumentsType is null)
            Errors.AlwaysThrow(
                new SystemError("Variadic (Unlimited) keyword arguments cannot be null after entering the " +
                                "argument validator"), location);

        foreach ((string key, RawMethodArgument rawArg) in matchedArgs)
        {
            using Evaluator evaluator = Evaluator.CreateChild(context);
            RuntimeObject valueAsObject = evaluator.EvaluateExpressionForValue(rawArg.Value);

            if (!valueAsObject.Type.IsSubclassOf(this.UnlimitedKeywordArgumentsType))
                Errors.AlwaysThrow(new ArgumentTypeMismatchError(
                    $"Cannot assign {valueAsObject.Type.Name} to {this.UnlimitedKeywordArgumentsType.Name}"), location);

            validatedArgs[key] = valueAsObject;
        }
    }

    private void ValidateUnlimitedPositionalArguments(Dictionary<string, RuntimeObject> validatedArgs,
                                                      Dictionary<string, RawMethodArgument> matchedArgs,
                                                      RuntimeContext context,
                                                      SourceLocation location)
    {
        if (this.UnlimitedPositionalArgsType is null)
            Errors.AlwaysThrow(
                new SystemError("Variadic (Unlimited) positional arguments cannot be null after entering the " +
                                "argument validator"), location);

        foreach ((string key, RawMethodArgument rawArg) in matchedArgs)
        {
            using Evaluator evaluator = Evaluator.CreateChild(context);
            RuntimeObject valueAsObject = evaluator.EvaluateExpressionForValue(rawArg.Value);

            if (!valueAsObject.Type.IsSubclassOf(this.UnlimitedPositionalArgsType))
                Errors.AlwaysThrow(new ArgumentTypeMismatchError(
                    $"Cannot assign {valueAsObject.Type.Name} to {this.UnlimitedPositionalArgsType.Name}"), location);

            validatedArgs[key] = valueAsObject;
        }
    }

    private Dictionary<string, RawMethodArgument> MatchArgumentsToParameter(Argument[] arguments,
                                                                            RuntimeContext context,
                                                                            SourceLocation location)
    {
        Dictionary<string, RawMethodArgument> matchedArgs = new();

        bool hasReachedKeywordArgument = false;

        bool requiresNoValidation = this.Parameters is null
                                    && this.UnlimitedKeywordArgumentsType is null
                                    && this.UnlimitedPositionalArgsType is null;

        if (requiresNoValidation) return this.HandleNoValidationArgumentMatching(arguments, location);

        // Todo: Handle *args and **kwargs

        for (int i = 0; i < arguments.Length; i++)
        {
            Argument arg = arguments[i];
            bool isPositionalArgument = arg.Identifier is null;

            if (isPositionalArgument && hasReachedKeywordArgument)
                Errors.AlwaysThrow(new InvalidSyntaxError("Positional arguments cannot exist after keyword arguments"),
                    location);

            if (!isPositionalArgument)
            {
                hasReachedKeywordArgument = true;
                this.AddKeywordArgument(matchedArgs, arg);
                continue;
            }

            ParameterDefinition? param = this.Parameters!.ElementAtOrDefault(i);
            if (param is null && this.UnlimitedPositionalArgsType is null)
            {
                Errors.RaiseError(new ArgumentSurplusError(
                        $"Callable {this.Name} takes {this.Parameters!.Count} parameters, but {arguments.Length} were provided."),
                    location);
                break;
            }

            if (param is null && this.UnlimitedPositionalArgsType is not null)
            {
                matchedArgs[$"__POSITIONAL_ARG_{i}"] = new RawMethodArgument(
                    name: $"__POSITIONAL_ARG_{i}",
                    value: arg.Value);
                continue;
            }

            this.AddPositionalArg(matchedArgs, param?.Name, arg.Value, i);
        }

        return matchedArgs;
    }

    private void AddPositionalArg(Dictionary<string, RawMethodArgument> matchedArgs, string? key, Ast[] value,
                                  int index)
    {
        if (this.UnlimitedPositionalArgsType is not null)
        {
            matchedArgs[$"__POSITIONAL_ARG_{index}"] = new RawMethodArgument(
                name: $"__POSITIONAL_ARG_{index}",
                value: value);
            return;
        }

        matchedArgs[key!] = new RawMethodArgument(
            name: key!,
            value: value);
    }

    private void AddKeywordArgument(Dictionary<string, RawMethodArgument> matchedArgs, Argument arg)
    {
        if (this.UnlimitedKeywordArgumentsType is null)
        {
            matchedArgs[arg.Identifier!.ValueAsString] = new RawMethodArgument(
                name: arg.Identifier.ValueAsString,
                value: arg.Value);
        }
    }

    private Dictionary<string, RawMethodArgument> HandleNoValidationArgumentMatching(Argument[] arguments, SourceLocation location)
    {
        if (!this.IsBuiltin)
            Errors.AlwaysThrow(new SystemError("Callable parameters are unvalidated, for a non-builtin method."),
                location);

        Dictionary<string, RawMethodArgument> matchedArgs = new();

        for (int index = 0; index < arguments.Length; index++)
        {
            Argument arg = arguments[index];
            string keyword = arg.Identifier?.ValueAsString ?? $"ARG_{index}";

            matchedArgs.Add(keyword, new RawMethodArgument(
                name: keyword,
                value: arg.Value));
        }

        return matchedArgs;
    }


    private readonly MethodBody? _builtinBody;
    private readonly List<List<Ast>>? _userDefinedBody;
}
