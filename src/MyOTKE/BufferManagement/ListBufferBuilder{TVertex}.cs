using MyOTKE.Core;
using MyOTKE.Core.Decorators;
using OpenTK.Graphics.OpenGL;
using System.Collections.Generic;
using System.Linq;

namespace MyOTKE.BufferManagement;

/// <summary>
/// Builder for <see cref="ListBuffer{TVertex}"/> instances.
/// </summary>
/// <typeparam name="TVertex">The vertex type of the buffer to be built.</typeparam>
public class ListBufferBuilder<TVertex>
    where TVertex : struct
{
    private readonly SynchronizedVertexArrayObjectBuilder<TVertex> builder;
    private readonly IList<int> atomIndices;

    /// <summary>
    /// Initializes a new instance of the <see cref="ListBufferBuilder{TVertex}"/> class.
    /// </summary>
    /// <param name="primitiveType">The OpenGL primitive type to be rendered.</param>
    /// <param name="atomCapacity">The desired capcity (in atoms) of the created buffer.</param>
    /// <param name="atomIndices">The vertex indices to use when rendering each atom.</param>
    public ListBufferBuilder(
        PrimitiveType primitiveType,
        int atomCapacity,
        IList<int> atomIndices)
    {
        var verticesPerAtom = atomIndices.Max() + 1; // Perhaps should throw if has unused indices..

        builder = new VertexArrayObjectBuilder(primitiveType)
            .WithNewAttributeBuffer<TVertex>(BufferUsageHint.DynamicDraw, atomCapacity * verticesPerAtom)
            .WithNewIndexBuffer(BufferUsageHint.DynamicDraw, atomCapacity * atomIndices.Count)
            .Synchronized();

        this.atomIndices = atomIndices;
    }

    /// <summary>
    /// Builds a <see cref="ListBuffer{TVertex}"/> instance based on the state of this builder.
    /// </summary>
    /// <returns>The new <see cref="ListBuffer{TVertex}"/> instance.</returns>
    public ListBuffer<TVertex> Build()
    {
        var vao = builder.Build();
        return new ListBuffer<TVertex>(vao, atomIndices);
    }
}
