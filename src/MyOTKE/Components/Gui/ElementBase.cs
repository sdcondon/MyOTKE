using MyOTKE.BufferManagement;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;

namespace MyOTKE.Components.Gui;

/// <summary>
/// Base class for GUI elements. Provides for a nested element hierarchy, with elements being placed relative to their parents.
/// </summary>
/// <param name="layout">The layout that the element should use.</param>
public abstract class ElementBase(Layout layout) : IDisposable
{
    private Layout layout = layout;
    private IListBufferItem<Vertex>? bufferItem;

    public event EventHandler<Vector2> Clicked;

    /// <summary>
    /// Gets the parent of this element.
    /// </summary>
    public IElementParent Parent { get; internal set; }

    /// <summary>
    /// Gets or sets the object that controls the positioning and size of this element.
    /// </summary>
    public Layout Layout
    {
        get => layout;
        set
        {
            layout = value;
            Refresh();
        }
    }

    /// <summary>
    /// Gets the position of the center of the element, in screen space.
    /// </summary>
    public Vector2 Center => Layout.GetCenter(this);

    /// <summary>
    /// Gets the size of the element, in screen space.
    /// </summary>
    public Vector2 Size => Layout.GetSize(this);

    /// <summary>
    /// Gets the position of the bottom-left corner of the element, in screen space.
    /// </summary>
    public Vector2 PosBL => this.Center - this.Size / 2;

    /// <summary>
    /// Gets the position of the bottom-right corner of the element, in screen space.
    /// </summary>
    public Vector2 PosBR => new(this.Center.X + this.Size.X / 2, this.Center.Y - this.Size.Y / 2);

    /// <summary>
    /// Gets the position of the top-left corner of the element, in screen space.
    /// </summary>
    public Vector2 PosTL => new(this.Center.X - this.Size.X / 2, this.Center.Y + this.Size.Y / 2);

    /// <summary>
    /// Gets the position of the top-right corner of the element, in screen space.
    /// </summary>
    public Vector2 PosTR => this.Center + this.Size / 2;

    /// <summary>
    /// Gets the list of vertices to be rendered for this GUI element (not including any children).
    /// </summary>
    public virtual IList<Vertex> Vertices { get; } = [];

    /// <inheritdoc />
    public virtual void Dispose()
    {
        bufferItem?.Dispose();
        GC.SuppressFinalize(this);
    }

    internal virtual void Load(ListBuffer<Vertex> buffer)
    {
        if (bufferItem != null)
        {
            throw new InvalidOperationException("Primitive already attached to a buffer.");
        }

        bufferItem = buffer.Add();
        Refresh();
    }

    internal virtual void Unload(ListBuffer<Vertex> buffer)
    {
        if (!object.ReferenceEquals(buffer, bufferItem.Parent))
        {
            throw new ArgumentException();
        }

        bufferItem.Dispose();
        bufferItem = null;
    }

    internal virtual void Refresh()
    {
        if (bufferItem != null)
        {
            bufferItem.Set(Vertices);
        }
    }

    internal void HandleClick(Vector2 clickLocation)
    {
        var isClickInBounds = clickLocation.X > this.PosTL.X
            && clickLocation.X < this.PosTR.X
            && clickLocation.Y < this.PosTL.Y
            && clickLocation.Y > this.PosBL.Y;

        if (isClickInBounds)
        {
            Clicked?.Invoke(this, clickLocation);
        }
    }
}
