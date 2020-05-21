#pragma warning disable SA1402
using OpenToolkit.Graphics.OpenGL;
using OTKOW.Core.VaoDecorators;

namespace OTKOW.Core
{
    /// <summary>
    /// Builder class for <see cref="GlVertexArrayObject"/> objects that presents a fluent-ish interface.
    /// </summary>
    public sealed class VertexArrayObjectBuilder2
    {
        private readonly PrimitiveType primitiveType;
        private (int count, uint[] data) indexSpec;

        /// <summary>
        /// Initializes a new instance of the <see cref="VertexArrayObjectBuilder2"/> class.
        /// </summary>
        /// <param name="primitiveType">The type of primitive data to be stored in the built VAO.</param>
        public VertexArrayObjectBuilder2(
            PrimitiveType primitiveType)
        {
            this.primitiveType = primitiveType;
        }

        /// <summary>
        /// Adds a new populated attribute buffer to be included in the built VAO.
        /// </summary>
        /// <typeparam name="T">The type of data to be stored in the buffer.</typeparam>
        /// <param name="bufferUsage">The usage type for the buffer.</param>
        /// <param name="data">The data with which to populate the buffer.</param>
        /// <returns>The updated builder.</returns>
        public VertexArrayObjectBuilder2<T> WithAttributeBuffer<T>(BufferUsageHint bufferUsage, T[] data)
            where T : struct
        {
            return new VertexArrayObjectBuilder2<T>(
                primitiveType,
                indexSpec,
                (bufferUsage, data.Length, data));
        }

        /// <summary>
        /// Adds a new empty attribute buffer to be included in the built VAO.
        /// </summary>
        /// <typeparam name="T">The type of data to be stored in the buffer.</typeparam>
        /// <param name="bufferUsage">The usage type for the buffer.</param>
        /// <param name="size">The size of the buffer, in instances of <see typeparamref="T"/>.</param>
        /// <returns>The updated builder.</returns>
        public VertexArrayObjectBuilder2<T> WithAttributeBuffer<T>(BufferUsageHint bufferUsage, int size)
            where T : struct
        {
            return new VertexArrayObjectBuilder2<T>(
                primitiveType,
                indexSpec,
                (bufferUsage, size, null));
        }

        /// <summary>
        /// Sets the index buffer to be included in the built VAO.
        /// </summary>
        /// <param name="data">The data with which to populate the buffer.</param>
        /// <returns>The updated builder.</returns>
        public VertexArrayObjectBuilder2 WithIndex(uint[] data)
        {
            this.indexSpec = (data.Length, data);
            return this;
        }

        /// <summary>
        /// Sets the index buffer to be included in the built VAO.
        /// </summary>
        /// <param name="capacity">The size of the index buffer.</param>
        /// <returns>The updated builder.</returns>
        public VertexArrayObjectBuilder2 WithIndex(int capacity)
        {
            this.indexSpec = (capacity, null);
            return this;
        }
    }

