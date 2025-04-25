using OpenTK.Mathematics;

namespace MyOTKE.Components.Gui;

/// <summary>
/// Interface for types that contain GUI elements. This includes container elements and the root GUI object.
/// </summary>
public interface IElementParent
{
    /// <summary>
    /// Gets the elements that this element contains.
    /// </summary>
    ElementCollection SubElements { get; }

    /// <summary>
    /// Gets the position of the center of this element, in screen space,
    /// for the purposes of child element position and size calculations.
    /// </summary>
    Vector2 Center { get; }

    /// <summary>
    /// Gets the size of this element, in screen space (i.e. pixels),
    /// for the purposes of child element position and size calculations.
    /// </summary>
    Vector2 Size { get; }
}
