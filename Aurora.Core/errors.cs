using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Aurora.Core;

public class Errors
{
    public static string ConfigFilePath { get; set; } = "AuroraConfig.json";
    public static List<string> Warnings = [];

    [DoesNotReturn]
    public static void OutputWarningsAndExit()
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Yellow;
        foreach (string warning in Warnings)
            Console.WriteLine(warning);
        Console.ResetColor();
    }

    [DoesNotReturn]
    public static T AlwaysThrow<T>(ErrorTypes error, SourceLocation location)
    {
        AlwaysThrow(error, location);
        throw new UnreachableException();
    }

    [DoesNotReturn]
    public static void AlwaysThrow(ErrorTypes error, SourceLocation location)
    {
        RaiseError(error, location, alwaysThrow: true);

        if (InternalVariables.DisableErrors)
            return;
        Environment.Exit(1);
        throw new UnreachableException();
    }

    public static void RaiseError(ErrorTypes error, SourceLocation location,
                                  bool alwaysThrow = false)
    {
        string positionMessage = $"{location.FilePath} Line {location.LineNumber} : Column {location.ColumnNumber}";

        string outputMessage = $"{{{positionMessage}}} ({error.Code}) {error.Title} - {error.Message}";

        bool isError = error.AlwaysError /*|| UserConfiguration.Errors.Contains(error.Code)*/ || alwaysThrow;

        if (!isError || InternalVariables.DisableErrors)
        {
            Logs.Warning(outputMessage);
            Warnings.Add("[WARNING]" + outputMessage);
            return;
        }

        Logs.Error(outputMessage);

        Environment.Exit(1);
    }

    public static void Log(string title, string message)
    {
        using var writer = File.AppendText(Logs.LogFilePath);
        writer.WriteLine($"Custom Log: {title} - {message}");
    }
}

public abstract class ErrorTypes
{
    public abstract string Title { get; }
    public abstract string Description { get; }
    public abstract string Message { get; }
    public abstract string Code { get; }
    public virtual bool AlwaysError => false;
}

public class ArgumentSurplusError : ErrorTypes
{
    public override string Title { get; }

    public sealed override string Description => "Too many arguments were provided for this method or operation";
    public override string Message { get; }

    public override string Code => "Aurora.ArgSurplus";

    public ArgumentSurplusError(string? message = null, bool user = true)
    {
        this.Message = string.IsNullOrEmpty(message) ? this.Description : message;
        this.Title = "Argument Surplus" + (user ? " (User)" : " (System)");
    }
}

public class ArgumentDeficitError : ErrorTypes
{
    public override string Title { get; }
    public sealed override string Description => "Not enough arguments were provided for this method or operation";
    public override string Message { get; }
    public override string Code => "Aurora.ArgDeficit";

    public override bool AlwaysError => true;

    public ArgumentDeficitError(string? message = null, bool user = true)
    {
        this.Message = string.IsNullOrEmpty(message) ? this.Description : message;
        this.Title = "Argument Deficit" + (user ? " (User)" : " (System)");
    }
}

public class ArgumentTypeMismatchError : ErrorTypes
{
    public override string Title { get; }

    public sealed override string Description =>
        "An argument was provided with a type that does not match the expected type";

    public override string Message { get; }

    public override string Code => "Aurora.ArgTypeMismatch";

    public ArgumentTypeMismatchError(string? message = null, bool user = true)
    {
        this.Message = string.IsNullOrEmpty(message) ? this.Description : message;
        this.Title = "Argument Type Mismatch" + (user ? " (User)" : " (System)");
    }
}

public class MissingRequiredArgError : ErrorTypes
{
    public override string Title { get; }
    public sealed override string Description => "A required argument is missing from this method call";
    public override string Message { get; }
    public override string Code => "Aurora.MissingRequiredArg";
    public override bool AlwaysError => true;

