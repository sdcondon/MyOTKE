namespace MyOTKE.Core;

/// <summary>
/// Interface OpenGL vertex buffer objects, interpreted as storing an array of .NET structs of a particular type.
/// <para/>
/// See https://www.khronos.org/opengl/wiki/Vertex_Specification#Vertex_Buffer_Object.
/// </summary>
/// <typeparam name="T">The .NET type of data to be stored in the buffer.</typeparam>
public interface IVertexBufferObject<T> : IBufferObject<T>
    where T : struct
{
    /// <summary>
    /// Gets the vertex attribute info for this buffer.
    /// </summary>
    GlVertexAttributeInfo[] Attributes { get; }
}
