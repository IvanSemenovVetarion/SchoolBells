using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace SchoolBells
{
    public sealed partial class MainForm
    {
        private static readonly string[] MusicDayLabels = new string[]
        {
            "Понедельник", "Вторник", "Среда", "Четверг",
            "Пятница", "Суббота", "Воскресенье", "Каждый день"
        };

        private DataGridView _musicGrid;
        private ListBox _playlistList;
        private ModernCheckBox _musicEnabledCheck;
        private ModernSlider _musicVolumeTrack;
        private Label _musicVolumeLabel;
        private bool _loadingMusicGrid;

        private Panel BuildMusicTab()
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
            content.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            content.RowStyles.Add(new RowStyle(SizeType.Absolute, 196f));
            content.BackColor = PageBack;
            page.Controls.Add(content);

            content.Controls.Add(BuildScheduledMusicGroup(), 0, 0);
            content.Controls.Add(BuildManualMusicGroup(), 0, 1);
            return page;
        }

        private GroupBox BuildScheduledMusicGroup()
        {
            CardPanel group = new CardPanel();
            group.Text = "Музыка по расписанию";
            group.Dock = DockStyle.Fill;
            group.Font = new Font("Segoe UI", 10f, FontStyle.Bold, GraphicsUnit.Point);
            group.ForeColor = TextColor;
            group.BackColor = PanelBack;
            group.Padding = new Padding(10, 22, 10, 8);
            group.Margin = new Padding(0, 0, 0, 8);

            TableLayoutPanel layout = new TableLayoutPanel();
            layout.Dock = DockStyle.Fill;
            layout.ColumnCount = 1;
            layout.RowCount = 3;
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42f));
            layout.BackColor = PanelBack;
            group.Controls.Add(layout);

            FlowLayoutPanel toolbar = new FlowLayoutPanel();
            toolbar.Dock = DockStyle.Top;
            toolbar.AutoSize = true;
            toolbar.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            toolbar.FlowDirection = FlowDirection.LeftToRight;
            toolbar.WrapContents = true;
            toolbar.Margin = new Padding(0);
            layout.Controls.Add(toolbar, 0, 0);

            Button add = MakeButton("Добавить музыку…", 156, 30, true);
            add.Click += delegate { AddMusicEntry(); };
            toolbar.Controls.Add(add);

            Button remove = MakeButton("Удалить", 84, 30, false);
            remove.Click += delegate { DeleteMusicEntry(); };
            toolbar.Controls.Add(remove);

            Button change = MakeButton("Файл…", 72, 30, false);
            change.Click += delegate { ChangeMusicFile(); };
            toolbar.Controls.Add(change);

            Button test = MakeButton("▶ Проверить", 110, 30, false);
            test.Click += delegate { PlaySelectedScheduledMusic(); };
            toolbar.Controls.Add(test);

            Button clear = MakeButton("Очистить", 90, 30, false);
            clear.Click += delegate { ClearMusicEntries(); };
            toolbar.Controls.Add(clear);

            _musicEnabledCheck = MakeCheckBox("Включить музыку по расписанию");
            _musicEnabledCheck.Checked = _settings.MusicEnabled;
            _musicEnabledCheck.Margin = new Padding(16, 7, 0, 0);
            _musicEnabledCheck.CheckedChanged += MusicEnabledChanged;
            toolbar.Controls.Add(_musicEnabledCheck);

            _musicGrid = new DataGridView();
            _musicGrid.Dock = DockStyle.Fill;
            _musicGrid.BackgroundColor = GridBack;
            _musicGrid.BorderStyle = BorderStyle.FixedSingle;
            _musicGrid.AllowUserToAddRows = false;
            _musicGrid.AllowUserToDeleteRows = false;
            _musicGrid.AllowUserToResizeRows = false;
            _musicGrid.RowHeadersVisible = false;
            _musicGrid.MultiSelect = false;
            _musicGrid.SelectionMode = DataGridViewSelectionMode.CellSelect;
            _musicGrid.EditMode = DataGridViewEditMode.EditOnKeystrokeOrF2;
            _musicGrid.EnableHeadersVisualStyles = false;
            _musicGrid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            _musicGrid.CellBorderStyle = DataGridViewCellBorderStyle.Single;
            _musicGrid.GridColor = GridLine;
            _musicGrid.RowTemplate.Height = 28;
            _musicGrid.ColumnHeadersHeight = 32;
            _musicGrid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            _musicGrid.Font = new Font("Segoe UI", 9.75f, FontStyle.Regular, GraphicsUnit.Point);
            _musicGrid.DefaultCellStyle.Font = new Font("Segoe UI", 9.75f, FontStyle.Regular, GraphicsUnit.Point);
            _musicGrid.DefaultCellStyle.SelectionBackColor = GridSelectionBack;
            _musicGrid.DefaultCellStyle.SelectionForeColor = Color.Black;
            _musicGrid.AlternatingRowsDefaultCellStyle.BackColor = GridAltBack;
            _musicGrid.ColumnHeadersDefaultCellStyle.BackColor = GridHeaderBack;
            _musicGrid.ColumnHeadersDefaultCellStyle.ForeColor = GridHeaderText;
            _musicGrid.ColumnHeadersDefaultCellStyle.SelectionBackColor = GridHeaderBack;
            _musicGrid.ColumnHeadersDefaultCellStyle.SelectionForeColor = GridHeaderText;
            _musicGrid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold, GraphicsUnit.Point);
            _musicGrid.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            _musicGrid.CellValidating += MusicGridCellValidating;
            _musicGrid.CellEndEdit += MusicGridCellEndEdit;
            _musicGrid.DataError += delegate(object sender, DataGridViewDataErrorEventArgs e)
            {
                e.ThrowException = false;
                ShowStatus("Не удалось прочитать значение ячейки.");
            };
            layout.Controls.Add(_musicGrid, 0, 1);

            DataGridViewComboBoxColumn dayColumn = new DataGridViewComboBoxColumn();
            dayColumn.Name = "day";
            dayColumn.HeaderText = "День";
            dayColumn.Width = 130;
            dayColumn.FlatStyle = FlatStyle.Flat;
            dayColumn.SortMode = DataGridViewColumnSortMode.NotSortable;
            dayColumn.Items.AddRange(new object[]
            {
                "Понедельник", "Вторник", "Среда", "Четверг",
                "Пятница", "Суббота", "Воскресенье", "Каждый день"
            });
            _musicGrid.Columns.Add(dayColumn);

            DataGridViewTextBoxColumn startColumn = new DataGridViewTextBoxColumn();
            startColumn.Name = "start";
            startColumn.HeaderText = "Начало";
            startColumn.Width = 82;
            startColumn.MaxInputLength = 8;
            startColumn.SortMode = DataGridViewColumnSortMode.NotSortable;
            startColumn.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            _musicGrid.Columns.Add(startColumn);

            DataGridViewTextBoxColumn endColumn = new DataGridViewTextBoxColumn();
            endColumn.Name = "end";
            endColumn.HeaderText = "Конец";
            endColumn.Width = 82;
            endColumn.MaxInputLength = 8;
            endColumn.SortMode = DataGridViewColumnSortMode.NotSortable;
            endColumn.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            _musicGrid.Columns.Add(endColumn);

            DataGridViewTextBoxColumn fileColumn = new DataGridViewTextBoxColumn();
            fileColumn.Name = "file";
            fileColumn.HeaderText = "Файл";
            fileColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            fileColumn.MinimumWidth = 160;
            fileColumn.ReadOnly = true;
            fileColumn.SortMode = DataGridViewColumnSortMode.NotSortable;
            _musicGrid.Columns.Add(fileColumn);

            DataGridViewTextBoxColumn commentColumn = new DataGridViewTextBoxColumn();
            commentColumn.Name = "comment";
            commentColumn.HeaderText = "Комментарий";
            commentColumn.Width = 180;
            commentColumn.MaxInputLength = 200;
            commentColumn.SortMode = DataGridViewColumnSortMode.NotSortable;
            _musicGrid.Columns.Add(commentColumn);

            ApplyModernGridStyle(_musicGrid);
            _musicGrid.Columns["start"].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            _musicGrid.Columns["end"].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;

            Label hint = MakeHintLabel(
                "Время можно вводить как 08:00, 0800 или 800. Файлы копируются в папку программы.\n" +
                "Музыка звучит с начала до конца интервала через динамики компьютера или школьную трансляцию.");
            hint.Dock = DockStyle.Fill;
            hint.TextAlign = ContentAlignment.MiddleLeft;
            hint.Margin = new Padding(0, 4, 0, 0);
            layout.Controls.Add(hint, 0, 2);

            _tips.SetToolTip(_musicGrid, "Расписание школьной музыки");
            _tips.SetToolTip(_musicEnabledCheck, "Автоматически включать музыку в указанное время");
            return group;
        }

        private GroupBox BuildManualMusicGroup()
        {
            CardPanel group = new CardPanel();
            group.Text = "Музыка вручную";
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

            _playlistList = new ListBox();
            _playlistList.Dock = DockStyle.Fill;
            _playlistList.IntegralHeight = false;
            _playlistList.BorderStyle = BorderStyle.FixedSingle;
            _playlistList.Font = new Font("Segoe UI", 9.75f, FontStyle.Regular, GraphicsUnit.Point);
            _playlistList.Margin = new Padding(0, 0, 8, 6);
            _playlistList.DoubleClick += delegate { PlaySelectedPlaylistMusic(); };
            layout.Controls.Add(_playlistList, 0, 0);
            layout.SetRowSpan(_playlistList, 2);

            FlowLayoutPanel buttons = new FlowLayoutPanel();
            buttons.Dock = DockStyle.Fill;
            buttons.FlowDirection = FlowDirection.LeftToRight;
            buttons.WrapContents = true;
            buttons.Margin = new Padding(0);
            layout.Controls.Add(buttons, 1, 0);

            Button addFiles = MakeButton("Добавить файлы…", 146, 30, false);
            addFiles.Click += delegate { AddPlaylistFiles(); };
            buttons.Controls.Add(addFiles);

            Button remove = MakeButton("Удалить", 84, 30, false);
            remove.Click += delegate { RemovePlaylistItem(); };
            buttons.Controls.Add(remove);

            Button play = MakeButton("▶ Включить", 104, 30, true);
            play.Click += delegate { PlaySelectedPlaylistMusic(); };
            buttons.Controls.Add(play);

            Button stop = MakeButton("■ Стоп", 80, 30, false);
            stop.Click += delegate { StopMusic(); ShowStatus("Музыка остановлена."); };
            buttons.Controls.Add(stop);

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

            _musicVolumeTrack = new ModernSlider();
            _musicVolumeTrack.Minimum = 0;
            _musicVolumeTrack.Maximum = 100;
            _musicVolumeTrack.TickFrequency = 10;
            _musicVolumeTrack.SmallChange = 5;
            _musicVolumeTrack.LargeChange = 10;
            _musicVolumeTrack.TickStyle = TickStyle.None;
            _musicVolumeTrack.Width = 150;
            _musicVolumeTrack.Height = 34;
            _musicVolumeTrack.Margin = new Padding(0, 2, 4, 0);
            _musicVolumeTrack.ValueChanged += MusicVolumeChanged;
            volumePanel.Controls.Add(_musicVolumeTrack);

            _musicVolumeLabel = new Label();
            _musicVolumeLabel.Text = "60 %";
            _musicVolumeLabel.Width = 48;
            _musicVolumeLabel.Height = 32;
            _musicVolumeLabel.TextAlign = ContentAlignment.MiddleLeft;
            _musicVolumeLabel.Font = new Font("Segoe UI", 9.25f, FontStyle.Bold, GraphicsUnit.Point);
            _musicVolumeLabel.ForeColor = TextColor;
            volumePanel.Controls.Add(_musicVolumeLabel);

            _tips.SetToolTip(_playlistList, "Файлы для ручного включения музыки");
            _tips.SetToolTip(_musicVolumeTrack, "Громкость музыки и объявлений");
            return group;
        }

        private void UpdateMusicControls()
        {
            if (_musicGrid == null)
            {
                return;
            }

            _updatingControls = true;
            try
            {
                _musicEnabledCheck.Checked = _settings.MusicEnabled;
                _musicVolumeTrack.Value = Math.Max(0, Math.Min(100, _settings.MusicVolume));
                _musicVolumeLabel.Text = _settings.MusicVolume.ToString() + " %";
                _musicPlayback.Volume = _settings.MusicVolume;
                RefreshMusicGrid();
                RefreshPlaylistList();
            }
            finally
            {
                _updatingControls = false;
            }
        }

        private void RefreshMusicGrid()
        {
            if (_musicGrid == null)
            {
                return;
            }

            _loadingMusicGrid = true;
            try
            {
                _musicGrid.Rows.Clear();
                for (int i = 0; i < _settings.MusicSchedule.Count; i++)
                {
                    MusicEntry entry = _settings.MusicSchedule[i];
                    int day = entry.Day < 0 || entry.Day > 7 ? 7 : entry.Day;
                    string fileLabel = string.IsNullOrEmpty(entry.Name) ? Path.GetFileName(entry.File) : entry.Name;
                    _musicGrid.Rows.Add(
                        MusicDayLabels[day],
                        TimeUtil.FormatTime(entry.Start),
                        TimeUtil.FormatTime(entry.End),
                        fileLabel,
                        entry.Comment);
                }
            }
            finally
            {
                _loadingMusicGrid = false;
            }
        }

        private void RefreshPlaylistList()
        {
            if (_playlistList == null)
            {
                return;
            }

            int selected = _playlistList.SelectedIndex;
            _playlistList.BeginUpdate();
            try
            {
                _playlistList.Items.Clear();
                for (int i = 0; i < _settings.MusicPlaylist.Count; i++)
                {
                    MediaItem item = _settings.MusicPlaylist[i];
                    _playlistList.Items.Add(string.IsNullOrEmpty(item.Name) ? Path.GetFileName(item.File) : item.Name);
                }
                if (selected >= 0 && selected < _playlistList.Items.Count)
                {
                    _playlistList.SelectedIndex = selected;
                }
            }
            finally
            {
                _playlistList.EndUpdate();
            }
        }

        private void AddMusicEntry()
        {
            StopMusic();
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Title = "Добавить музыку в расписание";
                dialog.Filter = "Аудиофайлы (*.mp3;*.wav;*.wma;*.m4a)|*.mp3;*.wav;*.wma;*.m4a|MP3 (*.mp3)|*.mp3|Все файлы (*.*)|*.*";
                dialog.CheckFileExists = true;
                dialog.InitialDirectory = _store.DataDir;
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                string error;
                string imported = _store.ImportMedia(dialog.FileName, "music_", out error);
                if (imported == null)
                {
                    MessageBox.Show(this, "Не удалось добавить музыку.\n\n" + error,
                        "Школьные звонки", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                TimeSpan start = new TimeSpan(8, 0, 0);
                TimeSpan end = new TimeSpan(8, 10, 0);
                if (_settings.MusicSchedule.Count > 0)
                {
                    MusicEntry last = _settings.MusicSchedule[_settings.MusicSchedule.Count - 1];
                    start = last.End + TimeSpan.FromMinutes(5);
                    end = start + TimeSpan.FromMinutes(10);
                    if (end >= TimeSpan.FromDays(1))
                    {
                        start = new TimeSpan(8, 0, 0);
                        end = new TimeSpan(8, 10, 0);
                    }
                }

                MusicEntry entry = new MusicEntry(7, start, end, imported, Path.GetFileName(dialog.FileName), "");
                _settings.MusicSchedule.Add(entry);
                _store.CleanupMedia(_settings);
                RefreshMusicGrid();
                SaveNow(false);
                ShowStatus("Музыка добавлена. Укажите день и время.");
                if (_musicGrid.Rows.Count > 0)
                {
                    _musicGrid.CurrentCell = _musicGrid.Rows[_musicGrid.Rows.Count - 1].Cells[0];
                }
            }
        }

        private void DeleteMusicEntry()
        {
            int row = _musicGrid == null || _musicGrid.CurrentCell == null ? -1 : _musicGrid.CurrentCell.RowIndex;
            if (row < 0 || row >= _settings.MusicSchedule.Count)
            {
                ShowStatus("Выберите запись в расписании музыки.");
                return;
            }

            _settings.MusicSchedule.RemoveAt(row);
            _store.CleanupMedia(_settings);
            RefreshMusicGrid();
            SaveNow(false);
            ShowStatus("Запись удалена.");
        }

        private void ClearMusicEntries()
        {
            if (_settings.MusicSchedule.Count == 0)
            {
                ShowStatus("Расписание музыки уже пусто.");
                return;
            }

            if (MessageBox.Show(this, "Удалить все записи музыки по расписанию?",
                "Музыка", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            _settings.MusicSchedule.Clear();
            _store.CleanupMedia(_settings);
            RefreshMusicGrid();
            SaveNow(false);
            ShowStatus("Расписание музыки очищено.");
        }

        private void ChangeMusicFile()
        {
            int row = _musicGrid == null || _musicGrid.CurrentCell == null ? -1 : _musicGrid.CurrentCell.RowIndex;
            if (row < 0 || row >= _settings.MusicSchedule.Count)
            {
                ShowStatus("Выберите запись в расписании музыки.");
                return;
            }

            StopMusic();
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Title = "Выбрать музыку";
                dialog.Filter = "Аудиофайлы (*.mp3;*.wav;*.wma;*.m4a)|*.mp3;*.wav;*.wma;*.m4a|Все файлы (*.*)|*.*";
                dialog.CheckFileExists = true;
                dialog.InitialDirectory = _store.DataDir;
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                string error;
                string imported = _store.ImportMedia(dialog.FileName, "music_", out error);
                if (imported == null)
                {
                    MessageBox.Show(this, "Не удалось заменить файл.\n\n" + error,
                        "Школьные звонки", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                MusicEntry entry = _settings.MusicSchedule[row];
                entry.File = imported;
                entry.Name = Path.GetFileName(dialog.FileName);
                _store.CleanupMedia(_settings);
                RefreshMusicGrid();
                SaveNow(false);
                ShowStatus("Файл заменён: " + entry.Name);
            }
        }

        private void MusicGridCellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            if (_loadingMusicGrid || e.RowIndex < 0)
            {
                return;
            }

            string column = _musicGrid.Columns[e.ColumnIndex].Name;
            string text = e.FormattedValue == null ? "" : e.FormattedValue.ToString().Trim();
            if (column == "start" || column == "end")
            {
                TimeSpan time;
                if (!TimeUtil.TryParseTime(text, out time))
                {
                    e.Cancel = true;
                    _musicGrid.Rows[e.RowIndex].Cells[e.ColumnIndex].ErrorText = "Введите время: ЧЧ:ММ или 0800";
                    ShowStatus("Неверный формат времени. Используйте ЧЧ:ММ или 0800.");
                }
                else
                {
                    _musicGrid.Rows[e.RowIndex].Cells[e.ColumnIndex].ErrorText = "";
                }
            }
        }

        private void MusicGridCellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (_loadingMusicGrid || e.RowIndex < 0)
            {
                return;
            }

            string column = _musicGrid.Columns[e.ColumnIndex].Name;
            if (column == "start" || column == "end")
            {
                string text = _musicGrid.Rows[e.RowIndex].Cells[e.ColumnIndex].Value == null
                    ? ""
                    : _musicGrid.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString().Trim();
                TimeSpan time;
                if (TimeUtil.TryParseTime(text, out time))
                {
                    _loadingMusicGrid = true;
                    try
                    {
                        _musicGrid.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = TimeUtil.FormatTime(time);
                    }
                    finally
                    {
                        _loadingMusicGrid = false;
                    }
                }
            }

            if (TryUpdateMusicRow(e.RowIndex))
            {
                SaveSoon();
            }
        }

        private bool TryUpdateMusicRow(int row)
        {
            if (_loadingMusicGrid || row < 0 || row >= _settings.MusicSchedule.Count || row >= _musicGrid.Rows.Count)
            {
                return true;
            }

            string dayText = _musicGrid.Rows[row].Cells[0].Value == null
                ? ""
                : _musicGrid.Rows[row].Cells[0].Value.ToString().Trim();
            int day = Array.IndexOf(MusicDayLabels, dayText);
            if (day < 0)
            {
                day = 7;
            }

            TimeSpan start;
            TimeSpan end;
            string startText = _musicGrid.Rows[row].Cells[1].Value == null
                ? ""
                : _musicGrid.Rows[row].Cells[1].Value.ToString().Trim();
            string endText = _musicGrid.Rows[row].Cells[2].Value == null
                ? ""
                : _musicGrid.Rows[row].Cells[2].Value.ToString().Trim();
            if (!TimeUtil.TryParseTime(startText, out start) ||
                !TimeUtil.TryParseTime(endText, out end) ||
                end <= start)
            {
                ShowStatus("Проверьте время музыки: конец должен быть позже начала.");
                return false;
            }

            MusicEntry entry = _settings.MusicSchedule[row];
            entry.Day = day;
            entry.Start = start;
            entry.End = end;
            entry.Comment = _musicGrid.Rows[row].Cells[4].Value == null
                ? ""
                : _musicGrid.Rows[row].Cells[4].Value.ToString().Trim();
            return true;
        }

        private void AddPlaylistFiles()
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Title = "Добавить музыку в ручной список";
                dialog.Filter = "Аудиофайлы (*.mp3;*.wav;*.wma;*.m4a)|*.mp3;*.wav;*.wma;*.m4a|Все файлы (*.*)|*.*";
                dialog.CheckFileExists = true;
                dialog.Multiselect = true;
                dialog.InitialDirectory = _store.DataDir;
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                int added = 0;
                for (int i = 0; i < dialog.FileNames.Length; i++)
                {
                    string error;
                    string imported = _store.ImportMedia(dialog.FileNames[i], "music_", out error);
                    if (imported != null)
                    {
                        _settings.MusicPlaylist.Add(new MediaItem(imported, Path.GetFileName(dialog.FileNames[i])));
                        added++;
                    }
                }

                _store.CleanupMedia(_settings);
                RefreshPlaylistList();
                SaveNow(false);
                ShowStatus("Добавлено файлов: " + added.ToString() + ".");
            }
        }

        private void RemovePlaylistItem()
        {
            int index = _playlistList == null ? -1 : _playlistList.SelectedIndex;
            if (index < 0 || index >= _settings.MusicPlaylist.Count)
            {
                ShowStatus("Выберите файл в списке.");
                return;
            }

            _settings.MusicPlaylist.RemoveAt(index);
            _store.CleanupMedia(_settings);
            RefreshPlaylistList();
            SaveNow(false);
            ShowStatus("Файл удалён из списка.");
        }

        private void PlaySelectedPlaylistMusic()
        {
            int index = _playlistList == null ? -1 : _playlistList.SelectedIndex;
            if (index < 0 && _settings.MusicPlaylist.Count > 0)
            {
                index = 0;
            }
            if (index < 0 || index >= _settings.MusicPlaylist.Count)
            {
                ShowStatus("Добавьте музыку в ручной список.");
                return;
            }

            MediaItem item = _settings.MusicPlaylist[index];
            string path = _store.ResolveMelodyPath(item.File);
            string error;
            if (PlayMusicNow(path, item.Name, out error))
            {
                ShowStatus("Музыка включена: " + item.Name);
            }
            else
            {
                ShowStatus("Не удалось включить музыку.");
                MessageBox.Show(this, "Не удалось воспроизвести музыку.\n\n" + error,
                    "Школьные звонки", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void PlaySelectedScheduledMusic()
        {
            int row = _musicGrid == null || _musicGrid.CurrentCell == null ? -1 : _musicGrid.CurrentCell.RowIndex;
            if (row < 0 || row >= _settings.MusicSchedule.Count)
            {
                ShowStatus("Выберите запись в расписании музыки.");
                return;
            }

            MusicEntry entry = _settings.MusicSchedule[row];
            string path = _store.ResolveMelodyPath(entry.File);
            string error;
            if (PlayMusicNow(path, entry.Name, out error))
            {
                ShowStatus("Проверка музыки: " + entry.Name);
            }
            else
            {
                ShowStatus("Не удалось воспроизвести музыку.");
                MessageBox.Show(this, "Не удалось воспроизвести музыку.\n\n" + error,
                    "Школьные звонки", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void MusicEnabledChanged(object sender, EventArgs e)
        {
            if (_updatingControls || _musicEnabledCheck == null)
            {
                return;
            }

            _settings.MusicEnabled = _musicEnabledCheck.Checked;
            if (!_settings.MusicEnabled)
            {
                if (_musicStopAt != DateTime.MinValue)
                {
                    StopMusic();
                }
                ShowStatus("Музыка по расписанию выключена.");
            }
            else
            {
                ShowStatus("Музыка по расписанию включена.");
            }
            SaveSoon();
        }

        private void MusicVolumeChanged(object sender, EventArgs e)
        {
            if (_updatingControls || _musicVolumeTrack == null)
            {
                return;
            }

            _settings.MusicVolume = _musicVolumeTrack.Value;
            _musicVolumeLabel.Text = _settings.MusicVolume.ToString() + " %";
            _musicPlayback.Volume = _settings.MusicVolume;
            SaveSoon();
        }

        private bool PlayMusicNow(string path, string name, out string error)
        {
            error = null;
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                error = "Файл музыки не найден.";
                return false;
            }

            StopLiveMicrophone();
            if (_recording)
            {
                CancelVoiceRecording();
            }
            _playback.Stop();
            _voicePlayback.Stop();
            _musicPlayback.Stop();
            _musicPlayback.Volume = _settings.MusicVolume;
            if (!_musicPlayback.Play(path, null, out error))
            {
                return false;
            }

            _musicStopAt = DateTime.MinValue;
            _musicNowName = name == null ? "" : name;
            return true;
        }

        private void CheckMusic(DateTime now)
        {
            if (!_settings.MusicEnabled)
            {
                if (_musicStopAt != DateTime.MinValue)
                {
                    StopMusic();
                }
                return;
            }

            if (_musicStopAt != DateTime.MinValue && now >= _musicStopAt)
            {
                StopMusic();
            }

            int weekday = ScheduleEngine.DayIndex(now);
            MusicEntry chosen = null;
            for (int i = 0; i < _settings.MusicSchedule.Count; i++)
            {
                MusicEntry entry = _settings.MusicSchedule[i];
                if (!MusicEngine.IsDue(entry, weekday, now.TimeOfDay, 5))
                {
                    continue;
                }
                if (!_playedMusic.Contains(MusicKey(now, entry)))
                {
                    chosen = entry;
                    break;
                }
            }

            if (chosen == null)
            {
                return;
            }

            for (int i = 0; i < _settings.MusicSchedule.Count; i++)
            {
                MusicEntry entry = _settings.MusicSchedule[i];
                if (MusicEngine.IsDue(entry, weekday, now.TimeOfDay, 5))
                {
                    _playedMusic.Add(MusicKey(now, entry));
                }
            }

            string path = _store.ResolveMelodyPath(chosen.File);
            string error;
            if (!PlayMusicNow(path, chosen.Name, out error))
            {
                ShowStatus("Не удалось включить музыку: " + chosen.Name + ". " + error);
                return;
            }

            _musicStopAt = now.Date + chosen.End;
            if (_musicStopAt <= now)
            {
                _musicStopAt = now.AddMinutes(10);
            }
            ShowStatus("Музыка: " + chosen.Name);
        }

        private static string MusicKey(DateTime now, MusicEntry entry)
        {
            return now.ToString("yyyyMMdd") + "|" + entry.Day.ToString() + "|" +
                   TimeUtil.FormatTime(entry.Start) + "|" + entry.File;
        }

        private void StopMusic()
        {
            _musicStopAt = DateTime.MinValue;
            _musicNowName = "";
            if (_musicPlayback != null)
            {
                _musicPlayback.Stop();
            }
        }
    }
}
