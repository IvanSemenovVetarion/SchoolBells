using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace SchoolBells
{
    public sealed class ThemePalette
    {
        public string Id;
        public string Name;
        public Color HeaderBack;
        public Color HeaderMuted;
        public Color HeaderButton;
        public Color HeaderButtonHover;
        public Color Accent;
        public Color AccentHover;
        public Color PageBack;
        public Color BorderColor;
        public Color TextColor;
        public Color Green;
        public Color GreenHover;
        public Color Red;
        public Color RedHover;
        public Color PanelBack;
        public Color GridBack;
        public Color GridHeaderBack;
        public Color GridHeaderText;
        public Color GridAltBack;
        public Color GridSelectionBack;
        public Color GridLine;
        public Color HintText;
        public Color StatusBack;
        public Color StatusText;
        public bool Snow;
    }

    public static class ThemeManager
    {
        private static readonly Dictionary<string, ThemePalette> Palettes =
            new Dictionary<string, ThemePalette>(StringComparer.OrdinalIgnoreCase);

        static ThemeManager()
        {
            Add(new ThemePalette
            {
                Id = "classic",
                Name = "Классическая синяя",
                HeaderBack = Color.FromArgb(22, 50, 79),
                HeaderMuted = Color.FromArgb(157, 180, 204),
                HeaderButton = Color.FromArgb(43, 74, 111),
                HeaderButtonHover = Color.FromArgb(58, 92, 134),
                Accent = Color.FromArgb(45, 108, 180),
                AccentHover = Color.FromArgb(58, 124, 198),
                PageBack = Color.FromArgb(245, 248, 252),
                BorderColor = Color.FromArgb(201, 214, 227),
                TextColor = Color.FromArgb(45, 62, 80),
                Green = Color.FromArgb(46, 125, 50),
                GreenHover = Color.FromArgb(56, 142, 60),
                Red = Color.FromArgb(183, 28, 28),
                RedHover = Color.FromArgb(211, 47, 47),
                PanelBack = Color.White,
                GridBack = Color.White,
                GridHeaderBack = Color.FromArgb(234, 241, 248),
                GridHeaderText = Color.FromArgb(49, 70, 91),
                GridAltBack = Color.FromArgb(245, 249, 254),
                GridSelectionBack = Color.FromArgb(214, 233, 255),
                GridLine = Color.FromArgb(220, 229, 239),
                HintText = Color.FromArgb(105, 120, 136),
                StatusBack = Color.FromArgb(232, 238, 246),
                StatusText = Color.FromArgb(64, 86, 109),
                Snow = false
            });

            Add(new ThemePalette
            {
                Id = "spring",
                Name = "Весенняя зелень",
                HeaderBack = Color.FromArgb(23, 74, 58),
                HeaderMuted = Color.FromArgb(171, 210, 190),
                HeaderButton = Color.FromArgb(37, 99, 78),
                HeaderButtonHover = Color.FromArgb(49, 123, 97),
                Accent = Color.FromArgb(33, 132, 96),
                AccentHover = Color.FromArgb(44, 157, 115),
                PageBack = Color.FromArgb(244, 250, 246),
                BorderColor = Color.FromArgb(196, 222, 207),
                TextColor = Color.FromArgb(39, 68, 55),
                Green = Color.FromArgb(46, 125, 50),
                GreenHover = Color.FromArgb(56, 142, 60),
                Red = Color.FromArgb(183, 28, 28),
                RedHover = Color.FromArgb(211, 47, 47),
                PanelBack = Color.White,
                GridBack = Color.White,
                GridHeaderBack = Color.FromArgb(230, 244, 236),
                GridHeaderText = Color.FromArgb(37, 76, 57),
                GridAltBack = Color.FromArgb(244, 251, 247),
                GridSelectionBack = Color.FromArgb(206, 238, 220),
                GridLine = Color.FromArgb(215, 234, 223),
                HintText = Color.FromArgb(91, 118, 102),
                StatusBack = Color.FromArgb(228, 242, 234),
                StatusText = Color.FromArgb(49, 88, 66),
                Snow = false
            });

            Add(new ThemePalette
            {
                Id = "summer",
                Name = "Летнее небо",
                HeaderBack = Color.FromArgb(15, 74, 112),
                HeaderMuted = Color.FromArgb(166, 207, 232),
                HeaderButton = Color.FromArgb(28, 101, 148),
                HeaderButtonHover = Color.FromArgb(39, 126, 181),
                Accent = Color.FromArgb(0, 131, 178),
                AccentHover = Color.FromArgb(0, 154, 204),
                PageBack = Color.FromArgb(244, 250, 254),
                BorderColor = Color.FromArgb(190, 219, 236),
                TextColor = Color.FromArgb(31, 65, 87),
                Green = Color.FromArgb(46, 125, 50),
                GreenHover = Color.FromArgb(56, 142, 60),
                Red = Color.FromArgb(183, 28, 28),
                RedHover = Color.FromArgb(211, 47, 47),
                PanelBack = Color.White,
                GridBack = Color.White,
                GridHeaderBack = Color.FromArgb(225, 241, 250),
                GridHeaderText = Color.FromArgb(28, 75, 104),
                GridAltBack = Color.FromArgb(242, 249, 253),
                GridSelectionBack = Color.FromArgb(202, 232, 247),
                GridLine = Color.FromArgb(208, 230, 243),
                HintText = Color.FromArgb(83, 117, 138),
                StatusBack = Color.FromArgb(225, 241, 250),
                StatusText = Color.FromArgb(39, 85, 113),
                Snow = false
            });

            Add(new ThemePalette
            {
                Id = "autumn",
                Name = "Осенняя охра",
                HeaderBack = Color.FromArgb(86, 48, 26),
                HeaderMuted = Color.FromArgb(226, 190, 153),
                HeaderButton = Color.FromArgb(116, 70, 37),
                HeaderButtonHover = Color.FromArgb(143, 89, 48),
                Accent = Color.FromArgb(178, 100, 34),
                AccentHover = Color.FromArgb(201, 119, 44),
                PageBack = Color.FromArgb(253, 249, 243),
                BorderColor = Color.FromArgb(231, 211, 188),
                TextColor = Color.FromArgb(77, 54, 38),
                Green = Color.FromArgb(76, 123, 55),
                GreenHover = Color.FromArgb(90, 141, 65),
                Red = Color.FromArgb(170, 54, 35),
                RedHover = Color.FromArgb(199, 69, 46),
                PanelBack = Color.White,
                GridBack = Color.White,
                GridHeaderBack = Color.FromArgb(248, 236, 220),
                GridHeaderText = Color.FromArgb(94, 62, 36),
                GridAltBack = Color.FromArgb(253, 247, 239),
                GridSelectionBack = Color.FromArgb(249, 225, 196),
                GridLine = Color.FromArgb(235, 219, 200),
                HintText = Color.FromArgb(126, 100, 76),
                StatusBack = Color.FromArgb(247, 235, 220),
                StatusText = Color.FromArgb(99, 69, 43),
                Snow = false
            });

            Add(new ThemePalette
            {
                Id = "winter",
                Name = "Зима / Новый год",
                HeaderBack = Color.FromArgb(16, 37, 64),
                HeaderMuted = Color.FromArgb(174, 200, 226),
                HeaderButton = Color.FromArgb(31, 62, 99),
                HeaderButtonHover = Color.FromArgb(46, 84, 128),
                Accent = Color.FromArgb(58, 132, 196),
                AccentHover = Color.FromArgb(77, 157, 223),
                PageBack = Color.FromArgb(243, 248, 253),
                BorderColor = Color.FromArgb(194, 214, 234),
                TextColor = Color.FromArgb(35, 58, 82),
                Green = Color.FromArgb(43, 122, 96),
                GreenHover = Color.FromArgb(54, 145, 114),
                Red = Color.FromArgb(177, 43, 53),
                RedHover = Color.FromArgb(207, 58, 69),
                PanelBack = Color.White,
                GridBack = Color.White,
                GridHeaderBack = Color.FromArgb(228, 240, 251),
                GridHeaderText = Color.FromArgb(39, 70, 103),
                GridAltBack = Color.FromArgb(244, 250, 255),
                GridSelectionBack = Color.FromArgb(205, 231, 252),
                GridLine = Color.FromArgb(210, 228, 243),
                HintText = Color.FromArgb(91, 116, 139),
                StatusBack = Color.FromArgb(228, 239, 250),
                StatusText = Color.FromArgb(43, 76, 108),
                Snow = true
            });

            Add(new ThemePalette
            {
                Id = "graphite",
                Name = "Графит",
                HeaderBack = Color.FromArgb(38, 43, 49),
                HeaderMuted = Color.FromArgb(171, 181, 191),
                HeaderButton = Color.FromArgb(55, 62, 70),
                HeaderButtonHover = Color.FromArgb(72, 81, 91),
                Accent = Color.FromArgb(84, 124, 173),
                AccentHover = Color.FromArgb(102, 145, 197),
                PageBack = Color.FromArgb(246, 247, 249),
                BorderColor = Color.FromArgb(207, 212, 218),
                TextColor = Color.FromArgb(50, 57, 64),
                Green = Color.FromArgb(56, 126, 85),
                GreenHover = Color.FromArgb(68, 146, 100),
                Red = Color.FromArgb(176, 57, 57),
                RedHover = Color.FromArgb(203, 70, 70),
                PanelBack = Color.White,
                GridBack = Color.White,
                GridHeaderBack = Color.FromArgb(235, 238, 241),
                GridHeaderText = Color.FromArgb(58, 66, 74),
                GridAltBack = Color.FromArgb(248, 249, 250),
                GridSelectionBack = Color.FromArgb(216, 229, 243),
                GridLine = Color.FromArgb(222, 227, 232),
                HintText = Color.FromArgb(109, 117, 125),
                StatusBack = Color.FromArgb(234, 237, 240),
                StatusText = Color.FromArgb(66, 74, 82),
                Snow = false
            });

            Add(new ThemePalette
            {
                Id = "warm",
                Name = "Тёплая терракота",
                HeaderBack = Color.FromArgb(91, 40, 45),
                HeaderMuted = Color.FromArgb(231, 185, 179),
                HeaderButton = Color.FromArgb(119, 57, 62),
                HeaderButtonHover = Color.FromArgb(147, 73, 78),
                Accent = Color.FromArgb(184, 79, 66),
                AccentHover = Color.FromArgb(205, 98, 84),
                PageBack = Color.FromArgb(253, 247, 245),
                BorderColor = Color.FromArgb(232, 207, 202),
                TextColor = Color.FromArgb(82, 49, 49),
                Green = Color.FromArgb(72, 125, 77),
                GreenHover = Color.FromArgb(87, 145, 92),
                Red = Color.FromArgb(175, 45, 45),
                RedHover = Color.FromArgb(203, 60, 60),
                PanelBack = Color.White,
                GridBack = Color.White,
                GridHeaderBack = Color.FromArgb(249, 233, 229),
                GridHeaderText = Color.FromArgb(97, 56, 53),
                GridAltBack = Color.FromArgb(254, 246, 244),
                GridSelectionBack = Color.FromArgb(250, 219, 212),
                GridLine = Color.FromArgb(238, 216, 211),
                HintText = Color.FromArgb(133, 97, 93),
                StatusBack = Color.FromArgb(249, 233, 229),
                StatusText = Color.FromArgb(104, 61, 58),
                Snow = false
            });
        }

        private static void Add(ThemePalette palette)
        {
            Palettes[palette.Id] = palette;
        }

        public static string[] Ids
        {
            get { return new string[] { "auto", "classic", "spring", "summer", "autumn", "winter", "graphite", "warm" }; }
        }

        public static string DisplayName(string id)
        {
            if (string.Equals(id, "auto", StringComparison.OrdinalIgnoreCase))
            {
                return "Авто (по сезону)";
            }
            ThemePalette palette;
            if (id != null && Palettes.TryGetValue(id, out palette))
            {
                return palette.Name;
            }
            return "Авто (по сезону)";
        }

        public static string SeasonId(DateTime date)
        {
            int month = date.Month;
            if (month >= 3 && month <= 5)
            {
                return "spring";
            }
            if (month >= 6 && month <= 8)
            {
                return "summer";
            }
            if (month >= 9 && month <= 11)
            {
                return "autumn";
            }
            return "winter";
        }

        public static ThemePalette Resolve(string id, DateTime date)
        {
            string key = id;
            if (string.IsNullOrEmpty(key) || string.Equals(key, "auto", StringComparison.OrdinalIgnoreCase))
            {
                key = SeasonId(date);
            }

            ThemePalette palette;
            if (!Palettes.TryGetValue(key, out palette))
            {
                palette = Palettes["classic"];
            }
            return palette;
        }
    }

    public sealed class SnowTableLayoutPanel : TableLayoutPanel
    {
        private sealed class Flake
        {
            public float X;
            public float Y;
            public float Radius;
            public float Speed;
            public float Drift;
            public float Phase;
        }

        private readonly List<Flake> _flakes = new List<Flake>();
        private readonly Timer _timer;
        private bool _enabled;
        private Random _random = new Random(20261231);

        public SnowTableLayoutPanel()
        {
            DoubleBuffered = true;
            _timer = new Timer();
            _timer.Interval = 70;
            _timer.Tick += delegate
            {
                if (!_enabled || Width <= 0 || Height <= 0)
                {
                    return;
                }
                for (int i = 0; i < _flakes.Count; i++)
                {
                    Flake flake = _flakes[i];
                    flake.Phase += 0.04f;
                    flake.Y += flake.Speed;
                    flake.X += flake.Drift + (float)Math.Sin(flake.Phase) * 0.35f;
                    if (flake.Y > Height + 8)
                    {
                        flake.Y = -8;
                        flake.X = _random.Next(0, Math.Max(1, Width));
                    }
                    if (flake.X < -10)
                    {
                        flake.X = Width + 8;
                    }
                    if (flake.X > Width + 10)
                    {
                        flake.X = -8;
                    }
                }
                Invalidate();
            };
        }

        public bool SnowEnabled
        {
            get { return _enabled; }
            set
            {
                if (_enabled == value)
                {
                    return;
                }
                _enabled = value;
                if (_enabled)
                {
                    EnsureFlakes();
                    _timer.Start();
                }
                else
                {
                    _timer.Stop();
                }
                Invalidate();
            }
        }

        protected override void OnResize(EventArgs eventargs)
        {
            base.OnResize(eventargs);
            EnsureFlakes();
        }

        private void EnsureFlakes()
        {
            int desired = Math.Max(30, Width / 18);
            while (_flakes.Count < desired)
            {
                Flake flake = new Flake();
                flake.X = _random.Next(0, Math.Max(1, Width));
                flake.Y = _random.Next(0, Math.Max(1, Height));
                flake.Radius = 1.5f + (float)_random.NextDouble() * 3.5f;
                flake.Speed = 0.35f + (float)_random.NextDouble() * 1.0f;
                flake.Drift = -0.18f + (float)_random.NextDouble() * 0.36f;
                flake.Phase = (float)_random.NextDouble() * 6.28f;
                _flakes.Add(flake);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (!_enabled || _flakes.Count == 0)
            {
                return;
            }

            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            for (int i = 0; i < _flakes.Count; i++)
            {
                Flake flake = _flakes[i];
                int alpha = 130 + (int)(90 * (0.5 + 0.5 * Math.Sin(flake.Phase)));
                Color color = Color.FromArgb(alpha, 255, 255, 255);
                using (SolidBrush brush = new SolidBrush(color))
                {
                    e.Graphics.FillEllipse(brush,
                        flake.X - flake.Radius,
                        flake.Y - flake.Radius,
                        flake.Radius * 2f,
                        flake.Radius * 2f);
                }
                if (flake.Radius > 3.4f)
                {
                    using (Pen pen = new Pen(color, 1f))
                    {
                        float arm = flake.Radius * 1.6f;
                        e.Graphics.DrawLine(pen, flake.X - arm, flake.Y, flake.X + arm, flake.Y);
                        e.Graphics.DrawLine(pen, flake.X, flake.Y - arm, flake.X, flake.Y + arm);
                    }
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _timer.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
