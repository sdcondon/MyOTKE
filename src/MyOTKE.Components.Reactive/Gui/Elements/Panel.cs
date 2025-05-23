﻿using OpenTK.Mathematics;
using System.Collections.Generic;

namespace MyOTKE.Components.Reactive.Gui.Elements;

/// <summary>
/// GUI element that is a colored panel - and can contain other elements.
/// </summary>
/// <param name="layout">The layout of the of the panel in relation to its parent.</param>
/// <param name="color">The color of the panel.</param>
public class Panel(Layout layout, Vector4 color) : ContainerElementBase(layout)
{
    private Vector4 color = color;

    /// <summary>
    /// Gets or sets the background color of the element.
    /// </summary>
    public Vector4 Color
    {
        get => color;
        set
        {
            color = value;
            OnPropertyChanged(nameof(Color));
        }
    }

    /// <inheritdoc />
    public override IList<Vertex> Vertices =>
    [
        new Vertex(PosTL, Color),
        new Vertex(PosTR, Color),
        new Vertex(PosBL, Color),
        new Vertex(PosBR, Color),
    ];
}
