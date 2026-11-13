namespace CSM_DevKit.Common.Utils.Abstractions;

/// <summary>
///     Represents an utility class to provide file templates acess through assembly embedded resources.
/// </summary>
public interface IFileTemplatesUtils {

    /// <summary>
    ///     Gets the file template content.
    /// </summary>
    /// <param name="fileName">
    ///     File name to get content.
    /// </param>
    /// <returns>
    ///     Read string file content.
    /// </returns>
    string Get(string fileName);
}
