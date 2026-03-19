namespace Aurora.Internals;

delegate RuntimeObject MethodBody(
    RuntimeObject self,
    Dictionary<string, List<Ast>> args,
    RuntimeContext context);