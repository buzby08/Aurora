using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Aurora.Core;

namespace Aurora.Evaluator.Internals;

public class RuntimeContext
{
    public static RuntimeContext? GlobalContext;

    public static string GlobalFilePath => GlobalContext?.CallSiteLocation.FilePath ??
                                           throw new InvalidOperationException("Global context not initialized");

    private readonly Dictionary<string, RuntimeObject> _variables = [];
    private string? thisVariable = null;

    public RuntimeContext? Parent { get; }
    public SourceLocation CallSiteLocation { get; }

    private RuntimeContext(RuntimeContext? parent, SourceLocation callSiteLocation)
    {
        this.Parent = parent;
        this.CallSiteLocation = callSiteLocation;
    }

    public static void CreateGlobalContext(string filePath)
    {
        GlobalContext ??= new RuntimeContext(null, new SourceLocation
        {
            FilePath = filePath,
            ColumnNumber = 0,
            LineNumber = 0,
            Offset = 0,
        });
    }

    public RuntimeContext CreateChild(SourceLocation callSiteLocation)
    {
        return new RuntimeContext(this, callSiteLocation);
    }

    public string[] GetVariables()
    {
        return this._variables.Keys.ToArray();
    }

    public string[] GetNewVariables(string[] oldVariables)
    {
        return this._variables.Keys.Except(oldVariables).ToArray();
    }


    private RuntimeObject? GetOrNull(string name)
    {
        if (this._variables.TryGetValue(name, out RuntimeObject? value))
            return value;

        return this.Parent?.GetOrNull(name);
    }

    /// <summary>
    /// Gets the value of a given variable, and throws an Aurora error if not found.
    /// </summary>
    /// <param name="name">The variable to get the value of.</param>
    /// <param name="systemFault">
    /// Indicates whether the variable not being found should be treated as a system fault. This defaults to
    /// <see langword="false"/>
    /// </param>
    /// <returns>The <see cref="RuntimeObject"/> of the variable.</returns>
    /// <exception cref="ObjectNotFoundError">
    /// This is when the variable does not exist in the current scope or any parent scopes. This is an Aurora runtime
    /// error, that gets passed to the user. If <paramref name="systemFault"/> is true, the users are indicated that
    /// this error is not because of their code. This is not a c# error.
    /// </exception>
    public RuntimeObject Get(string name, SourceLocation location, bool systemFault = false)
    {
        RuntimeObject? value = this.GetOrNull(name);

        if (value is null)
            Errors.AlwaysThrow(new ObjectNotFoundError($"Object `{name}` not found", user: !systemFault), location);

        return value;
    }

    public T Get<T>(string name, SourceLocation location, bool systemFault = false) where T : RuntimeObject
    {
        return (T)this.Get(name, location, systemFault);
    }

    /// <summary>
    /// Gets the value of a given variable, from the current context ONLY, and throws an error if it does not exist.
    /// </summary>
    /// <param name="name">The variable to get the value of.</param>
    /// <returns>The <see cref="RuntimeObject"/> of the variable.</returns>
    /// <exception cref="ObjectNotFoundError">
    /// This is when the variable does not exist in the current scope. This is an Aurora runtime
    /// error, that gets passed to the user. This gets thrown as a system fault. This is not a c# error.
    /// </exception>
    public RuntimeObject GetParam(string name)
    {
        if (this._variables.TryGetValue(name, out RuntimeObject? value))
            return value;

        Errors.AlwaysThrow(new ObjectNotFoundError($"Required object `{name}` not found", user: false), null);
        throw new UnreachableException();
    }

    public T GetParam<T>(string name) where T : RuntimeObject
    {
        return (T)this.GetParam(name);
    }

    public List<RuntimeObject> GetPositionalArgs()
    {
        List<RuntimeObject> result = new(this._variables.Count);

        foreach (KeyValuePair<string, RuntimeObject> pair in this._variables)
        {
            if (pair.Key.StartsWith("__POSITIONAL_ARG_", StringComparison.Ordinal))
                result.Add(pair.Value);
        }

        return result;
    }

    public RuntimeObject GetParamOrDefault(string name, RuntimeObject defaultValue)
    {
        return this._variables.GetValueOrDefault(name) ?? defaultValue;
    }

    public void Create(string name, RuntimeObject value, SourceLocation? location)
    {
        if (ReservedKeywords.Contains(name))
            Errors.AlwaysThrow(new InvalidSyntaxError($"Cannot set reserved keyword `{name}`"), location);

        this.InternalCreate(name, value, location);
    }

    private void InternalCreate(string name, RuntimeObject value, SourceLocation? location)
    {
        RuntimeObject? old = this._variables.GetValueOrDefault(name);

        if (old is null)
        {
            this._variables[name] = value;
            return;
        }

        Errors.AlwaysThrow(
            new VarAlreadyExistsError($"Variable `{name}` already exists. " +
                                      $"To redefine the variable, use .Set instead"), location);
    }

    public void Set(string name, RuntimeObject value, SourceLocation? location)
    {
        if (ReservedKeywords.Contains(name))
            Errors.AlwaysThrow(new InvalidSyntaxError($"Cannot redefine reserved keyword `{name}`"), location);

        this.InternalSet(name, value, location);
    }

    private void InternalSet(string name, RuntimeObject value, SourceLocation? location)
    {
        RuntimeObject? old = this._variables.GetValueOrDefault(name);

        if (old is null && this.Parent is not null)
        {
            this.Parent.Set(name, value, location);
            return;
        }

        if (old is null && this.Parent is null)
        {
            Errors.AlwaysThrow(new ObjectNotFoundError($"Variable `{name}` not found"), location);
            return;
        }

        if (old!.Type != value.Type)
            Errors.AlwaysThrow(
                new TypeMismatchError($"Cannot assign value of type {value.Type.Name} to {old.Type.Name}"), location);

        this._variables[name] = value;
    }

    internal void SetThis(string variable)
    {
        if (this.GetOrNull(variable) is null)
            return;

        this.thisVariable = variable;
    }

    internal void UpdateThis(RuntimeObject value)
    {
        if (this.thisVariable is null)
            Errors.AlwaysThrow(
                new UnsupportedOperationError("Cannot set a value to this where this hasnt been set", user: false),
                null);

        this.Set(this.thisVariable, value, null);
    }

    private static readonly string[] ReservedKeywords = ["this",];
}
