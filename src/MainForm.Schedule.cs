using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace SchoolBells
{
    public sealed partial class MainForm
    {
        private ListBox _dayList;
        private Label _todayLabel;
        private Label _daySummaryLabel;
        private DataGridView _grid;
        private Button _addButton;
        private Button _deleteButton;
        private Button _upButton;
        private Button _downButton;
        private Button _sortButton;
        private Button _copyButton;
        private Button _clearButton;
        private int _currentDayIndex = -1;
        private bool _loadingGrid;
        private bool _updatingDayList;
        private CheckBox[] _shortChecks;
        private bool _updatingShortDays;

        private TabPage BuildScheduleTab()
        {
            TabPage page = new TabPage("Расписание");
            page.BackColor = PageBack;
            page.Padding = new Padding(8);

            SplitContainer split = new SplitContainer();
            split.Dock = DockStyle.Fill;
            split.SplitterWidth = 6;
            split.Panel1MinSize = 0;
            split.Panel2MinSize = 0;
            split.BackColor = BorderColor;
            split.Panel1.BackColor = PageBack;
            split.Panel2.BackColor = PageBack;
            bool splitterReady = false;
            split.SizeChanged += delegate
            {
                if (!splitterReady && split.Width >= 700)
                {
                    splitterReady = true;
                    split.Panel1MinSize = 210;
                    split.Panel2MinSize = 420;
                    int desired = 238;
                    int maximum = split.Width - split.Panel2MinSize - split.SplitterWidth;
                    if (desired > maximum)
                    {
                        desired = maximum;
                    }
                    if (desired < split.Panel1MinSize)
                    {
                        desired = split.Panel1MinSize;
                    }
                    if (desired > 0)
                    {
                        split.SplitterDistance = desired;
                    }
                }
            };
            page.Controls.Add(split);

            TableLayoutPanel left = new TableLayoutPanel();
            left.Dock = DockStyle.Fill;
            left.ColumnCount = 1;
            left.RowCount = 5;
            left.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            left.RowStyles.Add(new RowStyle(SizeType.Absolute, 22f));
            left.RowStyles.Add(new RowStyle(SizeType.Absolute, 24f));
            left.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            left.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            left.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            left.Padding = new Padding(0, 0, 6, 0);
            split.Panel1.Controls.Add(left);

            Label daysCaption = new Label();
            daysCaption.Text = "ДНИ НЕДЕЛИ";
            daysCaption.Dock = DockStyle.Fill;
            daysCaption.TextAlign = ContentAlignment.MiddleLeft;
            daysCaption.ForeColor = HintText;
            daysCaption.Font = new Font("Segoe UI", 8.5f, FontStyle.Bold, GraphicsUnit.Point);
            left.Controls.Add(daysCaption, 0, 0);

            _todayLabel = new Label();
            _todayLabel.Text = "Сегодня — понедельник";
            _todayLabel.Dock = DockStyle.Fill;
            _todayLabel.TextAlign = ContentAlignment.MiddleLeft;
            _todayLabel.ForeColor = Accent;
            _todayLabel.Font = new Font("Segoe UI", 9f, FontStyle.Regular, GraphicsUnit.Point);
            _todayLabel.AutoEllipsis = true;
            left.Controls.Add(_todayLabel, 0, 1);

            _dayList = new ListBox();
            _dayList.Dock = DockStyle.Fill;
            _dayList.IntegralHeight = false;
            _dayList.BorderStyle = BorderStyle.FixedSingle;
            _dayList.Font = new Font("Segoe UI", 10.5f, FontStyle.Regular, GraphicsUnit.Point);
            _dayList.Margin = new Padding(0, 4, 0, 8);
            _dayList.SelectedIndexChanged += DayListSelectedIndexChanged;
            left.Controls.Add(_dayList, 0, 2);

            TableLayoutPanel dayButtons = new TableLayoutPanel();
            dayButtons.Dock = DockStyle.Top;
            dayButtons.AutoSize = true;
            dayButtons.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            dayButtons.ColumnCount = 1;
            dayButtons.RowCount = 2;
            dayButtons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            dayButtons.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            dayButtons.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            left.Controls.Add(dayButtons, 0, 3);

            _copyButton = MakeButton("Копировать день…", 200, 34, false);
            _copyButton.Dock = DockStyle.Top;
            _copyButton.Margin = new Padding(0, 3, 0, 3);
            _copyButton.Click += delegate { CopyCurrentDay(); };
            dayButtons.Controls.Add(_copyButton, 0, 0);

            _clearButton = MakeButton("Очистить день", 200, 34, false);
            _clearButton.Dock = DockStyle.Top;
            _clearButton.Margin = new Padding(0, 3, 0, 3);
            _clearButton.Click += delegate { ClearCurrentDay(); };
            dayButtons.Controls.Add(_clearButton, 0, 1);

            left.Controls.Add(BuildShortDayGroup(), 0, 4);

            _tips.SetToolTip(_copyButton, "Скопировать расписание этого дня в другие дни недели");
            _tips.SetToolTip(_clearButton, "Удалить все звонки выбранного дня");

            TableLayoutPanel right = new TableLayoutPanel();
            right.Dock = DockStyle.Fill;
            right.ColumnCount = 1;
            right.RowCount = 3;
            right.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            right.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            right.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            right.RowStyles.Add(new RowStyle(SizeType.Absolute, 30f));
            right.Padding = new Padding(8, 0, 0, 0);
            split.Panel2.Controls.Add(right);

            FlowLayoutPanel toolbar = new FlowLayoutPanel();
            toolbar.Dock = DockStyle.Top;
            toolbar.AutoSize = true;
            toolbar.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            toolbar.WrapContents = true;
            toolbar.FlowDirection = FlowDirection.LeftToRight;
            toolbar.Padding = new Padding(0, 0, 0, 8);
            right.Controls.Add(toolbar, 0, 0);

            _addButton = MakeButton("Добавить", 94, 32, true);
            _addButton.Click += delegate { AddBell(); };
            toolbar.Controls.Add(_addButton);

            _deleteButton = MakeButton("Удалить", 90, 32, false);
            _deleteButton.Click += delegate { DeleteBell(); };
            toolbar.Controls.Add(_deleteButton);

            _upButton = MakeButton("↑ Выше", 82, 32, false);
            _upButton.Click += delegate { MoveBell(-1); };
            toolbar.Controls.Add(_upButton);

            _downButton = MakeButton("↓ Ниже", 82, 32, false);
            _downButton.Click += delegate { MoveBell(1); };
            toolbar.Controls.Add(_downButton);

            _sortButton = MakeButton("Сортировать", 112, 32, false);
            _sortButton.Click += delegate { SortBells(); };
            toolbar.Controls.Add(_sortButton);

            _tips.SetToolTip(_addButton, "Добавить звонок в выбранный день");
            _tips.SetToolTip(_deleteButton, "Удалить выбранный звонок (или клавиша Delete)");
            _tips.SetToolTip(_upButton, "Переместить звонок выше");
            _tips.SetToolTip(_downButton, "Переместить звонок ниже");
            _tips.SetToolTip(_sortButton, "Упорядочить звонки по времени начала");

            Panel gridHost = new Panel();
            gridHost.Dock = DockStyle.Fill;
            gridHost.BackColor = PanelBack;
            gridHost.Padding = new Padding(0, 0, 0, 2);
            right.Controls.Add(gridHost, 0, 1);

            _grid = new BufferedDataGridView();
            _grid.Dock = DockStyle.Fill;
            _grid.BackgroundColor = GridBack;
            _grid.BorderStyle = BorderStyle.FixedSingle;
            _grid.AllowUserToAddRows = false;
            _grid.AllowUserToDeleteRows = false;
            _grid.AllowUserToResizeRows = false;
            _grid.RowHeadersVisible = false;
            _grid.MultiSelect = false;
            _grid.SelectionMode = DataGridViewSelectionMode.CellSelect;
            _grid.EditMode = DataGridViewEditMode.EditOnKeystrokeOrF2;
            _grid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
            _grid.EnableHeadersVisualStyles = false;
            _grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            _grid.CellBorderStyle = DataGridViewCellBorderStyle.Single;
            _grid.GridColor = GridLine;
            _grid.RowTemplate.Height = 30;
            _grid.ColumnHeadersHeight = 34;
            _grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            _grid.Font = new Font("Segoe UI", 10.5f, FontStyle.Regular, GraphicsUnit.Point);
            _grid.DefaultCellStyle.Font = new Font("Segoe UI", 10.5f, FontStyle.Regular, GraphicsUnit.Point);
            _grid.DefaultCellStyle.SelectionBackColor = GridSelectionBack;
            _grid.DefaultCellStyle.SelectionForeColor = Color.Black;
            _grid.DefaultCellStyle.Padding = new Padding(3, 0, 3, 0);
            _grid.AlternatingRowsDefaultCellStyle.BackColor = GridAltBack;
            _grid.ColumnHeadersDefaultCellStyle.BackColor = GridHeaderBack;
            _grid.ColumnHeadersDefaultCellStyle.ForeColor = GridHeaderText;
            _grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = GridHeaderBack;
            _grid.ColumnHeadersDefaultCellStyle.SelectionForeColor = GridHeaderText;
            _grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold, GraphicsUnit.Point);
            _grid.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            _grid.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
            _grid.CellValidating += GridCellValidating;
            _grid.CellEndEdit += GridCellEndEdit;
            _grid.RowValidating += GridRowValidating;
            _grid.DataError += delegate(object sender, DataGridViewDataErrorEventArgs e)
            {
                e.ThrowException = false;
                ShowStatus("Не удалось прочитать значение ячейки.");
            };
            _grid.KeyDown += GridKeyDown;
            gridHost.Controls.Add(_grid);

            DataGridViewTextBoxColumn numberColumn = new DataGridViewTextBoxColumn();
            numberColumn.Name = "number";
            numberColumn.HeaderText = "№";
            numberColumn.Width = 44;
            numberColumn.ReadOnly = true;
            numberColumn.SortMode = DataGridViewColumnSortMode.NotSortable;
            numberColumn.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            numberColumn.DefaultCellStyle.ForeColor = HintText;
            numberColumn.DefaultCellStyle.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold, GraphicsUnit.Point);
            _grid.Columns.Add(numberColumn);

            DataGridViewTextBoxColumn startColumn = new DataGridViewTextBoxColumn();
            startColumn.Name = "start";
            startColumn.HeaderText = "Начало";
            startColumn.Width = 92;
            startColumn.MaxInputLength = 8;
            startColumn.SortMode = DataGridViewColumnSortMode.NotSortable;
            startColumn.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            _grid.Columns.Add(startColumn);

            DataGridViewTextBoxColumn endColumn = new DataGridViewTextBoxColumn();
            endColumn.Name = "end";
            endColumn.HeaderText = "Конец";
            endColumn.Width = 92;
            endColumn.MaxInputLength = 8;
            endColumn.SortMode = DataGridViewColumnSortMode.NotSortable;
            endColumn.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            _grid.Columns.Add(endColumn);

            DataGridViewTextBoxColumn commentColumn = new DataGridViewTextBoxColumn();
            commentColumn.Name = "comment";
            commentColumn.HeaderText = "Комментарий";
            commentColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            commentColumn.MinimumWidth = 120;
            commentColumn.MaxInputLength = 200;
            commentColumn.SortMode = DataGridViewColumnSortMode.NotSortable;
            _grid.Columns.Add(commentColumn);

            _daySummaryLabel = new Label();
            _daySummaryLabel.Dock = DockStyle.Fill;
            _daySummaryLabel.TextAlign = ContentAlignment.MiddleLeft;
            _daySummaryLabel.ForeColor = HintText;
            _daySummaryLabel.Font = new Font("Segoe UI", 9.25f, FontStyle.Regular, GraphicsUnit.Point);
            _daySummaryLabel.AutoEllipsis = true;
            right.Controls.Add(_daySummaryLabel, 0, 2);

            UpdateTodayLabel();
            return page;
        }

        private GroupBox BuildShortDayGroup()
        {
            GroupBox group = new GroupBox();
            group.Text = "Короткий день";
            group.Dock = DockStyle.Top;
            group.AutoSize = true;
            group.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            group.Font = new Font("Segoe UI", 9f, FontStyle.Bold, GraphicsUnit.Point);
            group.ForeColor = TextColor;
            group.BackColor = PanelBack;
            group.Padding = new Padding(8, 20, 8, 8);
            group.Margin = new Padding(0, 6, 0, 0);

            TableLayoutPanel grid = new TableLayoutPanel();
            grid.Dock = DockStyle.Top;
            grid.AutoSize = true;
            grid.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            grid.ColumnCount = 4;
            grid.RowCount = 3;
            grid.BackColor = PanelBack;
            for (int column = 0; column < 4; column++)
            {
                grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
            }
            grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 24f));
            grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 24f));
            grid.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            group.Controls.Add(grid);

            _shortChecks = new CheckBox[7];
            _updatingShortDays = true;
            try
            {
                for (int day = 0; day < 7; day++)
                {
                    CheckBox check = new CheckBox();
                    check.Text = ScheduleEngine.DayNameShort(day);
                    check.AutoSize = true;
                    check.Font = new Font("Segoe UI", 9f, FontStyle.Regular, GraphicsUnit.Point);
                    check.ForeColor = TextColor;
                    check.Checked = _settings.IsShortDay(day);
                    check.Tag = day;
                    check.Margin = new Padding(2, 2, 2, 2);
                    check.CheckedChanged += ShortDayChanged;
                    _shortChecks[day] = check;
                    grid.Controls.Add(check, day % 4, day / 4);
                }

                Label hint = MakeHintLabel("Отметьте дни, когда действует шаблон «Короткий день».");
                hint.Dock = DockStyle.Fill;
                hint.TextAlign = ContentAlignment.TopLeft;
                hint.Padding = new Padding(2, 2, 0, 0);
                grid.Controls.Add(hint, 0, 2);
                grid.SetColumnSpan(hint, 4);
            }
            finally
            {
                _updatingShortDays = false;
            }

            return group;
        }

        private void ShortDayChanged(object sender, EventArgs e)
        {
            if (_updatingShortDays || _settings == null)
            {
                return;
            }

            CheckBox check = sender as CheckBox;
            if (check == null || check.Tag == null)
            {
                return;
            }

            int day = (int)check.Tag;
            _settings.SetShortDay(day, check.Checked);
            SaveSoon();
            RefreshDayList();
            if (_currentDayIndex >= 0 && _currentDayIndex <= 6)
            {
                LoadDay(_currentDayIndex);
            }
            UpdateHeader(DateTime.Now);
            ShowStatus(check.Checked
                ? ScheduleEngine.DayName(day) + " отмечен как короткий день."
                : ScheduleEngine.DayName(day) + " снова использует обычное расписание.");
        }

        private void RefreshDayList()
        {
            if (_dayList == null)
            {
                return;
            }

            _updatingDayList = true;
            int selected = _dayList.SelectedIndex;
            if (selected < 0)
            {
                selected = _currentDayIndex >= 0 ? _currentDayIndex : ScheduleEngine.DayIndex(DateTime.Now);
            }

            _dayList.BeginUpdate();
            try
            {
                _dayList.Items.Clear();
                for (int day = 0; day < 8; day++)
                {
                    DaySchedule schedule = _settings.GetEffectiveDay(day);
                    int count = schedule == null ? 0 : schedule.Bells.Count;
                    string text = ScheduleEngine.DayName(day) +
                                  (count == 0 ? "  (пусто)" : "  (" + count.ToString() + ")");
                    if (day <= 6 && _settings.IsShortDay(day))
                    {
                        text += " · короткий";
                    }
                    _dayList.Items.Add(text);
                }
                if (selected < 0 || selected > 7)
                {
                    selected = 0;
                }
                _dayList.SelectedIndex = selected;
            }
            finally
            {
                _dayList.EndUpdate();
                _updatingDayList = false;
            }

            UpdateTodayLabel();
        }

        private void DayListSelectedIndexChanged(object sender, EventArgs e)
        {
            if (_updatingDayList || _dayList.SelectedIndex < 0)
            {
                return;
            }

            int newIndex = _dayList.SelectedIndex;
            if (newIndex == _currentDayIndex)
            {
                return;
            }

            if (_currentDayIndex >= 0 && !CommitGridEdits())
            {
                _updatingDayList = true;
                _dayList.SelectedIndex = _currentDayIndex;
                _updatingDayList = false;
                return;
            }

            LoadDay(newIndex);
        }

        private DaySchedule CurrentDaySchedule()
        {
            if (_currentDayIndex < 0 || _currentDayIndex > 7)
            {
                return null;
            }
            return _settings.GetEffectiveDay(_currentDayIndex);
        }

        private void LoadDay(int dayIndex)
        {
            if (dayIndex < 0 || dayIndex > 7)
            {
                return;
            }

            _currentDayIndex = dayIndex;
            DaySchedule schedule = _settings.GetEffectiveDay(dayIndex);
            List<Bell> bells = schedule == null ? new List<Bell>() : schedule.Bells;
            _loadingGrid = true;
            _grid.Rows.Clear();
            try
            {
                for (int i = 0; i < bells.Count; i++)
                {
                    Bell bell = bells[i];
                    _grid.Rows.Add(
                        (i + 1).ToString(),
                        TimeUtil.FormatTime(bell.Start),
                        TimeUtil.FormatTime(bell.End),
                        bell.Comment);
                }
            }
            finally
            {
                _loadingGrid = false;
            }

            if (_grid.Rows.Count > 0)
            {
                _grid.CurrentCell = _grid.Rows[0].Cells[1];
                _grid.ClearSelection();
                _grid.Rows[0].Cells[1].Selected = true;
            }

            UpdateDaySummary();
            UpdateTodayLabel();
        }

        private void UpdateDaySummary()
        {
            if (_daySummaryLabel == null)
            {
                return;
            }

            if (_currentDayIndex < 0)
            {
                _daySummaryLabel.Text = "";
                return;
            }

            DaySchedule day = _settings.GetEffectiveDay(_currentDayIndex);
            if (day == null)
            {
                _daySummaryLabel.Text = "";
                return;
            }

            string dayName = ScheduleEngine.DayName(_currentDayIndex);
            bool today = _currentDayIndex <= 6 && ScheduleEngine.DayIndex(DateTime.Now) == _currentDayIndex;
            string prefix;
            if (_currentDayIndex == 7)
            {
                prefix = "Шаблон короткого дня";
            }
            else if (today)
            {
                prefix = "Сегодня, " + dayName.ToLowerInvariant();
            }
            else
            {
                prefix = dayName;
            }

            if (_currentDayIndex <= 6 && _settings.IsShortDay(_currentDayIndex))
            {
                prefix += " · используется шаблон «Короткий день»";
            }

            if (day.Bells.Count == 0)
            {
                _daySummaryLabel.Text = prefix + ": звонков нет. Нажмите «Добавить», чтобы создать первый.";
                return;
            }

            TimeSpan first = day.Bells[0].Start;
            TimeSpan last = day.Bells[0].End;
            for (int i = 1; i < day.Bells.Count; i++)
            {
                if (day.Bells[i].Start < first)
                {
                    first = day.Bells[i].Start;
                }
                if (day.Bells[i].End > last)
                {
                    last = day.Bells[i].End;
                }
            }

            _daySummaryLabel.Text = prefix + ": " +
                day.Bells.Count.ToString() + " " +
                Plural(day.Bells.Count, "звонок", "звонка", "звонков") +
                " · " + TimeUtil.FormatTime(first) + "–" + TimeUtil.FormatTime(last);
        }

        private void UpdateTodayLabel()
        {
            if (_todayLabel == null)
            {
                return;
            }

            int today = ScheduleEngine.DayIndex(DateTime.Now);
            _todayLabel.Text = "Сегодня — " + ScheduleEngine.DayName(today).ToLowerInvariant() +
                (_settings.IsShortDay(today) ? " · короткий день" : "");
        }

        private void AddBell()
        {
            if (!CommitGridEdits())
            {
                return;
            }

            List<Bell> bells = CurrentDaySchedule().Bells;
            TimeSpan start;
            TimeSpan end;
            if (bells.Count == 0)
            {
                start = new TimeSpan(8, 0, 0);
                end = new TimeSpan(8, 45, 0);
            }
            else
            {
                Bell last = bells[bells.Count - 1];
                TimeSpan lesson = last.End - last.Start;
                if (lesson <= TimeSpan.Zero || lesson > TimeSpan.FromHours(3))
                {
                    lesson = TimeSpan.FromMinutes(45);
                }
                start = last.End + TimeSpan.FromMinutes(10);
                end = start + lesson;
            }

            if (end >= TimeSpan.FromDays(1))
            {
                MessageBox.Show(this,
                    "Не удалось добавить звонок: время выходит за пределы суток.\nИзмените время последнего урока.",
                    "Школьные звонки",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            bells.Add(new Bell(start, end, ""));
            LoadDay(_currentDayIndex);
            RefreshDayList();
            SaveSoon();
            ShowStatus("Звонок добавлен. Укажите время начала и конца.");

            int row = bells.Count - 1;
            if (row >= 0 && row < _grid.Rows.Count)
            {
                _grid.CurrentCell = _grid.Rows[row].Cells[1];
                _grid.Focus();
                _grid.BeginEdit(true);
            }
        }

        private void DeleteBell()
        {
            if (!CommitGridEdits())
            {
                return;
            }

            int row = _grid.CurrentCell == null ? -1 : _grid.CurrentCell.RowIndex;
            List<Bell> bells = CurrentDaySchedule().Bells;
            if (row < 0 || row >= bells.Count)
            {
                ShowStatus("Выберите звонок в таблице.");
                return;
            }

            bells.RemoveAt(row);
            LoadDay(_currentDayIndex);
            RefreshDayList();
            SaveSoon();
            ShowStatus("Звонок удалён.");

            if (bells.Count > 0)
            {
                int select = Math.Min(row, bells.Count - 1);
                _grid.CurrentCell = _grid.Rows[select].Cells[1];
                _grid.Focus();
            }
        }

        private void MoveBell(int direction)
        {
            if (!CommitGridEdits())
            {
                return;
            }

            int row = _grid.CurrentCell == null ? -1 : _grid.CurrentCell.RowIndex;
            List<Bell> bells = CurrentDaySchedule().Bells;
            int target = row + direction;
            if (row < 0 || row >= bells.Count || target < 0 || target >= bells.Count)
            {
                return;
            }

            Bell temp = bells[row];
            bells[row] = bells[target];
            bells[target] = temp;
            LoadDay(_currentDayIndex);
            RefreshDayList();
            SaveSoon();
            _grid.CurrentCell = _grid.Rows[target].Cells[1];
            _grid.Focus();
            ShowStatus("Звонок перемещён.");
        }

        private void SortBells()
        {
            if (!CommitGridEdits())
            {
                return;
            }

            CurrentDaySchedule().Bells.Sort(delegate(Bell a, Bell b)
            {
                int result = a.Start.CompareTo(b.Start);
                if (result != 0)
                {
                    return result;
                }
                return a.End.CompareTo(b.End);
            });
            LoadDay(_currentDayIndex);
            SaveSoon();
            ShowStatus("Звонки упорядочены по времени начала.");
        }

        private void CopyCurrentDay()
        {
            if (!CommitGridEdits())
            {
                return;
            }

            DaySchedule source = CurrentDaySchedule();
            if (source.Bells.Count == 0)
            {
                DialogResult empty = MessageBox.Show(this,
                    "В дне «" + ScheduleEngine.DayName(_currentDayIndex) + "» нет звонков.\n" +
                    "Скопировать пустое расписание и очистить выбранные дни?",
                    "Копировать день",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);
                if (empty != DialogResult.Yes)
                {
                    return;
                }
            }

            using (CopyDaysForm dialog = new CopyDaysForm(_currentDayIndex, _settings))
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                List<int> targets = dialog.SelectedDays;
                if (targets.Count == 0)
                {
                    return;
                }

                bool overwrite = false;
                for (int i = 0; i < targets.Count; i++)
                {
                    DaySchedule targetDay = _settings.GetEffectiveDay(targets[i]);
                    if (targetDay != null && targetDay.Bells.Count > 0)
                    {
                        overwrite = true;
                        break;
                    }
                }

                if (overwrite)
                {
                    DialogResult confirm = MessageBox.Show(this,
                        "В выбранных днях уже есть расписание. Заменить его расписанием дня «" +
                        ScheduleEngine.DayName(_currentDayIndex) + "»?",
                        "Копировать день",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);
                    if (confirm != DialogResult.Yes)
                    {
                        return;
                    }
                }

                bool shortDayCopied = false;
                for (int i = 0; i < targets.Count; i++)
                {
                    int target = targets[i];
                    if (target == 7 || _settings.IsShortDay(target))
                    {
                        if (!shortDayCopied)
                        {
                            _settings.ShortDay = source.Clone();
                            shortDayCopied = true;
                        }
                    }
                    else
                    {
                        _settings.Days[target] = source.Clone();
                    }
                }

                RefreshDayList();
                UpdateDaySummary();
                UpdateHeader(DateTime.Now);
                SaveNow(true);

                List<string> names = new List<string>();
                for (int i = 0; i < targets.Count; i++)
                {
                    names.Add(ScheduleEngine.DayName(targets[i]));
                }
                ShowStatus("Расписание скопировано: " + string.Join(", ", names.ToArray()) + ".");
            }
        }

        private void ClearCurrentDay()
        {
            if (!CommitGridEdits())
            {
                return;
            }

            List<Bell> bells = CurrentDaySchedule().Bells;
            if (bells.Count == 0)
            {
                ShowStatus("В этом дне уже нет звонков.");
                return;
            }

            DialogResult confirm = MessageBox.Show(this,
                "Удалить все звонки дня «" + ScheduleEngine.DayName(_currentDayIndex) + "»?",
                "Очистить день",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes)
            {
                return;
            }

            bells.Clear();
            LoadDay(_currentDayIndex);
            RefreshDayList();
            UpdateHeader(DateTime.Now);
            SaveNow(true);
            ShowStatus("Расписание дня очищено.");
        }

        private void GridCellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            if (_loadingGrid || e.RowIndex < 0 || (e.ColumnIndex != 1 && e.ColumnIndex != 2))
            {
                return;
            }

            string text = e.FormattedValue == null ? "" : e.FormattedValue.ToString().Trim();
            TimeSpan time;
            if (!TimeUtil.TryParseTime(text, out time))
            {
                e.Cancel = true;
                _grid.Rows[e.RowIndex].Cells[e.ColumnIndex].ErrorText = "Введите время: ЧЧ:ММ или 0800";
                ShowStatus("Неверный формат времени. Используйте ЧЧ:ММ, 0800 или 800, например 08:00.");
            }
            else
            {
                _grid.Rows[e.RowIndex].Cells[e.ColumnIndex].ErrorText = "";
            }
        }

        private void GridCellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (_loadingGrid || e.RowIndex < 0)
            {
                return;
            }

            if (e.ColumnIndex == 1 || e.ColumnIndex == 2)
            {
                string text = _grid.Rows[e.RowIndex].Cells[e.ColumnIndex].Value == null
                    ? ""
                    : _grid.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString().Trim();
                TimeSpan time;
                if (TimeUtil.TryParseTime(text, out time))
                {
                    _loadingGrid = true;
                    try
                    {
                        _grid.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = TimeUtil.FormatTime(time);
                    }
                    finally
                    {
                        _loadingGrid = false;
                    }
                }
            }

            if (TryUpdateRow(e.RowIndex))
            {
                SaveSoon();
                UpdateDaySummary();
            }
        }

        private void GridRowValidating(object sender, DataGridViewCellCancelEventArgs e)
        {
            if (_loadingGrid || e.RowIndex < 0)
            {
                return;
            }

            if (!TryUpdateRow(e.RowIndex))
            {
                e.Cancel = true;
            }
        }

        private bool TryUpdateRow(int rowIndex)
        {
            if (_loadingGrid || _currentDayIndex < 0 || rowIndex < 0 || rowIndex >= _grid.Rows.Count)
            {
                return true;
            }

            List<Bell> bells = CurrentDaySchedule().Bells;
            if (rowIndex >= bells.Count)
            {
                return true;
            }

            TimeSpan start;
            TimeSpan end;
            string message;
            if (!TryReadRowTimes(rowIndex, out start, out end, out message))
            {
                SetRowError(rowIndex, message);
                ShowStatus(message);
                return false;
            }

            ClearRowError(rowIndex);
            Bell bell = bells[rowIndex];
            bell.Start = start;
            bell.End = end;
            bell.Comment = _grid.Rows[rowIndex].Cells[3].Value == null
                ? ""
                : _grid.Rows[rowIndex].Cells[3].Value.ToString().Trim();
            return true;
        }

        private bool TryReadRowTimes(int rowIndex, out TimeSpan start, out TimeSpan end, out string message)
        {
            start = TimeSpan.Zero;
            end = TimeSpan.Zero;
            message = null;

            string startText = _grid.Rows[rowIndex].Cells[1].Value == null
                ? ""
                : _grid.Rows[rowIndex].Cells[1].Value.ToString().Trim();
            string endText = _grid.Rows[rowIndex].Cells[2].Value == null
                ? ""
                : _grid.Rows[rowIndex].Cells[2].Value.ToString().Trim();

            if (!TimeUtil.TryParseTime(startText, out start))
            {
                message = "Введите начало урока: ЧЧ:ММ или 0800.";
                return false;
            }
            if (!TimeUtil.TryParseTime(endText, out end))
            {
                message = "Введите конец урока: ЧЧ:ММ или 0800.";
                return false;
            }
            if (end <= start)
            {
                message = "Конец урока должен быть позже начала.";
                return false;
            }
            return true;
        }

        private void SetRowError(int rowIndex, string message)
        {
            for (int column = 1; column <= 2; column++)
            {
                DataGridViewCell cell = _grid.Rows[rowIndex].Cells[column];
                cell.ErrorText = message;
                cell.Style.BackColor = Color.FromArgb(255, 235, 235);
            }
        }

        private void ClearRowError(int rowIndex)
        {
            for (int column = 1; column <= 2; column++)
            {
                DataGridViewCell cell = _grid.Rows[rowIndex].Cells[column];
                cell.ErrorText = "";
                cell.Style.BackColor = Color.Empty;
            }
        }

        private bool CommitGridEdits()
        {
            if (_grid == null)
            {
                return true;
            }

            if (_grid.IsCurrentCellInEditMode)
            {
                if (!_grid.EndEdit())
                {
                    return false;
                }
            }

            if (_currentDayIndex < 0 || _grid.CurrentCell == null || _grid.CurrentCell.RowIndex < 0)
            {
                return true;
            }

            return TryUpdateRow(_grid.CurrentCell.RowIndex);
        }

        private bool ValidateGridForClose()
        {
            if (_grid == null || _currentDayIndex < 0)
            {
                return true;
            }

            if (_grid.IsCurrentCellInEditMode)
            {
                _grid.EndEdit();
            }

            for (int row = 0; row < _grid.Rows.Count; row++)
            {
                if (!TryUpdateRow(row))
                {
                    return false;
                }
            }
            return true;
        }

        private void ShowGridError()
        {
            if (_grid == null)
            {
                return;
            }

            if (_tabs != null && _schedulePage != null)
            {
                _tabs.SelectedTab = _schedulePage;
            }

            for (int row = 0; row < _grid.Rows.Count; row++)
            {
                TimeSpan start;
                TimeSpan end;
                string message;
                if (!TryReadRowTimes(row, out start, out end, out message))
                {
                    if (row < _grid.Rows.Count)
                    {
                        _grid.CurrentCell = _grid.Rows[row].Cells[1];
                        _grid.Focus();
                    }
                    return;
                }
            }
        }

        private void GridKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete && !_grid.IsCurrentCellInEditMode)
            {
                DeleteBell();
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        private static string Plural(int count, string one, string few, string many)
        {
            int mod100 = count % 100;
            int mod10 = count % 10;
            if (mod100 >= 11 && mod100 <= 14)
            {
                return many;
            }
            if (mod10 == 1)
            {
                return one;
            }
            if (mod10 >= 2 && mod10 <= 4)
            {
                return few;
            }
            return many;
        }

        private sealed class BufferedDataGridView : DataGridView
        {
            public BufferedDataGridView()
            {
                DoubleBuffered = true;
            }
        }
    }
}
