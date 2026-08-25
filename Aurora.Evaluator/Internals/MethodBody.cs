namespace Aurora.Internals;

delegate RuntimeObject MethodBody(
    RuntimeObject self,
    Dictionary<string, RawMethodArgument> args,
    RuntimeContext context);

internal class RawMethodArgument(string name, AstList value, int? keywordPosition = null)
{
    public string Name = name;
    public AstList Value = value;
    public int? KeywordKeywordPosition = keywordPosition;
    public int? ValuePosition = value.Position;
}