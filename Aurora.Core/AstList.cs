using System.Collections;
using Aurora.Internals;

namespace Aurora;

internal class AstList : IEnumerable<Ast>
{
    public readonly List<Ast> Data = [];
    public int Count => this.Data.Count;
    public int? Position => this.Data.ElementAtOrDefault(0)?.Position;

    public void Add(Ast item)
    {
        this.Data.Add(item);
    }
    
    public void Clear()
    {
        this.Data.Clear();
    }

    public RuntimeObject Evaluate(RuntimeContext context)
    {
        if (this._evaluatedResult is not null) return this._evaluatedResult;
        
        this._evaluatedResult = Evaluator.EvaluateAstList(this, context);
        return this._evaluatedResult;
    }
    
    public IEnumerator<Ast> GetEnumerator()
    {
        return this.Data.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return this.GetEnumerator();
    }
    private RuntimeObject? _evaluatedResult = null;
}