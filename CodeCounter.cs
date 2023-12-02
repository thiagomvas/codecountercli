using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ConsoleTables;
using Microsoft.VisualBasic.FileIO;

namespace codecountercli
{
    internal class CodeCounter
    {
        public readonly Dictionary<string, string> FileNames = new()
        {
            { "cs", "C#" },
            { "java", "Java" },
            { "cpp", "C++" },
            { "py", "Python" },
            { "js", "JavaScript" },
            { "html", "HTML" },
            { "css", "CSS" },
            { "php", "PHP" },
            { "swift", "Swift" },
            { "rb", "Ruby" },
            { "go", "Go" },
            { "c", "C" },
            { "h", "Header" },
            { "ts", "TypeScript" },
            { "jsx", "React JSX" },
            { "tsx", "React TypeScript" },
            { "vue", "Vue.js" },
            { "json", "JSON" },
            { "xml", "XML" },
            { "sql", "SQL" },
            { "pl", "Perl" },
            { "sh", "Shell" },
            { "yaml", "YAML" },
            { "md", "Markdown" },
            { "dll", "DLL"},
            { "exe", "Executable"},
        };

        public string Query =
            ".cs, .java, .cpp, .py, .js, .html, .css, .php, .swift, .rb, .go, .c, .h, .ts, .jsx, .tsx, .vue, .json, .xml, .sql, .pl, .sh, .yaml, .md";

        private readonly Dictionary<string, int> lineCountPerType = new();
        private readonly Dictionary<string, int> lineCountPerFile = new();

        private string dir;

        public string[] QueryToArray() => Query.Replace(" ", "").Split(",");

        public ConsoleTable SummaryTable(string[] files)
        {
            ConsoleTable table = new("Language / File Type", "Lines of code", "Percentage of total");

            table.Options.EnableCount = false;

            int total = lineCountPerType.Values.Sum();

            foreach (var (key, value) in lineCountPerType.OrderByDescending(x => x.Value))
            {
                table.AddRow(FileNames.TryGetValue(key, out var name) ? name : key, value, $"{Percentage(value, total)}%".Replace(',', '.'));
            }

            return table;
        }

        public ConsoleTable LinesPerFileTable(string[] files, string rootDirectory)
        {
            ConsoleTable table = new("File Name", "Lines of Code", "File Type");

            var query = QueryToArray();

            table.Options.EnableCount = false;

            foreach (string file in files)
            {
                string fileName = Path.GetFileName(file);
                string fileType = fileName.Split(".").Last();
                int lineCount = CountLines(file);

                if (lineCountPerType.ContainsKey(fileType))
                    lineCountPerType[fileType] += lineCount;
                else
                    lineCountPerType.Add(fileType, lineCount);

                if (lineCountPerFile.ContainsKey(file))
                    lineCountPerFile[file] += lineCount;
                else
                    lineCountPerFile.Add(file, lineCount);

            }

            foreach ((string key, int value) in lineCountPerFile.OrderByDescending(x => x.Value))
            {   
                string fileType = Path.GetFileName(key).Split(".").Last();
                table.AddRow(Path.GetRelativePath(rootDirectory, key), value, FileNames.TryGetValue(fileType, out var name) ? name : fileType);
            }
            return table;
        }

        public string[] GetFiles(string directory)
        {
            List<string> found = new();
            var query = QueryToArray();
            try
            {
                foreach (string f in Directory.GetFiles(directory))
                {
                    if (query.Any(x => f.EndsWith(x)))
                        found.Add(f);
                }
                foreach (string d in Directory.GetDirectories(directory))
                {
                    found.AddRange(GetFiles(d));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error when searching for files:\n{e}");
            }

            return found.ToArray();
        }

        public int CountLines(string file)
        {
            int count = 0;
            try
            {
                using (StreamReader r = new StreamReader(file))
                {
                    while (r.ReadLine() is { } rawline)
                    {
                        string line = rawline.Trim();
                        if (string.IsNullOrWhiteSpace(line)) continue;
                        //if (!string.IsNullOrWhiteSpace(line) && !line.StartsWith("//") && !line.StartsWith("/*") &&
                        //    !line.StartsWith("*") &&
                        //    !line.StartsWith("#") && !line.StartsWith("<!--") && !line.EndsWith("-->") && !line.StartsWith("///"))
                        if (!Regex.Match(line, @"^(///|//|/\*|\*/|-->|<!--|#|\*)").Success)
                            count++;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error when counting lines in file {file}:\n{e}");
            }

            return count;
        }

        private float Percentage(int value, int total)
        {
            return MathF.Round((float)value / total * 100 * 100) / 100;
        }
    }
}
