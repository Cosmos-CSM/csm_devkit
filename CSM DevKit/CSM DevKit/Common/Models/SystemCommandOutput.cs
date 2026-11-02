namespace CSM_DevKit.Common.Models;

/// <summary>
///     Represents a system command line invokation result.
/// </summary>
public struct SystemCommandOutput {

    /// <summary>
    ///     Command exit code.
    /// </summary>
    public int ExitCode { get; set; }

    /// <summary>
    ///     Command output.
    /// </summary>
    public string Output { get; set; }

    /// <summary>
    ///     Command error.
    /// </summary>
    public string Error { get; set; }
}
