using Aurora.Evaluator.Internals;

namespace Aurora.Evaluator.BuiltinObjects;

public class InterfaceObject : RuntimeObject
{
    public RuntimeInterface Value;

    public InterfaceObject(RuntimeInterface value)
    {
        this.Value = value;
        this.Type = Builtins.Interface.Value;
    }

    public override bool Equals(RuntimeObject other)
    {
        if (other is not InterfaceObject interfaceObject)
            return false;

        return Value.Equals(interfaceObject.Value);

    }
}
