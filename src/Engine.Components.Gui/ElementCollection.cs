﻿using MyOTKE.ReactiveBuffers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;

namespace MyOTKE.Engine.Components.Gui;

public class ElementCollection : ICollection<ElementBase>
{
    private readonly IElementParent owner;
    private readonly ObservableComposite<IList<Vertex>> composite;
    private readonly Dictionary<ElementBase, Action> removalCallbacks = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="ElementCollection"/> class.
    /// </summary>
    /// <param name="owner">The parent of this collection.</param>
    public ElementCollection(IElementParent owner)
    {
        this.owner = owner;

        if (owner is ElementBase e)
        {
            this.composite = new ObservableComposite<IList<Vertex>>(MakeVerticesObservable(e));
        }
        else
        {
            this.composite = new ObservableComposite<IList<Vertex>>(Observable.Never<IList<Vertex>>());
        }
    }

    /// <inheritdoc />
    public int Count => removalCallbacks.Count;

    /// <inheritdoc />
    public bool IsReadOnly => false;

    /// <inheritdoc />
    public void Add(ElementBase element)
    {
        if (element.Parent != null)
        {
            throw new ArgumentException("Can't add element that already has a parent");
        }

        element.Parent = this.owner;
        if (element is IElementParent parent)
        {
            composite.Add(parent.SubElements.composite);
            removalCallbacks.Add(element, parent.SubElements.composite.Remove);
        }
        else
        {
            var leaf = new ObservableComposite<IList<Vertex>>(MakeVerticesObservable(element));
            composite.Add(leaf);
            removalCallbacks.Add(element, leaf.Remove);
        }
    }

    /// <inheritdoc />
    public bool Remove(ElementBase element)
    {
        if (removalCallbacks.TryGetValue(element, out var callback))
        {
            element.Parent = null;
            callback.Invoke();
            removalCallbacks.Remove(element);
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <inheritdoc />
    public void Clear()
    {
        foreach (var element in removalCallbacks.Keys)
        {
            Remove(element);
        }
    }

    /// <inheritdoc />
    public bool Contains(ElementBase item) => removalCallbacks.ContainsKey(item);

    /// <inheritdoc />
    public void CopyTo(ElementBase[] array, int arrayIndex) => removalCallbacks.Keys.CopyTo(array, arrayIndex);

    /// <inheritdoc />
    public IEnumerator<ElementBase> GetEnumerator() => removalCallbacks.Keys.GetEnumerator();

    /// <inheritdoc />)
    IEnumerator IEnumerable.GetEnumerator() => removalCallbacks.Keys.GetEnumerator();

    public IObservable<IObservable<IList<Vertex>>> Flatten() => composite.Flatten();

    private static IObservable<IList<Vertex>> MakeVerticesObservable(ElementBase elementBase)
    {
        return Observable.Defer(() => Observable
            .FromEventPattern<PropertyChangedEventHandler, PropertyChangedEventArgs>(
                h => elementBase.PropertyChanged += h,
                h => elementBase.PropertyChanged -= h)
            .Select(ev => ((ElementBase)ev.Sender).Vertices)
            .StartWith(elementBase.Vertices));
    }
}
