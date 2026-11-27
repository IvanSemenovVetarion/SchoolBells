using System;
using System.IO;
using System.Text;

namespace SchoolBells
{
    public sealed class MicrophoneRecorder : IDisposable
    {
        private const string Alias = "schoolmic";
        private bool _open;

        public bool IsRecording
        {
            get { return _open; }
        }

        public bool Start(out string error)
        {
            error = null;
            Close();

            int result = Send("open new type waveaudio alias " + Alias);
            if (result != 0)
            {
                error = "Не удалось открыть микрофон. Код MCI: " + result.ToString() +
                        ". Проверьте, что микрофон подключён и разрешён в параметрах Windows.";
                return false;
            }
            _open = true;

            Send("set " + Alias + " time format milliseconds");
            Send("set " + Alias + " channels 1 samplespersec 22050 bitspersample 16");

            result = Send("record " + Alias);
            if (result != 0)
            {
                error = "Не удалось начать запись. Код MCI: " + result.ToString() +
                        ". Возможно, микрофон занят другой программой.";
                Close();
                return false;
            }
            return true;
        }

        public bool StopAndSave(string path, out string error)
        {
            error = null;
            if (!_open)
            {
                error = "Запись не была начата.";
                return false;
            }

            try
            {
                Send("stop " + Alias);
                string commandPath = ShortPath(path);
                int result = Send("save " + Alias + " \"" + commandPath + "\"");
                if (result != 0)
                {
                    error = "Не удалось сохранить запись. Код MCI: " + result.ToString() + ".";
                    Close();
                    return false;
                }
            }
            catch (Exception ex)
            {
                error = ex.Message;
                Close();
                return false;
            }

            Close();
            return File.Exists(path);
        }

        public void Cancel()
        {
            if (!_open)
            {
                return;
            }
            Send("stop " + Alias);
            Close();
        }

        public void Close()
        {
            if (!_open)
            {
                return;
            }
            Send("close " + Alias);
            _open = false;
        }

        public void Dispose()
        {
            Cancel();
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
}
