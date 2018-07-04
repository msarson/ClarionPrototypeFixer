using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace ClarionPrototypeFixer
{
    /// <summary>
    /// This class will parse the prototypes of procedures and change any "ID" fields
    /// from LONG to CSTRING(17)
    /// </summary>
    public class PrototypeFixer
    {

        //Build a list of variables in the parameter that will be changed
        private List<string> variables = new List<string>();

        public PrototypeFixer(string source, bool isNonCodeFile)
        {
            Source = source;
            IsNonCodeFile = isNonCodeFile;
            ParseFile();
        }

        private void CheckForPrototype(string[] lines)
        {
            for (int i = 0; i < lines.Length; i++)
            {
                var currentLine = lines[i].ToUpper();
                if (currentLine.Contains("PROTOTYPE '") || currentLine.Contains("PARAMETERS '"))
                {
                    lines[i] = ParsePrototype(lines[i]);
                    if (FixedLine && currentLine.Contains("PROTOTYPE '"))
                    {
                        lines[i] = lines[i].Remove(lines[i].Length - 1) + ",NAME(''" + lines[i - 1].Substring(5) + "'')'";
                    }
                    FixedLine = false;
                    if (lines[i].ToUpper().Contains("PARAMETERS '") && !IsNonCodeFile)
                        break;
                }
            }
            Source = string.Join(Environment.NewLine, lines);
        }

        private string ConvertPrototype(string parameter)
        {
            var uParameter = parameter.ToUpper();
            if (uParameter.Contains("ID") && uParameter.Contains("LONG"))
            {
                var pos = uParameter.IndexOf("LONG") + 4;
                string varName = parameter.Substring(pos).TrimStart();
                varName = Regex.Replace(varName, @"[^A-Za-z0-9:]+", string.Empty);
                if (varName.ToUpper().EndsWith("ID"))
                {
                    variables.Add(varName.TrimStart());

                    parameter = parameter.ReplaceCaseInsensitive("LONG", "AccuraIDParameter");
                    StringBuilder param = new StringBuilder(parameter);
                    if (param.ToString().Contains("=0"))
                    {
                        param.Replace("=0", "= AccuraIDValue");
                        param.Replace("= 0", "= AccuraIDValue");
                        var c = param.ToString();
                    }
                    return param.ToString();
                }

            }
            return parameter;
        }

        void ParseFile()
        {
            string[] lines = Source.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            CheckForPrototype(lines);
        }

        private string ParsePrototype(string line)
        {
            if (Regex.Matches(line.ToUpper(), "ID").Count != 0)
                line = ParsePrototypeLine(line);
            return line;
        }

        private string ParsePrototypeLine(string line)
        {
            string currentPrototype = string.Empty;
            string lineBegining = string.Empty;
            if (line.Contains("PROTOTYPE"))
            {
                lineBegining = line.Substring(0, 12);
                currentPrototype = line.Substring(12);
            }
            else
            {
                lineBegining = line.Substring(0, 13);
                currentPrototype = line.Substring(13);
            }
            //   currentPrototype = currentPrototype.Substring(0, currentPrototype.Length - 2);
            var parms = currentPrototype.Split(',');
            for (int i = 0; i < parms.Length; i++)
                parms[i] = ConvertPrototype(parms[i]);

            currentPrototype = string.Join(",", parms);
            var sb = new StringBuilder(lineBegining + line);
            var newLine = sb.Replace(line, currentPrototype);
            if (newLine.ToString() != line)
            {
                line = newLine.ToString();
                FixedLine = true;
            }
            else
                FixedLine = false;

            return line;
        }

        public bool FixedLine { get; private set; }
        public bool IsNonCodeFile { get; private set; }

        public string Source { get; private set; }
    }
    public static class ExtensionMethods
    {
        public static string ReplaceCaseInsensitive(this string input,
                                                 string search,
                                                 string replacement)
        {
            string result = Regex.Replace(
                input,
                Regex.Escape(search),
                replacement.Replace("$", "$$"),
                RegexOptions.IgnoreCase
            );
            return result;
        }
    }
}
