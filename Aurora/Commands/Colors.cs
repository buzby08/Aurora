namespace Aurora.Commands;

internal static class Colors
{
    public const string Black = "\e[30m";
    public const string Red = "\e[31m";
    public const string Green = "\e[32m";
    public const string Yellow = "\e[33m";
    public const string Blue = "\e[34m";
    public const string Magenta = "\e[35m";
    public const string Cyan = "\e[36m";
    public const string White = "\e[37m";

    public const string BlackBg = "\e[40m";
    public const string RedBg = "\e[41m";
    public const string GreenBg = "\e[42m";
    public const string YellowBg = "\e[43m";
    public const string BlueBg = "\e[44m";
    public const string MagentaBg = "\e[45m";
    public const string CyanBg = "\e[46m";
    public const string WhiteBg = "\e[47m";

    public static string? Get(string item)
    {
        return item switch
        {
            "BLACK" => Black,
            "RED" => Red,
            "GREEN" => Green,
            "YELLOW" => Yellow,
            "BLUE" => Blue,
            "MAGENTA" => Magenta,
            "CYAN" => Cyan,
            "WHITE" => White,

            "BLACK_BG" => BlackBg,
            "RED_BG" => RedBg,
            "GREEN_BG" => GreenBg,
            "YELLOW_BG" => YellowBg,
            "BLUE_BG" => BlueBg,
            "MAGENTA_BG" => MagentaBg,
            "CYAN_BG" => CyanBg,
            "WHITE_BG" => WhiteBg,

            _ => null // The default case is handled here
        };
    }

    public static string Rgb(int red, int green, int blue, bool background=false)
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

        return $"\e[{colorMode}8;2;{red};{green};{blue}m";
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

        return Rgb(red, green, blue, background);
    }

    public static string Eval(string line)
    {
        if (!(line.StartsWith("Colors.bg") || line.StartsWith("Colors.fg")))
        {
            Errors.RaiseError(new InvalidMethodError("Colors call must specify 'fg' or 'bg'"));
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
            Errors.RaiseError(
                new InvalidAttributeError(
                    $"The color {GlobalVariables.ReprString(colorName)} is not a valid color."));

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
