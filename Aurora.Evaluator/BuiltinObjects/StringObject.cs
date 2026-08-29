using Aurora.Evaluator.Internals;

namespace Aurora.Evaluator.BuiltinObjects;

public class StringObject : RuntimeObject
{
    public string Value;

    public StringObject(string value)
    {
        this.Value = value;
        Type = Builtins.String;
    }

    public override bool Equals(RuntimeObject other)
    {
        if (other is StringObject stringObject)
            return this.Value == stringObject.Value;

        return false;
    }
}