using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MyOTKE.Engine.Components.Gui
{
    /// <summary>
    /// A GUI element consisting of some text.
    /// </summary>
    public class Text : ElementBase
    {
        private Vector4 color;
        private string content;
        private float horizontalAlignment;
        private float verticalAlignment;

        /// <summary>
        /// Initializes a new instance of the <see cref="Text"/> class.
        /// </summary>
        /// <param name="layout">The layout of the text in relation to its parent.</param>
        /// <param name="color">The color of the text.</param>
        /// <param name="content">The content of the text.</param>
        public Text(Layout layout, Vector4 color, string content = "")
            : base(layout)
        {
            if (Font == null)
            {
                throw new InvalidOperationException($"{nameof(Font)} property must be set prior to creating any {nameof(Text)} instances");
            }

            this.color = color;
            this.content = content;
        }

        /// <summary>
        /// Gets or sets the font of all text elements.
        /// </summary>
        /// <remarks>
        /// Yeah, not great. Should be specific to text elements.
        /// </remarks>
        public static Font Font { get; set; }

        /// <summary>
        /// Gets or sets the text color.
        /// </summary>
        public Vector4 Color
        {
            get => color;
            set
            {
                color = value;
                OnPropertyChanged(nameof(Color));
            }
        }

        /// <summary>
        /// Gets or sets the text.
        /// </summary>
        public string Content
        {
            get => content;
            set
            {
                content = value;
                OnPropertyChanged(nameof(Content));
            }
        }

        /// <summary>
        /// Gets or sets the horizontal aligment of the text within the element.
        /// </summary>
        public float HorizontalAlignment
        {
            get => horizontalAlignment;
            set
            {
                horizontalAlignment = value;
                OnPropertyChanged(nameof(HorizontalAlignment));
            }
        }

        /// <summary>
        /// Gets or sets the vertical aligment of the text within the element.
        /// </summary>
        public float VerticalAlignment
        {
            get => verticalAlignment;
            set
            {
                verticalAlignment = value;
                OnPropertyChanged(nameof(VerticalAlignment));
            }
        }

        /// <inheritdoc />
        public override IList<Vertex> Vertices
        {
            get
            {
                var scale = 1f;
                var vertices = new List<Vertex>();
                var lineHeight = Font.LineHeight / 64;
                var lines = GetLines(scale);
                var lineBottomLeft = this.PosTL - Vector2.UnitY * ((this.Size.Y - (lineHeight * lines.Count())) * verticalAlignment);
                lineBottomLeft.Y = (float)Math.Round(lineBottomLeft.Y); // Align to pixels looks better?
                foreach (var line in lines)
                {
                    lineBottomLeft.Y -= lineHeight;

                    if (line.Count > 0)
                    {
                        var lineSize = line[line.Count - 1].Position - line[0].Position;
                        lineBottomLeft.X = this.PosTL.X + ((this.Size.X - lineSize.X) * horizontalAlignment);
                        lineBottomLeft.X = (float)Math.Round(lineBottomLeft.X); // align to pixels looks better?

                        foreach (var v in line)
                        {
                            vertices.Add(new Vertex(
                                lineBottomLeft + v.Position,
                                v.Color,
                                (int)v.TexZ,
                                v.TexXY));
                        }
                    }
                }

                return vertices.ToArray();
            }
        }

        private IEnumerable<IList<Vertex>> GetLines(float scale)
        {
            List<Vertex> currentLine;
            float lineLength;

            void StartNewLine()
            {
                currentLine = new List<Vertex>();
                lineLength = 0f;
            }

            StartNewLine();

            foreach (var c in Content)
            {
                var glyph = Font[c];

                if (c == '\n' || (lineLength + glyph.Bearing.X * scale + glyph.Size.X > this.Size.X && currentLine.Count > 0))
                {
                    yield return currentLine;
                    StartNewLine();
                }

                if (c != '\n')
                {
                    AddChar(c, new Vector2(lineLength, 0f), currentLine, scale);
                    lineLength += glyph.Advance * scale;
                }
            }

            yield return currentLine;
        }

        private void AddChar(char c, Vector2 position, List<Vertex> vertices, float scale)
        {
            var glyphInfo = Font[c];

            var charSize = new Vector2(glyphInfo.Size.X * scale, glyphInfo.Size.Y * scale);
            var relativeCharSize = new Vector2(charSize.X / Font.Max.Width, charSize.Y / Font.Max.Height);

            var charPosBL = new Vector2(position.X + glyphInfo.Bearing.X * scale, position.Y + (glyphInfo.Bearing.Y - glyphInfo.Size.Y) * scale);
            var charPosBR = charPosBL + Vector2.UnitX * charSize.X;
            var charPosTL = charPosBL + Vector2.UnitY * charSize.Y;
            var charPosTR = charPosBL + charSize;

            vertices.AddRange(new[]
            {
                new Vertex(charPosTL, Color, (int)glyphInfo.ZOffset, Vector2.Zero),
                new Vertex(charPosTR, Color, (int)glyphInfo.ZOffset, relativeCharSize.X * Vector2.UnitX),
                new Vertex(charPosBL, Color, (int)glyphInfo.ZOffset, relativeCharSize.Y * Vector2.UnitY),
                new Vertex(charPosBR, Color, (int)glyphInfo.ZOffset, relativeCharSize),
            });
        }
    }
}
