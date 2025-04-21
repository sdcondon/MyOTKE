using MyOTKE.Engine.Utility;
using System.Text;

namespace MyOTKE.Engine.Components.Gui.Elements;

/// <summary>
/// A GUI element that is a stream of text. New messages can be pushed, old messages will eventually disappear.
/// </summary>
public sealed class TextStream : ContainerElementBase
{
    private readonly Text textElement;
    private readonly string[] lines;

    private int lastIndex = -1;

    /// <summary>
    /// Initializes a new instance of the <see cref="TextStream"/> class.
    /// </summary>
    /// <param name="layout">The layout of the element in relation to its parent.</param>
    /// <param name="textColor">The color of the text.</param>
    /// <param name="lineCount">The number of lines of text.</param>
    public TextStream(Layout layout, Color textColor, int lineCount)
        : base(layout)
    {
        this.SubElements.Add(this.textElement = new Text(Layout.Fill, textColor));
        this.lines = new string[lineCount];
    }

    /// <summary>
    /// Gets or sets the horizontal alignment of the text in the stream.
    /// </summary>
    public float HorizontalAlignment
    {
        get => this.textElement.HorizontalAlignment;
        set => this.textElement.HorizontalAlignment = value;
    }

    /// <summary>
    /// Pushes a new message to the text stream element.
    /// </summary>
    /// <param name="message">The message to push.</param>
    public void PushMessage(string message)
    {
        if (++lastIndex >= lines.Length)
        {
            lastIndex = 0;
        }

        lines[lastIndex] = message;

        var builder = new StringBuilder();
        for (int i = 1; i <= lines.Length; i++)
        {
            var line = lines[(lastIndex + i) % lines.Length];
            if (line != null)
            {
                builder.AppendLine(line);
            }
        }

        textElement.Content = builder.ToString();
    }
}
