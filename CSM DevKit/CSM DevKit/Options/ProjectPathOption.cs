using System.CommandLine;

namespace CSM_DevKit.Options;

/// <summary>
///     Represents a command option to determine the project to use.
/// </summary>
public class ProjectPathOption : Option<string> {

    /// <summary>
    ///     Creates a new instance.
    /// </summary>
    public ProjectPathOption()
        : base(
            "--projctPath",
            [
                    "-p"
                ]
            ) {

        Description = "Overrides the project path to detect databases to migrate, otherwise uses where the command is invoked";
    }
}
