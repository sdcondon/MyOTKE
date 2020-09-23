/*
#include <stdio.h>
#ifdef NANOVG_GLEW
#	include <GL/glew.h>
#endif
#ifdef __APPLE__
#	define GLFW_INCLUDE_GLCOREARB
#endif
#define GLFW_INCLUDE_GLEXT
#include <GLFW/glfw3.h>
#include "nanovg.h"
#define NANOVG_GL3_IMPLEMENTATION
#include "nanovg_gl.h"
#include "demo.h"
#include "perf.h"
*/

using OpenTK.Input;
using OpenTK.Graphics.OpenGL;
using System;
using OpenTK;
using OpenTK.Graphics;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace NanoVG
{
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    public static class Program
    {
        static bool blowup = false;
        static bool screenshot = false;
        static bool premult = false;

        public static void Main()
        {
            // This line creates a new instance, and wraps the instance in a using statement so it's automatically disposed once we've exited the block.
            using (Game game = new Game(800, 600, "LearnOpenTK"))
            {
                // Run takes a double, which is how many frames per second it should strive to reach.
                // You can leave that out and it'll just update as fast as the hardware will allow it.
                game.Run(60.0);
            }
        }

//        int main()
//        {
//            if (!glfwInit())
//            {
//                printf("Failed to init GLFW.");
//                return -1;
//            }

//            initGraph(out var fps, GRAPH_RENDER_FPS, "Frame Time");
//            initGraph(out var cpuGraph, GRAPH_RENDER_MS, "CPU Time");
//            initGraph(out var gpuGraph, GRAPH_RENDER_MS, "GPU Time");

//            glfwSetErrorCallback(errorcb);
//#if !_WIN32 // don't require this on win32, and works with more cards
//            glfwWindowHint(GLFW_CONTEXT_VERSION_MAJOR, 3);
//            glfwWindowHint(GLFW_CONTEXT_VERSION_MINOR, 2);
//            glfwWindowHint(GLFW_OPENGL_FORWARD_COMPAT, GL_TRUE);
//            glfwWindowHint(GLFW_OPENGL_PROFILE, GLFW_OPENGL_CORE_PROFILE);
//#endif
//            glfwWindowHint(GLFW_OPENGL_DEBUG_CONTEXT, 1);

//#if DEMO_MSAA
//            glfwWindowHint(GLFW_SAMPLES, 4);
//#endif
//            var window = glfwCreateWindow(1000, 600, "NanoVG", NULL, NULL);
//            //	window = glfwCreateWindow(1000, 600, "NanoVG", glfwGetPrimaryMonitor(), NULL);
//            if (!window)
//            {
//                glfwTerminate();
//                return -1;
//            }

//            glfwSetKeyCallback(window, key);

//            glfwMakeContextCurrent(window);
//#if NANOVG_GLEW
//            glewExperimental = GL_TRUE;
//            if (glewInit() != GLEW_OK)
//            {
//                printf("Could not init glew.\n");
//                return -1;
//            }

//            // GLEW generates GL error because it calls glGetString(GL_EXTENSIONS), we'll consume it here.
//            GL.GetError();
//#endif

//#if DEMO_MSAA
//            var vg = NanoVGGL.nvgCreateGL3(NanoVGGL.CreateFlags.NVG_STENCIL_STROKES | NanoVGGL.CreateFlags.NVG_DEBUG);
//#else
//            var vg = NanoVGGL.nvgCreateGL3(NanoVGGL.CreateFlags.NVG_ANTIALIAS | NanoVGGL.CreateFlags.NVG_STENCIL_STROKES | NanoVGGL.CreateFlags.NVG_DEBUG);
//#endif

//            Demo.LoadDemoData(vg, out var data);

//            glfwSwapInterval(0);

//            initGPUTimer(out var gpuTimer);

//            glfwSetTime(0);
//            var prevt = glfwGetTime();

//            while (!glfwWindowShouldClose(window))
//            {
//                float gpuTimes[3];

//                double t = glfwGetTime();
//                double dt = t - prevt;
//                prevt = t;

//                startGPUTimer(&gpuTimer);

//                glfwGetCursorPos(window, out double mx, out double my);
//                glfwGetWindowSize(window, out int winWidth, out int winHeight);
//                glfwGetFramebufferSize(window, out int fbWidth, out int fbHeight);

//                // Calculate pixel ratio for hi-dpi devices.
//                float pxRatio = (float)fbWidth / (float)winWidth;

//                // Update and render
//                GL.Viewport(0, 0, fbWidth, fbHeight);
//                if (premult)
//                {
//                    GL.ClearColor(0, 0, 0, 0);
//                }
//                else
//                {
//                    GL.ClearColor(0.3f, 0.3f, 0.32f, 1.0f);
//                }

//                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);

//                NVG.BeginFrame(vg, winWidth, winHeight, pxRatio);

//                Demo.RenderDemo(vg, (float)mx, (float)my, winWidth, winHeight, t, blowup, data);

//                renderGraph(vg, 5, 5, &fps);
//                renderGraph(vg, 5 + 200 + 5, 5, &cpuGraph);
//                if (gpuTimer.supported)
//                {
//                    renderGraph(vg, 5 + 200 + 5 + 200 + 5, 5, &gpuGraph);
//                }

//                NVG.EndFrame(vg);

//                // Measure the CPU time taken excluding swap buffers (as the swap may wait for GPU)
//                var cpuTime = glfwGetTime() - t;

//                updateGraph(&fps, dt);
//                updateGraph(&cpuGraph, cpuTime);

//                // We may get multiple results.
//                int n = stopGPUTimer(&gpuTimer, gpuTimes, 3);
//                for (int i = 0; i < n; i++)
//                {
//                    updateGraph(&gpuGraph, gpuTimes[i]);
//                }

//                if (screenshot)
//                {
//                    screenshot = false;
//                    Demo.SaveScreenShot(fbWidth, fbHeight, premult, "dump.png");
//                }

//                glfwSwapBuffers(window);
//                glfwPollEvents();
//            }

//            Demo.FreeDemoData(vg, data);

//            NanoVGGL.nvgDeleteGL3(vg);

//            printf("Average Frame Time: %.2f ms\n", getGraphAverage(&fps) * 1000.0f);
//            printf("          CPU Time: %.2f ms\n", getGraphAverage(&cpuGraph) * 1000.0f);
//            printf("          GPU Time: %.2f ms\n", getGraphAverage(&gpuGraph) * 1000.0f);

//            glfwTerminate();
//            return 0;
//        }
    }

    public class Game : GameWindow
    {
        private Context vg;

        static bool blowup = false;
        static bool screenshot = false;
        static bool premult = false;
        DemoData data;

        public Game(int width, int height, string title)
            : base(width, height, GraphicsMode.Default, title)
        {
        }

        protected override void OnLoad(EventArgs e)
        {
            GL.Enable(EnableCap.DebugOutput);
            GL.DebugMessageCallback(
                (source, type, id, severity, length, message, userParam) =>
                {
                    var marshalledMessage = Marshal.PtrToStringAnsi(message, length);
                    Debug.WriteLine($"[{id}] {source} {type} {severity}: {marshalledMessage}", "OPENGL");
                },
                IntPtr.Zero);

            GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);

#if DEMO_MSAA
            vg = GLNVGcontext.nvgCreateGL3(CreateFlags.NVG_STENCIL_STROKES | CreateFlags.NVG_DEBUG);
#else
            vg = GLNVGcontext.nvgCreateGL3(CreateFlags.NVG_ANTIALIAS | CreateFlags.NVG_STENCIL_STROKES | CreateFlags.NVG_DEBUG);
#endif

            Demo.LoadDemoData(vg, out data);

            base.OnLoad(e);
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            int fbWidth = this.ClientSize.Width;
            int fbHeight = this.ClientSize.Height;
            int winWidth = this.ClientSize.Width;
            int winHeight = this.ClientSize.Height;

            // Calculate pixel ratio for hi-dpi devices.
            float pxRatio = (float)fbWidth / (float)winWidth;

            GL.Viewport(0, 0, fbWidth, fbHeight);
            if (premult)
            {
                GL.ClearColor(0, 0, 0, 0);
            }
            else
            {
                GL.ClearColor(0.3f, 0.3f, 0.32f, 1.0f);
            }

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);

            vg.BeginFrame(winWidth, winHeight, pxRatio);

            var mstate = Mouse.GetCursorState();
            Demo.RenderDemo(vg, mstate.X, mstate.Y, winWidth, winHeight, (float)RenderPeriod, blowup, data);

            //renderGraph(vg, 5, 5, &fps);
            //renderGraph(vg, 5 + 200 + 5, 5, &cpuGraph);
            //if (gpuTimer.supported)
            //{
            //    renderGraph(vg, 5 + 200 + 5 + 200 + 5, 5, &gpuGraph);
            //}

            vg.EndFrame();

            Context.SwapBuffers();
            base.OnRenderFrame(e);
        }

        protected override void OnResize(EventArgs e)
        {
            GL.Viewport(0, 0, Width, Height);
            base.OnResize(e);
        }

        protected override void OnUnload(EventArgs e)
        {
            Demo.FreeDemoData(vg, data);
            GLNVGcontext.nvgDeleteGL3(vg);
            base.OnUnload(e);
        }

        protected override void OnKeyUp(KeyboardKeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Exit();
            }

            if (e.Key == Key.Space)
            {
                blowup = !blowup;
            }

            if (e.Key == Key.S)
            {
                screenshot = true;
            }

            if (e.Key == Key.P)
            {
                premult = !premult;
            }

            base.OnKeyUp(e);
        }
    }
}
