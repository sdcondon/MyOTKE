#pragma warning disable IDE0290
namespace MyOTKE.Core.Decorators;

/// <summary>
/// Decorator for <see cref="IVertexBufferObject{T}"/> that explicitly synchronizes with OpenGL (making it simple but slow..).
/// <para/>
/// See https://www.khronos.org/opengl/wiki/Synchronization#Implicit_synchronization for some info.
/// </summary>
/// <typeparam name="T">The .NET type of data to be stored in the buffer.</typeparam>
public class SynchronizedVertexBufferObject<T> : SynchronizedBufferObject<T>, IVertexBufferObject<T>
    where T : struct
{
    private readonly IVertexBufferObject<T> vertexBufferObject;

    /// <summary>
    /// Initializes a new instance of the <see cref="SynchronizedVertexBufferObject{T}" /> class.
    /// </summary>
    /// <param name="vertexBufferObject">The <see cref="IVertexBufferObject{T}"/> to wrap.</param>
    public SynchronizedVertexBufferObject(IVertexBufferObject<T> vertexBufferObject)
        : base(vertexBufferObject)
    {
        this.vertexBufferObject = vertexBufferObject;
    }

    /// <inheritdoc />
    public GlVertexAttributeInfo[] Attributes => vertexBufferObject.Attributes;
}
#pragma warning restore IDE0290
