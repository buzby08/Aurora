// namespace Aurora.Core;
//
// public class Ast
// {
//     public enum AstStates
//     {
//         Literal,
//         MethodCall,
//         AttributeAccess,
//         PartialMethodCall,
//         PartialAttributeAccess,
//         Collection,
//         Invalid,
//     }
//
//     private AstStates _state { get; set; } = AstStates.Invalid;
//
//     public AstStates State => this._state;
//
//     private TokenListItem? _target { get; set; }
//
//     public TokenListItem? Target
//     {
//         get => this._target;
//         set
//         {
//             this._target = value;
//             this.UpdateState();
//         }
//     }
//
//     public string? TargetAsString => this._target?.AsString;
//
//     private bool _isALiteral { get; set; } = false;
//
//     public bool IsALiteral
//     {
//         get => this._isALiteral;
//         set
//         {
//             this._isALiteral = value;
//             this.UpdateState();
//         }
//     }
//
//     private TokenListItem? _name { get; set; }
//
//     public TokenListItem? Name
//     {
//         get => this._name;
//         set
//         {
//             this._name = value;
//             this.UpdateState();
//         }
//     }
//
//     public string? NameAsString => this._name?.AsString;
//
//     private List<Argument>? _arguments { get; set; }
//
//     public List<Argument>? Arguments
//     {
//         get => this._arguments;
//         set
//         {
//             this._arguments = value;
//             this.UpdateState();
//         }
//     }
//
//     private List<AstList>? _containedCollection { get; set; }
//
//     public List<AstList>? ContainedCollection
//     {
//         get => this._containedCollection;
//         set
//         {
//             this._containedCollection = value;
//             this.UpdateState();
//         }
//     }
//
//     public int? Position
//     {
//         get
//         {
//             if (this._target is not null)
//                 return 0; // START
//
//             return 0; // START
//         }
//     }
//
//     public RuntimeObject Evaluate(RuntimeContext context, RuntimeObject? target = null)
//     {
//         bool isPartialOperation = this._state is AstStates.PartialAttributeAccess or AstStates.PartialMethodCall;
//         bool targetProvidedWhenNotNeeded = target is not null && !isPartialOperation;
//         bool targetNotProvidedWhenNeeded = target is null && isPartialOperation;
//
//         if (targetNotProvidedWhenNeeded || targetProvidedWhenNotNeeded)
//             Errors.AlwaysThrow(new SystemError($"Ast target state is invalid"), context);
//
//         if (this._state is AstStates.Literal)
//             return this.EvaluateLiteral(context);
//
//         if (target is null && this._target is null)
//             Errors.AlwaysThrow(new SystemError($"Ast target is null, and AST is not a literal"),
//                 context, position: 0 /* Start */);
//
//         target ??= RuntimeObject.CreateFromToken(this._target!.Value.Token, context);
//
//         return this._state switch
//         {
//             AstStates.MethodCall => this.EvaluateMethodCall(context, target),
//             AstStates.AttributeAccess => this.EvaluateAttributeAccess(context, target),
//             AstStates.PartialMethodCall => this.EvaluateMethodCall(context, target),
//             AstStates.PartialAttributeAccess => this.EvaluateAttributeAccess(context, target),
//             AstStates.Collection => this.EvaluateCollection(context),
//             _ => Errors.AlwaysThrow<RuntimeObject>(new SystemError("Ast state is invalid"), context),
//         };
//     }
//
//     private RuntimeObject EvaluateMethodCall(RuntimeContext context, RuntimeObject target)
//     {
//         Method method = null!;
//         if (target is Internals.Type type)
//             method = type.GetStaticMethod(this._name!.Value.AsString, context, 0 /* Start */);
//
//         if (target is not Internals.Type)
//             method = target.Type.GetInstanceMethod(this._name!.Value.AsString, context, 0 /* Start */);
//
//         return method.Invoke(target, this._arguments!, context);
//     }
//
//     private RuntimeObject EvaluateAttributeAccess(RuntimeContext context, RuntimeObject target)
//     {
//         if (target is Internals.Type type)
//             return type.GetStaticAttribute(this._name!.Value.AsString, context, 0 /* Start */)
//                 .GetValue(target, context);
//
//         return target.Type.GetInstanceAttribute(this._name!.Value.AsString, context, 0 /* Start */)
//             .GetValue(target, context);
//     }
//
//     private RuntimeObject EvaluateLiteral(RuntimeContext context)
//     {
//         return RuntimeObject.CreateFromToken(this._name!.Value.Token, context, 0 /* Start */);
//     }
//
//     private UnitObject EvaluateCollection(RuntimeContext context)
//     {
//         foreach (AstList astList in this._containedCollection!) Evaluator.EvaluateAll(); // Todo: Make evaluate ast list
//         return new UnitObject();
//     }
//
//     private void UpdateState()
//     {
//         switch (this._target)
//         {
//             case null when this._name is not null && this._arguments is null && this._isALiteral &&
//                            this._containedCollection is null:
//                 this._state = AstStates.Literal;
//                 return;
//
//             case not null when this._name is not null && this._arguments is not null && !this._isALiteral &&
//                                this._containedCollection is null:
//                 this._state = AstStates.MethodCall;
//                 return;
//
//             case not null when this._name is not null && this._arguments is null && !this._isALiteral &&
//                                this._containedCollection is null:
//                 this._state = AstStates.AttributeAccess;
//                 return;
//
//             case null when this._name is not null && this._arguments is not null && !this._isALiteral &&
//                            this._containedCollection is null:
//                 this._state = AstStates.PartialMethodCall;
//                 return;
//
//             case null when this._name is not null && this._arguments is null && !this._isALiteral &&
//                            this._containedCollection is null:
//                 this._state = AstStates.PartialAttributeAccess;
//                 return;
//
//             case null when this._name is null && this._arguments is null && !this._isALiteral &&
//                            this._containedCollection is not null:
//                 this._state = AstStates.Collection;
//                 return;
//
//             default:
//                 this._state = AstStates.Invalid;
//                 return;
//         }
//     }
// }
