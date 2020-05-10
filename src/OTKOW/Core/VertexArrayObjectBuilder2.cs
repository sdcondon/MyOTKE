using OpenToolkit.Graphics.OpenGL;
using OTKOW.Core.VaoDecorators;
using System;
using System.Collections.Generic;

namespace OTKOW.Core
{
    /// <summary>
    /// Builder class for <see cref="GlVertexArrayObject"/> objects that presents a fluent-ish interface.
    /// </summary>
    /// <remarks>
    /// Useful for setting up a VAO before the OpenGL context has initialized.
    /// </remarks>
    public sealed class VertexArrayObjectBuilder2<T1>
    {
        private readonly PrimitiveType primitiveType;
        private readonly List<(BufferUsageHint usage, Type elementType, int elementCount, Array data)> bufferSpecs = new List<(BufferUsageHint, Type, int, Array)>();
        private (int count, uint[] data) indexSpec;
        private Func<PrimitiveType, List<(BufferUsageHint, Type, int, Array)>, (int count, uint[] data), IVertexArrayObject> build;

        /// <summary>
        /// Initializes a new instance of the <see cref="VertexArrayObjectBuilder"/> class.
        /// </summary>
        /// <param name="primitiveType">The type of primitive data to be stored in the built VAO.</param>
        public VertexArrayObjectBuilder2(PrimitiveType primitiveType)
        {
            this.primitiveType = primitiveType;
            this.build = (primType, bufferSpecs, indexSpec) => new GlVertexArrayObject(primType, bufferSpecs, indexSpec);
        }

        /// <summary>
        /// Adds a new populated attribute buffer to be included in the built VAO.
        /// </summary>
        /// <param name="bufferUsage">The usage type for the buffer.</param>
        /// <param name="data">The data with which to populate the buffer.</param>
        /// <returns>The updated builder.</returns>
        public VertexArrayObjectBuilder WithAttributeBuffer(BufferUsageHint bufferUsage, Array data)
        {
            this.bufferSpecs.Add((bufferUsage, data.GetType().GetElementType(), data.Length, data));
            return this;
        }

        /// <summary>
        /// Adds a new empty attribute buffer to be included in the built VAO.
        /// </summary>
        /// <typeparam name="T">The type of data to be stored in the buffer.</typeparam>
        /// <param name="bufferUsage">The usage type for the buffer.</param>
        /// <param name="size">The size of the buffer, in instances of <see typeparamref="T"/>.</param>
        /// <returns>The updated builder.</returns>
        public VertexArrayObjectBuilder WithAttributeBuffer<T>(BufferUsageHint bufferUsage, int size)
        {
            this.bufferSpecs.Add((bufferUsage, typeof(T), size, null));
            return this;
        }

        /// <summary>
        /// Sets the index buffer to be included in the built VAO.
        /// </summary>
        /// <param name="data">The data with which to populate the buffer.</param>
        /// <returns>The updated builder.</returns>
        public VertexArrayObjectBuilder WithIndex(uint[] data)
        {
            this.indexSpec = (data.Length, data);
            return this;
        }

        /// <summary>
        /// Sets the index buffer to be included in the built VAO.
        /// </summary>
        /// <param name="capacity">The size of the index buffer.</param>
        /// <returns>The updated builder.</returns>
        public VertexArrayObjectBuilder WithIndex(int capacity)
        {
            this.indexSpec = (capacity, null);
            return this;
        }

        /// <summary>
        /// Specifies that the built VAO should be explicitly synchronized, with any pending changes flushed on each draw call.
        /// <para/>
        /// Specifically, means that the created VAO will be a <see cref="SynchronizedVao"/> instance.
        /// </summary>
        /// <returns>The updated builder.</returns>
        public VertexArrayObjectBuilder Synchronized()
        {
            var innerBuild = build;
            build = (primType, bufferSpecs, indexSpec) => new SynchronizedVao(innerBuild(primType, bufferSpecs, indexSpec));
            return this;
        }

