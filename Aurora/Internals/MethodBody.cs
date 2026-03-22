namespace Aurora.Internals;

delegate RuntimeObject MethodBody(
    RuntimeObject self,
    Dictionary<string, AstList> args,
    RuntimeContext context);