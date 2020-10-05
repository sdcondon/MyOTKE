using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System;
using System.Runtime.InteropServices;

namespace MyOTKE.Core
{
    /// <summary>
    /// A OpenGL vertex buffer object.
    /// </summary>
    /// <typeparam name="T">The .NET type of the data to be stored in the buffer. The type must be a blittable value type (or an exception will be thrown on type initialization).</typeparam>
    internal sealed class GlVertexBufferObject<T> : IVertexBufferObject<T>, IDisposable
        where T : struct
    {
        private static readonly int ElementSize = Marshal.SizeOf(typeof(T));
        private static readonly GlVertexAttributeInfo[] AttributessStatic = GlVertexAttributeInfo.ForType(typeof(T));

        /// <summary>
        /// Initializes a new instance of the <see cref="GlVertexBufferObject{T}"/> class. SIDE EFFECT: New buffer will be bound to the given target.
        /// </summary>
        /// <param name="target">OpenGL buffer target specification.</param>
        /// <param name="usage">OpenGL buffer usage specification.</param>
        /// <param name="elementCapacity">The maximum number of elements to be stored in the buffer.</param>
        /// <param name="elementData">The data to populate the buffer with.</param>
        public GlVertexBufferObject(BufferTarget target, BufferUsageHint usage, int elementCapacity, T[] elementData)
        {
            this.Capacity = elementCapacity;

            this.Id = GL.GenBuffer();
            GL.BindBuffer(target, this.Id); // NB: Side effect - leaves this buffer bound.
            GL.BufferData(target, ElementSize * elementCapacity, elementData, usage);
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="GlVertexBufferObject{T}"/> class.
        /// </summary>
        ~GlVertexBufferObject() => Dispose(false);

        /// <inheritdoc />
        public int Id { get; }

        /// <inheritdoc />
        public GlVertexAttributeInfo[] Attributes => AttributessStatic;

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
                    offset: new IntPtr(index * ElementSize),
                    size: ElementSize,
                    data: ref data);
                return data;
            }

            set
            {
                GL.NamedBufferSubData(
                    buffer: Id,
                    offset: new IntPtr(index * ElementSize),
                    size: ElementSize,
                    data: ref value);
            }
        }

        /// <inheritdoc />
        public void Copy(int readIndex, int writeIndex, int count)
        {
            GL.CopyNamedBufferSubData(
                readBuffer: Id,
                writeBuffer: Id,
                readOffset: new IntPtr(readIndex * ElementSize),
                writeOffset: new IntPtr(writeIndex * ElementSize),
                size: count * ElementSize);
        }

        /// <inheritdoc />
        public void Dispose() => Dispose(true);

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                GC.SuppressFinalize(this);
            }

            if (GraphicsContext.CurrentContext != null)
            {
                GL.DeleteBuffers(1, new[] { this.Id });
            }
        }
    }
}