    public MissingRequiredArgError(string? message = null, bool user = true)
    {
        this.Message = string.IsNullOrEmpty(message) ? this.Description : message;
        this.Title = "Missing Required Argument" + (user ? " (User)" : " (System)");
    }
}

public class UnexpectedKeywordArgError : ErrorTypes
{
    public override string Title { get; }
    public sealed override string Description => "An unexpected keyword argument was supplied";
    public override string Message { get; }
    public override string Code => "Aurora.UnexpectedKeywordArg";

    public UnexpectedKeywordArgError(string? message = null, bool user = true)
    {
        this.Message = string.IsNullOrEmpty(message) ? this.Description : message;
        this.Title = "Unexpected Keyword Argument" + (user ? " (User)" : " (System)");
    }
}

public class InvalidArgNameError : ErrorTypes
{
    public override string Title { get; }
    public sealed override string Description => "The provided argument name is invalid or reserved";
    public override string Message { get; }
    public override string Code => "Aurora.InvalidArgName";

    public InvalidArgNameError(string? message = null, bool user = true)
    {
        this.Message = string.IsNullOrEmpty(message) ? this.Description : message;
        this.Title = "Invalid Argument Name" + (user ? " (User)" : " (System)");
    }
}

public class ObjectNotFoundError : ErrorTypes
{
    public override string Title { get; }

    public sealed override string Description =>
        "Attempted to access an object that does not exist in the current context";

    public override string Message { get; }
    public override string Code => "Aurora.ObjectNotFound";

    public ObjectNotFoundError(string? message = null, bool user = true)
    {
        this.Message = string.IsNullOrEmpty(message) ? this.Description : message;
        this.Title = "Object Not Found" + (user ? " (User)" : " (System)");
    }
}

public class VarAlreadyExistsError : ErrorTypes
{
    public override string Title { get; }
    public sealed override string Description => "A variable with the same name already exists";
    public override string Message { get; }
    public override string Code => "Aurora.VarAlreadyExists";

    public VarAlreadyExistsError(string? message = null, bool user = true)
    {
        this.Message = string.IsNullOrEmpty(message) ? this.Description : message;
        this.Title = "Variable Already Exists" + (user ? " (User)" : " (System)");
    }
}

public class InvalidRangeError : ErrorTypes
{
    public override string Title { get; }
    public sealed override string Description => "The provided range is invalid";
    public override string Message { get; }
    public override string Code => "Aurora.InvalidRange";
    public override bool AlwaysError => false;

    public InvalidRangeError(string? message = null, bool user = true)
    {
        this.Message = string.IsNullOrEmpty(message) ? this.Description : message;
        this.Title = "Invalid Range Error" + (user ? " (User)" : " (System)");
    }
}

public class ImmutableVarModificationError : ErrorTypes
{
    public override string Title { get; }
    public sealed override string Description => "Attempted to modify a constant (immutable) variable";
    public override string Message { get; }
    public override string Code => "Aurora.ImmutableVarModification";
    public override bool AlwaysError => true;

    public ImmutableVarModificationError(string? message = null, bool user = true)
    {
        this.Message = string.IsNullOrEmpty(message) ? this.Description : message;
        this.Title = "Immutable Variable Modification" + (user ? " (User)" : " (System)");
    }
}

public class InvalidVarTypeError : ErrorTypes
{
    public override string Title { get; }
    public sealed override string Description => "The provided variable type is invalid or not recognised";
    public override string Message { get; }
    public override string Code => "Aurora.InvalidVarType";
    public override bool AlwaysError => true;

    public InvalidVarTypeError(string? message = null, bool user = true)
    {
        this.Message = string.IsNullOrEmpty(message) ? this.Description : message;
        this.Title = "Invalid Variable Type" + (user ? " (User)" : " (System)");
    }
}

