using OpenTK.Graphics.OpenGL;
using System;

namespace MyOTKE.Core
{
    /// <summary>
    /// A OpenGL uniform buffer object.
    /// </summary>
    /// <typeparam name="T">The .NET type of the data to be stored in the buffer. The type must be a blittable value type (or an exception will be thrown on type initialization).</typeparam>
    /// <remarks>
    /// See https://www.khronos.org/opengl/wiki/Uniform_Buffer_Object,
    /// https://learnopengl.com/Advanced-OpenGL/Advanced-GLSL.
    /// </remarks>
    public sealed class GlUniformBufferObject<T> : IBufferObject<T>, IDisposable
        where T : struct
    {
        private readonly GlUniformBlockBindingRef bindingRef;
        private readonly int elementSize;
        private readonly Action<int, int, T> setter;

        /// <summary>
        /// Initializes a new instance of the <see cref="GlUniformBufferObject{T}"/> class. SIDE EFFECT: New buffer will be bound to the UniformBuffer target.
        /// </summary>
        /// <param name="programId">ID of a linked program that uses the block that this buffer stores the data for.</param>
        /// <param name="blockName">The name of the block in the program that this buffer stores the data for.</param>
        /// <param name="usage">OpenGL buffer usage specification.</param>
        /// <param name="elementCapacity">The maximum number of elements to be stored in the buffer.</param>
        /// <param name="elementData">The data to populate the buffer with.</param>
        /// <remarks>
        /// TODO: I don't like the fact that we pass program ID and block name here (since this could be used by multiple programs) - would rather pass metadata - GlUniformBlockInfo or something.
        /// </remarks>
        internal GlUniformBufferObject(int programId, string blockName, BufferUsageHint usage, int elementCapacity, T[] elementData)
        {
            this.setter = GlMarshal.MakeBufferObjectUniformSetter<T>(programId, blockName);
            this.Capacity = elementCapacity;

            this.bindingRef = new GlUniformBlockBindingRef(programId, blockName);
            GL.BindBuffer(BufferTarget.UniformBuffer, this.Id); // NB: Side effect - leaves this buffer bound.
            GlDebug.ThrowIfGlError("Binding uniform buffer");

            var blockIndex = GL.GetUniformBlockIndex(programId, blockName);
            GlDebug.ThrowIfGlError("Getting uniform block index");
            GL.GetActiveUniformBlock(programId, blockIndex, ActiveUniformBlockParameter.UniformBlockDataSize, out elementSize);
            GlDebug.ThrowIfGlError("Getting uniform block size");
            GL.BufferData(BufferTarget.UniformBuffer, elementSize * elementCapacity, elementData, usage);
            GlDebug.ThrowIfGlError("Creating uniform buffer data store");

            // TODO: Not always what we want.. Replace by Bind(int index) in UBO class..?
            GL.BindBufferBase(BufferRangeTarget.UniformBuffer, bindingRef.BindingPoint, bindingRef.BufferRef.Id);
            GlDebug.ThrowIfGlError("setting uniform buffer binding");
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="GlUniformBufferObject{T}"/> class.
        /// </summary>
        ~GlUniformBufferObject() => Dispose(false);

        /// <inheritdoc />
        public int Id => bindingRef.BufferRef.Id;

        /// <summary>
        /// Gets the binding point that this buffer is bound to.
        /// </summary>
        public int BindingPoint => bindingRef.BindingPoint;

        /// <inheritdoc />
        public int Capacity { get; }

        /// <inheritdoc />
        public T this[int index]
        {
            get
            {
                // TODO: method for creating a getter..
                throw new NotImplementedException();
                ////T data = default;
                ////GL.GetNamedBufferSubData(
                ////    buffer: Id,
                ////    offset: new IntPtr(index * ElementSize),
                ////    size: ElementSize,
                ////    data: ref data);
                ////GlDebug.ThrowIfGlError("Getting uniform buffer sub-data");
                ////return data;
            }

            set
            {
                setter(this.Id, index, value);
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
            GlDebug.ThrowIfGlError("Copying uniform buffer sub-data");
        }

        /// <inheritdoc />
        public void Dispose() => Dispose(true);

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                GC.SuppressFinalize(this);
            }

            bindingRef.Dispose();
        }
    }
}
