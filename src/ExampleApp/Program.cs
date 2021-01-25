using MyOTKE.Engine;
using MyOTKE.Engine.Components.BasicExamples;
using MyOTKE.Engine.Components.Gui;
using MyOTKE.Engine.Components.ReactivePrimitives;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;

namespace MyOTKE.ExampleApp
{
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        public static void Main()
        {
            var window = new MyOTKEWindow(
                gameWindowSettings: GameWindowSettings.Default,
                nativeWindowSettings: NativeWindowSettings.Default,
                lockCursor: false,
                clearColor: Color.Black())
            {
                Title = "MyOTKE Example",
                WindowState = WindowState.Fullscreen,
                WindowBorder = WindowBorder.Resizable,
            };

            // Obviously not ideal to have to set font globally - need to sort this out, probably via some nice
            // texture management stuff in the core lib.
            Text.Font = new Font(@"Assets\Fonts\Inconsolata\Inconsolata-Regular.ttf");

            // MyOTKEWindow has a RootComponent property. Components can be composed of other components -
            // there's even a handy CompositeComponent base class to make this easy - see the two examples
            // below, and note that MenuComponent (which consists of a single GUI component and has
            // no overrides) is only a class of its own to make disposal on button press easy.
            window.RootComponent = new MenuComponent(window);

            window.Run();
        }

        private class MenuComponent : CompositeComponent
        {
            public MenuComponent(MyOTKEWindow view)
            {
                view.LockCursor = false;

                var systemInfo = new StringBuilder();
                systemInfo.AppendLine($"OpenGl version: {GL.GetString(StringName.Version)}");
                systemInfo.AppendLine($"OpenGl Shading Language version: {GL.GetString(StringName.ShadingLanguageVersion)}");
                systemInfo.AppendLine($"Vendor: {GL.GetString(StringName.Vendor)}");
                systemInfo.AppendLine($"Renderer: {GL.GetString(StringName.Renderer)}");

                AddComponent(new Gui(view, 1000)
                {
                    SubElements =
                    {
                        new Button(
                            layout: new Layout((0f, 0f), (0f, 0f), (200, 40), new Vector2(0, 70)),
                            color: Color.Blue(.5f),
                            textColor: Color.White(),
                            text: "FIRST PERSON DEMO",
                            v =>
                            {
                                view.RootComponent = new FirstPersonComponent(view);
                                this.Dispose();
                            }),
                        new Button(
                            layout: new Layout((0f, 0f), (0f, 0f), (200, 40), new Vector2(0, 10)),
                            color: Color.Blue(.5f),
                            textColor: Color.White(),
                            text: "ORBIT DEMO",
                            v =>
                            {
                                view.RootComponent = new OrbitComponent(view);
                                this.Dispose();
                            }),
                        new Button(
                            layout: new Layout((0f, 0f), (0f, 0f), (200, 40), new Vector2(0, -50)),
                            color: Color.Blue(.5f),
                            textColor: Color.White(),
                            text: "QUIT",
                            v => view.Close()),
                        new Text(
                            layout: new Layout((0f, -1f), (0f, -1f), (1f, 100)),
                            color: Color.Grey(0.7f),
                            content: systemInfo.ToString())
                        {
                            HorizontalAlignment = 0.5f,
                        },
                    },
                });
            }
        }

        private class FirstPersonComponent : CompositeComponent
        {
            private readonly MyOTKEWindow view;
            private readonly ICamera camera;

            private readonly ColoredLines lines;
            private readonly Text camTextElement;
            private readonly TextStream logElement;

            private readonly Subject<IList<Primitive>> cubeSubject = new Subject<IList<Primitive>>();
            private readonly Primitive[] cubePrimitives = new Primitive[1] { Primitive.Empty() };
            private Matrix4 cubeWorldMatrix = Matrix4.Identity;
            private Vector3 lastCamPosition = Vector3.Zero;

