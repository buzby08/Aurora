using System;
using System.Diagnostics;
using Aurora.Commands;

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
        CustomClass terminal = new CustomClass("terminal");
        terminal.AddMethod("write", Terminal.Write);
        terminal.AddMethod("read", Terminal.Read);
        terminal.AddMethod("clear", Terminal.Clear);
        terminal.AddAttribute("color", () => Terminal.Color);
        
        SystemClasses.Add("terminal", terminal);
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