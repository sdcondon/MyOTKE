﻿using OTKOW.Core;
using OTKOW.Core.VaoDecorators;
using OTKOW.ReactiveBuffers;
using OpenToolkit.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Numerics;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace OTKOW.Views.Renderables.Gui
{
    /// <summary>
    /// Renderable container for a set of graphical user interface elements.
    /// </summary>
    public class Gui : IRenderable, IElementParent
    {
        private const string ShaderResourceNamePrefix = "GLHDN.Views.Renderables.Gui.Shaders";

        private static readonly object programStateLock = new object();
        private static GlProgramBuilder programBuilder;
        private static GlProgram program;

        private readonly View view;

        private ReactiveBufferBuilder<Vertex> vertexBufferBuilder;
        private ReactiveBuffer<Vertex> vertexBuffer;
        private bool isDisposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="Gui"/> class, 
        /// </summary>
        /// <param name="view">The view from which to derive size and input.</param>
        public Gui(View view, int initialCapacity)
        {
            this.view = view;
            this.view.Resized += View_Resized;

            this.SubElements = new ElementCollection(this);

            if (program == null && programBuilder == null)
            {
                lock (programStateLock)
                {
                    if (program == null && programBuilder == null)
                    {
                        programBuilder = new GlProgramBuilder()
                            .WithShaderFromEmbeddedResource(ShaderType.VertexShader, $"{ShaderResourceNamePrefix}.Gui.Vertex.glsl")
                            .WithShaderFromEmbeddedResource(ShaderType.FragmentShader, $"{ShaderResourceNamePrefix}.Gui.Fragment.glsl")
                            .WithUniforms("P", "text");
                    }
                }
            }

            this.vertexBufferBuilder = new ReactiveBufferBuilder<Vertex>(
                PrimitiveType.Triangles,
                initialCapacity,
                new[] { 0, 2, 3, 0, 3, 1 },
                this.SubElements.Flatten());
        }

        /// <inheritdoc /> from IElementParent
        public ElementCollection SubElements { get; }

        /// <inheritdoc /> from IElementParent
        public Vector2 Center => Vector2.Zero;

        /// <inheritdoc /> from IElementParent
        public Vector2 Size => new Vector2(view.Width, view.Height);

        /// <inheritdoc />
        public event PropertyChangedEventHandler PropertyChanged;

        /// <inheritdoc />
        public event EventHandler<Vector2> Clicked;

        /// <inheritdoc /> from IRenderable
        public void Load()
        {
            ThrowIfDisposed();

            if (program == null)
            {
                lock (programStateLock)
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
            ThrowIfDisposed();

            if (view.WasLeftMouseButtonReleased)
            {
                Clicked?.Invoke(this, new Vector2(view.CursorPosition.X, -view.CursorPosition.Y));
            }
        }

        /// <inheritdoc /> from IRenderable
        public void Render()
        {
            ThrowIfDisposed();

            // Assume the GUI is drawn last and is independent - goes on top of everything drawn already - so clear the depth buffer
            GL.Clear(ClearBufferMask.DepthBufferBit);

            program.UseWithUniformValues(Matrix4x4.Transpose(Matrix4x4.CreateOrthographic(Size.X, Size.Y, 1f, -1f)), 0);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2DArray, Text.Font.TextureId);
            this.vertexBuffer.Draw();
        }

        public void Dispose()
        {
            this.view.Resized -= View_Resized;
            this.vertexBuffer?.Dispose();
            this.isDisposed = true;
        }

        private void ThrowIfDisposed()
        {
            if (isDisposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }

        private void View_Resized(object sender, Vector2 e)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Size)));
        }
    }
}
