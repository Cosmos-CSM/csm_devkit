using System.CommandLine;

using CSharp_Database_Extension.Common.Enums;

namespace CSM_DevKit.Options;

/// <summary>
///     Represents a command option to accept dataabse providers.
/// </summary>
public class DatabaseProvidersOption : Option<DatabaseProviders[]> {


    /// <summary>
    ///     Creates a new instance.
    /// </summary>
    public DatabaseProvidersOption() 
        : base(
            "--db-providers",
        [
                "-dbps",
                "-dbp",
                ]       
            ) {

        AllowMultipleArgumentsPerToken = true;
    }
}
