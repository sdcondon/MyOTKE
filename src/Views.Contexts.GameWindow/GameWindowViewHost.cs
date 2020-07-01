using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;
using System;
using System.Drawing;

namespace MyOTKE.Views.Contexts.GameWindow
{
    /// <summary>
    /// Windows form containing only a single OpenGL render control.
    /// </summary>
    /// <remarks>
    /// See https://opentk.net/learn/chapter1/1-creating-a-window.html for tutorial.
    /// </remarks>
    public sealed class GameWindowViewHost : OpenTK.GameWindow
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GameWindowViewHost"/> class.
        /// </summary>
        public GameWindowViewHost()
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
            : base(width, height, GraphicsMode.Default, title)
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
                parent.Load += (s, a) => Loading?.Invoke(this, a);
                parent.RenderFrame += (s, a) => RenderingFrame?.Invoke(this, a);
                parent.UpdateFrame += (s, a) => Updating?.Invoke(this, a);
                parent.Unload += (s, a) => Unloading?.Invoke(this, a);
                parent.KeyDown += (s, a) => KeyDown?.Invoke(this, a.Key);
                parent.KeyUp += (s, a) => KeyUp?.Invoke(this, a.Key);
                parent.MouseWheel += (s, a) => MouseWheel?.Invoke(this, a.Delta);
                parent.MouseDown += (s, a) =>
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
                parent.MouseUp += (s, a) =>
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
                parent.Resize += (s, a) => Resize?.Invoke(
                    this,
                    new System.Numerics.Vector2(parent.ClientSize.Width, parent.ClientSize.Height));
                parent.FocusedChanged += (s, a) => GotFocus?.Invoke(this, EventArgs.Empty);
            }

            /// <inheritdoc />
            public event EventHandler Loading;

            /// <inheritdoc />
            public event EventHandler<FrameEventArgs> RenderingFrame;

            /// <inheritdoc />
            public event EventHandler Updating;

            /// <inheritdoc />
            public event EventHandler Unloading;

            /// <inheritdoc />
            public event EventHandler<Key> KeyDown;

            /// <inheritdoc />
            public event EventHandler<Key> KeyUp;

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
            int IViewContext.Width => parent.ClientSize.Width;

            /// <inheritdoc />
            int IViewContext.Height => parent.ClientSize.Height;

            /// <inheritdoc />
            public bool IsFocused => parent.Focused;

            /// <inheritdoc />
            System.Numerics.Vector2 IViewContext.CursorPosition
            {
                get
                {
                    var state = Mouse.GetCursorState();
                    var point = parent.PointToClient(new Point(state.X, state.Y));
                    return new System.Numerics.Vector2(point.X, point.Y);
                }

                set
                {
                    var point = parent.PointToScreen(new Point((int)value.X, (int)value.Y));
                    Mouse.SetPosition(point.X, point.Y);
                }
            }

            /// <inheritdoc />
            public bool ShowCursor
            {
                set
                {
                    if (value)
                    {
                        parent.CursorVisible = value;
                    }
                    else
                    {
                        parent.CursorVisible = value;
                    }
                }
            }

            /// <inheritdoc />
            public void Exit()
            {
                parent.Exit();
            }
        }
    }
}