        /// <summary>
        /// Builds a new <see cref="GlVertexArrayObject"/> instance based on the state of the builder.
        /// </summary>
        /// <returns>The built VAO.</returns>
        public IVertexArrayObject Build()
        {
            return build(primitiveType, bufferSpecs, indexSpec);
        }
    }

    /// <summary>
    /// Builder class for <see cref="GlVertexArrayObject"/> objects that presents a fluent-ish interface.
    /// </summary>
    /// <remarks>
    /// Useful for setting up a VAO before the OpenGL context has initialized.
    /// </remarks>
    public sealed class VertexArrayObjectBuilder2<T1, T2>
    {
        private readonly PrimitiveType primitiveType;
        private readonly List<(BufferUsageHint usage, Type elementType, int elementCount, Array data)> bufferSpecs = new List<(BufferUsageHint, Type, int, Array)>();
        private (int count, uint[] data) indexSpec;
        private Func<PrimitiveType, List<(BufferUsageHint, Type, int, Array)>, (int count, uint[] data), IVertexArrayObject> build;

        /// <summary>
        /// Initializes a new instance of the <see cref="VertexArrayObjectBuilder"/> class.
        /// </summary>
        /// <param name="primitiveType">The type of primitive data to be stored in the built VAO.</param>
        public VertexArrayObjectBuilder2(PrimitiveType primitiveType)
        {
            this.primitiveType = primitiveType;
            this.build = (primType, bufferSpecs, indexSpec) => new GlVertexArrayObject(primType, bufferSpecs, indexSpec);
        }

        /// <summary>
        /// Adds a new populated attribute buffer to be included in the built VAO.
        /// </summary>
        /// <param name="bufferUsage">The usage type for the buffer.</param>
        /// <param name="data">The data with which to populate the buffer.</param>
        /// <returns>The updated builder.</returns>
        public VertexArrayObjectBuilder WithAttributeBuffer(BufferUsageHint bufferUsage, Array data)
        {
            this.bufferSpecs.Add((bufferUsage, data.GetType().GetElementType(), data.Length, data));
            return this;
        }

        /// <summary>
        /// Adds a new empty attribute buffer to be included in the built VAO.
        /// </summary>
        /// <typeparam name="T">The type of data to be stored in the buffer.</typeparam>
        /// <param name="bufferUsage">The usage type for the buffer.</param>
        /// <param name="size">The size of the buffer, in instances of <see typeparamref="T"/>.</param>
        /// <returns>The updated builder.</returns>
        public VertexArrayObjectBuilder WithAttributeBuffer<T>(BufferUsageHint bufferUsage, int size)
        {
            this.bufferSpecs.Add((bufferUsage, typeof(T), size, null));
            return this;
        }

        /// <summary>
        /// Sets the index buffer to be included in the built VAO.
        /// </summary>
        /// <param name="data">The data with which to populate the buffer.</param>
        /// <returns>The updated builder.</returns>
        public VertexArrayObjectBuilder WithIndex(uint[] data)
        {
            this.indexSpec = (data.Length, data);
            return this;
        }

        /// <summary>
        /// Sets the index buffer to be included in the built VAO.
        /// </summary>
        /// <param name="capacity">The size of the index buffer.</param>
        /// <returns>The updated builder.</returns>
        public VertexArrayObjectBuilder WithIndex(int capacity)
        {
            this.indexSpec = (capacity, null);
            return this;
        }

        /// <summary>
        /// Specifies that the built VAO should be explicitly synchronized, with any pending changes flushed on each draw call.
        /// <para/>
        /// Specifically, means that the created VAO will be a <see cref="SynchronizedVao"/> instance.
        /// </summary>
        /// <returns>The updated builder.</returns>
        public VertexArrayObjectBuilder Synchronized()
        {
            var innerBuild = build;
            build = (primType, bufferSpecs, indexSpec) => new SynchronizedVao(innerBuild(primType, bufferSpecs, indexSpec));
            return this;
        }

        /// <summary>
        /// Builds a new <see cref="GlVertexArrayObject"/> instance based on the state of the builder.
        /// </summary>
        /// <returns>The built VAO.</returns>
        public IVertexArrayObject Build()
        {
            return build(primitiveType, bufferSpecs, indexSpec);
        }
    }

