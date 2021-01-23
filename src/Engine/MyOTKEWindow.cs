using MyOTKE.Core;
using OpenTK.Graphics.OpenGL;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using System;
using System.Diagnostics;

namespace MyOTKE.Engine
{
    /// <summary>
    /// Encapsulates an interactive view rendered with OpenGl.
    /// </summary>
    /// <remarks>
    /// See https://opentk.net/learn/chapter1/1-creating-a-window.html for tutorial.
    /// </remarks>
    public sealed class MyOTKEWindow : GameWindow
    {
        private readonly Color clearColor;
        private readonly Stopwatch modelUpdateIntervalStopwatch = new Stopwatch();

        private IComponent renderable;
        private bool lockCursor;
        private bool isContextCreated;

        /// <summary>
        /// Initializes a new instance of the <see cref="MyOTKEWindow"/> class.
        /// </summary>
        /// <param name="gameWindowSettings">The GameWindow related settings.</param>
        /// <param name="nativeWindowSettings">The NativeWindow related settings.</param>
        /// <param name="lockCursor">A value indicating whether the cursor is placed back at the center of the view during each update.</param>
        /// <param name="clearColor">The color to clear the view with on each render call.</param>
        public MyOTKEWindow(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings, bool lockCursor, Color clearColor)
             : base(gameWindowSettings, nativeWindowSettings)
        {
            GlDebug.RegisterDebugCallback();

            this.LockCursor = lockCursor;
            this.clearColor = clearColor;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the cursor is placed back at the center of the view during each update.
        /// </summary>
        public bool LockCursor
        {
            get => lockCursor;
            set
            {
                CursorVisible = !value;
                lockCursor = value;
            }
        }

        /// <summary>
        /// Gets the aspect ratio of the view.
        /// </summary>
        public float AspectRatio => (float)ClientSize.X / ClientSize.Y;

        /// <summary>
        /// Gets the position of the center of a view, given its size.
        /// </summary>
        public System.Numerics.Vector2 Center => new System.Numerics.Vector2(ClientSize.X / 2, ClientSize.Y / 2);

        /// <summary>
        /// Gets the offset to the current mouse position from the center of the view.
        /// </summary>
        public System.Numerics.Vector2 CenterOffset => new System.Numerics.Vector2(MousePosition.X, MousePosition.Y) - Center;

        /// <summary>
        /// Gets or sets the root renderable of the view.
        /// </summary>
        public IComponent Renderable
        {
            get => renderable;
            set
            {
                if (isContextCreated)
                {
                    value.Load();
                }

                renderable = value;
            }
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Renderable.Dispose();
            }

            base.Dispose(disposing);
        }

        /// <inheritdoc />
        protected override void OnLoad()
        {
            GL.ClearColor(clearColor.R, clearColor.G, clearColor.B, clearColor.A);
            GL.Enable(EnableCap.DepthTest); // Enable depth test
            GL.DepthFunc(DepthFunction.Lequal); // Accept fragment if it closer to the camera than the former one
            GL.Enable(EnableCap.CullFace); // Cull triangles of which normal is not towards the camera

            // Transparency
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            renderable.Load();
            isContextCreated = true;
        }

        /// <inheritdoc />
        protected override void OnRenderFrame(FrameEventArgs eventArgs)
        {
            base.OnRenderFrame(eventArgs);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            renderable.Render();

            Context.SwapBuffers();
        }

        /// <inheritdoc />
        protected override void OnUpdateFrame(FrameEventArgs eventArgs)
        {
            if (IsFocused)
            {
                // Record update interval and restart stopwatch for it
                // Cap the effective elapsed time so that at worst,
                // the action will slow down as opposed to stuff jumping through walls..
                var elapsed = modelUpdateIntervalStopwatch.Elapsed;
                modelUpdateIntervalStopwatch.Restart();
                var maxEffectiveElapsed = TimeSpan.FromSeconds(0.1);
                if (elapsed > maxEffectiveElapsed)
                {
                    elapsed = maxEffectiveElapsed;
                }

                // Update the game world
                renderable.Update(elapsed);

                // Reset user input properties
                if (this.lockCursor)
                {
                    this.MousePosition = ClientSize.ToVector2() / 2;
                }
            }
        }

        /// <inheritdoc />
        protected override void OnUnload()
        {
            Dispose();
        }

        /// <inheritdoc />
        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);
            GL.Viewport(0, 0, (int)e.Size.X, (int)e.Size.Y);
        }

        /// <inheritdoc />
        protected override void OnFocusedChanged(FocusedChangedEventArgs e)
        {
            base.OnFocusedChanged(e);

            if (e.IsFocused && this.lockCursor)
            {
                MousePosition = ClientSize.ToVector2() / 2;
            }
        }
    }
}
