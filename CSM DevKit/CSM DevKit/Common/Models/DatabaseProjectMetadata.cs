using System.Reflection;

namespace CSM_DevKit.Common.Models;


/// <summary>
///     Represents a Database project metadata.
/// </summary>
public struct DatabaseProjectMetadata {

    /// <summary>
    ///     Project name.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    ///     Project absolute system file location.
    /// </summary>
    public string Location { get; set; }

    /// <summary>
    ///     Loaded assembly instance.
    /// </summary>
    public Assembly Assembly { get; set; }

    /// <summary>
    ///     Database context type.
    /// </summary>
    public Type DatabaseContext { get; set; }
}
