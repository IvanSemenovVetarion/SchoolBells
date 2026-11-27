using System;
using System.Collections.Generic;
using System.Globalization;

namespace SchoolBells
{
    public sealed class Bell
    {
        public TimeSpan Start;
        public TimeSpan End;
        public string Comment;

        public Bell()
        {
            Comment = "";
        }

        public Bell(TimeSpan start, TimeSpan end, string comment)
        {
            Start = start;
            End = end;
            Comment = comment == null ? "" : comment;
        }

        public Bell Clone()
        {
            return new Bell(Start, End, Comment);
        }
    }

    public sealed class DaySchedule
    {
        public List<Bell> Bells;

        public DaySchedule()
        {
            Bells = new List<Bell>();
        }

        public DaySchedule Clone()
        {
            DaySchedule copy = new DaySchedule();
            for (int i = 0; i < Bells.Count; i++)
            {
                copy.Bells.Add(Bells[i].Clone());
            }
            return copy;
        }
    }

    public sealed class MediaItem
    {
        public string File;
        public string Name;

        public MediaItem()
        {
            File = "";
            Name = "";
        }

        public MediaItem(string file, string name)
        {
            File = file == null ? "" : file;
            Name = name == null ? "" : name;
        }

        public MediaItem Clone()
        {
            return new MediaItem(File, Name);
        }
    }

    public sealed class MusicEntry
    {
        public int Day;
        public TimeSpan Start;
        public TimeSpan End;
        public string File;
        public string Name;
        public string Comment;

        public MusicEntry()
        {
            Day = 7;
            File = "";
            Name = "";
            Comment = "";
        }

        public MusicEntry(int day, TimeSpan start, TimeSpan end, string file, string name, string comment)
        {
            Day = day;
            Start = start;
            End = end;
            File = file == null ? "" : file;
            Name = name == null ? "" : name;
            Comment = comment == null ? "" : comment;
        }

        public MusicEntry Clone()
        {
            return new MusicEntry(Day, Start, End, File, Name, Comment);
        }

        public bool MatchesDay(int weekday)
        {
            return Day == 7 || Day == weekday;
        }
    }

    public sealed class AppSettings
    {
        public bool Enabled;
        public int Volume;
        public bool RingAtStart;
        public bool RingAtEnd;
        public bool AutoStart;
        public bool MinimizeToTray;
        public bool FirstRun;
        public string MelodyStartFile;
        public string MelodyEndFile;
        public string MelodyStartName;
        public string MelodyEndName;
        public List<DaySchedule> Days;
        public DaySchedule ShortDay;
        public bool[] ShortDays;
        public List<MusicEntry> MusicSchedule;
        public List<MediaItem> MusicPlaylist;
        public List<MediaItem> VoiceRecordings;
        public string Theme;
        public bool Snowflakes;
        public bool MusicEnabled;
        public int MusicVolume;
        public int VoiceVolume;

        public const int DayCount = 8;

        public AppSettings()
        {
            Enabled = true;
            Volume = 80;
            RingAtStart = true;
            RingAtEnd = true;
            AutoStart = false;
            MinimizeToTray = true;
            FirstRun = true;
            MelodyStartFile = "";
            MelodyEndFile = "";
            MelodyStartName = "";
            MelodyEndName = "";
            Days = new List<DaySchedule>();
            for (int i = 0; i < 7; i++)
            {
                Days.Add(new DaySchedule());
            }
            ShortDay = new DaySchedule();
            ShortDays = new bool[7];
            MusicSchedule = new List<MusicEntry>();
            MusicPlaylist = new List<MediaItem>();
            VoiceRecordings = new List<MediaItem>();
            Theme = "auto";
            Snowflakes = true;
            MusicEnabled = true;
            MusicVolume = 60;
            VoiceVolume = 90;
        }

        public void EnsureSevenDays()
        {
            while (Days.Count < 7)
            {
                Days.Add(new DaySchedule());
            }
            while (Days.Count > 7)
            {
                Days.RemoveAt(Days.Count - 1);
            }
            if (ShortDay == null)
            {
                ShortDay = new DaySchedule();
            }
            if (ShortDays == null || ShortDays.Length != 7)
            {
                bool[] fixedDays = new bool[7];
                if (ShortDays != null)
                {
                    for (int i = 0; i < 7 && i < ShortDays.Length; i++)
                    {
                        fixedDays[i] = ShortDays[i];
                    }
                }
                ShortDays = fixedDays;
            }
            if (MusicSchedule == null)
            {
                MusicSchedule = new List<MusicEntry>();
            }
            if (MusicPlaylist == null)
            {
                MusicPlaylist = new List<MediaItem>();
            }
            if (VoiceRecordings == null)
            {
                VoiceRecordings = new List<MediaItem>();
            }
        }

