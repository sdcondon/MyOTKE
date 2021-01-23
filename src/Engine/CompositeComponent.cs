using System;
using System.Collections.Generic;

namespace MyOTKE.Engine
{
    /// <summary>
    /// Base class for <see cref="IComponent"/> instances that are composed of sub-components.
    /// </summary>
    public abstract class CompositeComponent : IComponent, IDisposable
    {
        private readonly List<IComponent> renderables = new List<IComponent>();

        private bool contextCreated;

        /// <summary>
        /// Adds a sub-renderable.
        /// </summary>
        /// <param name="renderable">The sub-renderable to be added.</param>
        public void AddRenderable(IComponent renderable)
        {
            if (contextCreated)
            {
                renderable.Load();
            }

            renderables.Add(renderable);
        }

        /// <inheritdoc />
        public void Load()
        {
            for (int i = 0; i < renderables.Count; i++)
            {
                renderables[i].Load();
            }

            contextCreated = true;
        }

        /// <inheritdoc />
        public virtual void Update(TimeSpan elapsed)
        {
            for (int i = 0; i < renderables.Count; i++)
            {
                renderables[i].Update(elapsed);
            }
        }

        /// <inheritdoc />
        public void Render()
        {
            for (int i = 0; i < renderables.Count; i++)
            {
                renderables[i].Render();
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            for (int i = 0; i < renderables.Count; i++)
            {
                renderables[i].Dispose();
            }
        }
    }
}
