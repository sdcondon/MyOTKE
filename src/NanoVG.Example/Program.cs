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
using System.Collections.Generic;
using System.Numerics;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
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

        static void errorcb(int error, string desc)
        {
            Debug.WriteLine("GLFW error %d: %s\n", error, desc);
        }

        static void key(GLFWwindow* window, int key, int scancode, int action, int mods)
        {
            ////NVG_NOTUSED(scancode);
            ////NVG_NOTUSED(mods);
            if (key == GLFW_KEY_ESCAPE && action == GLFW_PRESS)
                glfwSetWindowShouldClose(window, GL_TRUE);
            if (key == GLFW_KEY_SPACE && action == GLFW_PRESS)
                blowup = !blowup;
            if (key == GLFW_KEY_S && action == GLFW_PRESS)
                screenshot = 1;
            if (key == GLFW_KEY_P && action == GLFW_PRESS)
                premult = !premult;
        }

        int main()
        {
            GLFWwindow* window;
            DemoData data;
            Context vg = null;
            GPUtimer gpuTimer;
            PerfGraph fps, cpuGraph, gpuGraph;
            double prevt = 0, cpuTime = 0;

            if (!glfwInit())
            {
                printf("Failed to init GLFW.");
                return -1;
            }

            initGraph(&fps, GRAPH_RENDER_FPS, "Frame Time");
            initGraph(&cpuGraph, GRAPH_RENDER_MS, "CPU Time");
            initGraph(&gpuGraph, GRAPH_RENDER_MS, "GPU Time");

            glfwSetErrorCallback(errorcb);
#if !_WIN32 // don't require this on win32, and works with more cards
            glfwWindowHint(GLFW_CONTEXT_VERSION_MAJOR, 3);
            glfwWindowHint(GLFW_CONTEXT_VERSION_MINOR, 2);
            glfwWindowHint(GLFW_OPENGL_FORWARD_COMPAT, GL_TRUE);
            glfwWindowHint(GLFW_OPENGL_PROFILE, GLFW_OPENGL_CORE_PROFILE);
#endif
            glfwWindowHint(GLFW_OPENGL_DEBUG_CONTEXT, 1);

#if DEMO_MSAA
            glfwWindowHint(GLFW_SAMPLES, 4);
#endif
            window = glfwCreateWindow(1000, 600, "NanoVG", NULL, NULL);
            //	window = glfwCreateWindow(1000, 600, "NanoVG", glfwGetPrimaryMonitor(), NULL);
            if (!window)
            {
                glfwTerminate();
                return -1;
            }

            glfwSetKeyCallback(window, key);

            glfwMakeContextCurrent(window);
#if NANOVG_GLEW
            glewExperimental = GL_TRUE;
            if (glewInit() != GLEW_OK)
            {
                printf("Could not init glew.\n");
                return -1;
            }

            // GLEW generates GL error because it calls glGetString(GL_EXTENSIONS), we'll consume it here.
            GL.GetError();
#endif

#if DEMO_MSAA
            vg = NanoVGGL.nvgCreateGL3(NanoVGGL.CreateFlags.NVG_STENCIL_STROKES | NanoVGGL.CreateFlags.NVG_DEBUG);
#else
            vg = NanoVGGL.nvgCreateGL3(NanoVGGL.CreateFlags.NVG_ANTIALIAS | NanoVGGL.CreateFlags.NVG_STENCIL_STROKES | NanoVGGL.CreateFlags.NVG_DEBUG);
#endif
            if (vg == NULL)
            {
                printf("Could not init nanovg.\n");
                return -1;
            }

            if (Demo.LoadDemoData(vg, &data) == -1)
            {
                return -1;
            }

            glfwSwapInterval(0);

            initGPUTimer(&gpuTimer);

            glfwSetTime(0);
            prevt = glfwGetTime();

            while (!glfwWindowShouldClose(window))
            {
                double mx, my, t, dt;
                int winWidth, winHeight;
                int fbWidth, fbHeight;
                float pxRatio;
                float gpuTimes[3];
                int i, n;

                t = glfwGetTime();
                dt = t - prevt;
                prevt = t;

                startGPUTimer(&gpuTimer);

                glfwGetCursorPos(window, &mx, &my);
                glfwGetWindowSize(window, &winWidth, &winHeight);
                glfwGetFramebufferSize(window, &fbWidth, &fbHeight);

                // Calculate pixel ration for hi-dpi devices.
                pxRatio = (float)fbWidth / (float)winWidth;

                // Update and render
                GL.Viewport(0, 0, fbWidth, fbHeight);
                if (premult)
                    GL.ClearColor(0, 0, 0, 0);
                else
                    GL.ClearColor(0.3f, 0.3f, 0.32f, 1.0f);
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);

                NVG.BeginFrame(vg, winWidth, winHeight, pxRatio);

                Demo.renderDemo(vg, mx, my, winWidth, winHeight, t, blowup, &data);

                renderGraph(vg, 5, 5, &fps);
                renderGraph(vg, 5 + 200 + 5, 5, &cpuGraph);
                if (gpuTimer.supported)
                    renderGraph(vg, 5 + 200 + 5 + 200 + 5, 5, &gpuGraph);

                NVG.EndFrame(vg);

                // Measure the CPU time taken excluding swap buffers (as the swap may wait for GPU)
                cpuTime = glfwGetTime() - t;

                updateGraph(&fps, dt);
                updateGraph(&cpuGraph, cpuTime);

                // We may get multiple results.
                n = stopGPUTimer(&gpuTimer, gpuTimes, 3);
                for (i = 0; i < n; i++)
                    updateGraph(&gpuGraph, gpuTimes[i]);

                if (screenshot)
                {
                    screenshot = false;
                    Demo.saveScreenShot(fbWidth, fbHeight, premult, "dump.png");
                }

                glfwSwapBuffers(window);
                glfwPollEvents();
            }

            Demo.FreeDemoData(vg, &data);

            nvgDeleteGL3(vg);

            printf("Average Frame Time: %.2f ms\n", getGraphAverage(&fps) * 1000.0f);
            printf("          CPU Time: %.2f ms\n", getGraphAverage(&cpuGraph) * 1000.0f);
            printf("          GPU Time: %.2f ms\n", getGraphAverage(&gpuGraph) * 1000.0f);

            glfwTerminate();
            return 0;
        }

    }
}
