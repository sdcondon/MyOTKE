using MyOTKE.BufferManagement;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;

namespace MyOTKE.Components.Primitives;

/// <summary>
/// Container for primitive vertex data.
/// </summary>
public abstract class Primitive
{
    private readonly List<PrimitiveVertex> vertices = [];
    private IListBufferItem<PrimitiveVertex>? bufferItem;

    /// <summary>
    /// Gets the list of vertices that comprise the primitive.
    /// </summary>
    public IReadOnlyList<PrimitiveVertex> Vertices => vertices;

    internal void AddToBuffer(ListBuffer<PrimitiveVertex> buffer)
    {
        if (bufferItem != null)
        {
            throw new InvalidOperationException("Primitive already attached to a buffer.");
        }

        bufferItem = buffer.Add();
        SetBufferItem();
    }

    internal bool RemoveFromBuffer(ListBuffer<PrimitiveVertex> buffer)
    {
        if (!object.ReferenceEquals(buffer, bufferItem.Parent))
        {
            return false;
        }

        bufferItem.Dispose();
        bufferItem = null;
        return true;
    }

    /// <summary>
    /// Adds a vertex to this primitive's vertex list.
    /// </summary>
    /// <param name="position">The position of the vertex to add.</param>
    /// <param name="color">Tne color of the vertex to add.</param>
    /// <param name="normal">The normal vector of the vertex to add.</param>
    protected void AddVertex(Vector3 position, Color color, Vector3 normal)
    {
        vertices.Add(new PrimitiveVertex(position, color, normal));
    }

    /// <summary>
    /// Clears this primitive's vertex list.
    /// </summary>
    protected void ClearVertices()
    {
        vertices.Clear();
    }

    /// <summary>
    /// Pushes this primitive's vertices back to the buffer it is attached to. 
    /// </summary>
    protected void SetBufferItem()
    {
        bufferItem?.Set(vertices);
    }
}
