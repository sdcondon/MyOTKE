using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using System;
using System.Diagnostics;

namespace MyOTKE.Views
{
    /// <summary>
    /// Encapsulates an interactive view rendered with OpenGl.
    /// </summary>
    /// <remarks>
    /// See https://opentk.net/learn/chapter1/1-creating-a-window.html for tutorial.
    /// </remarks>
    public sealed class View : GameWindow
    {
        private readonly Color clearColor;
        private readonly Stopwatch modelUpdateIntervalStopwatch = new Stopwatch();

        private IRenderable renderable;
        private bool lockCursor;
        private bool isContextCreated;

        /// <summary>
        /// Initializes a new instance of the <see cref="View"/> class.
        /// </summary>
        public View()
            : base(GameWindowSettings.Default, NativeWindowSettings.Default)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="View"/> class.
        /// </summary>
        /// <param name="width">The width of the window in pixels.</param>
        /// <param name="height">The height of the window in pixels.</param>
        /// <param name="title">The title of the window.</param>
        public View(int width, int height, string title)
            : base(GameWindowSettings.Default, new NativeWindowSettings { Size = new Vector2i(width, height), Title = title })
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="View"/> class.
        /// </summary>
        /// <param name="lockCursor">A value indicating whether the cursor is placed back at the center of the view during each update.</param>
        /// <param name="clearColor">The color to clear the view with on each render call.</param>
        public View(bool lockCursor, Color clearColor)
             : base(GameWindowSettings.Default, NativeWindowSettings.Default)
        {
            Debug.WriteLine("Registering OpenGL debug handler");
            GL.DebugMessageCallback(OnGlDebugMessage, IntPtr.Zero);

            ////KhronosApi.LogEnabled = true;
            ////KhronosApi.Log += KhronosApi_Log;

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
        /// Gets the position of the center of a context, given its size.
        /// </summary>
        public System.Numerics.Vector2 Center => new System.Numerics.Vector2(ClientSize.X / 2, ClientSize.Y / 2);

        /// <summary>
        /// 
        /// </summary>
        public System.Numerics.Vector2 CenterOffset => new System.Numerics.Vector2(MousePosition.X, MousePosition.Y) - Center;

        /// <summary>
        /// Gets or sets the root renderable of the view.
        /// </summary>
        public IRenderable Renderable
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

        protected override void OnRenderFrame(FrameEventArgs eventArgs)
        {
            base.OnRenderFrame(eventArgs);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            renderable.Render();

            Context.SwapBuffers();
        }

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

        protected override void OnUnload()
        {
            Dispose();
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);
            GL.Viewport(0, 0, (int)e.Size.X, (int)e.Size.Y);
        }

        protected override void OnFocusedChanged(FocusedChangedEventArgs e)
        {
            base.OnFocusedChanged(e);

            if (e.IsFocused && this.lockCursor)
            {
                MousePosition = ClientSize.ToVector2() / 2;
            }
        }

        private void OnGlDebugMessage(
            DebugSource source,
            DebugType type,
            int id,
            DebugSeverity severity,
            int length,
            IntPtr message,
            IntPtr userParam)
        {
            Debug.WriteLine($"{id} {source} {type} {severity}", "OPENGL");
        }

        ////private void KhronosApi_Log(object sender, KhronosLogEventArgs e)
        ////{
        ////    Debug.WriteLine($"{e.Name}({string.Join(',', e.Args)}) {e.ReturnValue}", "KHRONOS API");
        ////}
        ///
    }
}
