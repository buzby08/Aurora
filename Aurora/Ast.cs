namespace Aurora;

internal class Ast
{
    public enum AstStates
    {
        MethodCall,
        AttributeAccess,
        PartialMethodCall,
        PartialAttributeAccess,
        Invalid
    }

    private AstStates _state { get; set; } = AstStates.Invalid;

    public AstStates State => _state;

    private string? _target { get; set; } = null;

    public string? Target
    {
        get => _target;
        set
        {
            _target = value;
            this.UpdateState();
        }
    }

    private string? _name { get; set; } = null;

    public string? Name
    {
        get => _name;
        set
        {
            _name = value;
            this.UpdateState();
        }
    }

    private List<Argument>? _arguments { get; set; } = null;

    public List<Argument>? Arguments
    {
        get => _arguments;
        set
        {
            _arguments = value;
            this.UpdateState();
        }
    }

    public TokenListItem Evaluate()
    {
        // Todo: Implement

        throw new NotImplementedException();
    }

    private void UpdateState()
    {
        switch (this._target)
        {
            case not null when _name is not null && _arguments is not null:
                this._state = AstStates.MethodCall;
                return;

            case not null when _name is not null && _arguments is null:
                this._state = AstStates.AttributeAccess;
                return;

            case null when this._name is not null && this._arguments is not null:
                this._state = AstStates.PartialMethodCall;
                return;

            case null when this._name is not null && this._arguments is null:
                this._state = AstStates.PartialAttributeAccess;
                return;

            default:
                this._state = AstStates.Invalid;
                return;
        }
    }
}