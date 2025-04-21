using MyOTKE.Engine;
using MyOTKE.Engine.Components.Gui;
using MyOTKE.ExampleApp.Components;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using System;

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
            // Obviously not ideal to have to set font globally - need to sort this out, probably via some nice
            // texture management stuff in the core lib.
            Text.Font = new Font(@"Assets\Fonts\Inconsolata\Inconsolata-Regular.ttf");

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

            // MyOTKEWindow has a RootComponent property. Components can be composed of other components -
            // there's even a handy CompositeComponent base class to make this easy - see the two examples
            // below, and note that MenuComponent (which consists of a single GUI component and has
            // no overrides) is only a class of its own to make disposal on button press easy.
            window.RootComponent = new MenuComponent(window);

            window.Run();
        }
    }
}
