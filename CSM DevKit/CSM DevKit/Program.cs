
using System.CommandLine;

internal class Program
{
    private static int Main(string[] args)
    {
        Command dbCommands = new(
                "db",
                "Database related commands"
            );

        dbCommands.SetAction(
                (parseResult) =>
                {
                    Console.WriteLine("Welcome to database related commands");
                    return 0;
                }
            );
        
        
        RootCommand rootCommand = new("csmdk")
        {
            Subcommands = {
                    dbCommands
                }
        };


        return rootCommand.Parse(args).Invoke();
    }
}