using Aurora.BuiltinMethods;
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
        Collection,
        Invalid,
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

    private List<AstList>? _containedCollection { get; set; }

    public List<AstList>? ContainedCollection
    {
        get => _containedCollection;
        set
        {
            _containedCollection = value;
            this.UpdateState();
        }
    }

    public int? Position
    {
        get
        {
            if (this._target is not null)
                return this._target!.Value.StartCharPosition;

            return this._name?.StartCharPosition;
        }
    }

    public RuntimeObject Evaluate(RuntimeContext context, RuntimeObject? target = null)
    {
        bool isPartialOperation = this._state is AstStates.PartialAttributeAccess or AstStates.PartialMethodCall;
        bool targetProvidedWhenNotNeeded = target is not null && !isPartialOperation;
        bool targetNotProvidedWhenNeeded = target is null && isPartialOperation;

        if (targetNotProvidedWhenNeeded || targetProvidedWhenNotNeeded)
            Errors.AlwaysThrow(new SystemError($"Ast target state is invalid"), context);

        if (this._state is AstStates.Literal)
            return EvaluateLiteral(context);

        if (target is null && this._target is null)
            Errors.AlwaysThrow(new SystemError($"Ast target is null, and AST is not a literal"),
                context, position: this._name?.StartCharPosition);

        target ??= RuntimeObject.CreateFromToken(_target!.Value.Token, context);

        return this._state switch
        {
            AstStates.MethodCall => EvaluateMethodCall(context, target),
            AstStates.AttributeAccess => EvaluateAttributeAccess(context, target),
            AstStates.PartialMethodCall => EvaluateMethodCall(context, target),
            AstStates.PartialAttributeAccess => EvaluateAttributeAccess(context, target),
            AstStates.Collection => EvaluateCollection(context),
            _ => Errors.AlwaysThrow<RuntimeObject>(new SystemError("Ast state is invalid"), context),
        };
    }

    private RuntimeObject EvaluateMethodCall(RuntimeContext context, RuntimeObject target)
    {
        Method method = null!;
        if (target is Internals.Type type)
            method = type.GetStaticMethod(_name!.Value.AsString, context, _name?.StartCharPosition);

        if (target is not Internals.Type)
            method = target.Type.GetInstanceMethod(_name!.Value.AsString, context, _name?.StartCharPosition);

        return method.Invoke(target, _arguments!, context);
    }

    private RuntimeObject EvaluateAttributeAccess(RuntimeContext context, RuntimeObject target)
    {
        if (target is Internals.Type type)
            return type.GetStaticAttribute(_name!.Value.AsString, context, _name?.StartCharPosition)
                .GetValue(target, context);

        return target.Type.GetInstanceAttribute(_name!.Value.AsString, context, _name?.StartCharPosition)
            .GetValue(target, context);
    }

    private RuntimeObject EvaluateLiteral(RuntimeContext context)
    {
        return RuntimeObject.CreateFromToken(this._name!.Value.Token, context, _name?.StartCharPosition);
    }

    private UnitObject EvaluateCollection(RuntimeContext context)
    {
        foreach (AstList astList in _containedCollection!) Evaluator.EvaluateAll(); // Todo: Make evaluate ast list
        return new UnitObject();
    }

    private void UpdateState()
    {
        switch (this._target)
        {
            case null when this._name is not null && this._arguments is null && this._isALiteral &&
                           this._containedCollection is null:
                this._state = AstStates.Literal;
                return;

            case not null when _name is not null && _arguments is not null && !this._isALiteral &&
                               this._containedCollection is null:
                this._state = AstStates.MethodCall;
                return;

            case not null when _name is not null && _arguments is null && !this._isALiteral &&
                               this._containedCollection is null:
                this._state = AstStates.AttributeAccess;
                return;

            case null when this._name is not null && this._arguments is not null && !this._isALiteral &&
                           this._containedCollection is null:
                this._state = AstStates.PartialMethodCall;
                return;

            case null when this._name is not null && this._arguments is null && !this._isALiteral &&
                           this._containedCollection is null:
                this._state = AstStates.PartialAttributeAccess;
                return;

            case null when this._name is null && this._arguments is null && !this._isALiteral &&
                           this._containedCollection is not null:
                this._state = AstStates.Collection;
                return;

            default:
                this._state = AstStates.Invalid;
                return;
        }
    }
}
