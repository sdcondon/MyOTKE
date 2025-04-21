using OpenTK.Graphics.OpenGL;
using System;

namespace MyOTKE.Core.Decorators
{
    /// <summary>
    /// Decorator for <see cref="IVertexArrayObject{T1}"/> that explicitly synchronizes with OpenGL (making it simple but slow..).
    /// <para/>
    /// See https://www.khronos.org/opengl/wiki/Synchronization#Implicit_synchronization for some info.
    /// </summary>
    /// <typeparam name="T1">The type of the 1st buffer.</typeparam>
    /// <remarks>
    /// TODO: Look into some alternative decorators that do e.g. streaming - https://www.khronos.org/opengl/wiki/Buffer_Object_Streaming.
    /// </remarks>
    public sealed class SynchronizedVertexArrayObject<T1> : IVertexArrayObject<T1>, IDisposable
        where T1 : struct
    {
        private readonly IVertexArrayObject<T1> vertexArrayObject;
        private readonly SynchronizedVertexBufferObject<uint> indexBuffer;
        private readonly SynchronizedVertexBufferObject<T1> attributeBuffer1;

        /// <summary>
        /// Initializes a new instance of the <see cref="SynchronizedVertexArrayObject{T1}"/> class.
        /// </summary>
        /// <param name="vertexArrayObject">The VAO to apply synchronization to.</param>
        public SynchronizedVertexArrayObject(IVertexArrayObject<T1> vertexArrayObject)
        {
            this.vertexArrayObject = vertexArrayObject;
            this.indexBuffer = new SynchronizedVertexBufferObject<uint>(vertexArrayObject.IndexBuffer);
            this.attributeBuffer1 = new SynchronizedVertexBufferObject<T1>(vertexArrayObject.AttributeBuffer1);
        }

        /// <inheritdoc />
        public IVertexBufferObject<uint> IndexBuffer => indexBuffer;

        /// <inheritdoc />
        IVertexBufferObject<T1> IVertexArrayObject<T1>.AttributeBuffer1 => AttributeBuffer1;

        /// <summary>
        /// Gets the <see cref="SynchronizedVertexBufferObject{T1}"/> instance that serves as the 1st attribute buffer for this VAO.
        /// </summary>
        public SynchronizedVertexBufferObject<T1> AttributeBuffer1 => attributeBuffer1;

        /// <inheritdoc />
        public void Dispose()
        {
            // TODO LIFETIME MGMT: aggregated, not component - thus ideally not our responsibility
            // to Dispose (and thus no need for this class to be IDisposable)
            if (vertexArrayObject is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }

        /// <inheritdoc />
        public void Draw(int count)
        {
            GL.Finish();

            indexBuffer.FlushChanges();
            this.attributeBuffer1.FlushChanges();

            vertexArrayObject.Draw(count);
        }
    }

    /// <summary>
    /// Decorator for <see cref="IVertexArrayObject{T1, T2}"/> that explicitly synchronizes with OpenGL (making it simple but slow..).
    /// <para/>
    /// See https://www.khronos.org/opengl/wiki/Synchronization#Implicit_synchronization for some info.
    /// </summary>
    /// <typeparam name="T1">The type of the 1st buffer.</typeparam>
    /// <typeparam name="T2">The type of the 2nd buffer.</typeparam>
    /// <remarks>
    /// TODO: Look into some alternative decorators that do e.g. streaming - https://www.khronos.org/opengl/wiki/Buffer_Object_Streaming.
    /// </remarks>
    public sealed class SynchronizedVertexArrayObject<T1, T2> : IVertexArrayObject<T1, T2>, IDisposable
        where T1 : struct
        where T2 : struct
    {
        private readonly IVertexArrayObject<T1, T2> vertexArrayObject;
        private readonly SynchronizedVertexBufferObject<uint> indexBuffer;
        private readonly SynchronizedVertexBufferObject<T1> attributeBuffer1;
        private readonly SynchronizedVertexBufferObject<T2> attributeBuffer2;

        /// <summary>
        /// Initializes a new instance of the <see cref="SynchronizedVertexArrayObject{T1, T2}"/> class.
        /// </summary>
        /// <param name="vertexArrayObject">The VAO to apply synchronization to.</param>
        public SynchronizedVertexArrayObject(IVertexArrayObject<T1, T2> vertexArrayObject)
        {
            this.vertexArrayObject = vertexArrayObject;
            this.indexBuffer = new SynchronizedVertexBufferObject<uint>(vertexArrayObject.IndexBuffer);
            this.attributeBuffer1 = new SynchronizedVertexBufferObject<T1>(vertexArrayObject.AttributeBuffer1);
            this.attributeBuffer2 = new SynchronizedVertexBufferObject<T2>(vertexArrayObject.AttributeBuffer2);
        }

        /// <inheritdoc />
        public IVertexBufferObject<uint> IndexBuffer => indexBuffer;

        /// <inheritdoc />
        IVertexBufferObject<T1> IVertexArrayObject<T1, T2>.AttributeBuffer1 => AttributeBuffer1;

        /// <summary>
        /// Gets the <see cref="SynchronizedVertexBufferObject{T1}"/> instance that serves as the 1st attribute buffer for this VAO.
        /// </summary>
        public SynchronizedVertexBufferObject<T1> AttributeBuffer1 => attributeBuffer1;

        /// <inheritdoc />
        IVertexBufferObject<T2> IVertexArrayObject<T1, T2>.AttributeBuffer2 => AttributeBuffer2;

        /// <summary>
        /// Gets the <see cref="SynchronizedVertexBufferObject{T2}"/> instance that serves as the 2nd attribute buffer for this VAO.
        /// </summary>
        public SynchronizedVertexBufferObject<T2> AttributeBuffer2 => attributeBuffer2;

        /// <inheritdoc />
        public void Dispose()
        {
            // TODO LIFETIME MGMT: aggregated, not component - thus ideally not our responsibility
            // to Dispose (and thus no need for this class to be IDisposable)
            if (vertexArrayObject is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }

        /// <inheritdoc />
        public void Draw(int count)
        {
            GL.Finish();

            indexBuffer.FlushChanges();
            this.attributeBuffer1.FlushChanges();
            this.attributeBuffer2.FlushChanges();

            vertexArrayObject.Draw(count);
        }
    }

    /// <summary>
    /// Decorator for <see cref="IVertexArrayObject{T1, T2, T3}"/> that explicitly synchronizes with OpenGL (making it simple but slow..).
    /// <para/>
    /// See https://www.khronos.org/opengl/wiki/Synchronization#Implicit_synchronization for some info.
    /// </summary>
    /// <typeparam name="T1">The type of the 1st buffer.</typeparam>
    /// <typeparam name="T2">The type of the 2nd buffer.</typeparam>
    /// <typeparam name="T3">The type of the 3rd buffer.</typeparam>
    /// <remarks>
    /// TODO: Look into some alternative decorators that do e.g. streaming - https://www.khronos.org/opengl/wiki/Buffer_Object_Streaming.
    /// </remarks>
    public sealed class SynchronizedVertexArrayObject<T1, T2, T3> : IVertexArrayObject<T1, T2, T3>, IDisposable
        where T1 : struct
        where T2 : struct
        where T3 : struct
    {
        private readonly IVertexArrayObject<T1, T2, T3> vertexArrayObject;
        private readonly SynchronizedVertexBufferObject<uint> indexBuffer;
        private readonly SynchronizedVertexBufferObject<T1> attributeBuffer1;
        private readonly SynchronizedVertexBufferObject<T2> attributeBuffer2;
        private readonly SynchronizedVertexBufferObject<T3> attributeBuffer3;

        /// <summary>
        /// Initializes a new instance of the <see cref="SynchronizedVertexArrayObject{T1, T2, T3}"/> class.
        /// </summary>
        /// <param name="vertexArrayObject">The VAO to apply synchronization to.</param>
        public SynchronizedVertexArrayObject(IVertexArrayObject<T1, T2, T3> vertexArrayObject)
        {
            this.vertexArrayObject = vertexArrayObject;
            this.indexBuffer = new SynchronizedVertexBufferObject<uint>(vertexArrayObject.IndexBuffer);
            this.attributeBuffer1 = new SynchronizedVertexBufferObject<T1>(vertexArrayObject.AttributeBuffer1);
            this.attributeBuffer2 = new SynchronizedVertexBufferObject<T2>(vertexArrayObject.AttributeBuffer2);
            this.attributeBuffer3 = new SynchronizedVertexBufferObject<T3>(vertexArrayObject.AttributeBuffer3);
        }

        /// <inheritdoc />
        public IVertexBufferObject<uint> IndexBuffer => indexBuffer;

        /// <inheritdoc />
        IVertexBufferObject<T1> IVertexArrayObject<T1, T2, T3>.AttributeBuffer1 => AttributeBuffer1;

        /// <summary>
        /// Gets the <see cref="SynchronizedVertexBufferObject{T1}"/> instance that serves as the 1st attribute buffer for this VAO.
        /// </summary>
        public SynchronizedVertexBufferObject<T1> AttributeBuffer1 => attributeBuffer1;

        /// <inheritdoc />
        IVertexBufferObject<T2> IVertexArrayObject<T1, T2, T3>.AttributeBuffer2 => AttributeBuffer2;

        /// <summary>
        /// Gets the <see cref="SynchronizedVertexBufferObject{T2}"/> instance that serves as the 2nd attribute buffer for this VAO.
        /// </summary>
        public SynchronizedVertexBufferObject<T2> AttributeBuffer2 => attributeBuffer2;

        /// <inheritdoc />
        IVertexBufferObject<T3> IVertexArrayObject<T1, T2, T3>.AttributeBuffer3 => AttributeBuffer3;

        /// <summary>
        /// Gets the <see cref="SynchronizedVertexBufferObject{T3}"/> instance that serves as the 3rd attribute buffer for this VAO.
        /// </summary>
        public SynchronizedVertexBufferObject<T3> AttributeBuffer3 => attributeBuffer3;

        /// <inheritdoc />
        public void Dispose()
        {
            // TODO LIFETIME MGMT: aggregated, not component - thus ideally not our responsibility
            // to Dispose (and thus no need for this class to be IDisposable)
            if (vertexArrayObject is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }

        /// <inheritdoc />
        public void Draw(int count)
        {
            GL.Finish();

            indexBuffer.FlushChanges();
            this.attributeBuffer1.FlushChanges();
            this.attributeBuffer2.FlushChanges();
            this.attributeBuffer3.FlushChanges();

            vertexArrayObject.Draw(count);
        }
    }
}
