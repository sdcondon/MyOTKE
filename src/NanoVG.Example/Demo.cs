using OpenTK.Graphics.OpenGL;
using System;

namespace NanoVG
{
    public class DemoData
    {
        public int fontNormal;
        public int fontBold;
        public int fontIcons;
        public int fontEmoji;
        public int[] images = new int[12];
    }

    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    public static class Demo
    {
        private const string IconSearch = "\u1F50D";
        private const string IconCircledCross = "\u2716";
        private const string IconChevronRight = "\uE75E";
        private const string IconCheck = "\u2713";
        private const string IconLogin = "\uE740";
        private const string IconTrash = "\uE729";

        ////static float minf(float a, float b) { return a < b ? a : b; }
        ////static float maxf(float a, float b) { return a > b ? a : b; }
        ////static float absf(float a) { return a >= 0.0f ? a : -a; }
        private static float Clampf(float a, float mn, float mx) => a < mn ? mn : (a > mx ? mx : a);

        // Returns 1 if col.rgba is 0.0f,0.0f,0.0f,0.0f, 0 otherwise
        private static bool IsBlack(Color col)
        {
            return col.R == 0.0f && col.G == 0.0f && col.B == 0.0f && col.A == 0.0f;
        }

        private static void DrawWindow(Context vg, string title, float x, float y, float w, float h)
        {
            float cornerRadius = 3.0f;
            Paint shadowPaint;
            Paint headerPaint;

            NVG.Save(vg);
            ////nvgClearState(vg);

            // Window
            NVG.BeginPath(vg);
            NVG.RoundedRect(vg, x, y, w, h, cornerRadius);
            NVG.FillColor(vg, NVG.RGBA(28, 30, 34, 192));
            ////nvgFillColor(vg, nvgRGBA(0,0,0,128));
            NVG.Fill(vg);

            // Drop shadow
            shadowPaint = NVG.BoxGradient(vg, x, y + 2, w, h, cornerRadius * 2, 10, NVG.RGBA(0, 0, 0, 128), NVG.RGBA(0, 0, 0, 0));
            NVG.BeginPath(vg);
            NVG.Rect(vg, x - 10, y - 10, w + 20, h + 30);
            NVG.RoundedRect(vg, x, y, w, h, cornerRadius);
            NVG.PathWinding(vg, Winding.NVG_HOLE);
            NVG.FillPaint(vg, shadowPaint);
            NVG.Fill(vg);

            // Header
            headerPaint = NVG.LinearGradient(vg, x, y, x, y + 15, NVG.RGBA(255, 255, 255, 8), NVG.RGBA(0, 0, 0, 16));
            NVG.BeginPath(vg);
            NVG.RoundedRect(vg, x + 1, y + 1, w - 2, 30, cornerRadius - 1);
            NVG.FillPaint(vg, headerPaint);
            NVG.Fill(vg);
            NVG.BeginPath(vg);
            NVG.MoveTo(vg, x + 0.5f, y + 0.5f + 30);
            NVG.LineTo(vg, x + 0.5f + w - 1, y + 0.5f + 30);
            NVG.StrokeColor(vg, NVG.RGBA(0, 0, 0, 32));
            NVG.Stroke(vg);

            NVG.FontSize(vg, 15.0f);
            NVG.FontFace(vg, "sans-bold");
            NVG.TextAlign(vg, Align.NVG_ALIGN_CENTER | Align.NVG_ALIGN_MIDDLE);

            NVG.FontBlur(vg, 2);
            NVG.FillColor(vg, NVG.RGBA(0, 0, 0, 128));
            NVG.Text(vg, x + w / 2, y + 16 + 1, title, 0);

            NVG.FontBlur(vg, 0);
            NVG.FillColor(vg, NVG.RGBA(220, 220, 220, 160));
            NVG.Text(vg, x + w / 2, y + 16, title, 0);

            NVG.Restore(vg);
        }

        private static void DrawSearchBox(Context vg, string text, float x, float y, float w, float h)
        {
            Paint bg;
            float cornerRadius = h / 2 - 1;

            // Edit
            bg = NVG.BoxGradient(vg, x, y + 1.5f, w, h, h / 2, 5, NVG.RGBA(0, 0, 0, 16), NVG.RGBA(0, 0, 0, 92));
            NVG.BeginPath(vg);
            NVG.RoundedRect(vg, x, y, w, h, cornerRadius);
            NVG.FillPaint(vg, bg);
            NVG.Fill(vg);

            ////nvgBeginPath(vg);
            ////nvgRoundedRect(vg, x+0.5f,y+0.5f, w-1,h-1, cornerRadius-0.5f);
            ////nvgStrokeColor(vg, nvgRGBA(0,0,0,48));
            ////nvgStroke(vg);

            NVG.FontSize(vg, h * 1.3f);
            NVG.FontFace(vg, "icons");
            NVG.FillColor(vg, NVG.RGBA(255, 255, 255, 64));
            NVG.TextAlign(vg, Align.NVG_ALIGN_CENTER | Align.NVG_ALIGN_MIDDLE);
            NVG.Text(vg, x + h * 0.55f, y + h * 0.55f, IconSearch, 0);

            NVG.FontSize(vg, 17.0f);
            NVG.FontFace(vg, "sans");
            NVG.FillColor(vg, NVG.RGBA(255, 255, 255, 32));

            NVG.TextAlign(vg, Align.NVG_ALIGN_LEFT | Align.NVG_ALIGN_MIDDLE);
            NVG.Text(vg, x + h * 1.05f, y + h * 0.5f, text, 0);

            NVG.FontSize(vg, h * 1.3f);
            NVG.FontFace(vg, "icons");
            NVG.FillColor(vg, NVG.RGBA(255, 255, 255, 32));
            NVG.TextAlign(vg, Align.NVG_ALIGN_CENTER | Align.NVG_ALIGN_MIDDLE);
            NVG.Text(vg, x + w - h * 0.55f, y + h * 0.55f, IconCircledCross, 0);
        }

        private static void DrawDropDown(Context vg, string text, float x, float y, float w, float h)
        {
            Paint bg;
            float cornerRadius = 4.0f;

            bg = NVG.LinearGradient(vg, x, y, x, y + h, NVG.RGBA(255, 255, 255, 16), NVG.RGBA(0, 0, 0, 16));
            NVG.BeginPath(vg);
            NVG.RoundedRect(vg, x + 1, y + 1, w - 2, h - 2, cornerRadius - 1);
            NVG.FillPaint(vg, bg);
            NVG.Fill(vg);

            NVG.BeginPath(vg);
            NVG.RoundedRect(vg, x + 0.5f, y + 0.5f, w - 1, h - 1, cornerRadius - 0.5f);
            NVG.StrokeColor(vg, NVG.RGBA(0, 0, 0, 48));
            NVG.Stroke(vg);

            NVG.FontSize(vg, 17.0f);
            NVG.FontFace(vg, "sans");
            NVG.FillColor(vg, NVG.RGBA(255, 255, 255, 160));
            NVG.TextAlign(vg, Align.NVG_ALIGN_LEFT | Align.NVG_ALIGN_MIDDLE);
            NVG.Text(vg, x + h * 0.3f, y + h * 0.5f, text, 0);

            NVG.FontSize(vg, h * 1.3f);
            NVG.FontFace(vg, "icons");
            NVG.FillColor(vg, NVG.RGBA(255, 255, 255, 64));
            NVG.TextAlign(vg, Align.NVG_ALIGN_CENTER | Align.NVG_ALIGN_MIDDLE);
            NVG.Text(vg, x + w - h * 0.5f, y + h * 0.5f, IconChevronRight, 0);
        }

        private static void DrawLabel(Context vg, string text, float x, float y, float w, float h)
        {
            ////NVG_NOTUSED(w);

            NVG.FontSize(vg, 15.0f);
            NVG.FontFace(vg, "sans");
            NVG.FillColor(vg, NVG.RGBA(255, 255, 255, 128));

            NVG.TextAlign(vg, Align.NVG_ALIGN_LEFT | Align.NVG_ALIGN_MIDDLE);
            NVG.Text(vg, x, y + h * 0.5f, text, 0);
        }

        private static void DrawEditBoxBase(Context vg, float x, float y, float w, float h)
        {
            // Edit
            var bg = NVG.BoxGradient(vg, x + 1, y + 1 + 1.5f, w - 2, h - 2, 3, 4, NVG.RGBA(255, 255, 255, 32), NVG.RGBA(32, 32, 32, 32));
            NVG.BeginPath(vg);
            NVG.RoundedRect(vg, x + 1, y + 1, w - 2, h - 2, 4 - 1);
            NVG.FillPaint(vg, bg);
            NVG.Fill(vg);

            NVG.BeginPath(vg);
            NVG.RoundedRect(vg, x + 0.5f, y + 0.5f, w - 1, h - 1, 4 - 0.5f);
            NVG.StrokeColor(vg, NVG.RGBA(0, 0, 0, 48));
            NVG.Stroke(vg);
        }

        private static void DrawEditBox(Context vg, string text, float x, float y, float w, float h)
        {
            DrawEditBoxBase(vg, x, y, w, h);

            NVG.FontSize(vg, 17.0f);
            NVG.FontFace(vg, "sans");
            NVG.FillColor(vg, NVG.RGBA(255, 255, 255, 64));
            NVG.TextAlign(vg, Align.NVG_ALIGN_LEFT | Align.NVG_ALIGN_MIDDLE);
            NVG.Text(vg, x + h * 0.3f, y + h * 0.5f, text, 0);
        }

        private static void DrawEditBoxNum(Context vg, string text, string units, float x, float y, float w, float h)
        {
            float uw;

            DrawEditBoxBase(vg, x, y, w, h);

            uw = NVG.TextBounds(vg, 0, 0, units, 0, null);

            NVG.FontSize(vg, 15.0f);
            NVG.FontFace(vg, "sans");
            NVG.FillColor(vg, NVG.RGBA(255, 255, 255, 64));
            NVG.TextAlign(vg, Align.NVG_ALIGN_RIGHT | Align.NVG_ALIGN_MIDDLE);
            NVG.Text(vg, x + w - h * 0.3f, y + h * 0.5f, units, 0);

            NVG.FontSize(vg, 17.0f);
            NVG.FontFace(vg, "sans");
            NVG.FillColor(vg, NVG.RGBA(255, 255, 255, 128));
            NVG.TextAlign(vg, Align.NVG_ALIGN_RIGHT | Align.NVG_ALIGN_MIDDLE);
            NVG.Text(vg, x + w - uw - h * 0.5f, y + h * 0.5f, text, 0);
        }

        private static void DrawCheckBox(Context vg, string text, float x, float y, float w, float h)
        {
            Paint bg;
            ////NVG_NOTUSED(w);

            NVG.FontSize(vg, 15.0f);
            NVG.FontFace(vg, "sans");
            NVG.FillColor(vg, NVG.RGBA(255, 255, 255, 160));

            NVG.TextAlign(vg, Align.NVG_ALIGN_LEFT | Align.NVG_ALIGN_MIDDLE);
            NVG.Text(vg, x + 28, y + h * 0.5f, text, 0);

            bg = NVG.BoxGradient(vg, x + 1, y + (int)(h * 0.5f) - 9 + 1, 18, 18, 3, 3, NVG.RGBA(0, 0, 0, 32), NVG.RGBA(0, 0, 0, 92));
            NVG.BeginPath(vg);
            NVG.RoundedRect(vg, x + 1, y + (int)(h * 0.5f) - 9, 18, 18, 3);
            NVG.FillPaint(vg, bg);
            NVG.Fill(vg);

            NVG.FontSize(vg, 33);
            NVG.FontFace(vg, "icons");
            NVG.FillColor(vg, NVG.RGBA(255, 255, 255, 128));
            NVG.TextAlign(vg, Align.NVG_ALIGN_CENTER | Align.NVG_ALIGN_MIDDLE);
            NVG.Text(vg, x + 9 + 2, y + h * 0.5f, IconCheck, 0);
        }

        private static void DrawButton(Context vg, string preicon, string text, float x, float y, float w, float h, Color col)
        {
            Paint bg;
            float cornerRadius = 4.0f;
            float tw = 0, iw = 0;

            bg = NVG.LinearGradient(vg, x, y, x, y + h, NVG.RGBA(255, 255, 255, IsBlack(col) ? (byte)16 : (byte)32), NVG.RGBA(0, 0, 0, IsBlack(col) ? (byte)16 : (byte)32));
            NVG.BeginPath(vg);
            NVG.RoundedRect(vg, x + 1, y + 1, w - 2, h - 2, cornerRadius - 1);
            if (!IsBlack(col))
            {
                NVG.FillColor(vg, col);
                NVG.Fill(vg);
            }

            NVG.FillPaint(vg, bg);
            NVG.Fill(vg);

            NVG.BeginPath(vg);
            NVG.RoundedRect(vg, x + 0.5f, y + 0.5f, w - 1, h - 1, cornerRadius - 0.5f);
            NVG.StrokeColor(vg, NVG.RGBA(0, 0, 0, 48));
            NVG.Stroke(vg);

            NVG.FontSize(vg, 17.0f);
            NVG.FontFace(vg, "sans-bold");
            tw = NVG.TextBounds(vg, 0, 0, text, 0, null);
            if (!string.IsNullOrEmpty(preicon))
            {
                NVG.FontSize(vg, h * 1.3f);
                NVG.FontFace(vg, "icons");
                iw = NVG.TextBounds(vg, 0, 0, preicon, 0, null);
                iw += h * 0.15f;
            }

            if (!string.IsNullOrEmpty(preicon))
            {
                NVG.FontSize(vg, h * 1.3f);
                NVG.FontFace(vg, "icons");
                NVG.FillColor(vg, NVG.RGBA(255, 255, 255, 96));
                NVG.TextAlign(vg, Align.NVG_ALIGN_LEFT | Align.NVG_ALIGN_MIDDLE);
                NVG.Text(vg, x + w * 0.5f - tw * 0.5f - iw * 0.75f, y + h * 0.5f, preicon, 0);
            }

            NVG.FontSize(vg, 17.0f);
            NVG.FontFace(vg, "sans-bold");
            NVG.TextAlign(vg, Align.NVG_ALIGN_LEFT | Align.NVG_ALIGN_MIDDLE);
            NVG.FillColor(vg, NVG.RGBA(0, 0, 0, 160));
            NVG.Text(vg, x + w * 0.5f - tw * 0.5f + iw * 0.25f, y + h * 0.5f - 1, text, 0);
            NVG.FillColor(vg, NVG.RGBA(255, 255, 255, 160));
            NVG.Text(vg, x + w * 0.5f - tw * 0.5f + iw * 0.25f, y + h * 0.5f, text, 0);
        }

        private static void DrawSlider(Context vg, float pos, float x, float y, float w, float h)
        {
            Paint bg, knob;
            float cy = y + (int)(h * 0.5f);
            float kr = (int)(h * 0.25f);

            NVG.Save(vg);
            ////nvgClearState(vg);

            // Slot
            bg = NVG.BoxGradient(vg, x, cy - 2 + 1, w, 4, 2, 2, NVG.RGBA(0, 0, 0, 32), NVG.RGBA(0, 0, 0, 128));
            NVG.BeginPath(vg);
            NVG.RoundedRect(vg, x, cy - 2, w, 4, 2);
            NVG.FillPaint(vg, bg);
            NVG.Fill(vg);

            // Knob Shadow
            bg = NVG.RadialGradient(vg, x + (int)(pos * w), cy + 1, kr - 3, kr + 3, NVG.RGBA(0, 0, 0, 64), NVG.RGBA(0, 0, 0, 0));
            NVG.BeginPath(vg);
            NVG.Rect(vg, x + (int)(pos * w) - kr - 5, cy - kr - 5, kr * 2 + 5 + 5, kr * 2 + 5 + 5 + 3);
            NVG.Circle(vg, x + (int)(pos * w), cy, kr);
            NVG.PathWinding(vg, Winding.NVG_HOLE);
            NVG.FillPaint(vg, bg);
            NVG.Fill(vg);

            // Knob
            knob = NVG.LinearGradient(vg, x, cy - kr, x, cy + kr, NVG.RGBA(255, 255, 255, 16), NVG.RGBA(0, 0, 0, 16));
            NVG.BeginPath(vg);
            NVG.Circle(vg, x + (int)(pos * w), cy, kr - 1);
            NVG.FillColor(vg, NVG.RGBA(40, 43, 48, 255));
            NVG.Fill(vg);
            NVG.FillPaint(vg, knob);
            NVG.Fill(vg);

            NVG.BeginPath(vg);
            NVG.Circle(vg, x + (int)(pos * w), cy, kr - 0.5f);
            NVG.StrokeColor(vg, NVG.RGBA(0, 0, 0, 92));
            NVG.Stroke(vg);

            NVG.Restore(vg);
        }

        private static void DrawEyes(Context vg, float x, float y, float w, float h, float mx, float my, float t)
        {
            Paint gloss, bg;
            float ex = w * 0.23f;
            float ey = h * 0.5f;
            float lx = x + ex;
            float ly = y + ey;
            float rx = x + w - ex;
            float ry = y + ey;
            float dx, dy, d;
            float br = (ex < ey ? ex : ey) * 0.5f;
            float blink = 1 - (float)Math.Pow(Math.Sin(t * 0.5f), 200) * 0.8f;

            bg = NVG.LinearGradient(vg, x, y + h * 0.5f, x + w * 0.1f, y + h, NVG.RGBA(0, 0, 0, 32), NVG.RGBA(0, 0, 0, 16));
            NVG.BeginPath(vg);
            NVG.Ellipse(vg, lx + 3.0f, ly + 16.0f, ex, ey);
            NVG.Ellipse(vg, rx + 3.0f, ry + 16.0f, ex, ey);
            NVG.FillPaint(vg, bg);
            NVG.Fill(vg);

            bg = NVG.LinearGradient(vg, x, y + h * 0.25f, x + w * 0.1f, y + h, NVG.RGBA(220, 220, 220, 255), NVG.RGBA(128, 128, 128, 255));
            NVG.BeginPath(vg);
            NVG.Ellipse(vg, lx, ly, ex, ey);
            NVG.Ellipse(vg, rx, ry, ex, ey);
            NVG.FillPaint(vg, bg);
            NVG.Fill(vg);

            dx = (mx - rx) / (ex * 10);
            dy = (my - ry) / (ey * 10);
            d = (float)Math.Sqrt(dx * dx + dy * dy);
            if (d > 1.0f)
            {
                dx /= d;
                dy /= d;
            }

            dx *= ex * 0.4f;
            dy *= ey * 0.5f;
            NVG.BeginPath(vg);
            NVG.Ellipse(vg, lx + dx, ly + dy + ey * 0.25f * (1 - blink), br, br * blink);
            NVG.FillColor(vg, NVG.RGBA(32, 32, 32, 255));
            NVG.Fill(vg);

            dx = (mx - rx) / (ex * 10);
            dy = (my - ry) / (ey * 10);
            d = (float)Math.Sqrt(dx * dx + dy * dy);
            if (d > 1.0f)
            {
                dx /= d;
                dy /= d;
            }

            dx *= ex * 0.4f;
            dy *= ey * 0.5f;
            NVG.BeginPath(vg);
            NVG.Ellipse(vg, rx + dx, ry + dy + ey * 0.25f * (1 - blink), br, br * blink);
            NVG.FillColor(vg, NVG.RGBA(32, 32, 32, 255));
            NVG.Fill(vg);

            gloss = NVG.RadialGradient(vg, lx - ex * 0.25f, ly - ey * 0.5f, ex * 0.1f, ex * 0.75f, NVG.RGBA(255, 255, 255, 128), NVG.RGBA(255, 255, 255, 0));
            NVG.BeginPath(vg);
            NVG.Ellipse(vg, lx, ly, ex, ey);
            NVG.FillPaint(vg, gloss);
            NVG.Fill(vg);

            gloss = NVG.RadialGradient(vg, rx - ex * 0.25f, ry - ey * 0.5f, ex * 0.1f, ex * 0.75f, NVG.RGBA(255, 255, 255, 128), NVG.RGBA(255, 255, 255, 0));
            NVG.BeginPath(vg);
            NVG.Ellipse(vg, rx, ry, ex, ey);
            NVG.FillPaint(vg, gloss);
            NVG.Fill(vg);
        }

        private static void DrawGraph(Context vg, float x, float y, float w, float h, float t)
        {
            Paint bg;
            float[] samples = new float[]
            {
                (1 + (float)Math.Sin(t * 1.2345f + Math.Cos(t * 0.33457f) * 0.44f)) * 0.5f,
                (1 + (float)Math.Sin(t * 0.68363f + Math.Cos(t * 1.3f) * 1.55f)) * 0.5f,
                (1 + (float)Math.Sin(t * 1.1642f + Math.Cos(t * 0.33457) * 1.24f)) * 0.5f,
                (1 + (float)Math.Sin(t * 0.56345f + Math.Cos(t * 1.63f) * 0.14f)) * 0.5f,
                (1 + (float)Math.Sin(t * 1.6245f + Math.Cos(t * 0.254f) * 0.3f)) * 0.5f,
                (1 + (float)Math.Sin(t * 0.345f + Math.Cos(t * 0.03f) * 0.6f)) * 0.5f,
            };
            float[] sx = new float[6];
            float[] sy = new float[6];
            float dx = w / 5.0f;
            int i;

            for (i = 0; i < 6; i++)
            {
                sx[i] = x + i * dx;
                sy[i] = y + h * samples[i] * 0.8f;
            }

            // Graph background
            bg = NVG.LinearGradient(vg, x, y, x, y + h, NVG.RGBA(0, 160, 192, 0), NVG.RGBA(0, 160, 192, 64));
            NVG.BeginPath(vg);
            NVG.MoveTo(vg, sx[0], sy[0]);
            for (i = 1; i < 6; i++)
            {
                NVG.BezierTo(vg, sx[i - 1] + dx * 0.5f, sy[i - 1], sx[i] - dx * 0.5f, sy[i], sx[i], sy[i]);
            }

            NVG.LineTo(vg, x + w, y + h);
            NVG.LineTo(vg, x, y + h);
            NVG.FillPaint(vg, bg);
            NVG.Fill(vg);

            // Graph line
            NVG.BeginPath(vg);
            NVG.MoveTo(vg, sx[0], sy[0] + 2);
            for (i = 1; i < 6; i++)
            {
                NVG.BezierTo(vg, sx[i - 1] + dx * 0.5f, sy[i - 1] + 2, sx[i] - dx * 0.5f, sy[i] + 2, sx[i], sy[i] + 2);
            }

            NVG.StrokeColor(vg, NVG.RGBA(0, 0, 0, 32));
            NVG.StrokeWidth(vg, 3.0f);
            NVG.Stroke(vg);

            NVG.BeginPath(vg);
            NVG.MoveTo(vg, sx[0], sy[0]);
            for (i = 1; i < 6; i++)
            {
                NVG.BezierTo(vg, sx[i - 1] + dx * 0.5f, sy[i - 1], sx[i] - dx * 0.5f, sy[i], sx[i], sy[i]);
            }

            NVG.StrokeColor(vg, NVG.RGBA(0, 160, 192, 255));
            NVG.StrokeWidth(vg, 3.0f);
            NVG.Stroke(vg);

            // Graph sample pos
            for (i = 0; i < 6; i++)
            {
                bg = NVG.RadialGradient(vg, sx[i], sy[i] + 2, 3.0f, 8.0f, NVG.RGBA(0, 0, 0, 32), NVG.RGBA(0, 0, 0, 0));
                NVG.BeginPath(vg);
                NVG.Rect(vg, sx[i] - 10, sy[i] - 10 + 2, 20, 20);
                NVG.FillPaint(vg, bg);
                NVG.Fill(vg);
            }

            NVG.BeginPath(vg);
            for (i = 0; i < 6; i++)
            {
                NVG.Circle(vg, sx[i], sy[i], 4.0f);
            }

            NVG.FillColor(vg, NVG.RGBA(0, 160, 192, 255));
            NVG.Fill(vg);
            NVG.BeginPath(vg);
            for (i = 0; i < 6; i++)
            {
                NVG.Circle(vg, sx[i], sy[i], 2.0f);
            }

            NVG.FillColor(vg, NVG.RGBA(220, 220, 220, 255));
            NVG.Fill(vg);

            NVG.StrokeWidth(vg, 1.0f);
        }

        private static void DrawSpinner(Context vg, float cx, float cy, float r, float t)
        {
            float a0 = 0.0f + t * 6;
            float a1 = (float)Math.PI + t * 6;
            float r0 = r;
            float r1 = r * 0.75f;
            float ax, ay, bx, by;
            Paint paint;

            NVG.Save(vg);

            NVG.BeginPath(vg);
            NVG.Arc(vg, cx, cy, r0, a0, a1, Winding.NVG_CW);
            NVG.Arc(vg, cx, cy, r1, a1, a0, Winding.NVG_CCW);
            NVG.ClosePath(vg);
            ax = cx + (float)Math.Cos(a0) * (r0 + r1) * 0.5f;
            ay = cy + (float)Math.Sin(a0) * (r0 + r1) * 0.5f;
            bx = cx + (float)Math.Cos(a1) * (r0 + r1) * 0.5f;
            by = cy + (float)Math.Sin(a1) * (r0 + r1) * 0.5f;
            paint = NVG.LinearGradient(vg, ax, ay, bx, by, NVG.RGBA(0, 0, 0, 0), NVG.RGBA(0, 0, 0, 128));
            NVG.FillPaint(vg, paint);
            NVG.Fill(vg);

            NVG.Restore(vg);
        }

        private static void DrawThumbnails(Context vg, float x, float y, float w, float h, int[] images, float t)
        {
            float cornerRadius = 3.0f;
            Paint shadowPaint, imgPaint, fadePaint;
            float ix, iy, iw, ih;
            float thumb = 60.0f;
            float arry = 30.5f;
            float stackh = (images.Length / 2) * (thumb + 10) + 10;
            int i;
            float u = (1 + (float)Math.Cos(t * 0.5f)) * 0.5f;
            float u2 = (1 - (float)Math.Cos(t * 0.2f)) * 0.5f;
            float scrollh, dv;

            NVG.Save(vg);
            ////nvgClearState(vg);

            // Drop shadow
            shadowPaint = NVG.BoxGradient(vg, x, y + 4, w, h, cornerRadius * 2, 20, NVG.RGBA(0, 0, 0, 128), NVG.RGBA(0, 0, 0, 0));
            NVG.BeginPath(vg);
            NVG.Rect(vg, x - 10, y - 10, w + 20, h + 30);
            NVG.RoundedRect(vg, x, y, w, h, cornerRadius);
            NVG.PathWinding(vg, Winding.NVG_HOLE);
            NVG.FillPaint(vg, shadowPaint);
            NVG.Fill(vg);

            // Window
            NVG.BeginPath(vg);
            NVG.RoundedRect(vg, x, y, w, h, cornerRadius);
            NVG.MoveTo(vg, x - 10, y + arry);
            NVG.LineTo(vg, x + 1, y + arry - 11);
            NVG.LineTo(vg, x + 1, y + arry + 11);
            NVG.FillColor(vg, NVG.RGBA(200, 200, 200, 255));
            NVG.Fill(vg);

            NVG.Save(vg);
            NVG.Scissor(vg, x, y, w, h);
            NVG.Translate(vg, 0, -(stackh - h) * u);

            dv = 1.0f / (float)(images.Length - 1);

            for (i = 0; i < images.Length; i++)
            {
                float tx, ty, v, a;
                tx = x + 10;
                ty = y + 10;
                tx += (i % 2) * (thumb + 10);
                ty += (i / 2) * (thumb + 10);
                NVG.ImageSize(vg, images[i], out var imgw, out var imgh);
                if (imgw < imgh)
                {
                    iw = thumb;
                    ih = iw * (float)imgh / (float)imgw;
                    ix = 0;
                    iy = -(ih - thumb) * 0.5f;
                }
                else
                {
                    ih = thumb;
                    iw = ih * (float)imgw / (float)imgh;
                    ix = -(iw - thumb) * 0.5f;
                    iy = 0;
                }

                v = i * dv;
                a = Clampf((u2 - v) / dv, 0, 1);

                if (a < 1.0f)
                {
                    DrawSpinner(vg, tx + thumb / 2, ty + thumb / 2, thumb * 0.25f, t);
                }

                imgPaint = NVG.ImagePattern(vg, tx + ix, ty + iy, iw, ih, 0.0f / 180.0f * (float)Math.PI, images[i], a);
                NVG.BeginPath(vg);
                NVG.RoundedRect(vg, tx, ty, thumb, thumb, 5);
                NVG.FillPaint(vg, imgPaint);
                NVG.Fill(vg);

                shadowPaint = NVG.BoxGradient(vg, tx - 1, ty, thumb + 2, thumb + 2, 5, 3, NVG.RGBA(0, 0, 0, 128), NVG.RGBA(0, 0, 0, 0));
                NVG.BeginPath(vg);
                NVG.Rect(vg, tx - 5, ty - 5, thumb + 10, thumb + 10);
                NVG.RoundedRect(vg, tx, ty, thumb, thumb, 6);
                NVG.PathWinding(vg, Winding.NVG_HOLE);
                NVG.FillPaint(vg, shadowPaint);
                NVG.Fill(vg);

                NVG.BeginPath(vg);
                NVG.RoundedRect(vg, tx + 0.5f, ty + 0.5f, thumb - 1, thumb - 1, 4 - 0.5f);
                NVG.StrokeWidth(vg, 1.0f);
                NVG.StrokeColor(vg, NVG.RGBA(255, 255, 255, 192));
                NVG.Stroke(vg);
            }

            NVG.Restore(vg);

            // Hide fades
            fadePaint = NVG.LinearGradient(vg, x, y, x, y + 6, NVG.RGBA(200, 200, 200, 255), NVG.RGBA(200, 200, 200, 0));
            NVG.BeginPath(vg);
            NVG.Rect(vg, x + 4, y, w - 8, 6);
            NVG.FillPaint(vg, fadePaint);
            NVG.Fill(vg);

            fadePaint = NVG.LinearGradient(vg, x, y + h, x, y + h - 6, NVG.RGBA(200, 200, 200, 255), NVG.RGBA(200, 200, 200, 0));
            NVG.BeginPath(vg);
            NVG.Rect(vg, x + 4, y + h - 6, w - 8, 6);
            NVG.FillPaint(vg, fadePaint);
            NVG.Fill(vg);

            // Scroll bar
            shadowPaint = NVG.BoxGradient(vg, x + w - 12 + 1, y + 4 + 1, 8, h - 8, 3, 4, NVG.RGBA(0, 0, 0, 32), NVG.RGBA(0, 0, 0, 92));
            NVG.BeginPath(vg);
            NVG.RoundedRect(vg, x + w - 12, y + 4, 8, h - 8, 3);
            NVG.FillPaint(vg, shadowPaint);
            ////nvgFillColor(vg, nvgRGBA(255,0,0,128));
            NVG.Fill(vg);

            scrollh = (h / stackh) * (h - 8);
            shadowPaint = NVG.BoxGradient(vg, x + w - 12 - 1, y + 4 + (h - 8 - scrollh) * u - 1, 8, scrollh, 3, 4, NVG.RGBA(220, 220, 220, 255), NVG.RGBA(128, 128, 128, 255));
            NVG.BeginPath(vg);
            NVG.RoundedRect(vg, x + w - 12 + 1, y + 4 + 1 + (h - 8 - scrollh) * u, 8 - 2, scrollh - 2, 2);
            NVG.FillPaint(vg, shadowPaint);
            ////nvgFillColor(vg, nvgRGBA(0,0,0,128));
            NVG.Fill(vg);

            NVG.Restore(vg);
        }

        private static void DrawColorwheel(Context vg, float x, float y, float w, float h, float t)
        {
            int i;
            float r0, r1, ax, ay, bx, by, cx, cy, aeps, r;
            float hue = (float)Math.Sin(t * 0.12f);
            Paint paint;

            NVG.Save(vg);

            ////nvgBeginPath(vg);
            ////nvgRect(vg, x,y,w,h);
            ////nvgFillColor(vg, nvgRGBA(255,0,0,128));
            ////nvgFill(vg);

            cx = x + w * 0.5f;
            cy = y + h * 0.5f;
            r1 = (w < h ? w : h) * 0.5f - 5.0f;
            r0 = r1 - 20.0f;
            aeps = 0.5f / r1;   // half a pixel arc length in radians (2pi cancels out).

            for (i = 0; i < 6; i++)
            {
                float a0 = (float)i / 6.0f * (float)Math.PI * 2.0f - aeps;
                float a1 = (float)(i + 1.0f) / 6.0f * (float)Math.PI * 2.0f + aeps;
                NVG.BeginPath(vg);
                NVG.Arc(vg, cx, cy, r0, a0, a1, Winding.NVG_CW);
                NVG.Arc(vg, cx, cy, r1, a1, a0, Winding.NVG_CCW);
                NVG.ClosePath(vg);
                ax = cx + (float)Math.Cos(a0) * (r0 + r1) * 0.5f;
                ay = cy + (float)Math.Sin(a0) * (r0 + r1) * 0.5f;
                bx = cx + (float)Math.Cos(a1) * (r0 + r1) * 0.5f;
                by = cy + (float)Math.Sin(a1) * (r0 + r1) * 0.5f;
                paint = NVG.LinearGradient(vg, ax, ay, bx, by, NVG.HSLA(a0 / ((float)Math.PI * 2), 1.0f, 0.55f, 255), NVG.HSLA(a1 / ((float)Math.PI * 2), 1.0f, 0.55f, 255));
                NVG.FillPaint(vg, paint);
                NVG.Fill(vg);
            }

            NVG.BeginPath(vg);
            NVG.Circle(vg, cx, cy, r0 - 0.5f);
            NVG.Circle(vg, cx, cy, r1 + 0.5f);
            NVG.StrokeColor(vg, NVG.RGBA(0, 0, 0, 64));
            NVG.StrokeWidth(vg, 1.0f);
            NVG.Stroke(vg);

            // Selector
            NVG.Save(vg);
            NVG.Translate(vg, cx, cy);
            NVG.Rotate(vg, hue * (float)Math.PI * 2);

            // Marker on
            NVG.StrokeWidth(vg, 2.0f);
            NVG.BeginPath(vg);
            NVG.Rect(vg, r0 - 1, -3, r1 - r0 + 2, 6);
            NVG.StrokeColor(vg, NVG.RGBA(255, 255, 255, 192));
            NVG.Stroke(vg);

            paint = NVG.BoxGradient(vg, r0 - 3, -5, r1 - r0 + 6, 10, 2, 4, NVG.RGBA(0, 0, 0, 128), NVG.RGBA(0, 0, 0, 0));
            NVG.BeginPath(vg);
            NVG.Rect(vg, r0 - 2 - 10, -4 - 10, r1 - r0 + 4 + 20, 8 + 20);
            NVG.Rect(vg, r0 - 2, -4, r1 - r0 + 4, 8);
            NVG.PathWinding(vg, Winding.NVG_HOLE);
            NVG.FillPaint(vg, paint);
            NVG.Fill(vg);

            // Center triangle
            r = r0 - 6;
            ax = (float)Math.Cos(120.0f / 180.0f * (float)Math.PI) * r;
            ay = (float)Math.Sin(120.0f / 180.0f * (float)Math.PI) * r;
            bx = (float)Math.Cos(-120.0f / 180.0f * (float)Math.PI) * r;
            by = (float)Math.Sin(-120.0f / 180.0f * (float)Math.PI) * r;
            NVG.BeginPath(vg);
            NVG.MoveTo(vg, r, 0);
            NVG.LineTo(vg, ax, ay);
            NVG.LineTo(vg, bx, by);
            NVG.ClosePath(vg);
            paint = NVG.LinearGradient(vg, r, 0, ax, ay, NVG.HSLA(hue, 1.0f, 0.5f, 255), NVG.RGBA(255, 255, 255, 255));
            NVG.FillPaint(vg, paint);
            NVG.Fill(vg);
            paint = NVG.LinearGradient(vg, (r + ax) * 0.5f, (0 + ay) * 0.5f, bx, by, NVG.RGBA(0, 0, 0, 0), NVG.RGBA(0, 0, 0, 255));
            NVG.FillPaint(vg, paint);
            NVG.Fill(vg);
            NVG.StrokeColor(vg, NVG.RGBA(0, 0, 0, 64));
            NVG.Stroke(vg);

            // Select circle on triangle
            ax = (float)Math.Cos(120.0f / 180.0f * (float)Math.PI) * r * 0.3f;
            ay = (float)Math.Sin(120.0f / 180.0f * (float)Math.PI) * r * 0.4f;
            NVG.StrokeWidth(vg, 2.0f);
            NVG.BeginPath(vg);
            NVG.Circle(vg, ax, ay, 5);
            NVG.StrokeColor(vg, NVG.RGBA(255, 255, 255, 192));
            NVG.Stroke(vg);

            paint = NVG.RadialGradient(vg, ax, ay, 7, 9, NVG.RGBA(0, 0, 0, 64), NVG.RGBA(0, 0, 0, 0));
            NVG.BeginPath(vg);
            NVG.Rect(vg, ax - 20, ay - 20, 40, 40);
            NVG.Circle(vg, ax, ay, 7);
            NVG.PathWinding(vg, Winding.NVG_HOLE);
            NVG.FillPaint(vg, paint);
            NVG.Fill(vg);

            NVG.Restore(vg);

            NVG.Restore(vg);
        }

        private static void DrawLines(Context vg, float x, float y, float w, float h, float t)
        {
            int i, j;
            float pad = 5.0f, s = w / 9.0f - pad * 2;
            float[] pts = new float[4 * 2];
            float fx, fy;
            var joins = new nvgLineCap[] { nvgLineCap.NVG_MITER, nvgLineCap.NVG_ROUND, nvgLineCap.NVG_BEVEL };
            var caps = new nvgLineCap[] { nvgLineCap.NVG_BUTT, nvgLineCap.NVG_ROUND, nvgLineCap.NVG_SQUARE };
            ////NVG_NOTUSED(h);

            NVG.Save(vg);
            pts[0] = -s * 0.25f + (float)Math.Cos(t * 0.3f) * s * 0.5f;
            pts[1] = (float)Math.Sin(t * 0.3f) * s * 0.5f;
            pts[2] = -s * 0.25f;
            pts[3] = 0;
            pts[4] = s * 0.25f;
            pts[5] = 0;
            pts[6] = s * 0.25f + (float)Math.Cos(-t * 0.3f) * s * 0.5f;
            pts[7] = (float)Math.Sin(-t * 0.3f) * s * 0.5f;

            for (i = 0; i < 3; i++)
            {
                for (j = 0; j < 3; j++)
                {
                    fx = x + s * 0.5f + (i * 3 + j) / 9.0f * w + pad;
                    fy = y - s * 0.5f + pad;

                    NVG.LineCap(vg, caps[i]);
                    NVG.LineJoin(vg, joins[j]);

                    NVG.StrokeWidth(vg, s * 0.3f);
                    NVG.StrokeColor(vg, NVG.RGBA(0, 0, 0, 160));
                    NVG.BeginPath(vg);
                    NVG.MoveTo(vg, fx + pts[0], fy + pts[1]);
                    NVG.LineTo(vg, fx + pts[2], fy + pts[3]);
                    NVG.LineTo(vg, fx + pts[4], fy + pts[5]);
                    NVG.LineTo(vg, fx + pts[6], fy + pts[7]);
                    NVG.Stroke(vg);

                    NVG.LineCap(vg, nvgLineCap.NVG_BUTT);
                    NVG.LineJoin(vg, nvgLineCap.NVG_BEVEL);

                    NVG.StrokeWidth(vg, 1.0f);
                    NVG.StrokeColor(vg, NVG.RGBA(0, 192, 255, 255));
                    NVG.BeginPath(vg);
                    NVG.MoveTo(vg, fx + pts[0], fy + pts[1]);
                    NVG.LineTo(vg, fx + pts[2], fy + pts[3]);
                    NVG.LineTo(vg, fx + pts[4], fy + pts[5]);
                    NVG.LineTo(vg, fx + pts[6], fy + pts[7]);
                    NVG.Stroke(vg);
                }
            }

            NVG.Restore(vg);
        }

        public static void LoadDemoData(Context vg, out DemoData data)
        {
            if (vg == null)
            {
                throw new ArgumentNullException(nameof(vg));
            }

            data = new DemoData();

            ////for (int i = 0; i < 12; i++)
            ////{
            ////    string file = $"../example/images/image{i + 1}.jpg";
            ////    data.images[i] = NVG.CreateImage(vg, file, 0);
            ////    if (data.images[i] == 0)
            ////    {
            ////        throw new Exception($"Could not load {file}");
            ////    }
            ////}

            ////data.fontIcons = NVG.CreateFont(vg, "icons", "../example/entypo.ttf");
            ////if (data.fontIcons == -1)
            ////{
            ////    throw new Exception("Could not add font icons.");
            ////}

            ////data.fontNormal = NVG.CreateFont(vg, "sans", "../example/Roboto-Regular.ttf");
            ////if (data.fontNormal == -1)
            ////{
            ////    throw new Exception("Could not add font italic.");
            ////}

            ////data.fontBold = NVG.CreateFont(vg, "sans-bold", "../example/Roboto-Bold.ttf");
            ////if (data.fontBold == -1)
            ////{
            ////    throw new Exception("Could not add font bold.");
            ////}

            ////data.fontEmoji = NVG.CreateFont(vg, "emoji", "../example/NotoEmoji-Regular.ttf");
            ////if (data.fontEmoji == -1)
            ////{
            ////    throw new Exception("Could not add font emoji.");
            ////}

            ////NVG.AddFallbackFontId(vg, data.fontNormal, data.fontEmoji);
            ////NVG.AddFallbackFontId(vg, data.fontBold, data.fontEmoji);
        }

        public static void FreeDemoData(Context vg, DemoData data)
        {
            if (vg == null)
            {
                return;
            }

            for (int i = 0; i < 12; i++)
            {
                NVG.DeleteImage(vg, data.images[i]);
            }
        }

        private static void DrawParagraph(Context vg, float x, float y, float width, float height, float mx, float my)
        {
            ////TextRow rows[3];
            ////GlyphPosition glyphs[100];
            ////const char* text = "This is longer chunk of text.\n  \n  Would have used lorem ipsum but she    was busy jumping over the lazy dog with the fox and all the men who came to the aid of the party.🎉";
            ////const char* start;
            ////const char* end;
            ////int nrows, i, nglyphs, j, lnum = 0;
            ////float caretx, px;
            ////float bounds[4];
            ////float a;
            ////const char* hoverText = "Hover your mouse over the text to see calculated caret position.";
            ////float gx, gy;
            ////int gutter = 0;
            ////const char* boxText = "Testing\nsome multiline\ntext.";
            ////////NVG_NOTUSED(height);

            ////NVG.Save(vg);

            ////NVG.FontSize(vg, 15.0f);
            ////NVG.FontFace(vg, "sans");
            ////NVG.TextAlign(vg, Align.NVG_ALIGN_LEFT | Align.NVG_ALIGN_TOP);
            ////NVG.TextMetrics(vg, out var ascender, out var descender, out var lineh);

            ////// The text break API can be used to fill a large buffer of rows,
            ////// or to iterate over the text just few lines (or just one) at a time.
            ////// The "next" variable of the last returned item tells where to continue.
            ////start = text;
            ////end = text + strlen(text);
            ////while (nrows = NVG.TextBreakLines(vg, start, end, width, rows, 3))
            ////{
            ////    for (i = 0; i < nrows; i++)
            ////    {
            ////        NVGtextRow* row = &rows[i];
            ////        bool hit = mx > x && mx < (x + width) && my >= y && my < (y + lineh);

            ////        NVG.BeginPath(vg);
            ////        NVG.FillColor(vg, NVG.RGBA(255, 255, 255, hit ? (byte)64 : (byte)16));
            ////        NVG.Rect(vg, x + row->minx, y, row->maxx - row->minx, lineh);
            ////        NVG.Fill(vg);

            ////        NVG.FillColor(vg, NVG.RGBA(255, 255, 255, 255));
            ////        NVG.Text(vg, x, y, row->start, row->end);

            ////        if (hit)
            ////        {
            ////            caretx = (mx < x + row->width / 2) ? x : x + row->width;
            ////            px = x;
            ////            nglyphs = NVG.TextGlyphPositions(vg, x, y, row->start, row->end, glyphs, 100);
            ////            for (j = 0; j < nglyphs; j++)
            ////            {
            ////                float x0 = glyphs[j].x;
            ////                float x1 = (j + 1 < nglyphs) ? glyphs[j + 1].x : x + row->width;
            ////                float gx = x0 * 0.3f + x1 * 0.7f;
            ////                if (mx >= px && mx < gx)
            ////                {
            ////                    caretx = glyphs[j].x;
            ////                }

            ////                px = gx;
            ////            }

            ////            NVG.BeginPath(vg);
            ////            NVG.FillColor(vg, NVG.RGBA(255, 192, 0, 255));
            ////            NVG.Rect(vg, caretx, y, 1, lineh);
            ////            NVG.Fill(vg);

            ////            gutter = lnum + 1;
            ////            gx = x - 10;
            ////            gy = y + lineh / 2;
            ////        }

            ////        lnum++;
            ////        y += lineh;
            ////    }

            ////    // Keep going...
            ////    start = rows[nrows - 1].next;
            ////}

            ////if (gutter)
            ////{
            ////    char txt[16];
            ////    snprintf(txt, sizeof(txt), "%d", gutter);
            ////    NVG.FontSize(vg, 12.0f);
            ////    NVG.TextAlign(vg, Align.NVG_ALIGN_RIGHT | Align.NVG_ALIGN_MIDDLE);

            ////    NVG.TextBounds(vg, gx, gy, txt, 0, bounds);

            ////    NVG.BeginPath(vg);
            ////    NVG.FillColor(vg, NVG.RGBA(255, 192, 0, 255));
            ////    NVG.RoundedRect(vg, (int)bounds[0] - 4, (int)bounds[1] - 2, (int)(bounds[2] - bounds[0]) + 8, (int)(bounds[3] - bounds[1]) + 4, ((int)(bounds[3] - bounds[1]) + 4) / 2 - 1);
            ////    NVG.Fill(vg);

            ////    NVG.FillColor(vg, NVG.RGBA(32, 32, 32, 255));
            ////    NVG.Text(vg, gx, gy, txt, 0);
            ////}

            ////y += 20.0f;

            ////NVG.FontSize(vg, 11.0f);
            ////NVG.TextAlign(vg, Align.NVG_ALIGN_LEFT | Align.NVG_ALIGN_TOP);
            ////NVG.TextLineHeight(vg, 1.2f);

            ////NVG.TextBoxBounds(vg, x, y, 150, hoverText, 0, bounds);

            ////// Fade the tooltip out when close to it.
            ////gx = Clampf(mx, bounds[0], bounds[2]) - mx;
            ////gy = Clampf(my, bounds[1], bounds[3]) - my;
            ////a = (float)Math.Sqrt(gx * gx + gy * gy) / 30.0f;
            ////a = Clampf(a, 0, 1);
            ////NVG.GlobalAlpha(vg, a);

            ////NVG.BeginPath(vg);
            ////NVG.FillColor(vg, NVG.RGBA(220, 220, 220, 255));
            ////NVG.RoundedRect(vg, bounds[0] - 2, bounds[1] - 2, (int)(bounds[2] - bounds[0]) + 4, (int)(bounds[3] - bounds[1]) + 4, 3);
            ////px = (int)((bounds[2] + bounds[0]) / 2);
            ////NVG.MoveTo(vg, px, bounds[1] - 10);
            ////NVG.LineTo(vg, px + 7, bounds[1] + 1);
            ////NVG.LineTo(vg, px - 7, bounds[1] + 1);
            ////NVG.Fill(vg);

            ////NVG.FillColor(vg, NVG.RGBA(0, 0, 0, 220));
            ////NVG.TextBox(vg, x, y, 150, hoverText, 0);

            ////NVG.Restore(vg);
        }

        private static void DrawWidths(Context vg, float x, float y, float width)
        {
            NVG.Save(vg);
            NVG.StrokeColor(vg, NVG.RGBA(0, 0, 0, 255));

            for (int i = 0; i < 20; i++)
            {
                float w = (i + 0.5f) * 0.1f;
                NVG.StrokeWidth(vg, w);
                NVG.BeginPath(vg);
                NVG.MoveTo(vg, x, y);
                NVG.LineTo(vg, x + width, y + width * 0.3f);
                NVG.Stroke(vg);
                y += 10;
            }

            NVG.Restore(vg);
        }

        private static void DrawCaps(Context vg, float x, float y, float width)
        {
            int i;
            var caps = new nvgLineCap[] { nvgLineCap.NVG_BUTT, nvgLineCap.NVG_ROUND, nvgLineCap.NVG_SQUARE };
            float lineWidth = 8.0f;

            NVG.Save(vg);

            NVG.BeginPath(vg);
            NVG.Rect(vg, x - lineWidth / 2, y, width + lineWidth, 40);
            NVG.FillColor(vg, NVG.RGBA(255, 255, 255, 32));
            NVG.Fill(vg);

            NVG.BeginPath(vg);
            NVG.Rect(vg, x, y, width, 40);
            NVG.FillColor(vg, NVG.RGBA(255, 255, 255, 32));
            NVG.Fill(vg);

            NVG.StrokeWidth(vg, lineWidth);
            for (i = 0; i < 3; i++)
            {
                NVG.LineCap(vg, caps[i]);
                NVG.StrokeColor(vg, NVG.RGBA(0, 0, 0, 255));
                NVG.BeginPath(vg);
                NVG.MoveTo(vg, x, y + i * 10 + 5);
                NVG.LineTo(vg, x + width, y + i * 10 + 5);
                NVG.Stroke(vg);
            }

            NVG.Restore(vg);
        }

        private static void DrawScissor(Context vg, float x, float y, float t)
        {
            NVG.Save(vg);

            // Draw first rect and set scissor to it's area.
            NVG.Translate(vg, x, y);
            NVG.Rotate(vg, NVG.DegToRad(5));
            NVG.BeginPath(vg);
            NVG.Rect(vg, -20, -20, 60, 40);
            NVG.FillColor(vg, NVG.RGBA(255, 0, 0, 255));
            NVG.Fill(vg);
            NVG.Scissor(vg, -20, -20, 60, 40);

            // Draw second rectangle with offset and rotation.
            NVG.Translate(vg, 40, 0);
            NVG.Rotate(vg, t);

            // Draw the intended second rectangle without any scissoring.
            NVG.Save(vg);
            NVG.ResetScissor(vg);
            NVG.BeginPath(vg);
            NVG.Rect(vg, -20, -10, 60, 30);
            NVG.FillColor(vg, NVG.RGBA(255, 128, 0, 64));
            NVG.Fill(vg);
            NVG.Restore(vg);

            // Draw second rectangle with combined scissoring.
            NVG.IntersectScissor(vg, -20, -10, 60, 30);
            NVG.BeginPath(vg);
            NVG.Rect(vg, -20, -10, 60, 30);
            NVG.FillColor(vg, NVG.RGBA(255, 128, 0, 255));
            NVG.Fill(vg);

            NVG.Restore(vg);
        }

        public static void RenderDemo(Context vg, float mx, float my, float width, float height, float t, bool blowup, DemoData data)
        {
            float x, y, popy;

            DrawEyes(vg, width - 250, 50, 150, 100, mx, my, t);
            DrawParagraph(vg, width - 450, 50, 150, 100, mx, my);
            DrawGraph(vg, 0, height / 2, width, height / 2, t);
            DrawColorwheel(vg, width - 300, height - 300, 250.0f, 250.0f, t);

            // Line joints
            DrawLines(vg, 120, height - 50, 600, 50, t);

            // Line caps
            DrawWidths(vg, 10, 50, 30);

            // Line caps
            DrawCaps(vg, 10, 300, 30);

            DrawScissor(vg, 50, height - 80, t);

            NVG.Save(vg);
            if (blowup)
            {
                NVG.Rotate(vg, (float)Math.Sin(t * 0.3f) * 5.0f / 180.0f * (float)Math.PI);
                NVG.Scale(vg, 2.0f, 2.0f);
            }

            // Widgets
            DrawWindow(vg, "Widgets `n Stuff", 50, 50, 300, 400);
            x = 60;
            y = 95;
            DrawSearchBox(vg, "Search", x, y, 280, 25);
            y += 40;
            DrawDropDown(vg, "Effects", x, y, 280, 28);
            popy = y + 14;
            y += 45;

            // Form
            DrawLabel(vg, "Login", x, y, 280, 20);
            y += 25;
            DrawEditBox(vg, "Email", x, y, 280, 28);
            y += 35;
            DrawEditBox(vg, "Password", x, y, 280, 28);
            y += 38;
            DrawCheckBox(vg, "Remember me", x, y, 140, 28);
            DrawButton(vg, IconLogin, "Sign in", x + 138, y, 140, 28, NVG.RGBA(0, 96, 128, 255));
            y += 45;

            // Slider
            DrawLabel(vg, "Diameter", x, y, 280, 20);
            y += 25;
            DrawEditBoxNum(vg, "123.00", "px", x + 180, y, 100, 28);
            DrawSlider(vg, 0.4f, x, y, 170, 28);
            y += 55;

            DrawButton(vg, IconTrash, "Delete", x, y, 160, 28, NVG.RGBA(128, 16, 8, 255));
            DrawButton(vg, null, "Cancel", x + 170, y, 110, 28, NVG.RGBA(0, 0, 0, 0));

            // Thumbnails box
            ////DrawThumbnails(vg, 365, popy - 30, 160, 300, data.images, t);

            NVG.Restore(vg);
        }

        private static void UnpremultiplyAlpha(byte[] image, int w, int h, int stride)
        {
            // Unpremultiply
            for (int y = 0; y < h; y++)
            {
                int i = y * stride;
                for (int x = 0; x < w; x++)
                {
                    int r = image[i], g = image[i + 1], b = image[i + 2], a = image[i + 3];
                    if (a != 0)
                    {
                        image[i + 0] = (byte)Math.Min(r * 255 / a, 255);
                        image[i + 1] = (byte)Math.Min(g * 255 / a, 255);
                        image[i + 2] = (byte)Math.Min(b * 255 / a, 255);
                    }

                    i += 4;
                }
            }

            // Defringe
            for (int y = 0; y < h; y++)
            {
                int i = y * stride;
                for (int x = 0; x < w; x++)
                {
                    int r = 0, g = 0, b = 0, a = image[i + 3], n = 0;
                    if (a == 0)
                    {
                        if (x - 1 > 0 && image[i - 1] != 0)
                        {
                            r += image[i - 4];
                            g += image[i - 3];
                            b += image[i - 2];
                            n++;
                        }

                        if (x + 1 < w && image[i + 7] != 0)
                        {
                            r += image[i + 4];
                            g += image[i + 5];
                            b += image[i + 6];
                            n++;
                        }

                        if (y - 1 > 0 && image[i - stride + 3] != 0)
                        {
                            r += image[i - stride];
                            g += image[i - stride + 1];
                            b += image[i - stride + 2];
                            n++;
                        }

                        if (y + 1 < h && image[i + stride + 3] != 0)
                        {
                            r += image[i + stride];
                            g += image[i + stride + 1];
                            b += image[i + stride + 2];
                            n++;
                        }

                        if (n > 0)
                        {
                            image[i + 0] = (byte)(r / n);
                            image[i + 1] = (byte)(g / n);
                            image[i + 2] = (byte)(b / n);
                        }
                    }

                    i += 4;
                }
            }
        }

        private static void SetAlpha(byte[] image, int w, int h, int stride, byte a)
        {
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    image[y * stride + x * 4 + 3] = a;
                }
            }
        }

        private static void FlipHorizontal(byte[] image, int w, int h, int stride)
        {
            int i = 0, j = h - 1, k;
            while (i < j)
            {
                int ri = i * stride;
                int rj = j * stride;
                for (k = 0; k < w * 4; k++)
                {
                    byte t = image[ri + k];
                    image[ri + k] = image[rj + k];
                    image[rj + k] = t;
                }

                i++;
                j--;
            }
        }

        public static void SaveScreenShot(int w, int h, bool premult, string name)
        {
            byte[] image = new byte[w * h * 4];
            GL.ReadPixels(0, 0, w, h, PixelFormat.Rgba, PixelType.UnsignedByte, image);
            if (premult)
            {
                UnpremultiplyAlpha(image, w, h, w * 4);
            }
            else
            {
                SetAlpha(image, w, h, w * 4, 255);
            }

            FlipHorizontal(image, w, h, w * 4);
            ////stbi_write_png(name, w, h, 4, image, w * 4); TODO
        }
    }
}
