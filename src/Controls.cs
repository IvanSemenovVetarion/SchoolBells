using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace SchoolBells
{
    public static class WindowChrome
    {
        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attribute, ref int value, int size);

        public static void Apply(IntPtr handle, Color caption)
        {
            if (handle == IntPtr.Zero)
            {
                return;
            }
            try
            {
                int dark = 1;
                if (DwmSetWindowAttribute(handle, 20, ref dark, sizeof(int)) != 0)
                {
                    DwmSetWindowAttribute(handle, 19, ref dark, sizeof(int));
                }

                int color = (caption.R) | (caption.G << 8) | (caption.B << 16);
                DwmSetWindowAttribute(handle, 35, ref color, sizeof(int));

                int text = (255) | (255 << 8) | (255 << 16);
                DwmSetWindowAttribute(handle, 36, ref text, sizeof(int));
            }
            catch
            {
            }
        }
    }

    public static class UiTheme
    {
        public static Color PageBack = Color.FromArgb(245, 248, 252);
        public static Color CardBack = Color.White;
        public static Color Border = Color.FromArgb(210, 220, 232);
        public static Color Text = Color.FromArgb(45, 62, 80);
        public static Color Muted = Color.FromArgb(105, 120, 136);
        public static Color Accent = Color.FromArgb(45, 108, 180);
        public static Color AccentHover = Color.FromArgb(58, 124, 198);
        public static Color HeaderBack = Color.FromArgb(22, 50, 79);
        public static Color HeaderText = Color.White;
        public static Color Success = Color.FromArgb(46, 125, 50);
        public static Color Danger = Color.FromArgb(183, 28, 28);
        public static Color Track = Color.FromArgb(224, 231, 239);

        public static void Apply(ThemePalette palette)
        {
            if (palette == null)
            {
                return;
            }
            PageBack = palette.PageBack;
            CardBack = palette.PanelBack;
            Border = palette.BorderColor;
            Text = palette.TextColor;
            Muted = palette.HintText;
            Accent = palette.Accent;
            AccentHover = palette.AccentHover;
            HeaderBack = palette.HeaderBack;
            HeaderText = Color.White;
            Success = palette.Green;
            Danger = palette.Red;
            Track = palette.GridHeaderBack;
        }
    }

    internal static class RoundGeometry
    {
        public static GraphicsPath Create(Rectangle bounds, int radius)
        {
            int diameter = Math.Max(1, radius * 2);
            GraphicsPath path = new GraphicsPath();
            if (radius <= 0)
            {
                path.AddRectangle(bounds);
                return path;
            }

            Rectangle arc = new Rectangle(bounds.X, bounds.Y, diameter, diameter);
            path.AddArc(arc, 180, 90);
            arc.X = bounds.Right - diameter;
            path.AddArc(arc, 270, 90);
            arc.Y = bounds.Bottom - diameter;
            path.AddArc(arc, 0, 90);
            arc.X = bounds.X;
            path.AddArc(arc, 90, 90);
            path.CloseFigure();
            return path;
        }
    }

    public sealed class RoundedButton : Button
    {
        private bool _hover;
        private bool _pressed;
        private int _cornerRadius = 10;

        public RoundedButton()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            DoubleBuffered = true;
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            UseVisualStyleBackColor = false;
            Cursor = Cursors.Hand;
            TabStop = true;
        }

        public int CornerRadius
        {
            get { return _cornerRadius; }
            set
            {
                _cornerRadius = Math.Max(0, value);
                Invalidate();
            }
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            _hover = true;
            Invalidate();
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            _hover = false;
            _pressed = false;
            Invalidate();
            base.OnMouseLeave(e);
        }

        protected override void OnMouseDown(MouseEventArgs mevent)
        {
            _pressed = true;
            Invalidate();
            base.OnMouseDown(mevent);
        }

        protected override void OnMouseUp(MouseEventArgs mevent)
        {
            _pressed = false;
            Invalidate();
            base.OnMouseUp(mevent);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

            Color backdrop = Parent != null ? Parent.BackColor : UiTheme.PageBack;
            if (backdrop == Color.Transparent)
            {
                backdrop = UiTheme.PageBack;
            }
            using (SolidBrush clear = new SolidBrush(backdrop))
            {
                g.FillRectangle(clear, ClientRectangle);
            }

            Color fill = BackColor;
            if (!Enabled)
            {
                fill = ControlPaint.Light(fill, 0.3f);
            }
            else if (_pressed)
            {
                fill = ControlPaint.Dark(fill, 0.08f);
            }
            else if (_hover)
            {
                Color hover = FlatAppearance.MouseOverBackColor;
                if (hover != Color.Empty && hover != Color.Transparent)
                {
                    fill = hover;
                }
            }

            Rectangle bounds = new Rectangle(0, 0, Width - 1, Height - 1);
            using (GraphicsPath path = RoundGeometry.Create(bounds, _cornerRadius))
            {
                using (SolidBrush brush = new SolidBrush(fill))
                {
                    g.FillPath(brush, path);
                }
                if (FlatAppearance.BorderSize > 0)
                {
                    using (Pen pen = new Pen(FlatAppearance.BorderColor, FlatAppearance.BorderSize))
                    {
                        g.DrawPath(pen, path);
                    }
                }
            }

            Color textColor = Enabled ? ForeColor : UiTheme.Muted;
            TextRenderer.DrawText(g, Text, Font, ClientRectangle, textColor,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter |
                TextFormatFlags.NoPrefix | TextFormatFlags.EndEllipsis);

            if (Focused && ShowFocusCues)
            {
                Rectangle focus = new Rectangle(3, 3, Width - 7, Height - 7);
                using (GraphicsPath path = RoundGeometry.Create(focus, Math.Max(2, _cornerRadius - 3)))
                using (Pen pen = new Pen(Color.FromArgb(110, 255, 255, 255), 1f))
                {
                    g.DrawPath(pen, path);
                }
            }
        }
    }

    public class RoundedPanel : Panel
    {
        private int _cornerRadius = 14;

        public RoundedPanel()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            DoubleBuffered = true;
        }

        public int CornerRadius
        {
            get { return _cornerRadius; }
            set
            {
                _cornerRadius = Math.Max(0, value);
                Invalidate();
            }
        }

        public int BorderSize { get; set; }

        public Color BorderColor { get; set; }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            Color parent = Parent != null ? Parent.BackColor : UiTheme.PageBack;
            if (parent == Color.Transparent)
            {
                parent = UiTheme.PageBack;
            }
            using (SolidBrush clear = new SolidBrush(parent))
            {
                g.FillRectangle(clear, ClientRectangle);
            }

            Rectangle bounds = new Rectangle(0, 0, Width - 1, Height - 1);
            using (GraphicsPath path = RoundGeometry.Create(bounds, _cornerRadius))
            {
                using (SolidBrush brush = new SolidBrush(BackColor))
                {
                    g.FillPath(brush, path);
                }
                Color border = BorderColor == Color.Empty ? UiTheme.Border : BorderColor;
                int size = BorderSize <= 0 ? 1 : BorderSize;
                using (Pen pen = new Pen(border, size))
                {
                    g.DrawPath(pen, path);
                }
            }
        }
    }

    public sealed class CardPanel : GroupBox
    {
        private int _cornerRadius = 15;

        public CardPanel()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            DoubleBuffered = true;
            BackColor = UiTheme.CardBack;
            ForeColor = UiTheme.Text;
        }

        public int CornerRadius
        {
            get { return _cornerRadius; }
            set
            {
                _cornerRadius = Math.Max(0, value);
                Invalidate();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            Color parent = Parent != null ? Parent.BackColor : UiTheme.PageBack;
            if (parent == Color.Transparent)
            {
                parent = UiTheme.PageBack;
            }
            using (SolidBrush clear = new SolidBrush(parent))
            {
                g.FillRectangle(clear, ClientRectangle);
            }

            Rectangle bounds = new Rectangle(0, 0, Width - 1, Height - 1);
            using (GraphicsPath path = RoundGeometry.Create(bounds, _cornerRadius))
            {
                using (SolidBrush shadow = new SolidBrush(Color.FromArgb(14, 20, 40, 70)))
                {
                    Rectangle shadowBounds = new Rectangle(1, 2, Width - 2, Height - 1);
                    using (GraphicsPath shadowPath = RoundGeometry.Create(shadowBounds, _cornerRadius))
                    {
                        g.FillPath(shadow, shadowPath);
                    }
                }
                using (SolidBrush brush = new SolidBrush(BackColor))
                {
                    g.FillPath(brush, path);
                }
                using (Pen pen = new Pen(UiTheme.Border, 1f))
                {
                    g.DrawPath(pen, path);
                }
            }

            if (!string.IsNullOrEmpty(Text))
            {
                Rectangle accent = new Rectangle(14, 7, 3, 13);
                using (SolidBrush brush = new SolidBrush(UiTheme.Accent))
                using (GraphicsPath path = RoundGeometry.Create(accent, 2))
                {
                    g.FillPath(brush, path);
                }
                Rectangle title = new Rectangle(24, 1, Width - 36, 22);
                TextRenderer.DrawText(g, Text, Font, title, ForeColor,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
            }
        }
    }

    public sealed class ModernCheckBox : Control
    {
        private bool _hover;
        private bool _checked;

        public event EventHandler CheckedChanged;

        public ModernCheckBox()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor, true);
            DoubleBuffered = true;
            Cursor = Cursors.Hand;
            TabStop = true;
            Height = 24;
        }

        public bool Checked
        {
            get { return _checked; }
            set
            {
                if (_checked == value)
                {
                    return;
                }
                _checked = value;
                Invalidate();
                if (CheckedChanged != null)
                {
                    CheckedChanged(this, EventArgs.Empty);
                }
            }
        }

        public override Size GetPreferredSize(Size proposedSize)
        {
            Size textSize = TextRenderer.MeasureText(Text, Font);
            return new Size(textSize.Width + 32, Math.Max(24, textSize.Height + 6));
        }

        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);
            if (AutoSize)
            {
                Size = GetPreferredSize(Size.Empty);
            }
            Invalidate();
        }

        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);
            if (AutoSize)
            {
                Size = GetPreferredSize(Size.Empty);
            }
            Invalidate();
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            _hover = true;
            Invalidate();
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            _hover = false;
            Invalidate();
            base.OnMouseLeave(e);
        }

        protected override void OnClick(EventArgs e)
        {
            Checked = !Checked;
            base.OnClick(e);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Space)
            {
                Checked = !Checked;
                e.Handled = true;
            }
            base.OnKeyDown(e);
        }

        protected override void OnGotFocus(EventArgs e)
        {
            Invalidate();
            base.OnGotFocus(e);
        }

        protected override void OnLostFocus(EventArgs e)
        {
            Invalidate();
            base.OnLostFocus(e);
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            Graphics g = pevent.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            Color parent = Parent != null ? Parent.BackColor : UiTheme.CardBack;
            if (parent == Color.Transparent)
            {
                parent = UiTheme.CardBack;
            }
            using (SolidBrush clear = new SolidBrush(parent))
            {
                g.FillRectangle(clear, ClientRectangle);
            }

            const int boxSize = 18;
            int top = Math.Max(0, (Height - boxSize) / 2);
            Rectangle box = new Rectangle(1, top, boxSize, boxSize);
            using (GraphicsPath path = RoundGeometry.Create(box, 5))
            {
                Color fill = _checked ? UiTheme.Accent : UiTheme.CardBack;
                if (!Enabled)
                {
                    fill = ControlPaint.Light(fill, 0.4f);
                }
                else if (_hover && !_checked)
                {
                    fill = ControlPaint.Light(UiTheme.Accent, 0.9f);
                }
                using (SolidBrush brush = new SolidBrush(fill))
                {
                    g.FillPath(brush, path);
                }
                using (Pen pen = new Pen(_checked ? UiTheme.Accent : UiTheme.Border, 1.2f))
                {
                    g.DrawPath(pen, path);
                }
            }

            if (_checked)
            {
                using (Pen pen = new Pen(Color.White, 2f))
                {
                    pen.StartCap = LineCap.Round;
                    pen.EndCap = LineCap.Round;
                    g.DrawLine(pen, box.Left + 4, box.Top + 9, box.Left + 7.5f, box.Top + 12.5f);
                    g.DrawLine(pen, box.Left + 7.5f, box.Top + 12.5f, box.Left + 14, box.Top + 5.5f);
                }
            }

            Color textColor = Enabled ? ForeColor : UiTheme.Muted;
            Rectangle textBounds = new Rectangle(24, 0, Math.Max(0, Width - 26), Height);
            TextRenderer.DrawText(g, Text, Font, textBounds, textColor,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix |
                TextFormatFlags.EndEllipsis);

            if (Focused && ShowFocusCues)
            {
                using (Pen pen = new Pen(Color.FromArgb(90, UiTheme.Accent), 1f))
                {
                    g.DrawRectangle(pen, 0, top - 1, boxSize + 2, boxSize + 2);
                }
            }
        }
    }

    public sealed class ModernSlider : Control
    {
        private int _minimum;
        private int _maximum = 100;
        private int _value;
        private bool _dragging;

        public event EventHandler ValueChanged;

        public ModernSlider()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            DoubleBuffered = true;
            TabStop = true;
            Cursor = Cursors.Hand;
            Height = 36;
        }

        public int Minimum
        {
            get { return _minimum; }
            set
            {
                _minimum = value;
                if (_maximum < _minimum)
                {
                    _maximum = _minimum;
                }
                Value = _value;
                Invalidate();
            }
        }

        public int Maximum
        {
            get { return _maximum; }
            set
            {
                _maximum = value;
                if (_minimum > _maximum)
                {
                    _minimum = _maximum;
                }
                Value = _value;
                Invalidate();
            }
        }

        public int Value
        {
            get { return _value; }
            set
            {
                int clamped = value;
                if (clamped < _minimum)
                {
                    clamped = _minimum;
                }
                if (clamped > _maximum)
                {
                    clamped = _maximum;
                }
                if (_value == clamped)
                {
                    return;
                }
                _value = clamped;
                Invalidate();
                if (ValueChanged != null)
                {
                    ValueChanged(this, EventArgs.Empty);
                }
            }
        }

        public int TickFrequency { get; set; }
        public TickStyle TickStyle { get; set; }
        public int SmallChange { get; set; }
        public int LargeChange { get; set; }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _dragging = true;
                Capture = true;
                Focus();
                SetValueFromX(e.X);
            }
            base.OnMouseDown(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (_dragging)
            {
                SetValueFromX(e.X);
            }
            base.OnMouseMove(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _dragging = false;
                Capture = false;
            }
            base.OnMouseUp(e);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            int step = SmallChange <= 0 ? 1 : SmallChange;
            if (e.KeyCode == Keys.Left || e.KeyCode == Keys.Down)
            {
                Value -= step;
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Right || e.KeyCode == Keys.Up)
            {
                Value += step;
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Home)
            {
                Value = Minimum;
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.End)
            {
                Value = Maximum;
                e.Handled = true;
            }
            base.OnKeyDown(e);
        }

        private void SetValueFromX(int x)
        {
            int left = 12;
            int right = Math.Max(left + 1, Width - 12);
            int width = right - left;
            int span = _maximum - _minimum;
            if (width <= 0 || span <= 0)
            {
                return;
            }
            int position = x - left;
            if (position < 0)
            {
                position = 0;
            }
            if (position > width)
            {
                position = width;
            }
            Value = _minimum + (int)Math.Round((double)position / width * span);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            Color parent = Parent != null ? Parent.BackColor : UiTheme.CardBack;
            if (parent == Color.Transparent)
            {
                parent = UiTheme.CardBack;
            }
            using (SolidBrush clear = new SolidBrush(parent))
            {
                g.FillRectangle(clear, ClientRectangle);
            }

            int left = 12;
            int right = Math.Max(left + 1, Width - 12);
            int trackY = Height / 2 - 3;
            Rectangle track = new Rectangle(left, trackY, right - left, 6);
            int span = _maximum - _minimum;
            int position = span <= 0 ? 0 : (int)Math.Round((double)(_value - _minimum) / span * (track.Width - 1));
            Rectangle fill = new Rectangle(track.Left, track.Top, Math.Max(0, position), track.Height);

            using (GraphicsPath path = RoundGeometry.Create(track, 3))
            using (SolidBrush brush = new SolidBrush(UiTheme.Track))
            {
                g.FillPath(brush, path);
            }
            if (fill.Width > 0)
            {
                using (GraphicsPath path = RoundGeometry.Create(fill, 3))
                using (SolidBrush brush = new SolidBrush(UiTheme.Accent))
                {
                    g.FillPath(brush, path);
                }
            }

            int thumbX = track.Left + position;
            Rectangle thumb = new Rectangle(thumbX - 8, Height / 2 - 8, 16, 16);
            using (SolidBrush shadow = new SolidBrush(Color.FromArgb(35, 20, 40, 70)))
            {
                g.FillEllipse(shadow, thumb.X + 1, thumb.Y + 2, thumb.Width, thumb.Height);
            }
            using (SolidBrush brush = new SolidBrush(UiTheme.CardBack))
            {
                g.FillEllipse(brush, thumb);
            }
            using (Pen pen = new Pen(UiTheme.Accent, 2f))
            {
                g.DrawEllipse(pen, thumb);
            }

            if (Focused && ShowFocusCues)
            {
                using (Pen pen = new Pen(Color.FromArgb(80, UiTheme.Accent), 1f))
                {
                    g.DrawEllipse(pen, thumb.X - 3, thumb.Y - 3, thumb.Width + 6, thumb.Height + 6);
                }
            }
        }
    }

    public sealed class LevelMeter : Control
    {
        private double _value;

        public LevelMeter()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            DoubleBuffered = true;
            Height = 12;
        }

        public double Value
        {
            get { return _value; }
            set
            {
                double clamped = value;
                if (clamped < 0)
                {
                    clamped = 0;
                }
                if (clamped > 1)
                {
                    clamped = 1;
                }
                _value = clamped;
                Invalidate();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            Color parent = Parent != null ? Parent.BackColor : UiTheme.CardBack;
            if (parent == Color.Transparent)
            {
                parent = UiTheme.CardBack;
            }
            using (SolidBrush clear = new SolidBrush(parent))
            {
                g.FillRectangle(clear, ClientRectangle);
            }

            Rectangle bounds = new Rectangle(0, 1, Width - 1, Height - 3);
            using (GraphicsPath path = RoundGeometry.Create(bounds, 4))
            using (SolidBrush brush = new SolidBrush(UiTheme.Track))
            {
                g.FillPath(brush, path);
            }

            int width = (int)Math.Round((Width - 1) * _value);
            if (width > 1)
            {
                Rectangle fill = new Rectangle(0, 1, width, Height - 3);
                Color color = _value > 0.88 ? UiTheme.Danger : (_value > 0.65 ? Color.FromArgb(230, 150, 35) : UiTheme.Success);
                using (GraphicsPath path = RoundGeometry.Create(fill, 4))
                using (SolidBrush brush = new SolidBrush(color))
                {
                    g.FillPath(brush, path);
                }
            }
        }
    }
}
