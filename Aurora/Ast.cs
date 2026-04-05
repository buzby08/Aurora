using Aurora.Internals;

namespace Aurora;

internal class Ast
{
    public enum AstStates
    {
        Literal,
        MethodCall,
        AttributeAccess,
        PartialMethodCall,
        PartialAttributeAccess,
        Invalid
    }

    private AstStates _state { get; set; } = AstStates.Invalid;

    public AstStates State => _state;

    private TokenListItem? _target { get; set; }

    public TokenListItem? Target
    {
        get => _target;
        set
        {
            _target = value;
            this.UpdateState();
        }
    }

    public string? TargetAsString => _target?.AsString;

    private bool _isALiteral { get; set; } = false;

    public bool IsALiteral
    {
        get => _isALiteral;
        set
        {
            _isALiteral = value;
            this.UpdateState();
        }
    }

    private TokenListItem? _name { get; set; }

    public TokenListItem? Name
    {
        get => _name;
        set
        {
            _name = value;
            this.UpdateState();
        }
    }
    public string? NameAsString => _name?.AsString;

    private List<Argument>? _arguments { get; set; }

    public List<Argument>? Arguments
    {
        get => _arguments;
        set
        {
            _arguments = value;
            this.UpdateState();
        }
    }

    public RuntimeObject Evaluate(RuntimeContext context, RuntimeObject? target = null)
    {
        bool isPartialOperation = this._state is AstStates.PartialAttributeAccess or AstStates.PartialMethodCall;
        bool targetProvidedWhenNotNeeded = target is not null && !isPartialOperation;
        bool targetNotProvidedWhenNeeded = target is null && isPartialOperation;

        if (targetNotProvidedWhenNeeded || targetProvidedWhenNotNeeded)
            Errors.AlwaysThrow(new SystemError($"Ast target state is invalid"));

        if (this._state is AstStates.Literal)
            return EvaluateLiteral(context);

        if (target is null && this._target is null)
            Errors.AlwaysThrow(new SystemError($"Ast target is null, and AST is not a literal"),
                position: this._name?.StartCharPosition);

        target ??= RuntimeObject.CreateFromToken(_target!.Value.Token, context);

        return this._state switch
        {
            AstStates.MethodCall => EvaluateMethodCall(context, target),
            AstStates.AttributeAccess => EvaluateAttributeAccess(context, target),
            AstStates.PartialMethodCall => EvaluateMethodCall(context, target),
            AstStates.PartialAttributeAccess => EvaluateAttributeAccess(context, target),
            _ => Errors.AlwaysThrow<RuntimeObject>(new SystemError("Ast state is invalid")),
        };
    }

    private RuntimeObject EvaluateMethodCall(RuntimeContext context, RuntimeObject target)
    {
        Method method = null!;
        if (target is Internals.Type type)
            method = type.GetStaticMethod(_name!.Value.AsString, _name?.StartCharPosition);

        if (target is not Internals.Type)
            method = target.Type.GetInstanceMethod(_name!.Value.AsString);

        return method.Invoke(target, _arguments!, context);
    }

    private RuntimeObject EvaluateAttributeAccess(RuntimeContext context, RuntimeObject target)
    {
        if (target is Internals.Type type)
            return type.GetStaticAttribute(_name!.Value.AsString).GetValue(target, context);

        return target.Type.GetInstanceAttribute(_name!.Value.AsString, _name?.StartCharPosition).GetValue(target, context);
    }

    private RuntimeObject EvaluateLiteral(RuntimeContext context)
    {
        return RuntimeObject.CreateFromToken(this._name!.Value.Token, context, _name?.StartCharPosition);
    }

    private void UpdateState()
    {
        switch (this._target)
        {
            case null when this._name is not null && this._arguments is null && this._isALiteral:
                this._state = AstStates.Literal;
                return;

            case not null when _name is not null && _arguments is not null && !this._isALiteral:
                this._state = AstStates.MethodCall;
                return;

            case not null when _name is not null && _arguments is null && !this._isALiteral:
                this._state = AstStates.AttributeAccess;
                return;

            case null when this._name is not null && this._arguments is not null && !this._isALiteral:
                this._state = AstStates.PartialMethodCall;
                return;

            case null when this._name is not null && this._arguments is null && !this._isALiteral:
                this._state = AstStates.PartialAttributeAccess;
                return;

            default:
                this._state = AstStates.Invalid;
                return;
        }
    }
}