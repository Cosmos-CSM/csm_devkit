using CSM_DevKit.Common.Models;

namespace CSM_DevKit.Common.Utils.Abstractions;

/// <summary>
///     Represents a .Net scoped command utilities provider.
/// </summary>
public interface IDotnetCommandUtils {

    /// <summary>
    ///     Invokes dotnet to build assemblies.
    /// </summary>
    /// <param name="workingDir">
    ///     Dotnet working directory to build.
    /// </param>
    /// <returns>
    ///     System command output.
    /// </returns>
    SystemCommandOutput Build(string workingDir);

    /// <summary>
    ///     Invokes dotnet to restore assemblies.
    /// </summary>
    /// <param name="workingDir">
    ///     Dotnet working directory to restore.
    /// </param>
    /// <returns>
    ///     System command output.
    /// </returns>
    SystemCommandOutput Restore(string workingDir);

    /// <summary>
    ///     Invokes dotnet entity framework design runtime migrate command.
    /// </summary>
    /// <param name="workingDir">
    ///     Dotnet working directory to restore.
    /// </param>
    /// <param name="comment">
    ///     Migration title comment.
    /// </param>
    /// <param name="project">
    ///     Specified project 
    /// </param>
    /// <returns>
    ///     System command output.
    /// </returns>
    SystemCommandOutput EntityFrameworkMigrate(string workingDir, string comment, string? project = null);

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
    SystemCommandOutput Reference(string projectName, string reference, string workingDirectory, string referenceType = "reference");

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
    SystemCommandOutput CreateProject(string projectName, string workingDir, bool linkToSolution = true, string projectType = "classlib");
}
