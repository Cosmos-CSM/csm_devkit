using System.CommandLine;

using CSM_DevKit.Options;

namespace CSM_DevKit.Commands;

/// <summary>
///     Represents commands for Database interactions.
/// </summary>
public partial class DatabaseCommand : Command {

    /// <summary>
    ///     Instance to track [online] option value at this command scope. 
    /// </summary>
    public static readonly OnlineOption OnlineOption = new();

    /// <summary>
    ///     Instance to track [database] option value at this command scope.
    /// </summary>
    public static readonly DatabaseOption DatabaseOption = new();

    /// <summary>
    ///     Instance to track [workingDireectory] option value at this command scope.
    /// </summary>
    public static readonly WorkingDirectoryOption WorkingDirectoryOption = new();

    /// <summary>
    ///     Creates a new instancee
    /// </summary>
    public DatabaseCommand()
        : base(
            "db",
            "Database update related commands"
        ) {


        Add(OnlineOption);
        Add(DatabaseOption);
        Add(WorkingDirectoryOption);

        Add(new MigrateDatabaseCommand());
    }
}
