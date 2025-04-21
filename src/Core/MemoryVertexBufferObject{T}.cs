using System;
using System.Collections.Generic;
using System.Threading;

namespace MyOTKE.Core;

/// <summary>
/// Implementation of <see cref="IVertexBufferObject{T}"/> that just stores buffer content in memory, for testing purposes.
/// </summary>
/// <typeparam name="T">The .NET type of data to be stored in the buffer.</typeparam>
public class MemoryVertexBufferObject<T> : IVertexBufferObject<T>
    where T : struct
{
    private static int nextId = 0;
    private readonly T[] content;

    /// <summary>
    /// Initializes a new instance of the <see cref="MemoryVertexBufferObject{T}"/> class.
    /// </summary>
    /// <param name="capacity">The capacity of the buffer.</param>
    /// <param name="data">The data to populate the buffer with, or null.</param>
    public MemoryVertexBufferObject(int capacity, Array data)
    {
        Id = Interlocked.Increment(ref nextId);
        content = new T[capacity];
        data?.CopyTo(content, 0);
    }

    /// <summary>
    /// Gets the content of the buffer.
    /// </summary>
    public IReadOnlyList<T> Content => content;

    /// <inheritdoc />
    public int Id { get; }

    /// <inheritdoc />
    public GlVertexAttributeInfo[] Attributes => throw new NotImplementedException();

    /// <inheritdoc />
    public int Capacity => Content.Count;

    /// <inheritdoc />
    public T this[int index]
    {
        get => content[index];
        set => content[index] = value;
    }

    /// <inheritdoc />
    public void Copy(int readIndex, int writeIndex, int count) => Array.Copy(content, readIndex, content, writeIndex, count);
}
