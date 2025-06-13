using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Aurora
{
    internal class Errors
    {
        public static string ConfigFilePath { get; set; } = "AuroraConfig.json";

        [DoesNotReturn]
        public static T AlwaysThrow<T>(ErrorTypes error)
        {
            AlwaysThrow(error);
            throw new UnreachableException();
        }
        
        [DoesNotReturn]
        public static void AlwaysThrow(ErrorTypes error)
        {
            RaiseError(error, alwaysThrow: true);
            Environment.Exit(1);
            throw new UnreachableException();
        }
        
        public static void RaiseError(ErrorTypes error, bool alwaysThrow = false)
        {
            string outputMessage = GlobalVariables.LineNumber is not null
                ? $"{{Line {GlobalVariables.LineNumber}}} ({error.Code}) {error.Title} - {error.Message}"
                : $"{{Unknown line}} ({error.Code}) {error.Title} - {error.Message}";
            
            bool isError = error.AlwaysError || UserConfiguration.Errors.Contains(error.Code) || alwaysThrow;

            if (!isError)
            {
                GlobalVariables.LOGGER.Warning(outputMessage);
                return;
            }

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("[ERROR] " + outputMessage);
            Console.ResetColor();
            
            Environment.Exit(1);
        }

        public static void Log(string title, string message)
        {
            using var writer = File.AppendText(GlobalVariables.LOGGER.LogFilePath);
            writer.WriteLine($"Custom Log: {title} - {message}");
        }
    }

    internal abstract class ErrorTypes
    {
        public abstract string Title { get; }
        public abstract string Description { get; }
        public abstract string Message { get; }
        public abstract string Code { get;  }
        public virtual bool AlwaysError => false;
    }

    internal class ArgumentSurplusError : ErrorTypes
    {
        public override string Title { get; }

        public override string Description => "Too many arguments were provided for this method or operation";
        public override string Message { get; }

        public override string Code => "Aurora.ArgSurplus";

        public ArgumentSurplusError(string? message = null, bool user = true)
        {
            this.Message = string.IsNullOrEmpty(message) ? this.Description : message;
            this.Title = "Argument Surplus" + (user ? " (User)" : " (System)");
        }
    }

    internal class ArgumentDeficitError : ErrorTypes
    {
        public override string Title { get; }
        public override string Description => "Not enough arguments were provided for this method or operation";
        public override string Message { get; }
        public override string Code => "Aurora.ArgDeficit";

        public override bool AlwaysError => true;

        public ArgumentDeficitError(string? message = null, bool user = true)
        {
            this.Message = string.IsNullOrEmpty(message) ? this.Description : message;
            this.Title = "Argument Deficit" + (user ? " (User)" : " (System)");
        }
    }

    internal class ArgumentTypeMismatchError : ErrorTypes
    {
        public override string Title { get; }

        public override string Description =>
            "An argument was provided with a type that does not match the expected type";

        public override string Message { get; }

        public override string Code => "Aurora.ArgTypeMismatch";

        public ArgumentTypeMismatchError(string? message = null, bool user = true)
        {
            this.Message = string.IsNullOrEmpty(message) ? this.Description : message;
            this.Title = "Argument Type Mismatch" + (user ? " (User)" : " (System)");
        }
    }

    internal class MissingRequiredArgError : ErrorTypes
    {
        public override string Title { get; }
        public override string Description => "A required argument is missing from this method call";
        public override string Message { get; }
        public override string Code => "Aurora.MissingRequiredArg";
        public override bool AlwaysError => true;

        public MissingRequiredArgError(string? message = null, bool user = true)
        {
            this.Message = string.IsNullOrEmpty(message) ? this.Description : message;
            this.Title = "Missing Required Argument" + (user ? " (User)" : " (System)");
        }
    }

    internal class UnexpectedKeywordArgError : ErrorTypes
    {
        public override string Title { get; }
        public override string Description => "An unexpected keyword argument was supplied";
        public override string Message { get; }
        public override string Code => "Aurora.UnexpectedKeywordArg";

        public UnexpectedKeywordArgError(string? message = null, bool user = true)
        {
            this.Message = string.IsNullOrEmpty(message) ? this.Description : message;
            this.Title = "Unexpected Keyword Argument" + (user ? " (User)" : " (System)");
        }
    }

    internal class InvalidArgNameError : ErrorTypes
    {
        public override string Title { get; }
        public override string Description => "The provided argument name is invalid or reserved";
        public override string Message { get; }
        public override string Code => "Aurora.InvalidArgName";

        public InvalidArgNameError(string? message = null, bool user = true)
        {
            this.Message = string.IsNullOrEmpty(message) ? this.Description : message;
            this.Title = "Invalid Argument Name" + (user ? " (User)" : " (System)");
        }
    }

    internal class VarNotDefinedError : ErrorTypes
    {
        public override string Title { get; }
        public override string Description => "Attempted to access a variable that has not been defined";
        public override string Message { get; }
        public override string Code => "Aurora.VarNotDefined";

        public VarNotDefinedError(string? message = null, bool user = true)
        {
            this.Message = string.IsNullOrEmpty(message) ? this.Description : message;
            this.Title = "Variable Not Defined" + (user ? " (User)" : " (System)");
        }
    }

    internal class VarAlreadyExistsError : ErrorTypes
    {
        public override string Title { get; }
        public override string Description => "A variable with the same name already exists";
        public override string Message { get; }
        public override string Code => "Aurora.VarAlreadyExists";

        public VarAlreadyExistsError(string? message = null, bool user = true)
        {
            this.Message = string.IsNullOrEmpty(message) ? this.Description : message;
            this.Title = "Variable Already Exists" + (user ? " (User)" : " (System)");
        }
    }

    internal class ImmutableVarModificationError : ErrorTypes
    {
        public override string Title { get; }
        public override string Description => "Attempted to modify a constant (immutable) variable";
        public override string Message { get; }
        public override string Code => "Aurora.ImmutableVarModification";
        public override bool AlwaysError => true;

        public ImmutableVarModificationError(string? message = null, bool user = true)
        {
            this.Message = string.IsNullOrEmpty(message) ? this.Description : message;
            this.Title = "Immutable Variable Modification" + (user ? " (User)" : " (System)");
        }
    }

    internal class InvalidVarTypeError : ErrorTypes
    {
        public override string Title { get; }
        public override string Description => "The provided variable type is invalid or not recognised";
        public override string Message { get; }
        public override string Code => "Aurora.InvalidVarType";
        public override bool AlwaysError => true;

        public InvalidVarTypeError(string? message = null, bool user = true)
        {
            this.Message = string.IsNullOrEmpty(message) ? this.Description : message;
            this.Title = "Invalid Variable Type" + (user ? " (User)" : " (System)");
        }
    }

    internal class VarScopeViolationError : ErrorTypes
    {
        public override string Title { get; }
        public override string Description => "Access to a variable outside its defined scope";
        public override string Message { get; }
        public override string Code => "Aurora.VarScopeViolation";
        public override bool AlwaysError => true;

        public VarScopeViolationError(string? message = null, bool user = true)
        {
            this.Message = string.IsNullOrEmpty(message) ? this.Description : message;
            this.Title = "Variable Scope Violation" + (user ? " (User)" : " (System)");
        }
    }

    internal class TypeMismatchError : ErrorTypes
    {
        public override string Title { get; }
        public override string Description => "Operation cannot be performed due to incompatible data types";
        public override string Message { get; }
        public override string Code => "Aurora.TypeMismatch";

        public TypeMismatchError(string? message = null, bool user = true, bool alwaysError = false)
        {
            this.Message = string.IsNullOrEmpty(message) ? this.Description : message;
            this.Title = "Type Mismatch" + (user ? " (User)" : " (System)");
        }
    }

    internal class UnsupportedOperationError : ErrorTypes
    {
        public override string Title { get; }
        public override string Description => "Operation cannot be performed due to incompatible data types";
        public override string Message { get; }
        public override string Code => "Aurora.UnsupportedOperation";
        public override bool AlwaysError => true;

        public UnsupportedOperationError(string? message = null, bool user = true)
        {
            this.Message = string.IsNullOrEmpty(message) ? this.Description : message;
            this.Title = "Unsupported Operation" + (user ? " (User)" : " (System)");
        }
    }

    internal class OutOfRangeError : ErrorTypes
    {
        public override string Title { get; }
        public override string Description => "A value is outside the allowed range";
        public override string Message { get; }
        public override string Code => "Aurora.OutOfRange";
        public override bool AlwaysError => true;

        public OutOfRangeError(string? message = null, bool user = true)
        {
            this.Message = string.IsNullOrEmpty(message) ? this.Description : message;
            this.Title = "Out of Range" + (user ? " (User)" : " (System)");
        }
    }

    internal class DivisionByZeroError : ErrorTypes
    {
        public override string Title { get; }
        public override string Description => "Attempted to divide by zero";
        public override string Message { get; }
        public override string Code => "Aurora.DivisionByZero";
        public override bool AlwaysError => true;

        public DivisionByZeroError(string? message = null, bool user = true)
        {
            this.Message = string.IsNullOrEmpty(message) ? this.Description : message;
            this.Title = "Division by Zero" + (user ? " (User)" : " (System)");
        }
    }

    internal class UnexpectedTokenError : ErrorTypes
    {
        public override string Title { get; }
        public override string Description => "An unexpected token was encountered during parsing";
        public override string Message { get; }
        public override string Code => "Aurora.UnexpectedToken";
        public override bool AlwaysError => true;

        public UnexpectedTokenError(string? message = null, bool user = true)
        {
            this.Message = string.IsNullOrEmpty(message) ? this.Description : message;
            this.Title = "Unexpected Token" + (user ? " (User)" : " (System)");
        }
    }

    internal class InvalidSyntaxError : ErrorTypes
    {
        public override string Title { get; }
        public override string Description => "The syntax of this statement is invalid";
        public override string Message { get; }
        public override string Code => "Aurora.InvalidSyntax";
        public override bool AlwaysError => true;

        public InvalidSyntaxError(string? message = null, bool user = true)
        {
            this.Message = string.IsNullOrEmpty(message) ? this.Description : message;
            this.Title = "Invalid Syntax" + (user ? " (User)" : " (System)");
        }
    }

    internal class UnclosedDelimiterError : ErrorTypes
    {
        public override string Title { get; }
        public override string Description => "A delimiter (e.g. Parenthesis, brackets) was not closed";
        public override string Message { get; }
        public override string Code => "Aurora.UnclosedDelimiter";
        public override bool AlwaysError => true;

        public UnclosedDelimiterError(string? message = null, bool user = true)
        {
            this.Message = string.IsNullOrEmpty(message) ? this.Description : message;
            this.Title = "Unclosed Delimiter" + (user ? " (User)" : " (System)");
        }
    }

    internal class MissingSeparatorError : ErrorTypes
    {
        public override string Title { get; }
        public override string Description => "A required separator (e.g. ';', '.') is missing";
        public override string Message { get; }
        public override string Code => "Aurora.MissingSeparator";
        public override bool AlwaysError => true;

        public MissingSeparatorError(string? message = null, bool user = true)
        {
            this.Message = string.IsNullOrEmpty(message) ? this.Description : message;
            this.Title = "Missing Separator" + (user ? " (User)" : " (System)");
        }
    }

    internal class UnreachableCodeError : ErrorTypes
    {
        public override string Title { get; }
        public override string Description => "Code exists after a return, halt, or exit point";
        public override string Message { get; }
        public override string Code => "Aurora.UnreachableCode";

        public UnreachableCodeError(string? message = null, bool user = true)
        {
            this.Message = string.IsNullOrEmpty(message) ? this.Description : message;
            this.Title = "Unreachable Code" + (user ? " (User)" : " (System)");
        }
    }

    internal class InvalidReturnTypeError : ErrorTypes
    {
        public override string Title { get; }
        public override string Description => "Returned value does not match the declared return type";
        public override string Message { get; }
        public override string Code => "Aurora.InvalidReturnType";
        public override bool AlwaysError => true;

        public InvalidReturnTypeError(string? message = null, bool user = true)
        {
            this.Message = string.IsNullOrEmpty(message) ? this.Description : message;
            this.Title = "Invalid ReturnType" + (user ? " (User)" : " (System)");
        }
    }

    internal class ModuleNotFoundError : ErrorTypes
    {
        public override string Title { get; }
        public override string Description => "The specified module could not be found";
        public override string Message { get; }
        public override string Code => "Aurora.ModuleNotFound";
        public override bool AlwaysError => true;

        public ModuleNotFoundError(string? message = null, bool user = true)
        {
            this.Message = string.IsNullOrEmpty(message) ? this.Description : message;
            this.Title = "Module Not Found" + (user ? " (User)" : " (System)");
        }
    }

    internal class InvalidMethodError : ErrorTypes
    {
        public override string Title { get; }
        public override string Description => "The provided method could not be found";
        public override string Message { get; }
        public override string Code => "Aurora.InvalidMethod";
        public override bool AlwaysError => true;

        public InvalidMethodError(string? message = null, bool user = true)
        {
            this.Message = string.IsNullOrEmpty(message) ? this.Description : message;
            this.Title = "Invalid Method" + (user ? " (User)" : " (System)");
        }
    }

    internal class FileNotFoundError : ErrorTypes
    {
        public override string Title { get; }
        public override string Description => "The specified file could not be found";
        public override string Message { get; }
        public override string Code => "Aurora.FileNotFound";
        public override bool AlwaysError => true;

        public FileNotFoundError(string? message = null, bool user = true)
        {
            this.Message = string.IsNullOrEmpty(message) ? this.Description : message;
            this.Title = "File Not Found" + (user ? " (User)" : " (System)");
        }
    }
    
    internal class InvalidAttributeError : ErrorTypes
    {
        public override string Title { get; }
        public override string Description => "The provided attribute could not be found";
        public override string Message { get; }
        public override string Code => "Aurora.InvalidAttribute";
        public override bool AlwaysError => true;

        public InvalidAttributeError(string? message = null, bool user = true)
        {
            this.Message = string.IsNullOrEmpty(message) ? this.Description : message;
            this.Title = "Invalid Attribute" + (user ? " (User)" : " (System)");
        }
    }

    internal class InvalidMemberAccessError : ErrorTypes
    {
        public override string Title { get; }
        public override string Description => "Attempted to access an undefined or restricted class member";
        public override string Message { get; }
        public override string Code => "Aurora.InvalidMemberAccess";
        public override bool AlwaysError => true;

        public InvalidMemberAccessError(string? message = null, bool user = true)
        {
            this.Message = string.IsNullOrEmpty(message) ? this.Description : message;
            this.Title = "Invalid Member Access" + (user ? " (User)" : " (System)");
        }
    }

    internal class ConstantRedefinitionError : ErrorTypes
    {
        public override string Title { get; }
        public override string Description => "A constant cannot be redefined after initial assignment";
        public override string Message { get; }
        public override string Code => "Aurora.ConstantRedefinition";
        public override bool AlwaysError => true;

        public ConstantRedefinitionError(string? message = null, bool user = true)
        {
            this.Message = string.IsNullOrEmpty(message) ? this.Description : message;
            this.Title = "Constant Redefinition" + (user ? " (User)" : " (System)");
        }
    }

    internal class ConfigurationError : ErrorTypes
    {
        public override string Title { get; }
        public override string Description => "A problem occured while loading or interpreting the configuration";
        public override string Message { get; }
        public override string Code => "Aurora.Configuration";
        public override bool AlwaysError => true;

        public ConfigurationError(string? message = null, bool user = true)
        {
            this.Message = string.IsNullOrEmpty(message) ? this.Description : message;
            this.Title = "Configuration Error" + (user ? " (User)" : " (System)");
        }
    }

    internal class MaxExpressionDepthExceededError : ErrorTypes
    {
        public override string Title { get; }
        public override string Description => "The maximum number of expressions per line was exceeded";
        public override string Message { get; }
        public override string Code => "Aurora.ExpressionDepthExceeded";
        public override bool AlwaysError => true;

        public MaxExpressionDepthExceededError(string? message = null, bool user = true)
        {
            this.Message = string.IsNullOrEmpty(message) ? this.Description : message;
            this.Title = "Expression Depth Exceeded" + (user ? " (User)" : " (System)");
        }
    }

    internal class SystemError : ErrorTypes
    {
        public override string Title => "[SYSTEM]";
        public override string Description => "The system encountered a problem it could not handle";
        public override string Message { get; }
        public override string Code => "Aurora.System";
        public override bool AlwaysError => true;

        public SystemError(string? message = null)
        {
            this.Message = string.IsNullOrEmpty(message) ? this.Description : this.Description + "\n" + message;
        }
    }
}