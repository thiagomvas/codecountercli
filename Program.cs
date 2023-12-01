using System.CommandLine;

namespace codecountercli
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            var rootCommand = new RootCommand("A CLI that counts all the lines of code, excluding comments, in a folder");

            Option<string> filetypesOption = new(name: "--ignorefiles",
                description: "A list of file extensions to ignored, separated by a comma (,)",
                getDefaultValue: () => ".cs, .java, .cpp, .py, .js, .html, .css, .php, .swift, .rb, .go, .c, .h, .ts, .jsx, .tsx, .vue, .json, .xml, .sql, .pl, .sh, .yaml, .md");
            Option<string> folderOption = new(name: "--folder", description: "The target folder", getDefaultValue: Directory.GetCurrentDirectory);

            rootCommand.Add(filetypesOption);
            rootCommand.Add(folderOption);

            rootCommand.SetHandler((filetypes, folder) => {

                string[] fileExtensions = filetypes.Replace(" ", "").Split(",");

                Console.WriteLine($"Started search at {folder} ");

                List<string> files = DirSearch(folder, fileExtensions);

                Console.WriteLine($"Found {files.Count} files matching query:\n {filetypes}");
                foreach (var file in files)
                        Console.WriteLine($"File: {file} - {CountLines(file)} lines");


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
