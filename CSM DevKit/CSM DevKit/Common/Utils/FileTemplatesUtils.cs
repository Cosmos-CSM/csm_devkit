using System.Reflection;

using CSharp_Extension.Errors;

using CSM_DevKit.Common.Utils.Abstractions;

namespace CSM_DevKit.Common.Utils;

/// <inheritdoc cref="IFileTemplatesUtils"/>
public class FileTemplatesUtils : IFileTemplatesUtils {

    /// <inheritdoc/>
    public string Get(string fileName) {
        Assembly exeAssembly = Assembly.GetEntryAssembly()
             ?? throw new SystemError("Unable to locate execution assembly");

        string? safeAssemblyName = exeAssembly.GetName().Name?.Replace(' ', '_');
        string resourceFullName = $"{safeAssemblyName}.Common.FileTemplates.{fileName}";
        using Stream? stream = exeAssembly.GetManifestResourceStream(resourceFullName)
            ?? throw new SystemError(
                $"Unable to locate resource",
                data: new Dictionary<string, object?> {
                        { "ResourceName", resourceFullName }
                    }
                );

        using StreamReader reader = new(stream);
        return reader.ReadToEnd();
    }
}
