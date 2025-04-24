using MyOTKE.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MyOTKE.BufferManagement;

/// <summary>
/// Logic for creating and managing an Open GL buffer for a list of items that change over time,
/// each of which provide a list of vertices to the buffer.
/// </summary>
/// <typeparam name="TVertex">The vertex data type.</typeparam>
public class ListBuffer<TVertex> : IDisposable
    where TVertex : struct
{
    private readonly int verticesPerAtom;
    private readonly IList<int> atomIndices;
    private readonly IVertexArrayObject<TVertex> vao;
    private readonly List<ListBufferItem> linksByBufferIndex = [];

    private readonly int atomCapacity;

    /// <summary>
    /// Initializes a new instance of the <see cref="ListBuffer{TVertex}"/> class.
    /// </summary>
    /// <param name="vertexArrayObject">The VAO to populate.</param>
    /// <param name="atomIndices">The vertex indices to use when rendering each atom.</param>
    public ListBuffer(
        IVertexArrayObject<TVertex> vertexArrayObject,
        IList<int> atomIndices)
    {
        verticesPerAtom = atomIndices.Max() + 1; // Perhaps should throw if has unused indices..?
        this.atomIndices = atomIndices;
        vao = vertexArrayObject;
        atomCapacity = vao.AttributeBuffer1.Capacity / verticesPerAtom;
    }

    /// <inheritdoc />
    public IListBufferItem<TVertex> Add()
    {
        return new ListBufferItem(this);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        // TODO: Dispose vertexSource subscription

        // TODO LIFETIME MGMT: aggregated, not component - thus ideally not our responsibility to Dispose
        if (vao is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }

    /// <summary>
    /// Draw with the active program.
    /// </summary>
    public void Draw()
    {
        vao.Draw(linksByBufferIndex.Count * atomIndices.Count);
    }

    private class ListBufferItem(ListBuffer<TVertex> parent) : IListBufferItem<TVertex>
    {
        private readonly ListBuffer<TVertex> parent = parent;
        private readonly SortedList<int, int> bufferIndices = [];

        public TVertex this[int index] { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public void Set(IList<TVertex> vertices)
        {
            if (vertices.Count % parent.verticesPerAtom != 0)
            {
                throw new InvalidOperationException($"Attribute getter must return multiple of correct number of vertices ({parent.verticesPerAtom}), but actually returned {vertices.Count}.");
            }

            var atomIndex = 0;
            for (; atomIndex < vertices.Count / parent.verticesPerAtom; atomIndex++)
            {
                // Add a buffer index to the list if we need to
                if (atomIndex >= bufferIndices.Count)
                {
                    if (parent.linksByBufferIndex.Count >= parent.atomCapacity)
                    {
                        throw new InvalidOperationException("Buffer is full");
                    }

                    bufferIndices.Add(parent.linksByBufferIndex.Count, parent.linksByBufferIndex.Count);
                    parent.linksByBufferIndex.Add(this);
                }

                // Establish buffer index to write to
                var bufferIndex = bufferIndices.Values[atomIndex];

                // Set vertex attributes
                for (int i = 0; i < parent.verticesPerAtom; i++)
                {
                    parent.vao.AttributeBuffer1[bufferIndex * parent.verticesPerAtom + i] = vertices[atomIndex * parent.verticesPerAtom + i];
                }

                // Update the index
                for (int i = 0; i < parent.atomIndices.Count; i++)
                {
                    parent.vao.IndexBuffer[bufferIndex * parent.atomIndices.Count + i] =
                        (uint)(bufferIndex * parent.verticesPerAtom + parent.atomIndices[i]);
                }
            }

            while (atomIndex < bufferIndices.Count)
            {
                DeleteAtom(atomIndex);
            }
        }

        public void Dispose()
        {
            // TODO: (if/when resizing is supported) clear buffer data / shrink buffer?
            while (bufferIndices.Count > 0)
            {
                DeleteAtom(0);
            }
        }

        private void DeleteAtom(int atomIndex)
        {
            var index = bufferIndices.Values[atomIndex];

            // Grab the last link by buffer index, remove its last
            var finalBufferIndex = parent.linksByBufferIndex.Count - 1;
            var lastLink = parent.linksByBufferIndex[finalBufferIndex];
            lastLink.bufferIndices.RemoveAt(lastLink.bufferIndices.Count - 1);
            parent.linksByBufferIndex.RemoveAt(finalBufferIndex);

            // If last one in the buffer isn't the one being removed, move it
            // to replace the one being removed so that the buffer stays contiguous
            if (finalBufferIndex != index)
            {
                bufferIndices.Remove(index);
                lastLink.bufferIndices.Add(index, index);
                parent.vao.AttributeBuffer1.Copy(
                    finalBufferIndex * parent.verticesPerAtom,
                    index * parent.verticesPerAtom,
                    parent.verticesPerAtom);
                parent.linksByBufferIndex[index] = lastLink;
            }
        }
    }
}
