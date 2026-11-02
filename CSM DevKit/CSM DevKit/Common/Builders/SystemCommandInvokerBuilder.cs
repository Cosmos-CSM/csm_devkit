using System.Diagnostics;
using System.Text;

using CSM_DevKit.Common.Models;

namespace CSM_DevKit.Common.Builders;

/// <summary>
///     Represents a complex building handler for system commands invokation.
/// </summary>
public class SystemCommandInvokerBuilder {

    /// <summary>
    ///     Root command invokation.
    /// </summary>
    readonly string Root;

    /// <summary>
    ///     Commands working directory.
    /// </summary>
    string WorkingDirectory;

    /// <summary>
    ///     Creates a new instance.
    /// </summary>
    /// <param name="root">
    ///     Root command invokation.
    /// </param>
    /// <param name="workingDirectory">
    ///     Commands working directory.
    /// </param>
    public SystemCommandInvokerBuilder(string root, string? workingDirectory = null) {
        Root = root;
        WorkingDirectory = workingDirectory ?? Environment.CurrentDirectory;
    }

    /// <summary>
    ///     Changes command invoker handler working directory.
    /// </summary>
    /// <param name="workingDirectory"></param>
    public void UseWorkingDirectory(string workingDirectory) {
        WorkingDirectory = workingDirectory;
    }

    /// <summary>
    ///     Invokes a system command action.
    /// </summary>
    /// <param name="arguments">
    ///     Command arugments.
    /// </param>
    /// <param name="workingDirectory">
    ///     Commands working directory.
    /// </param>
    /// <param name="redirectOutput">
    ///     Whether application command output is read from <see cref="Process.StandardOutput"/> stream.
    /// </param>
    /// <param name="redirectError">
    ///     Whether application command error is read from <see cref="Process.StandardError"/> stream.
    /// </param>
    /// <param name="useShellExecute">
    ///     Whether to use the operating system shell to start the process.
    /// </param>
    /// <returns>
    ///     Invokation exit code, output read value, error read value.
    /// </returns>
    public SystemCommandOutput Invoke(string arguments, string? workingDirectory = null, bool redirectOutput = true, bool redirectError = true, bool useShellExecute = false) {
        var outputBuilder = new StringBuilder();
        var errorBuilder = new StringBuilder();

        using Process process = new() {
            StartInfo = new ProcessStartInfo {
                FileName = Root,
                Arguments = arguments,
                WorkingDirectory = workingDirectory ?? WorkingDirectory,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
            }
        };

        process.OutputDataReceived += (_, e) =>
        {
            if (e.Data != null)
                outputBuilder.AppendLine(e.Data);
        };

        process.ErrorDataReceived += (_, e) =>
        {
            if (e.Data != null)
                errorBuilder.AppendLine(e.Data);
        };

        process.Start();

        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        process.WaitForExit();

        return new SystemCommandOutput {
            ExitCode = process.ExitCode,
            Output = outputBuilder.ToString(),
            Error = errorBuilder.ToString()
        };
    }
}
