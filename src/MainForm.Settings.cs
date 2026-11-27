using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace SchoolBells
{
    public sealed partial class MainForm
    {
        private TextBox _startMelodyBox;
        private TextBox _endMelodyBox;
        private TextBox _dataDirBox;
        private Button _startBrowseButton;
        private Button _startPlayButton;
        private Button _startResetButton;
        private Button _endBrowseButton;
        private Button _endPlayButton;
        private Button _endResetButton;
        private Button _openDataButton;
        private Button _openReadmeButton;
        private ModernSlider _volumeTrack;
        private Label _volumeValueLabel;
        private ModernCheckBox _ringStartCheck;
        private ModernCheckBox _ringEndCheck;
        private ModernCheckBox _autoStartCheck;
        private ModernCheckBox _minimizeCheck;
        private ComboBox _themeCombo;
        private ModernCheckBox _snowCheck;

        private Panel BuildSettingsTab()
        {
            Panel page = new Panel();
            page.Dock = DockStyle.Fill;
            page.BackColor = PageBack;
            page.Padding = new Padding(2, 0, 2, 0);
            page.AutoScroll = true;

            TableLayoutPanel content = new TableLayoutPanel();
            content.Dock = DockStyle.Top;
            content.AutoSize = true;
            content.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            content.ColumnCount = 1;
            content.RowCount = 5;
            content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            for (int row = 0; row < 5; row++)
            {
                content.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            }
            content.Padding = new Padding(10, 4, 10, 10);
            content.BackColor = PageBack;
            page.Controls.Add(content);

            GroupBox melodies = BuildMelodyGroup();
            melodies.Dock = DockStyle.Top;
            melodies.Margin = new Padding(0, 0, 0, 10);
            content.Controls.Add(melodies, 0, 0);

            GroupBox appearance = BuildAppearanceGroup();
            appearance.Dock = DockStyle.Top;
            appearance.Margin = new Padding(0, 0, 0, 10);
            content.Controls.Add(appearance, 0, 1);

            GroupBox behavior = BuildBehaviorGroup();
            behavior.Dock = DockStyle.Top;
            behavior.Margin = new Padding(0, 0, 0, 10);
            content.Controls.Add(behavior, 0, 2);

            GroupBox data = BuildDataGroup();
            data.Dock = DockStyle.Top;
            data.Margin = new Padding(0, 0, 0, 10);
            content.Controls.Add(data, 0, 3);

            GroupBox about = BuildAboutGroup();
            about.Dock = DockStyle.Top;
            about.Margin = new Padding(0);
            content.Controls.Add(about, 0, 4);

            return page;
        }

        private GroupBox BuildMelodyGroup()
        {
            CardPanel group = new CardPanel();
            group.Text = "Мелодии звонков";
            group.Font = new Font("Segoe UI", 10f, FontStyle.Bold, GraphicsUnit.Point);
            group.ForeColor = TextColor;
            group.BackColor = PanelBack;
            group.Padding = new Padding(12, 24, 12, 12);
            group.AutoSize = true;
            group.AutoSizeMode = AutoSizeMode.GrowAndShrink;

            TableLayoutPanel table = new TableLayoutPanel();
            table.Dock = DockStyle.Top;
            table.AutoSize = true;
            table.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            table.ColumnCount = 3;
            table.RowCount = 4;
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 112f));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 252f));
            table.RowStyles.Add(new RowStyle(SizeType.Absolute, 36f));
            table.RowStyles.Add(new RowStyle(SizeType.Absolute, 36f));
            table.RowStyles.Add(new RowStyle(SizeType.Absolute, 42f));
            table.RowStyles.Add(new RowStyle(SizeType.Absolute, 44f));
            table.BackColor = PanelBack;
            group.Controls.Add(table);

            Label startLabel = new Label();
            startLabel.Text = "Начало урока:";
            startLabel.Dock = DockStyle.Fill;
            startLabel.TextAlign = ContentAlignment.MiddleLeft;
            startLabel.Font = new Font("Segoe UI", 9.5f, FontStyle.Regular, GraphicsUnit.Point);
            table.Controls.Add(startLabel, 0, 0);

            _startMelodyBox = new TextBox();
            _startMelodyBox.ReadOnly = true;
            _startMelodyBox.Dock = DockStyle.Fill;
            _startMelodyBox.BorderStyle = BorderStyle.FixedSingle;
            _startMelodyBox.BackColor = Color.White;
            _startMelodyBox.ForeColor = TextColor;
            _startMelodyBox.Font = new Font("Segoe UI", 9.5f, FontStyle.Regular, GraphicsUnit.Point);
            _startMelodyBox.Margin = new Padding(0, 6, 8, 6);
            _startMelodyBox.TabStop = false;
            table.Controls.Add(_startMelodyBox, 1, 0);

            FlowLayoutPanel startButtons = MakeButtonRow();
            table.Controls.Add(startButtons, 2, 0);

            _startBrowseButton = MakeButton("Выбрать MP3…", 120, 30, false);
            _startBrowseButton.Click += delegate { BrowseMelody(true); };
            startButtons.Controls.Add(_startBrowseButton);

            _startPlayButton = MakeButton("▶", 38, 30, false);
            _startPlayButton.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold, GraphicsUnit.Point);
            _startPlayButton.Click += delegate { PreviewMelody(true); };
            startButtons.Controls.Add(_startPlayButton);

            _startResetButton = MakeButton("Сброс", 76, 30, false);
            _startResetButton.Click += delegate { ResetMelody(true); };
            startButtons.Controls.Add(_startResetButton);

            Label endLabel = new Label();
            endLabel.Text = "Конец урока:";
            endLabel.Dock = DockStyle.Fill;
            endLabel.TextAlign = ContentAlignment.MiddleLeft;
            endLabel.Font = new Font("Segoe UI", 9.5f, FontStyle.Regular, GraphicsUnit.Point);
            table.Controls.Add(endLabel, 0, 1);

            _endMelodyBox = new TextBox();
            _endMelodyBox.ReadOnly = true;
            _endMelodyBox.Dock = DockStyle.Fill;
            _endMelodyBox.BorderStyle = BorderStyle.FixedSingle;
            _endMelodyBox.BackColor = Color.White;
            _endMelodyBox.ForeColor = TextColor;
            _endMelodyBox.Font = new Font("Segoe UI", 9.5f, FontStyle.Regular, GraphicsUnit.Point);
            _endMelodyBox.Margin = new Padding(0, 6, 8, 6);
            _endMelodyBox.TabStop = false;
            table.Controls.Add(_endMelodyBox, 1, 1);

            FlowLayoutPanel endButtons = MakeButtonRow();
            table.Controls.Add(endButtons, 2, 1);

            _endBrowseButton = MakeButton("Выбрать…", 88, 30, false);
            _endBrowseButton.Click += delegate { BrowseMelody(false); };
            endButtons.Controls.Add(_endBrowseButton);

            _endPlayButton = MakeButton("▶", 38, 30, false);
            _endPlayButton.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold, GraphicsUnit.Point);
            _endPlayButton.Click += delegate { PreviewMelody(false); };
            endButtons.Controls.Add(_endPlayButton);

            _endResetButton = MakeButton("Сброс", 76, 30, false);
            _endResetButton.Click += delegate { ResetMelody(false); };
            endButtons.Controls.Add(_endResetButton);

            Label hint = MakeHintLabel(
                "Если мелодия конца не выбрана, звучит мелодия начала. " +
                "Выбранный файл копируется в папку программы, поэтому оригинал можно перемещать и удалять.");
            hint.Dock = DockStyle.Fill;
            hint.TextAlign = ContentAlignment.TopLeft;
            hint.Padding = new Padding(0, 4, 0, 0);
            table.Controls.Add(hint, 0, 2);
            table.SetColumnSpan(hint, 3);

            Label volumeLabel = new Label();
            volumeLabel.Text = "Громкость:";
            volumeLabel.Dock = DockStyle.Fill;
            volumeLabel.TextAlign = ContentAlignment.MiddleLeft;
            volumeLabel.Font = new Font("Segoe UI", 9.5f, FontStyle.Regular, GraphicsUnit.Point);
            table.Controls.Add(volumeLabel, 0, 3);

            FlowLayoutPanel volumePanel = new FlowLayoutPanel();
            volumePanel.Dock = DockStyle.Fill;
            volumePanel.FlowDirection = FlowDirection.LeftToRight;
            volumePanel.WrapContents = false;
            volumePanel.Margin = new Padding(0);
            table.Controls.Add(volumePanel, 1, 3);
            table.SetColumnSpan(volumePanel, 2);

            _volumeTrack = new ModernSlider();
            _volumeTrack.Minimum = 0;
            _volumeTrack.Maximum = 100;
            _volumeTrack.TickFrequency = 10;
            _volumeTrack.SmallChange = 5;
            _volumeTrack.LargeChange = 10;
            _volumeTrack.TickStyle = TickStyle.None;
            _volumeTrack.Width = 320;
            _volumeTrack.Height = 40;
            _volumeTrack.Margin = new Padding(0, 2, 8, 0);
            _volumeTrack.ValueChanged += VolumeChanged;
            volumePanel.Controls.Add(_volumeTrack);

            _volumeValueLabel = new Label();
            _volumeValueLabel.Text = "80 %";
            _volumeValueLabel.Width = 70;
            _volumeValueLabel.Height = 40;
            _volumeValueLabel.TextAlign = ContentAlignment.MiddleLeft;
            _volumeValueLabel.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold, GraphicsUnit.Point);
            _volumeValueLabel.ForeColor = TextColor;
            volumePanel.Controls.Add(_volumeValueLabel);

            _tips.SetToolTip(_startMelodyBox, "Мелодия, которая звучит в начале урока");
            _tips.SetToolTip(_endMelodyBox, "Мелодия, которая звучит в конце урока");
            _tips.SetToolTip(_startBrowseButton, "Выбрать MP3 или WAV для начала урока");
            _tips.SetToolTip(_endBrowseButton, "Выбрать MP3 или WAV для конца урока");
            _tips.SetToolTip(_startPlayButton, "Прослушать мелодию начала урока");
            _tips.SetToolTip(_endPlayButton, "Прослушать мелодию конца урока");
            _tips.SetToolTip(_startResetButton, "Вернуть встроенный звонок");
            _tips.SetToolTip(_endResetButton, "Использовать мелодию начала урока");
            _tips.SetToolTip(_volumeTrack, "Громкость звонка. Общая громкость Windows тоже влияет.");
            return group;
        }

        private GroupBox BuildAppearanceGroup()
        {
            CardPanel group = new CardPanel();
            group.Text = "Оформление";
            group.Font = new Font("Segoe UI", 10f, FontStyle.Bold, GraphicsUnit.Point);
            group.ForeColor = TextColor;
            group.BackColor = PanelBack;
            group.Padding = new Padding(12, 24, 12, 12);
            group.AutoSize = true;
            group.AutoSizeMode = AutoSizeMode.GrowAndShrink;

            TableLayoutPanel table = new TableLayoutPanel();
            table.Dock = DockStyle.Top;
            table.AutoSize = true;
            table.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            table.ColumnCount = 2;
            table.RowCount = 3;
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 112f));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            table.RowStyles.Add(new RowStyle(SizeType.Absolute, 38f));
            table.RowStyles.Add(new RowStyle(SizeType.Absolute, 30f));
            table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            table.BackColor = PanelBack;
            group.Controls.Add(table);

            Label themeLabel = new Label();
            themeLabel.Text = "Тема:";
            themeLabel.Dock = DockStyle.Fill;
            themeLabel.TextAlign = ContentAlignment.MiddleLeft;
            themeLabel.Font = new Font("Segoe UI", 9.5f, FontStyle.Regular, GraphicsUnit.Point);
            table.Controls.Add(themeLabel, 0, 0);

            _themeCombo = new ComboBox();
            _themeCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            _themeCombo.Width = 310;
            _themeCombo.Anchor = AnchorStyles.Left;
            _themeCombo.Margin = new Padding(0, 5, 8, 5);
            _themeCombo.Font = new Font("Segoe UI", 9.5f, FontStyle.Regular, GraphicsUnit.Point);
            string[] ids = ThemeManager.Ids;
            for (int i = 0; i < ids.Length; i++)
            {
                _themeCombo.Items.Add(ThemeManager.DisplayName(ids[i]));
            }
            int initialTheme = Array.IndexOf(ids, _settings.Theme);
            _themeCombo.SelectedIndex = initialTheme >= 0 ? initialTheme : 0;
            _themeCombo.SelectedIndexChanged += ThemeChanged;
            table.Controls.Add(_themeCombo, 1, 0);

            _snowCheck = MakeCheckBox("Показывать снежинки (зимой)");
            _snowCheck.Dock = DockStyle.Fill;
            _snowCheck.Margin = new Padding(0, 2, 12, 2);
            _snowCheck.CheckedChanged += SnowOptionChanged;
            table.Controls.Add(_snowCheck, 1, 1);

            Label hint = MakeHintLabel(
                "Тема «Авто» переключается по сезону: весна, лето, осень и зима. " +
                "В зимней теме идёт снег; снежинки можно отключить.");
            hint.Dock = DockStyle.Fill;
            hint.TextAlign = ContentAlignment.TopLeft;
            hint.Margin = new Padding(0, 2, 0, 0);
            table.Controls.Add(hint, 0, 2);
            table.SetColumnSpan(hint, 2);

            _tips.SetToolTip(_themeCombo, "Цветовая схема программы");
            _tips.SetToolTip(_snowCheck, "Анимированные снежинки в зимней теме");
            return group;
        }

        private GroupBox BuildBehaviorGroup()
        {
            CardPanel group = new CardPanel();
            group.Text = "Поведение";
            group.Font = new Font("Segoe UI", 10f, FontStyle.Bold, GraphicsUnit.Point);
            group.ForeColor = TextColor;
            group.BackColor = PanelBack;
            group.Padding = new Padding(12, 24, 12, 12);
            group.AutoSize = true;
            group.AutoSizeMode = AutoSizeMode.GrowAndShrink;

            TableLayoutPanel grid = new TableLayoutPanel();
            grid.Dock = DockStyle.Top;
            grid.AutoSize = true;
            grid.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            grid.ColumnCount = 2;
            grid.RowCount = 3;
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 28f));
            grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 28f));
            grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 36f));
            grid.BackColor = PanelBack;
            group.Controls.Add(grid);

            _ringStartCheck = MakeCheckBox("Звонить в начале урока");
            _ringStartCheck.Dock = DockStyle.Fill;
            _ringStartCheck.Margin = new Padding(0, 2, 12, 2);
            _ringStartCheck.CheckedChanged += RingOptionsChanged;
            grid.Controls.Add(_ringStartCheck, 0, 0);

            _ringEndCheck = MakeCheckBox("Звонить в конце урока");
            _ringEndCheck.Dock = DockStyle.Fill;
            _ringEndCheck.Margin = new Padding(0, 2, 12, 2);
            _ringEndCheck.CheckedChanged += RingOptionsChanged;
            grid.Controls.Add(_ringEndCheck, 1, 0);

            _autoStartCheck = MakeCheckBox("Запускать вместе с Windows");
            _autoStartCheck.Dock = DockStyle.Fill;
            _autoStartCheck.Margin = new Padding(0, 2, 12, 2);
            _autoStartCheck.CheckedChanged += AutoStartChanged;
            grid.Controls.Add(_autoStartCheck, 0, 1);

            _minimizeCheck = MakeCheckBox("При закрытии сворачивать в трей");
            _minimizeCheck.Dock = DockStyle.Fill;
            _minimizeCheck.Margin = new Padding(0, 2, 12, 2);
            _minimizeCheck.CheckedChanged += MinimizeOptionChanged;
            grid.Controls.Add(_minimizeCheck, 1, 1);

            Label hint = MakeHintLabel(
                "Если оба пункта «Звонить…» выключены, автоматических звонков не будет. " +
                "Свернуть программу в трей можно кнопкой закрытия окна.");
            hint.Dock = DockStyle.Fill;
            hint.TextAlign = ContentAlignment.TopLeft;
            hint.Margin = new Padding(0, 4, 0, 0);
            grid.Controls.Add(hint, 0, 2);
            grid.SetColumnSpan(hint, 2);

            _tips.SetToolTip(_ringStartCheck, "Звук в начале каждого урока");
            _tips.SetToolTip(_ringEndCheck, "Звук в конце каждого урока");
            _tips.SetToolTip(_autoStartCheck, "Добавить программу в автозагрузку текущего пользователя Windows");
            _tips.SetToolTip(_minimizeCheck, "Кнопка закрытия окна будет сворачивать программу в трей");
            return group;
        }

        private GroupBox BuildDataGroup()
        {
            CardPanel group = new CardPanel();
            group.Text = "Данные программы";
            group.Font = new Font("Segoe UI", 10f, FontStyle.Bold, GraphicsUnit.Point);
            group.ForeColor = TextColor;
            group.BackColor = PanelBack;
            group.Padding = new Padding(12, 24, 12, 12);
            group.AutoSize = true;
            group.AutoSizeMode = AutoSizeMode.GrowAndShrink;

            TableLayoutPanel table = new TableLayoutPanel();
            table.Dock = DockStyle.Top;
            table.AutoSize = true;
            table.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            table.ColumnCount = 3;
            table.RowCount = 2;
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 58f));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 144f));
            table.RowStyles.Add(new RowStyle(SizeType.Absolute, 34f));
            table.RowStyles.Add(new RowStyle(SizeType.Absolute, 30f));
            table.BackColor = PanelBack;
            group.Controls.Add(table);

            Label caption = new Label();
            caption.Text = "Папка:";
            caption.Dock = DockStyle.Fill;
            caption.TextAlign = ContentAlignment.MiddleLeft;
            caption.Font = new Font("Segoe UI", 9.5f, FontStyle.Regular, GraphicsUnit.Point);
            table.Controls.Add(caption, 0, 0);

            _dataDirBox = new TextBox();
            _dataDirBox.ReadOnly = true;
            _dataDirBox.Dock = DockStyle.Fill;
            _dataDirBox.BorderStyle = BorderStyle.FixedSingle;
            _dataDirBox.BackColor = GridAltBack;
            _dataDirBox.ForeColor = StatusText;
            _dataDirBox.Font = new Font("Segoe UI", 8.75f, FontStyle.Regular, GraphicsUnit.Point);
            _dataDirBox.Margin = new Padding(0, 5, 8, 5);
            table.Controls.Add(_dataDirBox, 1, 0);

            _openDataButton = MakeButton("Открыть папку", 128, 30, false);
            _openDataButton.Dock = DockStyle.Top;
            _openDataButton.Margin = new Padding(8, 2, 0, 2);
            _openDataButton.Click += delegate { OpenDataFolder(); };
            table.Controls.Add(_openDataButton, 2, 0);

            Label hint = MakeHintLabel(
                "Для резервной копии скопируйте эту папку. Журнал звонков сохраняется в файле bells.log.");
            hint.Dock = DockStyle.Fill;
            hint.TextAlign = ContentAlignment.TopLeft;
            hint.Padding = new Padding(0, 4, 0, 0);
            table.Controls.Add(hint, 0, 1);
            table.SetColumnSpan(hint, 3);

            _tips.SetToolTip(_dataDirBox, "Настройки и мелодии находятся в этой папке");
            return group;
        }

        private GroupBox BuildAboutGroup()
        {
            CardPanel group = new CardPanel();
            group.Text = "О программе";
            group.Font = new Font("Segoe UI", 10f, FontStyle.Bold, GraphicsUnit.Point);
            group.ForeColor = TextColor;
            group.BackColor = PanelBack;
            group.Padding = new Padding(12, 24, 12, 12);
            group.AutoSize = true;
            group.AutoSizeMode = AutoSizeMode.GrowAndShrink;

            TableLayoutPanel table = new TableLayoutPanel();
            table.Dock = DockStyle.Top;
            table.AutoSize = true;
            table.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            table.ColumnCount = 2;
            table.RowCount = 1;
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 260f));
            table.RowStyles.Add(new RowStyle(SizeType.Absolute, 70f));
            table.BackColor = PanelBack;
            group.Controls.Add(table);

            TableLayoutPanel aboutText = new TableLayoutPanel();
            aboutText.Dock = DockStyle.Fill;
            aboutText.ColumnCount = 1;
            aboutText.RowCount = 3;
            aboutText.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            aboutText.RowStyles.Add(new RowStyle(SizeType.Absolute, 23f));
            aboutText.RowStyles.Add(new RowStyle(SizeType.Absolute, 21f));
            aboutText.RowStyles.Add(new RowStyle(SizeType.Absolute, 24f));
            aboutText.Margin = new Padding(0);
            aboutText.BackColor = PanelBack;
            table.Controls.Add(aboutText, 0, 0);

            Label title = new Label();
            title.Text = "Школьные звонки, версия 1.2";
            title.Dock = DockStyle.Fill;
            title.TextAlign = ContentAlignment.MiddleLeft;
            title.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold, GraphicsUnit.Point);
            title.ForeColor = TextColor;
            aboutText.Controls.Add(title, 0, 0);

            Label description = MakeHintLabel(
                "Windows 10/11 · не требует установки · настройки сохраняются автоматически");
            description.Dock = DockStyle.Fill;
            description.TextAlign = ContentAlignment.MiddleLeft;
            aboutText.Controls.Add(description, 0, 1);

            LinkLabel author = new LinkLabel();
            author.Text = "Автор: Иван Семёнов · github.com/IvanSemenovVetarion";
            author.Dock = DockStyle.Fill;
            author.TextAlign = ContentAlignment.MiddleLeft;
            author.Font = new Font("Segoe UI", 9f, FontStyle.Regular, GraphicsUnit.Point);
            author.LinkColor = Accent;
            author.ActiveLinkColor = AccentHover;
            author.LinkBehavior = LinkBehavior.HoverUnderline;
            author.LinkClicked += delegate { OpenAuthorPage(); };
            aboutText.Controls.Add(author, 0, 2);

            _openReadmeButton = MakeButton("Открыть инструкцию (README.txt)", 240, 30, false);
            _openReadmeButton.Anchor = AnchorStyles.Left;
            _openReadmeButton.Margin = new Padding(8, 18, 0, 7);
            _openReadmeButton.Click += delegate { OpenReadme(); };
            table.Controls.Add(_openReadmeButton, 1, 0);

            return group;
        }

        private FlowLayoutPanel MakeButtonRow()
        {
            FlowLayoutPanel panel = new FlowLayoutPanel();
            panel.Dock = DockStyle.Fill;
            panel.FlowDirection = FlowDirection.LeftToRight;
            panel.WrapContents = false;
            panel.Margin = new Padding(0);
            panel.BackColor = PanelBack;
            return panel;
        }

        private ModernCheckBox MakeCheckBox(string text)
        {
            ModernCheckBox check = new ModernCheckBox();
            check.Text = text;
            check.AutoSize = true;
            check.Font = new Font("Segoe UI", 9.5f, FontStyle.Regular, GraphicsUnit.Point);
            check.ForeColor = TextColor;
            check.Margin = new Padding(0, 2, 0, 8);
            return check;
        }

        private void UpdateSettingsControls()
        {
            if (_ringStartCheck == null)
            {
                return;
            }

            _updatingControls = true;
            try
            {
                _ringStartCheck.Checked = _settings.RingAtStart;
                _ringEndCheck.Checked = _settings.RingAtEnd;
                _autoStartCheck.Checked = _settings.AutoStart;
                _minimizeCheck.Checked = _settings.MinimizeToTray;
                if (_themeCombo != null)
                {
                    int themeIndex = Array.IndexOf(ThemeManager.Ids, _settings.Theme);
                    _themeCombo.SelectedIndex = themeIndex >= 0 ? themeIndex : 0;
                }
                if (_snowCheck != null)
                {
                    _snowCheck.Checked = _settings.Snowflakes;
                }
                _volumeTrack.Value = Math.Max(0, Math.Min(100, _settings.Volume));
                _volumeValueLabel.Text = _settings.Volume.ToString() + " %";
                _playback.Volume = _settings.Volume;
                _dataDirBox.Text = _store.DataDir;
                UpdateMelodyBoxes();
            }
            finally
            {
                _updatingControls = false;
            }
        }

        private void UpdateMelodyBoxes()
        {
            if (_startMelodyBox == null)
            {
                return;
            }

            string startFile = _settings.MelodyStartFile;
            string startDisplay = _settings.MelodyStartName;
            if (string.IsNullOrEmpty(startDisplay))
            {
                startDisplay = (string.IsNullOrEmpty(startFile) || string.Equals(startFile, "default_bell.wav", StringComparison.OrdinalIgnoreCase))
                    ? "Встроенный звонок"
                    : startFile;
            }

            string startPath = _store.ResolveMelodyPath(startFile);
            if (!string.IsNullOrEmpty(startFile) && !File.Exists(startPath))
            {
                startDisplay += "  (файл не найден)";
                _startMelodyBox.ForeColor = Red;
            }
            else
            {
                _startMelodyBox.ForeColor = TextColor;
            }
            _startMelodyBox.Text = startDisplay;
            _tips.SetToolTip(_startMelodyBox, string.IsNullOrEmpty(startPath) ? "Встроенный звонок" : startPath);

            if (string.IsNullOrEmpty(_settings.MelodyEndFile))
            {
                _endMelodyBox.Text = "— как в начале —";
                _endMelodyBox.ForeColor = HintText;
                _tips.SetToolTip(_endMelodyBox, "Будет использована мелодия начала урока");
            }
            else
            {
                string endDisplay = _settings.MelodyEndName;
                if (string.IsNullOrEmpty(endDisplay))
                {
                    endDisplay = _settings.MelodyEndFile;
                }
                string endPath = _store.ResolveMelodyPath(_settings.MelodyEndFile);
                if (!File.Exists(endPath))
                {
                    endDisplay += "  (файл не найден)";
                    _endMelodyBox.ForeColor = Red;
                }
                else
                {
                    _endMelodyBox.ForeColor = TextColor;
                }
                _endMelodyBox.Text = endDisplay;
                _tips.SetToolTip(_endMelodyBox, endPath);
            }
        }

        private void BrowseMelody(bool forStart)
        {
            StopPlayback();
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Title = forStart ? "Мелодия начала урока" : "Мелодия конца урока";
                dialog.Filter = "Аудиофайлы (*.mp3;*.wav;*.wma;*.m4a)|*.mp3;*.wav;*.wma;*.m4a|MP3 (*.mp3)|*.mp3|Все файлы (*.*)|*.*";
                dialog.CheckFileExists = true;
                string currentFile = forStart ? _settings.MelodyStartFile : _settings.MelodyEndFile;
                string currentPath = _store.ResolveMelodyPath(currentFile);
                if (!string.IsNullOrEmpty(currentPath) && File.Exists(currentPath))
                {
                    dialog.InitialDirectory = Path.GetDirectoryName(currentPath);
                }
                else
                {
                    dialog.InitialDirectory = _store.DataDir;
                }

                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                Cursor previous = Cursor;
                Cursor = Cursors.WaitCursor;
                ShowStatus("Копирование мелодии…");
                try
                {
                    string error;
                    string imported = _store.ImportMelody(dialog.FileName, forStart, out error);
                    if (imported == null)
                    {
                        MessageBox.Show(this,
                            "Не удалось загрузить мелодию.\n\n" + error,
                            "Школьные звонки",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);
                        ShowStatus("Мелодия не загружена.");
                        return;
                    }

                    string displayName = Path.GetFileName(dialog.FileName);
                    if (forStart)
                    {
                        _settings.MelodyStartFile = imported;
                        _settings.MelodyStartName = displayName;
                    }
                    else
                    {
                        _settings.MelodyEndFile = imported;
                        _settings.MelodyEndName = displayName;
                    }

                    _store.CleanupMelodies(_settings);
                    UpdateMelodyBoxes();
                    SaveNow(false);
                    ShowStatus("Мелодия загружена: " + displayName);
                    PreviewMelody(forStart);
                }
                finally
                {
                    Cursor = previous;
                }
            }
        }

        private void PreviewMelody(bool forStart)
        {
            string file = forStart ? _settings.MelodyStartFile : _settings.MelodyEndFile;
            if (string.IsNullOrEmpty(file))
            {
                file = _settings.MelodyStartFile;
            }
            if (string.IsNullOrEmpty(file))
            {
                file = "default_bell.wav";
            }

            string path = _store.ResolveMelodyPath(file);
            string error;
            bool ok = PlaySound(path, true, out error);
            if (ok)
            {
                ShowStatus("Прослушивание: " + (forStart ? "начало урока" : "конец урока"));
            }
            else
            {
                ShowStatus("Не удалось воспроизвести мелодию.");
                MessageBox.Show(this,
                    "Не удалось воспроизвести мелодию.\n\n" + error,
                    "Школьные звонки",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }

        private void ResetMelody(bool forStart)
        {
            StopPlayback();
            if (forStart)
            {
                _settings.MelodyStartFile = "default_bell.wav";
                _settings.MelodyStartName = "";
                DefaultBellGenerator.EnsureDefaultBell(_store.DataDir);
                _store.CleanupMelodies(_settings);
                UpdateMelodyBoxes();
                SaveNow(false);
                ShowStatus("Установлен встроенный звонок.");
                PreviewMelody(true);
            }
            else
            {
                _settings.MelodyEndFile = "";
                _settings.MelodyEndName = "";
                _store.CleanupMelodies(_settings);
                UpdateMelodyBoxes();
                SaveNow(false);
                ShowStatus("Для конца урока будет использоваться мелодия начала.");
            }
        }

        private void VolumeChanged(object sender, EventArgs e)
        {
            if (_updatingControls)
            {
                return;
            }

            _settings.Volume = _volumeTrack.Value;
            _volumeValueLabel.Text = _settings.Volume.ToString() + " %";
            _playback.Volume = _settings.Volume;
            SaveSoon();
        }

        private void ThemeChanged(object sender, EventArgs e)
        {
            if (_updatingControls || _themeCombo == null || _themeCombo.SelectedIndex < 0)
            {
                return;
            }

            string[] ids = ThemeManager.Ids;
            if (_themeCombo.SelectedIndex >= ids.Length)
            {
                return;
            }

            _settings.Theme = ids[_themeCombo.SelectedIndex];
            SaveNow(false);
            ShowStatus("Тема: " + ThemeManager.DisplayName(_settings.Theme) + ".");
            BeginInvoke(new MethodInvoker(delegate { RebuildInterface(); }));
        }

        private void SnowOptionChanged(object sender, EventArgs e)
        {
            if (_updatingControls || _snowCheck == null)
            {
                return;
            }

            _settings.Snowflakes = _snowCheck.Checked;
            SaveNow(false);
            ShowStatus(_settings.Snowflakes
                ? "Снежинки включены. Они видны в зимней теме."
                : "Снежинки отключены.");
            BeginInvoke(new MethodInvoker(delegate { RebuildInterface(); }));
        }

        private void RingOptionsChanged(object sender, EventArgs e)
        {
            if (_updatingControls)
            {
                return;
            }

            _settings.RingAtStart = _ringStartCheck.Checked;
            _settings.RingAtEnd = _ringEndCheck.Checked;
            SaveSoon();
            UpdateHeader(DateTime.Now);
            if (!_settings.RingAtStart && !_settings.RingAtEnd)
            {
                ShowStatus("Звонки в начале и конце отключены — автоматических звонков не будет.");
            }
            else
            {
                ShowStatus("Настройки звонков сохранены.");
            }
        }

        private void AutoStartChanged(object sender, EventArgs e)
        {
            if (_updatingControls)
            {
                return;
            }

            bool desired = _autoStartCheck.Checked;
            string error;
            if (!StartupManager.SetEnabled(desired, Application.ExecutablePath, out error))
            {
                MessageBox.Show(this,
                    "Не удалось изменить автозапуск.\n\n" + error,
                    "Школьные звонки",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                _updatingControls = true;
                _autoStartCheck.Checked = _settings.AutoStart;
                _updatingControls = false;
                return;
            }

            _settings.AutoStart = desired;
            SaveSoon();
            ShowStatus(desired ? "Программа добавлена в автозапуск Windows." : "Автозапуск отключён.");
        }

        private void MinimizeOptionChanged(object sender, EventArgs e)
        {
            if (_updatingControls)
            {
                return;
            }

            _settings.MinimizeToTray = _minimizeCheck.Checked;
            SaveSoon();
            ShowStatus(_settings.MinimizeToTray
                ? "При закрытии окна программа будет сворачиваться в трей."
                : "При закрытии окна программа будет завершаться.");
        }

        private void OpenDataFolder()
        {
            try
            {
                Process.Start("explorer.exe", "\"" + _store.DataDir + "\"");
            }
            catch (Exception ex)
            {
                Log.Error("Не удалось открыть папку данных", ex);
                MessageBox.Show(this, "Не удалось открыть папку:\n" + _store.DataDir,
                    "Школьные звонки", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void OpenAuthorPage()
        {
            try
            {
                ProcessStartInfo info = new ProcessStartInfo("https://github.com/IvanSemenovVetarion");
                info.UseShellExecute = true;
                Process.Start(info);
            }
            catch (Exception ex)
            {
                Log.Error("Не удалось открыть страницу автора", ex);
                MessageBox.Show(this, "Не удалось открыть ссылку.\n\n" + ex.Message,
                    "Школьные звонки", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void OpenReadme()
        {
            try
            {
                string dir = Path.GetDirectoryName(Application.ExecutablePath);
                string path = Path.Combine(dir, "README.txt");
                if (!File.Exists(path))
                {
                    path = Path.Combine(dir, "README.md");
                }
                if (!File.Exists(path))
                {
                    MessageBox.Show(this,
                        "Файл инструкции не найден рядом с программой.\nОжидался README.txt.",
                        "Школьные звонки",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    return;
                }

                ProcessStartInfo info = new ProcessStartInfo(path);
                info.UseShellExecute = true;
                Process.Start(info);
            }
            catch (Exception ex)
            {
                Log.Error("Не удалось открыть инструкцию", ex);
                MessageBox.Show(this, "Не удалось открыть инструкцию.\n\n" + ex.Message,
                    "Школьные звонки", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}
