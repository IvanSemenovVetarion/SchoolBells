using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;

namespace SchoolBells
{
    public sealed partial class MainForm
    {
        private ListBox _voiceList;
        private Button _recordButton;
        private Label _recordStatusLabel;
        private ModernSlider _voiceVolumeTrack;
        private Label _voiceVolumeLabel;
        private Button _liveButton;
        private Label _liveStatusLabel;
        private LevelMeter _liveLevel;
        private LiveMicrophone _liveMic;
        private DateTime _recordStartedAt;

        private Panel BuildVoiceTab()
        {
            Panel page = new Panel();
            page.Dock = DockStyle.Fill;
            page.BackColor = PageBack;
            page.Padding = new Padding(2, 0, 2, 0);

            TableLayoutPanel content = new TableLayoutPanel();
            content.Dock = DockStyle.Fill;
            content.ColumnCount = 1;
            content.RowCount = 2;
            content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            content.RowStyles.Add(new RowStyle(SizeType.Absolute, 232f));
            content.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            content.BackColor = PageBack;
            page.Controls.Add(content);

            TableLayoutPanel top = new TableLayoutPanel();
            top.Dock = DockStyle.Fill;
            top.ColumnCount = 2;
            top.RowCount = 1;
            top.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            top.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            top.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            top.BackColor = PageBack;
            top.Margin = new Padding(0, 0, 0, 8);
            content.Controls.Add(top, 0, 0);

            GroupBox live = BuildLiveGroup();
            live.Dock = DockStyle.Fill;
            live.Margin = new Padding(0, 0, 6, 0);
            top.Controls.Add(live, 0, 0);

            GroupBox record = BuildRecordGroup();
            record.Dock = DockStyle.Fill;
            record.Margin = new Padding(6, 0, 0, 0);
            top.Controls.Add(record, 1, 0);

            GroupBox archive = BuildVoiceArchiveGroup();
            archive.Dock = DockStyle.Fill;
            archive.Margin = new Padding(0, 6, 0, 0);
            content.Controls.Add(archive, 0, 1);
            return page;
        }

        private GroupBox BuildLiveGroup()
        {
            CardPanel group = new CardPanel();
            group.Text = "Прямая трансляция";
            group.Dock = DockStyle.Fill;
            group.Font = new Font("Segoe UI Semibold", 10f, FontStyle.Regular, GraphicsUnit.Point);
            group.ForeColor = TextColor;
            group.BackColor = PanelBack;
            group.Padding = new Padding(14, 32, 14, 10);
            group.Margin = new Padding(0);

            TableLayoutPanel table = new TableLayoutPanel();
            table.Dock = DockStyle.Fill;
            table.ColumnCount = 1;
            table.RowCount = 5;
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            table.RowStyles.Add(new RowStyle(SizeType.Absolute, 44f));
            table.RowStyles.Add(new RowStyle(SizeType.Absolute, 22f));
            table.RowStyles.Add(new RowStyle(SizeType.Absolute, 22f));
            table.RowStyles.Add(new RowStyle(SizeType.Absolute, 38f));
            table.RowStyles.Add(new RowStyle(SizeType.Absolute, 44f));
            table.BackColor = PanelBack;
            group.Controls.Add(table);

            _liveButton = MakeButton("● Включить микрофон", 220, 38, true);
            _liveButton.Dock = DockStyle.Fill;
            _liveButton.Margin = new Padding(0, 0, 0, 6);
            _liveButton.BackColor = Green;
            _liveButton.FlatAppearance.MouseOverBackColor = GreenHover;
            _liveButton.Font = new Font("Segoe UI Semibold", 10f, FontStyle.Regular, GraphicsUnit.Point);
            _liveButton.Click += delegate { ToggleLiveMicrophone(); };
            table.Controls.Add(_liveButton, 0, 0);

            _liveStatusLabel = new Label();
            _liveStatusLabel.Text = "Микрофон выключен";
            _liveStatusLabel.Dock = DockStyle.Fill;
            _liveStatusLabel.TextAlign = ContentAlignment.MiddleLeft;
            _liveStatusLabel.Font = new Font("Segoe UI", 9f, FontStyle.Regular, GraphicsUnit.Point);
            _liveStatusLabel.ForeColor = HintText;
            table.Controls.Add(_liveStatusLabel, 0, 1);

            TableLayoutPanel levelRow = new TableLayoutPanel();
            levelRow.Dock = DockStyle.Fill;
            levelRow.ColumnCount = 2;
            levelRow.RowCount = 1;
            levelRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 68f));
            levelRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            levelRow.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            levelRow.BackColor = PanelBack;
            levelRow.Margin = new Padding(0);
            table.Controls.Add(levelRow, 0, 2);

