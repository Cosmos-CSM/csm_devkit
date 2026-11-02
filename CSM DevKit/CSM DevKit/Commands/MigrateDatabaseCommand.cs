using System.Collections.Concurrent;
using System.CommandLine;
using System.Reflection;

using CSharp_Database_Extension.Abstractions.Interfaces;
using CSharp_Database_Extension.Common.Enums;

using CSharp_Extension.Common.Utils;
using CSharp_Extension.Errors;
using CSharp_Extension.Errors.Abstractions.Interfaces;

using CSM_DevKit.Common.Models;
using CSM_DevKit.Common.Spectre.Columns;
using CSM_DevKit.Common.Utils;
using CSM_DevKit.Options;

using Spectre.Console;

namespace CSM_DevKit.Commands;

/// <summary>
///     A command that generates EntityFramework migration scripts for the database model.
/// </summary>
public class MigrateDatabaseCommand : Command {

    /// <summary>
    ///     Command paramter access for Auto Select.
    /// </summary>
    public static readonly AutoSelectOption AutoSelectOption = new();

    /// <summary>
    ///     Command parameter access for Select All.
    /// </summary>
    public static readonly SelectAllOption SelectAllOption = new();

    /// <summary>
    ///     Command parameter access for Database Providers.
    /// </summary>
    public static readonly DatabaseProvidersOption DatabaseProvidersOption = new();

    /// <summary>
    ///     Command parameter access for Comment.
    /// </summary>
    public static readonly CommentOption CommentOption = new();


    /// <summary>
    ///     Dependency that handles [.Net] command invokations.
    /// </summary>
    readonly DotnetCommandUtils DotNetCommandUtils = new();

    /// Creates a new instancee.
    public MigrateDatabaseCommand()
        : base(
                "migrate",
                "Generates EntityFramework migration scripts"
            ) {

        Add(AutoSelectOption);
        Add(SelectAllOption);
        Add(CommentOption);
        Add(DatabaseProvidersOption);

        SetAction(OnAction);
    }

    public void OnAction(ParseResult parseResult) {

        try {
            Progress progressView = AnsiConsole.Progress();
            progressView.AutoClear(true);
            progressView.Columns(
                    new TaskDescriptionColumn(),
                    new ProgressBarColumn(),
                    new SpinnerColumn(),
                    new CustomElapsedTimeColumn()
                );

            string solutionDir = "";
            List<DatabaseProjectMetadata> databaseProjects = [];
            progressView.Start(
                    (progressContext) => {

                        // Step 1 - Determine working directory metadata.
                        (string workingDir, bool isSolutionDir) = DetermineWorkingDirectoryMetadata(progressContext, parseResult);

                        // Step 2 - Get generation context.
                        (solutionDir, databaseProjects) = GetDatabaseProjects(progressContext, workingDir, isSolutionDir);
                    }
                );

            // Step 3 - Determinate databases to migrate and migrate them.
            (List<DatabaseProjectMetadata> Databases, List<string> migrations) = MigrateDatabases(parseResult, solutionDir, databaseProjects);
            ConsoleUtils.Announce(
                    $"Succesful migrations",
                    new Dictionary<string, object?> {
                        { "Databases", string.Join(", ", Databases.Select(prj => prj.Name)) },
                        { "Migrations", string.Join(", ", migrations) }
                    }
                );
        } catch (AggregateException ex) {
            foreach (Exception inner in ex.Flatten().InnerExceptions) {
                if (inner is IError error) {
                    ConsoleUtils.Exception(error);
                } else {
                    throw;
                }
            }
        }
    }

