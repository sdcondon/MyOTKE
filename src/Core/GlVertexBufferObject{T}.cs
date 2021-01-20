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
        private static readonly int ElementSize = Marshal.SizeOf<T>();
        private static readonly GlVertexAttributeInfo[] AttributesStatic = GlVertexAttributeInfo.ForType<T>();

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
            GlDebug.ThrowIfGlError("creating buffer");
            GL.BindBuffer(target, this.Id); // NB: Side effect - leaves this buffer bound.
            GlDebug.ThrowIfGlError("binding buffer");
            GL.BufferData(target, ElementSize * elementCapacity, elementData, usage);
            GlDebug.ThrowIfGlError("creating buffer data store");
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="GlVertexBufferObject{T}"/> class.
        /// </summary>
        ~GlVertexBufferObject() => Dispose(false);

        /// <inheritdoc />
        public int Id { get; }

        /// <inheritdoc />
        public GlVertexAttributeInfo[] Attributes => AttributesStatic;

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
                GlDebug.ThrowIfGlError("getting buffer sub-data");
                return data;
            }

            set
            {
                GL.NamedBufferSubData(
                    buffer: Id,
                    offset: new IntPtr(index * ElementSize),
                    size: ElementSize,
                    data: ref value);
                GlDebug.ThrowIfGlError("setting buffer sub-data");
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
            GlDebug.ThrowIfGlError("copying buffer sub-data");
        }

        //// TODO?: The vast majority of time we'll probably want to interpret the buffer as storing a single type
        //// of object, but sometimes e.g.
        //// AsBufferOf<byte>()..
        //// AsBufferOf<int>(t => t.MyIntProp) - mapping func - would need to calculate stride (and modify this class to account for it when getting and setting)
        //// AsBufferOf<whatever>(attributeinfo) - not safe, but empowering..
        //// HOWEVER: Would need to decide what to do about finalizer - losing the 1-1 relationship between object and buffer means don't necessarily want to delete buffer in finalizer..
        ////public GlVertexBufferObject<TElement> AsBufferOf<TElement>()
        ////{
        ////    //// being able to use custom attributes would be cool? Would need to support creation thereof..
        ////    var mappedCapacity = Capacity; //... scale me (what if not an integer? error or ...)
        ////    return new GlVertexBufferObject<TElement>(Id, ...);
        ////}

        /// <inheritdoc />
        public void Dispose() => Dispose(true);

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                GC.SuppressFinalize(this);
            }

            GL.DeleteBuffer(this.Id);
        }
    }
}
