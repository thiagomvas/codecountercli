using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConsoleTables;

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
            { "md", "Markdown" }
        };

        public string Query =
            ".cs, .java, .cpp, .py, .js, .html, .css, .php, .swift, .rb, .go, .c, .h, .ts, .jsx, .tsx, .vue, .json, .xml, .sql, .pl, .sh, .yaml, .md";

        private readonly Dictionary<string, int> lineCountPerType = new();

        public ConsoleTable SummaryTable(string[] files)
        {
            ConsoleTable table = new("Language / File Type", "Lines of code", "Percentage of total");

            int total = lineCountPerType.Values.Sum();

            foreach (var (key, value) in lineCountPerType)
            {
                table.AddRow(FileNames.TryGetValue(key, out var name) ? name : key, value, Percentage(value, total));
            }

            return table;
        }

        public ConsoleTable LinesPerFileTable(string[] files)
        {
            ConsoleTable table = new("File Name, Lines of Code, File Type");
            foreach (string file in files)
            {
                string fileName = Path.GetFileName(file);
                string fileType = fileName.Split(".").Last();
                int lineCount = CountLines(file);

                if (lineCountPerType.ContainsKey(fileType))
                    lineCountPerType[fileType] += lineCount;
                else
                    lineCountPerType.Add(fileType, lineCount);

                table.AddRow(fileName, lineCount, FileNames.TryGetValue(fileType, out var name) ? name : fileType);
            }

            return table;
        }

        public string[] GetFiles(string directory)
        {
            List<string> found = new();

            try
            {
                foreach (string f in Directory.GetFiles(directory))
                {
                    if (Query.Any(x => f.EndsWith(x)))
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
                    while (r.ReadLine() is { } line)
                    {
                        if (!string.IsNullOrWhiteSpace(line) && !line.StartsWith("//") && !line.StartsWith("/*") &&
                            !line.StartsWith("*") &&
                            !line.StartsWith("#") && !line.StartsWith("<!--") && !line.EndsWith("-->"))
                        {
                            count++;
                        }
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
