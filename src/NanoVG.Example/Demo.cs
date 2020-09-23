using OpenTK.Graphics.OpenGL;
using System;
using static NanoVG.Color;

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

            vg.Save();
            ////nvgClearState(vg);

            // Window
            vg.BeginPath();
            vg.RoundedRect(x, y, w, h, cornerRadius);
            vg.FillColor(RGBA(28, 30, 34, 192));
            ////nvgFillColor(vg, nvgRGBA(0,0,0,128));
            vg.Fill();

            // Drop shadow
            shadowPaint = Paint.BoxGradient(x, y + 2, w, h, cornerRadius * 2, 10, RGBA(0, 0, 0, 128), RGBA(0, 0, 0, 0));
            vg.BeginPath();
            vg.Rect(x - 10, y - 10, w + 20, h + 30);
            vg.RoundedRect(x, y, w, h, cornerRadius);
            vg.PathWinding(Winding.HOLE);
            vg.FillPaint(shadowPaint);
            vg.Fill();

            // Header
            headerPaint = Paint.LinearGradient(x, y, x, y + 15, RGBA(255, 255, 255, 8), RGBA(0, 0, 0, 16));
            vg.BeginPath();
            vg.RoundedRect(x + 1, y + 1, w - 2, 30, cornerRadius - 1);
            vg.FillPaint(headerPaint);
            vg.Fill();
            vg.BeginPath();
            vg.MoveTo(x + 0.5f, y + 0.5f + 30);
            vg.LineTo(x + 0.5f + w - 1, y + 0.5f + 30);
            vg.StrokeColor(RGBA(0, 0, 0, 32));
            vg.Stroke();

            ////vg.FontSize(15.0f);
            ////vg.FontFace("sans-bold");
            ////vg.TextAlign(Align.NVG_ALIGN_CENTER | Align.NVG_ALIGN_MIDDLE);
            ////
            ////vg.FontBlur(2);
            ////vg.FillColor(NVG.RGBA(0, 0, 0, 128));
            ////vg.Text(x + w / 2, y + 16 + 1, title, 0);
            ////
            ////vg.FontBlur(0);
            ////vg.FillColor(NVG.RGBA(220, 220, 220, 160));
            ////vg.Text(x + w / 2, y + 16, title, 0);

            vg.Restore();
        }

        private static void DrawSearchBox(Context vg, string text, float x, float y, float w, float h)
        {
            Paint bg;
            float cornerRadius = h / 2 - 1;

            // Edit
            bg = Paint.BoxGradient(x, y + 1.5f, w, h, h / 2, 5, RGBA(0, 0, 0, 16), RGBA(0, 0, 0, 92));
            vg.BeginPath();
            vg.RoundedRect(x, y, w, h, cornerRadius);
            vg.FillPaint(bg);
            vg.Fill();

            // commented out in source
            ////nvgBeginPath();
            ////nvgRoundedRect(x+0.5f,y+0.5f, w-1,h-1, cornerRadius-0.5f);
            ////nvgStrokeColor(nvgRGBA(0,0,0,48));
            ////nvgStroke();

            ////vg.FontSize(h * 1.3f);
            ////vg.FontFace("icons");
            ////vg.FillColor(NVG.RGBA(255, 255, 255, 64));
            ////vg.TextAlign(Align.NVG_ALIGN_CENTER | Align.NVG_ALIGN_MIDDLE);
            ////vg.Text(x + h * 0.55f, y + h * 0.55f, IconSearch, 0);
            ////
            ////vg.FontSize(17.0f);
            ////vg.FontFace("sans");
            ////vg.FillColor(NVG.RGBA(255, 255, 255, 32));
            ////
            ////vg.TextAlign(Align.NVG_ALIGN_LEFT | Align.NVG_ALIGN_MIDDLE);
            ////vg.Text(x + h * 1.05f, y + h * 0.5f, text, 0);
            ////
            ////vg.FontSize(h * 1.3f);
            ////vg.FontFace("icons");
            ////vg.FillColor(NVG.RGBA(255, 255, 255, 32));
            ////vg.TextAlign(Align.NVG_ALIGN_CENTER | Align.NVG_ALIGN_MIDDLE);
            ////vg.Text(x + w - h * 0.55f, y + h * 0.55f, IconCircledCross, 0);
        }

        private static void DrawDropDown(Context vg, string text, float x, float y, float w, float h)
        {
            Paint bg;
            float cornerRadius = 4.0f;

            bg = Paint.LinearGradient(x, y, x, y + h, RGBA(255, 255, 255, 16), RGBA(0, 0, 0, 16));
            vg.BeginPath();
            vg.RoundedRect(x + 1, y + 1, w - 2, h - 2, cornerRadius - 1);
            vg.FillPaint(bg);
            vg.Fill();

            vg.BeginPath();
            vg.RoundedRect(x + 0.5f, y + 0.5f, w - 1, h - 1, cornerRadius - 0.5f);
            vg.StrokeColor(RGBA(0, 0, 0, 48));
            vg.Stroke();

            ////vg.FontSize(17.0f);
            ////vg.FontFace("sans");
            ////vg.FillColor(NVG.RGBA(255, 255, 255, 160));
            ////vg.TextAlign(Align.NVG_ALIGN_LEFT | Align.NVG_ALIGN_MIDDLE);
            ////vg.Text(x + h * 0.3f, y + h * 0.5f, text, 0);
            ////
            ////vg.FontSize(h * 1.3f);
            ////vg.FontFace("icons");
            ////vg.FillColor(NVG.RGBA(255, 255, 255, 64));
            ////vg.TextAlign(Align.NVG_ALIGN_CENTER | Align.NVG_ALIGN_MIDDLE);
            ////vg.Text(x + w - h * 0.5f, y + h * 0.5f, IconChevronRight, 0);
        }

        private static void DrawLabel(Context vg, string text, float x, float y, float w, float h)
        {
            ////NVG_NOTUSED(w);

            ////vg.FontSize(15.0f);
            ////vg.FontFace("sans");
            ////vg.FillColor(NVG.RGBA(255, 255, 255, 128));
            ////
            ////vg.TextAlign(Align.NVG_ALIGN_LEFT | Align.NVG_ALIGN_MIDDLE);
            ////vg.Text(x, y + h * 0.5f, text, 0);
        }

        private static void DrawEditBoxBase(Context vg, float x, float y, float w, float h)
        {
            // Edit
            var bg = Paint.BoxGradient(x + 1, y + 1 + 1.5f, w - 2, h - 2, 3, 4, RGBA(255, 255, 255, 32), RGBA(32, 32, 32, 32));
            vg.BeginPath();
            vg.RoundedRect(x + 1, y + 1, w - 2, h - 2, 4 - 1);
            vg.FillPaint(bg);
            vg.Fill();

            vg.BeginPath();
            vg.RoundedRect(x + 0.5f, y + 0.5f, w - 1, h - 1, 4 - 0.5f);
            vg.StrokeColor(RGBA(0, 0, 0, 48));
            vg.Stroke();
        }

        private static void DrawEditBox(Context vg, string text, float x, float y, float w, float h)
        {
            DrawEditBoxBase(vg, x, y, w, h);

            ////vg.FontSize(17.0f);
            ////vg.FontFace("sans");
            ////vg.FillColor(NVG.RGBA(255, 255, 255, 64));
            ////vg.TextAlign(Align.NVG_ALIGN_LEFT | Align.NVG_ALIGN_MIDDLE);
            ////vg.Text(x + h * 0.3f, y + h * 0.5f, text, 0);
        }

        private static void DrawEditBoxNum(Context vg, string text, string units, float x, float y, float w, float h)
        {
            float uw;

            DrawEditBoxBase(vg, x, y, w, h);

            ////uw = vg.TextBounds(0, 0, units, 0, null);
            ////
            ////vg.FontSize(15.0f);
            ////vg.FontFace("sans");
            ////vg.FillColor(NVG.RGBA(255, 255, 255, 64));
            ////vg.TextAlign(Align.NVG_ALIGN_RIGHT | Align.NVG_ALIGN_MIDDLE);
            ////vg.Text(x + w - h * 0.3f, y + h * 0.5f, units, 0);
            ////
            ////vg.FontSize(17.0f);
            ////vg.FontFace("sans");
            ////vg.FillColor(NVG.RGBA(255, 255, 255, 128));
            ////vg.TextAlign(Align.NVG_ALIGN_RIGHT | Align.NVG_ALIGN_MIDDLE);
            ////vg.Text(x + w - uw - h * 0.5f, y + h * 0.5f, text, 0);
        }

        private static void DrawCheckBox(Context vg, string text, float x, float y, float w, float h)
        {
            Paint bg;
            ////NVG_NOTUSED(w);

            ////vg.FontSize(15.0f);
            ////vg.FontFace("sans");
            ////vg.FillColor(NVG.RGBA(255, 255, 255, 160));
            ////
            ////vg.TextAlign(Align.NVG_ALIGN_LEFT | Align.NVG_ALIGN_MIDDLE);
            ////vg.Text(x + 28, y + h * 0.5f, text, 0);

            bg = Paint.BoxGradient(x + 1, y + (int)(h * 0.5f) - 9 + 1, 18, 18, 3, 3, RGBA(0, 0, 0, 32), RGBA(0, 0, 0, 92));
            vg.BeginPath();
            vg.RoundedRect(x + 1, y + (int)(h * 0.5f) - 9, 18, 18, 3);
            vg.FillPaint(bg);
            vg.Fill();

            ////vg.FontSize(33);
            ////vg.FontFace("icons");
            ////vg.FillColor(NVG.RGBA(255, 255, 255, 128));
            ////vg.TextAlign(Align.NVG_ALIGN_CENTER | Align.NVG_ALIGN_MIDDLE);
            ////vg.Text(x + 9 + 2, y + h * 0.5f, IconCheck, 0);
        }

        private static void DrawButton(Context vg, string preicon, string text, float x, float y, float w, float h, Color col)
        {
            Paint bg;
            float cornerRadius = 4.0f;
            float tw = 0, iw = 0;

            bg = Paint.LinearGradient(x, y, x, y + h, RGBA(255, 255, 255, IsBlack(col) ? (byte)16 : (byte)32), RGBA(0, 0, 0, IsBlack(col) ? (byte)16 : (byte)32));
            vg.BeginPath();
            vg.RoundedRect(x + 1, y + 1, w - 2, h - 2, cornerRadius - 1);
            if (!IsBlack(col))
            {
                vg.FillColor(col);
                vg.Fill();
            }

            vg.FillPaint(bg);
            vg.Fill();

            vg.BeginPath();
            vg.RoundedRect(x + 0.5f, y + 0.5f, w - 1, h - 1, cornerRadius - 0.5f);
            vg.StrokeColor(RGBA(0, 0, 0, 48));
            vg.Stroke();

            ////vg.FontSize(17.0f);
            ////vg.FontFace("sans-bold");
            ////tw = vg.TextBounds(0, 0, text, 0, null);
            ////if (!string.IsNullOrEmpty(preicon))
            ////{
            ////    vg.FontSize(h * 1.3f);
            ////    vg.FontFace("icons");
            ////    iw = vg.TextBounds(0, 0, preicon, 0, null);
            ////    iw += h * 0.15f;
            ////}
            ////
            ////if (!string.IsNullOrEmpty(preicon))
            ////{
            ////    vg.FontSize(h * 1.3f);
            ////    vg.FontFace("icons");
            ////    vg.FillColor(NVG.RGBA(255, 255, 255, 96));
            ////    vg.TextAlign(Align.NVG_ALIGN_LEFT | Align.NVG_ALIGN_MIDDLE);
            ////    vg.Text(x + w * 0.5f - tw * 0.5f - iw * 0.75f, y + h * 0.5f, preicon, 0);
            ////}
            ////
            ////vg.FontSize(17.0f);
            ////vg.FontFace("sans-bold");
            ////vg.TextAlign(Align.NVG_ALIGN_LEFT | Align.NVG_ALIGN_MIDDLE);
            ////vg.FillColor(RGBA(0, 0, 0, 160));
            ////vg.Text(x + w * 0.5f - tw * 0.5f + iw * 0.25f, y + h * 0.5f - 1, text, 0);
            ////vg.FillColor(RGBA(255, 255, 255, 160));
            ////vg.Text(x + w * 0.5f - tw * 0.5f + iw * 0.25f, y + h * 0.5f, text, 0);
        }

        private static void DrawSlider(Context vg, float pos, float x, float y, float w, float h)
        {
            Paint bg, knob;
            float cy = y + (int)(h * 0.5f);
            float kr = (int)(h * 0.25f);

            vg.Save();
            ////nvgClearState(vg);

            // Slot
            bg = Paint.BoxGradient(x, cy - 2 + 1, w, 4, 2, 2, RGBA(0, 0, 0, 32), RGBA(0, 0, 0, 128));
            vg.BeginPath();
            vg.RoundedRect(x, cy - 2, w, 4, 2);
            vg.FillPaint(bg);
            vg.Fill();

            // Knob Shadow
            bg = Paint.RadialGradient(x + (int)(pos * w), cy + 1, kr - 3, kr + 3, RGBA(0, 0, 0, 64), RGBA(0, 0, 0, 0));
            vg.BeginPath();
            vg.Rect(x + (int)(pos * w) - kr - 5, cy - kr - 5, kr * 2 + 5 + 5, kr * 2 + 5 + 5 + 3);
            vg.Circle(x + (int)(pos * w), cy, kr);
            vg.PathWinding(Winding.HOLE);
            vg.FillPaint(bg);
            vg.Fill();

            // Knob
            knob = Paint.LinearGradient(x, cy - kr, x, cy + kr, RGBA(255, 255, 255, 16), RGBA(0, 0, 0, 16));
            vg.BeginPath();
            vg.Circle(x + (int)(pos * w), cy, kr - 1);
            vg.FillColor(RGBA(40, 43, 48, 255));
            vg.Fill();
            vg.FillPaint(knob);
            vg.Fill();

            vg.BeginPath();
            vg.Circle(x + (int)(pos * w), cy, kr - 0.5f);
            vg.StrokeColor(RGBA(0, 0, 0, 92));
            vg.Stroke();

            vg.Restore();
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

            bg = Paint.LinearGradient(x, y + h * 0.5f, x + w * 0.1f, y + h, RGBA(0, 0, 0, 32), RGBA(0, 0, 0, 16));
            vg.BeginPath();
            vg.Ellipse(lx + 3.0f, ly + 16.0f, ex, ey);
            vg.Ellipse(rx + 3.0f, ry + 16.0f, ex, ey);
            vg.FillPaint(bg);
            vg.Fill();

            bg = Paint.LinearGradient(x, y + h * 0.25f, x + w * 0.1f, y + h, RGBA(220, 220, 220, 255), RGBA(128, 128, 128, 255));
            vg.BeginPath();
            vg.Ellipse(lx, ly, ex, ey);
            vg.Ellipse(rx, ry, ex, ey);
            vg.FillPaint(bg);
            vg.Fill();

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
            vg.BeginPath();
            vg.Ellipse(lx + dx, ly + dy + ey * 0.25f * (1 - blink), br, br * blink);
            vg.FillColor(RGBA(32, 32, 32, 255));
            vg.Fill();

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
            vg.BeginPath();
            vg.Ellipse(rx + dx, ry + dy + ey * 0.25f * (1 - blink), br, br * blink);
            vg.FillColor(RGBA(32, 32, 32, 255));
            vg.Fill();

            gloss = Paint.RadialGradient(lx - ex * 0.25f, ly - ey * 0.5f, ex * 0.1f, ex * 0.75f, RGBA(255, 255, 255, 128), RGBA(255, 255, 255, 0));
            vg.BeginPath();
            vg.Ellipse(lx, ly, ex, ey);
            vg.FillPaint(gloss);
            vg.Fill();

            gloss = Paint.RadialGradient(rx - ex * 0.25f, ry - ey * 0.5f, ex * 0.1f, ex * 0.75f, RGBA(255, 255, 255, 128), RGBA(255, 255, 255, 0));
            vg.BeginPath();
            vg.Ellipse(rx, ry, ex, ey);
            vg.FillPaint(gloss);
            vg.Fill();
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
            bg = Paint.LinearGradient(x, y, x, y + h, RGBA(0, 160, 192, 0), RGBA(0, 160, 192, 64));
            vg.BeginPath();
            vg.MoveTo(sx[0], sy[0]);
            for (i = 1; i < 6; i++)
            {
                vg.BezierTo(sx[i - 1] + dx * 0.5f, sy[i - 1], sx[i] - dx * 0.5f, sy[i], sx[i], sy[i]);
            }

            vg.LineTo(x + w, y + h);
            vg.LineTo(x, y + h);
            vg.FillPaint(bg);
            vg.Fill();

            // Graph line
            vg.BeginPath();
            vg.MoveTo(sx[0], sy[0] + 2);
            for (i = 1; i < 6; i++)
            {
                vg.BezierTo(sx[i - 1] + dx * 0.5f, sy[i - 1] + 2, sx[i] - dx * 0.5f, sy[i] + 2, sx[i], sy[i] + 2);
            }

            vg.StrokeColor(RGBA(0, 0, 0, 32));
            vg.StrokeWidth(3.0f);
            vg.Stroke();

            vg.BeginPath();
            vg.MoveTo(sx[0], sy[0]);
            for (i = 1; i < 6; i++)
            {
                vg.BezierTo(sx[i - 1] + dx * 0.5f, sy[i - 1], sx[i] - dx * 0.5f, sy[i], sx[i], sy[i]);
            }

            vg.StrokeColor(RGBA(0, 160, 192, 255));
            vg.StrokeWidth(3.0f);
            vg.Stroke();

            // Graph sample pos
            for (i = 0; i < 6; i++)
            {
                bg = Paint.RadialGradient(sx[i], sy[i] + 2, 3.0f, 8.0f, RGBA(0, 0, 0, 32), RGBA(0, 0, 0, 0));
                vg.BeginPath();
                vg.Rect(sx[i] - 10, sy[i] - 10 + 2, 20, 20);
                vg.FillPaint(bg);
                vg.Fill();
            }

            vg.BeginPath();
            for (i = 0; i < 6; i++)
            {
                vg.Circle(sx[i], sy[i], 4.0f);
            }

            vg.FillColor(RGBA(0, 160, 192, 255));
            vg.Fill();
            vg.BeginPath();
            for (i = 0; i < 6; i++)
            {
                vg.Circle(sx[i], sy[i], 2.0f);
            }

            vg.FillColor(RGBA(220, 220, 220, 255));
            vg.Fill();

            vg.StrokeWidth(1.0f);
        }

        private static void DrawSpinner(Context vg, float cx, float cy, float r, float t)
        {
            float a0 = 0.0f + t * 6;
            float a1 = (float)Math.PI + t * 6;
            float r0 = r;
            float r1 = r * 0.75f;
            float ax, ay, bx, by;
            Paint paint;

            vg.Save();

            vg.BeginPath();
            vg.Arc(cx, cy, r0, a0, a1, Winding.CW);
            vg.Arc(cx, cy, r1, a1, a0, Winding.CCW);
            vg.ClosePath();
            ax = cx + (float)Math.Cos(a0) * (r0 + r1) * 0.5f;
            ay = cy + (float)Math.Sin(a0) * (r0 + r1) * 0.5f;
            bx = cx + (float)Math.Cos(a1) * (r0 + r1) * 0.5f;
            by = cy + (float)Math.Sin(a1) * (r0 + r1) * 0.5f;
            paint = Paint.LinearGradient(ax, ay, bx, by, RGBA(0, 0, 0, 0), RGBA(0, 0, 0, 128));
            vg.FillPaint(paint);
            vg.Fill();

            vg.Restore();
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

            vg.Save();
            ////nvgClearState(vg);

            // Drop shadow
            shadowPaint = Paint.BoxGradient(x, y + 4, w, h, cornerRadius * 2, 20, RGBA(0, 0, 0, 128), RGBA(0, 0, 0, 0));
            vg.BeginPath();
            vg.Rect(x - 10, y - 10, w + 20, h + 30);
            vg.RoundedRect(x, y, w, h, cornerRadius);
            vg.PathWinding(Winding.HOLE);
            vg.FillPaint(shadowPaint);
            vg.Fill();

            // Window
            vg.BeginPath();
            vg.RoundedRect(x, y, w, h, cornerRadius);
            vg.MoveTo(x - 10, y + arry);
            vg.LineTo(x + 1, y + arry - 11);
            vg.LineTo(x + 1, y + arry + 11);
            vg.FillColor(RGBA(200, 200, 200, 255));
            vg.Fill();

            vg.Save();
            vg.Scissor(x, y, w, h);
            vg.Translate(0, -(stackh - h) * u);

            dv = 1.0f / (float)(images.Length - 1);

            for (i = 0; i < images.Length; i++)
            {
                float tx, ty, v, a;
                tx = x + 10;
                ty = y + 10;
                tx += (i % 2) * (thumb + 10);
                ty += (i / 2) * (thumb + 10);
                vg.ImageSize(images[i], out var imgw, out var imgh);
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

                imgPaint = Paint.ImagePattern(tx + ix, ty + iy, iw, ih, 0.0f / 180.0f * (float)Math.PI, images[i], a);
                vg.BeginPath();
                vg.RoundedRect(tx, ty, thumb, thumb, 5);
                vg.FillPaint(imgPaint);
                vg.Fill();

                shadowPaint = Paint.BoxGradient(tx - 1, ty, thumb + 2, thumb + 2, 5, 3, RGBA(0, 0, 0, 128), RGBA(0, 0, 0, 0));
                vg.BeginPath();
                vg.Rect(tx - 5, ty - 5, thumb + 10, thumb + 10);
                vg.RoundedRect(tx, ty, thumb, thumb, 6);
                vg.PathWinding(Winding.HOLE);
                vg.FillPaint(shadowPaint);
                vg.Fill();

                vg.BeginPath();
                vg.RoundedRect(tx + 0.5f, ty + 0.5f, thumb - 1, thumb - 1, 4 - 0.5f);
                vg.StrokeWidth(1.0f);
                vg.StrokeColor(RGBA(255, 255, 255, 192));
                vg.Stroke();
            }

            vg.Restore();

            // Hide fades
            fadePaint = Paint.LinearGradient(x, y, x, y + 6, RGBA(200, 200, 200, 255), RGBA(200, 200, 200, 0));
            vg.BeginPath();
            vg.Rect(x + 4, y, w - 8, 6);
            vg.FillPaint(fadePaint);
            vg.Fill();

            fadePaint = Paint.LinearGradient(x, y + h, x, y + h - 6, RGBA(200, 200, 200, 255), RGBA(200, 200, 200, 0));
            vg.BeginPath();
            vg.Rect(x + 4, y + h - 6, w - 8, 6);
            vg.FillPaint(fadePaint);
            vg.Fill();

            // Scroll bar
            shadowPaint = Paint.BoxGradient(x + w - 12 + 1, y + 4 + 1, 8, h - 8, 3, 4, RGBA(0, 0, 0, 32), RGBA(0, 0, 0, 92));
            vg.BeginPath();
            vg.RoundedRect(x + w - 12, y + 4, 8, h - 8, 3);
            vg.FillPaint(shadowPaint);
            ////nvgFillColor(vg, nvgRGBA(255,0,0,128));
            vg.Fill();

            scrollh = (h / stackh) * (h - 8);
            shadowPaint = Paint.BoxGradient(x + w - 12 - 1, y + 4 + (h - 8 - scrollh) * u - 1, 8, scrollh, 3, 4, RGBA(220, 220, 220, 255), RGBA(128, 128, 128, 255));
            vg.BeginPath();
            vg.RoundedRect(x + w - 12 + 1, y + 4 + 1 + (h - 8 - scrollh) * u, 8 - 2, scrollh - 2, 2);
            vg.FillPaint(shadowPaint);
            ////nvgFillColor(vg, nvgRGBA(0,0,0,128));
            vg.Fill();

            vg.Restore();
        }

        private static void DrawColorwheel(Context vg, float x, float y, float w, float h, float t)
        {
            int i;
            float r0, r1, ax, ay, bx, by, cx, cy, aeps, r;
            float hue = (float)Math.Sin(t * 0.12f);
            Paint paint;

            vg.Save();

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
                vg.BeginPath();
                vg.Arc(cx, cy, r0, a0, a1, Winding.CW);
                vg.Arc(cx, cy, r1, a1, a0, Winding.CCW);
                vg.ClosePath();
                ax = cx + (float)Math.Cos(a0) * (r0 + r1) * 0.5f;
                ay = cy + (float)Math.Sin(a0) * (r0 + r1) * 0.5f;
                bx = cx + (float)Math.Cos(a1) * (r0 + r1) * 0.5f;
                by = cy + (float)Math.Sin(a1) * (r0 + r1) * 0.5f;
                paint = Paint.LinearGradient(ax, ay, bx, by, HSLA(a0 / ((float)Math.PI * 2), 1.0f, 0.55f, 255), HSLA(a1 / ((float)Math.PI * 2), 1.0f, 0.55f, 255));
                vg.FillPaint(paint);
                vg.Fill();
            }

            vg.BeginPath();
            vg.Circle(cx, cy, r0 - 0.5f);
            vg.Circle(cx, cy, r1 + 0.5f);
            vg.StrokeColor(RGBA(0, 0, 0, 64));
            vg.StrokeWidth(1.0f);
            vg.Stroke();

            // Selector
            vg.Save();
            vg.Translate(cx, cy);
            vg.Rotate(hue * (float)Math.PI * 2);

            // Marker on
            vg.StrokeWidth(2.0f);
            vg.BeginPath();
            vg.Rect(r0 - 1, -3, r1 - r0 + 2, 6);
            vg.StrokeColor(RGBA(255, 255, 255, 192));
            vg.Stroke();

            paint = Paint.BoxGradient(r0 - 3, -5, r1 - r0 + 6, 10, 2, 4, RGBA(0, 0, 0, 128), RGBA(0, 0, 0, 0));
            vg.BeginPath();
            vg.Rect(r0 - 2 - 10, -4 - 10, r1 - r0 + 4 + 20, 8 + 20);
            vg.Rect(r0 - 2, -4, r1 - r0 + 4, 8);
            vg.PathWinding(Winding.HOLE);
            vg.FillPaint(paint);
            vg.Fill();

            // Center triangle
            r = r0 - 6;
            ax = (float)Math.Cos(120.0f / 180.0f * (float)Math.PI) * r;
            ay = (float)Math.Sin(120.0f / 180.0f * (float)Math.PI) * r;
            bx = (float)Math.Cos(-120.0f / 180.0f * (float)Math.PI) * r;
            by = (float)Math.Sin(-120.0f / 180.0f * (float)Math.PI) * r;
            vg.BeginPath();
            vg.MoveTo(r, 0);
            vg.LineTo(ax, ay);
            vg.LineTo(bx, by);
            vg.ClosePath();
            paint = Paint.LinearGradient(r, 0, ax, ay, HSLA(hue, 1.0f, 0.5f, 255), RGBA(255, 255, 255, 255));
            vg.FillPaint(paint);
            vg.Fill();
            paint = Paint.LinearGradient((r + ax) * 0.5f, (0 + ay) * 0.5f, bx, by, RGBA(0, 0, 0, 0), RGBA(0, 0, 0, 255));
            vg.FillPaint(paint);
            vg.Fill();
            vg.StrokeColor(RGBA(0, 0, 0, 64));
            vg.Stroke();

            // Select circle on triangle
            ax = (float)Math.Cos(120.0f / 180.0f * (float)Math.PI) * r * 0.3f;
            ay = (float)Math.Sin(120.0f / 180.0f * (float)Math.PI) * r * 0.4f;
            vg.StrokeWidth(2.0f);
            vg.BeginPath();
            vg.Circle(ax, ay, 5);
            vg.StrokeColor(RGBA(255, 255, 255, 192));
            vg.Stroke();

            paint = Paint.RadialGradient(ax, ay, 7, 9, RGBA(0, 0, 0, 64), RGBA(0, 0, 0, 0));
            vg.BeginPath();
            vg.Rect(ax - 20, ay - 20, 40, 40);
            vg.Circle(ax, ay, 7);
            vg.PathWinding(Winding.HOLE);
            vg.FillPaint(paint);
            vg.Fill();

            vg.Restore();

            vg.Restore();
        }

        private static void DrawLines(Context vg, float x, float y, float w, float h, float t)
        {
            int i, j;
            float pad = 5.0f, s = w / 9.0f - pad * 2;
            float[] pts = new float[4 * 2];
            float fx, fy;
            var joins = new LineCap[] { LineCap.MITER, LineCap.ROUND, LineCap.BEVEL };
            var caps = new LineCap[] { LineCap.BUTT, LineCap.ROUND, LineCap.SQUARE };
            ////NVG_NOTUSED(h);

            vg.Save();
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

                    vg.LineCap(caps[i]);
                    vg.LineJoin(joins[j]);

                    vg.StrokeWidth(s * 0.3f);
                    vg.StrokeColor(RGBA(0, 0, 0, 160));
                    vg.BeginPath();
                    vg.MoveTo(fx + pts[0], fy + pts[1]);
                    vg.LineTo(fx + pts[2], fy + pts[3]);
                    vg.LineTo(fx + pts[4], fy + pts[5]);
                    vg.LineTo(fx + pts[6], fy + pts[7]);
                    vg.Stroke();

                    vg.LineCap(LineCap.BUTT);
                    vg.LineJoin(LineCap.BEVEL);

                    vg.StrokeWidth(1.0f);
                    vg.StrokeColor(RGBA(0, 192, 255, 255));
                    vg.BeginPath();
                    vg.MoveTo(fx + pts[0], fy + pts[1]);
                    vg.LineTo(fx + pts[2], fy + pts[3]);
                    vg.LineTo(fx + pts[4], fy + pts[5]);
                    vg.LineTo(fx + pts[6], fy + pts[7]);
                    vg.Stroke();
                }
            }

            vg.Restore();
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
                vg.DeleteImage(data.images[i]);
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
            vg.Save();
            vg.StrokeColor(RGBA(0, 0, 0, 255));

            for (int i = 0; i < 20; i++)
            {
                float w = (i + 0.5f) * 0.1f;
                vg.StrokeWidth(w);
                vg.BeginPath();
                vg.MoveTo(x, y);
                vg.LineTo(x + width, y + width * 0.3f);
                vg.Stroke();
                y += 10;
            }

            vg.Restore();
        }

        private static void DrawCaps(Context vg, float x, float y, float width)
        {
            var caps = new LineCap[] { LineCap.BUTT, LineCap.ROUND, LineCap.SQUARE };
            float lineWidth = 8.0f;

            vg.Save();

            vg.BeginPath();
            vg.Rect(x - lineWidth / 2, y, width + lineWidth, 40);
            vg.FillColor(RGBA(255, 255, 255, 32));
            vg.Fill();

            vg.BeginPath();
            vg.Rect(x, y, width, 40);
            vg.FillColor(RGBA(255, 255, 255, 32));
            vg.Fill();

            vg.StrokeWidth(lineWidth);
            for (int i = 0; i < 3; i++)
            {
                vg.LineCap(caps[i]);
                vg.StrokeColor(RGBA(0, 0, 0, 255));
                vg.BeginPath();
                vg.MoveTo(x, y + i * 10 + 5);
                vg.LineTo(x + width, y + i * 10 + 5);
                vg.Stroke();
            }

            vg.Restore();
        }

        private static void DrawScissor(Context vg, float x, float y, float t)
        {
            vg.Save();

            // Draw first rect and set scissor to it's area.
            vg.Translate(x, y);
            vg.Rotate(Context.DegToRad(5));
            vg.BeginPath();
            vg.Rect(-20, -20, 60, 40);
            vg.FillColor(RGBA(255, 0, 0, 255));
            vg.Fill();
            vg.Scissor(-20, -20, 60, 40);

            // Draw second rectangle with offset and rotation.
            vg.Translate(40, 0);
            vg.Rotate(t);

            // Draw the intended second rectangle without any scissoring.
            vg.Save();
            vg.ResetScissor();
            vg.BeginPath();
            vg.Rect(-20, -10, 60, 30);
            vg.FillColor(RGBA(255, 128, 0, 64));
            vg.Fill();
            vg.Restore();

            // Draw second rectangle with combined scissoring.
            vg.IntersectScissor(-20, -10, 60, 30);
            vg.BeginPath();
            vg.Rect(-20, -10, 60, 30);
            vg.FillColor(RGBA(255, 128, 0, 255));
            vg.Fill();

            vg.Restore();
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

            vg.Save();
            if (blowup)
            {
                vg.Rotate((float)Math.Sin(t * 0.3f) * 5.0f / 180.0f * (float)Math.PI);
                vg.Scale(2.0f, 2.0f);
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
            DrawButton(vg, IconLogin, "Sign in", x + 138, y, 140, 28, RGBA(0, 96, 128, 255));
            y += 45;

            // Slider
            DrawLabel(vg, "Diameter", x, y, 280, 20);
            y += 25;
            DrawEditBoxNum(vg, "123.00", "px", x + 180, y, 100, 28);
            DrawSlider(vg, 0.4f, x, y, 170, 28);
            y += 55;

            DrawButton(vg, IconTrash, "Delete", x, y, 160, 28, RGBA(128, 16, 8, 255));
            DrawButton(vg, null, "Cancel", x + 170, y, 110, 28, RGBA(0, 0, 0, 0));

            // Thumbnails box
            ////DrawThumbnails(vg, 365, popy - 30, 160, 300, data.images, t);

            vg.Restore();
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
