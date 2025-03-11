using System.ComponentModel;
using System.Text.RegularExpressions;

namespace Aurora
{
    class Terminal
    {
        public static void Clear()
        {
            Console.Clear();
        }

        // public static void Eval(List<Token> line)
        // {
        //     List<(object, int)> expectedFormat = [
        //         (typeof(WordToken), 1),
        //         (typeof(SeparatorToken), 1),
        //         (typeof(WordToken), 1),
        //         (typeof(BracketToken), 1),
        //         (typeof(BaseToken), -1),
        //         (typeof(BracketToken), 1)
        //     ];
        //     if (!Match.MatchTokens(line, expectedFormat, ignoreSequenceOverflow:true))
        //     {
        //         Errors.RaiseError("Syntax Error (system)", "Call to terminal is invalid");
        //     }

        //     if (line[^1].Type != BracketToken.TOKEN_TYPE || (line[^1].IsNormal && line[^1].IsClosed))
        //     {
        //         Errors.RaiseError("Syntax error", "Missing closing bracket in terminal call");
        //     }
        // }

    }

    class Colors
    {
        public const string BLACK = "\x1b[30m";
        public const string RED = "\x1b[31m";
        public const string GREEN = "\x1b[32m";
        public const string YELLOW = "\x1b[33m";
        public const string BLUE = "\x1b[34m";
        public const string MAGENTA = "\x1b[35m";
        public const string CYAN = "\x1b[36m";
        public const string WHITE = "\x1b[37m";

        public const string BLACK_BG = "\x1b[40m";
        public const string RED_BG = "\x1b[41m";
        public const string GREEN_BG = "\x1b[42m";
        public const string YELLOW_BG = "\x1b[43m";
        public const string BLUE_BG = "\x1b[44m";
        public const string MAGENTA_BG = "\x1b[45m";
        public const string CYAN_BG = "\x1b[46m";
        public const string WHITE_BG = "\x1b[47m";

        public static string? Get(string item)
        {
            switch (item)
            {
                case "BLACK":
                    return BLACK;
                case "RED":
                    return RED;
                case "GREEN":
                    return GREEN;
                case "YELLOW":
                    return YELLOW;
                case "BLUE":
                    return BLUE;
                case "MAGENTA":
                    return MAGENTA;
                case "CYAN":
                    return CYAN;
                case "WHITE":
                    return WHITE;

                case "BLACK_BG":
                    return BLACK_BG;
                case "RED_BG":
                    return RED_BG;
                case "GREEN_BG":
                    return GREEN_BG;
                case "YELLOW_BG":
                    return YELLOW_BG;
                case "BLUE_BG":
                    return BLUE_BG;
                case "MAGENTA_BG":
                    return MAGENTA_BG;
                case "CYAN_BG":
                    return CYAN_BG;
                case "WHITE_BG":
                    return WHITE_BG;
                
                default:
                    return null;
            }
        }

        public static string RGB(int red, int green, int blue, bool background=false)
        {
            int colorMode = background ? 4 : 3;

            if (
                red < 0 || red > 255
                || green < 0 || green > 255
                || blue < 0 || blue > 255
            )
            {
                throw new InvalidDataException("RGB values must be between 0 and 255");
            }

            return $"\x1b[{colorMode}8;2;{red};{green};{blue}m";
        }

        public static string Hex(string hex, bool background=false)
        {
            if (!hex.StartsWith('#'))
            {
                throw new FormatException("Hexadecimal values must start with a hashtag ('#')");
            }

            int red = Convert.ToInt32(hex.Substring(1, 2), 16);
            int green = Convert.ToInt32(hex.Substring(3, 2), 16);
            int blue = Convert.ToInt32(hex.Substring(5, 2), 16);

            return RGB(red, green, blue, background);
        }

        public static string Eval(string line)
        {
            if (!(line.StartsWith("Colors.bg") || line.StartsWith("Colors.fg")))
            {
                Errors.RaiseError("Invalid call", "Colors call must specify 'fg' or 'bg'");
            }

            bool background = line.StartsWith("Colors.bg");

            string rgbRegex = @"Colors\.[fb]g\.RGB\((?<args>.*?)\);?";
            string hexRegex = @"Colors\.[fb]g\.hex\((?<args>.*?)\);?";
            string normalRegex = @"Colors\.[fb]g\.(?<colorName>. *?);?";
            
            // Match rgbMatch = Regex.Match(line, rgbRegex);
            // Match hexMatch = Regex.Match(line, hexRegex);
            // Match normalMatch = Regex.Match(line, normalRegex);

            // if (!(rgbMatch.Success || hexMatch.Success || normalMatch.Success))
            // {
            //     Errors.RaiseError("Invalid call", "Invalid call to Colors. Please specify .fg or .bg, and the color name. Refer to the documentation for help.");
            // }




            string rawArgs;

            // rawArgs = rgbMatch.Groups["args"].Value;

            // List<string> positionalArgs;
            // Dictionary<string, string> keywordArgs;
            // Parsers.ParseArgs(rawArgs, out positionalArgs, out keywordArgs);

            return "";
        }

        private static string HandleDefault(string colorName)
        {
            string? colorValue = Get(colorName);

            if (colorValue is null)
            {
                Errors.RaiseError("Invalid color name", $"The color {GlobalVariables.ReprString(colorName)} is not a valid color.");
            }

            return colorValue;
        }

        // private static string HandleRGB(List<string> positional, Dictionary<string, string> keyword){
        //     if (keyword.Count > 0)
        //     {
        //         Errors.RaiseError("Invalid call", "Colors RGB does not take any keyword arguments");
        //     }

        //     if (positional.Count != 3)
        //     {
        //         Errors.RaiseError("Invalid call", "Colors RGB requires exactly 3 positional arguments");
        //     }

        //     string red = positional[0];
        //     string green = positional[1];
        //     string blue = positional[2];

        //     if (!(red.All(char.IsDigit) || green.All(char.IsDigit) || blue.All(char.IsDigit)))
        //     {
        //         Errors.RaiseError("Invalid call", "Colors RGB only accepts integers.");
        //     }
        // }
    }
}