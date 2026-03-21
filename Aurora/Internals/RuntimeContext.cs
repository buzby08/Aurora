using System.Diagnostics;
using Aurora.BuiltinMethods;

namespace Aurora.Internals;

internal class RuntimeContext(RuntimeContext? parent)
{
    private readonly Dictionary<string, RuntimeObject> _variables = [];

    public RuntimeContext? Parent { get; } = parent;

    private RuntimeObject? GetOrNull(string name)
    {
        if (_variables.TryGetValue(name, out var value))
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
    public RuntimeObject Get(string name, bool systemFault = false)
    {
        RuntimeObject? value = this.GetOrNull(name);

        if (value is null)
            Errors.AlwaysThrow(new ObjectNotFoundError($"Object `{name}` not found", user: !systemFault));

        return value;
    }

    public RuntimeObject GetParam(string name)
    {
        if (_variables.TryGetValue(name, out var value))
            return value;

        Errors.AlwaysThrow(new ObjectNotFoundError($"Object `{name}` not found", user: false));
        throw new UnreachableException();
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

    public void Create(string name, RuntimeObject value)
    {
        RuntimeObject? old = this._variables.GetValueOrDefault(name);

        if (old is null)
        {
            _variables[name] = value;
            return;
        }

        Errors.AlwaysThrow(
            new VarAlreadyExistsError($"Variable `{name}` already exists. " +
                                      $"To redefine the variable, use .Set instead"));
    }

    public void Set(string name, RuntimeObject value)
    {
        RuntimeObject? old = this._variables.GetValueOrDefault(name);

        if (old is null && Parent is not null)
        {
            Parent.Set(name, value);
            return;
        }

        if (old is null && Parent is null)
        {
            Errors.AlwaysThrow(new ObjectNotFoundError($"Variable `{name}` not found"));
            return;
        }

        if (old!.Type != value.Type)
            Errors.AlwaysThrow(
                new TypeMismatchError($"Cannot assign value of type {value.Type.Name} to {old.Type.Name}"));

        _variables[name] = value;
    }
}