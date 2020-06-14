﻿using System;
using System.Text;

namespace MyOTKE.Views.Renderables.Gui
{
    public sealed class TextStream : ContainerElementBase
    {
        private readonly Text textElement;
        private readonly string[] lines;

        private int lastIndex = -1;

        public TextStream(Layout layout, Color textColor, int lineCount)
            : base(layout)
        {
            this.SubElements.Add(this.textElement = new Text(Layout.Fill, textColor));
            this.lines = new string[lineCount];
        }

        public float HorizontalAlignment
        {
            get => this.textElement.HorizontalAlignment;
            set => this.textElement.HorizontalAlignment = value;
        }

        public void PushMessage(string message)
        {
            if (++lastIndex >= lines.Length)
            {
                lastIndex = 0;
            }

            lines[lastIndex] = message;

            var builder = new StringBuilder();
            for (int i = 1; i <= lines.Length; i++)
            {
                var line = lines[(lastIndex + i) % lines.Length];
                if (line != null)
                {
                    builder.AppendLine(line);
                }
            }
            textElement.Content = builder.ToString();
        }
    }
}