    /// <summary>
    /// Builder class for <see cref="GlVertexArrayObject{T1}"/> objects that presents a fluent-ish interface.
    /// </summary>
    /// <typeparam name="T1">The type of data to be stored in the 1st buffer.</typeparam>
    public sealed class VertexArrayObjectBuilder2<T1>
        where T1 : struct
    {
        private readonly PrimitiveType primitiveType;
        private readonly (BufferUsageHint usage, int elementCount, T1[] data) bufferSpec1;
        private (int count, uint[] data) indexSpec;

        /// <summary>
        /// Initializes a new instance of the <see cref="VertexArrayObjectBuilder2{T1}"/> class.
        /// </summary>
        /// <param name="primitiveType">The type of primitive data to be stored in the built VAO.</param>
        /// <param name="indexSpec">Specification for the index buffer.</param>
        /// <param name="bufferSpec1">Specification for the 1st buffer.</param>
        internal VertexArrayObjectBuilder2(
            PrimitiveType primitiveType,
            (int count, uint[] data) indexSpec,
            (BufferUsageHint usage, int elementCount, T1[] data) bufferSpec1)
        {
            this.primitiveType = primitiveType;
        }

        /// <summary>
        /// Adds a new populated attribute buffer to be included in the built VAO.
        /// </summary>
        /// <typeparam name="T">The type of data to be stored in the buffer.</typeparam>
        /// <param name="bufferUsage">The usage type for the buffer.</param>
        /// <param name="data">The data with which to populate the buffer.</param>
        /// <returns>The updated builder.</returns>
        public VertexArrayObjectBuilder2<T1, T> WithAttributeBuffer<T>(BufferUsageHint bufferUsage, T[] data)
            where T : struct
        {
            return new VertexArrayObjectBuilder2<T1, T>(
                primitiveType,
                indexSpec,
                bufferSpec1,
                (bufferUsage, data.Length, data));
        }

        /// <summary>
        /// Adds a new empty attribute buffer to be included in the built VAO.
        /// </summary>
        /// <typeparam name="T">The type of data to be stored in the buffer.</typeparam>
        /// <param name="bufferUsage">The usage type for the buffer.</param>
        /// <param name="size">The size of the buffer, in instances of <see typeparamref="T"/>.</param>
        /// <returns>The updated builder.</returns>
        public VertexArrayObjectBuilder2<T1, T> WithAttributeBuffer<T>(BufferUsageHint bufferUsage, int size)
            where T : struct
        {
            return new VertexArrayObjectBuilder2<T1, T>(
                primitiveType,
                indexSpec,
                bufferSpec1,
                (bufferUsage, size, null));
        }

        /// <summary>
        /// Sets the index buffer to be included in the built VAO.
        /// </summary>
        /// <param name="data">The data with which to populate the buffer.</param>
        /// <returns>The updated builder.</returns>
        public VertexArrayObjectBuilder2<T1> WithIndex(uint[] data)
        {
            this.indexSpec = (data.Length, data);
            return this;
        }

        /// <summary>
        /// Sets the index buffer to be included in the built VAO.
        /// </summary>
        /// <param name="capacity">The size of the index buffer.</param>
        /// <returns>The updated builder.</returns>
        public VertexArrayObjectBuilder2<T1> WithIndex(int capacity)
        {
            this.indexSpec = (capacity, null);
            return this;
        }

        /// <summary>
        /// Specifies that the built VAO should be explicitly synchronized, with any pending changes flushed on each draw call.
        /// <para/>
        /// Specifically, means that the created VAO will be a <see cref="SynchronizedVao{T1}"/> instance.
        /// </summary>
        /// <returns>The updated builder.</returns>
        public SynchronizedVertexArrayObjectBuilder<T1> Synchronized()
        {
            return new SynchronizedVertexArrayObjectBuilder<T1>(this);
        }

        /// <summary>
        /// Builds a new <see cref="IVertexArrayObject{T1}"/> instance based on the state of the builder.
        /// </summary>
        /// <returns>The built VAO.</returns>
        public IVertexArrayObject<T1> Build()
        {
            return new GlVertexArrayObject<T1>(
                primitiveType,
                bufferSpec1,
                indexSpec);
        }
    }

