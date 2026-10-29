using System.CommandLine;

using CSM_DevKit.Options;

namespace CSM_DevKit.Commands;

/// <summary>
///     Represents commands for Database interactions.
/// </summary>
public partial class DatabaseCommand : Command {

    /// <summary>
    ///     Creates a new instancee
    /// </summary>
    public DatabaseCommand()
        : base(
            "db",
            "Database update related commands"
        ) {


        Add(new OnlineOption());
        Add(new DatabaseOption());
        Add(new ProjectPathOption());

        Add(new MigrateDatabaseCommand());
    }
}
