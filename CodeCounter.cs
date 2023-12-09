using System.Reflection;
using System.Text.RegularExpressions;
using ConsoleTables;
using Newtonsoft.Json;

namespace codecountercli
{
    internal class CodeCounter
    {

        public CodeCounter()
        {
            string json = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "languages.json"));
            List<Language> languages = JsonConvert.DeserializeObject<List<Language>>(json);

            foreach (Language lang in languages)
            {
                string name = lang.Name;
                if(lang.Extensions.Length == 0) continue;
                foreach (string extension in lang.Extensions)
                {
                    extensionToName.TryAdd(extension, name);
                    extensions.Add(extension);
                }
            }
        }
        Dictionary<string, string> extensionToName = new Dictionary<string, string>();
        private HashSet<string> extensions = new();
        public readonly Dictionary<string, string> FileNames = new();

        public int totalLinesOfCode = 0;

        public string Query = string.Empty;

        private readonly Dictionary<string, int> lineCountPerType = new();
        private readonly Dictionary<string, int> lineCountPerFile = new();

        public string[] QueryToArray() => Query.Replace(" ", "").Split(",");


        public ConsoleTable SummaryTable()
        {
            ConsoleTable table = new("Language / File Type", "Lines of code", "Percentage of total");

            table.Options.EnableCount = false;

            int total = lineCountPerType.Values.Sum();

            foreach (var (key, value) in lineCountPerType.OrderByDescending(x => x.Value))
            {
                table.AddRow(key, value, $"{Percentage(value, total)}%".Replace(',', '.'));
            }

            return table;
        }

        public void GetAllData(string folder)
        {
            if(string.IsNullOrWhiteSpace(Query)) Console.WriteLine("No query specified, counting all files...");
            else Console.WriteLine($"Query: {Query}");

            Console.WriteLine("Reading files...");
            string[] files = GetFiles(folder);
            Console.WriteLine($"\nFound {files.Length} files...");
            Console.WriteLine("Counting lines...");
            RunCounter(files);
            Console.WriteLine("");
        }

        public void RunCounter(string[] files)
        {


            foreach (string file in files)
            {
                string fileType = Path.GetExtension(file);
                string key = extensionToName.TryGetValue(fileType, out string name) ? name.Trim() : fileType;
                int lineCount = CountLines(file);

                if (lineCountPerType.ContainsKey(key))
                    lineCountPerType[key] += lineCount;
                else
                    lineCountPerType.Add(key, lineCount);

                if (lineCountPerFile.ContainsKey(file))
                    lineCountPerFile[file] += lineCount;
                else
                    lineCountPerFile.Add(file, lineCount);

            }
        }

        public ConsoleTable LinesPerFileTable(string rootDirectory, bool shortFileNames)
        {
            ConsoleTable table = new("File Name", "Lines of Code", "File Type");
            table.Options.EnableCount = false;

            table.Options.EnableCount = false;
            foreach ((string key, int value) in lineCountPerFile.OrderByDescending(x => x.Value))
            {   
                string fileType = Path.GetFileName(key).Split(".").Last();
                string fileName = "";
                if(shortFileNames)
                    fileName = Path.GetFileName(key);
                else
                    fileName = Path.GetRelativePath(rootDirectory, key);
                table.AddRow(fileName, value, FileNames.TryGetValue(fileType, out var name) ? name : fileType);
            }
            return table;
        }

        public string[] GetFiles(string directory)
        {
            bool hasQuery = !string.IsNullOrWhiteSpace(Query);
            var query = QueryToArray();
            List<string> found = new();
            try
            {
                foreach (string f in Directory.GetFiles(directory))
                {
                    if (hasQuery && query.Any(x => Path.GetExtension(f) == x))
                    {
                        found.Add(f);
                    }
                    else if (!hasQuery && extensions.Any(x => Path.GetExtension(f) == x))
                    {
                        found.Add(f);
                    }
                    Console.Write($"\rReading File: {Path.GetFileName(f)}");
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
            Console.Write($"\rCounting lines for {Path.GetFileName(file)}");
            try
            {
                using (StreamReader r = new StreamReader(file))
                {
                    while (r.ReadLine() is { } rawline)
                    {
                        string line = rawline.Trim();
                        if (string.IsNullOrWhiteSpace(line)) continue;
                        if (!Regex.Match(line, @"^(///|//|/\*|\*/|-->|<!--|#|\*)").Success)
                            count++;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error when counting lines in file {file}:\n{e}");
            }
            totalLinesOfCode += count;
            return count;
        }

        private float Percentage(int value, int total)
        {
            return MathF.Round((float)value / total * 100 * 100) / 100;
        }
    }
}