    /// <summary>
    /// Builder class for <see cref="GlVertexArrayObject{T1, T2}"/> objects that presents a fluent-ish interface.
    /// </summary>
    /// <typeparam name="T1">The type of data to be stored in the 1st buffer.</typeparam>
    /// <typeparam name="T2">The type of data to be stored in the 2nd buffer.</typeparam>
    public sealed class VertexArrayObjectBuilder2<T1, T2>
        where T1 : struct
        where T2 : struct
    {
        private readonly PrimitiveType primitiveType;
        private readonly (BufferUsageHint usage, int elementCount, T1[] data) bufferSpec1;
        private readonly (BufferUsageHint usage, int elementCount, T2[] data) bufferSpec2;
        private (int count, uint[] data) indexSpec;

        /// <summary>
        /// Initializes a new instance of the <see cref="VertexArrayObjectBuilder2{T1, T2}"/> class.
        /// </summary>
        /// <param name="primitiveType">The type of primitive data to be stored in the built VAO.</param>
        /// <param name="indexSpec">Specification for the index buffer.</param>
        /// <param name="bufferSpec1">Specification for the 1st buffer.</param>
        /// <param name="bufferSpec2">Specification for the 2nd buffer.</param>
        internal VertexArrayObjectBuilder2(
            PrimitiveType primitiveType,
            (int count, uint[] data) indexSpec,
            (BufferUsageHint usage, int elementCount, T1[] data) bufferSpec1,
            (BufferUsageHint usage, int elementCount, T2[] data) bufferSpec2)
        {
            this.primitiveType = primitiveType;
        }

        /// <summary>
        /// Adds a new populated attribute buffer to be included in the built VAO.
        /// </summary>
        /// <typeparam name="T">The type of data to be stored in the buffer.</typeparam>
        /// <param name="bufferUsage">The usage type for the buffer.</param>
        /// <param name="data">The data with which to populate the buffer.</param>
        /// <returns>The updated builder.</returns>
        public VertexArrayObjectBuilder2<T1, T2, T> WithAttributeBuffer<T>(BufferUsageHint bufferUsage, T[] data)
            where T : struct
        {
            return new VertexArrayObjectBuilder2<T1, T2, T>(
                primitiveType,
                indexSpec,
                bufferSpec1,
                bufferSpec2,
                (bufferUsage, data.Length, data));
        }

        /// <summary>
        /// Adds a new empty attribute buffer to be included in the built VAO.
        /// </summary>
        /// <typeparam name="T">The type of data to be stored in the buffer.</typeparam>
        /// <param name="bufferUsage">The usage type for the buffer.</param>
        /// <param name="size">The size of the buffer, in instances of <see typeparamref="T"/>.</param>
        /// <returns>The updated builder.</returns>
        public VertexArrayObjectBuilder2<T1, T2, T> WithAttributeBuffer<T>(BufferUsageHint bufferUsage, int size)
            where T : struct
        {
            return new VertexArrayObjectBuilder2<T1, T2, T>(
                primitiveType,
                indexSpec,
                bufferSpec1,
                bufferSpec2,
                (bufferUsage, size, null));
        }

        /// <summary>
        /// Sets the index buffer to be included in the built VAO.
        /// </summary>
        /// <param name="data">The data with which to populate the buffer.</param>
        /// <returns>The updated builder.</returns>
        public VertexArrayObjectBuilder2<T1, T2> WithIndex(uint[] data)
        {
            this.indexSpec = (data.Length, data);
            return this;
        }

        /// <summary>
        /// Sets the index buffer to be included in the built VAO.
        /// </summary>
        /// <param name="capacity">The size of the index buffer.</param>
        /// <returns>The updated builder.</returns>
        public VertexArrayObjectBuilder2<T1, T2> WithIndex(int capacity)
        {
            this.indexSpec = (capacity, null);
            return this;
        }

        /// <summary>
        /// Specifies that the built VAO should be explicitly synchronized, with any pending changes flushed on each draw call.
        /// <para/>
        /// Specifically, means that the created VAO will be a <see cref="SynchronizedVao{T1, T2}"/> instance.
        /// </summary>
        /// <returns>The updated builder.</returns>
        public SynchronizedVertexArrayObjectBuilder<T1, T2> Synchronized()
        {
            return new SynchronizedVertexArrayObjectBuilder<T1, T2>(this);
        }

        /// <summary>
        /// Builds a new <see cref="IVertexArrayObject{T1, T2}"/> instance based on the state of the builder.
        /// </summary>
        /// <returns>The built VAO.</returns>
        public IVertexArrayObject<T1, T2> Build()
        {
            return new GlVertexArrayObject<T1, T2>(
                primitiveType,
                bufferSpec1,
                bufferSpec2,
                indexSpec);
        }
    }

