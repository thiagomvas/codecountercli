using System.CommandLine;
using ConsoleTables;

namespace codecountercli
{
    internal class Program
    {
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

            Option<bool> showSummaryOption = new(name: "--showsummary", description: "Show a summary of the results",
                               getDefaultValue: () => true);

            Option<bool> showFileOption = new(name: "--showfileinfo", description: "Show the results for each file",
                               getDefaultValue: () => false);

            Option<bool> shortFileNamesOption = new(name: "--shortfilenames", description: "Shows only the file name instead of the path to it on the file info table.",
                                              getDefaultValue: () => false);


            rootCommand.Add(filetypesOption);
            rootCommand.Add(folderOption);
            rootCommand.Add(showSummaryOption);
            rootCommand.Add(showFileOption);
            rootCommand.Add(shortFileNamesOption);


            // Root Command Handler
            rootCommand.SetHandler((fileTypes, folder, showperfile, showsummary, shortfilenames) =>
            {
                counter.Query = fileTypes;

                counter.GetAllData(folder);



                if(showperfile) counter.LinesPerFileTable(folder, shortfilenames).Write();
                if(showsummary) counter.SummaryTable().Write();

                Console.WriteLine($"Total Lines of code: {counter.totalLinesOfCode}");

            }, filetypesOption, folderOption, showFileOption, showSummaryOption, shortFileNamesOption);

            await rootCommand.InvokeAsync(args);
        }
    }
}
