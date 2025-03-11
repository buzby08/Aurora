using System.Collections;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Xml;

namespace Aurora
{
    static class Parsers
    {
        public static void ParseArgs(
            string input,
            out List<string> positional,
            out Dictionary<string, string> keyword
        )
        {
            positional = [];
            keyword = [];

            bool inString = false;
            bool foundKeywordArg = false;

            bool currentArgIsKeyword = false;
            char? currentArgStringStart = null;
            List<char> currentArgValue = [];

            foreach (char character in input)
            {
                if (character is ' ' & !inString)
                {
                    continue;
                    
                }
                if (character is not ('"' or '\'' or ';' or '='))
                {
                    currentArgValue.Add(character);
                    continue;
                }

                if (character is ')' && !inString)
                {
                    return;
                }

                if (character == '=' && !inString)
                {
                    currentArgValue.Add(character);
                    currentArgIsKeyword = true;
                    foundKeywordArg = true;
                    continue;
                }

                if (character == ';' && currentArgIsKeyword && !inString)
                {
                    (string key, string value) = SplitKeywordArg(currentArgValue);
                    foundKeywordArg = true;
                    keyword[key] = value;
                    currentArgValue = [];
                    currentArgIsKeyword = false;
                    continue;
                }

                if (character == ';' && !inString && !foundKeywordArg && !currentArgIsKeyword)
                {
                    positional.Add(string.Join("", currentArgValue));
                    currentArgValue = [];
                    continue;
                }

                if (character == ';' && !inString && !currentArgIsKeyword && foundKeywordArg)
                {
                    Errors.RaiseError("Invalid function call", "Positional arguments must come before keyword arguments");
                }

                if (character is '"' or '\'' && !inString)
                {
                    inString = true;
                    currentArgStringStart = character;
                    currentArgValue.Add(character);
                    continue;
                }

                if (character is '"' or '\'' && inString && currentArgStringStart == character)
                {
                    inString = false;
                    currentArgStringStart = null;
                    currentArgValue.Add(character);
                }

                if (character is '"' or '\'' && inString && currentArgStringStart != character)
                {
                    currentArgValue.Add(character);
                    continue;
                }

            }

            if (inString)
            {
                Errors.RaiseError("Invalid call", "The argument call contained an unclosed string");
            }

            return;
        }

        private static Tuple<string, string> SplitKeywordArg(List<char> argValue)
        {
            int equalsPos = argValue.IndexOf('=');

            if (equalsPos == -1)
            {
                Console.WriteLine($"[{string.Join(", ", argValue)}]");
                Errors.RaiseError("System error", "Could not handle keyword argument parsing - Function call was broken");
            }

            List<char> key = argValue[..equalsPos];
            List<char> value = argValue[(equalsPos+1)..];

            return new Tuple<string, string> (string.Join("", key), string.Join("", value));
        }
    }    
}