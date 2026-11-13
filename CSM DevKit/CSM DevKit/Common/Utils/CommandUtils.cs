using CSharp_Extension.Errors;

using CSM_DevKit.Common.Utils.Abstractions;

namespace CSM_DevKit.Common.Utils;


/// <inheritdoc/>
public class CommandUtils
    : ICommandUtils {

    /// <summary>
    ///     Creates a new instance.
    /// </summary>
    public CommandUtils() {
    }

    /// <inheritdoc/>
    public (string workingDir, bool? isSolutionDir) DeterminateWorkingDirectoryType(string? workingDir, bool strictCheck = false) {
        bool? isSolutionDirectory = null;

        string workingDirectory = workingDir ?? Environment.CurrentDirectory;

        if (!Path.IsPathFullyQualified(workingDirectory)) {
            workingDirectory = Path.GetFullPath(workingDirectory, Environment.CurrentDirectory);
        }

        string[] workingDirectoryFiles = Directory.GetFiles(workingDirectory, "*", SearchOption.TopDirectoryOnly);
        foreach (string wdFile in workingDirectoryFiles) {
            if (
                wdFile.EndsWith(".sln", StringComparison.OrdinalIgnoreCase)
                || wdFile.EndsWith(".slnx", StringComparison.OrdinalIgnoreCase) // --> Support for .Net 9 new format
            ) {
                isSolutionDirectory = true;
                break;
            }

            if (wdFile.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase)) {
                isSolutionDirectory = false;
                break;
            }
        }

        if (isSolutionDirectory == null && strictCheck)
            throw new SystemError("Working directory doesn't contain a valid solution neither project file");

        return (workingDirectory, isSolutionDirectory);
    }
}
