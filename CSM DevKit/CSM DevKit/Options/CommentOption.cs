using System.CommandLine;

namespace CSM_DevKit.Options;

/// <summary>
///     Represents a command option to accept a comment value.
/// </summary>
public class CommentOption : Option<string> {
    public CommentOption()
        : base(
                "--comment",
                [
                    "-c"
                    ]
            ) {


        Description = "Command comment";
    }
}
