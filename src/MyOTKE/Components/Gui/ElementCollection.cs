using MyOTKE.BufferManagement;
using OpenTK.Mathematics;
using System;
using System.Collections;
using System.Collections.Generic;

namespace MyOTKE.Components.Gui;

public class ElementCollection : ICollection<ElementBase>
{
    private readonly IElementParent owner;
    private readonly List<ElementBase> content = [];

    private ListBuffer<Vertex>? buffer;

    /// <summary>
    /// Initializes a new instance of the <see cref="ElementCollection"/> class.
    /// </summary>
    /// <param name="owner">The parent of this collection.</param>
    public ElementCollection(IElementParent owner) => this.owner = owner;

    /// <inheritdoc />
    public int Count => content.Count;

    /// <inheritdoc />
    public bool IsReadOnly => false;

    /// <inheritdoc />
    public void Add(ElementBase element)
    {
        if (element.Parent != null)
        {
            throw new ArgumentException("Can't add element that already has a parent");
        }

        element.Parent = this.owner;
        if (buffer != null)
        {
            element.Load(buffer);
        }

        content.Add(element);
    }

    /// <inheritdoc />
    public bool Remove(ElementBase element)
    {
        if (!content.Remove(element))
        {

            return false;
        }

        element.Parent = null;
        if (buffer != null)
        {
            element.Unload(buffer);
        }

        return true;
    }

    /// <inheritdoc />
    public void Clear()
    {
        foreach (var element in content)
        {
            element.Parent = null;
            if (buffer != null)
            {
                element.Unload(buffer);
            }
        }

        content.Clear();
    }

    /// <inheritdoc />
    public bool Contains(ElementBase item) => content.Contains(item);

    /// <inheritdoc />
    public void CopyTo(ElementBase[] array, int arrayIndex) => content.CopyTo(array, arrayIndex);

    /// <inheritdoc />
    public IEnumerator<ElementBase> GetEnumerator() => content.GetEnumerator();

    /// <inheritdoc />)
    IEnumerator IEnumerable.GetEnumerator() => content.GetEnumerator();

    internal void Load(ListBuffer<Vertex> buffer)
    {
        this.buffer = buffer;
        foreach (var item in content)
        {
            item.Load(buffer);
        }
    }

    internal void HandleClick(Vector2 clickLocation)
    {
        foreach (var item in content)
        {
            item.HandleClick(clickLocation);
        }
    }

    internal void Unload(ListBuffer<Vertex> buffer)
    {
        this.buffer = null;
        foreach (var item in content)
        {
            item.Unload(buffer);
        }
    }

    internal void Refresh()
    {
        foreach (var item in content)
        {
            item.Refresh();
        }
    }
}