    /// <summary>
    /// Builder class for <see cref="GlVertexArrayObject"/> objects that presents a fluent-ish interface.
    /// </summary>
    /// <remarks>
    /// Useful for setting up a VAO before the OpenGL context has initialized.
    /// </remarks>
    public sealed class VertexArrayObjectBuilder2<T1, T2, T3>
    {
        private readonly PrimitiveType primitiveType;
        private readonly List<(BufferUsageHint usage, Type elementType, int elementCount, Array data)> bufferSpecs = new List<(BufferUsageHint, Type, int, Array)>();
        private (int count, uint[] data) indexSpec;
        private Func<PrimitiveType, List<(BufferUsageHint, Type, int, Array)>, (int count, uint[] data), IVertexArrayObject> build;

        /// <summary>
        /// Initializes a new instance of the <see cref="VertexArrayObjectBuilder"/> class.
        /// </summary>
        /// <param name="primitiveType">The type of primitive data to be stored in the built VAO.</param>
        public VertexArrayObjectBuilder2(PrimitiveType primitiveType)
        {
            this.primitiveType = primitiveType;
            this.build = (primType, bufferSpecs, indexSpec) => new GlVertexArrayObject(primType, bufferSpecs, indexSpec);
        }

        /// <summary>
        /// Adds a new populated attribute buffer to be included in the built VAO.
        /// </summary>
        /// <param name="bufferUsage">The usage type for the buffer.</param>
        /// <param name="data">The data with which to populate the buffer.</param>
        /// <returns>The updated builder.</returns>
        public VertexArrayObjectBuilder WithAttributeBuffer(BufferUsageHint bufferUsage, Array data)
        {
            this.bufferSpecs.Add((bufferUsage, data.GetType().GetElementType(), data.Length, data));
            return this;
        }

        /// <summary>
        /// Adds a new empty attribute buffer to be included in the built VAO.
        /// </summary>
        /// <typeparam name="T">The type of data to be stored in the buffer.</typeparam>
        /// <param name="bufferUsage">The usage type for the buffer.</param>
        /// <param name="size">The size of the buffer, in instances of <see typeparamref="T"/>.</param>
        /// <returns>The updated builder.</returns>
        public VertexArrayObjectBuilder WithAttributeBuffer<T>(BufferUsageHint bufferUsage, int size)
        {
            this.bufferSpecs.Add((bufferUsage, typeof(T), size, null));
            return this;
        }

        /// <summary>
        /// Sets the index buffer to be included in the built VAO.
        /// </summary>
        /// <param name="data">The data with which to populate the buffer.</param>
        /// <returns>The updated builder.</returns>
        public VertexArrayObjectBuilder WithIndex(uint[] data)
        {
            this.indexSpec = (data.Length, data);
            return this;
        }

        /// <summary>
        /// Sets the index buffer to be included in the built VAO.
        /// </summary>
        /// <param name="capacity">The size of the index buffer.</param>
        /// <returns>The updated builder.</returns>
        public VertexArrayObjectBuilder WithIndex(int capacity)
        {
            this.indexSpec = (capacity, null);
            return this;
        }

        /// <summary>
        /// Specifies that the built VAO should be explicitly synchronized, with any pending changes flushed on each draw call.
        /// <para/>
        /// Specifically, means that the created VAO will be a <see cref="SynchronizedVao"/> instance.
        /// </summary>
        /// <returns>The updated builder.</returns>
        public VertexArrayObjectBuilder Synchronized()
        {
            var innerBuild = build;
            build = (primType, bufferSpecs, indexSpec) => new SynchronizedVao(innerBuild(primType, bufferSpecs, indexSpec));
            return this;
        }

        /// <summary>
        /// Builds a new <see cref="GlVertexArrayObject"/> instance based on the state of the builder.
        /// </summary>
        /// <returns>The built VAO.</returns>
        public IVertexArrayObject Build()
        {
            return build(primitiveType, bufferSpecs, indexSpec);
        }
    }

}