    (List<DatabaseProjectMetadata> Databases, List<string> Migrations) MigrateDatabases(ParseResult parseResult, string solutionDir, List<DatabaseProjectMetadata> databaseProjects) {
        bool autoSelect = parseResult.GetValue(AutoSelectOption);
        bool selectAll = parseResult.GetValue(SelectAllOption);

        List<DatabaseProjectMetadata> toMigrateProjects = [];
        if (selectAll) {
            toMigrateProjects = databaseProjects;
        } else if (autoSelect && databaseProjects.Count == 1) {
            toMigrateProjects.Add(databaseProjects[0]);
        } else if (!autoSelect) {

            MultiSelectionPrompt<string> selectionPrompt = new() {
                Title = "Select databases to migrate",
                Required = true,
                PageSize = 10,
                InstructionsText = "[grey](Press [blue]<space>[/] to toggle, "
                                    + "[green]<enter>[/] to accept)[/]",
            };

            selectionPrompt.AddChoices(
                    databaseProjects.Select(
                            dbProject => dbProject.Name
                        )
                );

            List<string> selectedDatabases = AnsiConsole.Prompt(selectionPrompt);

            foreach (string selectedDatabase in selectedDatabases) {
                DatabaseProjectMetadata dbProjectCtx = databaseProjects.FirstOrDefault(
                        dbProject => dbProject.Name == selectedDatabase
                    );

                toMigrateProjects.Add(dbProjectCtx);
            }
        }


        // Performing migration.
        DatabaseProviders[] toUseProviders = [];
        DatabaseProviders[]? cmdProviders = parseResult.GetValue(DatabaseProvidersOption);
        if (cmdProviders != null && cmdProviders.Length > 0) {
            toUseProviders = [.. cmdProviders];
        } else {

            MultiSelectionPrompt<DatabaseProviders> providerSelectionPrompt = new() {
                Title = "Select database providers for migrations",
                Required = true,
                PageSize = 10,
                InstructionsText = "[grey](Press [blue]<space>[/] to toggle, "
                                    + "[green]<enter>[/] to accept)[/]",
            };

            providerSelectionPrompt.AddChoices(
                    DatabaseProviders.PostgresSQL,
                    DatabaseProviders.SQLServer
                );


            toUseProviders = [.. AnsiConsole.Prompt(providerSelectionPrompt)];
        }

        ConcurrentBag<string> migrations = [];
        List<Task<DatabaseProjectMetadata>> migrationTasks = [];
        foreach (DatabaseProjectMetadata toMigrateProject in toMigrateProjects) {
            string comment = parseResult.GetValue(CommentOption) ?? Guid.NewGuid().ToString();
            Task<DatabaseProjectMetadata> asyncMigration = Task.Run(
                    async () => {
                        Status statusView = AnsiConsole.Status().Spinner(Spinner.Known.Aesthetic);
                        statusView.SpinnerStyle(
                                Style.Parse("teal")
                            );

                        await statusView.StartAsync(
                                $"  Migrating database project [teal]{toMigrateProject.Name}[/]",
                                async (statusContext) => {

                                    foreach (DatabaseProviders provider in toUseProviders) {
                                        statusContext.Status($"Detecting ({provider}) migration project");
                                        string migrationName = $"{toMigrateProject.Name}.{provider}";

                                        DirectoryInfo solutionDirInfo = new(solutionDir);
                                        FileInfo? projFile = solutionDirInfo.GetFiles($"{migrationName}.csproj", SearchOption.AllDirectories).FirstOrDefault();

                                        // Provider migrations project doesn´t exist, we create one.
                                        if (projFile == null) {
                                            statusContext.Status($"Creating migration project for {provider}");
                                            DotNetCommandUtils.CreateProject(migrationName, solutionDir);
                                            projFile = solutionDirInfo.GetFiles($"{migrationName}.csproj", SearchOption.AllDirectories).FirstOrDefault();
                                        }

                                        if (projFile == null)
                                            throw new SystemError(
                                                    "Migrations project not found",
                                                    data: new Dictionary<string, object?> {
                                                        { "Project Name", migrationName },
                                                        { "Solution Dir", solutionDir },
                                                    }
                                                );

                                        // Here we have the provider migrations project.
                                        statusContext.Status($"Referencing database context project");
                                        string slnDbPrjRelative = Path.GetRelativePath(solutionDir, $"{toMigrateProject.Location}{Path.DirectorySeparatorChar}{toMigrateProject.Name}.csproj");

                                        SystemCommandOutput databaseReferenceOutput = DotNetCommandUtils.Reference(migrationName, slnDbPrjRelative, solutionDir); // Referencing database context.
                                        if (databaseReferenceOutput.ExitCode != 0) {
                                            throw new SystemError(databaseReferenceOutput.Error);
                                        }

                                        statusContext.Status($"Referencing EF Design package");
                                        DotNetCommandUtils.Reference(
                                                migrationName,
                                                "Microsoft.EntityFrameworkCore.Design",
                                                solutionDir,
                                                referenceType: "package"
                                            ); // Referencing EF Design System.

                                        string providerPackage = provider switch {
                                            DatabaseProviders.PostgresSQL => "Npgsql.EntityFrameworkCore.PostgreSQL",
                                            DatabaseProviders.SQLServer => "Microsoft.EntityFrameworkCore.SqlServer",

                                            _ => throw new SystemError($"Unsupported database provider ({provider})")
                                        };

                                        statusContext.Status($"Referencing EF Provider Scoped package");
                                        DotNetCommandUtils.Reference(
                                                migrationName,
                                                providerPackage,
                                                solutionDir,
                                                referenceType: "package"
                                            ); // Referencing EF Provider package.

                                        statusContext.Status($"Referencing CSharp Extension Database package");
                                        DotNetCommandUtils.Reference(
                                                migrationName,
                                                "CSharp.Database.Extension",
                                                solutionDir,
                                                referenceType: "package"
                                            );

                                        // --> Configuring project. 
                                        string templateClassFile = Path.Combine(projFile!.Directory!.FullName, "Class1.cs");
                                        if (File.Exists(templateClassFile)) {
                                            File.Delete(templateClassFile);
                                        }

                                        statusContext.Status($"Generating migrations design factory file");
                                        string factoryFilePath = Path.Combine(projFile!.Directory!.FullName, $"{toMigrateProject.Name}.DesignFactory.cs");
                                        string providerMethod = provider switch {
                                            DatabaseProviders.SQLServer => "UseSqlServer",
                                            DatabaseProviders.PostgresSQL => "UseNpgsql",
                                            _ => throw new SystemError(
                                                    $"Unsupported database provider",
                                                    data: new Dictionary<string, object?> {
                                                        { "Provider", provider }
                                                    }
                                                ),
                                        };
                                        await File.WriteAllTextAsync(
                                                factoryFilePath,
                                                $$"""
                                                        using Microsoft.EntityFrameworkCore.Design;
                                                        using CSharp_Database_Extension.Common.Enums;
                                                        using CSharp_Database_Extension.Common.Models;

                                                        namespace {{migrationName}};

                                                        /// Auto-Generated file by CSM DevKit auto migrations generation per provider ({{DateTime.UtcNow}})[UTC]. 

                                                        public sealed class DesignTimeDbContextFactory
                                                            : IDesignTimeDbContextFactory<{{toMigrateProject.DatabaseContext.Name}}>
                                                        {
                                                            public {{toMigrateProject.DatabaseContext.Name}} CreateDbContext(string[] args)
                                                            {
                                                                DatabaseOptions<{{toMigrateProject.DatabaseContext.Name}}> options = new() {
                                                                    Provider = {{nameof(DatabaseProviders)}}.{{provider}},
                                                                    Migrations = GetType().Assembly
                                                                };

                                                                return new {{toMigrateProject.DatabaseContext.Name}}(options);
                                                            }
                                                        }
                                                        """
                                            );

                                        statusContext.Status("Restoring solution after generation");
                                        DotNetCommandUtils.Restore(solutionDir);

                                        statusContext.Status($"Generating EF Migration");
                                        SystemCommandOutput commandOutput = DotNetCommandUtils.EntityFrameworkMigrate(
                                                solutionDir,
                                                comment,
                                                Path.GetRelativePath(
                                                        solutionDir,
                                                        projFile.FullName
                                                    )
                                            );

                                        if (commandOutput.ExitCode != 0) {
                                            throw new SystemError(
                                                    $"{commandOutput.Error}",
                                                    data: new Dictionary<string, object?> {
                                                        { "Output", commandOutput.Output },
                                                        { "ExitCode", commandOutput.ExitCode }
                                                    }
                                                );
                                        }

                                        migrations.Add(migrationName);
                                    }
                                }
                            );

                        return toMigrateProject;
                    }
                );

            migrationTasks.Add(asyncMigration);
        }

        Task.WaitAll(migrationTasks);

        IEnumerable<DatabaseProjectMetadata> generatedMigrations = migrationTasks.Select(task => task.Result);

        return ([.. generatedMigrations], [.. migrations]);
    }

