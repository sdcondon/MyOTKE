﻿using System;
using System.Collections.Generic;
using System.Threading;

namespace OTKOW.Core
{
    /// <summary>
    /// Implementation of <see cref="IVertexBufferObject"/> that just stores buffer content in memory, for testing purposes.
    /// </summary>
    public class MemoryVertexBufferObject : IVertexBufferObject
    {
        private static int nextId = 0;
        private readonly object[] content;

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryVertexBufferObject"/> class.
        /// </summary>
        /// <param name="capacity">The capacity of the buffer.</param>
        /// <param name="data">The data to populate the buffer with, or null.</param>
        public MemoryVertexBufferObject(int capacity, Array data)
        {
            Id = Interlocked.Increment(ref nextId);
            content = new object[capacity];
            data?.CopyTo(content, 0);
        }

        /// <summary>
        /// Gets the content of the buffer.
        /// </summary>
        public IReadOnlyList<object> Content => content;

        /// <inheritdoc />
        public int Id { get; }

        /// <inheritdoc />
        public GlVertexAttributeInfo[] Attributes => throw new NotImplementedException();

        /// <inheritdoc />
        public int Capacity => Content.Count;

        /// <inheritdoc />
        public object this[int index]
        {
            get => content[index];
            set => content[index] = value;
        }

        /// <inheritdoc />
        public void Copy<T>(int readIndex, int writeIndex, int count) => Array.Copy(content, readIndex, content, writeIndex, count);

        /// <inheritdoc />
        public T GetAs<T>(int index) => (T)Content[index];
    }
}
