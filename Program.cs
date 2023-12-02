using System.CommandLine;
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
            var rootCommand =
                new RootCommand("A CLI that counts all the lines of code, excluding comments, in a folder");
            CodeCounter counter = new();
            // Options
            Option<string> filetypesOption = new(name: "--query",
                description: "A list of file extensions to search for, separated by a comma (,)",
                getDefaultValue: () => counter.Query);

            Option<string> folderOption = new(name: "--folder", description: "The target folder",
                getDefaultValue: Directory.GetCurrentDirectory);

            rootCommand.Add(filetypesOption);
            rootCommand.Add(folderOption);


            // Root Command Handler
            rootCommand.SetHandler((fileTypes, folder) =>
            {
                counter.Query = fileTypes;

                string[] files = counter.GetFiles(folder);
                counter.LinesPerFileTable(files).Write();
                counter.SummaryTable(files).Write();

            }, filetypesOption, folderOption);

            await rootCommand.InvokeAsync(args);
        }
    }
}
