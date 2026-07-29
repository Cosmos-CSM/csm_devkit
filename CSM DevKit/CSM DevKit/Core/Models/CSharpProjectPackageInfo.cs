using System.Reflection;
using System.Text.Json;

using CSM_Database_Core.Abstractions.Interfaces;

using CSM_Foundation_Core.Core.Errors;

using Microsoft.EntityFrameworkCore;

using Spectre.Console;

namespace CSM_DevKit.Core.Models;

/// <summary>
///     Represents a Project Package Reference Info for CSharp language.
/// </summary>
public class CSharpProjectPackageInfo {

    /// <summary>
    ///     Package name.
    /// </summary>
    required public string Name;

    /// <summary>
    ///     Project path referencing this package info.
    /// </summary>
    required public string Path;

    /// <summary>
    ///     Package referenced version.
    /// </summary>
    required public string Version;

    /// <summary>
    ///     Whether the load is online.
    /// </summary>
    public bool IsOnline = false;

    /// <summary>
    ///     The project target framework
    /// </summary>
    public string FrameworkVersion = "";

    /// <summary>
    ///     Builds the Database context instance based on the assembly referenced.
    /// </summary>
    /// <returns>
    ///     Database context instance.
    /// </returns>
    /// <exception cref="SystemError"></exception>
    public DbContext GetContext() {

        if (IsOnline) {
            throw new SystemError("Currently online database migration is not supported");
        }

        string outputPath = System.IO.Path.Combine(Path, "bin", "Debug", FrameworkVersion);
        string? dllPath = Directory.GetFiles(
                outputPath,
                Name,
                SearchOption.TopDirectoryOnly
            )
            .FirstOrDefault();

        // --> If we couldn't fin the DLL from the bin folder, we look at nuget packages cache.
        if (dllPath is null) {
            string assetsPath = System.IO.Path.Combine(Path, "obj", "project.assets.json");
            using JsonDocument doc = JsonDocument.Parse(File.ReadAllText(assetsPath));

            JsonElement libraries = doc.RootElement.GetProperty("libraries");

            string? packageKey = libraries.EnumerateObject()
                .Select(
                x => x.Name
                )
                .FirstOrDefault(
                    name => name.StartsWith(Name + "/", StringComparison.OrdinalIgnoreCase)
                )
                ?? throw new SystemError($"Database library ({Name}) metadata not found in project.assets.json");

            var targets = doc.RootElement.GetProperty("targets");

            JsonElement targetFrameworkNode = targets.EnumerateObject().First().Value;

            JsonElement packageNode = targetFrameworkNode.GetProperty(packageKey);

            if (!packageNode.TryGetProperty("runtime", out JsonElement runtimeNode))
                throw new SystemError($"Package {Name} has no runtime assets");

            string relativeDllPath = runtimeNode.EnumerateObject()
                .Select(
                    x => x.Name
                )
                .FirstOrDefault(
                    name => name.EndsWith(".dll", StringComparison.OrdinalIgnoreCase)
                )
                ?? throw new Exception($"Package {Name} contains no DLL");

            string nugetCache = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                ".nuget", "packages"
            );

            dllPath = System.IO.Path.Combine(
                    nugetCache,
                    Name.ToLower(),
                    Version,
                    relativeDllPath.Replace('/', System.IO.Path.DirectorySeparatorChar)
                );
        }

        if (dllPath is null)
            throw new SystemError($"Couldn't locate assembly DLL for ({Name})");

        // --> Loading assembly
        Assembly assembly = Assembly.LoadFrom(dllPath);

        IEnumerable<Type> allTypes = assembly.GetTypes();

        IEnumerable<Type> databaseClasses = assembly.GetTypes().Where(
                assemblyClass =>
                    assemblyClass.IsClass
                    && assemblyClass.IsPublic
                    && !assemblyClass.IsAbstract
                    && assemblyClass.IsAssignableTo(typeof(IDatabase))
            );

        if (!databaseClasses.Any())
            throw new SystemError("Assembly doesn't contain a valid DatabaseBase derivable class");

        if (databaseClasses.Count() > 1)
            throw new SystemError("Assembly contains more than one derivable DatabaseBase reference");

        Type derivableType = databaseClasses.First();
        DbContext context = (DbContext?)Activator.CreateInstance(derivableType)
            ?? throw new SystemError("Unable to convert found derivable DatabaseBase to DbContext");

        return context;
    }
}
