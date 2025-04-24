using MyOTKE.Cameras;
using MyOTKE.Components;
using MyOTKE.Components.Primitives;
using MyOTKE.Components.Reactive.Gui;
using MyOTKE.Components.Reactive.Gui.Elements;
using MyOTKE.Components.StaticMeshes;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;

namespace MyOTKE.ExampleApp.Components;

public class FirstPersonComponent : CompositeComponent
{
    private readonly MyOTKEWindow view;
    private readonly FirstPersonCamera camera;

    private readonly ColoredLines lines;
    private readonly Text camTextElement;
    private readonly TextStream logElement;

    private readonly TrianglesPrimitive cube = new();
    private Matrix4 cubeWorldMatrix = Matrix4.Identity;
    private Vector3 lastCamPosition = Vector3.Zero;

    public FirstPersonComponent(MyOTKEWindow view)
    {
        this.view = view;
        camera = new FirstPersonCamera(
            view,
            movementSpeed: 3.0f,
            rotationSpeed: (float)Math.PI / 2.0f,
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
            [ 0, 1, 2 ],
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
            [ 0, 1, 2 ])
        {
            AmbientLightColor = Color.Grey(),
        });

        AddComponent(new PrimitivesComponent(camera, [cube], 12)
        {
            AmbientLightColor = Color.Grey(0.1f),
            DirectedLightDirection = new Vector3(0, 1f, 0f),
            DirectedLightColor = Color.Grey(),
        });

        AddComponent(lines = new(camera)
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
        var cameraPosition = camera.GetPosition();
        if (cameraPosition != lastCamPosition)
        {
            camTextElement.Content = $"Cam@{cameraPosition:F2}\n\nUse WASD to move the camera\nClick LMB to add a ray\nPress SPACE to toggle cam mode\nPress q to quit";
            lastCamPosition = cameraPosition;
        }

        // NB: No new heap allocations each time to avoid GC pressure - same array, same primitive.
        // Could do with more helpers to make this easier. Perhaps Primitive should be a struct after all..
        cubeWorldMatrix *= Matrix4.CreateRotationZ((float)elapsed.TotalSeconds);
        cubeWorldMatrix *= Matrix4.CreateRotationY((float)elapsed.TotalSeconds / 2);
        cube.SetAsCuboid(new Vector3(.5f, 1f, 0.75f), cubeWorldMatrix, Color.Red());

        if (view.IsMouseButtonReleased(MouseButton.Left))
        {
            var ray = new Ray(camera, view);
            lines.AddLine(ray.Origin, ray.Origin + ray.Direction * 10);
            logElement.PushMessage($"RAY FROM {ray.Origin:F2}");
        }

        if (view.IsKeyReleased(Keys.Space))
        {
            view.LockCursor = !view.LockCursor;

            // BUG: is this a OTK bug? First update after switching back to mouse aim registers large
            // centeroffset if mouse was away from center - despite LockCursor being set between cam updates.
            // view direction thus jumps..
            ////view.MousePosition = view.ClientRectangle.Center; // Doesn't work, perhaps unsurprisingly
        }

        if (view.IsKeyReleased(Keys.Q))
        {
            view.RootComponent = new MenuComponent(view);
            this.Dispose();
        }
    }
}