            Label levelLabel = new Label();
            levelLabel.Text = "Уровень:";
            levelLabel.Dock = DockStyle.Fill;
            levelLabel.TextAlign = ContentAlignment.MiddleLeft;
            levelLabel.Font = new Font("Segoe UI", 8.5f, FontStyle.Regular, GraphicsUnit.Point);
            levelLabel.ForeColor = HintText;
            levelRow.Controls.Add(levelLabel, 0, 0);

            _liveLevel = new LevelMeter();
            _liveLevel.Dock = DockStyle.Fill;
            _liveLevel.Margin = new Padding(0, 5, 0, 5);
            levelRow.Controls.Add(_liveLevel, 1, 0);

            TableLayoutPanel volumeRow = new TableLayoutPanel();
            volumeRow.Dock = DockStyle.Fill;
            volumeRow.ColumnCount = 3;
            volumeRow.RowCount = 1;
            volumeRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 82f));
            volumeRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            volumeRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 54f));
            volumeRow.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            volumeRow.BackColor = PanelBack;
            volumeRow.Margin = new Padding(0);
            table.Controls.Add(volumeRow, 0, 3);

            Label volumeLabel = new Label();
            volumeLabel.Text = "Громкость:";
            volumeLabel.Dock = DockStyle.Fill;
            volumeLabel.TextAlign = ContentAlignment.MiddleLeft;
            volumeLabel.Font = new Font("Segoe UI", 8.5f, FontStyle.Regular, GraphicsUnit.Point);
            volumeLabel.ForeColor = HintText;
            volumeRow.Controls.Add(volumeLabel, 0, 0);

            _voiceVolumeTrack = new ModernSlider();
            _voiceVolumeTrack.Minimum = 0;
            _voiceVolumeTrack.Maximum = 100;
            _voiceVolumeTrack.TickFrequency = 10;
            _voiceVolumeTrack.SmallChange = 5;
            _voiceVolumeTrack.LargeChange = 10;
            _voiceVolumeTrack.TickStyle = TickStyle.None;
            _voiceVolumeTrack.Dock = DockStyle.Fill;
            _voiceVolumeTrack.Height = 32;
            _voiceVolumeTrack.Margin = new Padding(0);
            _voiceVolumeTrack.ValueChanged += VoiceVolumeChanged;
            volumeRow.Controls.Add(_voiceVolumeTrack, 1, 0);

            _voiceVolumeLabel = new Label();
            _voiceVolumeLabel.Text = "90 %";
            _voiceVolumeLabel.Dock = DockStyle.Fill;
            _voiceVolumeLabel.TextAlign = ContentAlignment.MiddleRight;
            _voiceVolumeLabel.Font = new Font("Segoe UI Semibold", 8.75f, FontStyle.Regular, GraphicsUnit.Point);
            _voiceVolumeLabel.ForeColor = TextColor;
            volumeRow.Controls.Add(_voiceVolumeLabel, 2, 0);

            Label hint = MakeHintLabel(
                "Говорите в микрофон — звук сразу идёт на выход, без записи.\n" +
                "Подключите выход к школьной акустике. Избегайте обратной связи.");
            hint.Dock = DockStyle.Fill;
            hint.TextAlign = ContentAlignment.TopLeft;
            hint.Margin = new Padding(0, 2, 0, 0);
            table.Controls.Add(hint, 0, 4);

            _tips.SetToolTip(_liveButton,
                "Включить или выключить прямую трансляцию с микрофона. Избегайте акустической обратной связи.");
            _tips.SetToolTip(_voiceVolumeTrack, "Громкость прямой трансляции и записей");
            return group;
        }

        private GroupBox BuildRecordGroup()
        {
            CardPanel group = new CardPanel();
            group.Text = "Запись объявления";
            group.Dock = DockStyle.Fill;
            group.Font = new Font("Segoe UI Semibold", 10f, FontStyle.Regular, GraphicsUnit.Point);
            group.ForeColor = TextColor;
            group.BackColor = PanelBack;
            group.Padding = new Padding(14, 32, 14, 10);
            group.Margin = new Padding(0);

            TableLayoutPanel table = new TableLayoutPanel();
            table.Dock = DockStyle.Fill;
            table.ColumnCount = 1;
            table.RowCount = 3;
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            table.RowStyles.Add(new RowStyle(SizeType.Absolute, 48f));
            table.RowStyles.Add(new RowStyle(SizeType.Absolute, 22f));
            table.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            table.BackColor = PanelBack;
            group.Controls.Add(table);

            FlowLayoutPanel buttons = new FlowLayoutPanel();
            buttons.Dock = DockStyle.Fill;
            buttons.FlowDirection = FlowDirection.LeftToRight;
            buttons.WrapContents = true;
            buttons.BackColor = PanelBack;
            buttons.Margin = new Padding(0);
            table.Controls.Add(buttons, 0, 0);

            _recordButton = MakeButton("● Начать запись", 156, 36, true);
            _recordButton.BackColor = Red;
            _recordButton.FlatAppearance.MouseOverBackColor = RedHover;
            _recordButton.Font = new Font("Segoe UI Semibold", 9.5f, FontStyle.Regular, GraphicsUnit.Point);
            _recordButton.Click += delegate { ToggleVoiceRecording(); };
            buttons.Controls.Add(_recordButton);

            Button stop = MakeButton("■ Сохранить запись", 174, 36, false);
            stop.Click += delegate { StopVoiceRecording(true); };
            buttons.Controls.Add(stop);

            _recordStatusLabel = new Label();
            _recordStatusLabel.Text = "Готово к записи";
            _recordStatusLabel.Dock = DockStyle.Fill;
            _recordStatusLabel.TextAlign = ContentAlignment.MiddleLeft;
            _recordStatusLabel.Font = new Font("Segoe UI", 9f, FontStyle.Regular, GraphicsUnit.Point);
            _recordStatusLabel.ForeColor = HintText;
            _recordStatusLabel.Margin = new Padding(2, 2, 0, 0);
            table.Controls.Add(_recordStatusLabel, 0, 1);

            Label hint = MakeHintLabel(
                "Запишите объявление заранее, чтобы включить его одной кнопкой.\n" +
                "Запись сохранится в папке программы и появится в архиве ниже.\n" +
                "Для прямой речи без записи используйте «Прямую трансляцию».");
            hint.Dock = DockStyle.Fill;
            hint.TextAlign = ContentAlignment.TopLeft;
            hint.Margin = new Padding(0, 4, 0, 0);
            table.Controls.Add(hint, 0, 2);

            _tips.SetToolTip(_recordButton, "Начать или остановить запись с микрофона");
            _tips.SetToolTip(stop, "Остановить запись и сохранить WAV-файл");
            return group;
        }

        private GroupBox BuildVoiceArchiveGroup()
        {
            CardPanel group = new CardPanel();
            group.Text = "Архив объявлений";
            group.Dock = DockStyle.Fill;
            group.Font = new Font("Segoe UI Semibold", 10f, FontStyle.Regular, GraphicsUnit.Point);
            group.ForeColor = TextColor;
            group.BackColor = PanelBack;
            group.Padding = new Padding(14, 32, 14, 12);
            group.Margin = new Padding(0);

            TableLayoutPanel layout = new TableLayoutPanel();
            layout.Dock = DockStyle.Fill;
            layout.ColumnCount = 2;
            layout.RowCount = 2;
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 310f));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44f));
            layout.BackColor = PanelBack;
            group.Controls.Add(layout);

            _voiceList = new ListBox();
            _voiceList.Dock = DockStyle.Fill;
            _voiceList.IntegralHeight = false;
            _voiceList.BorderStyle = BorderStyle.None;
            _voiceList.BackColor = PanelBack;
            _voiceList.ForeColor = TextColor;
            _voiceList.Font = new Font("Segoe UI", 10f, FontStyle.Regular, GraphicsUnit.Point);
            _voiceList.DrawMode = DrawMode.OwnerDrawFixed;
            _voiceList.ItemHeight = 32;
            _voiceList.DoubleClick += delegate { PlaySelectedVoice(); };
            _voiceList.DrawItem += VoiceListDrawItem;

            RoundedPanel listHost = new RoundedPanel();
            listHost.Dock = DockStyle.Fill;
            listHost.BackColor = PanelBack;
            listHost.CornerRadius = 12;
            listHost.Padding = new Padding(5);
            listHost.Margin = new Padding(0, 0, 10, 6);
            listHost.Controls.Add(_voiceList);
            layout.Controls.Add(listHost, 0, 0);
            layout.SetRowSpan(listHost, 2);

            FlowLayoutPanel buttons = new FlowLayoutPanel();
            buttons.Dock = DockStyle.Fill;
            buttons.FlowDirection = FlowDirection.LeftToRight;
            buttons.WrapContents = true;
            buttons.BackColor = PanelBack;
            buttons.Margin = new Padding(0);
            layout.Controls.Add(buttons, 1, 0);

            Button play = MakeButton("▶ Прослушать", 122, 32, true);
            play.Click += delegate { PlaySelectedVoice(); };
            buttons.Controls.Add(play);

            Button delete = MakeButton("Удалить", 84, 32, false);
            delete.Click += delegate { DeleteSelectedVoice(); };
            buttons.Controls.Add(delete);

            Button refresh = MakeButton("Обновить", 90, 32, false);
            refresh.Click += delegate { RefreshVoiceList(); ShowStatus("Список объявлений обновлён."); };
            buttons.Controls.Add(refresh);

            Button folder = MakeButton("Открыть папку", 112, 32, false);
            folder.Click += delegate { OpenDataFolder(); };
            buttons.Controls.Add(folder);

            Label hint = MakeHintLabel("Двойной клик по объявлению — прослушать.");
            hint.Dock = DockStyle.Fill;
            hint.TextAlign = ContentAlignment.MiddleLeft;
            hint.Margin = new Padding(2, 0, 0, 0);
            layout.Controls.Add(hint, 1, 1);

            _tips.SetToolTip(_voiceList, "Сохранённые голосовые объявления");
            return group;
        }

        private void VoiceListDrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0 || e.Index >= _voiceList.Items.Count)
            {
                return;
            }

            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using (SolidBrush clear = new SolidBrush(PanelBack))
            {
                g.FillRectangle(clear, e.Bounds);
            }

            bool selected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
            Rectangle row = new Rectangle(e.Bounds.X + 2, e.Bounds.Y + 2, e.Bounds.Width - 5, e.Bounds.Height - 4);
            if (selected)
            {
                using (GraphicsPath path = RoundGeometry.Create(row, 8))
                using (SolidBrush brush = new SolidBrush(Accent))
                {
                    g.FillPath(brush, path);
                }
            }

            Color textColor = selected ? Color.White : TextColor;
            Rectangle textBounds = new Rectangle(row.X + 9, row.Y, row.Width - 14, row.Height);
            TextRenderer.DrawText(g, _voiceList.Items[e.Index].ToString(), e.Font, textBounds, textColor,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix |
                TextFormatFlags.EndEllipsis);
        }

        private void UpdateVoiceControls()
        {
            if (_voiceList == null)
            {
                return;
            }

            _updatingControls = true;
            try
            {
                _voiceVolumeTrack.Value = Math.Max(0, Math.Min(100, _settings.VoiceVolume));
                _voiceVolumeLabel.Text = _settings.VoiceVolume.ToString() + " %";
                _voicePlayback.Volume = _settings.VoiceVolume;
                if (_liveMic != null)
                {
                    _liveMic.Gain = _settings.VoiceVolume / 100.0;
                }

                bool live = _liveMic != null && _liveMic.IsRunning;
                _liveButton.Text = live ? "■ Остановить трансляцию" : "● Включить микрофон";
                _liveButton.BackColor = live ? Red : Green;
                _liveButton.FlatAppearance.MouseOverBackColor = live ? RedHover : GreenHover;
                _liveStatusLabel.Text = live ? "Микрофон транслируется на выход" : "Микрофон выключен";
                _liveStatusLabel.ForeColor = live ? Green : HintText;
                if (!live)
                {
                    _liveLevel.Value = 0;
                }

                _recordButton.Text = _recording ? "■ Остановить запись" : "● Начать запись";
                _recordButton.BackColor = Red;
                _recordButton.FlatAppearance.MouseOverBackColor = RedHover;
                if (!_recording)
                {
                    _recordStatusLabel.Text = "Готово к записи";
                    _recordStatusLabel.ForeColor = HintText;
                }
                RefreshVoiceList();
            }
            finally
            {
                _updatingControls = false;
            }
        }

        private void RefreshVoiceList()
        {
            if (_voiceList == null)
            {
                return;
            }

            int selected = _voiceList.SelectedIndex;
            _voiceList.BeginUpdate();
            try
            {
                _voiceList.Items.Clear();
                for (int i = 0; i < _settings.VoiceRecordings.Count; i++)
                {
                    MediaItem item = _settings.VoiceRecordings[i];
                    _voiceList.Items.Add(string.IsNullOrEmpty(item.Name) ? Path.GetFileName(item.File) : item.Name);
                }
                if (selected >= 0 && selected < _voiceList.Items.Count)
                {
                    _voiceList.SelectedIndex = selected;
                }
                else if (_voiceList.Items.Count > 0)
                {
                    _voiceList.SelectedIndex = _voiceList.Items.Count - 1;
                }
            }
            finally
            {
                _voiceList.EndUpdate();
            }
        }

        private void ToggleLiveMicrophone()
        {
            if (_liveMic != null && _liveMic.IsRunning)
            {
                StopLiveMicrophone();
            }
            else
            {
                StartLiveMicrophone();
            }
        }

        private void StartLiveMicrophone()
        {
            if (_liveMic != null && _liveMic.IsRunning)
            {
                return;
            }

            if (_recording)
            {
                CancelVoiceRecording();
            }
            StopPlayback();
            _musicStopAt = DateTime.MinValue;

            LiveMicrophone mic = new LiveMicrophone();
            mic.Gain = _settings.VoiceVolume / 100.0;
            mic.LevelChanged += LiveLevelChanged;
            string error;
            if (!mic.Start(out error))
            {
                mic.Dispose();
                Log.Error("Не удалось включить прямую трансляцию микрофона: " + error, null);
                _liveStatusLabel.Text = "Не удалось включить микрофон";
                _liveStatusLabel.ForeColor = Red;
                _liveLevel.Value = 0;
                MessageBox.Show(this,
                    "Не удалось включить микрофон.\n\n" + error +
                    "\n\nПроверьте микрофон в параметрах Windows: Параметры → Конфиденциальность → Микрофон.",
                    "Прямая трансляция",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            _liveMic = mic;
            _liveButton.Text = "■ Остановить трансляцию";
            _liveButton.BackColor = Red;
            _liveButton.FlatAppearance.MouseOverBackColor = RedHover;
            _liveStatusLabel.Text = "Микрофон транслируется на выход";
            _liveStatusLabel.ForeColor = Green;
            ShowStatus("Прямая трансляция с микрофона включена.");
        }

        private void StopLiveMicrophone()
        {
            if (_liveMic == null)
            {
                return;
            }

            LiveMicrophone mic = _liveMic;
            _liveMic = null;
            try
            {
                mic.LevelChanged -= LiveLevelChanged;
                mic.Stop();
                mic.Dispose();
            }
            catch (Exception ex)
            {
                Log.Error("Не удалось остановить прямую трансляцию микрофона", ex);
            }

            if (_liveButton != null && !_liveButton.IsDisposed)
            {
                _liveButton.Text = "● Включить микрофон";
                _liveButton.BackColor = Green;
                _liveButton.FlatAppearance.MouseOverBackColor = GreenHover;
            }
            if (_liveStatusLabel != null && !_liveStatusLabel.IsDisposed)
            {
                _liveStatusLabel.Text = "Микрофон выключен";
                _liveStatusLabel.ForeColor = HintText;
            }
            if (_liveLevel != null && !_liveLevel.IsDisposed)
            {
                _liveLevel.Value = 0;
            }
            ShowStatus("Прямая трансляция с микрофона остановлена.");
        }

        private void LiveLevelChanged(double level)
        {
            if (IsDisposed || !IsHandleCreated)
            {
                return;
            }
            try
            {
                BeginInvoke(new MethodInvoker(delegate
                {
                    if (_liveMic != null && _liveLevel != null && !_liveLevel.IsDisposed)
                    {
                        _liveLevel.Value = level;
                    }
                }));
            }
            catch
            {
            }
        }

        private void ToggleVoiceRecording()
        {
            if (_recording)
            {
                StopVoiceRecording(true);
            }
            else
            {
                StartVoiceRecording();
            }
        }

        private void StartVoiceRecording()
        {
            if (_recording)
            {
                return;
            }

            StopLiveMicrophone();
            StopPlayback();
            _musicStopAt = DateTime.MinValue;

            string error;
            if (!_recorder.Start(out error))
            {
                Log.Error("Не удалось начать запись с микрофона: " + error, null);
                MessageBox.Show(this,
                    "Не удалось начать запись.\n\n" + error +
                    "\n\nПроверьте микрофон в параметрах Windows: Параметры → Конфиденциальность → Микрофон.",
                    "Запись объявления",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            _recording = true;
            _recordStartedAt = DateTime.Now;
            _recordButton.Text = "■ Остановить запись";
            _recordButton.BackColor = Red;
            _recordButton.FlatAppearance.MouseOverBackColor = RedHover;
            _recordStatusLabel.Text = "Идёт запись… Говорите в микрофон.";
            _recordStatusLabel.ForeColor = Red;
            ShowStatus("Идёт запись голосового объявления.");
        }

        private void StopVoiceRecording(bool save)
        {
            if (!_recording)
            {
                return;
            }

            _recording = false;
            _recordButton.Text = "● Начать запись";
            _recordButton.BackColor = Red;
            _recordButton.FlatAppearance.MouseOverBackColor = RedHover;
            _recordStatusLabel.ForeColor = HintText;

            if (!save)
            {
                _recorder.Cancel();
                _recordStatusLabel.Text = "Запись отменена.";
                ShowStatus("Запись отменена.");
                return;
            }

            string fileName = _store.CreateVoiceFileName();
            string path = Path.Combine(_store.DataDir, fileName);
            string error;
            if (!_recorder.StopAndSave(path, out error))
            {
                Log.Error("Не удалось сохранить голосовую запись: " + error, null);
                _recordStatusLabel.Text = "Не удалось сохранить запись.";
                MessageBox.Show(this, "Не удалось сохранить запись.\n\n" + error,
                    "Запись объявления", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string displayName = "Объявление " + _recordStartedAt.ToString("dd.MM.yyyy HH:mm");
            _settings.VoiceRecordings.Add(new MediaItem(fileName, displayName));
            _store.CleanupMedia(_settings);
            RefreshVoiceList();
            SaveNow(false);

            _recordStatusLabel.Text = "Сохранено: " + displayName;
            ShowStatus("Голосовое объявление сохранено.");
            PlayVoiceFile(path, displayName);
        }

        private void CancelVoiceRecording()
        {
            if (!_recording)
            {
                return;
            }
            StopVoiceRecording(false);
        }

        private void PlaySelectedVoice()
        {
            int index = _voiceList == null ? -1 : _voiceList.SelectedIndex;
            if (index < 0 || index >= _settings.VoiceRecordings.Count)
            {
                ShowStatus("Выберите объявление в архиве.");
                return;
            }

            MediaItem item = _settings.VoiceRecordings[index];
            string path = _store.ResolveMelodyPath(item.File);
            if (!PlayVoiceFile(path, item.Name))
            {
                MessageBox.Show(this, "Не удалось воспроизвести объявление.\n\nФайл не найден или повреждён.",
                    "Голосовое оповещение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private bool PlayVoiceFile(string path, string name)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                return false;
            }

            StopLiveMicrophone();
            if (_recording)
            {
                CancelVoiceRecording();
            }
            _playback.Stop();
            _musicPlayback.Stop();
            _musicStopAt = DateTime.MinValue;
            _voicePlayback.Stop();
            _voicePlayback.Volume = _settings.VoiceVolume;
            string error;
            if (!_voicePlayback.Play(path, null, out error))
            {
                Log.Error("Не удалось воспроизвести голосовое объявление: " + error, null);
                ShowStatus("Не удалось воспроизвести объявление. " + error);
                return false;
            }

            ShowStatus("Объявление: " + name);
            return true;
        }

        private void DeleteSelectedVoice()
        {
            int index = _voiceList == null ? -1 : _voiceList.SelectedIndex;
            if (index < 0 || index >= _settings.VoiceRecordings.Count)
            {
                ShowStatus("Выберите объявление в архиве.");
                return;
            }

            MediaItem item = _settings.VoiceRecordings[index];
            if (MessageBox.Show(this, "Удалить объявление «" + item.Name + "»?",
                "Голосовое оповещение", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            _settings.VoiceRecordings.RemoveAt(index);
            _store.CleanupMedia(_settings);
            RefreshVoiceList();
            SaveNow(false);
            ShowStatus("Объявление удалено.");
        }

        private void VoiceVolumeChanged(object sender, EventArgs e)
        {
            if (_updatingControls || _voiceVolumeTrack == null)
            {
                return;
            }

            _settings.VoiceVolume = _voiceVolumeTrack.Value;
            _voiceVolumeLabel.Text = _settings.VoiceVolume.ToString() + " %";
            _voicePlayback.Volume = _settings.VoiceVolume;
            if (_liveMic != null)
            {
                _liveMic.Gain = _settings.VoiceVolume / 100.0;
            }
            SaveSoon();
        }
    }
}
