﻿using System;
using System.ComponentModel;
using System.Numerics;

namespace MyOTKE.Renderables.Gui
{
    /// <summary>
    /// Interface for types that contain GUI elements. This includes parent elements and the root GUI object.
    /// </summary>
    public interface IElementParent : INotifyPropertyChanged
    {
        /// <summary>
        /// Callback that is invoked when the element is clicked.
        /// </summary>
        event EventHandler<Vector2> Clicked;

        /// <summary>
        /// Gets the elements that this object contains.
        /// </summary>
        ElementCollection SubElements { get; }

        /// <summary>
        /// Gets the position of the center of this object, in screen space.
        /// </summary>
        Vector2 Center { get; }

        /// <summary>
        /// Gets the size of this object, in screen space (i.e. pixels).
        /// </summary>
        Vector2 Size { get; }
    }
}
