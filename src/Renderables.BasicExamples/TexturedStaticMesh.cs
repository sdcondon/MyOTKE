﻿using MyOTKE.Core;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;

namespace MyOTKE.Renderables.BasicExamples
{
    /// <summary>
    /// Renderable class for static 3D geometry.
    /// </summary>
    public sealed class TexturedStaticMesh : IRenderable
    {
        private static readonly object ProgramStateLock = new object();
        private static GlProgramWithDUBBuilder<Uniforms> programBuilder;
        private static GlProgramWithDUB<Uniforms> program;

        private readonly IViewProjection viewProjection;
        private readonly string textureFilePath;

        private int[] textures;
        private VertexArrayObjectBuilder<Vertex> vertexArrayObjectBuilder;
        private GlVertexArrayObject<Vertex> vertexArrayObject;
        private bool isDisposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="TexturedStaticMesh"/> class.
        /// </summary>
        /// <param name="viewProjection">Provider for view and projection matrices.</param>
        /// <param name="vertices">The vertices of the mesh to be rendered.</param>
        /// <param name="indices">The vertex indices to use when rendering.</param>
        /// <param name="textureFilePath">The path to the file to use for the mesh's texture.</param>
        public TexturedStaticMesh(
            IViewProjection viewProjection,
            IEnumerable<Vertex> vertices,
            IEnumerable<uint> indices,
            string textureFilePath)
        {
            this.viewProjection = viewProjection;

            if (program == null && programBuilder == null)
            {
                lock (ProgramStateLock)
                {
                    if (program == null && programBuilder == null)
                    {
                        programBuilder = new GlProgramBuilder()
                            .WithVertexShaderFromEmbeddedResource("Textured.Vertex.glsl")
                            .WithFragmentShaderFromEmbeddedResource("Textured.Fragment.glsl")
                            .WithDefaultUniformBlock<Uniforms>();
                    }
                }
            }

            this.vertexArrayObjectBuilder = new VertexArrayObjectBuilder(PrimitiveType.Triangles)
                .WithAttributeBuffer(BufferUsageHint.StaticDraw, vertices.ToArray())
                .WithIndex(indices.ToArray());

            this.textureFilePath = textureFilePath;
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="TexturedStaticMesh"/> class.
        /// </summary>
        ~TexturedStaticMesh() => Dispose(false);

        /// <summary>
        /// Gets or sets the model transform for this mesh.
        /// </summary>
        public Matrix4x4 Model { get; set; } = Matrix4x4.Identity;

        /// <summary>
        /// Gets or sets the lighting applied as a minimum to every fragment.
        /// </summary>
        public Color AmbientLightColor { get; set; } = Color.Transparent();

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

            this.textures = new int[1];
            this.textures[0] = Path.GetExtension(textureFilePath) == ".DDS" ? TextureLoader.LoadDDS(textureFilePath) : TextureLoader.LoadBMP(textureFilePath);

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
                MVP = Matrix4x4.Transpose(this.Model * this.viewProjection.View * this.viewProjection.Projection),
                V = Matrix4x4.Transpose(this.viewProjection.View),
                M = Matrix4x4.Transpose(this.Model),
                TextureSampler = 0,
                AmbientLightColor = AmbientLightColor,
                LightPosition = PointLightPosition,
                LightColor = PointLightColor,
                LightPower = PointLightPower,
            });

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, textures[0]);

            this.vertexArrayObject.Draw(-1);
        }

        /// <inheritdoc />
        public void Dispose() => Dispose(true);

        private void Dispose(bool disposing)
        {
            GL.DeleteTextures(textures.Length, textures);

            if (disposing)
            {
                this.vertexArrayObject?.Dispose();
                this.isDisposed = true;
                GC.SuppressFinalize(this);
            }
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
            /// Gets the texture coordinate of the vertex.
            /// </summary>
            public readonly Vector2 UV;

            /// <summary>
            /// Gets the normal vector of the vertex.
            /// </summary>
            public readonly Vector3 Normal;

            /// <summary>
            /// Initializes a new instance of the <see cref="Vertex"/> struct.
            /// </summary>
            /// <param name="position">The position of the vertex.</param>
            /// <param name="uv">The texture coordinate of the vertex.</param>
            /// <param name="normal">The normal vector of the vertex.</param>
            public Vertex(Vector3 position, Vector2 uv, Vector3 normal)
            {
                Position = position;
                UV = uv;
                Normal = normal;
            }
        }

        private struct Uniforms
        {
            public Matrix4x4 MVP;
            public Matrix4x4 V;
            public Matrix4x4 M;
            public int TextureSampler;
            public Vector3 AmbientLightColor;
            public Vector3 LightPosition;
            public Vector3 LightColor;
            public float LightPower;
        }
    }
}
