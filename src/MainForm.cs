using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Media;
using System.Windows.Forms;

namespace SchoolBells
{
    public sealed partial class MainForm : Form
    {
        private readonly SettingsStore _store;
        private readonly AppSettings _settings;
        private readonly string _loadWarning;
        private readonly bool _startMinimized;
        private readonly string _initialTab;
        private readonly PlaybackEngine _playback;
        private readonly PlaybackEngine _musicPlayback;
        private readonly PlaybackEngine _voicePlayback;
        private readonly HashSet<string> _played;
        private readonly HashSet<string> _playedMusic;
        private readonly ToolTip _tips;

        private readonly Timer _timer;
        private readonly Timer _saveTimer;
        private readonly Timer _autoStopTimer;

        private Control _root;
        private SnowTableLayoutPanel _headerPanel;
        private ThemePalette _palette;
        private bool _snowActive;
        private DateTime _musicStopAt = DateTime.MinValue;
        private string _musicNowName = "";
        private MicrophoneRecorder _recorder;
        private bool _recording;
        private bool _rebuilding;

        private NotifyIcon _tray;
        private ContextMenuStrip _trayMenu;
        private ToolStripMenuItem _trayEnabledItem;
        private ToolStripMenuItem _trayStopItem;

        private Label _clockLabel;
        private Label _nextLabel;
        private Button _toggleButton;
        private Button _testButton;
        private Button _stopButton;
        private Panel _pageHost;
        private Panel _schedulePage;
        private Panel _settingsPage;
        private Panel _musicPage;
        private Panel _voicePage;
        private readonly System.Collections.Generic.List<RoundedButton> _tabButtons =
            new System.Collections.Generic.List<RoundedButton>();
        private readonly System.Collections.Generic.List<Panel> _pages =
            new System.Collections.Generic.List<Panel>();
        private Label _statusLabel;
        private int _selectedTabIndex;

        private bool _reallyExit;
        private bool _trayHintShown;
        private bool _updatingControls;
        private DateTime _lastDate;

        private static Color HeaderBack = Color.FromArgb(22, 50, 79);
        private static Color HeaderMuted = Color.FromArgb(157, 180, 204);
        private static Color HeaderButton = Color.FromArgb(43, 74, 111);
        private static Color HeaderButtonHover = Color.FromArgb(58, 92, 134);
        private static Color Accent = Color.FromArgb(45, 108, 180);
        private static Color AccentHover = Color.FromArgb(58, 124, 198);
        private static Color PageBack = Color.FromArgb(245, 248, 252);
        private static Color BorderColor = Color.FromArgb(201, 214, 227);
        private static Color TextColor = Color.FromArgb(45, 62, 80);
        private static Color Green = Color.FromArgb(46, 125, 50);
        private static Color GreenHover = Color.FromArgb(56, 142, 60);
        private static Color Red = Color.FromArgb(183, 28, 28);
        private static Color RedHover = Color.FromArgb(211, 47, 47);
        private static Color PanelBack = Color.White;
        private static Color GridBack = Color.White;
        private static Color GridHeaderBack = Color.FromArgb(234, 241, 248);
        private static Color GridHeaderText = Color.FromArgb(49, 70, 91);
        private static Color GridAltBack = Color.FromArgb(245, 249, 254);
        private static Color GridSelectionBack = Color.FromArgb(214, 233, 255);
        private static Color GridLine = Color.FromArgb(220, 229, 239);
        private static Color HintText = Color.FromArgb(105, 120, 136);
        private static Color StatusBack = Color.FromArgb(232, 238, 246);
        private static Color StatusText = Color.FromArgb(64, 86, 109);

        public MainForm(SettingsStore store, AppSettings settings, string loadWarning, bool startMinimized, string initialTab)
        {
            _store = store;
            _settings = settings;
            _loadWarning = loadWarning;
            _startMinimized = startMinimized;
            _initialTab = initialTab;
            _played = new HashSet<string>();
            _playedMusic = new HashSet<string>();
            _playback = new PlaybackEngine(settings.Volume);
            _musicPlayback = new PlaybackEngine(settings.MusicVolume);
            _voicePlayback = new PlaybackEngine(settings.VoiceVolume);
            _recorder = new MicrophoneRecorder();
            _palette = ThemeManager.Resolve(settings.Theme, DateTime.Now);
            ApplyPalette(_palette);
            _snowActive = settings.Snowflakes && _palette.Snow;
            _tips = new ToolTip();
            _tips.AutoPopDelay = 15000;
            _tips.InitialDelay = 400;
            _tips.ReshowDelay = 100;
            _tips.ShowAlways = true;

            _lastDate = DateTime.Today;

            _timer = new Timer();
            _timer.Interval = 1000;
            _timer.Tick += TimerTick;

            _saveTimer = new Timer();
            _saveTimer.Interval = 900;
            _saveTimer.Tick += delegate
            {
                _saveTimer.Stop();
                SaveNow(false);
            };

            _autoStopTimer = new Timer();
            _autoStopTimer.Interval = 30000;
            _autoStopTimer.Tick += delegate
            {
                _autoStopTimer.Stop();
                _playback.Stop();
            };

            SetFormProperties();
            BuildRoot();
            BuildTray();
            SyncAutostart();

            RefreshDayList();
            int initialDay = ChooseInitialDay();
            _dayList.SelectedIndex = initialDay;
            if (_currentDayIndex != initialDay)
            {
                LoadDay(initialDay);
            }

            UpdateSettingsControls();
            UpdateMusicControls();
            UpdateVoiceControls();
            UpdateHeader(DateTime.Now);
            UpdateToggleButton();
            UpdateTrayState();
            SelectTabByKey(_initialTab);
        }

