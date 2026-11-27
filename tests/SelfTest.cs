using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SchoolBells
{
    internal static class SelfTest
    {
        private static int _failures;

        [STAThread]
        private static int Main(string[] args)
        {
            try
            {
                Console.OutputEncoding = Encoding.UTF8;
            }
            catch
            {
            }

            Console.WriteLine("SchoolBells self-test");
            TestTimeParsing();
            TestEvents();
            TestDayIndex();
            TestShortDays();
            TestMusicEngine();
            TestThemes();
            TestSettingsRoundTrip();
            TestMelodyImport();
            TestDefaultBell();

            bool audio = true;
            for (int i = 0; i < args.Length; i++)
            {
                if (string.Equals(args[i], "--no-audio", StringComparison.OrdinalIgnoreCase))
                {
                    audio = false;
                }
            }

            if (audio)
            {
                TestAudio(args);
            }

            Console.WriteLine(_failures == 0 ? "ALL TESTS PASSED" : "FAILURES: " + _failures);
            return _failures == 0 ? 0 : 1;
        }

        private static void Check(bool condition, string message)
        {
            if (!condition)
            {
                _failures++;
                Console.WriteLine("FAIL: " + message);
            }
            else
            {
                Console.WriteLine("ok: " + message);
            }
        }

        private static void TestTimeParsing()
        {
            TimeSpan time;
            Check(TimeUtil.TryParseTime("08:05", out time) && time.Hours == 8 && time.Minutes == 5, "parse 08:05");
            Check(TimeUtil.TryParseTime("8.5", out time) && time.Hours == 8 && time.Minutes == 5, "parse 8.5");
            Check(TimeUtil.TryParseTime(" 23:59 ", out time) && time.Hours == 23 && time.Minutes == 59, "parse 23:59");
            Check(TimeUtil.TryParseTime("0800", out time) && time.Hours == 8 && time.Minutes == 0, "parse 0800");
            Check(TimeUtil.TryParseTime("800", out time) && time.Hours == 8 && time.Minutes == 0, "parse 800");
            Check(TimeUtil.TryParseTime("8", out time) && time.Hours == 8 && time.Minutes == 0, "parse 8");
            Check(TimeUtil.TryParseTime("930", out time) && time.Hours == 9 && time.Minutes == 30, "parse 930");
            Check(TimeUtil.TryParseTime("1230", out time) && time.Hours == 12 && time.Minutes == 30, "parse 1230");
            Check(TimeUtil.TryParseTime("8:5", out time) && time.Hours == 8 && time.Minutes == 5, "parse 8:5");
            Check(!TimeUtil.TryParseTime("24:00", out time), "reject 24:00");
            Check(!TimeUtil.TryParseTime("08:60", out time), "reject 08:60");
            Check(!TimeUtil.TryParseTime("00800", out time), "reject 00800");
            Check(!TimeUtil.TryParseTime("abc", out time), "reject abc");
            Check(TimeUtil.FormatTime(new TimeSpan(8, 5, 0)) == "08:05", "format time");
            Check(TimeUtil.FormatTime(TimeSpan.Zero) == "00:00", "format midnight");
        }

        private static void TestEvents()
        {
            DaySchedule day = new DaySchedule();
            day.Bells.Add(new Bell(new TimeSpan(8, 0, 0), new TimeSpan(8, 45, 0), "1"));
            day.Bells.Add(new Bell(new TimeSpan(8, 55, 0), new TimeSpan(9, 40, 0), "2"));

            List<BellEvent> events = ScheduleEngine.BuildEvents(day, true, true);
            Check(events.Count == 4, "four events");
            Check(events[0].Time == new TimeSpan(8, 0, 0) && events[0].IsStart, "first is 08:00 start");
            Check(events[1].Time == new TimeSpan(8, 45, 0) && !events[1].IsStart, "second is 08:45 end");
            Check(events[2].Time == new TimeSpan(8, 55, 0), "third is 08:55");

            List<BellEvent> onlyStarts = ScheduleEngine.BuildEvents(day, true, false);
            Check(onlyStarts.Count == 2, "starts only");

            BellEvent next = ScheduleEngine.GetNextEvent(events, new TimeSpan(8, 0, 1));
            Check(next != null && next.Time == new TimeSpan(8, 45, 0), "next after 08:00:01");

            List<BellEvent> due = ScheduleEngine.FindDueEvents(events, new TimeSpan(8, 0, 3), 5);
            Check(due.Count == 1 && due[0].Time == new TimeSpan(8, 0, 0), "due within tolerance");

            due = ScheduleEngine.FindDueEvents(events, new TimeSpan(8, 0, 6), 5);
            Check(due.Count == 0, "not due after tolerance");

            due = ScheduleEngine.FindDueEvents(events, new TimeSpan(7, 59, 59), 5);
            Check(due.Count == 0, "not due before time");

            string key1 = ScheduleEngine.MakePlayedKey(new DateTime(2026, 11, 23), events[0]);
            string key2 = ScheduleEngine.MakePlayedKey(new DateTime(2026, 11, 24), events[0]);
            Check(key1 != key2, "played key includes date");
        }

        private static void TestDayIndex()
        {
            Check(ScheduleEngine.DayIndex(new DateTime(2026, 11, 23)) == 0, "2026-11-23 is Monday");
            Check(ScheduleEngine.DayIndex(new DateTime(2026, 11, 29)) == 6, "2026-11-29 is Sunday");
            Check(ScheduleEngine.DayName(0) == "Понедельник", "day name");
        }

        private static void TestShortDays()
        {
            AppSettings settings = new AppSettings();
            Check(!settings.IsShortDay(0), "monday is normal by default");
            settings.SetShortDay(0, true);
            Check(settings.IsShortDay(0), "monday can be short");
            Check(settings.GetEffectiveDay(0) == settings.ShortDay, "short monday uses short template");
            Check(settings.GetEffectiveDay(1) == settings.Days[1], "tuesday uses normal template");

            settings.ShortDay.Bells.Add(new Bell(new TimeSpan(8, 0, 0), new TimeSpan(8, 30, 0), "короткий"));
            List<BellEvent> events = ScheduleEngine.BuildEvents(settings.GetEffectiveDay(0), true, true);
            Check(events.Count == 2, "short day events");
            Check(ScheduleEngine.DayName(7) == "Короткий день", "short day name");
            Check(settings.GetDay(7) == settings.ShortDay, "get day 7");
        }

        private static void TestMusicEngine()
        {
            MusicEntry everyday = new MusicEntry(7, new TimeSpan(8, 0, 0), new TimeSpan(8, 10, 0),
                "music.mp3", "Музыка", "");
            MusicEntry monday = new MusicEntry(0, new TimeSpan(8, 0, 0), new TimeSpan(8, 10, 0),
                "music.mp3", "Музыка", "");

            Check(everyday.MatchesDay(3), "everyday music matches wednesday");
            Check(monday.MatchesDay(0) && !monday.MatchesDay(1), "monday music only matches monday");
            Check(MusicEngine.IsDue(everyday, 3, new TimeSpan(8, 0, 2), 5), "music due within tolerance");
            Check(!MusicEngine.IsDue(everyday, 3, new TimeSpan(8, 0, 6), 5), "music not due after tolerance");
            Check(!MusicEngine.IsDue(everyday, 3, new TimeSpan(7, 59, 59), 5), "music not due before start");
            Check(MusicEngine.IsDue(monday, 0, new TimeSpan(8, 0, 0), 5), "monday music due on monday");
            Check(!MusicEngine.IsDue(monday, 1, new TimeSpan(8, 0, 0), 5), "monday music not due on tuesday");
        }

        private static void TestThemes()
        {
            Check(ThemeManager.Resolve("auto", new DateTime(2026, 12, 15)).Id == "winter", "auto winter theme");
            Check(ThemeManager.Resolve("auto", new DateTime(2026, 3, 15)).Id == "spring", "auto spring theme");
            Check(ThemeManager.Resolve("auto", new DateTime(2026, 7, 15)).Id == "summer", "auto summer theme");
            Check(ThemeManager.Resolve("auto", new DateTime(2026, 10, 15)).Id == "autumn", "auto autumn theme");
            Check(ThemeManager.Resolve("winter", DateTime.Now).Snow, "winter theme has snow");
            Check(!ThemeManager.Resolve("classic", DateTime.Now).Snow, "classic theme has no snow");
        }

        private static void TestSettingsRoundTrip()
        {
            string dir = Path.Combine(Path.GetTempPath(), "sb_test_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            try
            {
                SettingsStore store = new SettingsStore(dir);
                AppSettings settings = new AppSettings();
                settings.Enabled = false;
                settings.Volume = 37;
                settings.RingAtStart = true;
                settings.RingAtEnd = false;
                settings.AutoStart = true;
                settings.MinimizeToTray = false;
                settings.FirstRun = false;
                settings.MelodyStartFile = "start.mp3";
                settings.MelodyEndFile = "";
                settings.Days[0].Bells.Add(new Bell(new TimeSpan(8, 0, 0), new TimeSpan(8, 45, 0), "1 урок"));
                settings.Days[0].Bells.Add(new Bell(new TimeSpan(8, 55, 0), new TimeSpan(9, 40, 0), "2\tурок"));
                settings.Days[1] = settings.Days[0].Clone();
                settings.Days[1].Bells[0].Comment = "копия";
                settings.Theme = "winter";
                settings.Snowflakes = false;
                settings.MusicEnabled = false;
                settings.MusicVolume = 44;
                settings.VoiceVolume = 77;
                settings.SetShortDay(1, true);
                settings.ShortDay.Bells.Add(new Bell(new TimeSpan(8, 0, 0), new TimeSpan(8, 30, 0), "короткий"));
                settings.MusicSchedule.Add(new MusicEntry(7, new TimeSpan(8, 0, 0), new TimeSpan(8, 10, 0),
                    "music_a.mp3", "Зарядка", "утро"));
                settings.MusicPlaylist.Add(new MediaItem("music_p.mp3", "Плейлист"));
                settings.VoiceRecordings.Add(new MediaItem("voice_v.wav", "Объявление"));

                Check(store.Save(settings), "save settings");
                Check(store.Save(settings), "save settings second time");

                LoadOutcome outcome = store.Load();
                AppSettings loaded = outcome.Settings;
                Check(outcome.Warning == null, "no load warning");
                Check(loaded.Enabled == false, "loaded enabled");
                Check(loaded.Volume == 37, "loaded volume");
                Check(loaded.RingAtStart == true && loaded.RingAtEnd == false, "loaded ring flags");
                Check(loaded.AutoStart == true && loaded.MinimizeToTray == false, "loaded behavior flags");
                Check(loaded.FirstRun == false, "loaded first run");
                Check(loaded.MelodyStartFile == "start.mp3", "loaded start melody");
                Check(loaded.Days[0].Bells.Count == 2, "loaded bell count");
                Check(loaded.Days[0].Bells[1].Comment == "2 урок", "sanitized comment");
                Check(loaded.Days[1].Bells.Count == 2 && loaded.Days[1].Bells[0].Comment == "копия", "loaded copied day");
                Check(loaded.Days[6].Bells.Count == 0, "empty sunday");
                Check(loaded.Theme == "winter", "loaded theme");
                Check(loaded.Snowflakes == false, "loaded snowflakes");
                Check(loaded.MusicEnabled == false, "loaded music enabled");
                Check(loaded.MusicVolume == 44 && loaded.VoiceVolume == 77, "loaded media volumes");
                Check(loaded.IsShortDay(1), "loaded short day flag");
                Check(loaded.ShortDay.Bells.Count == 1, "loaded short day schedule");
                Check(loaded.MusicSchedule.Count == 1 &&
                      loaded.MusicSchedule[0].Day == 7 &&
                      loaded.MusicSchedule[0].Name == "Зарядка" &&
                      loaded.MusicSchedule[0].Comment == "утро", "loaded music schedule");
                Check(loaded.MusicPlaylist.Count == 1 && loaded.MusicPlaylist[0].File == "music_p.mp3", "loaded playlist");
                Check(loaded.VoiceRecordings.Count == 1 && loaded.VoiceRecordings[0].Name == "Объявление", "loaded voice archive");
                Check(File.Exists(store.SettingsPath + ".bak"), "backup exists");
            }
            finally
            {
                try
                {
                    Directory.Delete(dir, true);
                }
                catch
                {
                }
            }
        }

        private static void TestMelodyImport()
        {
            string dir = Path.Combine(Path.GetTempPath(), "sb_test_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            try
            {
                string wav = Path.Combine(dir, "source.wav");
                DefaultBellGenerator.EnsureDefaultBell(dir);
                File.Copy(Path.Combine(dir, "default_bell.wav"), wav, true);

                SettingsStore store = new SettingsStore(dir);
                string error;
                string imported = store.ImportMelody(wav, true, out error);
                Check(error == null && imported != null, "import melody");
                Check(File.Exists(Path.Combine(dir, imported)), "imported file exists");
                Check(imported.StartsWith("melody_start_"), "imported name prefix");

                AppSettings settings = new AppSettings();
                settings.MelodyStartFile = imported;
                settings.MelodyEndFile = "";
                File.WriteAllText(Path.Combine(dir, "melody_start_old.mp3"), "x");
                store.CleanupMelodies(settings);
                Check(!File.Exists(Path.Combine(dir, "melody_start_old.mp3")), "old melody removed");
                Check(File.Exists(Path.Combine(dir, imported)), "current melody kept");
            }
            finally
            {
                try
                {
                    Directory.Delete(dir, true);
                }
                catch
                {
                }
            }
        }

        private static void TestDefaultBell()
        {
            string dir = Path.Combine(Path.GetTempPath(), "sb_test_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            try
            {
                string path = DefaultBellGenerator.EnsureDefaultBell(dir);
                Check(File.Exists(path), "default bell generated");
                Check(new FileInfo(path).Length > 10000, "default bell has data");
                byte[] header = new byte[12];
                using (FileStream stream = File.OpenRead(path))
                {
                    stream.Read(header, 0, header.Length);
                }
                Check(header[0] == 'R' && header[1] == 'I' && header[2] == 'F' && header[3] == 'F', "wav RIFF header");
                Check(header[8] == 'W' && header[9] == 'A' && header[10] == 'V' && header[11] == 'E', "wav WAVE header");
            }
            finally
            {
                try
                {
                    Directory.Delete(dir, true);
                }
                catch
                {
                }
            }
        }

        private static void TestAudio(string[] args)
        {
            string dir = Path.Combine(Path.GetTempPath(), "sb_test_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            try
            {
                string wav = DefaultBellGenerator.EnsureDefaultBell(dir);
                using (PlaybackEngine engine = new PlaybackEngine(0))
                {
                    engine.Prewarm();
                    string error;
                    bool played = engine.Play(wav, wav, out error);
                    Check(played, "play default wav at volume 0" + (error == null ? "" : " (" + error + ")"));
                    System.Threading.Thread.Sleep(700);
                    engine.Stop();
                }

                string mp3 = null;
                for (int i = 0; i < args.Length; i++)
                {
                    if (File.Exists(args[i]) && args[i].EndsWith(".mp3", StringComparison.OrdinalIgnoreCase))
                    {
                        mp3 = args[i];
                        break;
                    }
                }
                if (mp3 != null)
                {
                    using (PlaybackEngine engine = new PlaybackEngine(0))
                    {
                        string error;
                        bool played = engine.Play(mp3, wav, out error);
                        Check(played, "play mp3 at volume 0" + (error == null ? "" : " (" + error + ")"));
                        System.Threading.Thread.Sleep(700);
                        engine.Stop();
                    }

                    using (MciPlayer mci = new MciPlayer())
                    {
                        string error;
                        bool played = mci.Play(mp3, 0, out error);
                        Check(played, "mci play mp3 at volume 0" + (error == null ? "" : " (" + error + ")"));
                        System.Threading.Thread.Sleep(500);
                        mci.Stop();
                    }
                }
            }
            finally
            {
                try
                {
                    Directory.Delete(dir, true);
                }
                catch
                {
                }
            }
        }
    }
}
