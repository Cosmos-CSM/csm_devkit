using CSM_DevKit.Common.Builders;
using CSM_DevKit.Common.Models;
using CSM_DevKit.Common.Utils.Abstractions;

namespace CSM_DevKit.Common.Utils;

/// <summary>
///     Provide utilities for [dotnet] commands invokation.
/// </summary>
public class DotnetCommandUtils
    : IDotnetCommandUtils {

    /// <summary>
    ///     Internal system command invoker.
    /// </summary>
    readonly SystemCommandInvokerBuilder CmdInvoker;

    /// <summary>
    ///     Creates a new instance.
    /// </summary>
    /// <param name="CmdInvoker">
    ///     Command invoker context to use.
    /// </param>
    public DotnetCommandUtils(SystemCommandInvokerBuilder? CmdInvoker = null) {
        this.CmdInvoker = CmdInvoker ?? new SystemCommandInvokerBuilder("dotnet");
    }

    /// <inheritdoc/>
    public SystemCommandOutput Build(string workingDir) {
        return CmdInvoker.Invoke("build", workingDir);
    }

    /// <inheritdoc/>
    public SystemCommandOutput Restore(string workingDir) {
        return CmdInvoker.Invoke("restore", workingDir);
    }

    /// <inheritdoc/>
    public SystemCommandOutput EntityFrameworkMigrate(string workingDir, string comment, string? project = null) {

        if (string.IsNullOrWhiteSpace(comment)) {
            comment = $"AutoComment-{DateTime.UtcNow:dd_MM_yyyy}";
        }

        string command = $"ef migrations add {comment}";
        if (!string.IsNullOrWhiteSpace(project)) {
            command += $" --project {project}";
        }

        return CmdInvoker.Invoke(
                command,
                workingDir
            );
    }

    /// <inheritdoc/>
    public SystemCommandOutput CreateProject(string projectName, string workingDir, bool linkToSolution = true, string projectType = "classlib") {

        SystemCommandOutput createOutput = CmdInvoker.Invoke(
                $"new {projectType} -n {projectName}",
                workingDirectory: workingDir
            );

        if (!linkToSolution)
            return createOutput;

        if (createOutput.ExitCode != 0)
            return createOutput;

        SystemCommandOutput linkOutput = CmdInvoker.Invoke(
                $"sln add {projectName}{Path.DirectorySeparatorChar}{projectName}.csproj",
                workingDirectory: workingDir
            );


        return linkOutput;
    }

    /// <inheritdoc/>
    public SystemCommandOutput Reference(string projectName, string reference, string workingDirectory, string referenceType = "reference") {
        string command = $"add {projectName} {referenceType} {reference}";

        return CmdInvoker.Invoke(command, workingDirectory);
    }
}