public class VarScopeViolationError : ErrorTypes
{
    public override string Title { get; }
    public sealed override string Description => "Access to a variable outside its defined scope";
    public override string Message { get; }
    public override string Code => "Aurora.VarScopeViolation";
    public override bool AlwaysError => true;

    public VarScopeViolationError(string? message = null, bool user = true)
    {
        this.Message = string.IsNullOrEmpty(message) ? this.Description : message;
        this.Title = "Variable Scope Violation" + (user ? " (User)" : " (System)");
    }
}

public class TypeMismatchError : ErrorTypes
{
    public override string Title { get; }
    public sealed override string Description => "Operation cannot be performed due to incompatible data types";
    public override string Message { get; }
    public override string Code => "Aurora.TypeMismatch";

    public TypeMismatchError(string? message = null, bool user = true, bool alwaysError = false)
    {
        this.Message = string.IsNullOrEmpty(message) ? this.Description : message;
        this.Title = "Type Mismatch" + (user ? " (User)" : " (System)");
    }
}

public class UnsupportedOperationError : ErrorTypes
{
    public override string Title { get; }
    public sealed override string Description => "Operation cannot be performed due to incompatible data types";
    public override string Message { get; }
    public override string Code => "Aurora.UnsupportedOperation";
    public override bool AlwaysError => true;

    public UnsupportedOperationError(string? message = null, bool user = true)
    {
        this.Message = string.IsNullOrEmpty(message) ? this.Description : message;
        this.Title = "Unsupported Operation" + (user ? " (User)" : " (System)");
    }
}

public class OutOfRangeError : ErrorTypes
{
    public override string Title { get; }
    public sealed override string Description => "A value is outside the allowed range";
    public override string Message { get; }
    public override string Code => "Aurora.OutOfRange";
    public override bool AlwaysError => true;

    public OutOfRangeError(string? message = null, bool user = true)
    {
        this.Message = string.IsNullOrEmpty(message) ? this.Description : message;
        this.Title = "Out of Range" + (user ? " (User)" : " (System)");
    }
}

public class DivisionByZeroError : ErrorTypes
{
    public override string Title { get; }
    public sealed override string Description => "Attempted to divide by zero";
    public override string Message { get; }
    public override string Code => "Aurora.DivisionByZero";
    public override bool AlwaysError => true;

    public DivisionByZeroError(string? message = null, bool user = true)
    {
        this.Message = string.IsNullOrEmpty(message) ? this.Description : message;
        this.Title = "Division by Zero" + (user ? " (User)" : " (System)");
    }
}

public class UnexpectedTokenError : ErrorTypes
{
    public override string Title { get; }
    public sealed override string Description => "An unexpected token was encountered during parsing";
    public override string Message { get; }
    public override string Code => "Aurora.UnexpectedToken";
    public override bool AlwaysError => true;

    public UnexpectedTokenError(string? message = null, bool user = true)
    {
        this.Message = string.IsNullOrEmpty(message) ? this.Description : message;
        this.Title = "Unexpected Token" + (user ? " (User)" : " (System)");
    }
}

public class InvalidSyntaxError : ErrorTypes
{
    public override string Title { get; }
    public sealed override string Description => "The syntax of this statement is invalid";
    public override string Message { get; }
    public override string Code => "Aurora.InvalidSyntax";
    public override bool AlwaysError => true;

    public InvalidSyntaxError(string? message = null, bool user = true)
    {
        this.Message = string.IsNullOrEmpty(message) ? this.Description : message;
        this.Title = "Invalid Syntax" + (user ? " (User)" : " (System)");
    }
}

public class UnclosedDelimiterError : ErrorTypes
{
    public override string Title { get; }
    public sealed override string Description => "A delimiter (e.g. Parenthesis, brackets) was not closed";
    public override string Message { get; }
    public override string Code => "Aurora.UnclosedDelimiter";
    public override bool AlwaysError => true;

