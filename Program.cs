using System.Reflection;
using System.Security.AccessControl;
using System.Text.Json;
using CommandLine;

namespace Aurora 
{

    static class GlobalVariables
    {
        public static Logs logger = new();
        public static Errors errors = new();
        public static string codeFilePath = "";
        public static int? lineNumber = null;

        public static Tuple<char, char> stringStartChars = new ('"', '\'');

        public static string? ReprString(string? value)
        {
            if (string.IsNullOrEmpty(value)) { return value; }

            char quote = value.Contains('\'') ? '"': '\'';
            return $"{quote}{value}{quote}";
        }
    }

    class Program
    {
        readonly Logs logger = GlobalVariables.logger;

        string[] ReadCode(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                Errors.RaiseError("No File Path", "Please provide a file path to execute");
            }
            if (!File.Exists(filePath))
            {
                Errors.RaiseError("File Not Found", $"The file - {filePath} - was not found");
            }
            if (!filePath.EndsWith(".aur"))
            {
                logger.Warning("Aurora code should be written in an aurora file (ending with .aur).");
            }
            return File.ReadAllLines(filePath);
        }

        void RunOptionsAndReturnExitCode(Options opts)
        {
            GlobalVariables.codeFilePath = opts.FilePath;

            if (opts.NoConsole)
            {
                logger.noConsole = true;
            }

            if (!string.IsNullOrEmpty(opts.LogFile))
            {
                logger.logFilePath = opts.LogFile;
            }

            if (opts.Debug)
            {
                logger.allowDebug = true;
                logger.Debug("Debug messages enabled");

                opts.Verbose = !opts.Strict || opts.Verbose;
            }

            if (opts.Verbose)
            {
                logger.allowVerbose = true;
                logger.Verbose("Verbose messages enabled");

                opts.Warning = !opts.Strict || opts.Warning;
            }

            if (opts.Warning)
            {
                logger.allowWarning = true;
                logger.Warning("Warning messages enabled");
            }
        }

        void HandleParseError(IEnumerable<Error> errs)
        {
            if (errs.IsHelp() || errs.IsVersion())
            {
                Environment.Exit(0);
            }
            
            foreach (Error err in errs)
            {
                logger.ForceLog($"System Error: {err}");
            }
            Errors.RaiseError("System Error", "The system encountered an error it could not handle");
        }

        void Run(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
                .WithParsed<Options>(opts => RunOptionsAndReturnExitCode(opts))
                .WithNotParsed<Options>((errs) => HandleParseError(errs));
            

            string[] code = ReadCode(GlobalVariables.codeFilePath);
            Console.WriteLine("Done");
        }

        void EvaluateCode(string[] code)
        {
            for (int index = 0; index < code.Length; index++)
            {
                string line = code[index];
                GlobalVariables.lineNumber = index+1;

                int decimalLocation = line.IndexOf('.');

                if (decimalLocation == -1)
                {
                    Errors.RaiseError(
                        "Invalid format",
                        $"No valid task was found for line {GlobalVariables.lineNumber} in {GlobalVariables.codeFilePath}"
                    );
                }
            }
        }

        public static void Main(string[] args)
        {
            var tokens = new List<Token> ([
                new IntegerToken(6),
                new IntegerToken(3),
                new IntegerToken(7),
                new FloatToken((float)3.14),
                new StringToken("\"Hello\"")
            ]);

            var sequence = new List<(List<Type>, int)> ([
                ([typeof(IntegerToken)], -1),
                ([typeof(FloatToken)], 1),
                ([typeof(StringToken)], 1)
            ]);

            var sequenceTwo = new List<(List<Type>, int)> ([
                ([typeof(IntegerToken)], -1),
                ([typeof(FloatToken)], -1),
                ([typeof(StringToken)], 1),
                ([typeof(WordToken)], 1)
            ]);

            var sequenceThree = new List<(List<Type>, int)> ([
                ([typeof(BaseToken)], -1),
                ([typeof(FloatToken)], 1),
                ([typeof(StringToken)], 1)
            ]);
            var sequenceFour = new List<(List<Type>, int)> ([
                ([typeof(BaseToken)], -1),
                ([typeof(WordToken)], 0),
                ([typeof(FloatToken)], 1),
                ([typeof(StringToken)], 1)
            ]);

            Console.WriteLine(Match.MatchTokens(tokens, sequence));
            Console.WriteLine(Match.MatchTokens(tokens, sequenceTwo));
            Console.WriteLine(Match.MatchTokens(tokens, sequenceThree));
            Console.WriteLine(Match.MatchTokens(tokens, sequenceFour));

            Environment.Exit(0);

            Program program = new();
            program.Run(args);
        }
    }
}