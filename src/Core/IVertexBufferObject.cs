namespace MyOTKE.Core
{
    /// <summary>
    /// Interface for types representing an OpenGL vertex buffer object.
    /// </summary>
    /// <typeparam name="T">The .NET type of data to be stored in the buffer.</typeparam>
    public interface IVertexBufferObject<T>
        where T : struct
    {
        /// <summary>
        /// Gets the ID of the buffer object.
        /// </summary>
        int Id { get; }

        /// <summary>
        /// Gets the vertex attribute info for this buffer.
        /// </summary>
        GlVertexAttributeInfo[] Attributes { get; }

        /// <summary>
        /// Gets the number of vertices that the buffer has the capacity for.
        /// </summary>
        int Capacity { get; }

        /// <summary>
        /// Sets data for the vertex at a particular index.
        /// </summary>
        /// <param name="index">The index of the object to set.</param>
        T this[int index] { get;  set; }

        /// <summary>
        /// Copy data internally within the buffer.
        /// </summary>
        /// <param name="readIndex">The (object) index to read from.</param>
        /// <param name="writeIndex">The (object) index to write to.</param>
        /// <param name="count">The number of objects to copy.</param>
        void Copy(int readIndex, int writeIndex, int count);
    }
}
