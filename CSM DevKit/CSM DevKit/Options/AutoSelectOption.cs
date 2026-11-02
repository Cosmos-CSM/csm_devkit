using System.CommandLine;

namespace CSM_DevKit.Options;

/// <summary>
///     Represents a command option that indicates command will execute automatic when only one option is viable.
/// </summary>
public class AutoSelectOption : Option<bool> {

    /// <summary>
    ///     Creates a new instance.
    /// </summary>
    public AutoSelectOption()
        : base(
            "--auto-select",
            [
                "-as",
                "-auto-s",
                "-a-select"
                ]
        ) {
    }
}
