using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;

namespace MyOTKE.Views.Contexts.GameWindow
{
    /// <summary>
    /// Windows form containing only a single OpenGL render control.
    /// </summary>
    /// <remarks>
    /// See https://opentk.net/learn/chapter1/1-creating-a-window.html for tutorial.
    /// </remarks>
    public sealed class GameWindowViewHost : OpenTK.Windowing.Desktop.GameWindow
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GameWindowViewHost"/> class.
        /// </summary>
        public GameWindowViewHost()
            : base(GameWindowSettings.Default, NativeWindowSettings.Default)
        {
            ViewContext = new GameWindowViewContext(this);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GameWindowViewHost"/> class.
        /// </summary>
        /// <param name="width">The width of the window in pixels.</param>
        /// <param name="height">The height of the window in pixels.</param>
        /// <param name="title">The title of the window.</param>
        public GameWindowViewHost(int width, int height, string title)
            : base(GameWindowSettings.Default, new NativeWindowSettings { Size = new Vector2i(width, height), Title = title })
        {
            ViewContext = new GameWindowViewContext(this);
        }

        /// <summary>
        /// Gets the <see cref="IViewContext" /> of this form for <see cref="View"/> instances to use.
        /// </summary>
        public IViewContext ViewContext { get; }

        /// <inheritdoc />
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            Context.SwapBuffers();
        }

        private class GameWindowViewContext : IViewContext
        {
            private readonly GameWindowViewHost parent;

            public GameWindowViewContext(GameWindowViewHost parent)
            {
                this.parent = parent;
                parent.Load += () => Loading?.Invoke(this, EventArgs.Empty);
                parent.RenderFrame += a => RenderingFrame?.Invoke(this, a);
                parent.UpdateFrame += a => Updating?.Invoke(this, a);
                parent.Unload += () => Unloading?.Invoke(this, EventArgs.Empty);
                parent.KeyDown += a => KeyDown?.Invoke(this, a.Key);
                parent.KeyUp += a => KeyUp?.Invoke(this, a.Key);
                parent.MouseWheel += a => MouseWheel?.Invoke(this, (int)a.OffsetX);

                parent.MouseDown += a =>
                {
                    switch (a.Button)
                    {
                        case MouseButton.Left:
                            LeftMouseDown?.Invoke(this, EventArgs.Empty);
                            break;
                        case MouseButton.Right:
                            RightMouseDown?.Invoke(this, EventArgs.Empty);
                            break;
                        case MouseButton.Middle:
                            MiddleMouseDown?.Invoke(this, EventArgs.Empty);
                            break;
                    }
                };

                parent.MouseUp += a =>
                {
                    switch (a.Button)
                    {
                        case MouseButton.Left:
                            LeftMouseUp?.Invoke(this, EventArgs.Empty);
                            break;
                        case MouseButton.Right:
                            RightMouseUp?.Invoke(this, EventArgs.Empty);
                            break;
                        case MouseButton.Middle:
                            MiddleMouseUp?.Invoke(this, EventArgs.Empty);
                            break;
                    }
                };

                parent.Resize += a => Resize?.Invoke(
                    this,
                    new System.Numerics.Vector2(parent.ClientSize.X, parent.ClientSize.Y));

                parent.FocusedChanged += a => GotFocus?.Invoke(this, EventArgs.Empty);
            }

            /// <inheritdoc />
            public event EventHandler Loading;

            /// <inheritdoc />
            public event EventHandler<FrameEventArgs> RenderingFrame;

            /// <inheritdoc />
            public event EventHandler<FrameEventArgs> Updating;

            /// <inheritdoc />
            public event EventHandler Unloading;

            /// <inheritdoc />
            public event EventHandler<Keys> KeyDown;

            /// <inheritdoc />
            public event EventHandler<Keys> KeyUp;

            /// <inheritdoc />
            public event EventHandler<int> MouseWheel;

            /// <inheritdoc />
            public event EventHandler LeftMouseDown;

            /// <inheritdoc />
            public event EventHandler LeftMouseUp;

            /// <inheritdoc />
            public event EventHandler RightMouseDown;

            /// <inheritdoc />
            public event EventHandler RightMouseUp;

            /// <inheritdoc />
            public event EventHandler MiddleMouseDown;

            /// <inheritdoc />
            public event EventHandler MiddleMouseUp;

            /// <inheritdoc />
            public event EventHandler<System.Numerics.Vector2> Resize;

            /// <inheritdoc />
            public event EventHandler GotFocus;

            /// <inheritdoc />
            int IViewContext.Width => parent.ClientSize.X;

            /// <inheritdoc />
            int IViewContext.Height => parent.ClientSize.Y;

            /// <inheritdoc />
            public bool IsFocused => parent.IsFocused;

            /// <inheritdoc />
            public System.Numerics.Vector2 CursorPosition
            {
                get
                {
                    return new System.Numerics.Vector2(parent.MousePosition.X, parent.MousePosition.Y);
                }

                set
                {
                    parent.MousePosition = new Vector2(value.X, value.Y);
                }
            }

            /// <inheritdoc />
            public bool ShowCursor
            {
                set
                {
                    parent.CursorVisible = value;
                }
            }

            /// <inheritdoc />
            public void Close()
            {
                parent.Close();
            }
        }
    }
}
