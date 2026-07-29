
using System.CommandLine;

using CSM_DevKit.Commands;

using CSM_Foundation_Core.Core.Utils;

internal class Program {
    private static int Main(string[] args) {

        try {
            RootCommand rootCommand = new("csmdk") {
                Subcommands = {
                    new DatabaseCommand()
                }
            };

            return rootCommand
                .Parse(args)
                .Invoke(
                    new InvocationConfiguration {
                        EnableDefaultExceptionHandler = false,
                    }
                );
        } catch (Exception x) {
            ConsoleUtils.Error(x.Message);

            return -1;
        }
    }
}