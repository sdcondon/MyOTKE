namespace MyOTKE.Core
{
    /// <summary>
    /// Interface for types representing an OpenGL vertex array object.
    /// <para />
    /// See https://www.khronos.org/opengl/wiki/Vertex_Specification#Vertex_Array_Object.
    /// </summary>
    /// <typeparam name="T1">The type of the 1st attribute buffer.</typeparam>
    public interface IVertexArrayObject<T1>
        where T1 : struct
    {
        /// <summary>
        /// Gets the index buffer object for this VAO, if there is one.
        /// </summary>
        IVertexBufferObject<uint> IndexBuffer { get; }

        /// <summary>
        /// Gets the 1st attribute buffer object referenced by this VAO.
        /// </summary>
        IVertexBufferObject<T1> AttributeBuffer1 { get; }

        /// <summary>
        /// Draw with the active program.
        /// </summary>
        /// <param name="count">The number of vertices to draw, or -1 to draw all vertices.</param>
        void Draw(int count);
    }

    /// <summary>
    /// Interface for types representing an OpenGL vertex array object.
    /// <para />
    /// See https://www.khronos.org/opengl/wiki/Vertex_Specification#Vertex_Array_Object.
    /// </summary>
    /// <typeparam name="T1">The type of the 1st attribute buffer.</typeparam>
    /// <typeparam name="T2">The type of the 2nd attribute buffer.</typeparam>
    public interface IVertexArrayObject<T1, T2>
        where T1 : struct
        where T2 : struct
    {
        /// <summary>
        /// Gets the index buffer object for this VAO, if there is one.
        /// </summary>
        IVertexBufferObject<uint> IndexBuffer { get; }

        /// <summary>
        /// Gets the 1st attribute buffer object referenced by this VAO.
        /// </summary>
        IVertexBufferObject<T1> AttributeBuffer1 { get; }

        /// <summary>
        /// Gets the 2nd attribute buffer object referenced by this VAO.
        /// </summary>
        IVertexBufferObject<T2> AttributeBuffer2 { get; }

        /// <summary>
        /// Draw with the active program.
        /// </summary>
        /// <param name="count">The number of vertices to draw, or -1 to draw all vertices.</param>
        void Draw(int count);
    }

    /// <summary>
    /// Interface for types representing an OpenGL vertex array object.
    /// <para />
    /// See https://www.khronos.org/opengl/wiki/Vertex_Specification#Vertex_Array_Object.
    /// </summary>
    /// <typeparam name="T1">The type of the 1st attribute buffer.</typeparam>
    /// <typeparam name="T2">The type of the 2nd attribute buffer.</typeparam>
    /// <typeparam name="T3">The type of the 3rd attribute buffer.</typeparam>
    public interface IVertexArrayObject<T1, T2, T3>
        where T1 : struct
        where T2 : struct
        where T3 : struct
    {
        /// <summary>
        /// Gets the index buffer object for this VAO, if there is one.
        /// </summary>
        IVertexBufferObject<uint> IndexBuffer { get; }

        /// <summary>
        /// Gets the 1st attribute buffer object referenced by this VAO.
        /// </summary>
        IVertexBufferObject<T1> AttributeBuffer1 { get; }

        /// <summary>
        /// Gets the 2nd attribute buffer object referenced by this VAO.
        /// </summary>
        IVertexBufferObject<T2> AttributeBuffer2 { get; }

        /// <summary>
        /// Gets the 3rd attribute buffer object referenced by this VAO.
        /// </summary>
        IVertexBufferObject<T3> AttributeBuffer3 { get; }

        /// <summary>
        /// Draw with the active program.
        /// </summary>
        /// <param name="count">The number of vertices to draw, or -1 to draw all vertices.</param>
        void Draw(int count);
    }
}