        private void SelectTabByKey(string key)
        {
            if (_pages.Count == 0 || string.IsNullOrEmpty(key))
            {
                return;
            }

            string value = key.Trim().ToLowerInvariant();
            int index = 0;
            if (value == "settings" || value == "настройки")
            {
                index = 1;
            }
            else if (value == "music" || value == "музыка")
            {
                index = 2;
            }
            else if (value == "voice" || value == "голос" || value == "голосовое")
            {
                index = 3;
            }

            SelectTab(index);
        }

        private void SetFormProperties()
        {
            Text = "Школьные звонки";
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(1080, 680);
            MinimumSize = new Size(900, 600);
            Font = new Font("Segoe UI", 9.5f, FontStyle.Regular, GraphicsUnit.Point);
            BackColor = PageBack;
            AutoScaleDimensions = new SizeF(96f, 96f);
            AutoScaleMode = AutoScaleMode.Dpi;
            KeyPreview = true;
            DoubleBuffered = true;
            MaximizeBox = true;
            MinimizeBox = true;

            try
            {
                this.Icon = System.Drawing.Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            }
            catch
            {
            }
        }

        private void BuildRoot()
        {
            _tabButtons.Clear();
            _pages.Clear();

            TableLayoutPanel root = new TableLayoutPanel();
            _root = root;
            root.Dock = DockStyle.Fill;
            root.ColumnCount = 1;
            root.RowCount = 3;
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 94f));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 30f));
            root.BackColor = PageBack;
            Controls.Add(root);

            root.Controls.Add(BuildHeader(), 0, 0);

            TableLayoutPanel body = new TableLayoutPanel();
            body.Dock = DockStyle.Fill;
            body.ColumnCount = 1;
            body.RowCount = 2;
            body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            body.RowStyles.Add(new RowStyle(SizeType.Absolute, 46f));
            body.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            body.Padding = new Padding(14, 8, 14, 0);
            body.BackColor = PageBack;
            body.Controls.Add(BuildTabBar(), 0, 0);

            _pageHost = new Panel();
            _pageHost.Dock = DockStyle.Fill;
            _pageHost.BackColor = PageBack;
            _pageHost.Padding = new Padding(0, 8, 0, 8);
            body.Controls.Add(_pageHost, 0, 1);
            root.Controls.Add(body, 0, 1);

            _schedulePage = BuildScheduleTab();
            _settingsPage = BuildSettingsTab();
            _musicPage = BuildMusicTab();
            _voicePage = BuildVoiceTab();
            AddPage(_schedulePage);
            AddPage(_settingsPage);
            AddPage(_musicPage);
            AddPage(_voicePage);
            SelectTab(_selectedTabIndex);

            Panel status = new Panel();
            status.Dock = DockStyle.Fill;
            status.BackColor = StatusBack;
            status.Padding = new Padding(14, 0, 14, 0);
            status.Paint += delegate(object sender, PaintEventArgs e)
            {
                using (Pen pen = new Pen(BorderColor, 1f))
                {
                    e.Graphics.DrawLine(pen, 0, 0, status.Width, 0);
                }
            };

            TableLayoutPanel statusLayout = new TableLayoutPanel();
            statusLayout.Dock = DockStyle.Fill;
            statusLayout.ColumnCount = 2;
            statusLayout.RowCount = 1;
            statusLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            statusLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 56f));
            statusLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            statusLayout.BackColor = StatusBack;
            status.Controls.Add(statusLayout);

            _statusLabel = new Label();
            _statusLabel.Dock = DockStyle.Fill;
            _statusLabel.TextAlign = ContentAlignment.MiddleLeft;
            _statusLabel.Text = "Готово";
            _statusLabel.ForeColor = StatusText;
            _statusLabel.Font = new Font("Segoe UI", 9f, FontStyle.Regular, GraphicsUnit.Point);
            statusLayout.Controls.Add(_statusLabel, 0, 0);

            Label version = new Label();
            version.Text = "v1.2";
            version.Dock = DockStyle.Fill;
            version.TextAlign = ContentAlignment.MiddleRight;
            version.ForeColor = HintText;
            version.Font = new Font("Segoe UI", 9f, FontStyle.Regular, GraphicsUnit.Point);
            statusLayout.Controls.Add(version, 1, 0);

            root.Controls.Add(status, 0, 2);
        }

        private Panel BuildTabBar()
        {
            Panel bar = new Panel();
            bar.Dock = DockStyle.Fill;
            bar.BackColor = PageBack;
            bar.Margin = new Padding(0);

            FlowLayoutPanel flow = new FlowLayoutPanel();
            flow.Dock = DockStyle.Fill;
            flow.FlowDirection = FlowDirection.LeftToRight;
            flow.WrapContents = false;
            flow.BackColor = PageBack;
            flow.Padding = new Padding(0, 2, 0, 2);
            flow.Margin = new Padding(0);
            bar.Controls.Add(flow);

            string[] labels = new string[] { "Расписание", "Настройки", "Музыка", "Голосовое оповещение" };
            int[] widths = new int[] { 128, 118, 92, 198 };
            for (int i = 0; i < labels.Length; i++)
            {
                int tabIndex = i;
                RoundedButton button = new RoundedButton();
                button.Text = labels[i];
                button.Size = new Size(widths[i], 38);
                button.CornerRadius = 12;
                button.Margin = new Padding(0, 0, 8, 0);
                button.Font = new Font("Segoe UI Semibold", 10f, FontStyle.Regular, GraphicsUnit.Point);
                button.FlatAppearance.BorderSize = 0;
                button.Click += delegate { SelectTab(tabIndex); };
                flow.Controls.Add(button);
                _tabButtons.Add(button);
            }
            return bar;
        }

        private void AddPage(Panel page)
        {
            page.Dock = DockStyle.Fill;
            page.Visible = false;
            _pages.Add(page);
            _pageHost.Controls.Add(page);
        }

        private void SelectTab(int index)
        {
            if (index < 0 || index >= _pages.Count)
            {
                return;
            }
            _selectedTabIndex = index;
            for (int i = 0; i < _pages.Count; i++)
            {
                bool selected = i == index;
                _pages[i].Visible = selected;
                if (i < _tabButtons.Count)
                {
                    RoundedButton button = _tabButtons[i];
                    button.BackColor = selected ? Accent : PageBack;
                    button.ForeColor = selected ? Color.White : TextColor;
                    button.FlatAppearance.MouseOverBackColor = selected ? AccentHover : GridHeaderBack;
                    button.Invalidate();
                }
            }
        }

        private Panel BuildHeader()
        {
            Panel header = new Panel();
            header.Dock = DockStyle.Fill;
            header.BackColor = HeaderBack;
            header.Margin = new Padding(0);

            SnowTableLayoutPanel layout = new SnowTableLayoutPanel();
            _headerPanel = layout;
            layout.Dock = DockStyle.Fill;
            layout.ColumnCount = 3;
            layout.RowCount = 1;
            layout.Padding = new Padding(14, 6, 14, 6);
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 170f));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 330f));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            layout.BackColor = HeaderBack;
            layout.SnowEnabled = _snowActive;
            header.Controls.Add(layout);

            TableLayoutPanel left = new TableLayoutPanel();
            left.Dock = DockStyle.Fill;
            left.ColumnCount = 1;
            left.RowCount = 2;
            left.RowStyles.Add(new RowStyle(SizeType.Absolute, 22f));
            left.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            left.BackColor = HeaderBack;
            left.Margin = new Padding(0);

            Label title = new Label();
            title.Text = "ШКОЛЬНЫЕ ЗВОНКИ";
            title.Dock = DockStyle.Fill;
            title.TextAlign = ContentAlignment.MiddleLeft;
            title.ForeColor = HeaderMuted;
            title.Font = new Font("Segoe UI", 8.5f, FontStyle.Bold, GraphicsUnit.Point);
            left.Controls.Add(title, 0, 0);

            _clockLabel = new Label();
            _clockLabel.Text = "00:00:00";
            _clockLabel.Dock = DockStyle.Fill;
            _clockLabel.TextAlign = ContentAlignment.MiddleLeft;
            _clockLabel.ForeColor = Color.White;
            _clockLabel.Font = new Font("Segoe UI", 21f, FontStyle.Bold, GraphicsUnit.Point);
            left.Controls.Add(_clockLabel, 0, 1);
            layout.Controls.Add(left, 0, 0);

            TableLayoutPanel middle = new TableLayoutPanel();
            middle.Dock = DockStyle.Fill;
            middle.ColumnCount = 1;
            middle.RowCount = 2;
            middle.RowStyles.Add(new RowStyle(SizeType.Absolute, 22f));
            middle.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            middle.BackColor = HeaderBack;
            middle.Margin = new Padding(8, 0, 8, 0);

            Label nextCaption = new Label();
            nextCaption.Text = "СЛЕДУЮЩИЙ ЗВОНОК";
            nextCaption.Dock = DockStyle.Fill;
            nextCaption.TextAlign = ContentAlignment.MiddleLeft;
            nextCaption.ForeColor = HeaderMuted;
            nextCaption.Font = new Font("Segoe UI", 8.5f, FontStyle.Bold, GraphicsUnit.Point);
            middle.Controls.Add(nextCaption, 0, 0);

            _nextLabel = new Label();
            _nextLabel.Text = "—";
            _nextLabel.Dock = DockStyle.Fill;
            _nextLabel.TextAlign = ContentAlignment.MiddleLeft;
            _nextLabel.ForeColor = Color.White;
            _nextLabel.Font = new Font("Segoe UI", 11.5f, FontStyle.Bold, GraphicsUnit.Point);
            _nextLabel.AutoEllipsis = true;
            middle.Controls.Add(_nextLabel, 0, 1);
            layout.Controls.Add(middle, 1, 0);

            TableLayoutPanel right = new TableLayoutPanel();
            right.Dock = DockStyle.Fill;
            right.ColumnCount = 3;
            right.RowCount = 1;
            right.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 144f));
            right.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 124f));
            right.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 40f));
            right.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            right.BackColor = HeaderBack;
            right.Margin = new Padding(0);

            _toggleButton = new RoundedButton();
            _toggleButton.Text = "Звонки включены";
            _toggleButton.Dock = DockStyle.Fill;
            _toggleButton.Margin = new Padding(3, 14, 3, 14);
            _toggleButton.FlatStyle = FlatStyle.Flat;
            _toggleButton.FlatAppearance.BorderSize = 0;
            ((RoundedButton)_toggleButton).CornerRadius = 11;
            _toggleButton.BackColor = Green;
            _toggleButton.ForeColor = Color.White;
            _toggleButton.Font = new Font("Segoe UI", 9.25f, FontStyle.Bold, GraphicsUnit.Point);
            _toggleButton.Cursor = Cursors.Hand;
            _toggleButton.UseVisualStyleBackColor = false;
            _toggleButton.Click += delegate { SetBellsEnabled(!_settings.Enabled); };
            right.Controls.Add(_toggleButton, 0, 0);

            _testButton = new RoundedButton();
            _testButton.Text = "Пробный звонок";
            _testButton.Dock = DockStyle.Fill;
            _testButton.Margin = new Padding(3, 14, 3, 14);
            _testButton.FlatStyle = FlatStyle.Flat;
            _testButton.FlatAppearance.BorderSize = 0;
            ((RoundedButton)_testButton).CornerRadius = 11;
            _testButton.BackColor = HeaderButton;
            _testButton.ForeColor = Color.White;
            _testButton.Font = new Font("Segoe UI", 9.25f, FontStyle.Regular, GraphicsUnit.Point);
            _testButton.Cursor = Cursors.Hand;
            _testButton.UseVisualStyleBackColor = false;
            _testButton.FlatAppearance.MouseOverBackColor = HeaderButtonHover;
            _testButton.Click += delegate { PlayTestBell(); };
            right.Controls.Add(_testButton, 1, 0);

            _stopButton = new RoundedButton();
            _stopButton.Text = "■";
            _stopButton.Dock = DockStyle.Fill;
            _stopButton.Margin = new Padding(3, 14, 3, 14);
            _stopButton.FlatStyle = FlatStyle.Flat;
            _stopButton.FlatAppearance.BorderSize = 0;
            ((RoundedButton)_stopButton).CornerRadius = 11;
            _stopButton.BackColor = HeaderButton;
            _stopButton.ForeColor = Color.White;
            _stopButton.Font = new Font("Segoe UI", 13f, FontStyle.Bold, GraphicsUnit.Point);
            _stopButton.Cursor = Cursors.Hand;
            _stopButton.UseVisualStyleBackColor = false;
            _stopButton.FlatAppearance.MouseOverBackColor = HeaderButtonHover;
            _stopButton.Click += delegate { StopPlayback(); ShowStatus("Звук остановлен."); };
            right.Controls.Add(_stopButton, 2, 0);

            layout.Controls.Add(right, 2, 0);

            _tips.SetToolTip(_toggleButton, "Включить или выключить автоматические звонки");
            _tips.SetToolTip(_testButton, "Прослушать мелодию начала урока");
            _tips.SetToolTip(_stopButton, "Остановить звук");
            return header;
        }

        private void BuildTray()
        {
            _trayMenu = new ContextMenuStrip();
            _trayMenu.Font = new Font("Segoe UI", 9.5f, FontStyle.Regular, GraphicsUnit.Point);

            ToolStripMenuItem open = new ToolStripMenuItem("Открыть");
            open.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold, GraphicsUnit.Point);
            open.Click += delegate { ShowFromTray(); };
            _trayMenu.Items.Add(open);

            _trayMenu.Items.Add(new ToolStripSeparator());

            _trayEnabledItem = new ToolStripMenuItem("Звонки включены");
            _trayEnabledItem.CheckOnClick = false;
            _trayEnabledItem.Click += delegate { SetBellsEnabled(!_settings.Enabled); };
            _trayMenu.Items.Add(_trayEnabledItem);

            ToolStripMenuItem test = new ToolStripMenuItem("Пробный звонок");
            test.Click += delegate { PlayTestBell(); };
            _trayMenu.Items.Add(test);

            _trayStopItem = new ToolStripMenuItem("Остановить звук");
            _trayStopItem.Click += delegate { StopPlayback(); };
            _trayMenu.Items.Add(_trayStopItem);

            _trayMenu.Items.Add(new ToolStripSeparator());

            ToolStripMenuItem exit = new ToolStripMenuItem("Выход");
            exit.Click += delegate
            {
                _reallyExit = true;
                Close();
            };
            _trayMenu.Items.Add(exit);

            _trayMenu.Opening += delegate
            {
                _trayEnabledItem.Checked = _settings.Enabled;
                _trayStopItem.Enabled = true;
            };

            _tray = new NotifyIcon();
            _tray.Icon = Icon != null ? Icon : SystemIcons.Application;
            _tray.Text = "Школьные звонки";
            _tray.ContextMenuStrip = _trayMenu;
            _tray.Visible = true;
            _tray.DoubleClick += delegate { ShowFromTray(); };
            _tray.BalloonTipClicked += delegate { ShowFromTray(); };
        }

        private void SyncAutostart()
        {
            bool registryEnabled = StartupManager.IsEnabled();
            bool changed = false;
            if (_settings.AutoStart != registryEnabled)
            {
                _settings.AutoStart = registryEnabled;
                changed = true;
            }

            if (_settings.AutoStart)
            {
                string error;
                if (!StartupManager.SetEnabled(true, Application.ExecutablePath, out error))
                {
                    Log.Error("Не удалось обновить автозапуск: " + error, null);
                }
            }

            if (changed)
            {
                SaveSoon();
            }
        }

        private int ChooseInitialDay()
        {
            int today = ScheduleEngine.DayIndex(DateTime.Now);
            DaySchedule todaySchedule = _settings.GetEffectiveDay(today);
            if (todaySchedule != null && todaySchedule.Bells.Count > 0)
            {
                return today;
            }
            for (int day = 0; day < 7; day++)
            {
                DaySchedule schedule = _settings.GetEffectiveDay(day);
                if (schedule != null && schedule.Bells.Count > 0)
                {
                    return day;
                }
            }
            return today;
        }

        private void TimerTick(object sender, EventArgs e)
        {
            DateTime now = DateTime.Now;
            if (now.Date != _lastDate)
            {
                _lastDate = now.Date;
                _played.Clear();
                _playedMusic.Clear();
                StopMusic();
                UpdateTodayLabel();
                UpdateDaySummary();
                if (string.Equals(_settings.Theme, "auto", StringComparison.OrdinalIgnoreCase) &&
                    _palette != null && _palette.Id != ThemeManager.SeasonId(now))
                {
                    BeginInvoke(new MethodInvoker(delegate { RebuildInterface(); }));
                }
            }

            CheckBells(now);
            CheckMusic(now);
            UpdateHeader(now);
        }

        private void CheckBells(DateTime now)
        {
            if (!_settings.Enabled)
            {
                return;
            }

            int dayIndex = ScheduleEngine.DayIndex(now);
            DaySchedule day = ScheduleEngine.ResolveDay(_settings, dayIndex);
            List<BellEvent> events = ScheduleEngine.BuildEvents(day, _settings.RingAtStart, _settings.RingAtEnd);
            List<BellEvent> due = ScheduleEngine.FindDueEvents(events, now.TimeOfDay, 5);
            if (due.Count == 0)
            {
                return;
            }

            BellEvent chosen = null;
            for (int i = 0; i < due.Count; i++)
            {
                string key = ScheduleEngine.MakePlayedKey(now, due[i]);
                if (!_played.Contains(key))
                {
                    chosen = due[i];
                    break;
                }
            }
            if (chosen == null)
            {
                return;
            }

            for (int i = 0; i < due.Count; i++)
            {
                _played.Add(ScheduleEngine.MakePlayedKey(now, due[i]));
            }

            PlayBellEvent(chosen, now);
        }

        private void PlayBellEvent(BellEvent item, DateTime now)
        {
            string melodyFile = item.IsStart ? _settings.MelodyStartFile : _settings.MelodyEndFile;
            if (string.IsNullOrEmpty(melodyFile))
            {
                melodyFile = _settings.MelodyStartFile;
            }
            if (string.IsNullOrEmpty(melodyFile))
            {
                melodyFile = "default_bell.wav";
            }

            string path = _store.ResolveMelodyPath(melodyFile);
            StopMusic();
            string error;
            bool ok = PlaySound(path, false, out error);
            string description = ScheduleEngine.EventDescription(item);
            Log.Bell(now, description, Path.GetFileName(path), ok, error);

            if (ok)
            {
                ShowStatus("Прозвенел звонок: " + description);
            }
            else
            {
                ShowStatus("Не удалось воспроизвести звонок. " + error);
                try
                {
                    SystemSounds.Exclamation.Play();
                }
                catch
                {
                }
            }
        }

        private void PlayTestBell()
        {
            string melodyFile = _settings.MelodyStartFile;
            if (string.IsNullOrEmpty(melodyFile))
            {
                melodyFile = "default_bell.wav";
            }

            string path = _store.ResolveMelodyPath(melodyFile);
            string error;
            bool ok = PlaySound(path, true, out error);
            if (ok)
            {
                ShowStatus("Пробный звонок: " + Path.GetFileName(path));
            }
            else
            {
                ShowStatus("Не удалось воспроизвести мелодию.");
                MessageBox.Show(this,
                    "Не удалось воспроизвести мелодию.\n\n" + error +
                    "\n\nПроверьте, что файл не повреждён и громкость Windows включена.",
                    "Школьные звонки",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }

        private bool PlaySound(string path, bool showError, out string error)
        {
            error = null;
            StopLiveMicrophone();
            if (_recording)
            {
                CancelVoiceRecording();
            }
            if (_musicPlayback != null)
            {
                _musicPlayback.Stop();
            }
            if (_voicePlayback != null)
            {
                _voicePlayback.Stop();
            }
            _musicStopAt = DateTime.MinValue;
            _musicNowName = "";
            _playback.Volume = _settings.Volume;
            string fallback = Path.Combine(_store.DataDir, "default_bell.wav");
            bool ok = _playback.Play(path, fallback, out error);
            if (ok)
            {
                _autoStopTimer.Stop();
                _autoStopTimer.Start();
            }
            else if (showError)
            {
                Log.Error("Не удалось воспроизвести звук: " + path + " | " + error, null);
            }
            return ok;
        }

        private void StopPlayback()
        {
            StopLiveMicrophone();
            _autoStopTimer.Stop();
            _playback.Stop();
            if (_musicPlayback != null)
            {
                _musicPlayback.Stop();
            }
            if (_voicePlayback != null)
            {
                _voicePlayback.Stop();
            }
            _musicStopAt = DateTime.MinValue;
            _musicNowName = "";
        }

        private void SetBellsEnabled(bool enabled)
        {
            if (_settings.Enabled == enabled)
            {
                return;
            }

            _settings.Enabled = enabled;
            UpdateToggleButton();
            UpdateHeader(DateTime.Now);
            UpdateTrayState();
            ShowStatus(enabled ? "Звонки включены." : "Звонки выключены.");
            SaveSoon();
        }

        private void UpdateToggleButton()
        {
            if (_toggleButton == null)
            {
                return;
            }

            _toggleButton.Text = _settings.Enabled ? "Звонки включены" : "Звонки выключены";
            _toggleButton.BackColor = _settings.Enabled ? Green : Red;
            _toggleButton.FlatAppearance.MouseOverBackColor = _settings.Enabled ? GreenHover : RedHover;
        }

        private void UpdateHeader(DateTime now)
        {
            if (_clockLabel != null)
            {
                string clock = now.ToString("HH:mm:ss");
                if (_clockLabel.Text != clock)
                {
                    _clockLabel.Text = clock;
                }
            }

            if (_nextLabel == null)
            {
                return;
            }

            string text;
            Color color;
            if (!_settings.Enabled)
            {
                text = "Звонки выключены";
                color = Color.FromArgb(255, 180, 169);
            }
            else
            {
                DaySchedule day = ScheduleEngine.ResolveDay(_settings, ScheduleEngine.DayIndex(now));
                List<BellEvent> events = ScheduleEngine.BuildEvents(day, _settings.RingAtStart, _settings.RingAtEnd);
                if (day == null || day.Bells.Count == 0)
                {
                    text = "Сегодня звонков нет";
                    color = Color.FromArgb(199, 214, 229);
                }
                else if (events.Count == 0)
                {
                    text = "Звонки в начале и конце отключены";
                    color = Color.FromArgb(199, 214, 229);
                }
                else
                {
                    BellEvent next = ScheduleEngine.GetNextEvent(events, now.TimeOfDay);
                    if (next == null)
                    {
                        text = "На сегодня звонков больше нет";
                        color = Color.FromArgb(199, 214, 229);
                    }
                    else
                    {
                        text = ScheduleEngine.EventDescription(next) + " · " +
                               TimeUtil.FormatCountdown(next.Time - now.TimeOfDay);
                        color = Color.White;
                    }
                }
            }

            if (_nextLabel.Text != text)
            {
                _nextLabel.Text = text;
            }
            _nextLabel.ForeColor = color;
            UpdateTrayState();
        }

        private void UpdateTrayState()
        {
            if (_tray == null)
            {
                return;
            }

            _trayEnabledItem.Checked = _settings.Enabled;
            string text;
            if (!_settings.Enabled)
            {
                text = "Школьные звонки — выключены";
            }
            else
            {
                DateTime now = DateTime.Now;
                DaySchedule day = ScheduleEngine.ResolveDay(_settings, ScheduleEngine.DayIndex(now));
                List<BellEvent> events = ScheduleEngine.BuildEvents(day, _settings.RingAtStart, _settings.RingAtEnd);
                BellEvent next = ScheduleEngine.GetNextEvent(events, now.TimeOfDay);
                text = next == null ? "Школьные звонки" : "Школьные звонки — " + TimeUtil.FormatTime(next.Time);
            }

            if (text.Length > 63)
            {
                text = text.Substring(0, 63);
            }
            if (_tray.Text != text)
            {
                _tray.Text = text;
            }
        }

        private void ShowStatus(string text)
        {
            if (_statusLabel != null)
            {
                _statusLabel.Text = text;
            }
        }

        private void SaveSoon()
        {
            if (_saveTimer == null)
            {
                return;
            }
            _saveTimer.Stop();
            _saveTimer.Start();
        }

        private bool SaveNow(bool showStatus)
        {
            bool ok = _store.Save(_settings);
            if (!ok)
            {
                ShowStatus("Не удалось сохранить настройки. Проверьте папку: " + _store.DataDir);
            }
            else if (showStatus)
            {
                ShowStatus("Настройки сохранены.");
            }
            return ok;
        }

        private void ShowFromTray()
        {
            ShowInTaskbar = true;
            if (!Visible)
            {
                Show();
            }
            WindowState = FormWindowState.Normal;
            Activate();
            BringToFront();
        }

        private void HideToTray(bool showBalloon)
        {
            StopLiveMicrophone();
            if (_recording)
            {
                CancelVoiceRecording();
            }
            WindowState = FormWindowState.Normal;
            ShowInTaskbar = false;
            Hide();
            if (showBalloon && !_trayHintShown)
            {
                _trayHintShown = true;
                try
                {
                    _tray.ShowBalloonTip(4000, "Школьные звонки",
                        "Программа свёрнута в трей и продолжает звонить по расписанию.",
                        ToolTipIcon.Info);
                }
                catch
                {
                }
            }
        }

        private void ShowWelcome()
        {
            _settings.FirstRun = false;
            SaveSoon();
            MessageBox.Show(this,
                "Добро пожаловать в «Школьные звонки»!\n\n" +
                "1. Выберите день недели в списке слева.\n" +
                "2. Нажмите «Добавить» и укажите начало и конец урока. Время можно вводить как 08:00, 0800 или 800 — программа приведёт его к виду ЧЧ:ММ.\n" +
                "3. Заполните остальные дни. Кнопка «Копировать день…» переносит расписание сразу в несколько дней.\n" +
                "4. Для коротких дней отметьте нужные дни в блоке «Короткий день» и заполните шаблон «Короткий день» в списке дней.\n" +
                "5. Во вкладке «Настройки» выберите мелодии, тему оформления и громкость.\n" +
                "6. Во вкладках «Музыка» и «Голосовое оповещение» можно включать музыку по расписанию и записывать объявления с микрофона.\n\n" +
                "Программа работает в фоне: при закрытии окна она сворачивается в трей и продолжает звонить.",
                "Школьные звонки",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            if (_addButton != null && _addButton.Visible)
            {
                _addButton.Focus();
            }
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            if (_palette != null)
            {
                WindowChrome.Apply(Handle, _palette.HeaderBack);
            }
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            if (_palette != null)
            {
                WindowChrome.Apply(Handle, _palette.HeaderBack);
            }
            _timer.Start();

            BeginInvoke(new MethodInvoker(delegate
            {
                if (_palette != null)
                {
                    WindowChrome.Apply(Handle, _palette.HeaderBack);
                }
                _playback.Prewarm();

                if (!string.IsNullOrEmpty(_loadWarning) && !_startMinimized)
                {
                    MessageBox.Show(this, _loadWarning, "Школьные звонки",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

                if (_settings.FirstRun)
                {
                    ShowWelcome();
                }
                else if (_startMinimized)
                {
                    HideToTray(false);
                }
            }));
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            bool shuttingDown = e.CloseReason == CloseReason.WindowsShutDown;
            if (!shuttingDown && !ValidateGridForClose())
            {
                e.Cancel = true;
                ShowFromTray();
                MessageBox.Show(this,
                    "В расписании есть ошибка. Исправьте время в выделенной строке и попробуйте снова.",
                    "Школьные звонки",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                ShowGridError();
                return;
            }

            if (e.CloseReason == CloseReason.UserClosing && _settings.MinimizeToTray && !_reallyExit)
            {
                e.Cancel = true;
                HideToTray(true);
                return;
            }

            StopLiveMicrophone();
            if (_recording)
            {
                CancelVoiceRecording();
            }
            SaveNow(false);
            StopPlayback();
            StopMusic();
            _timer.Stop();
            _saveTimer.Stop();
            _autoStopTimer.Stop();
            if (_tray != null)
            {
                _tray.Visible = false;
            }
            base.OnFormClosing(e);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_timer != null)
                {
                    _timer.Dispose();
                }
                if (_saveTimer != null)
                {
                    _saveTimer.Dispose();
                }
                if (_autoStopTimer != null)
                {
                    _autoStopTimer.Dispose();
                }
                if (_tray != null)
                {
                    _tray.Visible = false;
                    _tray.Dispose();
                }
                if (_trayMenu != null)
                {
                    _trayMenu.Dispose();
                }
                if (_tips != null)
                {
                    _tips.Dispose();
                }
                if (_playback != null)
                {
                    _playback.Dispose();
                }
                if (_musicPlayback != null)
                {
                    _musicPlayback.Dispose();
                }
                if (_voicePlayback != null)
                {
                    _voicePlayback.Dispose();
                }
                if (_recorder != null)
                {
                    _recorder.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        private void ApplyPalette(ThemePalette palette)
        {
            if (palette == null)
            {
                return;
            }
            _palette = palette;
            UiTheme.Apply(palette);
            HeaderBack = palette.HeaderBack;
            if (IsHandleCreated)
            {
                WindowChrome.Apply(Handle, palette.HeaderBack);
            }
            HeaderMuted = palette.HeaderMuted;
            HeaderButton = palette.HeaderButton;
            HeaderButtonHover = palette.HeaderButtonHover;
            Accent = palette.Accent;
            AccentHover = palette.AccentHover;
            PageBack = palette.PageBack;
            BorderColor = palette.BorderColor;
            TextColor = palette.TextColor;
            Green = palette.Green;
            GreenHover = palette.GreenHover;
            Red = palette.Red;
            RedHover = palette.RedHover;
            PanelBack = palette.PanelBack;
            GridBack = palette.GridBack;
            GridHeaderBack = palette.GridHeaderBack;
            GridHeaderText = palette.GridHeaderText;
            GridAltBack = palette.GridAltBack;
            GridSelectionBack = palette.GridSelectionBack;
            GridLine = palette.GridLine;
            HintText = palette.HintText;
            StatusBack = palette.StatusBack;
            StatusText = palette.StatusText;
        }

        private void RebuildInterface()
        {
            if (_rebuilding)
            {
                return;
            }
            _rebuilding = true;
            try
            {
                int selectedTab = _selectedTabIndex;
                StopLiveMicrophone();
                if (_recording)
                {
                    CancelVoiceRecording();
                }
                StopMusic();

                ApplyPalette(ThemeManager.Resolve(_settings.Theme, DateTime.Now));
                _snowActive = _settings.Snowflakes && _palette.Snow;

                SuspendLayout();
                if (_root != null)
                {
                    Controls.Remove(_root);
                    _root.Dispose();
                    _root = null;
                }
                BuildRoot();
                ResumeLayout(true);

                RefreshDayList();
                if (_currentDayIndex < 0 || _currentDayIndex > 7)
                {
                    _currentDayIndex = ChooseInitialDay();
                }
                if (_dayList != null && _currentDayIndex >= 0 && _currentDayIndex < _dayList.Items.Count)
                {
                    _dayList.SelectedIndex = _currentDayIndex;
                }
                LoadDay(_currentDayIndex);
                UpdateSettingsControls();
                UpdateMusicControls();
                UpdateVoiceControls();
                UpdateHeader(DateTime.Now);
                UpdateToggleButton();
                UpdateTrayState();
                SelectTab(selectedTab);
            }
            finally
            {
                _rebuilding = false;
            }
        }

        private void ApplyModernGridStyle(DataGridView grid)
        {
            grid.BackgroundColor = PanelBack;
            grid.BorderStyle = BorderStyle.None;
            grid.EnableHeadersVisualStyles = false;
            grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            grid.GridColor = GridLine;
            grid.RowTemplate.Height = 34;
            grid.ColumnHeadersHeight = 40;
            grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            grid.Font = new Font("Segoe UI", 10f, FontStyle.Regular, GraphicsUnit.Point);
            grid.DefaultCellStyle.Font = new Font("Segoe UI", 10f, FontStyle.Regular, GraphicsUnit.Point);
            grid.DefaultCellStyle.BackColor = PanelBack;
            grid.DefaultCellStyle.ForeColor = TextColor;
            grid.DefaultCellStyle.SelectionBackColor = GridSelectionBack;
            grid.DefaultCellStyle.SelectionForeColor = TextColor;
            grid.DefaultCellStyle.Padding = new Padding(6, 0, 6, 0);
            grid.AlternatingRowsDefaultCellStyle.BackColor = GridAltBack;
            grid.ColumnHeadersDefaultCellStyle.BackColor = PanelBack;
            grid.ColumnHeadersDefaultCellStyle.ForeColor = HintText;
            grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = PanelBack;
            grid.ColumnHeadersDefaultCellStyle.SelectionForeColor = HintText;
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 9f, FontStyle.Regular, GraphicsUnit.Point);
            grid.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            grid.RowHeadersVisible = false;
        }

        private Button MakeButton(string text, int width, int height, bool accent)
        {
            RoundedButton button = new RoundedButton();
            button.Text = text;
            button.Size = new Size(width, height);
            button.CornerRadius = 10;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = accent ? 0 : 1;
            button.FlatAppearance.BorderColor = BorderColor;
            button.BackColor = accent ? Accent : PanelBack;
            button.ForeColor = accent ? Color.White : TextColor;
            button.Font = new Font(accent ? "Segoe UI Semibold" : "Segoe UI", 9.25f,
                FontStyle.Regular, GraphicsUnit.Point);
            button.Cursor = Cursors.Hand;
            button.UseVisualStyleBackColor = false;
            button.Margin = new Padding(0, 0, 6, 0);
            button.FlatAppearance.MouseOverBackColor = accent ? AccentHover : GridHeaderBack;
            return button;
        }

        private Label MakeHintLabel(string text)
        {
            Label label = new Label();
            label.Text = text;
            label.AutoSize = false;
            label.ForeColor = HintText;
            label.Font = new Font("Segoe UI", 8.5f, FontStyle.Regular, GraphicsUnit.Point);
            return label;
        }
    }
}
