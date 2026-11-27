using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace SchoolBells
{
    public sealed class PlaybackEngine : IDisposable
    {
        private object _wmp;
        private bool _wmpTried;
        private bool _wmpAvailable;
        private int _volume;
        private readonly MciPlayer _mci;
        private string _lastError;

        public PlaybackEngine(int volume)
        {
            _volume = ClampVolume(volume);
            _mci = new MciPlayer();
        }

        public string LastError
        {
            get { return _lastError; }
        }

        public int Volume
        {
            get { return _volume; }
            set
            {
                _volume = ClampVolume(value);
                if (_wmpAvailable)
                {
                    TrySetWmpVolume();
                }
            }
        }

        public void Prewarm()
        {
            EnsureWmp();
        }

        public bool Play(string path, string fallbackPath, out string error)
        {
            error = null;
            _lastError = null;
            Stop();

            if (string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                if (!string.IsNullOrEmpty(fallbackPath) && File.Exists(fallbackPath))
                {
                    path = fallbackPath;
                }
                else
                {
                    error = "Файл мелодии не найден.";
                    _lastError = error;
                    return false;
                }
            }

            string wmpError;
            if (TryPlayWmp(path, out wmpError))
            {
                return true;
            }

            string mciError;
            if (TryPlayMci(path, out mciError))
            {
                return true;
            }

            if (!string.IsNullOrEmpty(fallbackPath) && File.Exists(fallbackPath) &&
                !string.Equals(Path.GetFullPath(path), Path.GetFullPath(fallbackPath), StringComparison.OrdinalIgnoreCase))
            {
                if (TryPlayWmp(fallbackPath, out wmpError) || TryPlayMci(fallbackPath, out mciError))
                {
                    return true;
                }
            }

            error = "Не удалось воспроизвести мелодию.";
            if (!string.IsNullOrEmpty(wmpError))
            {
                error += " " + wmpError;
            }
            if (!string.IsNullOrEmpty(mciError))
            {
                error += " " + mciError;
            }
            _lastError = error;
            return false;
        }

        public void Stop()
        {
            if (_wmp != null)
            {
                try
                {
                    object controls = GetProperty(_wmp, "controls");
                    Invoke(controls, "stop", null);
                }
                catch
                {
                }
            }

            try
            {
                _mci.Stop();
            }
            catch
            {
            }
        }

        public void Dispose()
        {
            Stop();

            if (_wmp != null)
            {
                try
                {
                    Invoke(_wmp, "close", null);
                }
                catch
                {
                }

                try
                {
                    Marshal.FinalReleaseComObject(_wmp);
                }
                catch
                {
                }
                _wmp = null;
                _wmpAvailable = false;
            }

            _mci.Dispose();
        }

        private bool TryPlayWmp(string path, out string error)
        {
            error = null;
            if (!EnsureWmp())
            {
                error = "Проигрыватель Windows Media недоступен.";
                return false;
            }

            try
            {
                TrySetWmpVolume();
                SetProperty(_wmp, "URL", path);
                object controls = GetProperty(_wmp, "controls");
                Invoke(controls, "play", null);
                return true;
            }
            catch (Exception ex)
            {
                error = "Windows Media: " + ex.Message;
                Log.Error("Ошибка воспроизведения через Windows Media: " + path, ex);
                return false;
            }
        }

        private bool TryPlayMci(string path, out string error)
        {
            error = null;
            try
            {
                if (_mci.Play(path, _volume, out error))
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                error = ex.Message;
                Log.Error("Ошибка воспроизведения через MCI: " + path, ex);
            }
            return false;
        }

        private bool EnsureWmp()
        {
            if (_wmpTried)
            {
                return _wmpAvailable;
            }
            _wmpTried = true;

            try
            {
                Type type = Type.GetTypeFromProgID("WMPlayer.OCX");
                if (type == null)
                {
                    _wmpAvailable = false;
                    return false;
                }

                _wmp = Activator.CreateInstance(type);
                try
                {
                    SetProperty(_wmp, "URL", "");
                }
                catch
                {
                }

                object settings = GetProperty(_wmp, "settings");
                SetProperty(settings, "autoStart", false);
                SetProperty(settings, "mute", false);
                SetProperty(settings, "playCount", 1);
                SetProperty(settings, "volume", _volume);
                _wmpAvailable = true;
                return true;
            }
            catch (Exception ex)
            {
                Log.Error("Не удалось создать проигрыватель Windows Media", ex);
                _wmpAvailable = false;
                if (_wmp != null)
                {
                    try
                    {
                        Marshal.FinalReleaseComObject(_wmp);
                    }
                    catch
                    {
                    }
                    _wmp = null;
                }
                return false;
            }
        }

        private void TrySetWmpVolume()
        {
            try
            {
                object settings = GetProperty(_wmp, "settings");
                SetProperty(settings, "volume", _volume);
            }
            catch (Exception ex)
            {
                Log.Error("Не удалось установить громкость Windows Media", ex);
            }
        }

        private static object GetProperty(object target, string name)
        {
            return target.GetType().InvokeMember(
                name,
                BindingFlags.GetProperty,
                null,
                target,
                null);
        }

        private static void SetProperty(object target, string name, object value)
        {
            target.GetType().InvokeMember(
                name,
                BindingFlags.SetProperty,
                null,
                target,
                new object[] { value });
        }

        private static object Invoke(object target, string name, object[] args)
        {
            return target.GetType().InvokeMember(
                name,
                BindingFlags.InvokeMethod,
                null,
                target,
                args);
        }

        private static int ClampVolume(int volume)
        {
            if (volume < 0)
            {
                return 0;
            }
            if (volume > 100)
            {
                return 100;
            }
            return volume;
        }
    }

    internal sealed class MciPlayer : IDisposable
    {
        private const string Alias = "schoolbell";
        private bool _open;

        public bool Play(string path, int volume, out string error)
        {
            error = null;
            Close();

            string commandPath = ShortPath(path);
            string device = path.EndsWith(".wav", StringComparison.OrdinalIgnoreCase)
                ? "waveaudio"
                : "mpegvideo";

            string openCommand = "open \"" + commandPath + "\" type " + device + " alias " + Alias;
            int result = Send(openCommand);
            if (result != 0)
            {
                error = "MCI open: " + ErrorText(result);
                return false;
            }
            _open = true;

            Send("set " + Alias + " time format milliseconds");
            int scaledVolume = volume * 10;
            Send("setaudio " + Alias + " volume to " + scaledVolume.ToString());

            result = Send("play " + Alias + " from 0");
            if (result != 0)
            {
                error = "MCI play: " + ErrorText(result);
                Close();
                return false;
            }
            return true;
        }

        public void Stop()
        {
            if (!_open)
            {
                return;
            }
            Send("stop " + Alias);
            Send("seek " + Alias + " to start");
        }

        public void Close()
        {
            if (!_open)
            {
                return;
            }
            Send("stop " + Alias);
            Send("close " + Alias);
            _open = false;
        }

        public void Dispose()
        {
            Close();
        }

        private static int Send(string command)
        {
            try
            {
                return MciNative.mciSendString(command, null, 0, IntPtr.Zero);
            }
            catch
            {
                return -1;
            }
        }

        private static string ErrorText(int code)
        {
            try
            {
                StringBuilder text = new StringBuilder(256);
                if (MciNative.mciGetErrorString(code, text, text.Capacity))
                {
                    return text.ToString();
                }
            }
            catch
            {
            }
            return "код " + code.ToString();
        }

        private static string ShortPath(string path)
        {
            try
            {
                StringBuilder shortPath = new StringBuilder(1024);
                int length = MciNative.GetShortPathName(path, shortPath, shortPath.Capacity);
                if (length > 0 && length < shortPath.Capacity)
                {
                    return shortPath.ToString();
                }
            }
            catch
            {
            }
            return path;
        }
    }

    internal static class MciNative
    {
        [DllImport("winmm.dll", CharSet = CharSet.Ansi, SetLastError = false)]
        public static extern int mciSendString(string command, StringBuilder returnValue, int returnLength, IntPtr callback);

        [DllImport("winmm.dll", CharSet = CharSet.Ansi, SetLastError = false)]
        public static extern bool mciGetErrorString(int errorCode, StringBuilder errorText, int errorTextSize);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = false)]
        public static extern int GetShortPathName(string longPath, StringBuilder shortPath, int shortPathLength);
    }
}
