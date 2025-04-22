using MyOTKE.Engine;
using MyOTKE.Engine.Components.Gui;
using MyOTKE.Engine.Components.Gui.Elements;
using MyOTKE.Engine.Components.ReactivePrimitives;
using MyOTKE.Engine.Components.ReactivePrimitives.Primitives;
using MyOTKE.Engine.Utility;
using MyOTKE.Engine.Utility.Cameras;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace MyOTKE.ExampleApp.Components;

public class OrbitComponent : CompositeComponent
{
    private readonly MyOTKEWindow view;
    private readonly OrbitCameraAligned camera;

    private readonly Text camTextElement;
    private readonly TextStream logElement;

    private readonly Subject<IList<Primitive>> cubeSubject = new();
    private readonly Primitive[] cubePrimitives = [Primitive.Cuboid(Vector3.One, Matrix4.Identity, Color.Red())];
    private Vector3 lastCamPosition = Vector3.Zero;

    public OrbitComponent(MyOTKEWindow view)
    {
        this.view = view;
        camera = new OrbitCameraAligned(
            view,
            rotationSpeedBase: 0.5f,
            fieldOfViewRadians: (float)Math.PI / 4.0f,
            nearPlaneDistance: 0.1f,
            farPlaneDistance: 100f);

        AddComponent(new PrimitiveRenderer(camera, Observable.Return(cubeSubject), 12)
        {
            AmbientLightColor = Color.Grey(0.1f),
            DirectedLightDirection = new Vector3(.2f, .4f, .6f),
            DirectedLightColor = Color.Grey(0.8f),
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

    /// <inheritdoc />
    public override void Update(TimeSpan elapsed)
    {
        base.Update(elapsed);

        camera.Update(elapsed);

        // todo: dumb shouldnt need to push it every time. should prob ditch reactivex.
        // or observable.defer?
        cubeSubject.OnNext(cubePrimitives);

        // Avoid GC pressure for string unless needed - would be better to do this reactively though
        // (e.g. reactive linq to take at intervals or debounce)
        var cameraPosition = camera.GetPosition();
        if (camera.GetPosition() != lastCamPosition)
        {
            camTextElement.Content = $"Cam@{cameraPosition:F2}\n\nUse WASD to move the camera\nPress q to quit";
            lastCamPosition = cameraPosition;
        }

        if (view.IsKeyReleased(Keys.Q))
        {
            view.RootComponent = new MenuComponent(view);
            this.Dispose();
        }
    }
}
