using System;
using System.Collections.Generic;

namespace MyOTKE.Components;

/// <summary>
/// Base class for <see cref="IComponent"/> implementations that are composed of sub-components.
/// </summary>
public abstract class CompositeComponent : IComponent, IDisposable
{
    private readonly List<IComponent> components = [];

    private bool contextCreated;

    /// <summary>
    /// Adds a sub-component.
    /// </summary>
    /// <param name="component">The sub-component to be added.</param>
    public void AddComponent(IComponent component)
    {
        if (contextCreated)
        {
            component.Load();
        }

        components.Add(component);
    }

    /// <inheritdoc />
    public void Load()
    {
        for (int i = 0; i < components.Count; i++)
        {
            components[i].Load();
        }

        contextCreated = true;
    }

    /// <inheritdoc />
    public virtual void Update(TimeSpan elapsed)
    {
        for (int i = 0; i < components.Count; i++)
        {
            components[i].Update(elapsed);
        }
    }

    /// <inheritdoc />
    public void Render()
    {
        for (int i = 0; i < components.Count; i++)
        {
            components[i].Render();
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        for (int i = 0; i < components.Count; i++)
        {
            components[i].Dispose();
        }

        GC.SuppressFinalize(this);
    }
}
