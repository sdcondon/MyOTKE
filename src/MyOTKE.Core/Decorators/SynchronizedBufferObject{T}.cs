using System;
using System.Collections.Concurrent;

namespace MyOTKE.Core.Decorators;

/// <summary>
/// Decorator for <see cref="IBufferObject{T}"/> that explicitly synchronizes with OpenGL (making it simple but slow..).
/// <para/>
/// See https://www.khronos.org/opengl/wiki/Synchronization#Implicit_synchronization for some info.
/// </summary>
/// <typeparam name="T">The .NET type of data to be stored in the buffer.</typeparam>
/// <param name="bufferObject">The <see cref="IBufferObject{T}"/> to wrap.</param>
public class SynchronizedBufferObject<T>(IVertexBufferObject<T> bufferObject) : IBufferObject<T>
    where T : struct
{
    private readonly IBufferObject<T> bufferObject = bufferObject;
    private readonly ConcurrentQueue<Action> actions = new();

    /// <inheritdoc />
    public int Id => bufferObject.Id;

    /// <inheritdoc />
    public int Capacity => bufferObject.Capacity;

    /// <inheritdoc />
    public T this[int index]
    {
        get => bufferObject[index];
        set => actions.Enqueue(() => bufferObject[index] = value);
    }

    /// <inheritdoc />
    public void Copy(int readIndex, int writeIndex, int count)
    {
        actions.Enqueue(() => bufferObject.Copy(readIndex, writeIndex, count));
    }

    /// <summary>
    /// Flush any changes to the underlying buffer.
    /// </summary>
    public void FlushChanges()
    {
        // Only process the actions in the queue at the outset in case they are being continually added.
        for (int i = actions.Count; i > 0; i--)
        {
            actions.TryDequeue(out var action);
            action?.Invoke();
        }
    }
}
