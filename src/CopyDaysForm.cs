using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace SchoolBells
{
    public sealed class CopyDaysForm : Form
    {
        private readonly CheckedListBox _list;
        private readonly int _sourceDay;
        private readonly List<int> _dayIndices;

        public List<int> SelectedDays { get; private set; }

        public CopyDaysForm(int sourceDay, AppSettings settings)
        {
            _sourceDay = sourceDay;
            _dayIndices = new List<int>();
            SelectedDays = new List<int>();

            Text = "Копировать день";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            ClientSize = new Size(472, 436);
            Font = new Font("Segoe UI", 9.5f, FontStyle.Regular, GraphicsUnit.Point);
            BackColor = UiTheme.PageBack;

            Label caption = new Label();
            caption.Text = "Скопировать расписание дня «" + ScheduleEngine.DayName(sourceDay) + "»";
            caption.AutoSize = false;
            caption.Location = new Point(20, 16);
            caption.Size = new Size(432, 26);
            caption.Font = new Font("Segoe UI Semibold", 11.5f, FontStyle.Regular, GraphicsUnit.Point);
            caption.ForeColor = UiTheme.Text;
            Controls.Add(caption);

            Label hint = new Label();
            hint.Text = "Отметьте дни, в которые нужно перенести расписание. Существующее расписание выбранных дней будет заменено.";
            hint.AutoSize = false;
            hint.Location = new Point(20, 46);
            hint.Size = new Size(432, 36);
            hint.ForeColor = UiTheme.Muted;
            hint.Font = new Font("Segoe UI", 8.75f, FontStyle.Regular, GraphicsUnit.Point);
            Controls.Add(hint);

            RoundedPanel listHost = new RoundedPanel();
            listHost.Location = new Point(20, 90);
            listHost.Size = new Size(432, 258);
            listHost.BackColor = UiTheme.CardBack;
            listHost.CornerRadius = 12;
            listHost.Padding = new Padding(7);
            Controls.Add(listHost);

            _list = new CheckedListBox();
            _list.Dock = DockStyle.Fill;
            _list.CheckOnClick = true;
            _list.IntegralHeight = false;
            _list.BorderStyle = BorderStyle.None;
            _list.BackColor = UiTheme.CardBack;
            _list.ForeColor = UiTheme.Text;
            _list.Font = new Font("Segoe UI", 10f, FontStyle.Regular, GraphicsUnit.Point);
            for (int day = 0; day < 8; day++)
            {
                if (day == sourceDay)
                {
                    continue;
                }
                DaySchedule schedule = settings.GetEffectiveDay(day);
                int count = schedule == null ? 0 : schedule.Bells.Count;
                string suffix = count == 0
                    ? "пусто"
                    : count.ToString() + " " + Plural(count, "звонок", "звонка", "звонков");
                string label = ScheduleEngine.DayName(day);
                if (day <= 6 && settings.IsShortDay(day))
                {
                    label += " (короткий)";
                }
                _list.Items.Add(label + "  —  " + suffix, false);
                _dayIndices.Add(day);
            }
            listHost.Controls.Add(_list);

            RoundedButton selectAll = MakeDialogButton("Выбрать все", 112, 32, false);
            selectAll.Location = new Point(20, 362);
            selectAll.Click += delegate { SetAllChecked(true); };
            Controls.Add(selectAll);

            RoundedButton clearAll = MakeDialogButton("Снять все", 100, 32, false);
            clearAll.Location = new Point(140, 362);
            clearAll.Click += delegate { SetAllChecked(false); };
            Controls.Add(clearAll);

            RoundedButton ok = MakeDialogButton("Копировать", 124, 34, true);
            ok.Location = new Point(248, 361);
            ok.Click += OkClick;
            Controls.Add(ok);

            RoundedButton cancel = MakeDialogButton("Отмена", 84, 34, false);
            cancel.Location = new Point(380, 361);
            cancel.DialogResult = DialogResult.Cancel;
            Controls.Add(cancel);

            AcceptButton = ok;
            CancelButton = cancel;
            ActiveControl = _list;
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            WindowChrome.Apply(Handle, UiTheme.HeaderBack);
        }

        private RoundedButton MakeDialogButton(string text, int width, int height, bool accent)
        {
            RoundedButton button = new RoundedButton();
            button.Text = text;
            button.Size = new Size(width, height);
            button.CornerRadius = 9;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = accent ? 0 : 1;
            button.FlatAppearance.BorderColor = UiTheme.Border;
            button.BackColor = accent ? UiTheme.Accent : UiTheme.CardBack;
            button.ForeColor = accent ? Color.White : UiTheme.Text;
            button.Font = new Font(accent ? "Segoe UI Semibold" : "Segoe UI", 9.25f,
                FontStyle.Regular, GraphicsUnit.Point);
            button.FlatAppearance.MouseOverBackColor = accent ? UiTheme.AccentHover : UiTheme.Track;
            return button;
        }

        private void OkClick(object sender, EventArgs e)
        {
            List<int> selected = new List<int>();
            for (int i = 0; i < _list.Items.Count; i++)
            {
                if (_list.GetItemChecked(i))
                {
                    selected.Add(_dayIndices[i]);
                }
            }

            if (selected.Count == 0)
            {
                MessageBox.Show(this, "Отметьте хотя бы один день.", "Копировать день",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            SelectedDays = selected;
            DialogResult = DialogResult.OK;
        }

        private void SetAllChecked(bool value)
        {
            for (int i = 0; i < _list.Items.Count; i++)
            {
                _list.SetItemChecked(i, value);
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
    }
}