            public FirstPersonComponent(MyOTKEWindow view)
            {
                this.view = view;
                camera = new FirstPersonCamera(
                    view,
                    movementSpeed: 3.0f,
                    rotationSpeed: 0.005f,
                    fieldOfViewRadians: (float)Math.PI / 4.0f,
                    nearPlaneDistance: 0.1f,
                    farPlaneDistance: 100f,
                    initialPosition: new Vector3(0f, 0f, 3f),
                    initialHorizontalAngleRadians: (float)Math.PI,
                    initialVerticalAngleRadians: 0f);

                var texturedTriangleVertices = new[]
                {
                    new TexturedStaticMesh.Vertex(
                        new Vector3(-1f, -1f, -2f),
                        new Vector2(0f, 0f),
                        new Vector3(0f, 0f, 1f)),
                    new TexturedStaticMesh.Vertex(
                        new Vector3(1f, -1f, -2f),
                        new Vector2(1f, 0f),
                        new Vector3(0f, 0f, 1f)),
                    new TexturedStaticMesh.Vertex(
                        new Vector3(0f, 1f, -2f),
                        new Vector2(0.5f, 1f),
                        new Vector3(0f, 0f, 1f)),
                };
                AddComponent(new TexturedStaticMesh(
                    camera,
                    texturedTriangleVertices,
                    new uint[] { 0, 1, 2 },
                    @"Assets\Textures\foo.bmp")
                {
                    AmbientLightColor = Color.Grey(),
                });

                var coloredTriangleVertices = new[]
                {
                    new ColoredStaticMesh.Vertex(
                        new Vector3(2f, 2f, -3f),
                        Color.Red(),
                        new Vector3(0f, 0f, 1f)),
                    new ColoredStaticMesh.Vertex(
                        new Vector3(-2f, 2f, -3f),
                        Color.Green(),
                        new Vector3(0f, 0f, 1f)),
                    new ColoredStaticMesh.Vertex(
                        new Vector3(0f, -2f, -3f),
                        Color.Blue(),
                        new Vector3(0f, 0f, 1f)),
                };
                AddComponent(new ColoredStaticMesh(
                    camera,
                    coloredTriangleVertices,
                    new uint[] { 0, 1, 2 })
                {
                    AmbientLightColor = Color.Grey(),
                });

                AddComponent(new PrimitiveRenderer(camera, Observable.Return(cubeSubject), 12)
                {
                    AmbientLightColor = Color.Grey(0.1f),
                    DirectedLightDirection = new Vector3(0, 1f, 0f),
                    DirectedLightColor = Color.Grey(),
                });

                AddComponent(lines = new ColoredLines(camera)
                {
                    AmbientLightColor = Color.Grey(),
                });

                camTextElement = new Text(
                    new Layout((-1f, 1f), (-1f, 1f), (1f, 0f)),
                    color: Color.White());

                logElement = new TextStream(
                    new Layout((-1f, 1f), (-1f, 1f), (1f, 0f), new Vector2(0, -100)),
                    textColor: Color.White(),
                    10);

                AddComponent(new Gui(view, 1000)
                {
                    SubElements =
                    {
                        new Panel(
                            layout: new Layout((-1f, 0f), (-1f, 0f), (250, 1f)),
                            color: Color.White(0.05f))
                        {
                            SubElements =
                            {
                                camTextElement,
                                logElement,
                            },
                        },
                    },
                });
            }

