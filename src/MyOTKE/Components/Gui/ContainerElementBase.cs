using MyOTKE.BufferManagement;
using OpenTK.Mathematics;
using System;

namespace MyOTKE.Components.Gui;

/// <summary>
/// Base class for GUI elements that can contain other elements.
/// </summary>
public abstract class ContainerElementBase : ElementBase, IElementParent
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ContainerElementBase"/> class.
    /// </summary>
    /// <param name="layout">The layout of the element in relation to its parent.</param>
    public ContainerElementBase(Layout layout)
        : base(layout)
    {
        this.SubElements = new ElementCollection(this);
    }

    /// <inheritdoc /> from IElementParent
    public ElementCollection SubElements { get; }

    /// <inheritdoc />
    public override void Dispose()
    {
        // todo: dispose subelements
        base.Dispose();
    }

    internal override void Load(ListBuffer<Vertex> buffer)
    {
        base.Load(buffer);
        SubElements.Load(buffer);
    }

    internal override void Unload(ListBuffer<Vertex> buffer)
    {
        base.Unload(buffer);
        SubElements.Unload(buffer);
    }

    internal override void Refresh()
    {
        base.Refresh();
        SubElements.Refresh();
    }
}