    public UnclosedDelimiterError(string? message = null, bool user = true)
    {
        this.Message = string.IsNullOrEmpty(message) ? this.Description : message;
        this.Title = "Unclosed Delimiter" + (user ? " (User)" : " (System)");
    }
}

public class MissingSeparatorError : ErrorTypes
{
    public override string Title { get; }
    public sealed override string Description => "A required separator (e.g. ';', '.') is missing";
    public override string Message { get; }
    public override string Code => "Aurora.MissingSeparator";
    public override bool AlwaysError => true;

    public MissingSeparatorError(string? message = null, bool user = true)
    {
        this.Message = string.IsNullOrEmpty(message) ? this.Description : message;
        this.Title = "Missing Separator" + (user ? " (User)" : " (System)");
    }
}

public class UnreachableCodeError : ErrorTypes
{
    public override string Title { get; }
    public sealed override string Description => "Code exists after a return, halt, or exit point";
    public override string Message { get; }
    public override string Code => "Aurora.UnreachableCode";

    public UnreachableCodeError(string? message = null, bool user = true)
    {
        this.Message = string.IsNullOrEmpty(message) ? this.Description : message;
        this.Title = "Unreachable Code" + (user ? " (User)" : " (System)");
    }
}

public class InvalidReturnTypeError : ErrorTypes
{
    public override string Title { get; }
    public sealed override string Description => "Returned value does not match the declared return type";
    public override string Message { get; }
    public override string Code => "Aurora.InvalidReturnType";
    public override bool AlwaysError => true;

    public InvalidReturnTypeError(string? message = null, bool user = true)
    {
        this.Message = string.IsNullOrEmpty(message) ? this.Description : message;
        this.Title = "Invalid ReturnType" + (user ? " (User)" : " (System)");
    }
}

public class ModuleNotFoundError : ErrorTypes
{
    public override string Title { get; }
    public sealed override string Description => "The specified module could not be found";
    public override string Message { get; }
    public override string Code => "Aurora.ModuleNotFound";
    public override bool AlwaysError => true;

    public ModuleNotFoundError(string? message = null, bool user = true)
    {
        this.Message = string.IsNullOrEmpty(message) ? this.Description : message;
        this.Title = "Module Not Found" + (user ? " (User)" : " (System)");
    }
}

public class InvalidMethodError : ErrorTypes
{
    public override string Title { get; }
    public sealed override string Description => "The provided method could not be found";
    public override string Message { get; }
    public override string Code => "Aurora.InvalidMethod";
    public override bool AlwaysError => true;

    public InvalidMethodError(string? message = null, bool user = true)
    {
        this.Message = string.IsNullOrEmpty(message) ? this.Description : message;
        this.Title = "Invalid Callable" + (user ? " (User)" : " (System)");
    }
}

public class FileNotFoundError : ErrorTypes
{
    public override string Title { get; }
    public sealed override string Description => "The specified file could not be found";
    public override string Message { get; }
    public override string Code => "Aurora.FileNotFound";
    public override bool AlwaysError => true;

    public FileNotFoundError(string? message = null, bool user = true)
    {
        this.Message = string.IsNullOrEmpty(message) ? this.Description : message;
        this.Title = "File Not Found" + (user ? " (User)" : " (System)");
    }
}

public class InvalidAttributeError : ErrorTypes
{
    public override string Title { get; }
    public sealed override string Description => "The provided attribute could not be found";
    public override string Message { get; }
    public override string Code => "Aurora.InvalidAttribute";
    public override bool AlwaysError => true;

    public InvalidAttributeError(string? message = null, bool user = true)
    {
        this.Message = string.IsNullOrEmpty(message) ? this.Description : message;
        this.Title = "Invalid Attribute" + (user ? " (User)" : " (System)");
    }
}

public class InvalidMemberAccessError : ErrorTypes
{
    public override string Title { get; }
    public sealed override string Description => "Attempted to access an undefined or restricted class member";
    public override string Message { get; }
    public override string Code => "Aurora.InvalidMemberAccess";
    public override bool AlwaysError => true;

