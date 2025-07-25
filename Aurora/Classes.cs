using System;
using System.Diagnostics;
using Aurora.Commands;
using String = System.String;

namespace Aurora;

internal static class Classes
{
    public static string? CurrentSelectedClass = null;
    public static string? CurrentSelectedMethod = null;
    public static string? CurrentSelectedProperty = null;

    public static Dictionary<string, CustomClass> SystemClasses { get; set; } = new();
    public static Dictionary<string, CustomClass> UserClasses { get; set; } = new();

    public static void RegisterClass(string name, CustomClass customClass)
    {
        UserClasses[name] = customClass;
    }

    public static void RegisterSystemClasses()
    {
        CustomClass terminal = new CustomClass("Terminal");
        terminal.AddMethod("write", Terminal.Write);
        terminal.AddMethod("read", Terminal.Read);
        terminal.AddMethod("readInt", Terminal.ReadInt);
        terminal.AddMethod("readFloat", Terminal.ReadFloat);
        terminal.AddMethod("readBoolean", Terminal.ReadBoolean);
        terminal.AddMethod("clear", Terminal.Clear);
        terminal.AddAttribute("color", () => Terminal.Color);
        SystemClasses.Add("Terminal", terminal);

        CustomClass variables = new CustomClass("Variables");
        variables.AddMethod("create", Commands.Variables.Create);
        variables.AddMethod("edit", Commands.Variables.Edit);
        SystemClasses.Add("Variables", variables);

        CustomClass integer = new CustomClass("Integer");
        integer.AddMethod("create", Integer.Create);
        SystemClasses.Add("Integer", integer);

        CustomClass floatClass = new CustomClass("Float");
        floatClass.AddMethod("create", Float.Create);
        SystemClasses.Add("Float", floatClass);

        CustomClass stringClass = new CustomClass("String");
        stringClass.AddMethod("create", StringClass.Create);
        SystemClasses.Add("String", stringClass);

        CustomClass boolean = new CustomClass("Boolean");
        boolean.AddMethod("create", Commands.Boolean.Create);
        boolean.AddMethod("toStyle", Commands.Boolean.ToStyle);
        SystemClasses.Add("Boolean", boolean);
    }

    public static CustomClass GetClass(string name)
    {
        if (ClassExists(name))
            return SystemClasses.TryGetValue(name, out var cls) ? cls : UserClasses.GetValueOrDefault(name, null);

        Errors.AlwaysThrow(new ModuleNotFoundError($"The class '{name}' does not exist in this context"));
        throw new UnreachableException();
    }

    public static Token CallClass(List<Token> positionals, Dictionary<string, Token> keywords)
    {
        if (CurrentSelectedClass is null)
        {
            Errors.AlwaysThrow(new SystemError("The system tried to access a class that did not exist!"));
        }

        if (!ClassExists(CurrentSelectedClass))
        {
            Errors.AlwaysThrow(
                new ModuleNotFoundError($"The class '{CurrentSelectedClass}' doesnt exist in the current context"));
        }

        CustomClass currentClass = GetClass(CurrentSelectedClass);

        if (CurrentSelectedMethod is null)
        {
            Errors.AlwaysThrow(new SystemError("The system tried to call a method that did not exist!"));
        }

        if (!currentClass.HasMethod(CurrentSelectedMethod))
        {
            Errors.AlwaysThrow(
                new InvalidMethodError($"The class '{CurrentSelectedClass}' has no method '{CurrentSelectedMethod}'"));
        }

        CustomClass.CustomMethod currentMethod = currentClass.Methods[CurrentSelectedMethod];

        return currentMethod(positionals, keywords);
    }

    public static bool ClassExists(string name) => SystemClasses.ContainsKey(name) || UserClasses.ContainsKey(name);
}