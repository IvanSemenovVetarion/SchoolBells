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
        private readonly AppSettings _settings;
        private readonly List<int> _dayIndices;

        public List<int> SelectedDays { get; private set; }

        public CopyDaysForm(int sourceDay, AppSettings settings)
        {
            _sourceDay = sourceDay;
            _settings = settings;
            _dayIndices = new List<int>();
            SelectedDays = new List<int>();

            Text = "Копировать день";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            ClientSize = new Size(430, 390);
            Font = new Font("Segoe UI", 9.5f, FontStyle.Regular, GraphicsUnit.Point);
            BackColor = Color.FromArgb(247, 249, 252);

            Label caption = new Label();
            caption.Text = "Скопировать расписание дня «" + ScheduleEngine.DayName(sourceDay) + "» в дни:";
            caption.AutoSize = false;
            caption.Location = new Point(16, 14);
            caption.Size = new Size(398, 24);
            caption.Font = new Font("Segoe UI", 10f, FontStyle.Bold, GraphicsUnit.Point);
            Controls.Add(caption);

            Label hint = new Label();
            hint.Text = "Отметьте нужные дни. Существующее расписание выбранных дней будет заменено.";
            hint.AutoSize = false;
            hint.Location = new Point(16, 40);
            hint.Size = new Size(398, 34);
            hint.ForeColor = Color.FromArgb(95, 108, 122);
            Controls.Add(hint);

            _list = new CheckedListBox();
            _list.CheckOnClick = true;
            _list.IntegralHeight = false;
            _list.BorderStyle = BorderStyle.FixedSingle;
            _list.Font = new Font("Segoe UI", 10f, FontStyle.Regular, GraphicsUnit.Point);
            _list.Location = new Point(16, 80);
            _list.Size = new Size(398, 222);
            _list.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
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
            Controls.Add(_list);

            Button selectAll = new Button();
            selectAll.Text = "Выбрать все";
            selectAll.FlatStyle = FlatStyle.Flat;
            selectAll.FlatAppearance.BorderColor = Color.FromArgb(198, 210, 224);
            selectAll.BackColor = Color.White;
            selectAll.Location = new Point(16, 316);
            selectAll.Size = new Size(100, 30);
            selectAll.Click += delegate { SetAllChecked(true); };
            Controls.Add(selectAll);

            Button clearAll = new Button();
            clearAll.Text = "Снять все";
            clearAll.FlatStyle = FlatStyle.Flat;
            clearAll.FlatAppearance.BorderColor = Color.FromArgb(198, 210, 224);
            clearAll.BackColor = Color.White;
            clearAll.Location = new Point(122, 316);
            clearAll.Size = new Size(90, 30);
            clearAll.Click += delegate { SetAllChecked(false); };
            Controls.Add(clearAll);

            Button ok = new Button();
            ok.Text = "Копировать";
            ok.DialogResult = DialogResult.None;
            ok.BackColor = Color.FromArgb(45, 108, 180);
            ok.ForeColor = Color.White;
            ok.FlatStyle = FlatStyle.Flat;
            ok.FlatAppearance.BorderSize = 0;
            ok.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold, GraphicsUnit.Point);
            ok.Location = new Point(224, 316);
            ok.Size = new Size(110, 32);
            ok.Click += OkClick;
            Controls.Add(ok);

            Button cancel = new Button();
            cancel.Text = "Отмена";
            cancel.DialogResult = DialogResult.Cancel;
            cancel.FlatStyle = FlatStyle.Flat;
            cancel.FlatAppearance.BorderColor = Color.FromArgb(198, 210, 224);
            cancel.BackColor = Color.White;
            cancel.Location = new Point(342, 316);
            cancel.Size = new Size(72, 32);
            Controls.Add(cancel);

            AcceptButton = ok;
            CancelButton = cancel;
            ActiveControl = _list;
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
