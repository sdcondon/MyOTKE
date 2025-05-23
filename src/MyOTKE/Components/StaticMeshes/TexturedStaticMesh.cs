﻿using MyOTKE.Core;
using MyOTKE.Core.IO;
using MyOTKE.Cameras;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MyOTKE.Components.StaticMeshes;

/// <summary>
/// Simple component class that draws static 3D geometry.
/// </summary>
public sealed class TexturedStaticMesh : IComponent
{
    private static readonly object ProgramStateLock = new();
    private static GlProgramWithDUBBuilder<DefaultUniformBlock, CameraUniformBlock> programBuilder;
    private static GlProgramWithDUB<DefaultUniformBlock, CameraUniformBlock> program;

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
                        .WithVertexShaderFromEmbeddedResource("StaticMeshes.Textured.Vertex.glsl")
                        .WithFragmentShaderFromEmbeddedResource("StaticMeshes.Textured.Fragment.glsl")
                        .WithDefaultUniformBlock<DefaultUniformBlock>()
                        .WithSharedUniformBufferObject<CameraUniformBlock>("Camera", BufferUsageHint.DynamicDraw, 1);
                }
            }
        }

        vertexArrayObjectBuilder = new VertexArrayObjectBuilder(PrimitiveType.Triangles)
            .WithNewAttributeBuffer(BufferUsageHint.StaticDraw, vertices.ToArray())
            .WithNewIndexBuffer(BufferUsageHint.StaticDraw, [.. indices]);

        this.textureFilePath = textureFilePath;
    }

    /// <summary>
    /// Finalizes an instance of the <see cref="TexturedStaticMesh"/> class.
    /// </summary>
    ~TexturedStaticMesh() => Dispose(false);

    /// <summary>
    /// Gets or sets the model transform for this mesh.
    /// </summary>
    public Matrix4 Model { get; set; } = Matrix4.Identity;

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
        ObjectDisposedException.ThrowIf(isDisposed, this);

        textures = new int[1];
        textures[0] = Path.GetExtension(textureFilePath) == ".DDS" ? TextureLoader.LoadDDS(textureFilePath) : TextureLoader.LoadBMP(textureFilePath);

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

        vertexArrayObject = (GlVertexArrayObject<Vertex>)vertexArrayObjectBuilder.Build();
        vertexArrayObjectBuilder = null;
    }

    /// <inheritdoc />
    public void Update(TimeSpan elapsed)
    {
    }

    /// <inheritdoc />
    public void Render()
    {
        ObjectDisposedException.ThrowIf(isDisposed, this);

        // camera should be managing this.
        program.UniformBuffer1[0] = new CameraUniformBlock
        {
            V = viewProjection.View,
            P = viewProjection.Projection,
        };

        program.UseWithDefaultUniformBlock(new DefaultUniformBlock
        {
            M = Model,
            TextureSampler = 0,
            AmbientLightColor = AmbientLightColor,
            LightPosition = PointLightPosition,
            LightColor = PointLightColor,
            LightPower = PointLightPower,
        });

        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.Texture2D, textures[0]);

        vertexArrayObject.Draw(-1);
    }

    /// <inheritdoc />
    public void Dispose() => Dispose(true);

    private void Dispose(bool disposing)
    {
        GL.DeleteTextures(textures.Length, textures);

        if (disposing)
        {
            vertexArrayObject?.Dispose();
            isDisposed = true;
            GC.SuppressFinalize(this);
        }
    }

    /// <summary>
    /// Container struct for the attributes of a vertex.
    /// </summary>
    /// <param name="position">The position of the vertex.</param>
    /// <param name="uv">The texture coordinate of the vertex.</param>
    /// <param name="normal">The normal vector of the vertex.</param>
    public readonly struct Vertex(Vector3 position, Vector2 uv, Vector3 normal)
    {
        /// <summary>
        /// Gets the position of the vertex.
        /// </summary>
        public readonly Vector3 Position = position;

        /// <summary>
        /// Gets the texture coordinate of the vertex.
        /// </summary>
        public readonly Vector2 UV = uv;

        /// <summary>
        /// Gets the normal vector of the vertex.
        /// </summary>
        public readonly Vector3 Normal = normal;
    }

    private struct CameraUniformBlock
    {
        public Matrix4 V;
        public Matrix4 P;
    }

    private struct DefaultUniformBlock
    {
        public Matrix4 M;
        public int TextureSampler;
        public Vector3 AmbientLightColor;
        public Vector3 LightPosition;
        public Vector3 LightColor;
        public float LightPower;
    }
}
