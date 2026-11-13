
using System.CommandLine;

using CSharp_Extension.Common.Utils;
using CSharp_Extension.Errors.Abstractions.Interfaces;

using CSM_DevKit.Commands;
using CSM_DevKit.Common.Utils;
using CSM_DevKit.Common.Utils.Abstractions;

internal class Program {
    private static int Main(string[] args) {

        ICommandUtils cmdUtils = new CommandUtils();
        IDotnetCommandUtils dotnetCmdUtils = new DotnetCommandUtils();
        IFileTemplatesUtils fileTemplateUtils = new FileTemplatesUtils();

        try {
            MigrateDatabaseCommand migrateDatabaseCommand = new(cmdUtils, dotnetCmdUtils, fileTemplateUtils);

            DatabaseCommand databaseCommand = new(migrateDatabaseCommand);

            RootCommand rootCommand = new("Provides utilities methods for CSM developers and maintainers") {
                Subcommands = {
                    databaseCommand,
                },

            };

            return rootCommand
                .Parse(args)
                .Invoke(
                    new InvocationConfiguration {
                        EnableDefaultExceptionHandler = false,
                    }
                );
        } catch (Exception x) when (x.InnerException is IError error) {
            ConsoleUtils.Exception(error);
            return -1;
        } catch (Exception x) {
            ConsoleUtils.Error(x.Message);
            return -1;
        }
    }
}