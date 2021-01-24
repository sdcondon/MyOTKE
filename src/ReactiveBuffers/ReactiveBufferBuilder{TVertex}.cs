using MyOTKE.Core;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MyOTKE.ReactiveBuffers
{
    /// <summary>
    /// Builder for <see cref="ReactiveBuffer{TVertex}"/> instances.
    /// </summary>
    /// <typeparam name="TVertex">The vertex type of the buffer to be built.</typeparam>
    public class ReactiveBufferBuilder<TVertex>
        where TVertex : struct
    {
        private readonly SynchronizedVertexArrayObjectBuilder<TVertex> builder;
        private readonly IObservable<IObservable<IList<TVertex>>> vertexSource;
        private readonly IList<int> indices;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReactiveBufferBuilder{TVertex}"/> class.
        /// </summary>
        /// <param name="primitiveType">The OpenGL primitive type to be rendered.</param>
        /// <param name="atomCapacity">The desired capcity (in atoms) of the created buffer.</param>
        /// <param name="atomIndices">The vertex indices to use when rendering each atom.</param>
        /// <param name="atomSource">
        /// The outer observable should push an inner observable (atom) for each new renderable entity.
        /// The atoms should push a new list of vertices whenever the atom's state changes, and complete when it is removed.
        /// </param>
        public ReactiveBufferBuilder(
            PrimitiveType primitiveType,
            int atomCapacity,
            IList<int> atomIndices,
            IObservable<IObservable<IList<TVertex>>> atomSource)
        {
            var verticesPerAtom = atomIndices.Max() + 1; // Perhaps should throw if has unused indices..

            builder = new VertexArrayObjectBuilder(primitiveType)
                .WithNewAttributeBuffer<TVertex>(BufferUsageHint.DynamicDraw, atomCapacity * verticesPerAtom)
                .WithNewIndexBuffer(BufferUsageHint.DynamicDraw, atomCapacity * atomIndices.Count)
                .Synchronized();

            this.vertexSource = atomSource;
            this.indices = atomIndices;
        }

        /// <summary>
        /// Builds a <see cref="ReactiveBuffer{TVertex}"/> instance based on the state of this builder.
        /// </summary>
        /// <returns>The new <see cref="ReactiveBuffer{TVertex}"/> instance.</returns>
        public ReactiveBuffer<TVertex> Build()
        {
            var vao = builder.Build();
            return new ReactiveBuffer<TVertex>(vao, vertexSource, indices);
        }
    }
}
