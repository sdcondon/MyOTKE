using MyOTKE.BufferManagement;
using OpenTK.Mathematics;
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

    protected void AddVertex(Vector3 position, Color color, Vector3 normal)
    {
        vertices.Add(new PrimitiveVertex(position, color, normal));
    }

    protected void ClearVertices()
    {
        vertices.Clear();
    }

    protected void SetBufferItem()
    {
        bufferItem?.Set(vertices);
    }
}
