#pragma warning disable SA1402
using OpenTK.Graphics.OpenGL;
using System;

namespace MyOTKE.Core;

/// <summary>
/// Represents an OpenGL vertex array object.
/// <para />
/// See https://www.khronos.org/opengl/wiki/Vertex_Specification#Vertex_Array_Object.
/// </summary>
/// <typeparam name="T1">The type of the 1st attribute buffer.</typeparam>
public sealed class GlVertexArrayObject<T1> : IVertexArrayObject<T1>, IDisposable
    where T1 : struct
{
    private readonly int id;
    private readonly PrimitiveType primitiveType;
    private readonly GlVertexBufferObject<uint> indexBuffer;
    private readonly GlVertexBufferObject<T1> attributeBuffer1;

    /// <summary>
    /// Initializes a new instance of the <see cref="GlVertexArrayObject{T1}"/> class. SIDE EFFECT: new VAO will be bound.
    /// </summary>
    /// <param name="primitiveType">OpenGL primitive type.</param>
    /// <param name="attributeBufferSpec1">Spec for the 1st buffer in this VAO.</param>
    /// <param name="indexSpec">The spec for the index of this VAO.</param>
    internal GlVertexArrayObject(
        PrimitiveType primitiveType,
        (BufferUsageHint usage, int capacity, T1[] data) attributeBufferSpec1,
        (BufferUsageHint usage, int capacity, uint[] data) indexSpec)
    {
        // Record primitive type for use in draw calls, create and bind the VAO
        this.primitiveType = primitiveType;
        this.id = GL.GenVertexArray(); // superbible uses CreateVertexArray?
        GlDebug.ThrowIfGlError("creating vertex array");
        GL.BindVertexArray(id);
        GlDebug.ThrowIfGlError("binding vertex array");

        // Set up the attribute buffers
        int k = 0;
        GlVertexBufferObject<T> MakeBuffer<T>((BufferUsageHint usage, int capacity, T[] data) attributeBufferSpec)
            where T : struct
        {
            var buffer = new GlVertexBufferObject<T>(
                BufferTarget.ArrayBuffer,
                attributeBufferSpec.usage,
                attributeBufferSpec.capacity,
                attributeBufferSpec.data);
            for (uint j = 0; j < buffer.Attributes.Length; j++, k++)
            {
                var attribute = buffer.Attributes[j];
                GL.EnableVertexAttribArray(k);
                GL.VertexAttribPointer(
                    index: k, // must match the layout in the shader
                    size: attribute.Multiple,
                    type: attribute.Type,
                    normalized: false,
                    stride: attribute.Stride,
                    pointer: attribute.Offset);
            }

            return buffer;
        }

        attributeBuffer1 = MakeBuffer(attributeBufferSpec1);

        // Establish element count & populate index buffer if there is one
        if (indexSpec.capacity > 0)
        {
            this.indexBuffer = new GlVertexBufferObject<uint>(BufferTarget.ElementArrayBuffer, indexSpec.usage, indexSpec.capacity, indexSpec.data);
        }
    }

    /// <summary>
    /// Finalizes an instance of the <see cref="GlVertexArrayObject{T1}"/> class.
    /// </summary>
    ~GlVertexArrayObject() => Dispose(false);

    /// <summary>
    /// Gets the number of vertices to be rendered.
    /// </summary>
    public int VertexCount => indexBuffer?.Capacity ?? attributeBuffer1.Capacity;

    /// <inheritdoc />
    public IVertexBufferObject<uint> IndexBuffer => this.indexBuffer;

    /// <inheritdoc />
    public IVertexBufferObject<T1> AttributeBuffer1 => attributeBuffer1;

    /// <inheritdoc />
    public void Draw(int count = -1)
    {
        GL.BindVertexArray(this.id);

        if (indexBuffer != null)
        {
            // There's an index buffer (which will be bound) - bind it and draw
            GL.DrawElements(
                mode: this.primitiveType,
                count: count == -1 ? this.VertexCount : count,
                type: DrawElementsType.UnsignedInt,
                indices: IntPtr.Zero);
            GlDebug.ThrowIfGlError("drawing elements");
        }
        else
        {
            // No index - so draw directly from attribute data
            GL.DrawArrays(
                mode: this.primitiveType,
                first: 0,
                count: count == -1 ? this.VertexCount : count);
            GlDebug.ThrowIfGlError("drawing arrays");
        }
    }

    /// <inheritdoc />
    public void Dispose() => Dispose(true);

    ////public void ResizeAttributeBuffer(int bufferIndex, int newSize)
    ////{
    ////    //var newId = Gl.GenBuffer();
    ////    //Gl.NamedBufferData(newId, (uint)(Marshal.SizeOf(elementType) * value), null, usage);
    ////    //Gl.CopyNamedBufferSubData(this.Id, newId, 0, 0, (uint)(Marshal.SizeOf(elementType) * count));
    ////    //count = value;
    ////}

    private void Dispose(bool disposing)
    {
        if (disposing)
        {
            attributeBuffer1.Dispose();

            if (indexBuffer != null)
            {
                indexBuffer.Dispose();
            }

            GC.SuppressFinalize(this);
        }

        GL.DeleteVertexArray(this.id);
        GlDebug.ThrowIfGlError("deleting vertex array");
    }
}

