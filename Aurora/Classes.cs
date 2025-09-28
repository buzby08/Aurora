using System;
using System.Diagnostics;
using Aurora.Commands;
using Math = Aurora.Commands.Math;
using String = string;

namespace Aurora;

internal static class Classes
{
    private const String _owner = "Aurora.Classes";

    public static String? CurrentSelectedClass = null;
    public static String? CurrentSelectedMethod = null;
    public static String? CurrentSelectedProperty = null;

    public static Dictionary<String, CustomClass> SystemClasses =>
        Memory.Get("SystemClasses", _owner)?.Value ?? new Dictionary<String, CustomClass>();

    public static Dictionary<String, CustomClass> UserClasses =>
        Memory.Get("UserClasses", _owner)?.Value ?? new Dictionary<String, CustomClass>();

    private static void RegisterSystemClass(string name, CustomClass systemClass)
    {
        Dictionary<String, CustomClass> systemClasses = SystemClasses;
        systemClasses.Add(name, systemClass);
        Memory.Update("SystemClasses", _owner, systemClasses);
    }

    public static void RegisterClass(String name, CustomClass customClass)
    {
        Dictionary<String, CustomClass> userClasses = UserClasses;
        userClasses.Add(name, customClass);
        Memory.Update("UserClasses", _owner, userClasses);
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
        RegisterSystemClass("Terminal", terminal);

        CustomClass variables = new CustomClass("Variables");
        variables.AddMethod("create", Commands.Variables.Create);
        variables.AddMethod("edit", Commands.Variables.Edit);
        RegisterSystemClass("Variables", variables);

        CustomClass integer = new CustomClass("Integer");
        integer.AddMethod("create", Integer.Create);
        RegisterSystemClass("Integer", integer);

        CustomClass floatClass = new CustomClass("Float");
        floatClass.AddMethod("create", Float.Create);
        RegisterSystemClass("Float", floatClass);

        CustomClass stringClass = new CustomClass("String");
        stringClass.AddMethod("create", StringClass.Create);
        RegisterSystemClass("String", stringClass);

        CustomClass boolean = new CustomClass("Boolean");
        boolean.AddMethod("create", Commands.Boolean.Create);
        boolean.AddMethod("toStyle", Commands.Boolean.ToStyle);
        boolean.AddAttribute("wordOptionStyle", () => Commands.Boolean.WordOptionStyle);
        boolean.AddAttribute("numberOptionStyle", () => Commands.Boolean.NumberOptionStyle);
        boolean.AddAttribute("binaryOptionStyle", () => Commands.Boolean.BinaryOptionStyle);
        boolean.AddAttribute("charOptionStyle", () => Commands.Boolean.CharOptionStyle);
        RegisterSystemClass("Boolean", boolean);

        CustomClass math = new CustomClass("Math");
        math.AddMethod("pow", Math.Pow);
        math.AddMethod("abs", Math.Abs);
        math.AddMethod("round", Math.Round);
        RegisterSystemClass("Math", math);

        CustomClass system = new CustomClass("System");
        system.AddMethod("exit", SystemCommands.Exit);
        system.AddMethod("restart", SystemCommands.Restart);
        system.AddMethod("reload", SystemCommands.Reload);
        RegisterSystemClass("System", system);
    }

    public static CustomClass GetClass(string name)
    {
        if (ClassExists(name))
            return SystemClasses.TryGetValue(name, out var cls) ? cls : UserClasses.GetValueOrDefault(name, null);

        Errors.AlwaysThrow(new ModuleNotFoundError($"The class '{name}' does not exist in this context"));
        throw new UnreachableException();
    }

    public static Token CallClass(List<Token> positionals, Dictionary<string, Token> keywords, List<Ast> raw)
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

        return currentMethod(positionals, keywords, raw);
    }

    public static void InitialiseClasses()
    {
        MemoryItem systemClasses = new MemoryItem
        {
            Owner = _owner,
            Name = "SystemClasses",
            Value = new Dictionary<string, CustomClass>()
        };
        MemoryItem userClasses = new MemoryItem
        {
            Owner = _owner,
            Name = "UserClasses",
            Value = new Dictionary<string, CustomClass>()
        };

        Memory.Save(systemClasses);
        Memory.Save(userClasses);

        RegisterSystemClasses();
    }

    public static bool ClassExists(string name) => SystemClasses.ContainsKey(name) || UserClasses.ContainsKey(name);
}