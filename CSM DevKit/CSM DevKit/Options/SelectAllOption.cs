using System.CommandLine;

namespace CSM_DevKit.Options;


/// <summary>
///     Represents a command option to indicate to use all available options.
/// </summary>
public class SelectAllOption : Option<bool> {
    public SelectAllOption()
        : base(
            "--select-all",
     [
                "-sA",
                "-select-a",
                "s-all"
                ]
            ) {
    }
}