    /// <summary>
    /// Builder class for <see cref="GlVertexArrayObject{T1, T2, T3}"/> objects that presents a fluent-ish interface.
    /// </summary>
    /// <typeparam name="T1">The type of data to be stored in the 1st buffer.</typeparam>
    /// <typeparam name="T2">The type of data to be stored in the 2nd buffer.</typeparam>
    /// <typeparam name="T3">The type of data to be stored in the 3rd buffer.</typeparam>
    public sealed class VertexArrayObjectBuilder2<T1, T2, T3>
        where T1 : struct
        where T2 : struct
        where T3 : struct
    {
        private readonly PrimitiveType primitiveType;
        private readonly (BufferUsageHint usage, int elementCount, T1[] data) bufferSpec1;
        private readonly (BufferUsageHint usage, int elementCount, T2[] data) bufferSpec2;
        private readonly (BufferUsageHint usage, int elementCount, T3[] data) bufferSpec3;
        private (int count, uint[] data) indexSpec;

        /// <summary>
        /// Initializes a new instance of the <see cref="VertexArrayObjectBuilder2{T1, T2, T3}"/> class.
        /// </summary>
        /// <param name="primitiveType">The type of primitive data to be stored in the built VAO.</param>
        /// <param name="indexSpec">Specification for the index buffer.</param>
        /// <param name="bufferSpec1">Specification for the 1st buffer.</param>
        /// <param name="bufferSpec2">Specification for the 2nd buffer.</param>
        /// <param name="bufferSpec3">Specification for the 3rd buffer.</param>
        internal VertexArrayObjectBuilder2(
            PrimitiveType primitiveType,
            (int count, uint[] data) indexSpec,
            (BufferUsageHint usage, int elementCount, T1[] data) bufferSpec1,
            (BufferUsageHint usage, int elementCount, T2[] data) bufferSpec2,
            (BufferUsageHint usage, int elementCount, T3[] data) bufferSpec3)
        {
            this.primitiveType = primitiveType;
        }

        /// <summary>
        /// Sets the index buffer to be included in the built VAO.
        /// </summary>
        /// <param name="data">The data with which to populate the buffer.</param>
        /// <returns>The updated builder.</returns>
        public VertexArrayObjectBuilder2<T1, T2, T3> WithIndex(uint[] data)
        {
            this.indexSpec = (data.Length, data);
            return this;
        }

        /// <summary>
        /// Sets the index buffer to be included in the built VAO.
        /// </summary>
        /// <param name="capacity">The size of the index buffer.</param>
        /// <returns>The updated builder.</returns>
        public VertexArrayObjectBuilder2<T1, T2, T3> WithIndex(int capacity)
        {
            this.indexSpec = (capacity, null);
            return this;
        }

        /// <summary>
        /// Specifies that the built VAO should be explicitly synchronized, with any pending changes flushed on each draw call.
        /// <para/>
        /// Specifically, means that the created VAO will be a <see cref="SynchronizedVao{T1, T2, T3}"/> instance.
        /// </summary>
        /// <returns>The updated builder.</returns>
        public SynchronizedVertexArrayObjectBuilder<T1, T2, T3> Synchronized()
        {
            return new SynchronizedVertexArrayObjectBuilder<T1, T2, T3>(this);
        }

        /// <summary>
        /// Builds a new <see cref="IVertexArrayObject{T1, T2, T3}"/> instance based on the state of the builder.
        /// </summary>
        /// <returns>The built VAO.</returns>
        public IVertexArrayObject<T1, T2, T3> Build()
        {
            return new GlVertexArrayObject<T1, T2, T3>(
                primitiveType,
                bufferSpec1,
                bufferSpec2,
                bufferSpec3,
                indexSpec);
        }
    }

