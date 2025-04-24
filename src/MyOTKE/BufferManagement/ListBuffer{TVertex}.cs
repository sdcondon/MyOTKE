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
        private readonly SortedList<int, int> bufferIndices = [];

        public ListBuffer<TVertex> Parent { get; } = parent;

        public void Set(IList<TVertex> vertices)
        {
            if (vertices.Count % Parent.verticesPerAtom != 0)
            {
                throw new InvalidOperationException($"Attribute getter must return multiple of correct number of vertices ({Parent.verticesPerAtom}), but actually returned {vertices.Count}.");
            }

            var atomIndex = 0;
            for (; atomIndex < vertices.Count / Parent.verticesPerAtom; atomIndex++)
            {
                // Add a buffer index to the list if we need to
                if (atomIndex >= bufferIndices.Count)
                {
                    if (Parent.linksByBufferIndex.Count >= Parent.atomCapacity)
                    {
                        throw new InvalidOperationException("Buffer is full");
                    }

                    bufferIndices.Add(Parent.linksByBufferIndex.Count, Parent.linksByBufferIndex.Count);
                    Parent.linksByBufferIndex.Add(this);
                }

                // Establish buffer index to write to
                var bufferIndex = bufferIndices.Values[atomIndex];

                // Set vertex attributes
                for (int i = 0; i < Parent.verticesPerAtom; i++)
                {
                    Parent.vao.AttributeBuffer1[bufferIndex * Parent.verticesPerAtom + i] = vertices[atomIndex * Parent.verticesPerAtom + i];
                }

                // Update the index
                for (int i = 0; i < Parent.atomIndices.Count; i++)
                {
                    Parent.vao.IndexBuffer[bufferIndex * Parent.atomIndices.Count + i] =
                        (uint)(bufferIndex * Parent.verticesPerAtom + Parent.atomIndices[i]);
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
            var finalBufferIndex = Parent.linksByBufferIndex.Count - 1;
            var lastLink = Parent.linksByBufferIndex[finalBufferIndex];
            lastLink.bufferIndices.RemoveAt(lastLink.bufferIndices.Count - 1);
            Parent.linksByBufferIndex.RemoveAt(finalBufferIndex);

            // If last one in the buffer isn't the one being removed, move it
            // to replace the one being removed so that the buffer stays contiguous
            if (finalBufferIndex != index)
            {
                bufferIndices.Remove(index);
                lastLink.bufferIndices.Add(index, index);
                Parent.vao.AttributeBuffer1.Copy(
                    finalBufferIndex * Parent.verticesPerAtom,
                    index * Parent.verticesPerAtom,
                    Parent.verticesPerAtom);
                Parent.linksByBufferIndex[index] = lastLink;
            }
        }
    }
}