        public DaySchedule GetDay(int index)
        {
            if (index == 7)
            {
                return ShortDay;
            }
            if (index < 0 || index > 6)
            {
                return null;
            }
            return Days[index];
        }

        public void SetDay(int index, DaySchedule schedule)
        {
            if (index == 7)
            {
                ShortDay = schedule;
            }
            else if (index >= 0 && index <= 6)
            {
                Days[index] = schedule;
            }
        }

        public bool IsShortDay(int weekday)
        {
            return ShortDays != null && weekday >= 0 && weekday < 7 && ShortDays[weekday];
        }

        public void SetShortDay(int weekday, bool value)
        {
            EnsureSevenDays();
            if (weekday >= 0 && weekday < 7)
            {
                ShortDays[weekday] = value;
            }
        }

        public DaySchedule GetEffectiveDay(int weekday)
        {
            if (weekday == 7)
            {
                return ShortDay;
            }
            if (weekday < 0 || weekday > 6)
            {
                return null;
            }
            return IsShortDay(weekday) ? ShortDay : Days[weekday];
        }
    }

    public sealed class BellEvent
    {
        public TimeSpan Time;
        public bool IsStart;
        public int LessonIndex;
        public string Comment;

        public BellEvent(TimeSpan time, bool isStart, int lessonIndex, string comment)
        {
            Time = time;
            IsStart = isStart;
            LessonIndex = lessonIndex;
            Comment = comment == null ? "" : comment;
        }

        public string KindText
        {
            get { return IsStart ? "начало" : "конец"; }
        }
    }

    public static class TimeUtil
    {
        public static string FormatTime(TimeSpan time)
        {
            int hours = time.Hours;
            if (time.Days > 0)
            {
                hours += time.Days * 24;
            }
            return string.Format(CultureInfo.InvariantCulture, "{0:00}:{1:00}", hours, time.Minutes);
        }

        public static bool TryParseTime(string text, out TimeSpan result)
        {
            result = TimeSpan.Zero;
            if (text == null)
            {
                return false;
            }

            text = text.Trim();
            if (text.Length == 0)
            {
                return false;
            }

            // Пользователи часто вводят 0800, 800, 8, 8:00 или 8.00.
            text = text.Replace("：", ":").Replace("。", ".").Replace(" ", "");
            if (text.Length == 0)
            {
                return false;
            }

            int hours;
            int minutes;

            if (text.IndexOf(':') >= 0 || text.IndexOf('.') >= 0)
            {
                string[] parts = text.Split(new char[] { ':', '.' }, StringSplitOptions.None);
                if (parts.Length != 2)
                {
                    return false;
                }
                if (!int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out hours) ||
                    !int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out minutes))
                {
                    return false;
                }
            }
            else
            {
                for (int i = 0; i < text.Length; i++)
                {
                    if (!char.IsDigit(text[i]))
                    {
                        return false;
                    }
                }

                if (text.Length <= 2)
                {
                    // 8 -> 08:00, 08 -> 08:00
                    if (!int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out hours))
                    {
                        return false;
                    }
                    minutes = 0;
                }
                else if (text.Length == 3)
                {
                    // 800 -> 08:00, 930 -> 09:30
                    if (!int.TryParse(text.Substring(0, 1), NumberStyles.Integer, CultureInfo.InvariantCulture, out hours) ||
                        !int.TryParse(text.Substring(1, 2), NumberStyles.Integer, CultureInfo.InvariantCulture, out minutes))
                    {
                        return false;
                    }
                }
                else if (text.Length == 4)
                {
                    // 0800 -> 08:00, 1230 -> 12:30
                    if (!int.TryParse(text.Substring(0, 2), NumberStyles.Integer, CultureInfo.InvariantCulture, out hours) ||
                        !int.TryParse(text.Substring(2, 2), NumberStyles.Integer, CultureInfo.InvariantCulture, out minutes))
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }

            if (hours < 0 || hours > 23 || minutes < 0 || minutes > 59)
            {
                return false;
            }