    /// <summary>
    /// Builder class for <see cref="GlVertexArrayObject{T1}"/> objects that presents a fluent-ish interface.
    /// </summary>
    /// <typeparam name="T1">The type of data to be stored in the 1st buffer.</typeparam>
    /// <remarks>
    /// Useful for setting up a VAO before the OpenGL context has initialized.
    /// </remarks>
    public sealed class SynchronizedVertexArrayObjectBuilder<T1>
        where T1 : struct
    {
        private readonly VertexArrayObjectBuilder2<T1> innerBuilder;

        /// <summary>
        /// Initializes a new instance of the <see cref="SynchronizedVertexArrayObjectBuilder{T1}"/> class.
        /// </summary>
        /// <param name="innerBuilder">The builder wrapped by this builder.</param>
        public SynchronizedVertexArrayObjectBuilder(VertexArrayObjectBuilder2<T1> innerBuilder)
        {
            this.innerBuilder = innerBuilder;
        }

        /// <summary>
        /// Builds a new <see cref="IVertexArrayObject{T1}"/> instance based on the state of the builder.
        /// </summary>
        /// <returns>The built VAO.</returns>
        public IVertexArrayObject<T1> Build()
        {
            return new SynchronizedVao<T1>(innerBuilder.Build());
        }
    }

    /// <summary>
    /// Builder class for <see cref="GlVertexArrayObject{T1, T2}"/> objects that presents a fluent-ish interface.
    /// </summary>
    /// <typeparam name="T1">The type of data to be stored in the 1st buffer.</typeparam>
    /// <typeparam name="T2">The type of data to be stored in the 2nd buffer.</typeparam>
    /// <remarks>
    /// Useful for setting up a VAO before the OpenGL context has initialized.
    /// </remarks>
    public sealed class SynchronizedVertexArrayObjectBuilder<T1, T2>
        where T1 : struct
        where T2 : struct
    {
        private readonly VertexArrayObjectBuilder2<T1, T2> innerBuilder;

        /// <summary>
        /// Initializes a new instance of the <see cref="SynchronizedVertexArrayObjectBuilder{T1, T2}"/> class.
        /// </summary>
        /// <param name="innerBuilder">The builder wrapped by this builder.</param>
        public SynchronizedVertexArrayObjectBuilder(VertexArrayObjectBuilder2<T1, T2> innerBuilder)
        {
            this.innerBuilder = innerBuilder;
        }

        /// <summary>
        /// Builds a new <see cref="IVertexArrayObject{T1, T2}"/> instance based on the state of the builder.
        /// </summary>
        /// <returns>The built VAO.</returns>
        public IVertexArrayObject<T1, T2> Build()
        {
            return new SynchronizedVao<T1, T2>(innerBuilder.Build());
        }
    }

    /// <summary>
    /// Builder class for <see cref="GlVertexArrayObject{T1, T2, T3}"/> objects that presents a fluent-ish interface.
    /// </summary>
    /// <typeparam name="T1">The type of data to be stored in the 1st buffer.</typeparam>
    /// <typeparam name="T2">The type of data to be stored in the 2nd buffer.</typeparam>
    /// <typeparam name="T3">The type of data to be stored in the 3rd buffer.</typeparam>
    /// <remarks>
    /// Useful for setting up a VAO before the OpenGL context has initialized.
    /// </remarks>
    public sealed class SynchronizedVertexArrayObjectBuilder<T1, T2, T3>
        where T1 : struct
        where T2 : struct
        where T3 : struct
    {
        private readonly VertexArrayObjectBuilder2<T1, T2, T3> innerBuilder;

        /// <summary>
        /// Initializes a new instance of the <see cref="SynchronizedVertexArrayObjectBuilder{T1, T2, T3}"/> class.
        /// </summary>
        /// <param name="innerBuilder">The builder wrapped by this builder.</param>
        public SynchronizedVertexArrayObjectBuilder(VertexArrayObjectBuilder2<T1, T2, T3> innerBuilder)
        {
            this.innerBuilder = innerBuilder;
        }

        /// <summary>
        /// Builds a new <see cref="IVertexArrayObject{T1, T2, T3}"/> instance based on the state of the builder.
        /// </summary>
        /// <returns>The built VAO.</returns>
        public IVertexArrayObject<T1, T2, T3> Build()
        {
            return new SynchronizedVao<T1, T2, T3>(innerBuilder.Build());
        }
    }
}
#pragma warning restore SA1402