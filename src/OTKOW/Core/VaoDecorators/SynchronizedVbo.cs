using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace OTKOW.Core.VaoDecorators
{
    public class SynchronizedVertexBufferObject<T> : IVertexBufferObject<T>
        where T : struct
    {
        private readonly IVertexBufferObject<T> vertexBufferObject;
        private readonly ConcurrentQueue<Action> actions = new ConcurrentQueue<Action>();

        public SynchronizedVertexBufferObject(IVertexBufferObject<T> vertexBufferObject)
        {
            this.vertexBufferObject = vertexBufferObject;
        }

        public int Id => vertexBufferObject.Id;

        public GlVertexAttributeInfo[] Attributes => vertexBufferObject.Attributes;

        public int Capacity => vertexBufferObject.Capacity;

        public T this[int index]
        {
            set => actions.Enqueue(() => vertexBufferObject[index] = value);
            get => vertexBufferObject[index];
        }

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
