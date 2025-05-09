﻿#pragma warning disable IDE0290
using OpenTK.Graphics.OpenGL;

namespace MyOTKE.Core;

/// <summary>
/// Builder class for <see cref="GlVertexArrayObject"/> objects that presents a fluent-ish interface.
/// </summary>
public sealed class VertexArrayObjectBuilder
{
    private readonly PrimitiveType primitiveType;
    private (BufferUsageHint usage, int count, uint[] data) indexSpec;

    /// <summary>
    /// Initializes a new instance of the <see cref="VertexArrayObjectBuilder"/> class.
    /// </summary>
    /// <param name="primitiveType">The type of primitive data to be stored in the built VAO.</param>
    public VertexArrayObjectBuilder(
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
    public VertexArrayObjectBuilder<T> WithNewAttributeBuffer<T>(BufferUsageHint bufferUsage, T[] data)
        where T : struct
    {
        return new VertexArrayObjectBuilder<T>(
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
    public VertexArrayObjectBuilder<T> WithNewAttributeBuffer<T>(BufferUsageHint bufferUsage, int size)
        where T : struct
    {
        return new VertexArrayObjectBuilder<T>(
            primitiveType,
            indexSpec,
            (bufferUsage, size, null));
    }

    /// <summary>
    /// Sets the index buffer to be included in the built VAO.
    /// </summary>
    /// <param name="bufferUsage">The usage type for the buffer.</param>
    /// <param name="data">The data with which to populate the buffer.</param>
    /// <returns>The updated builder.</returns>
    public VertexArrayObjectBuilder WithNewIndexBuffer(BufferUsageHint bufferUsage, uint[] data)
    {
        this.indexSpec = (bufferUsage, data.Length, data);
        return this;
    }

    /// <summary>
    /// Sets the index buffer to be included in the built VAO.
    /// </summary>
    /// <param name="bufferUsage">The usage type for the buffer.</param>
    /// <param name="capacity">The size of the index buffer.</param>
    /// <returns>The updated builder.</returns>
    public VertexArrayObjectBuilder WithNewIndexBuffer(BufferUsageHint bufferUsage, int capacity)
    {
        this.indexSpec = (bufferUsage, capacity, null);
        return this;
    }
}

/// <summary>
/// Builder class for <see cref="GlVertexArrayObject{T1}"/> objects that presents a fluent-ish interface.
/// </summary>
/// <typeparam name="T1">The type of data to be stored in the 1st buffer.</typeparam>
public sealed class VertexArrayObjectBuilder<T1>
    where T1 : struct
{
    private readonly PrimitiveType primitiveType;
    private readonly (BufferUsageHint usage, int elementCount, T1[] data) bufferSpec1;
    private (BufferUsageHint usage, int count, uint[] data) indexSpec;

    /// <summary>
    /// Initializes a new instance of the <see cref="VertexArrayObjectBuilder{T1}"/> class.
    /// </summary>
    /// <param name="primitiveType">The type of primitive data to be stored in the built VAO.</param>
    /// <param name="indexSpec">Specification for the index buffer.</param>
    /// <param name="bufferSpec1">Specification for the 1st buffer.</param>
    internal VertexArrayObjectBuilder(
        PrimitiveType primitiveType,
        (BufferUsageHint usage, int count, uint[] data) indexSpec,
        (BufferUsageHint usage, int elementCount, T1[] data) bufferSpec1)
    {
        this.primitiveType = primitiveType;
        this.indexSpec = indexSpec;
        this.bufferSpec1 = bufferSpec1;
    }

    /// <summary>
    /// Adds a new populated attribute buffer to be included in the built VAO.
    /// </summary>
    /// <typeparam name="T">The type of data to be stored in the buffer.</typeparam>
    /// <param name="bufferUsage">The usage type for the buffer.</param>
    /// <param name="data">The data with which to populate the buffer.</param>
    /// <returns>The updated builder.</returns>
    public VertexArrayObjectBuilder<T1, T> WithNewAttributeBuffer<T>(BufferUsageHint bufferUsage, T[] data)
        where T : struct
    {
        return new VertexArrayObjectBuilder<T1, T>(
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
    public VertexArrayObjectBuilder<T1, T> WithNewAttributeBuffer<T>(BufferUsageHint bufferUsage, int size)
        where T : struct
    {
        return new VertexArrayObjectBuilder<T1, T>(
            primitiveType,
            indexSpec,
            bufferSpec1,
            (bufferUsage, size, null));
    }

    /// <summary>
    /// Sets the index buffer to be included in the built VAO.
    /// </summary>
    /// <param name="bufferUsage">The usage type for the buffer.</param>
    /// <param name="data">The data with which to populate the buffer.</param>
    /// <returns>The updated builder.</returns>
    public VertexArrayObjectBuilder<T1> WithNewIndexBuffer(BufferUsageHint bufferUsage, uint[] data)
    {
        this.indexSpec = (bufferUsage, data.Length, data);
        return this;
    }

    /// <summary>
    /// Sets the index buffer to be included in the built VAO.
    /// </summary>
    /// <param name="bufferUsage">The usage type for the buffer.</param>
    /// <param name="capacity">The size of the index buffer.</param>
    /// <returns>The updated builder.</returns>
    public VertexArrayObjectBuilder<T1> WithNewIndexBuffer(BufferUsageHint bufferUsage, int capacity)
    {
        this.indexSpec = (bufferUsage, capacity, null);
        return this;
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
public sealed class VertexArrayObjectBuilder<T1, T2>
    where T1 : struct
    where T2 : struct
{
    private readonly PrimitiveType primitiveType;
    private readonly (BufferUsageHint usage, int elementCount, T1[] data) bufferSpec1;
    private readonly (BufferUsageHint usage, int elementCount, T2[] data) bufferSpec2;
    private (BufferUsageHint usage, int count, uint[] data) indexSpec;

    /// <summary>
    /// Initializes a new instance of the <see cref="VertexArrayObjectBuilder{T1, T2}"/> class.
    /// </summary>
    /// <param name="primitiveType">The type of primitive data to be stored in the built VAO.</param>
    /// <param name="indexSpec">Specification for the index buffer.</param>
    /// <param name="bufferSpec1">Specification for the 1st buffer.</param>
    /// <param name="bufferSpec2">Specification for the 2nd buffer.</param>
    internal VertexArrayObjectBuilder(
        PrimitiveType primitiveType,
        (BufferUsageHint usage, int count, uint[] data) indexSpec,
        (BufferUsageHint usage, int elementCount, T1[] data) bufferSpec1,
        (BufferUsageHint usage, int elementCount, T2[] data) bufferSpec2)
    {
        this.primitiveType = primitiveType;
        this.indexSpec = indexSpec;
        this.bufferSpec1 = bufferSpec1;
        this.bufferSpec2 = bufferSpec2;
    }

    /// <summary>
    /// Adds a new populated attribute buffer to be included in the built VAO.
    /// </summary>
    /// <typeparam name="T">The type of data to be stored in the buffer.</typeparam>
    /// <param name="bufferUsage">The usage type for the buffer.</param>
    /// <param name="data">The data with which to populate the buffer.</param>
    /// <returns>The updated builder.</returns>
    public VertexArrayObjectBuilder<T1, T2, T> WithNewAttributeBuffer<T>(BufferUsageHint bufferUsage, T[] data)
        where T : struct
    {
        return new VertexArrayObjectBuilder<T1, T2, T>(
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
    public VertexArrayObjectBuilder<T1, T2, T> WithNewAttributeBuffer<T>(BufferUsageHint bufferUsage, int size)
        where T : struct
    {
        return new VertexArrayObjectBuilder<T1, T2, T>(
            primitiveType,
            indexSpec,
            bufferSpec1,
            bufferSpec2,
            (bufferUsage, size, null));
    }

    /// <summary>
    /// Sets the index buffer to be included in the built VAO.
    /// </summary>
    /// <param name="bufferUsage">The usage type for the buffer.</param>
    /// <param name="data">The data with which to populate the buffer.</param>
    /// <returns>The updated builder.</returns>
    public VertexArrayObjectBuilder<T1, T2> WithNewIndexBuffer(BufferUsageHint bufferUsage, uint[] data)
    {
        this.indexSpec = (bufferUsage, data.Length, data);
        return this;
    }

    /// <summary>
    /// Sets the index buffer to be included in the built VAO.
    /// </summary>
    /// <param name="bufferUsage">The usage type for the buffer.</param>
    /// <param name="capacity">The size of the index buffer.</param>
    /// <returns>The updated builder.</returns>
    public VertexArrayObjectBuilder<T1, T2> WithNewIndexBuffer(BufferUsageHint bufferUsage, int capacity)
    {
        this.indexSpec = (bufferUsage, capacity, null);
        return this;
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
public sealed class VertexArrayObjectBuilder<T1, T2, T3>
    where T1 : struct
    where T2 : struct
    where T3 : struct
{
    private readonly PrimitiveType primitiveType;
    private readonly (BufferUsageHint usage, int elementCount, T1[] data) bufferSpec1;
    private readonly (BufferUsageHint usage, int elementCount, T2[] data) bufferSpec2;
    private readonly (BufferUsageHint usage, int elementCount, T3[] data) bufferSpec3;
    private (BufferUsageHint usage, int count, uint[] data) indexSpec;

    /// <summary>
    /// Initializes a new instance of the <see cref="VertexArrayObjectBuilder{T1, T2, T3}"/> class.
    /// </summary>
    /// <param name="primitiveType">The type of primitive data to be stored in the built VAO.</param>
    /// <param name="indexSpec">Specification for the index buffer.</param>
    /// <param name="bufferSpec1">Specification for the 1st buffer.</param>
    /// <param name="bufferSpec2">Specification for the 2nd buffer.</param>
    /// <param name="bufferSpec3">Specification for the 3rd buffer.</param>
    internal VertexArrayObjectBuilder(
        PrimitiveType primitiveType,
        (BufferUsageHint usage, int count, uint[] data) indexSpec,
        (BufferUsageHint usage, int elementCount, T1[] data) bufferSpec1,
        (BufferUsageHint usage, int elementCount, T2[] data) bufferSpec2,
        (BufferUsageHint usage, int elementCount, T3[] data) bufferSpec3)
    {
        this.primitiveType = primitiveType;
        this.indexSpec = indexSpec;
        this.bufferSpec1 = bufferSpec1;
        this.bufferSpec2 = bufferSpec2;
        this.bufferSpec3 = bufferSpec3;
    }

    /// <summary>
    /// Sets the index buffer to be included in the built VAO.
    /// </summary>
    /// <param name="bufferUsage">The usage type for the buffer.</param>
    /// <param name="data">The data with which to populate the buffer.</param>
    /// <returns>The updated builder.</returns>
    public VertexArrayObjectBuilder<T1, T2, T3> WithNewIndexBuffer(BufferUsageHint bufferUsage, uint[] data)
    {
        this.indexSpec = (bufferUsage, data.Length, data);
        return this;
    }

    /// <summary>
    /// Sets the index buffer to be included in the built VAO.
    /// </summary>
    /// <param name="bufferUsage">The usage type for the buffer.</param>
    /// <param name="capacity">The size of the index buffer.</param>
    /// <returns>The updated builder.</returns>
    public VertexArrayObjectBuilder<T1, T2, T3> WithNewIndexBuffer(BufferUsageHint bufferUsage, int capacity)
    {
        this.indexSpec = (bufferUsage, capacity, null);
        return this;
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
#pragma warning restore IDE0290
