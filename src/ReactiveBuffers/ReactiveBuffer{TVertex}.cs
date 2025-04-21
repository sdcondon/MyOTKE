﻿using MyOTKE.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MyOTKE.ReactiveBuffers;

/// <summary>
/// Logic for creating and managing an Open GL buffer for a (observable) set of (observable) items that change over time,
/// each of which provide a list of vertices to the buffer.
/// </summary>
/// <typeparam name="TVertex">The vertex data type.</typeparam>
/// <remarks>
/// TODO: worth considering turning this into an IObserver.
/// </remarks>
public class ReactiveBuffer<TVertex> : IDisposable
    where TVertex : struct
{
    private readonly IObservable<IObservable<IList<TVertex>>> vertexSource;
    private readonly int verticesPerAtom;
    private readonly IList<int> indices;
    private readonly IVertexArrayObject<TVertex> vao;
    private readonly List<ItemObserver> linksByBufferIndex = [];

    private readonly int atomCapacity;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReactiveBuffer{TVertex}"/> class.
    /// </summary>
    /// <param name="vertexArrayObject">The VAO to populate.</param>
    /// <param name="atomSource">
    /// The outer observable should push an inner observable (atom) for each new renderable entity.
    /// The atoms should push a new list of vertices whenever the atom's state changes, and complete when it is removed.
    /// </param>
    /// <param name="atomIndices">The vertex indices to use when rendering each atom.</param>
    public ReactiveBuffer(
        IVertexArrayObject<TVertex> vertexArrayObject,
        IObservable<IObservable<IList<TVertex>>> atomSource,
        IList<int> atomIndices)
    {
        this.vertexSource = atomSource;
        this.verticesPerAtom = atomIndices.Max() + 1; // Perhaps should throw if has unused indices..?
        this.indices = atomIndices;
        this.vao = vertexArrayObject;
        this.atomCapacity = vao.AttributeBuffer1.Capacity / verticesPerAtom;

        this.vertexSource.Subscribe(i => i.Subscribe(new ItemObserver(this)));
    }

    /// <inheritdoc />
    public void Dispose()
    {
        // TODO: Dispose vertexSource subscription

        // TODO LIFETIME MGMT: aggregated, not component - thus ideally not our responsibility to Dispose
        if (this.vao is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }

    /// <summary>
    /// Draw with the active program.
    /// </summary>
    public void Draw()
    {
        this.vao.Draw(linksByBufferIndex.Count * indices.Count);
    }

    private class ItemObserver : IObserver<IList<TVertex>>
    {
        private readonly ReactiveBuffer<TVertex> parent;
        private readonly SortedList<int, int> bufferIndices = [];

        public ItemObserver(ReactiveBuffer<TVertex> parent)
        {
            this.parent = parent;
        }

        public void OnNext(IList<TVertex> vertices)
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
                    if (this.parent.linksByBufferIndex.Count >= this.parent.atomCapacity)
                    {
                        throw new InvalidOperationException("Buffer is full");
                    }

                    bufferIndices.Add(this.parent.linksByBufferIndex.Count, this.parent.linksByBufferIndex.Count);
                    this.parent.linksByBufferIndex.Add(this);
                }

                // Establish buffer index to write to
                var bufferIndex = bufferIndices.Values[atomIndex];

                // Set vertex attributes
                for (int i = 0; i < parent.verticesPerAtom; i++)
                {
                    parent.vao.AttributeBuffer1[bufferIndex * parent.verticesPerAtom + i] = vertices[atomIndex * parent.verticesPerAtom + i];
                }

                // Update the index
                for (int i = 0; i < parent.indices.Count; i++)
                {
                    parent.vao.IndexBuffer[bufferIndex * parent.indices.Count + i] =
                        (uint)(bufferIndex * parent.verticesPerAtom + parent.indices[i]);
                }
            }

            while (atomIndex < bufferIndices.Count)
            {
                DeleteAtom(atomIndex);
            }
        }

        public void OnCompleted()
        {
            // TODO: (if/when resizing is supported) clear buffer data / shrink buffer?
            while (bufferIndices.Count > 0)
            {
                DeleteAtom(0);
            }
        }

        public void OnError(Exception error)
        {
            throw new AggregateException("Reactive buffer source errored", error);
        }

        private void DeleteAtom(int atomIndex)
        {
            var index = bufferIndices.Values[atomIndex];

            // Grab the last link by buffer index, remove its last
            var finalBufferIndex = this.parent.linksByBufferIndex.Count - 1;
            var lastLink = this.parent.linksByBufferIndex[finalBufferIndex];
            lastLink.bufferIndices.RemoveAt(lastLink.bufferIndices.Count - 1);
            parent.linksByBufferIndex.RemoveAt(finalBufferIndex);

            // If last one in the buffer isn't the one being removed, move it
            // to replace the one being removed so that the buffer stays contiguous
            if (finalBufferIndex != index)
            {
                this.bufferIndices.Remove(index);
                lastLink.bufferIndices.Add(index, index);
                this.parent.vao.AttributeBuffer1.Copy(
                    finalBufferIndex * parent.verticesPerAtom,
                    index * parent.verticesPerAtom,
                    this.parent.verticesPerAtom);
                parent.linksByBufferIndex[index] = lastLink;
            }
        }
    }
}
