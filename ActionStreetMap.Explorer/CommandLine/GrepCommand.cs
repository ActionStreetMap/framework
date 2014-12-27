using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ActionStreetMap.Explorer.CommandLine
{
    /// <summary>
    ///     Simple grep implementation. Ported from http://www.codeproject.com/Articles/1485/A-C-Grep-Application.
    /// </summary>
    public class GrepCommand : ICommand
    {
        /// <inheritdoc />
        public string Name { get { return "grep"; } }

        /// <inheritdoc />
        public string Description { get { return "simple grep implementation"; } }

        /// <summary>
        ///     Content to search.
        /// </summary>
        public List<string> Content { get; set; }

        /// <inheritdoc />
        public string Execute(params string[] args)
        {
            var response = new StringBuilder();
            var grep = new ConsoleGrep(response);
            var commandLine = new Arguments(args);
            if (commandLine["h"] != null || commandLine["H"] != null)
            {
                grep.PrintHelp();
                return response.ToString();
            }
            // The arguments /e and /f are mandatory
            if (commandLine["e"] != null)
                grep.RegEx = (string)commandLine["e"];
            else
            {
                response.AppendLine("Error: No Regular Expression specified!");
                response.AppendLine();
                grep.PrintHelp();
                return response.ToString();
            }

            grep.IgnoreCase = (commandLine["i"] != null);
            grep.LineNumbers = (commandLine["n"] != null);
            grep.CountLines = (commandLine["c"] != null);

            // Do the search
            grep.Search(Content.Take(Content.Count - 1).ToList());

            return response.ToString();
        }

        #region Nested classes

        private class ConsoleGrep
        {
            private readonly StringBuilder _response;

            public bool IgnoreCase { get; set; }
            public bool LineNumbers { get; set; }
            public bool CountLines { get; set; }
            public string RegEx { get; set; }

            public ConsoleGrep(StringBuilder response)
            {
                _response = response;
            }

            /// <summary>
            ///     Search Function.
            /// </summary>
            /// <param name="log">Input content.</param>
            public void Search(List<string> log)
            {
                String strResults = "Grep Results:\r\n\r\n";
                int iLine = 0, iCount = 0;
                bool bEmpty = true;

                var enumerator = log.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    var strLine = enumerator.Current;
                    var lines = strLine.Split('\n');
                    foreach (var text in lines)
                    {
                        iLine++;
                        //Using Regular Expressions as a real Grep
                        Match mtch;
                        if (IgnoreCase)
                            mtch = Regex.Match(text, RegEx, RegexOptions.IgnoreCase);
                        else
                            mtch = Regex.Match(text, RegEx);
                        if (mtch.Success)
                        {
                            bEmpty = false;
                            iCount++;
                            //Add the Line to Results string
                            if (LineNumbers)
                                strResults += "  " + iLine + ": " + text + "\r\n";
                            else
                                strResults += "  " + text + "\r\n";
                        }
                    }
                }

                if (CountLines)
                    strResults += "  " + iCount + " Lines Matched\r\n";
                strResults += "\r\n";

                _response.AppendLine(bEmpty ? "No matches found!" : strResults);
            }

            //Print Help
            public void PrintHelp()
            {
                _response.AppendLine("Usage: grep [/h|/H]");
                _response.AppendLine("       grep [/c] [/i] [/n] /e:reg_exp");
            }
        }
        #endregion
    }
}
