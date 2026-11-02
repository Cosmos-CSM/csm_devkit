using CSharp_Extension.Errors;

using CSM_DevKit.Common.Builders;
using CSM_DevKit.Common.Models;

namespace CSM_DevKit.Common.Utils;

/// <summary>
///     Provide utilities for [dotnet] commands invokation.
/// </summary>
public class DotnetCommandUtils {

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

    /// <summary>
    ///     References a project to another project.
    /// </summary>
    /// <param name="projectName">
    ///     Dependendant project getting the refereence.
    /// </param>
    /// <param name="reference">
    ///     Reference to include.
    /// </param>
    /// <param name="workingDirectory">
    ///     Project directory.
    /// </param>
    /// <param name="referenceType">
    ///     Refeerence type.
    /// </param>
    /// <returns></returns>
    public SystemCommandOutput Reference(string projectName, string reference, string workingDirectory, string referenceType = "reference") {
        string command = $"add {projectName} {referenceType} {reference}";

        return CmdInvoker.Invoke(command, workingDirectory);
    }

    /// <summary>
    ///     Invokes dotnet to build assemblies.
    /// </summary>
    /// <param name="workingDir">
    ///     Dotnet working directory to build.
    /// </param>
    /// <returns>
    ///     System command output.
    /// </returns>
    public SystemCommandOutput Build(string workingDir) {
        return CmdInvoker.Invoke("build", workingDir);
    }

    /// <summary>
    ///     Invokes dotnet to restore assemblies.
    /// </summary>
    /// <param name="workingDir">
    ///     Dotnet working directory to restore.
    /// </param>
    /// <returns>
    ///     System command output.
    /// </returns>
    public SystemCommandOutput Restore(string workingDir) {
        return CmdInvoker.Invoke("restore", workingDir);
    }

    /// <summary>
    ///     Invokes a dotnet new project creation.
    /// </summary>
    /// <param name="projectName">
    ///     Name of the project.
    /// </param>
    /// <param name="workingDir">
    ///     Directory where the project will be created.
    /// </param>
    /// <param name="linkToSolution">
    ///     Whether new project must be referenced to the solution.
    /// </param>
    /// <param name="projectType">
    ///     Type of project.
    /// </param>
    /// <returns>
    ///     Command results.
    /// </returns>
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
}
