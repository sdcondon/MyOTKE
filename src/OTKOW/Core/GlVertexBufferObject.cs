using OpenToolkit.Graphics;
using OpenToolkit.Graphics.OpenGL;
using System;
using System.Runtime.InteropServices;

namespace OTKOW.Core
{
    /// <summary>
    /// A OpenGL vertex buffer object.
    /// </summary>
    internal sealed class GlVertexBufferObject<T> : IVertexBufferObject, IDisposable
        where T : struct
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GlVertexBufferObject"/> class. SIDE EFFECT: New buffer will be bound to the given target.
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

            this.Id = GL.GenBuffer();
            GL.BindBuffer(target, this.Id); // NB: Side effect - leaves this buffer bound.
            GL.BufferData(target, Marshal.SizeOf(elementType) * elementCapacity, elementData, usage);
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="GlVertexBufferObject"/> class.
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
            ////get
            ////{
            ////    Gl.GetNamedBufferSubData(
            ////        buffer: Id,
            ////        offset: new IntPtr(index * Marshal.SizeOf(value)),
            ////        size: (uint)Marshal.SizeOf(value),
            ////        data: null);
            ////}
            set
            {
                GL.NamedBufferSubData(
                    buffer: Id,
                    offset: new IntPtr(index * Marshal.SizeOf(value)),
                    size: Marshal.SizeOf(value),
                    data: ref value);
            }
        }

        /// <inheritdoc />
        public void Copy<T>(int readIndex, int writeIndex, int count)
        {
            var elementSize = Marshal.SizeOf(typeof(T));
            GL.CopyNamedBufferSubData(
                readBuffer: Id,
                writeBuffer: Id,
                readOffset: new IntPtr(readIndex * elementSize),
                writeOffset: new IntPtr(writeIndex * elementSize),
                size: count * elementSize);
        }

        /// <inheritdoc />
        public T GetAs<T>(int index)
        {
            var elementSize = Marshal.SizeOf(typeof(T));
            var ptr = Marshal.AllocHGlobal(elementSize);
            try
            {
                GL.GetNamedBufferSubData(
                    buffer: this.Id,
                    offset: new IntPtr(index * elementSize),
                    size: elementSize,
                    data: ptr);
                return Marshal.PtrToStructure<T>(ptr);
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }
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
