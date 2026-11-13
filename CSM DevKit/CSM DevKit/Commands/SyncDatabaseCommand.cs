using System.CommandLine;
using System.Reflection;

using CSM_DevKit.Common.Utils.Abstractions;

using Microsoft.IdentityModel.Tokens;

using Spectre.Console;

namespace CSM_DevKit.Commands;

/// <summary>
///     Represents the database synchronize command, used to apply EF Shaped databases migrations to local instances.
/// </summary>
public class SyncDatabaseCommand : Command {

    /// <summary>
    ///     Command utils dependency.
    /// </summary>
    readonly ICommandUtils _cmdUtils;

    /// <summary>
    ///     Dotnet Commands utils dependency.
    /// </summary>
    readonly IDotnetCommandUtils _dotnetCmdUtils;

    /// <summary>
    ///     Creates a new instance.
    /// </summary>
    public SyncDatabaseCommand(
            ICommandUtils cmdUtils,
            IDotnetCommandUtils dotnetCmdUtils
        ) : base(
        "sync",
        "Synchronizes local database instances with database migrations"
        ) {
        _cmdUtils = cmdUtils;
        _dotnetCmdUtils = dotnetCmdUtils;

        SetAction(OnAction);
    }

    /// <summary>
    ///     Action triggered on command invokation.
    /// </summary>
    /// <param name="parseResult">
    ///     CMD context.
    /// </param>
    public void OnAction(ParseResult parseResult) {
        Status statusView = AnsiConsole.Status();

        statusView.Start(
                "Loading information for database synchronization",
                statusCtx => {
                    // Based on working directory we determine if we are at a solution or project based working dir.
                    string? initWorkingDir = parseResult.GetValue(DatabaseCommand.WorkingDirectoryOption);
                    (string workingDir, _) = _cmdUtils.DeterminateWorkingDirectoryType(initWorkingDir, true);

                    statusCtx.Status("Building working directory assemblies");
                    _dotnetCmdUtils.Build(workingDir);

                    // After getting sanitized working directory, we get all .csproj metadata to determine database dependencies.
                    statusCtx.Status("Determinating viable database projects");
                    string[] workingDirProjects = Directory.GetFiles(workingDir, "*.csproj", SearchOption.AllDirectories);
                    foreach (string projectFilePath in workingDirProjects) {

                        string? projectDir = Path.GetDirectoryName(projectFilePath);
                        if(projectDir == null)
                            continue;

                        string projectFileName = Path.GetFileNameWithoutExtension(projectFilePath);
                        IEnumerable<string> projectDlls = Directory.GetFiles(projectDir, $"{projectFileName}.dll")
                            .Where(
                                    projectDll => projectDll.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}")
                                );
                        if(projectDlls.IsNullOrEmpty() || projectDlls.Count() > 1)
                            continue;

                        string projectDllPath = projectDlls.First();
                        Assembly loadingAssembly = Assembly.LoadFrom(projectDllPath);
                    }
                }
            );
    }
}