/// <summary>
/// Represents an OpenGL vertex array object.
/// <para />
/// See https://www.khronos.org/opengl/wiki/Vertex_Specification#Vertex_Array_Object.
/// </summary>
/// <typeparam name="T1">The type of the 1st attribute buffer.</typeparam>
/// <typeparam name="T2">The type of the 2nd attribute buffer.</typeparam>
public sealed class GlVertexArrayObject<T1, T2> : IVertexArrayObject<T1, T2>, IDisposable
    where T1 : struct
    where T2 : struct
{
    private readonly int id;
    private readonly PrimitiveType primitiveType;
    private readonly GlVertexBufferObject<uint> indexBuffer;
    private readonly GlVertexBufferObject<T1> attributeBuffer1;
    private readonly GlVertexBufferObject<T2> attributeBuffer2;

    /// <summary>
    /// Initializes a new instance of the <see cref="GlVertexArrayObject{T1, T2}"/> class. SIDE EFFECT: new VAO will be bound.
    /// </summary>
    /// <param name="primitiveType">OpenGL primitive type.</param>
    /// <param name="attributeBufferSpec1">Spec for the 1st buffer in this VAO.</param>
    /// <param name="attributeBufferSpec2">Spec for the 2nd buffer in this VAO.</param>
    /// <param name="indexSpec">The spec for the index of this VAO.</param>
    internal GlVertexArrayObject(
        PrimitiveType primitiveType,
        (BufferUsageHint usage, int capacity, T1[] data) attributeBufferSpec1,
        (BufferUsageHint usage, int capacity, T2[] data) attributeBufferSpec2,
        (BufferUsageHint usage, int capacity, uint[] data) indexSpec)
    {
        // Record primitive type for use in draw calls, create and bind the VAO
        this.primitiveType = primitiveType;
        this.id = GL.GenVertexArray(); // superbible uses CreateVertexArray?
        GlDebug.ThrowIfGlError("creating vertex array");
        GL.BindVertexArray(id);
        GlDebug.ThrowIfGlError("binding vertex array");

        // Set up the attribute buffers
        int k = 0;
        GlVertexBufferObject<T> MakeBuffer<T>((BufferUsageHint usage, int capacity, T[] data) attributeBufferSpec)
            where T : struct
        {
            var buffer = new GlVertexBufferObject<T>(
                BufferTarget.ArrayBuffer,
                attributeBufferSpec.usage,
                attributeBufferSpec.capacity,
                attributeBufferSpec.data);
            for (uint j = 0; j < buffer.Attributes.Length; j++, k++)
            {
                var attribute = buffer.Attributes[j];
                GL.EnableVertexAttribArray(k);
                GL.VertexAttribPointer(
                    index: k, // must match the layout in the shader
                    size: attribute.Multiple,
                    type: attribute.Type,
                    normalized: false,
                    stride: attribute.Stride,
                    pointer: attribute.Offset);
            }

            return buffer;
        }

        attributeBuffer1 = MakeBuffer(attributeBufferSpec1);
        attributeBuffer2 = MakeBuffer(attributeBufferSpec2);

        // Establish element count & populate index buffer if there is one
        if (indexSpec.capacity > 0)
        {
            this.indexBuffer = new GlVertexBufferObject<uint>(BufferTarget.ElementArrayBuffer, indexSpec.usage, indexSpec.capacity, indexSpec.data);
        }
    }

    /// <summary>
    /// Finalizes an instance of the <see cref="GlVertexArrayObject{T1, T2}"/> class.
    /// </summary>
    ~GlVertexArrayObject() => Dispose(false);

    /// <summary>
    /// Gets the number of vertices to be rendered.
    /// </summary>
    public int VertexCount => indexBuffer?.Capacity ?? attributeBuffer1.Capacity;

    /// <inheritdoc />
    public IVertexBufferObject<uint> IndexBuffer => this.indexBuffer;

    /// <inheritdoc />
    public IVertexBufferObject<T1> AttributeBuffer1 => attributeBuffer1;

    /// <inheritdoc />
    public IVertexBufferObject<T2> AttributeBuffer2 => attributeBuffer2;

    /// <inheritdoc />
    public void Draw(int count = -1)
    {
        GL.BindVertexArray(this.id);

        if (indexBuffer != null)
        {
            // There's an index buffer (which will be bound) - bind it and draw
            GL.DrawElements(
                mode: this.primitiveType,
                count: count == -1 ? this.VertexCount : count,
                type: DrawElementsType.UnsignedInt,
                indices: IntPtr.Zero);
            GlDebug.ThrowIfGlError("drawing elements");
        }
        else
        {
            // No index - so draw directly from attribute data
            GL.DrawArrays(
                mode: this.primitiveType,
                first: 0,
                count: count == -1 ? this.VertexCount : count);
            GlDebug.ThrowIfGlError("drawing arrays");
        }
    }

    /// <inheritdoc />
    public void Dispose() => Dispose(true);

    ////public void ResizeAttributeBuffer(int bufferIndex, int newSize)
    ////{
    ////    //var newId = Gl.GenBuffer();
    ////    //Gl.NamedBufferData(newId, (uint)(Marshal.SizeOf(elementType) * value), null, usage);
    ////    //Gl.CopyNamedBufferSubData(this.Id, newId, 0, 0, (uint)(Marshal.SizeOf(elementType) * count));
    ////    //count = value;
    ////}

    private void Dispose(bool disposing)
    {
        if (disposing)
        {
            attributeBuffer1.Dispose();
            attributeBuffer2.Dispose();

            if (indexBuffer != null)
            {
                indexBuffer.Dispose();
            }

            GC.SuppressFinalize(this);
        }

        GL.DeleteVertexArray(this.id);
        GlDebug.ThrowIfGlError("deleting vertex array");
    }
}

