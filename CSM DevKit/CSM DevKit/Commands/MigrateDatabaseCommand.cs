using System.CommandLine;

namespace CSM_DevKit.Commands;

/// <summary>
///     A command that generates EntityFramework migration scripts for the database model.
/// </summary>
public class MigrateDatabaseCommand : Command {


    /// Creates a new instancee.
    public MigrateDatabaseCommand() 
        : base(
                "migrate",
                "Generates EntityFramework migration scripts"
            ) {

        SetAction(OnAction);
    }

    public void OnAction(ParseResult parseResult) {

    }
}
