using MyOTKE.Core;
using MyOTKE.Cameras;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using MyOTKE.BufferManagement;

namespace MyOTKE.Components.Primitives;

/// <summary>
/// Implementation of <see cref="IComponent" /> that renders a set of primitive shapes from an observable sequence of source data.
/// </summary>
public class Primitives : IComponent
{
    private static readonly object ProgramStateLock = new();
    private static GlProgramWithDUBBuilder<Uniforms> programBuilder;
    private static GlProgramWithDUB<Uniforms> program;

    private readonly IViewProjection camera;
    private readonly Queue<Primitive> initialPrimitives;

    private ListBufferBuilder<PrimitiveVertex> coloredTriangleBufferBuilder;
    private ListBuffer<PrimitiveVertex> coloredTriangleBuffer;
    private ListBufferBuilder<PrimitiveVertex> coloredLineBufferBuilder;
    private ListBuffer<PrimitiveVertex> coloredLineBuffer;
    private bool isDisposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="Primitives"/> class.
    /// </summary>
    /// <param name="camera">Provider for view and projection matrices.</param>
    /// <param name="initialPrimitives">The initial primitives to display.</param>
    /// <param name="capacity">The maximum number of triangles and lines that can be rendered at once.</param>
    public Primitives(
        IViewProjection camera,
        IEnumerable<Primitive> initialPrimitives,
        int capacity)
    {
        this.camera = camera;
        this.initialPrimitives = new(initialPrimitives);

        if (program == null && programBuilder == null)
        {
            lock (ProgramStateLock)
            {
                if (program == null && programBuilder == null)
                {
                    programBuilder = new GlProgramBuilder()
                        .WithVertexShaderFromEmbeddedResource("Primitives.Colored.Vertex.glsl")
                        .WithFragmentShaderFromEmbeddedResource("Primitives.Colored.Fragment.glsl")
                        .WithDefaultUniformBlock<Uniforms>();
                }
            }
        }

        // Re-use a single vertex list for every primitive to reduce GC burden - NB not re-entrant
        coloredTriangleBufferBuilder = new ListBufferBuilder<PrimitiveVertex>(
            PrimitiveType.Triangles,
            capacity,
            [0, 1, 2]);

        // Re-use a single vertex list for every primitive to reduce GC burden - NB not re-entrant
        coloredLineBufferBuilder = new ListBufferBuilder<PrimitiveVertex>(
            PrimitiveType.Lines,
            capacity,
            [0, 1]);
    }

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

        coloredTriangleBuffer = coloredTriangleBufferBuilder.Build();
        coloredTriangleBufferBuilder = null;

        coloredLineBuffer = coloredLineBufferBuilder.Build();
        coloredLineBufferBuilder = null;

        foreach (var primitive in initialPrimitives)
        {
            Add(primitive);
        }
    }

    /// <summary>
    /// Adds a primitive.
    /// </summary>
    /// <param name="primitive">The primitive to add.</param>
    public void Add(Primitive primitive)
    {
        // meh, thread-safety
        if (primitive is TrianglesPrimitive triangles)
        {
            if (coloredTriangleBuffer != null)
            {
                triangles.AddToBuffer(coloredTriangleBuffer);
            }
            else
            {
                initialPrimitives.Enqueue(triangles);
            }
        }
        else if (primitive is LinesPrimitive lines)
        {
            if (coloredLineBuffer != null)
            {
                lines.AddToBuffer(coloredTriangleBuffer);
            }
            else
            {
                initialPrimitives.Enqueue(lines);
            }
        }
    }

    /// <summary>
    /// Removes a primitive.
    /// </summary>
    /// <param name="primitive">The primitive to remove.</param>
    public void Remove(TrianglesPrimitive primitive)
    {
        // code smell: will be removed from whatever buffer it is in, even if its not this one.
        primitive.RemoveFromBuffer();
    }

    /// <inheritdoc />
    public void Update(TimeSpan elapsed)
    {
    }

    /// <inheritdoc />
    public void Render()
    {
        ThrowIfDisposed();

        program.UseWithDefaultUniformBlock(new Uniforms
        {
            MVP = camera.View * camera.Projection,
            V = camera.View,
            M = Matrix4.Identity,
            AmbientLightColor = AmbientLightColor,
            DirectedLightDirection = DirectedLightDirection,
            DirectedLightColor = DirectedLightColor,
            PointLightPosition = PointLightPosition,
            PointLightColor = PointLightColor,
            PointLightPower = PointLightPower,
        });
        coloredTriangleBuffer.Draw();
        coloredLineBuffer.Draw();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        coloredTriangleBuffer.Dispose();
        coloredLineBuffer.Dispose();
        GC.SuppressFinalize(this);
        isDisposed = true;
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(isDisposed, this);
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