/// <summary>
/// Represents an OpenGL vertex array object.
/// <para />
/// See https://www.khronos.org/opengl/wiki/Vertex_Specification#Vertex_Array_Object.
/// </summary>
/// <typeparam name="T1">The type of the 1st attribute buffer.</typeparam>
/// <typeparam name="T2">The type of the 2nd attribute buffer.</typeparam>
/// <typeparam name="T3">The type of the 3rd attribute buffer.</typeparam>
public sealed class GlVertexArrayObject<T1, T2, T3> : IVertexArrayObject<T1, T2, T3>, IDisposable
    where T1 : struct
    where T2 : struct
    where T3 : struct
{
    private readonly int id;
    private readonly PrimitiveType primitiveType;
    private readonly GlVertexBufferObject<uint> indexBuffer;
    private readonly GlVertexBufferObject<T1> attributeBuffer1;
    private readonly GlVertexBufferObject<T2> attributeBuffer2;
    private readonly GlVertexBufferObject<T3> attributeBuffer3;

    /// <summary>
    /// Initializes a new instance of the <see cref="GlVertexArrayObject{T1, T2, T3}"/> class. SIDE EFFECT: new VAO will be bound.
    /// </summary>
    /// <param name="primitiveType">OpenGL primitive type.</param>
    /// <param name="attributeBufferSpec1">Spec for the 1st buffer in this VAO.</param>
    /// <param name="attributeBufferSpec2">Spec for the 2nd buffer in this VAO.</param>
    /// <param name="attributeBufferSpec3">Spec for the 3rd buffer in this VAO.</param>
    /// <param name="indexSpec">The spec for the index of this VAO.</param>
    internal GlVertexArrayObject(
        PrimitiveType primitiveType,
        (BufferUsageHint usage, int capacity, T1[] data) attributeBufferSpec1,
        (BufferUsageHint usage, int capacity, T2[] data) attributeBufferSpec2,
        (BufferUsageHint usage, int capacity, T3[] data) attributeBufferSpec3,
        (BufferUsageHint usage, int capacity, uint[] data) indexSpec)
    {
        // Record primitive type for use in draw calls, create and bind the VAO
        this.primitiveType = primitiveType;
        this.id = GL.GenVertexArray(); // superbible uses CreateVertexArray?
        GlDebug.ThrowIfGlError("creating vertex array");
        GL.BindVertexArray(id);
        GlDebug.ThrowIfGlError("binding vertex array");

        // Set up the attribute buffers
        int k = 0;
        GlVertexBufferObject<T> MakeBuffer<T>((BufferUsageHint usage, int capacity, T[] data) attributeBufferSpec)
            where T : struct
        {
            var buffer = new GlVertexBufferObject<T>(
                BufferTarget.ArrayBuffer,
                attributeBufferSpec.usage,
                attributeBufferSpec.capacity,
                attributeBufferSpec.data);
            for (uint j = 0; j < buffer.Attributes.Length; j++, k++)
            {
                var attribute = buffer.Attributes[j];
                GL.EnableVertexAttribArray(k);
                GL.VertexAttribPointer(
                    index: k, // must match the layout in the shader
                    size: attribute.Multiple,
                    type: attribute.Type,
                    normalized: false,
                    stride: attribute.Stride,
                    pointer: attribute.Offset);
            }

            return buffer;
        }

        attributeBuffer1 = MakeBuffer(attributeBufferSpec1);
        attributeBuffer2 = MakeBuffer(attributeBufferSpec2);
        attributeBuffer3 = MakeBuffer(attributeBufferSpec3);

        // Establish element count & populate index buffer if there is one
        if (indexSpec.capacity > 0)
        {
            this.indexBuffer = new GlVertexBufferObject<uint>(BufferTarget.ElementArrayBuffer, indexSpec.usage, indexSpec.capacity, indexSpec.data);
        }
    }

    /// <summary>
    /// Finalizes an instance of the <see cref="GlVertexArrayObject{T1, T2, T3}"/> class.
    /// </summary>
    ~GlVertexArrayObject() => Dispose(false);

    /// <summary>
    /// Gets the number of vertices to be rendered.
    /// </summary>
    public int VertexCount => indexBuffer?.Capacity ?? attributeBuffer1.Capacity;

    /// <inheritdoc />
    public IVertexBufferObject<uint> IndexBuffer => this.indexBuffer;

    /// <inheritdoc />
    public IVertexBufferObject<T1> AttributeBuffer1 => attributeBuffer1;

    /// <inheritdoc />
    public IVertexBufferObject<T2> AttributeBuffer2 => attributeBuffer2;

    /// <inheritdoc />
    public IVertexBufferObject<T3> AttributeBuffer3 => attributeBuffer3;

    /// <inheritdoc />
    public void Draw(int count = -1)
    {
        GL.BindVertexArray(this.id);

        if (indexBuffer != null)
        {
            // There's an index buffer (which will be bound) - bind it and draw
            GL.DrawElements(
                mode: this.primitiveType,
                count: count == -1 ? this.VertexCount : count,
                type: DrawElementsType.UnsignedInt,
                indices: IntPtr.Zero);
            GlDebug.ThrowIfGlError("drawing elements");
        }
        else
        {
            // No index - so draw directly from attribute data
            GL.DrawArrays(
                mode: this.primitiveType,
                first: 0,
                count: count == -1 ? this.VertexCount : count);
            GlDebug.ThrowIfGlError("drawing arrays");
        }
    }

    /// <inheritdoc />
    public void Dispose() => Dispose(true);

    ////public void ResizeAttributeBuffer(int bufferIndex, int newSize)
    ////{
    ////    //var newId = Gl.GenBuffer();
    ////    //Gl.NamedBufferData(newId, (uint)(Marshal.SizeOf(elementType) * value), null, usage);
    ////    //Gl.CopyNamedBufferSubData(this.Id, newId, 0, 0, (uint)(Marshal.SizeOf(elementType) * count));
    ////    //count = value;
    ////}

    private void Dispose(bool disposing)
    {
        if (disposing)
        {
            attributeBuffer1.Dispose();
            attributeBuffer2.Dispose();
            attributeBuffer3.Dispose();

            if (indexBuffer != null)
            {
                indexBuffer.Dispose();
            }

            GC.SuppressFinalize(this);
        }

        GL.DeleteVertexArray(this.id);
        GlDebug.ThrowIfGlError("deleting vertex array");
    }
}
#pragma warning restore SA1402