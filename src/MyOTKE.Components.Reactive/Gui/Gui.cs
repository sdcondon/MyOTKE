﻿using MyOTKE.Components.Reactive.Gui.Elements;
using MyOTKE.Core;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.ComponentModel;

namespace MyOTKE.Components.Reactive.Gui;

/// <summary>
/// Component class for a set of graphical user interface elements.
/// </summary>
public class Gui : IComponent, IElementParent
{
    private static readonly object ProgramStateLock = new();
    private static GlProgramWithDUBBuilder<Uniforms> programBuilder;
    private static GlProgramWithDUB<Uniforms> program;

    private readonly MyOTKEWindow view;

    private ReactiveBufferBuilder<Vertex> vertexBufferBuilder;
    private ReactiveBuffer<Vertex> vertexBuffer;
    private bool isDisposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="Gui"/> class.
    /// </summary>
    /// <param name="view">The view from which to derive size and input.</param>
    /// <param name="initialCapacity">Initial capacity of the GUI, in vertices.</param>
    public Gui(MyOTKEWindow view, int initialCapacity)
    {
        this.view = view;
        this.view.Resize += View_Resized;

        this.SubElements = new ElementCollection(this);

        if (program == null && programBuilder == null)
        {
            lock (ProgramStateLock)
            {
                if (program == null && programBuilder == null)
                {
                    programBuilder = new GlProgramBuilder()
                        .WithVertexShaderFromEmbeddedResource("Gui.Vertex.glsl")
                        .WithFragmentShaderFromEmbeddedResource("Gui.Fragment.glsl")
                        .WithDefaultUniformBlock<Uniforms>();
                }
            }
        }

        this.vertexBufferBuilder = new ReactiveBufferBuilder<Vertex>(
            PrimitiveType.Triangles,
            initialCapacity,
            [0, 2, 3, 0, 3, 1],
            this.SubElements.Flatten());
    }

    /// <inheritdoc />
    public event PropertyChangedEventHandler PropertyChanged;

    /// <inheritdoc />
    public event EventHandler<Vector2> Clicked;

    /// <inheritdoc /> from IElementParent
    public ElementCollection SubElements { get; }

    /// <inheritdoc /> from IElementParent
    public Vector2 Center => Vector2.Zero;

    /// <inheritdoc /> from IElementParent
    public Vector2 Size => new(view.ClientSize.X, view.ClientSize.Y);

    /// <inheritdoc /> from IComponent
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

        this.vertexBuffer = this.vertexBufferBuilder.Build();
        this.vertexBufferBuilder = null;
    }

    /// <inheritdoc />
    public void Update(TimeSpan elapsed)
    {
        ObjectDisposedException.ThrowIf(isDisposed, this);

        if (view.IsMouseButtonReleased(MouseButton.Left))
        {
            Clicked?.Invoke(this, new Vector2(view.MouseCenterOffset.X, -view.MouseCenterOffset.Y));
        }
    }

    /// <inheritdoc /> from IComponent
    public void Render()
    {
        ObjectDisposedException.ThrowIf(isDisposed, this);

        // Assume the GUI is drawn last and is independent - goes on top of everything drawn already - so clear the depth buffer
        GL.Clear(ClearBufferMask.DepthBufferBit);

        program.UseWithDefaultUniformBlock(new Uniforms
        {
            P = Matrix4.Transpose(Matrix4.CreateOrthographic(Size.X, Size.Y, 1f, -1f)),
            text = 0,
        });
        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.Texture2DArray, Text.Font.TextureId);
        this.vertexBuffer.Draw();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        this.view.Resize -= View_Resized;
        this.vertexBuffer?.Dispose();
        GC.SuppressFinalize(this);
        this.isDisposed = true;
    }

    private void View_Resized(ResizeEventArgs e)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Size)));
    }

    private struct Uniforms
    {
        public Matrix4 P;
        public int text;
    }
}
