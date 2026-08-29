// using System.Diagnostics;
// using Aurora.Core;
// using Aurora.Evaluator.Internals;
//
// namespace Aurora.Evaluator.BuiltinObjects;
//
// internal class MethodObject : CallableObject
// {
//     public string Name { get; }
//     public Internals.Type ReturnType { get; }
//     public RuntimeObject? BoundSelf { get; }
//     private readonly List<ParameterDefinition>? Parameters;
//     private Internals.Type? UnlimitedPositionalArgsType { get; }
//     private Internals.Type? UnlimitedKeywordArgumentsType { get; }
//     private readonly MethodBody? _builtinBody;
//     private readonly List<List<Ast>>? _userDefinedBody;
//     private bool IsBuiltin => this._builtinBody is not null;
//
//     public override RuntimeObject Invoke(List<Argument> arguments, RuntimeContext parentContext, SourceLocation callSiteLocation)
//     {
//         if (this.BoundSelf is null)
//             Errors.AlwaysThrow(new UnsupportedOperationError($"Method `{this.Name}` is not bound"),
//                 null /* Todo: Try add a better source value*/);
//
//         RuntimeContext methodContext = parentContext.CreateChild(callSiteLocation);
//
//         Dictionary<string, RawMethodArgument> matchedArgs = this.MatchArgumentsToParameter(arguments, parentContext);
//
//         bool doNotValidate = this.IsBuiltin && this.Parameters is null;
//         if (doNotValidate)
//             return this._builtinBody!(this.BoundSelf, matchedArgs, methodContext);
//
//         Dictionary<string, RuntimeObject> validatedArgs = [];
//
//         if (this.UnlimitedPositionalArgsType is not null)
//             this.ValidateUnlimitedPositionalArguments(validatedArgs, matchedArgs, methodContext);
//
//         if (this.UnlimitedKeywordArgumentsType is not null)
//             this.ValidateUnlimitedKeywordArguments(validatedArgs, matchedArgs, methodContext);
//
//         if (this.Parameters is not null)
//             this.ValidateArguments(validatedArgs, matchedArgs, methodContext);
//
//         foreach (var (key, value) in validatedArgs)
//             methodContext.Create(key, value);
//
//         RuntimeObject returnedObject = null!;
//
//         if (this.IsBuiltin)
//             returnedObject = this._builtinBody!(this.BoundSelf, matchedArgs, methodContext);
//
//         if (!this.IsBuiltin)
//             returnedObject = Evaluator.ExecuteMethodAst(
//                 this._userDefinedBody!,
//                 this.BoundSelf,
//                 methodContext);
//
//         if (!returnedObject.Type.IsSubclassOf(this.ReturnType))
//             Errors.AlwaysThrow(new TypeMismatchError(
//                     $"Callable is declared to return a value of type {this.ReturnType.Name}, but a value of " +
//                     $"type {returnedObject.Type.Name} was returned",
//                     user: false),
//                 null /* Todo: Try add a better source value*/);
//
//         return returnedObject;
//         // Todo: Add invoke logic from other class here
//     }
//
//     private void ValidateArguments(Dictionary<string, RuntimeObject> validatedArgs,
//                                    Dictionary<string, RawMethodArgument> matchedArgs,
//                                    RuntimeContext context)
//     {
//         if (this.Parameters is null)
//             Errors.AlwaysThrow(new SystemError(
//                 "Cannot validate built-in method arguments when it has been specified not to validate " +
//                 "arguments"), null /* Todo: Try add a better source value*/);
//
//         foreach (ParameterDefinition parameter in this.Parameters)
//         {
//             validatedArgs[parameter.Name] = parameter.DefaultValue!;
//         }
//
//         foreach (var (key, rawArg) in matchedArgs)
//         {
//             RuntimeObject argObject = rawArg.Value.Evaluate(context);
//             ParameterDefinition? paramDefinition = this.Parameters.FirstOrDefault(x => x.Name == key);
//
//             if (paramDefinition is null && this.UnlimitedKeywordArgumentsType is null &&
//                 this.UnlimitedPositionalArgsType is null)
//                 Errors.AlwaysThrow(new ArgumentDeficitError($"Callable {this.Name} has no attribute `{key}`"),
//                     null /* Todo: Try add a better source value*/);
//
//             if (paramDefinition is not null && !argObject.Type.IsSubclassOf(paramDefinition.Type))
//                 Errors.AlwaysThrow(
//                     new TypeMismatchError(
//                         $"Cannot assign {argObject.Type.Name} to parameter {paramDefinition.Name} of " +
//                         $"type {paramDefinition.Type.Name}"), null /* Todo: Try add a better source value*/);
//
//             validatedArgs[key] = argObject;
//         }
//
//         foreach (var (key, value) in validatedArgs)
//             if (value is null)
//                 Errors.AlwaysThrow(new ArgumentDeficitError($"Parameter `{key}` is required"),
//                     null /* Todo: Try add a better source value*/);
//     }
//
//     private void ValidateUnlimitedKeywordArguments(Dictionary<string, RuntimeObject> validatedArgs,
//                                                    Dictionary<string, RawMethodArgument> matchedArgs,
//                                                    RuntimeContext context)
//     {
//         if (this.UnlimitedKeywordArgumentsType is null)
//             Errors.AlwaysThrow(
//                 new SystemError("Variadic (Unlimited) keyword arguments cannot be null after entering the " +
//                                 "argument validator"), null /* Todo: Try add a better source value*/);
//
//         foreach ((string key, RawMethodArgument rawArg) in matchedArgs)
//         {
//             RuntimeObject valueAsObject = rawArg.Value.Evaluate(context);
//
//             if (!valueAsObject.Type.IsSubclassOf(this.UnlimitedKeywordArgumentsType))
//                 Errors.AlwaysThrow(new ArgumentTypeMismatchError(
//                     $"Cannot assign {valueAsObject.Type.Name} to {this.UnlimitedKeywordArgumentsType.Name}"), context);
//
//             validatedArgs[key] = valueAsObject;
//         }
//     }
//
//     private void ValidateUnlimitedPositionalArguments(Dictionary<string, RuntimeObject> validatedArgs,
//                                                       Dictionary<string, RawMethodArgument> matchedArgs,
//                                                       RuntimeContext context)
//     {
//         if (this.UnlimitedPositionalArgsType is null)
//             Errors.AlwaysThrow(
//                 new SystemError("Variadic (Unlimited) positional arguments cannot be null after entering the " +
//                                 "argument validator"), null /* Todo: Try add a better source value*/);
//
//         foreach ((string key, RawMethodArgument rawArg) in matchedArgs)
//         {
//             RuntimeObject valueAsObject = rawArg.Value.Evaluate(context);
//
//             if (!valueAsObject.Type.IsSubclassOf(this.UnlimitedPositionalArgsType))
//                 Errors.AlwaysThrow(new ArgumentTypeMismatchError(
//                         $"Cannot assign {valueAsObject.Type.Name} to {this.UnlimitedPositionalArgsType.Name}"),
//                     null /* Todo: Try add a better source value*/);
//
//             validatedArgs[key] = valueAsObject;
//         }
//     }
//
//     private Dictionary<string, RawMethodArgument> MatchArgumentsToParameter(List<Argument> arguments,
//                                                                             RuntimeContext context)
//     {
//         Dictionary<string, RawMethodArgument> matchedArgs = new();
//
//         bool hasReachedKeywordArgument = false;
//
//         bool requiresNoValidation = this.Parameters is null
//                                     && this.UnlimitedKeywordArgumentsType is null
//                                     && this.UnlimitedPositionalArgsType is null;
//
//         if (requiresNoValidation) return this.HandleNoValidationArgumentMatching(arguments, context);
//
//         // Todo: Handle *args and **kwargs
//
//         for (int i = 0; i < arguments.Count; i++)
//         {
//             Argument arg = arguments[i];
//             bool isPositionalArgument = arg.Keyword is null;
//
//             if (isPositionalArgument && hasReachedKeywordArgument)
//                 Errors.AlwaysThrow(new InvalidSyntaxError("Positional arguments cannot exist after keyword arguments"),
//                     null /* Todo: Try add a better source value*/);
//
//             if (!isPositionalArgument)
//             {
//                 hasReachedKeywordArgument = true;
//                 this.AddKeywordArgument(matchedArgs, arg, context);
//                 continue;
//             }
//
//             ParameterDefinition? param = this.Parameters!.ElementAtOrDefault(i);
//             if (param is null && this.UnlimitedPositionalArgsType is null)
//             {
//                 Errors.RaiseError(new ArgumentSurplusError(
//                         $"Callable {this.Name} takes {this.Parameters!.Count} parameters, but {arguments.Count} were provided."),
//                     null /* Todo: Try add a better source value*/);
//                 break;
//             }
//
//             if (param is null && this.UnlimitedPositionalArgsType is not null)
//             {
//                 matchedArgs[$"__POSITIONAL_ARG_{i}"] = new RawMethodArgument(
//                     name: $"__POSITIONAL_ARG_{i}",
//                     value: arg.ValueAsAsts(context));
//                 continue;
//             }
//
//             this.AddPositionalArg(matchedArgs, param?.Name, arg.ValueAsAsts(context), i);
//         }
//
//         return matchedArgs;
//     }
//
//     private void AddPositionalArg(Dictionary<string, RawMethodArgument> matchedArgs, string? key, AstList value,
//                                   int index)
//     {
//         if (this.UnlimitedPositionalArgsType is not null)
//         {
//             matchedArgs[$"__POSITIONAL_ARG_{index}"] = new RawMethodArgument(
//                 name: $"__POSITIONAL_ARG_{index}",
//                 value: value);
//             return;
//         }
//
//         matchedArgs[key!] = new RawMethodArgument(
//             name: key!,
//             value: value);
//     }
//
//     private void AddKeywordArgument(Dictionary<string, RawMethodArgument> matchedArgs, Argument arg,
//                                     RuntimeContext context)
//     {
//         if (this.UnlimitedKeywordArgumentsType is null)
//         {
//             matchedArgs[arg.Keyword!.Value.AsString] = new RawMethodArgument(
//                 name: arg.Keyword!.Value.AsString,
//                 value: arg.ValueAsAsts(context),
//                 keywordPosition: arg.KeywordPosition);
//         }
//     }
//
//     private Dictionary<string, RawMethodArgument> HandleNoValidationArgumentMatching(List<Argument> arguments,
//         RuntimeContext context)
//     {
//         if (!this.IsBuiltin)
//             Errors.AlwaysThrow(new SystemError("Callable parameters is unvalidated, for a non-builtin method."),
//                 null /* Todo: Try add a better source value*/);
//
//         Dictionary<string, RawMethodArgument> matchedArgs = new();
//
//         foreach (Argument arg in arguments)
//         {
//             string keyword = arg.Keyword?.AsString ?? Guid.NewGuid().ToString();
//
//             matchedArgs.Add(keyword, new RawMethodArgument(
//                 name: keyword,
//                 value: arg.ValueAsAsts(context),
//                 keywordPosition: arg.KeywordPosition));
//         }
//
//         return matchedArgs;
//     }
//
//     public override bool Equals(RuntimeObject other)
//     {
//         Errors.AlwaysThrow(new UnsupportedOperationError("Cannot compare equality of methods"),
//             null /* Todo: Try add a better source value*/);
//         throw new UnreachableException();
//     }
// }
