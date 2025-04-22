#pragma warning disable IDE0290
using OpenTK.Graphics.OpenGL;
using System;

namespace MyOTKE.Core;

/// <summary>
/// Implementation of <see cref="IVertexArrayObject{T1}"/> that just stores buffer content in memory, for testing purposes.
/// </summary>
/// <typeparam name="T1">The type of the 1st buffer.</typeparam>
public class MemoryVertexArrayObject<T1> : IVertexArrayObject<T1>
    where T1 : struct
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MemoryVertexArrayObject{T1}"/> class.
    /// </summary>
    /// <param name="attributeBufferSpec1">Spec for the 1st buffer in this VAO.</param>
    /// <param name="indexSpec">Spec for the index of this VAO.</param>
    public MemoryVertexArrayObject(
        (BufferUsageHint usage, int capacity, T1[] data) attributeBufferSpec1,
        (int capacity, uint[] data) indexSpec)
    {
        AttributeBuffer1 = new MemoryVertexBufferObject<T1>(attributeBufferSpec1.capacity, attributeBufferSpec1.data);
        IndexBuffer = new MemoryVertexBufferObject<uint>(indexSpec.capacity, indexSpec.data);
    }

    /// <inheritdoc />
    IVertexBufferObject<uint> IVertexArrayObject<T1>.IndexBuffer => IndexBuffer;

    /// <summary>
    /// Gets the index buffer of this VAO.
    /// </summary>
    public MemoryVertexBufferObject<uint> IndexBuffer { get; }

    /// <inheritdoc />
    IVertexBufferObject<T1> IVertexArrayObject<T1>.AttributeBuffer1 => AttributeBuffer1;

    /// <summary>
    /// Gets the 1st attribute buffer of this VAO.
    /// </summary>
    public MemoryVertexBufferObject<T1> AttributeBuffer1 { get; }

    /// <inheritdoc />
    public void Draw(int count) => throw new NotImplementedException();
}

/// <summary>
/// Implementation of <see cref="IVertexArrayObject{T1, T2}"/> that just stores buffer content in memory, for testing purposes.
/// </summary>
/// <typeparam name="T1">The type of the 1st buffer.</typeparam>
/// <typeparam name="T2">The type of the 2nd buffer.</typeparam>
public class MemoryVertexArrayObject<T1, T2> : IVertexArrayObject<T1, T2>
    where T1 : struct
    where T2 : struct
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MemoryVertexArrayObject{T1, T2}"/> class.
    /// </summary>
    /// <param name="attributeBufferSpec1">Spec for the 1st buffer in this VAO.</param>
    /// <param name="attributeBufferSpec2">Spec for the 2nd buffer in this VAO.</param>
    /// <param name="indexSpec">Spec for the index of this VAO.</param>
    public MemoryVertexArrayObject(
        (BufferUsageHint usage, int capacity, T1[] data) attributeBufferSpec1,
        (BufferUsageHint usage, int capacity, T2[] data) attributeBufferSpec2,
        (int capacity, uint[] data) indexSpec)
    {
        AttributeBuffer1 = new MemoryVertexBufferObject<T1>(attributeBufferSpec1.capacity, attributeBufferSpec1.data);
        AttributeBuffer2 = new MemoryVertexBufferObject<T2>(attributeBufferSpec2.capacity, attributeBufferSpec2.data);
        IndexBuffer = new MemoryVertexBufferObject<uint>(indexSpec.capacity, indexSpec.data);
    }

    /// <inheritdoc />
    IVertexBufferObject<uint> IVertexArrayObject<T1, T2>.IndexBuffer => IndexBuffer;

    /// <summary>
    /// Gets the index buffer of this VAO.
    /// </summary>
    public MemoryVertexBufferObject<uint> IndexBuffer { get; }

    /// <inheritdoc />
    IVertexBufferObject<T1> IVertexArrayObject<T1, T2>.AttributeBuffer1 => AttributeBuffer1;

    /// <summary>
    /// Gets the 1st attribute buffer of this VAO.
    /// </summary>
    public MemoryVertexBufferObject<T1> AttributeBuffer1 { get; }

    /// <inheritdoc />
    IVertexBufferObject<T2> IVertexArrayObject<T1, T2>.AttributeBuffer2 => AttributeBuffer2;

    /// <summary>
    /// Gets the 2nd attribute buffer of this VAO.
    /// </summary>
    public MemoryVertexBufferObject<T2> AttributeBuffer2 { get; }

    /// <inheritdoc />
    public void Draw(int count) => throw new NotImplementedException();
}