    public InvalidMemberAccessError(string? message = null, bool user = true)
    {
        this.Message = string.IsNullOrEmpty(message) ? this.Description : message;
        this.Title = "Invalid Member Access" + (user ? " (User)" : " (System)");
    }
}

public class ConstantRedefinitionError : ErrorTypes
{
    public override string Title { get; }
    public sealed override string Description => "A constant cannot be redefined after initial assignment";
    public override string Message { get; }
    public override string Code => "Aurora.ConstantRedefinition";
    public override bool AlwaysError => true;

    public ConstantRedefinitionError(string? message = null, bool user = true)
    {
        this.Message = string.IsNullOrEmpty(message) ? this.Description : message;
        this.Title = "Constant Redefinition" + (user ? " (User)" : " (System)");
    }
}

public class ConfigurationError : ErrorTypes
{
    public override string Title { get; }
    public sealed override string Description => "A problem occured while loading or interpreting the configuration";
    public override string Message { get; }
    public override string Code => "Aurora.Configuration";
    public override bool AlwaysError => true;

    public ConfigurationError(string? message = null, bool user = true)
    {
        this.Message = string.IsNullOrEmpty(message) ? this.Description : message;
        this.Title = "Configuration Error" + (user ? " (User)" : " (System)");
    }
}

public class MaxExpressionDepthExceededError : ErrorTypes
{
    public override string Title { get; }
    public sealed override string Description => "The maximum number of expressions per line was exceeded";
    public override string Message { get; }
    public override string Code => "Aurora.ExpressionDepthExceeded";
    public override bool AlwaysError => true;

    public MaxExpressionDepthExceededError(string? message = null, bool user = true)
    {
        this.Message = string.IsNullOrEmpty(message) ? this.Description : message;
        this.Title = "Expression Depth Exceeded" + (user ? " (User)" : " (System)");
    }
}

public class MaxRecursionDepthExceededError : ErrorTypes
{
    public override string Title { get; }
    public sealed override string Description => "The system encountered its maximum recursion depth.";
    public override string Message { get; }
    public override string Code => "Aurora.RecursionDepthExceeded";
    public override bool AlwaysError => true;

    public MaxRecursionDepthExceededError(string? message = null, bool user = true)
    {
        this.Message = string.IsNullOrEmpty(message) ? this.Description : message;
        this.Title = "Recursion Depth Exceeded" + (user ? " (User)" : " (System)");
    }
}

public class MemoryError : ErrorTypes
{
    public override string Title { get; }
    public sealed override string Description => "The system tried to use or access invalid memory.";
    public override string Message { get; }
    public override string Code => "Aurora.MemoryError";
    public override bool AlwaysError => true;

    public MemoryError(string? message = null, bool user = false)
    {
        this.Message = string.IsNullOrEmpty(message) ? this.Description : message;
        this.Title = "Memory Error" + (user ? " (User)" : " (System)");
    }
}

public class EofError : ErrorTypes
{
    public override string Title { get; }
    public sealed override string Description => "End of file was found before an expression finished.";
    public override string Message { get; }
    public override string Code => "Aurora.EofError";
    public override bool AlwaysError => true;

    public EofError(string? message = null, bool user = false)
    {
        this.Message = string.IsNullOrEmpty(message) ? this.Description : message;
        this.Title = "Eof Error" + (user ? " (User)" : " (System)");
    }
}

public class SystemError : ErrorTypes
{
    public override string Title => "[SYSTEM]";
    public sealed override string Description => "The system encountered a problem it could not handle";
    public override string Message { get; }
    public override string Code => "Aurora.System";
    public override bool AlwaysError => true;

    public SystemError(string? message = null)
    {
        this.Message = string.IsNullOrEmpty(message) ? this.Description : this.Description + "\n" + message;
    }
}
