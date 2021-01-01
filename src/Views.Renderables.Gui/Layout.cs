﻿using System.Numerics;

namespace MyOTKE.Renderables.Gui
{
    /// <summary>
    /// Container for positioning data for an element (relative to its parent).
    /// </summary>
    public class Layout
    {
        private readonly Dimensions parentOrigin;
        private readonly Dimensions localOrigin;
        private readonly Dimensions relativeSize;
        private readonly Vector2 offset;

        /// <summary>
        /// Initializes a new instance of the <see cref="Layout"/> class.
        /// </summary>
        /// <param name="parentOrigin">the position in parent-space of the local origin.</param>
        /// <param name="localOrigin">the position relative to the center of the element that will be placed at the parent origin.</param>
        /// <param name="relativeSize">the size of the element in relation to its parent.</param>
        /// <param name="offset">The offset to apply when placing the local origin at the parent origin.</param>
        public Layout(Dimensions parentOrigin, Dimensions localOrigin, Dimensions relativeSize, Vector2 offset)
        {
            this.parentOrigin = parentOrigin;
            this.localOrigin = localOrigin;
            this.relativeSize = relativeSize;
            this.offset = offset;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Layout"/> class.
        /// </summary>
        /// <param name="parentOrigin">the position in parent-space of the local origin.</param>
        /// <param name="localOrigin">the position relative to the center of the element that will be placed at the parent origin.</param>
        /// <param name="relativeSize">the size of the element in relation to its parent.</param>
        public Layout(Dimensions parentOrigin, Dimensions localOrigin, Dimensions relativeSize)
            : this(parentOrigin, localOrigin, relativeSize, Vector2.Zero)
        {
        }

        /// <summary>
        /// Gets a layout wherein an element fills its parent.
        /// </summary>
        public static Layout Fill { get; } = new Layout((0f, 0f), (0f, 0f), (1f, 1f));

        /// <summary>
        /// Gets the screen-space position of the center of an element using this layout.
        /// </summary>
        /// <param name="element">The element to get the screen-space position of the center of.</param>
        /// <returns>The screen-space position of the center of the element.</returns>
        public Vector2 GetCenter(ElementBase element)
        {
            var parentOriginScreenSpace = new Vector2(
                element.Parent.Center.X + (parentOrigin.IsXRelative ? parentOrigin.X * element.Parent.Size.X / 2 : parentOrigin.X),
                element.Parent.Center.Y + (parentOrigin.IsYRelative ? parentOrigin.Y * element.Parent.Size.Y / 2 : parentOrigin.Y));

            return offset + new Vector2(
                parentOriginScreenSpace.X - (localOrigin.IsXRelative ? localOrigin.X * element.Size.X / 2 : localOrigin.X),
                parentOriginScreenSpace.Y - (localOrigin.IsYRelative ? localOrigin.Y * element.Size.Y / 2 : localOrigin.Y));
        }

        /// <summary>
        /// Gets the screen-space size of an element using this layout.
        /// </summary>
        /// <param name="element">The element to get the screen-space size of.</param>
        /// <returns>The screen-space size of the element.</returns>
        public Vector2 GetSize(ElementBase element)
        {
            return new Vector2(
                relativeSize.IsXRelative ? element.Parent.Size.X * relativeSize.X : relativeSize.X,
                relativeSize.IsYRelative ? element.Parent.Size.Y * relativeSize.Y : relativeSize.Y);
        }

        /// <summary>
        /// Container for information about some dimensions.
        /// </summary>
        public struct Dimensions
        {
            private Vector2 value;

            public Dimensions(int absoluteX, int absoluteY)
            {
                value = new Vector2(absoluteX, absoluteY);
                IsXRelative = false;
                IsYRelative = false;
            }

            public Dimensions(int absoluteX, float relativeY)
            {
                value = new Vector2(absoluteX, relativeY);
                IsXRelative = false;
                IsYRelative = true;
            }

            public Dimensions(float relativeX, int absoluteY)
            {
                value = new Vector2(relativeX, absoluteY);
                IsXRelative = true;
                IsYRelative = false;
            }

            public Dimensions(float relativeX, float relativeY)
            {
                value = new Vector2(relativeX, relativeY);
                IsXRelative = true;
                IsYRelative = true;
            }

            public float X => value.X;

            public float Y => value.Y;

            public bool IsXRelative { get; }

            public bool IsYRelative { get; }

            public static implicit operator Dimensions((int absoluteX, int absoluteY) tuple) => new Dimensions(tuple.absoluteX, tuple.absoluteY);

            public static implicit operator Dimensions((int absoluteX, float relativeY) tuple) => new Dimensions(tuple.absoluteX, tuple.relativeY);

            public static implicit operator Dimensions((float relativeX, int absoluteY) tuple) => new Dimensions(tuple.relativeX, tuple.absoluteY);

            public static implicit operator Dimensions((float relativeX, float relativeY) tuple) => new Dimensions(tuple.relativeX, tuple.relativeY);
        }
    }
}