    /// <summary>
    ///     Step 2. Get database projects from working directory.
    /// </summary>
    /// <param name="progressContext">
    /// </param>
    /// <param name="workingDir"></param>
    /// <param name="isSolutionDir"></param>
    /// <returns></returns>
    /// <exception cref="SystemError"></exception>
    (string solutionDir, List<DatabaseProjectMetadata> dbProjMetadatas) GetDatabaseProjects(ProgressContext progressContext, string workingDir, bool isSolutionDir) {
        ProgressTask task = progressContext.AddTask(
                    "Determinating solution directory",
                    maxValue: 4
                );

        string solutionDir = workingDir;
        if (!isSolutionDir) {
            // When workingDir is a project dir, we want to find the immediate solution dir.
            DirectoryInfo? current = new(workingDir);

            string? solutionDirTmp = null;
            while (solutionDirTmp == null && current != null) {
                bool hasSolution = current.EnumerateFiles("*.sln").Any() || current.EnumerateFiles("*.slnx").Any();

                if (hasSolution)
                    solutionDirTmp = current.FullName;

                current = current.Parent;
            }

            if (solutionDirTmp == null) {
                throw new SystemError($"Unable to determinate solution directory for current project working directory ({workingDir})");
            }

            solutionDir = solutionDirTmp;
        }

        task.Increment(1);
        task.Description = "Building solution assemblies";

        DotNetCommandUtils.Build(solutionDir);

        // Getting dll assemblies that are from databases.
        task.Increment(1);
        task.Description = "Tracking assemblies";

        char systemSeparator = Path.DirectorySeparatorChar;
        string[] assemblyFilePaths = Directory.GetFiles(workingDir, "*.dll", SearchOption.AllDirectories);
        IEnumerable<string> binAssemblies = assemblyFilePaths.Where(
                x => x.Contains($"{systemSeparator}bin{systemSeparator}")
            );


        task.Increment(1);
        task.Description = "Detecting database assemblies";

        List<DatabaseProjectMetadata> dbProjsMetadatas = [];
        foreach (string assemblyFile in binAssemblies) {
            Assembly assembly = Assembly.LoadFrom(assemblyFile);

            Type[] assemblyTypes = assembly.GetTypes();
            IEnumerable<Type> databases = assemblyTypes
                .Where(
                        (type) => type.IsClass && !type.IsAbstract && typeof(IDatabase).IsAssignableFrom(type)
                    );

            if (!databases.Any()) {
                continue;
            }

            if (databases.Count() > 1) {
                throw new SystemError(
                        $"Only one database context is expected per assembly",
                        data: new Dictionary<string, object?> {
                            { "Assembly", assembly.FullName },
                            { "Database Contexts", string.Join(", ", databases.Select(db => db.FullName)) }
                        }
                    );
            }

            DirectoryInfo? acumProjDir = new(
                    Path.GetDirectoryName(assembly.Location)!
                );
            string? projectDir = null;
            while (acumProjDir != null && projectDir == null) {
                if (acumProjDir.EnumerateFiles("*.csproj").Any()) {
                    projectDir = acumProjDir.FullName;
                }

                acumProjDir = acumProjDir.Parent;
            }

            if (projectDir == null) {
                throw new SystemError($"Unable to determinate project dir for assembly ({assembly.Location})");
            }

            dbProjsMetadatas.Add(
                    new DatabaseProjectMetadata {
                        Name = assembly.GetName().Name ?? string.Empty,
                        Location = projectDir,
                        Assembly = assembly,
                        DatabaseContext = databases.First(),
                    }
                );
        }

        if (dbProjsMetadatas.Count == 0) {
            task.StopTask();
            throw new SystemError("Context projects doesn´t contain any database to migrate");
        }

        task.Increment(1);
        return (solutionDir, dbProjsMetadatas);
    }

