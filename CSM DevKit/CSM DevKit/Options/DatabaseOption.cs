using System.CommandLine;

namespace CSM_DevKit.Options;

/// <summary>
///     Represents a command option to specify a determined database to use for operation.
/// </summary>
public class DatabaseOption : Option<string?> {

    /// <summary>
    ///     Creates a new instance.
    /// </summary>
    public DatabaseOption()
        : base(
            "--database",
     ["-d"]
            ) {

        Description = "Predefines a database to migrate if available in the project references, the name must be defined as the package name. (ex. CSM.Security.Database)";
    }
}