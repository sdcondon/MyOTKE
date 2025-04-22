using MyOTKE.Components;
using MyOTKE.Engine;
using MyOTKE.Engine.Components.Gui;
using MyOTKE.Engine.Components.Gui.Elements;
using MyOTKE.Engine.Utility;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using System.Text;

namespace MyOTKE.ExampleApp.Components;

public class MenuComponent : CompositeComponent
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
