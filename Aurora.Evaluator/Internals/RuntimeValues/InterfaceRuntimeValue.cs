using Aurora.Core;
using Aurora.Evaluator.BuiltinObjects;

namespace Aurora.Evaluator.Internals.RuntimeValues;

public class InterfaceRuntimeValue(RuntimeInterface value) : BaseRuntimeValue(value)
{
    public RuntimeInterface GetValue(SourceLocation? location) => base.GetValue<RuntimeInterface>(location)!;
    public RuntimeInterface RawValue => (RuntimeInterface)this.Value!;

    public string Name => this.RawValue.Name;

    public static RuntimeObject GetAsObject(RuntimeInterface value) => new(new InterfaceRuntimeValue(value), Builtins.Interface);

    public override RuntimeObject GetAsRuntimeObject()
    {
        return new RuntimeObject(this, Builtins.Interface);
    }

    public void AddMethod(string name, TypeObject returnType, ParameterDefinition[] parameters)
    {
        this.RawValue.AddMethod(name, returnType, parameters);
    }

    public Method GetFilledMethod(string at, MethodBody func, SourceLocation? o)
    {
        return this.RawValue.GetFilledMethod(at, func, o);
    }
}