    /// <summary>
    ///     Step 1. Determinate if the command working directory is a solution or project hosting directory.
    /// </summary>
    /// <returns>
    ///     Working directory absolute path, whether it is a solution or project hosting directory.
    /// </returns>
    static (string workingDirectory, bool isSolutionDirectory) DetermineWorkingDirectoryMetadata(ProgressContext progressContext, ParseResult parseResult) {
        ProgressTask task = progressContext.AddTask(
                            "Determinating working directory",
                            maxValue: 2
                        );

        bool? isSolutionDirectory = null;
        string workingDirectory = parseResult.GetValue(DatabaseCommand.WorkingDirectoryOption)
            ?? Environment.CurrentDirectory;

        if (!Path.IsPathFullyQualified(workingDirectory)) {
            workingDirectory = Path.GetFullPath(workingDirectory, Environment.CurrentDirectory);
        }
        task.Increment(1);

        string[] workingDirectoryFiles = Directory.GetFiles(workingDirectory, "*", SearchOption.TopDirectoryOnly);
        foreach (string wdFile in workingDirectoryFiles) {
            if (
                wdFile.EndsWith(".sln", StringComparison.OrdinalIgnoreCase)
                || wdFile.EndsWith(".sln", StringComparison.OrdinalIgnoreCase) // --> Support for .Net 9 new format
            ) {
                isSolutionDirectory = true;
                break;
            }

            if (wdFile.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase)) {
                isSolutionDirectory = false;
                break;
            }
        }

        if (isSolutionDirectory == null) {
            throw new SystemError("Working directory doesn't contain a valid solution neither project file");
        }


        task.Increment(1);
        return (workingDirectory, isSolutionDirectory ?? false);
    }
}
