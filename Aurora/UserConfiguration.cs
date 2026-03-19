// using System.Text.Json;
//
// namespace Aurora;
//
// /// <summary>
// /// The user configured settings, stored statically.
// /// </summary>
// internal static class UserConfiguration
// {
//     public static int MaxExpressionDepth = 250;
//     public static List<string> Errors = [];
//     public static List<string> Warnings = [];
//     public static List<string> Ignore = [];
//
//     /// <summary>
//     /// Applies the configuration present in the file path provided; This is usually a JSON file
//     /// </summary>
//     /// <param name="filePath">The file containing the user configuration in a JSON object</param>
//     public static void ApplyConfiguration(string? filePath = null)
//     {
//         InternalVariables.LOGGER.Verbose($"Applying configuration from Path: {filePath}");
//         filePath = string.IsNullOrEmpty(filePath) ? InternalVariables.ConfigFilePath : filePath;
//         Dictionary<string, object> config = Json.ReadDict<object>(filePath);
//
//         object maxExpressionDepth = config.GetValueOrDefault("MaxExpressionDepth", MaxExpressionDepth);
//         object errors = config.GetValueOrDefault("Errors", Errors);
//         object warnings = config.GetValueOrDefault("Warnings", Warnings);
//         object ignore = config.GetValueOrDefault("Ignore", Ignore);
//         object showTimestamp = config.GetValueOrDefault("ShowTimestamp", InternalVariables.LOGGER.ShowTimestamp);
//         object clearLogFile = config.GetValueOrDefault("ClearLogFile", InternalVariables.LOGGER.ClearFile);
//         object debug = config.GetValueOrDefault("Debug", InternalVariables.LOGGER.AllowDebug);
//         object verbose = config.GetValueOrDefault("Verbose", InternalVariables.LOGGER.AllowVerbose);
//         object warning = config.GetValueOrDefault("Warning", InternalVariables.LOGGER.AllowWarning);
//         object noConsole = config.GetValueOrDefault("NoConsole", InternalVariables.LOGGER.NoConsole);
//         object strict = config.GetValueOrDefault("Strict", InternalVariables.StrictFlagMode);
//         object inlineStackTrace = config.GetValueOrDefault("InlineStackTrace", InternalVariables.InlineStackTrace);
//         object disableEasterEggs = config.GetValueOrDefault("DisableEasterEggs", !InternalVariables.EasterEggs);
//         object logFile = config.GetValueOrDefault("LogFile", "");
//
//         if (maxExpressionDepth is not int)
//             Aurora.Errors.RaiseError(new ConfigurationError("MaxExpressionDepth must be an integer"));
//
//         if (showTimestamp is JsonElement showTimestampElement)
//             showTimestamp = showTimestampElement.GetBoolean();
//
//         if (clearLogFile is JsonElement clearLogFileElement)
//             clearLogFile = clearLogFileElement.GetBoolean();
//
//         if (debug is JsonElement debugElement)
//             debug = debugElement.GetBoolean();
//
//         if (verbose is JsonElement verboseElement)
//             verbose = verboseElement.GetBoolean();
//
//         if (warning is JsonElement warningElement)
//             warning = warningElement.GetBoolean();
//
//         if (noConsole is JsonElement noConsoleElement)
//             noConsole = noConsoleElement.GetBoolean();
//
//         if (strict is JsonElement strictElement)
//             strict = strictElement.GetBoolean();
//
//         if (inlineStackTrace is JsonElement inlineStackTraceElement)
//             inlineStackTrace = inlineStackTraceElement.GetBoolean();
//
//         if (disableEasterEggs is JsonElement disableEasterEggsElement)
//             disableEasterEggs = disableEasterEggsElement.GetBoolean();
//
//         if (logFile is JsonElement logFileElement)
//             logFile = logFileElement.GetString() ?? "";
//
//         if (showTimestamp is not bool)
//             Aurora.Errors.RaiseError(new ConfigurationError("ShowTimestamp must be a boolean"));
//
//         if (clearLogFile is not bool)
//             Aurora.Errors.RaiseError(new ConfigurationError("ClearLogFile must be a boolean"));
//
//         if (debug is not bool)
//             Aurora.Errors.RaiseError(new ConfigurationError("Debug must be a boolean"));
//
//         if (verbose is not bool)
//             Aurora.Errors.RaiseError(new ConfigurationError("Verbose must be a boolean"));
//
//         if (warning is not bool)
//             Aurora.Errors.RaiseError(new ConfigurationError("Warning must be a boolean"));
//
//         if (noConsole is not bool)
//             Aurora.Errors.RaiseError(new ConfigurationError("NoConsole must be a boolean"));
//
//         if (strict is not bool)
//             Aurora.Errors.RaiseError(new ConfigurationError("StrictFlags must be a boolean"));
//
//         if (inlineStackTrace is not bool)
//             Aurora.Errors.RaiseError(new ConfigurationError("InlineStackTrace must be a boolean"));
//
//         if (disableEasterEggs is not bool)
//             Aurora.Errors.RaiseError(new ConfigurationError("DisableEasterEggs must be a boolean"));
//
//         if (logFile is not string)
//             Aurora.Errors.RaiseError(new ConfigurationError("LogFile must be a string"));
//
//         if ((errors is not List<object> errorList || errorList.All(item => item is string)) &&
//             errors is not List<string>)
//             Aurora.Errors.RaiseError(new ConfigurationError("Errors must be a list of strings"));
//
//         if ((warnings is not List<object> warningList || warningList.All(item => item is string)) &&
//             warnings is not List<string>)
//             Aurora.Errors.RaiseError(new ConfigurationError("Warnings must be a list of strings"));
//
//         if ((ignore is not List<object> ignoreList || ignoreList.All(item => item is string)) &&
//             ignore is not List<string>)
//             Aurora.Errors.RaiseError(new ConfigurationError("Ignore must be a list of strings"));
//
//         MaxExpressionDepth = (int)maxExpressionDepth;
//         Errors = (List<string>)errors;
//         Warnings = (List<string>)warnings;
//         Ignore = (List<string>)ignore;
//
//         InternalVariables.LOGGER.ShowTimestamp = (bool)showTimestamp;
//         InternalVariables.LOGGER.ClearFile = (bool)clearLogFile;
//
//         bool debugBool = InternalVariables.LOGGER.AllowDebug || (bool)debug;
//         bool verboseBool = InternalVariables.LOGGER.AllowVerbose || (bool)verbose;
//         bool warningBool = InternalVariables.LOGGER.AllowWarning || (bool)warning;
//         bool noConsoleBool = InternalVariables.LOGGER.NoConsole || (bool)noConsole;
//         bool strictBool = InternalVariables.StrictFlagMode || (bool)strict;
//         bool inlineStackTraceBool = InternalVariables.InlineStackTrace || (bool)inlineStackTrace;
//         bool disableEasterEggsBool = !InternalVariables.EasterEggs || (bool)disableEasterEggs;
//         string logFileString = (string)logFile;
//
//         Program.ApplyOptions(noConsoleBool, debugBool, verboseBool, warningBool, strictBool, inlineStackTraceBool,
//             disableEasterEggsBool, logFileString);
//     }
// }