            result = new TimeSpan(hours, minutes, 0);
            return true;
        }

        public static string FormatCountdown(TimeSpan span)
        {
            if (span.TotalSeconds < 0)
            {
                span = TimeSpan.Zero;
            }

            int total = (int)Math.Ceiling(span.TotalSeconds);
            if (total < 60)
            {
                return "через " + total.ToString(CultureInfo.InvariantCulture) + " с";
            }
            if (total < 3600)
            {
                int mins = total / 60;
                int secs = total % 60;
                return "через " + mins.ToString(CultureInfo.InvariantCulture) + " мин " +
                       secs.ToString("00", CultureInfo.InvariantCulture) + " с";
            }

            int hours = total / 3600;
            int minutes = (total % 3600) / 60;
            return "через " + hours.ToString(CultureInfo.InvariantCulture) + " ч " +
                   minutes.ToString("00", CultureInfo.InvariantCulture) + " мин";
        }
    }

    public static class MusicEngine
    {
        public static bool IsDue(MusicEntry entry, int weekday, TimeSpan now, int toleranceSeconds)
        {
            if (entry == null || !entry.MatchesDay(weekday))
            {
                return false;
            }
            TimeSpan delta = now - entry.Start;
            return delta >= TimeSpan.Zero && delta <= TimeSpan.FromSeconds(toleranceSeconds);
        }
    }

    public static class ScheduleEngine
    {
        private static readonly string[] DayNames = new string[]
        {
            "Понедельник", "Вторник", "Среда", "Четверг",
            "Пятница", "Суббота", "Воскресенье", "Короткий день"
        };

        private static readonly string[] DayNamesShort = new string[]
        {
            "Пн", "Вт", "Ср", "Чт", "Пт", "Сб", "Вс", "Кор."
        };

        public static string DayName(int index)
        {
            if (index < 0 || index > 7)
            {
                return "";
            }
            return DayNames[index];
        }

        public static string DayNameShort(int index)
        {
            if (index < 0 || index > 7)
            {
                return "";
            }
            return DayNamesShort[index];
        }

        public static DaySchedule ResolveDay(AppSettings settings, int weekday)
        {
            if (settings == null)
            {
                return null;
            }
            return settings.GetEffectiveDay(weekday);
        }

        public static int DayIndex(DateTime date)
        {
            return ((int)date.DayOfWeek + 6) % 7;
        }

        public static List<BellEvent> BuildEvents(DaySchedule day, bool ringStart, bool ringEnd)
        {
            List<BellEvent> events = new List<BellEvent>();
            if (day == null)
            {
                return events;
            }

            for (int i = 0; i < day.Bells.Count; i++)
            {
                Bell bell = day.Bells[i];
                if (ringStart)
                {
                    events.Add(new BellEvent(bell.Start, true, i + 1, bell.Comment));
                }
                if (ringEnd)
                {
                    events.Add(new BellEvent(bell.End, false, i + 1, bell.Comment));
                }
            }

            events.Sort(delegate(BellEvent a, BellEvent b)
            {
                int result = a.Time.CompareTo(b.Time);
                if (result != 0)
                {
                    return result;
                }
                return a.IsStart.CompareTo(b.IsStart);
            });

            return events;
        }

        public static BellEvent GetNextEvent(List<BellEvent> events, TimeSpan now)
        {
            if (events == null)
            {
                return null;
            }

            for (int i = 0; i < events.Count; i++)
            {
                if (events[i].Time > now)
                {
                    return events[i];
                }
            }
            return null;
        }

        public static List<BellEvent> FindDueEvents(List<BellEvent> events, TimeSpan now, int toleranceSeconds)
        {
            List<BellEvent> due = new List<BellEvent>();
            if (events == null)
            {
                return due;
            }

            TimeSpan tolerance = TimeSpan.FromSeconds(toleranceSeconds);
            for (int i = 0; i < events.Count; i++)
            {
                BellEvent item = events[i];
                if (item.Time <= now && now - item.Time <= tolerance)
                {
                    due.Add(item);
                }
            }
            return due;
        }

        public static string MakePlayedKey(DateTime date, BellEvent item)
        {
            return date.ToString("yyyyMMdd", CultureInfo.InvariantCulture) + "|" +
                   TimeUtil.FormatTime(item.Time) + "|" +
                   (item.IsStart ? "S" : "E");
        }

        public static string EventDescription(BellEvent item)
        {
            if (item == null)
            {
                return "";
            }
            return TimeUtil.FormatTime(item.Time) + " · урок " +
                   item.LessonIndex.ToString(CultureInfo.InvariantCulture) +
                   " (" + item.KindText + ")";
        }
    }
}
