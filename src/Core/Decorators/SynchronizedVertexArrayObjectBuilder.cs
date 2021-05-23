#pragma warning disable SA1204
#pragma warning disable SA1402
using MyOTKE.Core.VaoDecorators;

namespace MyOTKE.Core
{
    /// <summary>
    /// Builder class for <see cref="GlVertexArrayObject{T1}"/> objects that presents a fluent-ish interface.
    /// </summary>
    /// <typeparam name="T1">The type of data to be stored in the 1st buffer.</typeparam>
    /// <remarks>
    /// Useful for setting up a VAO before the OpenGL context has initialized.
    /// </remarks>
    public sealed class SynchronizedVertexArrayObjectBuilder<T1>
        where T1 : struct
    {
        private readonly VertexArrayObjectBuilder<T1> innerBuilder;

        /// <summary>
        /// Initializes a new instance of the <see cref="SynchronizedVertexArrayObjectBuilder{T1}"/> class.
        /// </summary>
        /// <param name="innerBuilder">The builder wrapped by this builder.</param>
        public SynchronizedVertexArrayObjectBuilder(VertexArrayObjectBuilder<T1> innerBuilder)
        {
            this.innerBuilder = innerBuilder;
        }

        /// <summary>
        /// Builds a new <see cref="IVertexArrayObject{T1}"/> instance based on the state of the builder.
        /// </summary>
        /// <returns>The built VAO.</returns>
        public IVertexArrayObject<T1> Build()
        {
            return new SynchronizedVertexArrayObject<T1>(innerBuilder.Build());
        }
    }

    /// <summary>
    /// Extension methods for the <see cref="VertexArrayObjectBuilder{T1}"/> class.
    /// </summary>
    public static class VertexArrayObjectBuilderExtensions1
    {
        /// <summary>
        /// Specifies that the built VAO should be explicitly synchronized, with any pending changes flushed on each draw call.
        /// <para/>
        /// Specifically, means that the created VAO will be a <see cref="SynchronizedVertexArrayObject{T1}"/> instance.
        /// </summary>
        /// <typeparam name="T1">The type of data to be stored in the 1st buffer.</typeparam>
        /// <param name="builder">The builder to act on.</param>
        /// <returns>The updated builder.</returns>
        public static SynchronizedVertexArrayObjectBuilder<T1> Synchronized<T1>(this VertexArrayObjectBuilder<T1> builder)
            where T1 : struct
        {
            return new SynchronizedVertexArrayObjectBuilder<T1>(builder);
        }
    }

    /// <summary>
    /// Builder class for <see cref="GlVertexArrayObject{T1, T2}"/> objects that presents a fluent-ish interface.
    /// </summary>
    /// <typeparam name="T1">The type of data to be stored in the 1st buffer.</typeparam>
    /// <typeparam name="T2">The type of data to be stored in the 2nd buffer.</typeparam>
    /// <remarks>
    /// Useful for setting up a VAO before the OpenGL context has initialized.
    /// </remarks>
    public sealed class SynchronizedVertexArrayObjectBuilder<T1, T2>
        where T1 : struct
        where T2 : struct
    {
        private readonly VertexArrayObjectBuilder<T1, T2> innerBuilder;

        /// <summary>
        /// Initializes a new instance of the <see cref="SynchronizedVertexArrayObjectBuilder{T1, T2}"/> class.
        /// </summary>
        /// <param name="innerBuilder">The builder wrapped by this builder.</param>
        public SynchronizedVertexArrayObjectBuilder(VertexArrayObjectBuilder<T1, T2> innerBuilder)
        {
            this.innerBuilder = innerBuilder;
        }

        /// <summary>
        /// Builds a new <see cref="IVertexArrayObject{T1, T2}"/> instance based on the state of the builder.
        /// </summary>
        /// <returns>The built VAO.</returns>
        public IVertexArrayObject<T1, T2> Build()
        {
            return new SynchronizedVertexArrayObject<T1, T2>(innerBuilder.Build());
        }
    }

