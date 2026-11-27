using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Microsoft.Win32;

namespace SchoolBells
{
    public static class AppPaths
    {
        public static string ResolveDataDir(string exePath)
        {
            string exeDir = Path.GetDirectoryName(exePath);
            if (!string.IsNullOrEmpty(exeDir))
            {
                string portable = Path.Combine(exeDir, "SchoolBellsData");
                if (TryPrepareWritable(portable))
                {
                    return portable;
                }
            }

            try
            {
                string appData = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "SchoolBells");
                if (TryPrepareWritable(appData))
                {
                    return appData;
                }
            }
            catch
            {
            }

            string temp = Path.Combine(Path.GetTempPath(), "SchoolBells");
            Directory.CreateDirectory(temp);
            return temp;
        }

        private static bool TryPrepareWritable(string dir)
        {
            try
            {
                Directory.CreateDirectory(dir);
                string probe = Path.Combine(dir, ".write-test");
                File.WriteAllText(probe, "ok");
                File.Delete(probe);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }

    public static class Log
    {
        private static readonly object Sync = new object();
        private static string _dataDir = "";

        public static void Configure(string dataDir)
        {
            _dataDir = dataDir;
        }

        public static void Error(string message, Exception exception)
        {
            try
            {
                if (string.IsNullOrEmpty(_dataDir))
                {
                    return;
                }

                string path = Path.Combine(_dataDir, "errors.log");
                RotateIfNeeded(path);
                StringBuilder text = new StringBuilder();
                text.Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture));
                text.Append('\t');
                text.Append(message);
                if (exception != null)
                {
                    text.Append(" | ");
                    text.Append(exception.GetType().Name);
                    text.Append(": ");
                    text.Append(exception.Message);
                    text.Append(" | ");
                    text.Append(exception.StackTrace);
                }
                text.AppendLine();
                lock (Sync)
                {
                    File.AppendAllText(path, text.ToString(), new UTF8Encoding(false));
                }
            }
            catch
            {
            }
        }

