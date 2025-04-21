using MyOTKE.Engine.Components.Gui.Elements;
using MyOTKE.Engine.Utility;
using OpenTK.Mathematics;
using System;

namespace MyOTKE.Engine.Components.Gui;

/// <summary>
/// A clickable GUI button consisting of some text on a colored panel.
/// </summary>
public sealed class Button : ContainerElementBase
{
    private readonly Action<Vector2> clickHandler;
    private readonly Panel panel;

    /// <summary>
    /// Initializes a new instance of the <see cref="Button"/> class.
    /// </summary>
    /// <param name="layout">The layout of the button in relation to it parent.</param>
    /// <param name="color">The background color of the button.</param>
    /// <param name="textColor">The text color of the button.</param>
    /// <param name="text">The text of the button.</param>
    /// <param name="clickHandler">The handler to be invoked when the button is clicked.</param>
    public Button(Layout layout, Color color, Color textColor, string text, Action<Vector2> clickHandler)
        : base(layout)
    {
        this.SubElements.Add(this.panel = new Panel(Layout.Fill, color)
        {
            SubElements =
            {
                new Text(Layout.Fill, textColor, text)
                {
                    HorizontalAlignment = 0.5f,
                    VerticalAlignment = 0.5f,
                },
            },
        });

        this.clickHandler = clickHandler;
    }

    /// <summary>
    /// Gets or sets the background color of the button.
    /// </summary>
    public Vector4 Color
    {
        get => this.panel.Color;
        set => this.panel.Color = value;
    }

    /// <summary>
    /// Invokes the Clicked event.
    /// </summary>
    /// <param name="position">The position of the click to include in the event args.</param>
    protected override void OnClicked(Vector2 position)
    {
        clickHandler(position);
        base.OnClicked(position);
    }
}