            public override void Update(TimeSpan elapsed)
            {
                base.Update(elapsed);

                camera.Update(elapsed);

                // Avoid GC pressure for string unless needed - would be better to do this reactively though
                // (e.g. reactive linq to take at intervals or debounce)
                if (camera.Position != lastCamPosition)
                {
                    camTextElement.Content = $"Cam@{camera.Position:F2}\n\nUse WASD to move the camera\nClick LMB to add a ray\nPress SPACE to toggle cam mode\nPress q to quit";
                    lastCamPosition = camera.Position;
                }

                // NB: No new heap allocations each time to avoid GC pressure - same array, same primitive.
                // Could do with more helpers to make this easier. Perhaps Primitive should be a struct after all..
                cubeWorldMatrix *= Matrix4.CreateRotationZ((float)elapsed.TotalSeconds);
                cubeWorldMatrix *= Matrix4.CreateRotationY((float)elapsed.TotalSeconds / 2);
                cubePrimitives[0].SetCuboid(new Vector3(.5f, 1f, 0.75f), cubeWorldMatrix, Color.Red());
                cubeSubject.OnNext(cubePrimitives);

                if (view.IsMouseButtonReleased(MouseButton.Left))
                {
                    var ray = new Ray(camera, view);
                    lines.AddLine(ray.Origin, ray.Origin + ray.Direction * 10);
                    logElement.PushMessage($"RAY FROM {ray.Origin:F2}");
                }

                if (view.IsKeyReleased(Keys.Space))
                {
                    view.LockCursor = !view.LockCursor;
                }

                if (view.IsKeyReleased(Keys.Q))
                {
                    view.RootComponent = new MenuComponent(view);
                    this.Dispose();
                }
            }
        }

        private class OrbitComponent : CompositeComponent
        {
            private readonly MyOTKEWindow view;
            private readonly ICamera camera;

            private readonly Text camTextElement;
            private readonly TextStream logElement;

            private readonly Subject<IList<Primitive>> cubeSubject = new Subject<IList<Primitive>>();
            private readonly Primitive[] cubePrimitives = new Primitive[1] { Primitive.Empty() };
            private Matrix4 cubeWorldMatrix = Matrix4.Identity;
            private Vector3 lastCamPosition = Vector3.Zero;

            public OrbitComponent(MyOTKEWindow view)
            {
                this.view = view;
                camera = new OrbitCameraAligned(
                    view,
                    rotationSpeedBase: 0.5f,
                    fieldOfViewRadians: (float)Math.PI / 4.0f,
                    nearPlaneDistance: 0.1f,
                    farPlaneDistance: 100f)
                {
                };

                AddComponent(new PrimitiveRenderer(camera, Observable.Return(cubeSubject), 12)
                {
                    AmbientLightColor = Color.Grey(0.1f),
                    DirectedLightDirection = new Vector3(0, 1f, 0f),
                    DirectedLightColor = Color.Grey(),
                });

                camTextElement = new Text(
                    new Layout((-1f, 1f), (-1f, 1f), (1f, 0f)),
                    color: Color.White());

                logElement = new TextStream(
                    new Layout((-1f, 1f), (-1f, 1f), (1f, 0f), new Vector2(0, -100)),
                    textColor: Color.White(),
                    10);

                AddComponent(new Gui(view, 1000)
                {
                    SubElements =
                    {
                        new Panel(
                            layout: new Layout((-1f, 0f), (-1f, 0f), (250, 1f)),
                            color: Color.White(0.05f))
                        {
                            SubElements =
                            {
                                camTextElement,
                                logElement,
                            },
                        },
                    },
                });
            }

            public override void Update(TimeSpan elapsed)
            {
                base.Update(elapsed);

                camera.Update(elapsed);

                // Avoid GC pressure for string unless needed - would be better to do this reactively though
                // (e.g. reactive linq to take at intervals or debounce)
                if (camera.Position != lastCamPosition)
                {
                    camTextElement.Content = $"Cam@{camera.Position:F2}\n\nUse WASD to move the camera\nClick LMB to add a ray\nPress SPACE to toggle cam mode\nPress q to quit";
                    lastCamPosition = camera.Position;
                }

                if (view.IsKeyReleased(Keys.Q))
                {
                    view.RootComponent = new MenuComponent(view);
                    this.Dispose();
                }
            }
        }
    }
}
