﻿using System.CommandLine;
using ConsoleTables;

namespace codecountercli
{
    internal class Program
    {

        static readonly Dictionary<string, string> FileNames = new()
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

        private static async Task Main(string[] args)
        {
            var rootCommand = new RootCommand("A CLI that counts all the lines of code, excluding comments, in a folder");

            // Options
            Option<string> filetypesOption = new(name: "--ignorefiles",
                description: "A list of file extensions to ignored, separated by a comma (,)",
                getDefaultValue: () => ".cs, .java, .cpp, .py, .js, .html, .css, .php, .swift, .rb, .go, .c, .h, .ts, .jsx, .tsx, .vue, .json, .xml, .sql, .pl, .sh, .yaml, .md");
            Option<string> folderOption = new(name: "--folder", description: "The target folder", getDefaultValue: Directory.GetCurrentDirectory);

            rootCommand.Add(filetypesOption);
            rootCommand.Add(folderOption);


            // Root Command Handler
            rootCommand.SetHandler((filetypes, folder) => {

                string[] fileExtensions = filetypes.Replace(" ", "").Split(","); // Splits file types into an array of strings

                Console.WriteLine($"Started search at {folder} ");

                List<string> files = DirSearch(folder, fileExtensions); // Searches for files in the folder
                Dictionary<string, int> lineCountPerType = new();

                Console.WriteLine($"Found {files.Count} files matching query:\n {filetypes}");
                if (files.Count == 0) return;

                ConsoleTable table = new("File Name", "Line Count", "File Type");

                foreach (var file in files)
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

                table.Write();

                ConsoleTable summaryTable = new("File Type", "Line Count");

                foreach (var (key, value) in lineCountPerType)
                {
                    summaryTable.AddRow(FileNames.TryGetValue(key, out var name) ? name : key, value);
                }   

                Console.WriteLine("\n Summary: ");
                summaryTable.Write();

            }, filetypesOption, folderOption);

            await rootCommand.InvokeAsync(args);
        }            

        static List<string> DirSearch(string sDir, string[] fileTypeExtensions)
        {
            List<string> files = new List<string>();
            try
            {
                foreach (string f in Directory.GetFiles(sDir))
                {
                    if(fileTypeExtensions.Any(x => f.EndsWith(x)))
                        files.Add(f);
                }
                foreach (string d in Directory.GetDirectories(sDir))
                {
                    files.AddRange(DirSearch(d, fileTypeExtensions));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error when searching for files:\n{e}");
            }

            return files;
        }

        static int CountLines(string file)
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
    }
}