    /// <summary>
    /// Extension methods for the <see cref="VertexArrayObjectBuilder{T1, T2}"/> class.
    /// </summary>
    public static class VertexArrayObjectBuilderExtensions2
    {
        /// <summary>
        /// Specifies that the built VAO should be explicitly synchronized, with any pending changes flushed on each draw call.
        /// <para/>
        /// Specifically, means that the created VAO will be a <see cref="SynchronizedVertexArrayObject{T1, T2}"/> instance.
        /// </summary>
        /// <typeparam name="T1">The type of data to be stored in the 1st buffer.</typeparam>
        /// <typeparam name="T2">The type of data to be stored in the 2nd buffer.</typeparam>
        /// <param name="builder">The builder to act on.</param>
        /// <returns>The updated builder.</returns>
        public static SynchronizedVertexArrayObjectBuilder<T1, T2> Synchronized<T1, T2>(this VertexArrayObjectBuilder<T1, T2> builder)
            where T1 : struct
            where T2 : struct
        {
            return new SynchronizedVertexArrayObjectBuilder<T1, T2>(builder);
        }
    }

    /// <summary>
    /// Builder class for <see cref="GlVertexArrayObject{T1, T2, T3}"/> objects that presents a fluent-ish interface.
    /// </summary>
    /// <typeparam name="T1">The type of data to be stored in the 1st buffer.</typeparam>
    /// <typeparam name="T2">The type of data to be stored in the 2nd buffer.</typeparam>
    /// <typeparam name="T3">The type of data to be stored in the 3rd buffer.</typeparam>
    /// <remarks>
    /// Useful for setting up a VAO before the OpenGL context has initialized.
    /// </remarks>
    public sealed class SynchronizedVertexArrayObjectBuilder<T1, T2, T3>
        where T1 : struct
        where T2 : struct
        where T3 : struct
    {
        private readonly VertexArrayObjectBuilder<T1, T2, T3> innerBuilder;

        /// <summary>
        /// Initializes a new instance of the <see cref="SynchronizedVertexArrayObjectBuilder{T1, T2, T3}"/> class.
        /// </summary>
        /// <param name="innerBuilder">The builder wrapped by this builder.</param>
        public SynchronizedVertexArrayObjectBuilder(VertexArrayObjectBuilder<T1, T2, T3> innerBuilder)
        {
            this.innerBuilder = innerBuilder;
        }

        /// <summary>
        /// Builds a new <see cref="IVertexArrayObject{T1, T2, T3}"/> instance based on the state of the builder.
        /// </summary>
        /// <returns>The built VAO.</returns>
        public IVertexArrayObject<T1, T2, T3> Build()
        {
            return new SynchronizedVertexArrayObject<T1, T2, T3>(innerBuilder.Build());
        }
    }

    /// <summary>
    /// Extension methods for the <see cref="VertexArrayObjectBuilder{T1, T2, T3}"/> class.
    /// </summary>
    public static class VertexArrayObjectBuilderExtensions3
    {
        /// <summary>
        /// Specifies that the built VAO should be explicitly synchronized, with any pending changes flushed on each draw call.
        /// <para/>
        /// Specifically, means that the created VAO will be a <see cref="SynchronizedVertexArrayObject{T1, T2, T3}"/> instance.
        /// </summary>
        /// <typeparam name="T1">The type of data to be stored in the 1st buffer.</typeparam>
        /// <typeparam name="T2">The type of data to be stored in the 2nd buffer.</typeparam>
        /// <typeparam name="T3">The type of data to be stored in the 3rd buffer.</typeparam>
        /// <param name="builder">The builder to act on.</param>
        /// <returns>The updated builder.</returns>
        public static SynchronizedVertexArrayObjectBuilder<T1, T2, T3> Synchronized<T1, T2, T3>(this VertexArrayObjectBuilder<T1, T2, T3> builder)
            where T1 : struct
            where T2 : struct
            where T3 : struct
        {
            return new SynchronizedVertexArrayObjectBuilder<T1, T2, T3>(builder);
        }
    }
}
#pragma warning restore SA1402
#pragma warning restore SA1204