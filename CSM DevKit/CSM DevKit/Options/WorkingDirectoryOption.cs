using System.CommandLine;

namespace CSM_DevKit.Options;

/// <summary>
///     Represents a command option to determine the project to use.
/// </summary>
public class WorkingDirectoryOption : Option<string> {

    /// <summary>
    ///     Creates a new instance.
    /// </summary>
    public WorkingDirectoryOption()
        : base(
            "--workingDirectory",
            [
                    "-wd"
                ]
            ) {

        Description = "Overrides the commands working directory to use";
    }
}