        public static void Bell(DateTime when, string description, string melodyFile, bool success, string error)
        {
            try
            {
                if (string.IsNullOrEmpty(_dataDir))
                {
                    return;
                }

                string path = Path.Combine(_dataDir, "bells.log");
                RotateIfNeeded(path);
                StringBuilder text = new StringBuilder();
                text.Append(when.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture));
                text.Append('\t');
                text.Append(description);
                text.Append('\t');
                text.Append(melodyFile);
                text.Append('\t');
                text.Append(success ? "OK" : "ERROR");
                if (!success && !string.IsNullOrEmpty(error))
                {
                    text.Append(": ");
                    text.Append(error);
                }
                text.AppendLine();
                lock (Sync)
                {
                    File.AppendAllText(path, text.ToString(), new UTF8Encoding(false));
                }
            }
            catch
            {
            }
        }

        private static void RotateIfNeeded(string path)
        {
            try
            {
                FileInfo info = new FileInfo(path);
                if (info.Exists && info.Length > 512 * 1024)
                {
                    string old = path + ".old";
                    if (File.Exists(old))
                    {
                        File.Delete(old);
                    }
                    File.Move(path, old);
                }
            }
            catch
            {
            }
        }
    }

    public sealed class LoadOutcome
    {
        public AppSettings Settings;
        public string Warning;

        public LoadOutcome(AppSettings settings, string warning)
        {
            Settings = settings;
            Warning = warning;
        }
    }

    public sealed class SettingsStore
    {
        public string DataDir { get; private set; }
        public string SettingsPath { get; private set; }

        public SettingsStore(string dataDir)
        {
            DataDir = dataDir;
            SettingsPath = Path.Combine(dataDir, "settings.txt");
        }

        public LoadOutcome Load()
        {
            bool exists = File.Exists(SettingsPath);
            string backup = SettingsPath + ".bak";
            string pathToRead = exists ? SettingsPath : (File.Exists(backup) ? backup : null);

            AppSettings settings = new AppSettings();
            settings.FirstRun = !exists;
            if (pathToRead == null)
            {
                return new LoadOutcome(settings, null);
            }

            int badLines = 0;
            try
            {
                string[] lines = File.ReadAllLines(pathToRead, Encoding.UTF8);
                int currentDay = -1;
                for (int i = 0; i < lines.Length; i++)
                {
                    string line = lines[i];
                    if (line == null)
                    {
                        continue;
                    }
                    line = line.TrimEnd('\r', '\n');
                    if (line.Length == 0 || line.TrimStart().StartsWith("#", StringComparison.Ordinal))
                    {
                        continue;
                    }

                    int equals = line.IndexOf('=');
                    if (equals < 0)
                    {
                        continue;
                    }

                    string key = line.Substring(0, equals).Trim().ToLowerInvariant();
                    string value = line.Substring(equals + 1);

                    if (key == "enabled")
                    {
                        settings.Enabled = ParseBool(value, true);
                    }
                    else if (key == "volume")
                    {
                        int volume;
                        if (int.TryParse(value.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out volume))
                        {
                            settings.Volume = Math.Max(0, Math.Min(100, volume));
                        }
                    }
                    else if (key == "ringstart")
                    {
                        settings.RingAtStart = ParseBool(value, true);
                    }
                    else if (key == "ringend")
                    {
                        settings.RingAtEnd = ParseBool(value, true);
                    }
                    else if (key == "autostart")
                    {
                        settings.AutoStart = ParseBool(value, false);
                    }
                    else if (key == "minimizetray")
                    {
                        settings.MinimizeToTray = ParseBool(value, true);
                    }
                    else if (key == "firstrun")
                    {
                        settings.FirstRun = ParseBool(value, true);
                    }
                    else if (key == "melodystart")
                    {
                        settings.MelodyStartFile = value.Trim();
                    }
                    else if (key == "melodyend")
                    {
                        settings.MelodyEndFile = value.Trim();
                    }
                    else if (key == "melodystartname")
                    {
                        settings.MelodyStartName = value.Trim();
                    }
                    else if (key == "melodyendname")
                    {
                        settings.MelodyEndName = value.Trim();
                    }
                    else if (key == "theme")
                    {
                        settings.Theme = value.Trim().ToLowerInvariant();
                    }
                    else if (key == "snowflakes")
                    {
                        settings.Snowflakes = ParseBool(value, true);
                    }
                    else if (key == "musicenabled")
                    {
                        settings.MusicEnabled = ParseBool(value, true);
                    }
                    else if (key == "musicvolume")
                    {
                        int musicVolume;
                        if (int.TryParse(value.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out musicVolume))
                        {
                            settings.MusicVolume = Math.Max(0, Math.Min(100, musicVolume));
                        }
                    }
                    else if (key == "voicevolume")
                    {
                        int voiceVolume;
                        if (int.TryParse(value.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out voiceVolume))
                        {
                            settings.VoiceVolume = Math.Max(0, Math.Min(100, voiceVolume));
                        }
                    }
                    else if (key == "shortdays")
                    {
                        for (int d = 0; d < 7; d++)
                        {
                            settings.ShortDays[d] = false;
                        }
                        string[] shortParts = value.Split(new char[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        for (int p = 0; p < shortParts.Length; p++)
                        {
                            int shortDay;
                            if (int.TryParse(shortParts[p], NumberStyles.Integer, CultureInfo.InvariantCulture, out shortDay) &&
                                shortDay >= 0 && shortDay <= 6)
                            {
                                settings.ShortDays[shortDay] = true;
                            }
                        }
                    }
                    else if (key == "music")
                    {
                        string[] parts = value.Split('\t');
                        if (parts.Length < 4)
                        {
                            badLines++;
                            continue;
                        }

                        int musicDay;
                        TimeSpan musicStart;
                        TimeSpan musicEnd;
                        if (!int.TryParse(parts[0].Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out musicDay) ||
                            musicDay < 0 || musicDay > 7 ||
                            !TimeUtil.TryParseTime(parts[1], out musicStart) ||
                            !TimeUtil.TryParseTime(parts[2], out musicEnd) ||
                            musicEnd <= musicStart)
                        {
                            badLines++;
                            continue;
                        }

                        string musicName = parts.Length >= 5 ? parts[4].Trim() : Path.GetFileName(parts[3].Trim());
                        StringBuilder musicComment = new StringBuilder(parts.Length >= 6 ? parts[5] : "");
                        for (int p = 6; p < parts.Length; p++)
                        {
                            musicComment.Append(' ');
                            musicComment.Append(parts[p]);
                        }
                        settings.MusicSchedule.Add(new MusicEntry(musicDay, musicStart, musicEnd,
                            parts[3].Trim(), musicName, musicComment.ToString().Trim()));
                    }
                    else if (key == "playlist" || key == "voice")
                    {
                        string[] parts = value.Split('\t');
                        if (parts.Length < 1 || parts[0].Trim().Length == 0)
                        {
                            badLines++;
                            continue;
                        }
                        string mediaFile = parts[0].Trim();
                        string mediaName = parts.Length >= 2 ? parts[1].Trim() : Path.GetFileName(mediaFile);
                        MediaItem item = new MediaItem(mediaFile, mediaName);
                        if (key == "playlist")
                        {
                            settings.MusicPlaylist.Add(item);
                        }
                        else
                        {
                            settings.VoiceRecordings.Add(item);
                        }
                    }
                    else if (key == "day")
                    {
                        int day;
                        if (int.TryParse(value.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out day) &&
                            day >= 0 && day <= 7)
                        {
                            currentDay = day;
                        }
                    }
                    else if (key == "bell")
                    {
                        if (currentDay < 0)
                        {
                            badLines++;
                            continue;
                        }

                        string[] parts = value.Split('\t');
                        if (parts.Length < 2)
                        {
                            badLines++;
                            continue;
                        }

                        TimeSpan start;
                        TimeSpan end;
                        if (!TimeUtil.TryParseTime(parts[0], out start) ||
                            !TimeUtil.TryParseTime(parts[1], out end) ||
                            end <= start)
                        {
                            badLines++;
                            continue;
                        }

                        string comment = "";
                        if (parts.Length >= 3)
                        {
                            StringBuilder commentBuilder = new StringBuilder(parts[2]);
                            for (int p = 3; p < parts.Length; p++)
                            {
                                commentBuilder.Append(' ');
                                commentBuilder.Append(parts[p]);
                            }
                            comment = commentBuilder.ToString();
                        }

                        DaySchedule targetDay = settings.GetDay(currentDay);
                        if (targetDay != null)
                        {
                            targetDay.Bells.Add(new Bell(start, end, comment));
                        }
                    }
                }

                settings.EnsureSevenDays();
                string warning = badLines > 0
                    ? "При чтении расписания пропущено строк с ошибками: " + badLines.ToString(CultureInfo.InvariantCulture) + "."
                    : null;
                return new LoadOutcome(settings, warning);
            }
            catch (Exception ex)
            {
                Log.Error("Не удалось прочитать настройки: " + pathToRead, ex);
                settings.EnsureSevenDays();
                return new LoadOutcome(settings,
                    "Не удалось прочитать файл настроек. Программа запущена с настройками по умолчанию.");
            }
        }

        public bool Save(AppSettings settings)
        {
            try
            {
                settings.EnsureSevenDays();
                StringBuilder text = new StringBuilder();
                text.AppendLine("# Школьные звонки — файл настроек");
                text.AppendLine("version=2");
                text.AppendLine("enabled=" + BoolText(settings.Enabled));
                text.AppendLine("volume=" + settings.Volume.ToString(CultureInfo.InvariantCulture));
                text.AppendLine("ringstart=" + BoolText(settings.RingAtStart));
                text.AppendLine("ringend=" + BoolText(settings.RingAtEnd));
                text.AppendLine("autostart=" + BoolText(settings.AutoStart));
                text.AppendLine("minimizetray=" + BoolText(settings.MinimizeToTray));
                text.AppendLine("firstrun=" + BoolText(settings.FirstRun));
                text.AppendLine("melodystart=" + (settings.MelodyStartFile == null ? "" : settings.MelodyStartFile));
                text.AppendLine("melodyend=" + (settings.MelodyEndFile == null ? "" : settings.MelodyEndFile));
                text.AppendLine("melodystartname=" + (settings.MelodyStartName == null ? "" : settings.MelodyStartName));
                text.AppendLine("melodyendname=" + (settings.MelodyEndName == null ? "" : settings.MelodyEndName));
                text.AppendLine("theme=" + (settings.Theme == null ? "auto" : settings.Theme));
                text.AppendLine("snowflakes=" + BoolText(settings.Snowflakes));
                text.AppendLine("musicenabled=" + BoolText(settings.MusicEnabled));
                text.AppendLine("musicvolume=" + settings.MusicVolume.ToString(CultureInfo.InvariantCulture));
                text.AppendLine("voicevolume=" + settings.VoiceVolume.ToString(CultureInfo.InvariantCulture));

                StringBuilder shortDays = new StringBuilder();
                for (int d = 0; d < 7; d++)
                {
                    if (settings.ShortDays[d])
                    {
                        if (shortDays.Length > 0)
                        {
                            shortDays.Append(',');
                        }
                        shortDays.Append(d.ToString(CultureInfo.InvariantCulture));
                    }
                }
                text.AppendLine("shortdays=" + shortDays.ToString());

                for (int day = 0; day < 8; day++)
                {
                    text.AppendLine("day=" + day.ToString(CultureInfo.InvariantCulture));
                    DaySchedule schedule = settings.GetDay(day);
                    List<Bell> bells = schedule == null ? new List<Bell>() : schedule.Bells;
                    for (int i = 0; i < bells.Count; i++)
                    {
                        Bell bell = bells[i];
                        text.Append("bell=");
                        text.Append(TimeUtil.FormatTime(bell.Start));
                        text.Append('\t');
                        text.Append(TimeUtil.FormatTime(bell.End));
                        text.Append('\t');
                        text.Append(SanitizeComment(bell.Comment));
                        text.AppendLine();
                    }
                }

                for (int i = 0; i < settings.MusicSchedule.Count; i++)
                {
                    MusicEntry entry = settings.MusicSchedule[i];
                    text.Append("music=");
                    text.Append(entry.Day.ToString(CultureInfo.InvariantCulture));
                    text.Append('\t');
                    text.Append(TimeUtil.FormatTime(entry.Start));
                    text.Append('\t');
                    text.Append(TimeUtil.FormatTime(entry.End));
                    text.Append('\t');
                    text.Append(entry.File);
                    text.Append('\t');
                    text.Append(SanitizeComment(entry.Name));
                    text.Append('\t');
                    text.Append(SanitizeComment(entry.Comment));
                    text.AppendLine();
                }

                for (int i = 0; i < settings.MusicPlaylist.Count; i++)
                {
                    MediaItem item = settings.MusicPlaylist[i];
                    text.Append("playlist=");
                    text.Append(item.File);
                    text.Append('\t');
                    text.Append(SanitizeComment(item.Name));
                    text.AppendLine();
                }

                for (int i = 0; i < settings.VoiceRecordings.Count; i++)
                {
                    MediaItem item = settings.VoiceRecordings[i];
                    text.Append("voice=");
                    text.Append(item.File);
                    text.Append('\t');
                    text.Append(SanitizeComment(item.Name));
                    text.AppendLine();
                }

                string content = text.ToString();
                string temp = SettingsPath + ".tmp";
                File.WriteAllText(temp, content, new UTF8Encoding(false));

                if (File.Exists(SettingsPath))
                {
                    try
                    {
                        File.Copy(SettingsPath, SettingsPath + ".bak", true);
                    }
                    catch
                    {
                    }
                }

                File.Copy(temp, SettingsPath, true);
                File.Delete(temp);
                return true;
            }
            catch (Exception ex)
            {
                Log.Error("Не удалось сохранить настройки", ex);
                return false;
            }
        }

        public string ResolveMelodyPath(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return "";
            }
            if (Path.IsPathRooted(fileName))
            {
                return fileName;
            }
            return Path.Combine(DataDir, fileName);
        }

        public string ImportMelody(string sourcePath, bool forStart, out string error)
        {
            return ImportMedia(sourcePath, forStart ? "melody_start_" : "melody_end_", out error);
        }

        public string ImportMedia(string sourcePath, string prefix, out string error)
        {
            error = null;
            try
            {
                if (!File.Exists(sourcePath))
                {
                    error = "Файл не найден.";
                    return null;
                }

                string extension = Path.GetExtension(sourcePath);
                if (string.IsNullOrEmpty(extension))
                {
                    error = "Не удалось определить формат файла.";
                    return null;
                }
                extension = extension.ToLowerInvariant();
                if (extension != ".mp3" && extension != ".wav" && extension != ".wma" && extension != ".m4a")
                {
                    error = "Поддерживаются файлы MP3, WAV, WMA и M4A.";
                    return null;
                }

                string stamp = DateTime.Now.ToString("yyyyMMdd_HHmmss_fff", CultureInfo.InvariantCulture);
                string fileName = prefix + stamp + extension;
                string destination = Path.Combine(DataDir, fileName);
                int suffix = 1;
                while (File.Exists(destination))
                {
                    fileName = prefix + stamp + "_" + suffix.ToString(CultureInfo.InvariantCulture) + extension;
                    destination = Path.Combine(DataDir, fileName);
                    suffix++;
                }

                File.Copy(sourcePath, destination, false);
                return fileName;
            }
            catch (Exception ex)
            {
                Log.Error("Не удалось скопировать аудиофайл", ex);
                error = ex.Message;
                return null;
            }
        }

        public string CreateVoiceFileName()
        {
            string stamp = DateTime.Now.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture);
            string fileName = "voice_" + stamp + ".wav";
            string destination = Path.Combine(DataDir, fileName);
            int suffix = 1;
            while (File.Exists(destination))
            {
                fileName = "voice_" + stamp + "_" + suffix.ToString(CultureInfo.InvariantCulture) + ".wav";
                destination = Path.Combine(DataDir, fileName);
                suffix++;
            }
            return fileName;
        }

        public void CleanupMelodies(AppSettings settings)
        {
            CleanupMedia(settings);
        }

        public void CleanupMedia(AppSettings settings)
        {
            try
            {
                HashSet<string> keep = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                if (!string.IsNullOrEmpty(settings.MelodyStartFile))
                {
                    keep.Add(Path.GetFileName(settings.MelodyStartFile));
                }
                if (!string.IsNullOrEmpty(settings.MelodyEndFile))
                {
                    keep.Add(Path.GetFileName(settings.MelodyEndFile));
                }
                for (int i = 0; i < settings.MusicSchedule.Count; i++)
                {
                    keep.Add(Path.GetFileName(settings.MusicSchedule[i].File));
                }
                for (int i = 0; i < settings.MusicPlaylist.Count; i++)
                {
                    keep.Add(Path.GetFileName(settings.MusicPlaylist[i].File));
                }
                for (int i = 0; i < settings.VoiceRecordings.Count; i++)
                {
                    keep.Add(Path.GetFileName(settings.VoiceRecordings[i].File));
                }
                keep.Add("default_bell.wav");

                string[] files = Directory.GetFiles(DataDir);
                for (int i = 0; i < files.Length; i++)
                {
                    string name = Path.GetFileName(files[i]);
                    if (name.StartsWith("melody_start_", StringComparison.OrdinalIgnoreCase) ||
                        name.StartsWith("melody_end_", StringComparison.OrdinalIgnoreCase) ||
                        name.StartsWith("music_", StringComparison.OrdinalIgnoreCase) ||
                        name.StartsWith("voice_", StringComparison.OrdinalIgnoreCase) ||
                        name.EndsWith(".tmp", StringComparison.OrdinalIgnoreCase))
                    {
                        if (!keep.Contains(name))
                        {
                            try
                            {
                                File.Delete(files[i]);
                            }
                            catch
                            {
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error("Не удалось удалить старые аудиофайлы", ex);
            }
        }

        private static bool ParseBool(string text, bool defaultValue)
        {
            if (text == null)
            {
                return defaultValue;
            }
            string value = text.Trim().ToLowerInvariant();
            if (value == "1" || value == "true" || value == "yes" || value == "on")
            {
                return true;
            }
            if (value == "0" || value == "false" || value == "no" || value == "off")
            {
                return false;
            }
            return defaultValue;
        }

        private static string BoolText(bool value)
        {
            return value ? "1" : "0";
        }

        private static string SanitizeComment(string comment)
        {
            if (string.IsNullOrEmpty(comment))
            {
                return "";
            }
            StringBuilder text = new StringBuilder(comment.Length);
            for (int i = 0; i < comment.Length; i++)
            {
                char c = comment[i];
                if (c == '\t' || c == '\r' || c == '\n')
                {
                    text.Append(' ');
                }
                else
                {
                    text.Append(c);
                }
            }
            return text.ToString().Trim();
        }
    }

    public static class DefaultBellGenerator
    {
        public static string EnsureDefaultBell(string dataDir)
        {
            string path = Path.Combine(dataDir, "default_bell.wav");
            if (File.Exists(path))
            {
                return path;
            }

            try
            {
                Generate(path);
            }
            catch (Exception ex)
            {
                Log.Error("Не удалось создать встроенный звонок", ex);
            }
            return path;
        }

        private static void Generate(string path)
        {
            const int sampleRate = 22050;
            const double duration = 3.8;
            int sampleCount = (int)(sampleRate * duration);
            short[] samples = new short[sampleCount];

            AddTone(samples, sampleRate, 0.0, 1.6, 880.0, 0.40);
            AddTone(samples, sampleRate, 0.62, 2.2, 659.25, 0.44);
            AddTone(samples, sampleRate, 0.62, 2.2, 1318.5, 0.10);
            AddTone(samples, sampleRate, 0.0, 1.6, 1760.0, 0.06);

            double peak = 1.0;
            for (int i = 0; i < samples.Length; i++)
            {
                double value = Math.Abs(samples[i] / 32767.0);
                if (value > peak)
                {
                    peak = value;
                }
            }
            double normalize = peak > 0.92 ? 0.92 / peak : 1.0;

            string temp = path + ".tmp";
            using (FileStream stream = new FileStream(temp, FileMode.Create, FileAccess.Write))
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                int dataBytes = samples.Length * 2;
                writer.Write(new char[] { 'R', 'I', 'F', 'F' });
                writer.Write(36 + dataBytes);
                writer.Write(new char[] { 'W', 'A', 'V', 'E' });
                writer.Write(new char[] { 'f', 'm', 't', ' ' });
                writer.Write(16);
                writer.Write((short)1);
                writer.Write((short)1);
                writer.Write(sampleRate);
                writer.Write(sampleRate * 2);
                writer.Write((short)2);
                writer.Write((short)16);
                writer.Write(new char[] { 'd', 'a', 't', 'a' });
                writer.Write(dataBytes);
                for (int i = 0; i < samples.Length; i++)
                {
                    int value = (int)Math.Round(samples[i] * normalize);
                    if (value > 32767)
                    {
                        value = 32767;
                    }
                    if (value < -32768)
                    {
                        value = -32768;
                    }
                    writer.Write((short)value);
                }
            }

            if (File.Exists(path))
            {
                File.Delete(path);
            }
            File.Move(temp, path);
        }

        private static void AddTone(short[] samples, int sampleRate, double startSeconds, double durationSeconds,
            double frequency, double amplitude)
        {
            int start = (int)(startSeconds * sampleRate);
            int length = (int)(durationSeconds * sampleRate);
            int end = Math.Min(samples.Length, start + length);
            for (int i = start; i < end; i++)
            {
                double t = (double)(i - start) / sampleRate;
                double attack = Math.Min(1.0, t / 0.006);
                double decay = Math.Exp(-t * 2.2);
                double envelope = attack * decay;
                double phase = 2.0 * Math.PI * frequency * t;
                double value = Math.Sin(phase) + 0.30 * Math.Sin(phase * 2.0) + 0.10 * Math.Sin(phase * 3.0);
                double mixed = samples[i] / 32767.0 + value * amplitude * envelope;
                if (mixed > 1.0)
                {
                    mixed = 1.0;
                }
                if (mixed < -1.0)
                {
                    mixed = -1.0;
                }
                samples[i] = (short)Math.Round(mixed * 32767.0);
            }
        }
    }

    public static class StartupManager
    {
        private const string RunKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
        private const string ValueName = "SchoolBells";

        public static bool IsEnabled()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RunKey, false))
                {
                    if (key == null)
                    {
                        return false;
                    }
                    object value = key.GetValue(ValueName);
                    return value != null && value.ToString().Length > 0;
                }
            }
            catch (Exception ex)
            {
                Log.Error("Не удалось прочитать автозапуск", ex);
                return false;
            }
        }

        public static bool SetEnabled(bool enabled, string exePath, out string error)
        {
            error = null;
            try
            {
                using (RegistryKey key = Registry.CurrentUser.CreateSubKey(RunKey))
                {
                    if (key == null)
                    {
                        error = "Не удалось открыть раздел автозапуска.";
                        return false;
                    }

                    if (enabled)
                    {
                        string command = "\"" + exePath + "\" --autostart";
                        key.SetValue(ValueName, command, RegistryValueKind.String);
                    }
                    else
                    {
                        key.DeleteValue(ValueName, false);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Log.Error("Не удалось изменить автозапуск", ex);
                error = ex.Message;
                return false;
            }
        }
    }
}
