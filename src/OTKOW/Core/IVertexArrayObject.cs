using System;

namespace OTKOW.Core
{
    /// <summary>
    /// Interface for types representing an OpenGL vertex array object.
    /// </summary>
    /// <typeparam name="T1">The type of the 1st buffer.</typeparam>
    public interface IVertexArrayObject<T1>
        where T1 : struct
    {
        /// <summary>
        /// Gets the index buffer object for this VAO, if there is one.
        /// </summary>
        IVertexBufferObject<uint> IndexBuffer { get; }

        /// <summary>
        /// Gets the set of buffer objects contained within this VAO.
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
    /// </summary>
    /// <typeparam name="T1">The type of the 1st buffer.</typeparam>
    /// <typeparam name="T2">The type of the 2nd buffer.</typeparam>
    public interface IVertexArrayObject<T1, T2>
        where T1 : struct
        where T2 : struct
    {
        /// <summary>
        /// Gets the index buffer object for this VAO, if there is one.
        /// </summary>
        IVertexBufferObject<uint> IndexBuffer { get; }

        /// <summary>
        /// Gets the set of buffer objects contained within this VAO.
        /// </summary>
        IVertexBufferObject<T1> AttributeBuffer1 { get; }

        /// <summary>
        /// Gets the set of buffer objects contained within this VAO.
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
    /// </summary>
    /// <typeparam name="T1">The type of the 1st buffer.</typeparam>
    /// <typeparam name="T2">The type of the 2nd buffer.</typeparam>
    /// <typeparam name="T3">The type of the 3rd buffer.</typeparam>
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
        /// Gets the set of buffer objects contained within this VAO.
        /// </summary>
        IVertexBufferObject<T1> AttributeBuffer1 { get; }

        /// <summary>
        /// Gets the set of buffer objects contained within this VAO.
        /// </summary>
        IVertexBufferObject<T2> AttributeBuffer2 { get; }

        /// <summary>
        /// Gets the set of buffer objects contained within this VAO.
        /// </summary>
        IVertexBufferObject<T3> AttributeBuffer3 { get; }

        /// <summary>
        /// Draw with the active program.
        /// </summary>
        /// <param name="count">The number of vertices to draw, or -1 to draw all vertices.</param>
        void Draw(int count);
    }

}