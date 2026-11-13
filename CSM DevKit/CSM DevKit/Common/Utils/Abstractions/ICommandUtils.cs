using CSharp_Extension.Errors;

namespace CSM_DevKit.Common.Utils.Abstractions;


/// <summary>
///     Represeents a generic command utilities class.
/// </summary>
public interface ICommandUtils {


    /// <summary>
    ///     Determinates whether command working directory is solution-scoped or project-scoped. 
    /// </summary>
    /// <param name="workingDir">
    ///     Command initial working directory resolved, if <c>null</c> will be used <see cref="Environment.CurrentDirectory"/>
    /// </param>
    /// <param name="strictCheck">
    ///     Whether an exception should be thrown if no valid scope is found at working directory.
    /// </param>
    /// <returns>
    ///     <list type="bullet">
    ///         <item>
    ///             <c>workingDir</c>: Sanitized working directory, full calculated path from given <paramref name="workingDir"/>
    ///         </item>
    ///         <item>
    ///             <c>isSolutionDir</c>: Whether the working directory is solution-scoped or not (project-scoped). <c>null</c> means neither. 
    ///         </item>
    ///     </list>
    /// </returns>
    /// <exception cref="SystemError">
    ///     Thrown on no valid scope found for current command working directory.
    /// </exception>
    (string workingDir, bool? isSolutionDir) DeterminateWorkingDirectoryType(string? workingDir, bool strictCheck = false);
}
