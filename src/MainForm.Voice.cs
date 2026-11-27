using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace SchoolBells
{
    public sealed partial class MainForm
    {
        private ListBox _voiceList;
        private Button _recordButton;
        private Label _recordStatusLabel;
        private TrackBar _voiceVolumeTrack;
        private Label _voiceVolumeLabel;
        private DateTime _recordStartedAt;

        private TabPage BuildVoiceTab()
        {
            TabPage page = new TabPage("Голосовое оповещение");
            page.BackColor = PageBack;
            page.Padding = new Padding(8);

            TableLayoutPanel content = new TableLayoutPanel();
            content.Dock = DockStyle.Fill;
            content.ColumnCount = 1;
            content.RowCount = 2;
            content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            content.RowStyles.Add(new RowStyle(SizeType.Absolute, 196f));
            content.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            content.BackColor = PageBack;
            page.Controls.Add(content);

            content.Controls.Add(BuildRecordGroup(), 0, 0);
            content.Controls.Add(BuildVoiceArchiveGroup(), 0, 1);
            return page;
        }

        private GroupBox BuildRecordGroup()
        {
            GroupBox group = new GroupBox();
            group.Text = "Запись с микрофона";
            group.Dock = DockStyle.Fill;
            group.Font = new Font("Segoe UI", 10f, FontStyle.Bold, GraphicsUnit.Point);
            group.ForeColor = TextColor;
            group.BackColor = PanelBack;
            group.Padding = new Padding(12, 24, 12, 10);
            group.Margin = new Padding(0, 0, 0, 8);

            TableLayoutPanel table = new TableLayoutPanel();
            table.Dock = DockStyle.Fill;
            table.ColumnCount = 2;
            table.RowCount = 3;
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 190f));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            table.RowStyles.Add(new RowStyle(SizeType.Absolute, 46f));
            table.RowStyles.Add(new RowStyle(SizeType.Absolute, 40f));
            table.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            table.BackColor = PanelBack;
            group.Controls.Add(table);

            FlowLayoutPanel buttons = new FlowLayoutPanel();
            buttons.Dock = DockStyle.Fill;
            buttons.FlowDirection = FlowDirection.LeftToRight;
            buttons.WrapContents = false;
            buttons.Margin = new Padding(0);
            table.Controls.Add(buttons, 0, 0);
            table.SetColumnSpan(buttons, 2);

            _recordButton = MakeButton("● Начать запись", 170, 34, true);
            _recordButton.BackColor = Red;
            _recordButton.FlatAppearance.MouseOverBackColor = RedHover;
            _recordButton.Click += delegate { ToggleVoiceRecording(); };
            buttons.Controls.Add(_recordButton);

            Button stop = MakeButton("■ Остановить и сохранить", 220, 34, false);
            stop.Click += delegate { StopVoiceRecording(true); };
            buttons.Controls.Add(stop);

            _recordStatusLabel = new Label();
            _recordStatusLabel.Text = "Готово к записи";
            _recordStatusLabel.Dock = DockStyle.Fill;
            _recordStatusLabel.TextAlign = ContentAlignment.MiddleLeft;
            _recordStatusLabel.Font = new Font("Segoe UI", 9.5f, FontStyle.Regular, GraphicsUnit.Point);
            _recordStatusLabel.ForeColor = HintText;
            _recordStatusLabel.Margin = new Padding(4, 2, 0, 0);
            table.Controls.Add(_recordStatusLabel, 0, 1);
            table.SetColumnSpan(_recordStatusLabel, 2);

            Label hint = MakeHintLabel(
                "Нажмите «Начать запись», произнесите объявление и нажмите «Остановить».\n" +
                "Запись сохранится в папке программы и появится в архиве ниже.\n" +
                "Для трансляции на школу подключите выход компьютера к школьной акустике.");
            hint.Dock = DockStyle.Fill;
            hint.TextAlign = ContentAlignment.TopLeft;
            hint.Margin = new Padding(0, 4, 0, 0);
            table.Controls.Add(hint, 0, 2);
            table.SetColumnSpan(hint, 2);

            _tips.SetToolTip(_recordButton, "Начать или остановить запись с микрофона");
            _tips.SetToolTip(stop, "Остановить запись и сохранить WAV-файл");
            return group;
        }

        private GroupBox BuildVoiceArchiveGroup()
        {
            GroupBox group = new GroupBox();
            group.Text = "Архив объявлений";
            group.Dock = DockStyle.Fill;
            group.Font = new Font("Segoe UI", 10f, FontStyle.Bold, GraphicsUnit.Point);
            group.ForeColor = TextColor;
            group.BackColor = PanelBack;
            group.Padding = new Padding(10, 22, 10, 8);
            group.Margin = new Padding(0);

            TableLayoutPanel layout = new TableLayoutPanel();
            layout.Dock = DockStyle.Fill;
            layout.ColumnCount = 2;
            layout.RowCount = 2;
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 300f));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42f));
            layout.BackColor = PanelBack;
            group.Controls.Add(layout);

            _voiceList = new ListBox();
            _voiceList.Dock = DockStyle.Fill;
            _voiceList.IntegralHeight = false;
            _voiceList.BorderStyle = BorderStyle.FixedSingle;
            _voiceList.Font = new Font("Segoe UI", 9.75f, FontStyle.Regular, GraphicsUnit.Point);
            _voiceList.Margin = new Padding(0, 0, 8, 6);
            _voiceList.DoubleClick += delegate { PlaySelectedVoice(); };
            layout.Controls.Add(_voiceList, 0, 0);
            layout.SetRowSpan(_voiceList, 2);

            FlowLayoutPanel buttons = new FlowLayoutPanel();
            buttons.Dock = DockStyle.Fill;
            buttons.FlowDirection = FlowDirection.LeftToRight;
            buttons.WrapContents = true;
            buttons.Margin = new Padding(0);
            layout.Controls.Add(buttons, 1, 0);

            Button play = MakeButton("▶ Прослушать", 122, 30, true);
            play.Click += delegate { PlaySelectedVoice(); };
            buttons.Controls.Add(play);

            Button delete = MakeButton("Удалить", 84, 30, false);
            delete.Click += delegate { DeleteSelectedVoice(); };
            buttons.Controls.Add(delete);

            Button refresh = MakeButton("Обновить", 90, 30, false);
            refresh.Click += delegate { RefreshVoiceList(); ShowStatus("Список объявлений обновлён."); };
            buttons.Controls.Add(refresh);

            Button folder = MakeButton("Открыть папку", 112, 30, false);
            folder.Click += delegate { OpenDataFolder(); };
            buttons.Controls.Add(folder);

            FlowLayoutPanel volumePanel = new FlowLayoutPanel();
            volumePanel.Dock = DockStyle.Fill;
            volumePanel.FlowDirection = FlowDirection.LeftToRight;
            volumePanel.WrapContents = false;
            volumePanel.Margin = new Padding(0);
            layout.Controls.Add(volumePanel, 1, 1);

            Label volumeLabel = new Label();
            volumeLabel.Text = "Громкость:";
            volumeLabel.AutoSize = true;
            volumeLabel.Margin = new Padding(0, 8, 4, 0);
            volumeLabel.Font = new Font("Segoe UI", 9.25f, FontStyle.Regular, GraphicsUnit.Point);
            volumePanel.Controls.Add(volumeLabel);

            _voiceVolumeTrack = new TrackBar();
            _voiceVolumeTrack.Minimum = 0;
            _voiceVolumeTrack.Maximum = 100;
            _voiceVolumeTrack.TickFrequency = 10;
            _voiceVolumeTrack.SmallChange = 5;
            _voiceVolumeTrack.LargeChange = 10;
            _voiceVolumeTrack.TickStyle = TickStyle.None;
            _voiceVolumeTrack.Width = 150;
            _voiceVolumeTrack.Height = 34;
            _voiceVolumeTrack.Margin = new Padding(0, 2, 4, 0);
            _voiceVolumeTrack.ValueChanged += VoiceVolumeChanged;
            volumePanel.Controls.Add(_voiceVolumeTrack);

            _voiceVolumeLabel = new Label();
            _voiceVolumeLabel.Text = "90 %";
            _voiceVolumeLabel.Width = 48;
            _voiceVolumeLabel.Height = 32;
            _voiceVolumeLabel.TextAlign = ContentAlignment.MiddleLeft;
            _voiceVolumeLabel.Font = new Font("Segoe UI", 9.25f, FontStyle.Bold, GraphicsUnit.Point);
            _voiceVolumeLabel.ForeColor = TextColor;
            volumePanel.Controls.Add(_voiceVolumeLabel);

            _tips.SetToolTip(_voiceList, "Сохранённые голосовые объявления");
            _tips.SetToolTip(_voiceVolumeTrack, "Громкость голосовых объявлений");
            return group;
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
                _recordButton.Text = _recording ? "■ Остановить запись" : "● Начать запись";
                _recordButton.BackColor = _recording ? Red : Accent;
                _recordButton.FlatAppearance.MouseOverBackColor = _recording ? RedHover : AccentHover;
                if (!_recording)
                {
                    _recordStatusLabel.Text = "Готово к записи";
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

            _playback.Stop();
            _musicPlayback.Stop();
            _voicePlayback.Stop();
            _musicStopAt = DateTime.MinValue;

            string error;
            if (!_recorder.Start(out error))
            {
                Log.Error("Не удалось начать запись с микрофона: " + error, null);
                MessageBox.Show(this,
                    "Не удалось начать запись.\n\n" + error +
                    "\n\nПроверьте микрофон в параметрах Windows: Параметры → Конфиденциальность → Микрофон.",
                    "Голосовое оповещение",
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
            _recordButton.BackColor = Accent;
            _recordButton.FlatAppearance.MouseOverBackColor = AccentHover;
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
                    "Голосовое оповещение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
            SaveSoon();
        }
    }
}
