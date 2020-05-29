using System;
using System.Collections.Concurrent;

namespace OTKOW.Core.VaoDecorators
{
    /// <summary>
    /// Decorator for <see cref="IVertexBufferObject{T}"/> that explicitly synchronizes with OpenGL (making it simple but slow..).
    /// <para/>
    /// See https://www.khronos.org/opengl/wiki/Synchronization#Implicit_synchronization for some info.
    /// </summary>
    /// <typeparam name="T">The .NET type of data to be stored in the buffer.</typeparam>
    public class SynchronizedVertexBufferObject<T> : IVertexBufferObject<T>
        where T : struct
    {
        private readonly IVertexBufferObject<T> vertexBufferObject;
        private readonly ConcurrentQueue<Action> actions = new ConcurrentQueue<Action>();

        /// <summary>
        /// Initializes a new instance of the <see cref="SynchronizedVertexBufferObject{T}" /> class.
        /// </summary>
        /// <param name="vertexBufferObject">The <see cref="IVertexBufferObject{T}"/> to wrap.</param>
        public SynchronizedVertexBufferObject(IVertexBufferObject<T> vertexBufferObject)
        {
            this.vertexBufferObject = vertexBufferObject;
        }

        /// <inheritdoc />
        public int Id => vertexBufferObject.Id;

        /// <inheritdoc />
        public GlVertexAttributeInfo[] Attributes => vertexBufferObject.Attributes;

        /// <inheritdoc />
        public int Capacity => vertexBufferObject.Capacity;

        /// <inheritdoc />
        public T this[int index]
        {
            get => vertexBufferObject[index];
            set => actions.Enqueue(() => vertexBufferObject[index] = value);
        }

        /// <inheritdoc />
        public void Copy(int readIndex, int writeIndex, int count)
        {
            actions.Enqueue(() => vertexBufferObject.Copy(readIndex, writeIndex, count));
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
}
