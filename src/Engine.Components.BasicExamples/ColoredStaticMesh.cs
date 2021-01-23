using MyOTKE.Core;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MyOTKE.Engine.Components.BasicExamples
{
    /// <summary>
    /// Simple component class that draws static 3D geometry.
    /// </summary>
    public class ColoredStaticMesh : IComponent
    {
        private static readonly object ProgramStateLock = new object();
        private static GlProgramWithDUBBuilder<Uniforms> programBuilder;
        private static GlProgramWithDUB<Uniforms> program;

        private readonly IViewProjection viewProjection;

        private VertexArrayObjectBuilder<Vertex> vertexArrayObjectBuilder;
        private GlVertexArrayObject<Vertex> vertexArrayObject;
        private bool isDisposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="ColoredStaticMesh"/> class.
        /// </summary>
        /// <param name="viewProjection">The provider for the view and projection matrices to use when rendering.</param>
        /// <param name="vertices">The vertices of the mesh.</param>
        /// <param name="indices">The indices (into the provided vertices) to use for actually rendering the mesh.</param>
        public ColoredStaticMesh(
            IViewProjection viewProjection,
            IEnumerable<Vertex> vertices,
            IEnumerable<uint> indices)
        {
            this.viewProjection = viewProjection;

            if (program == null && programBuilder == null)
            {
                lock (ProgramStateLock)
                {
                    if (program == null && programBuilder == null)
                    {
                        programBuilder = new GlProgramBuilder()
                            .WithVertexShaderFromEmbeddedResource("Colored.Vertex.glsl")
                            .WithFragmentShaderFromEmbeddedResource("Colored.Fragment.glsl")
                            .WithDefaultUniformBlock<Uniforms>();
                    }
                }
            }

            this.vertexArrayObjectBuilder = new VertexArrayObjectBuilder(PrimitiveType.Triangles)
                .WithNewAttributeBuffer(BufferUsageHint.StaticDraw, vertices.ToArray())
                .WithNewIndexBuffer(BufferUsageHint.StaticDraw, indices.ToArray());
        }

        /// <summary>
        /// Gets or sets the model transform for this mesh.
        /// </summary>
        public Matrix4 Model { get; set; } = Matrix4.Identity;

        /// <summary>
        /// Gets or sets the lighting applied as a minimum to every fragment.
        /// </summary>
        public Color AmbientLightColor { get; set; } = Color.Transparent();

        /// <summary>
        /// Gets or sets the directed light direction. Fragments facing this direction are lit with the directed light color.
        /// </summary>
        public Vector3 DirectedLightDirection { get; set; } = Vector3.Zero;

        /// <summary>
        /// Gets or sets the directed light color, applied to fragments facing the directed light direction.
        /// </summary>
        public Color DirectedLightColor { get; set; } = Color.Transparent();

        /// <summary>
        /// Gets or sets the point light position. Fragments facing this position are lit with the point light color.
        /// </summary>
        public Vector3 PointLightPosition { get; set; } = Vector3.Zero;

        /// <summary>
        /// Gets or sets the point light color, applied to fragments facing the point light position.
        /// </summary>
        public Color PointLightColor { get; set; } = Color.Transparent();

        /// <summary>
        /// Gets or sets the point light power. Affects the intensity with which fragments are lit by the point light.
        /// </summary>
        public float PointLightPower { get; set; } = 0f;

        /// <inheritdoc />
        public void Load()
        {
            ThrowIfDisposed();

            if (program == null)
            {
                lock (ProgramStateLock)
                {
                    if (program == null)
                    {
                        program = programBuilder.Build();
                        programBuilder = null;
                    }
                }
            }

            this.vertexArrayObject = (GlVertexArrayObject<Vertex>)this.vertexArrayObjectBuilder.Build();
            this.vertexArrayObjectBuilder = null;
        }

        /// <inheritdoc />
        public void Update(TimeSpan elapsed)
        {
        }

        /// <inheritdoc />
        public void Render()
        {
            ThrowIfDisposed();

            program.UseWithUniformValues(new Uniforms
            {
                MVP = this.Model * this.viewProjection.View * this.viewProjection.Projection,
                V = this.viewProjection.View,
                M = this.Model,
                AmbientLightColor = AmbientLightColor,
                DirectedLightDirection = DirectedLightDirection,
                DirectedLightColor = DirectedLightColor,
                PointLightPosition = PointLightPosition,
                PointLightColor = PointLightColor,
                PointLightPower = PointLightPower,
            });
            this.vertexArrayObject.Draw(-1);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            this.vertexArrayObject?.Dispose();
            isDisposed = true;
        }

        private void ThrowIfDisposed()
        {
            if (isDisposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }

        /// <summary>
        /// Container struct for the attributes of a vertex.
        /// </summary>
        public struct Vertex
        {
            /// <summary>
            /// Gets the position of the vertex.
            /// </summary>
            public readonly Vector3 Position;

            /// <summary>
            /// Gets the color of the vertex.
            /// </summary>
            public readonly Vector3 Color;

            /// <summary>
            /// Gets the normal vector of the vertex.
            /// </summary>
            public readonly Vector3 Normal;

            /// <summary>
            /// Initializes a new instance of the <see cref="Vertex"/> struct.
            /// </summary>
            /// <param name="position">The position of the vertex.</param>
            /// <param name="color">The color of the vertex.</param>
            /// <param name="normal">The normal vector of the vertex.</param>
            public Vertex(Vector3 position, Vector3 color, Vector3 normal)
            {
                Position = position;
                Color = color;
                Normal = normal;
            }
        }

        private struct Uniforms
        {
            public Matrix4 MVP;
            public Matrix4 V;
            public Matrix4 M;
            public Vector3 AmbientLightColor;
            public Vector3 DirectedLightDirection;
            public Vector3 DirectedLightColor;
            public Vector3 PointLightPosition;
            public Vector3 PointLightColor;
            public float PointLightPower;
        }
    }
}