/// <summary>
/// Implementation of <see cref="IVertexArrayObject{T1, T2, T3}"/> that just stores buffer content in memory, for testing purposes.
/// </summary>
/// <typeparam name="T1">The type of the 1st buffer.</typeparam>
/// <typeparam name="T2">The type of the 2nd buffer.</typeparam>
/// <typeparam name="T3">The type of the 3rd buffer.</typeparam>
public class MemoryVertexArrayObject<T1, T2, T3> : IVertexArrayObject<T1, T2, T3>
    where T1 : struct
    where T2 : struct
    where T3 : struct
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MemoryVertexArrayObject{T1, T2, T3}"/> class.
    /// </summary>
    /// <param name="attributeBufferSpec1">Spec for the 1st buffer in this VAO.</param>
    /// <param name="attributeBufferSpec2">Spec for the 2nd buffer in this VAO.</param>
    /// <param name="attributeBufferSpec3">Spec for the 3rd buffer in this VAO.</param>
    /// <param name="indexSpec">Spec for the index of this VAO.</param>
    public MemoryVertexArrayObject(
        (BufferUsageHint usage, int capacity, T1[] data) attributeBufferSpec1,
        (BufferUsageHint usage, int capacity, T2[] data) attributeBufferSpec2,
        (BufferUsageHint usage, int capacity, T3[] data) attributeBufferSpec3,
        (int capacity, uint[] data) indexSpec)
    {
        AttributeBuffer1 = new MemoryVertexBufferObject<T1>(attributeBufferSpec1.capacity, attributeBufferSpec1.data);
        AttributeBuffer2 = new MemoryVertexBufferObject<T2>(attributeBufferSpec2.capacity, attributeBufferSpec2.data);
        AttributeBuffer3 = new MemoryVertexBufferObject<T3>(attributeBufferSpec3.capacity, attributeBufferSpec3.data);
        IndexBuffer = new MemoryVertexBufferObject<uint>(indexSpec.capacity, indexSpec.data);
    }

    /// <inheritdoc />
    IVertexBufferObject<uint> IVertexArrayObject<T1, T2, T3>.IndexBuffer => IndexBuffer;

    /// <summary>
    /// Gets the index buffer of this VAO.
    /// </summary>
    public MemoryVertexBufferObject<uint> IndexBuffer { get; }

    /// <inheritdoc />
    IVertexBufferObject<T1> IVertexArrayObject<T1, T2, T3>.AttributeBuffer1 => AttributeBuffer1;

    /// <summary>
    /// Gets the 1st attribute buffer of this VAO.
    /// </summary>
    public MemoryVertexBufferObject<T1> AttributeBuffer1 { get; }

    /// <inheritdoc />
    IVertexBufferObject<T2> IVertexArrayObject<T1, T2, T3>.AttributeBuffer2 => AttributeBuffer2;

    /// <summary>
    /// Gets the 2nd attribute buffer of this VAO.
    /// </summary>
    public MemoryVertexBufferObject<T2> AttributeBuffer2 { get; }

    /// <inheritdoc />
    IVertexBufferObject<T3> IVertexArrayObject<T1, T2, T3>.AttributeBuffer3 => AttributeBuffer3;

    /// <summary>
    /// Gets the 3rd attribute buffer of this VAO.
    /// </summary>
    public MemoryVertexBufferObject<T3> AttributeBuffer3 { get; }

    /// <inheritdoc />
    public void Draw(int count) => throw new NotImplementedException();
}
#pragma warning restore IDE0290