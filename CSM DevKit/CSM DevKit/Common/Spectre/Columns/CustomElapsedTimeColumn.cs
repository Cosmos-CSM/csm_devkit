using Spectre.Console;
using Spectre.Console.Rendering;

namespace CSM_DevKit.Common.Spectre.Columns;

class CustomElapsedTimeColumn : ProgressColumn {

    protected override bool NoWrap => false;

    /// <summary>
    /// Gets or sets the style of the remaining time text.
    /// </summary>
    public Style Style { get; set; } = Color.Blue;

    /// <inheritdoc/>
    public override IRenderable Render(RenderOptions options, ProgressTask task, TimeSpan deltaTime) {
        var elapsed = task.ElapsedTime;
        if (elapsed == null) {
            return new Markup("--:--:--");
        }

        if (elapsed.Value.TotalHours > 99) {
            return new Markup("**:**:**");
        }

        return new Text($"{elapsed.Value:hh\\:mm\\:ss\\.fff}", Style);
    }

    /// <inheritdoc/>
    public override int? GetColumnWidth(RenderOptions options) {
        return 12;
    }
}
