using System.CommandLine;

namespace CSM_DevKit.Options;

/// <summary>
///     Represents a command option that determines usage is with online sources, otherwise will use local resources.
/// </summary>
public class OnlineOption : Option<bool> {

    /// <summary>
    ///     Creates a new instance.
    /// </summary>
    public OnlineOption() 
        : base(
            "--onlineResources",
            [ 
                    "-or" 
                ]
        ) {

        Description = "Determines if command will use online resources";
    }
}
