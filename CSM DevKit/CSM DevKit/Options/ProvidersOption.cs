using System.CommandLine;

using CSharp_Database_Extension.Common.Enums;

namespace CSM_DevKit.Options;

/// <summary>
///     Represents an option to determine what database provider to use.
/// </summary>
public class ProvidersOption : Option<DatabaseProviders> {

    /// <summary>
    ///     Creates a new instance.
    /// </summary>
    public ProvidersOption() 
        : base(
                "--dbProviders",
                [
                    "-dbP",
                    "-dbp"
                    ]
            ) {
    }


}
