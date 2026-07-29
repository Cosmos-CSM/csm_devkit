using System.CommandLine;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Xml.Linq;

using CSM_Database_Core.Abstractions.Interfaces;

using CSM_DevKit.Core.Models;

using CSM_Foundation_Core.Core.Errors;
using CSM_Foundation_Core.Core.Utils;

using Microsoft.EntityFrameworkCore;

using Spectre.Console;

namespace CSM_DevKit.Commands;

/// <summary>
///     Represents commands for Database interactions.
/// </summary>
public partial class DatabaseCommand : Command {


    /// <summary>
    ///     Defines if the assembly loading is online.
    /// </summary>
    readonly Option<bool> _onlineOption = new(
                "--online",
                [
                        "-o"
                    ]
            );

    /// <summary>
    ///     Overrides the project path to calculate database.
    /// </summary>
    readonly Option<string> _projectPathOption = new(
            "--projectPath",
            [
                    "-p"
                ]
        );

    /// <summary>
    ///     Creates a new instancee
    /// </summary>
    public DatabaseCommand()
        : base(
            "db",
            "Database update related commands"
        ) {

        // --> Adding configurations
        Add(_onlineOption);
        Add(_projectPathOption);

        SetAction(OnAction);
    }

    /// <summary>
    ///     Executes when the command is invoked.
    /// </summary>
    /// <param name="context">
    ///     Command line execution context.
    /// </param>
    /// <exception cref="SystemError"></exception>
    void OnAction(ParseResult context) {
        bool isSrcOnline = context.GetValue(_onlineOption);

        // --> Detecting databases

        CSharpProjectPackageInfo[] dbPackages = [];
        AnsiConsole.Status().Spinner(
                Spinner.Known.Aesthetic
            )
            .Start(
                "Detecting business databases",
                ctx => {

                    if (isSrcOnline) {
                        throw new SystemError("Currently online source dbPackage are not supported.");
                    } else {
                        dbPackages = GetOfflineDatabases(context, ctx);
                    }

                    if (dbPackages.Length == 0)
                        throw new SystemError("No CSM databases found");
                }
            );

        /// --> Selecting database to migrate.
        CSharpProjectPackageInfo choosenDb = AnsiConsole.Prompt(
                new SelectionPrompt<CSharpProjectPackageInfo>()
                    .Title("Choose your [green]Database[/]: ")
                    .PageSize(10)
                    .AddChoices(dbPackages)
                    .UseConverter(
                        dbPackage => $"{dbPackage.Name} ([green]{dbPackage.Version}[/])"
                    )
                    .HighlightStyle(
                        new Style(
                                foreground: Color.MediumPurple,
                                decoration: Decoration.Underline
                            )
                    )
            );

        /// Load the assembly and get the DbContext class to migrate.
        DbContext databaseContext = choosenDb.GetContext();

        IEnumerable<string> pendingMigrations = [];
        AnsiConsole.Status().Spinner(
                Spinner.Known.Aesthetic
            )
            .Start(
                "Updating local database migrations",
                statusCtx => {

                    pendingMigrations = databaseContext.Database.GetPendingMigrations();
                    if (pendingMigrations.Any())
                        databaseContext.Database.Migrate();
                }
            );


        ConsoleUtils.Success(
                $"Database migrated",
                new Dictionary<string, object?> {
                    { "Is Online", isSrcOnline },
                    { "Database Package", choosenDb.Name },
                    { "Database Signature", ((IDatabase)databaseContext).Sign },
                    {
                        $"Pending Migrations ({pendingMigrations.Count()})",
                        string.Join(", ", pendingMigrations)
                    },
                }
            );
    }

    /// <summary>
    ///     Gets the referenced database packages on offline mode.
    /// </summary>
    /// <param name="context">
    ///     Command line execution context.
    /// </param>
    /// <param name="progressCtx">
    ///     Console progress indicator status context.
    /// </param>
    /// <returns>
    ///     Database packages refereces found.
    /// </returns>
    /// <exception cref="SystemError"></exception>
    CSharpProjectPackageInfo[] GetOfflineDatabases(ParseResult context, StatusContext progressCtx) {
        string cwd = context.GetValue<string>(_projectPathOption) ?? Environment.CurrentDirectory;

        string csproj = Directory.GetFiles(cwd, "*.csproj")?.FirstOrDefault()
            ?? throw new SystemError("No C# project found in this directory.");

        XDocument doc = XDocument.Load(csproj);
        string targetFramework = doc.Descendants("TargetFramework").First().Value;

        List<CSharpProjectPackageInfo> packages = [
            ..doc
                .Descendants("PackageReference")
                .Select(
                    x => new CSharpProjectPackageInfo {
                        Path = cwd,
                        IsOnline = false,
                        FrameworkVersion = targetFramework,
                        Name = x.Attribute("Include")?.Value ?? "",
                        Version = x.Attribute("Version")?.Value ?? "",
                    }
                )
                .Where(
                    package => {
                        if (string.IsNullOrWhiteSpace(package.Name))
                            return false;

                        string lowerName = package.Name.ToLower();

                        return DatabaseNamesRegEx().IsMatch(lowerName);
                    }
                )
                .DistinctBy(
                    dbPackage => dbPackage.Name
                )
            ];

        progressCtx.Status("Building project assemblies");

        // --> now we build the project to get the correct dependency assembly on local.
        var psi = new ProcessStartInfo {
            FileName = "dotnet",
            Arguments = $"build \"{csproj}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using Process? process = Process.Start(psi);

        process?.WaitForExit();
        string? output = process?.StandardOutput.ReadToEnd();
        string? error = process?.StandardError.ReadToEnd();

        if (!string.IsNullOrWhiteSpace(error))
            throw new SystemError(error);

        return [.. packages];
    }

    /// NOT IMPLEMENTED YET
    static string[] GetOnlineDatabases() {
        throw new NotImplementedException("Online database loading not implemented yet");
    }

    /// <summary>
    ///     Regex rule for database packages namings.
    /// </summary>
    /// <returns></returns>
    [GeneratedRegex(@"^csm\.(?!database)[^.]+\.database(\..+)?$")]
    private static partial Regex DatabaseNamesRegEx();
}
