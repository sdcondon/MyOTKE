using MyOTKE.Cameras;
using MyOTKE.Core;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reactive.Linq;

namespace MyOTKE.Components.Reactive.Primitives;

/// <summary>
/// Simple component class that draws a set of 3D lines.
/// </summary>
public class ColoredLines : IComponent
{
    private static readonly object ProgramStateLock = new();
    private static GlProgramWithDUBBuilder<DefaultUniformBlock, CameraUniformBlock> programBuilder;
    private static GlProgramWithDUB<DefaultUniformBlock, CameraUniformBlock> program;

    private readonly IViewProjection viewProjection;
    private readonly ObservableCollection<Line> lines;

    private ReactiveBufferBuilder<Vertex> linesBufferBuilder;
    private ReactiveBuffer<Vertex> linesBuffer;
    private bool isDisposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="ColoredLines"/> class.
    /// </summary>
    /// <param name="viewProjection">The view and projection matrices to use when rendering.</param>
    public ColoredLines(IViewProjection viewProjection)
    {
        this.viewProjection = viewProjection;
        lines = [];

        if (program == null && programBuilder == null)
        {
            lock (ProgramStateLock)
            {
                if (program == null && programBuilder == null)
                {
                    programBuilder = new GlProgramBuilder()
                        .WithVertexShaderFromEmbeddedResource("Primitives.ColoredLines.Vertex.glsl")
                        .WithFragmentShaderFromEmbeddedResource("Primitives.ColoredLines.Fragment.glsl")
                        .WithDefaultUniformBlock<DefaultUniformBlock>()
                        .WithSharedUniformBufferObject<CameraUniformBlock>("Camera", BufferUsageHint.DynamicDraw, 1);
                }
            }
        }

        linesBufferBuilder = new ReactiveBufferBuilder<Vertex>(
            primitiveType: PrimitiveType.Lines,
            atomCapacity: 100,
            atomIndices: [ 0, 1 ],
            atomSource: lines.ToObservable<Line>()
                .Select(o => o
                    .Select(i => new[]
                    {
                        new Vertex(i.From, Color.White(), i.From),
                        new Vertex(i.To, Color.White(), i.To),
                    })));
    }

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

    /// <summary>
    /// Adds a line to be rendered.
    /// </summary>
    /// <param name="from">The position of the start of the line.</param>
    /// <param name="to">The position of the end of the line.</param>
    public void AddLine(Vector3 from, Vector3 to)
    {
        ObjectDisposedException.ThrowIf(isDisposed, this);

        lines.Add(new Line(from, to));
    }

    /// <summary>
    /// Clears all of the lines.
    /// </summary>
    public void ClearLines()
    {
        ObjectDisposedException.ThrowIf(isDisposed, this);

        lines.Clear();
    }

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

        linesBuffer = linesBufferBuilder.Build();
        linesBufferBuilder = null;
    }

    /// <inheritdoc />
    public void Update(TimeSpan elapsed)
    {
    }

    /// <inheritdoc />
    public void Render()
    {
        ObjectDisposedException.ThrowIf(isDisposed, this);

        // TODO: camera should be managing this
        program.UniformBuffer1[0] = new CameraUniformBlock
        {
            V = viewProjection.View,
            P = viewProjection.Projection,
        };

        program.UseWithDefaultUniformBlock(new DefaultUniformBlock
        {
            M = Matrix4.Identity,
            AmbientLightColor = AmbientLightColor,
            PointLightPosition = PointLightPosition,
            PointLightColor = PointLightColor,
            PointLightPower = PointLightPower,
        });

        linesBuffer.Draw();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        linesBuffer?.Dispose();
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
        public Vector3 PointLightPosition;
        public Vector3 PointLightColor;
        public float PointLightPower;
    }

    private readonly struct Vertex(Vector3 position, Vector3 color, Vector3 normal)
    {
        public readonly Vector3 Position = position;
        public readonly Vector3 Color = color;
        public readonly Vector3 Normal = normal;
    }

    private class Line : INotifyPropertyChanged
    {
        private Vector3 from;
        private Vector3 to;

        public Line(Vector3 from, Vector3 to)
        {
            From = from;
            To = to;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public Vector3 From
        {
            get => from;
            set
            {
                from = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(From)));
            }
        }

        public Vector3 To
        {
            get => to;
            set
            {
                to = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(To)));
            }
        }
    }
}
