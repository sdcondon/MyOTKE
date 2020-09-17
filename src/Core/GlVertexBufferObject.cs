using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System;
using System.Runtime.InteropServices;

namespace MyOTKE.Core
{
    /// <summary>
    /// A OpenGL vertex buffer object.
    /// </summary>
    /// <typeparam name="T">The .NET type of data to be stored in the buffer.</typeparam>
    internal sealed class GlVertexBufferObject<T> : IVertexBufferObject<T>, IDisposable
        where T : struct
    {
        private readonly int elementSize;

        /// <summary>
        /// Initializes a new instance of the <see cref="GlVertexBufferObject{T}"/> class. SIDE EFFECT: New buffer will be bound to the given target.
        /// </summary>
        /// <param name="target">OpenGL buffer target specification.</param>
        /// <param name="usage">OpenGL buffer usage specification.</param>
        /// <param name="elementType">The type of elements to be stored in the buffer. The data type must be a blittable value type (or an exception will be thrown).</param>
        /// <param name="elementCapacity">The maximum number of elements to be stored in the buffer.</param>
        /// <param name="elementData">The data to populate the buffer with.</param>
        public GlVertexBufferObject(BufferTarget target, BufferUsageHint usage, Type elementType, int elementCapacity, T[] elementData)
        {
            this.Attributes = GlVertexAttributeInfo.ForType(elementType);
            this.Capacity = elementCapacity;
            elementSize = Marshal.SizeOf(typeof(T));

            this.Id = GL.GenBuffer();
            GL.BindBuffer(target, this.Id); // NB: Side effect - leaves this buffer bound.
            GL.BufferData(target, Marshal.SizeOf(elementType) * elementCapacity, elementData, usage);
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="GlVertexBufferObject{T}"/> class.
        /// </summary>
        ~GlVertexBufferObject() => Dispose(false);

        /// <inheritdoc />
        public int Id { get; }

        /// <inheritdoc />
        public GlVertexAttributeInfo[] Attributes { get; }

        /// <inheritdoc />
        public int Capacity { get; }

        /// <inheritdoc />
        public T this[int index]
        {
            get
            {
                T data = default;
                GL.GetNamedBufferSubData(
                    buffer: Id,
                    offset: new IntPtr(index * elementSize),
                    size: elementSize,
                    data: ref data);
                return data;
            }

            set
            {
                GL.NamedBufferSubData(
                    buffer: Id,
                    offset: new IntPtr(index * elementSize),
                    size: elementSize,
                    data: ref value);
            }
        }

        /// <inheritdoc />
        public void Copy(int readIndex, int writeIndex, int count)
        {
            GL.CopyNamedBufferSubData(
                readBuffer: Id,
                writeBuffer: Id,
                readOffset: new IntPtr(readIndex * elementSize),
                writeOffset: new IntPtr(writeIndex * elementSize),
                size: count * elementSize);
        }

        /// <inheritdoc />
        public void Dispose() => Dispose(true);

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                GC.SuppressFinalize(this);
            }

            ////if (GraphicsContext.CurrentContext != null)
            ////{
            GL.DeleteBuffers(1, new[] { this.Id });
            ////}
        }
    }
}
