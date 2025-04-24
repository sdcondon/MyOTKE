using MyOTKE.Core;
using MyOTKE.Cameras;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

namespace MyOTKE.Components.Reactive.Primitives;

/// <summary>
/// Implementation of <see cref="IComponent" /> that renders a set of primitive shapes from an observable sequence of source data.
/// </summary>
public class PrimitiveRenderer : IComponent
{
    private static readonly object ProgramStateLock = new();
    private static GlProgramWithDUBBuilder<DefaultUniformBlock, CameraUniformBlock> programBuilder;
    private static GlProgramWithDUB<DefaultUniformBlock, CameraUniformBlock> program;

    private readonly IViewProjection camera;
    private readonly IObservable<IObservable<IList<Primitive>>> source;

    private ReactiveBufferBuilder<PrimitiveVertex> coloredTriangleBufferBuilder;
    private ReactiveBuffer<PrimitiveVertex> coloredTriangleBuffer;
    private ReactiveBufferBuilder<PrimitiveVertex> coloredLineBufferBuilder;
    private ReactiveBuffer<PrimitiveVertex> coloredLineBuffer;
    private bool isDisposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="PrimitiveRenderer"/> class.
    /// </summary>
    /// <param name="camera">Provider for view and projection matrices.</param>
    /// <param name="source">Source data. Outer sequence pushes different renderable entities, each of which pushes each time its state changes.</param>
    /// <param name="capacity">The maximum number of triangles and lines that can be rendered at once.</param>
    public PrimitiveRenderer(
        IViewProjection camera,
        IObservable<IObservable<IList<Primitive>>> source,
        int capacity)
    {
        this.camera = camera;
        this.source = source;

        if (program == null && programBuilder == null)
        {
            lock (ProgramStateLock)
            {
                if (program == null && programBuilder == null)
                {
                    programBuilder = new GlProgramBuilder()
                        .WithVertexShaderFromEmbeddedResource("Primitives.Colored.Vertex.glsl")
                        .WithFragmentShaderFromEmbeddedResource("Primitives.Colored.Fragment.glsl")
                        .WithDefaultUniformBlock<DefaultUniformBlock>()
                        .WithSharedUniformBufferObject<CameraUniformBlock>("Camera", BufferUsageHint.DynamicDraw, 1);
                }
            }
        }

        // Re-use a single vertex list for every primitive to reduce GC burden - NB not re-entrant
        var triangleVertexList = new List<PrimitiveVertex>();
        coloredTriangleBufferBuilder = new ReactiveBufferBuilder<PrimitiveVertex>(
            PrimitiveType.Triangles,
            capacity,
            [0, 1, 2],
            this.source.Select(pso => pso.Select(ps =>
            {
                triangleVertexList.Clear();
                for (int i = 0; i < ps.Count; i++)
                {
                    if (ps[i].IsTrianglePrimitive)
                    {
                        triangleVertexList.AddRange(ps[i].Vertices);
                    }
                }

                return triangleVertexList;
            })));

        // Re-use a single vertex list for every primitive to reduce GC burden - NB not re-entrant
        var lineVertexList = new List<PrimitiveVertex>();
        coloredLineBufferBuilder = new ReactiveBufferBuilder<PrimitiveVertex>(
            PrimitiveType.Lines,
            capacity,
            [0, 1],
            this.source.Select(pso => pso.Select(ps =>
            {
                lineVertexList.Clear();
                for (int i = 0; i < ps.Count; i++)
                {
                    if (!ps[i].IsTrianglePrimitive)
                    {
                        lineVertexList.AddRange(ps[i].Vertices);
                    }
                }

                return lineVertexList;
            })));
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
        ObjectDisposedException.ThrowIf(isDisposed, this);

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
    }

    /// <inheritdoc />
    public void Update(TimeSpan elapsed)
    {
    }

    /// <inheritdoc />
    public void Render()
    {
        ObjectDisposedException.ThrowIf(isDisposed, this);

        // TODO: camera should manage this..
        program.UniformBuffer1[0] = new CameraUniformBlock
        {
            V = camera.View,
            P = camera.Projection,
        };

        program.UseWithDefaultUniformBlock(new DefaultUniformBlock
        {
            M = Matrix4.Identity,
            AmbientLightColor = AmbientLightColor,
            DirectedLightDirection = DirectedLightDirection,
            DirectedLightColor = DirectedLightColor,
            PointLightPosition = PointLightPosition,
            PointLightColor = PointLightColor,
            PointLightPower = PointLightPower
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

    private struct CameraUniformBlock
    {
        public Matrix4 V;
        public Matrix4 P;
    }

    private struct DefaultUniformBlock
    {
        public Matrix4 M;
        public Vector3 AmbientLightColor;
        public Vector3 DirectedLightDirection;
        public Vector3 DirectedLightColor;
        public Vector3 PointLightPosition;
        public Vector3 PointLightColor;
        public float PointLightPower;
    }
}
