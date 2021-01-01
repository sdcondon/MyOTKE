using System;
using System.Numerics;

namespace MyOTKE.Renderables.Gui
{
    /// <summary>
    /// Base class for GUI elements that can contain other elements.
    /// </summary>
    public abstract class ContainerElementBase : ElementBase, IElementParent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ContainerElementBase"/> class.
        /// </summary>
        /// <param name="layout">The layout of the element in relation to its parent.</param>
        public ContainerElementBase(Layout layout)
            : base(layout)
        {
            this.SubElements = new ElementCollection(this);
        }

        /// <inheritdoc />
        public event EventHandler<Vector2> Clicked;

        /// <inheritdoc /> from IElementParent
        public ElementCollection SubElements { get; }

        /// <inheritdoc />
        public override void Dispose()
        {
            // todo: dispose subelements
            base.Dispose();
        }

        /// <inheritdoc />
        protected override void OnClicked(Vector2 position)
        {
            Clicked?.Invoke(this, position);
        }
    }
}
