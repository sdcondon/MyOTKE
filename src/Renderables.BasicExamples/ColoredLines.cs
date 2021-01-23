using MyOTKE.Core;
using MyOTKE.ReactiveBuffers;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Reactive.Linq;

namespace MyOTKE.Engine.Components.BasicExamples
{
    /// <summary>
    /// Renderable class for 3D lines. For debug utilities.
    /// </summary>
    public class ColoredLines : IComponent
    {
        private static readonly object ProgramStateLock = new object();
        private static GlProgramWithDUBBuilder<Uniforms> programBuilder;
        private static GlProgramWithDUB<Uniforms> program;

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
            this.lines = new ObservableCollection<Line>();

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

            this.linesBufferBuilder = new ReactiveBufferBuilder<Vertex>(
                primitiveType: PrimitiveType.Lines,
                atomCapacity: 100,
                atomIndices: new[] { 0, 1 },
                atomSource: ((INotifyCollectionChanged)lines).ToObservable<Line>()
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
            ThrowIfDisposed();

            this.lines.Add(new Line(from, to));
        }

        /// <summary>
        /// Clears all of the lines.
        /// </summary>
        public void ClearLines()
        {
            ThrowIfDisposed();

            this.lines.Clear();
        }

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

            this.linesBuffer = linesBufferBuilder.Build();
            this.linesBufferBuilder = null;
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
                MVP = this.viewProjection.View * this.viewProjection.Projection,
                V = this.viewProjection.View,
                M = Matrix4.Identity,
                AmbientLightColor = AmbientLightColor,
                PointLightPosition = PointLightPosition,
                PointLightColor = PointLightColor,
                PointLightPower = PointLightPower,
            });
            this.linesBuffer.Draw();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            this.linesBuffer?.Dispose();
            isDisposed = true;
        }

        private void ThrowIfDisposed()
        {
            if (isDisposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }

        private struct Uniforms
        {
            public Matrix4 MVP;
            public Matrix4 V;
            public Matrix4 M;
            public Vector3 AmbientLightColor;
            public Vector3 PointLightPosition;
            public Vector3 PointLightColor;
            public float PointLightPower;
        }

        private struct Vertex
        {
            public readonly Vector3 Position;
            public readonly Vector3 Color;
            public readonly Vector3 Normal;

            public Vertex(Vector3 position, Vector3 color, Vector3 normal)
            {
                this.Position = position;
                this.Color = color;
                this.Normal = normal;
            }
        }

        private class Line : INotifyPropertyChanged
        {
            private Vector3 from;
            private Vector3 to;

            public Line(Vector3 from, Vector3 to)
            {
                this.From = from;
                this.To = to;
            }

            public event PropertyChangedEventHandler PropertyChanged;

            public Vector3 From
            {
                get => from;
                set
                {
                    this.from = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(From)));
                }
            }

            public Vector3 To
            {
                get => to;
                set
                {
                    this.to = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(To)));
                }
            }
        }
    }
}
