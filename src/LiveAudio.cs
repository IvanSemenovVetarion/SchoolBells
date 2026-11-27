using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace SchoolBells
{
    public sealed class LiveMicrophone : IDisposable
    {
        private const int SampleRate = 22050;
        private const int Channels = 1;
        private const int BitsPerSample = 16;
        private const int BufferBytes = 2048;
        private const int InputBufferCount = 4;
        private const int OutputBufferCount = 3;

        private const uint WaveMapper = 0xFFFFFFFF;
        private const uint CallbackFunction = 0x00030000;
        private const uint WaveFormatPcm = 1;
        private const int WimData = 0x3C0;
        private const int WomDone = 0x3BC;
        private const int WhdrDone = 0x00000001;

        private readonly object _sync = new object();
        private readonly byte[] _ring = new byte[SampleRate * 2 * 2];
        private readonly byte[] _scratch = new byte[BufferBytes];

        private IntPtr _waveIn;
        private IntPtr _waveOut;
        private WaveCallback _inCallback;
        private WaveCallback _outCallback;
        private IntPtr[] _inHeaders;
        private IntPtr[] _inData;
        private IntPtr[] _outHeaders;
        private IntPtr[] _outData;
        private int _ringRead;
        private int _ringWrite;
        private int _ringCount;
        private bool _running;
        private double _level;
        private double _gain = 1.0;

        public event Action<double> LevelChanged;

        public double Gain
        {
            get { return _gain; }
            set
            {
                if (value < 0)
                {
                    value = 0;
                }
                if (value > 1.5)
                {
                    value = 1.5;
                }
                _gain = value;
            }
        }

        public bool IsRunning
        {
            get { return _running; }
        }

        public double Level
        {
            get { return _level; }
        }

        public bool Start(out string error)
        {
            error = null;
            if (_running)
            {
                return true;
            }

            if (NativeMethods.waveInGetNumDevs() == 0)
            {
                error = "Микрофон не найден. Подключите микрофон и проверьте параметры звука Windows.";
                return false;
            }
            if (NativeMethods.waveOutGetNumDevs() == 0)
            {
                error = "Устройство воспроизведения не найдено.";
                return false;
            }

            try
            {
                _inCallback = WaveInCallback;
                _outCallback = WaveOutCallback;
                _inHeaders = new IntPtr[InputBufferCount];
                _inData = new IntPtr[InputBufferCount];
                _outHeaders = new IntPtr[OutputBufferCount];
                _outData = new IntPtr[OutputBufferCount];
                _ringRead = 0;
                _ringWrite = 0;
                _ringCount = 0;
                _level = 0;

                WAVEFORMATEX format = new WAVEFORMATEX();
                format.wFormatTag = (short)WaveFormatPcm;
                format.nChannels = (short)Channels;
                format.nSamplesPerSec = SampleRate;
                format.wBitsPerSample = (short)BitsPerSample;
                format.nBlockAlign = (short)(Channels * BitsPerSample / 8);
                format.nAvgBytesPerSec = SampleRate * format.nBlockAlign;
                format.cbSize = 0;

                uint result = NativeMethods.waveInOpen(out _waveIn, WaveMapper, ref format,
                    _inCallback, IntPtr.Zero, CallbackFunction);
                if (result != 0)
                {
                    error = "Не удалось открыть микрофон (код " + result.ToString() + "). " +
                            "Проверьте, что микрофон разрешён в параметрах Windows.";
                    Cleanup();
                    return false;
                }

                for (int i = 0; i < InputBufferCount; i++)
                {
                    _inHeaders[i] = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(WAVEHDR)));
                    _inData[i] = Marshal.AllocHGlobal(BufferBytes);
                    ZeroMemory(_inData[i], BufferBytes);
                    WAVEHDR header = new WAVEHDR();
                    header.lpData = _inData[i];
                    header.dwBufferLength = BufferBytes;
                    header.dwUser = (IntPtr)i;
                    Marshal.StructureToPtr(header, _inHeaders[i], false);
                    result = NativeMethods.waveInPrepareHeader(_waveIn, _inHeaders[i], (uint)Marshal.SizeOf(typeof(WAVEHDR)));
                    if (result != 0)
                    {
                        error = "Не удалось подготовить буфер микрофона (код " + result.ToString() + ").";
                        Cleanup();
                        return false;
                    }
                    result = NativeMethods.waveInAddBuffer(_waveIn, _inHeaders[i], (uint)Marshal.SizeOf(typeof(WAVEHDR)));
                    if (result != 0)
                    {
                        error = "Не удалось запустить буфер микрофона (код " + result.ToString() + ").";
                        Cleanup();
                        return false;
                    }
                }

                result = NativeMethods.waveOutOpen(out _waveOut, WaveMapper, ref format,
                    _outCallback, IntPtr.Zero, CallbackFunction);
                if (result != 0)
                {
                    error = "Не удалось открыть устройство воспроизведения (код " + result.ToString() + ").";
                    Cleanup();
                    return false;
                }

                for (int i = 0; i < OutputBufferCount; i++)
                {
                    _outHeaders[i] = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(WAVEHDR)));
                    _outData[i] = Marshal.AllocHGlobal(BufferBytes);
                    ZeroMemory(_outData[i], BufferBytes);
                    WAVEHDR header = new WAVEHDR();
                    header.lpData = _outData[i];
                    header.dwBufferLength = BufferBytes;
                    header.dwUser = (IntPtr)i;
                    Marshal.StructureToPtr(header, _outHeaders[i], false);
                    result = NativeMethods.waveOutPrepareHeader(_waveOut, _outHeaders[i], (uint)Marshal.SizeOf(typeof(WAVEHDR)));
                    if (result != 0)
                    {
                        error = "Не удалось подготовить буфер воспроизведения (код " + result.ToString() + ").";
                        Cleanup();
                        return false;
                    }
                }

                _running = true;
                for (int i = 0; i < OutputBufferCount; i++)
                {
                    result = NativeMethods.waveOutWrite(_waveOut, _outHeaders[i], (uint)Marshal.SizeOf(typeof(WAVEHDR)));
                    if (result != 0)
                    {
                        error = "Не удалось запустить воспроизведение (код " + result.ToString() + ").";
                        _running = false;
                        Cleanup();
                        return false;
                    }
                }

                result = NativeMethods.waveInStart(_waveIn);
                if (result != 0)
                {
                    error = "Не удалось начать запись с микрофона (код " + result.ToString() + ").";
                    _running = false;
                    Cleanup();
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                _running = false;
                Cleanup();
                return false;
            }
        }

        public void Stop()
        {
            if (!_running && _waveIn == IntPtr.Zero && _waveOut == IntPtr.Zero)
            {
                return;
            }
            _running = false;
            Thread.Sleep(30);
            Cleanup();
            _level = 0;
            Action<double> handler = LevelChanged;
            if (handler != null)
            {
                try
                {
                    handler(0);
                }
                catch
                {
                }
            }
        }

        public void Dispose()
        {
            Stop();
        }

        private void Cleanup()
        {
            if (_waveIn != IntPtr.Zero)
            {
                try
                {
                    NativeMethods.waveInStop(_waveIn);
                    NativeMethods.waveInReset(_waveIn);
                }
                catch
                {
                }
                if (_inHeaders != null)
                {
                    for (int i = 0; i < _inHeaders.Length; i++)
                    {
                        if (_inHeaders[i] != IntPtr.Zero)
                        {
                            try
                            {
                                NativeMethods.waveInUnprepareHeader(_waveIn, _inHeaders[i],
                                    (uint)Marshal.SizeOf(typeof(WAVEHDR)));
                            }
                            catch
                            {
                            }
                        }
                    }
                }
                try
                {
                    NativeMethods.waveInClose(_waveIn);
                }
                catch
                {
                }
                _waveIn = IntPtr.Zero;
            }

            if (_waveOut != IntPtr.Zero)
            {
                try
                {
                    NativeMethods.waveOutReset(_waveOut);
                }
                catch
                {
                }
                if (_outHeaders != null)
                {
                    for (int i = 0; i < _outHeaders.Length; i++)
                    {
                        if (_outHeaders[i] != IntPtr.Zero)
                        {
                            try
                            {
                                NativeMethods.waveOutUnprepareHeader(_waveOut, _outHeaders[i],
                                    (uint)Marshal.SizeOf(typeof(WAVEHDR)));
                            }
                            catch
                            {
                            }
                        }
                    }
                }
                try
                {
                    NativeMethods.waveOutClose(_waveOut);
                }
                catch
                {
                }
                _waveOut = IntPtr.Zero;
            }

            FreeBuffers();
        }

        private void FreeBuffers()
        {
            if (_inHeaders != null)
            {
                for (int i = 0; i < _inHeaders.Length; i++)
                {
                    if (_inHeaders[i] != IntPtr.Zero)
                    {
                        Marshal.FreeHGlobal(_inHeaders[i]);
                        _inHeaders[i] = IntPtr.Zero;
                    }
                }
            }
            if (_inData != null)
            {
                for (int i = 0; i < _inData.Length; i++)
                {
                    if (_inData[i] != IntPtr.Zero)
                    {
                        Marshal.FreeHGlobal(_inData[i]);
                        _inData[i] = IntPtr.Zero;
                    }
                }
            }
            if (_outHeaders != null)
            {
                for (int i = 0; i < _outHeaders.Length; i++)
                {
                    if (_outHeaders[i] != IntPtr.Zero)
                    {
                        Marshal.FreeHGlobal(_outHeaders[i]);
                        _outHeaders[i] = IntPtr.Zero;
                    }
                }
            }
            if (_outData != null)
            {
                for (int i = 0; i < _outData.Length; i++)
                {
                    if (_outData[i] != IntPtr.Zero)
                    {
                        Marshal.FreeHGlobal(_outData[i]);
                        _outData[i] = IntPtr.Zero;
                    }
                }
            }
            _inHeaders = null;
            _inData = null;
            _outHeaders = null;
            _outData = null;
        }

        private void WaveInCallback(IntPtr hwi, uint message, IntPtr instance, IntPtr param1, IntPtr param2)
        {
            if (message != WimData || !_running || param1 == IntPtr.Zero)
            {
                return;
            }

            try
            {
                WAVEHDR header = (WAVEHDR)Marshal.PtrToStructure(param1, typeof(WAVEHDR));
                int bytes = header.dwBytesRecorded;
                if (bytes > 0 && bytes <= header.dwBufferLength)
                {
                    byte[] data = new byte[bytes];
                    Marshal.Copy(header.lpData, data, 0, bytes);
                    lock (_sync)
                    {
                        WriteRing(data, bytes);
                    }
                    UpdateLevel(data, bytes);
                }

                if (_running)
                {
                    header.dwBytesRecorded = 0;
                    Marshal.StructureToPtr(header, param1, false);
                    NativeMethods.waveInAddBuffer(_waveIn, param1, (uint)Marshal.SizeOf(typeof(WAVEHDR)));
                }
            }
            catch
            {
            }
        }

        private void WaveOutCallback(IntPtr hwo, uint message, IntPtr instance, IntPtr param1, IntPtr param2)
        {
            if (message != WomDone || !_running || param1 == IntPtr.Zero)
            {
                return;
            }

            try
            {
                WAVEHDR header = (WAVEHDR)Marshal.PtrToStructure(param1, typeof(WAVEHDR));
                lock (_sync)
                {
                    FillScratch();
                    Marshal.Copy(_scratch, 0, header.lpData, BufferBytes);
                }
                header.dwBufferLength = BufferBytes;
                header.dwFlags &= ~WhdrDone;
                Marshal.StructureToPtr(header, param1, false);
                NativeMethods.waveOutWrite(_waveOut, param1, (uint)Marshal.SizeOf(typeof(WAVEHDR)));
            }
            catch
            {
            }
        }

        private void WriteRing(byte[] data, int count)
        {
            if (count >= _ring.Length)
            {
                Buffer.BlockCopy(data, count - _ring.Length, _ring, 0, _ring.Length);
                _ringRead = 0;
                _ringWrite = 0;
                _ringCount = _ring.Length;
                return;
            }

            int overflow = _ringCount + count - _ring.Length;
            if (overflow > 0)
            {
                _ringRead = (_ringRead + overflow) % _ring.Length;
                _ringCount -= overflow;
            }

            int first = Math.Min(count, _ring.Length - _ringWrite);
            Buffer.BlockCopy(data, 0, _ring, _ringWrite, first);
            int second = count - first;
            if (second > 0)
            {
                Buffer.BlockCopy(data, first, _ring, 0, second);
            }
            _ringWrite = (_ringWrite + count) % _ring.Length;
            _ringCount += count;
        }

        private void FillScratch()
        {
            for (int i = 0; i < _scratch.Length; i++)
            {
                _scratch[i] = 0;
            }

            int count = Math.Min(_scratch.Length, _ringCount);
            if (count <= 0)
            {
                return;
            }

            int first = Math.Min(count, _ring.Length - _ringRead);
            Buffer.BlockCopy(_ring, _ringRead, _scratch, 0, first);
            int second = count - first;
            if (second > 0)
            {
                Buffer.BlockCopy(_ring, 0, _scratch, first, second);
            }
            _ringRead = (_ringRead + count) % _ring.Length;
            _ringCount -= count;
            ApplyGain();
        }

        private void ApplyGain()
        {
            if (Math.Abs(_gain - 1.0) < 0.01)
            {
                return;
            }
            for (int i = 0; i + 1 < _scratch.Length; i += 2)
            {
                short sample = (short)(_scratch[i] | (_scratch[i + 1] << 8));
                int value = (int)Math.Round(sample * _gain);
                if (value > 32767)
                {
                    value = 32767;
                }
                if (value < -32768)
                {
                    value = -32768;
                }
                _scratch[i] = (byte)(value & 0xFF);
                _scratch[i + 1] = (byte)((value >> 8) & 0xFF);
            }
        }

        private void UpdateLevel(byte[] data, int count)
        {
            int peak = 0;
            for (int i = 0; i + 1 < count; i += 2)
            {
                short sample = (short)(data[i] | (data[i + 1] << 8));
                int value = sample < 0 ? -sample : sample;
                if (value > peak)
                {
                    peak = value;
                }
            }

            double target = Math.Min(1.0, peak / 10000.0);
            _level = _level * 0.55 + target * 0.45;
            Action<double> handler = LevelChanged;
            if (handler != null)
            {
                try
                {
                    handler(_level);
                }
                catch
                {
                }
            }
        }

        private static void ZeroMemory(IntPtr pointer, int count)
        {
            for (int i = 0; i < count; i++)
            {
                Marshal.WriteByte(pointer, i, 0);
            }
        }

        private delegate void WaveCallback(IntPtr handle, uint message, IntPtr instance, IntPtr param1, IntPtr param2);

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct WAVEFORMATEX
        {
            public short wFormatTag;
            public short nChannels;
            public int nSamplesPerSec;
            public int nAvgBytesPerSec;
            public short nBlockAlign;
            public short wBitsPerSample;
            public short cbSize;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct WAVEHDR
        {
            public IntPtr lpData;
            public int dwBufferLength;
            public int dwBytesRecorded;
            public IntPtr dwUser;
            public int dwFlags;
            public int dwLoops;
            public IntPtr lpNext;
            public IntPtr reserved;
        }

        private static class NativeMethods
        {
            [DllImport("winmm.dll")]
            public static extern uint waveInGetNumDevs();

            [DllImport("winmm.dll")]
            public static extern uint waveOutGetNumDevs();

            [DllImport("winmm.dll")]
            public static extern uint waveInOpen(out IntPtr handle, uint deviceId, ref WAVEFORMATEX format,
                WaveCallback callback, IntPtr instance, uint flags);

            [DllImport("winmm.dll")]
            public static extern uint waveOutOpen(out IntPtr handle, uint deviceId, ref WAVEFORMATEX format,
                WaveCallback callback, IntPtr instance, uint flags);

            [DllImport("winmm.dll")]
            public static extern uint waveInPrepareHeader(IntPtr handle, IntPtr header, uint size);

            [DllImport("winmm.dll")]
            public static extern uint waveInUnprepareHeader(IntPtr handle, IntPtr header, uint size);

            [DllImport("winmm.dll")]
            public static extern uint waveInAddBuffer(IntPtr handle, IntPtr header, uint size);

            [DllImport("winmm.dll")]
            public static extern uint waveInStart(IntPtr handle);

            [DllImport("winmm.dll")]
            public static extern uint waveInStop(IntPtr handle);

            [DllImport("winmm.dll")]
            public static extern uint waveInReset(IntPtr handle);

            [DllImport("winmm.dll")]
            public static extern uint waveInClose(IntPtr handle);

            [DllImport("winmm.dll")]
            public static extern uint waveOutPrepareHeader(IntPtr handle, IntPtr header, uint size);

            [DllImport("winmm.dll")]
            public static extern uint waveOutUnprepareHeader(IntPtr handle, IntPtr header, uint size);

            [DllImport("winmm.dll")]
            public static extern uint waveOutWrite(IntPtr handle, IntPtr header, uint size);

            [DllImport("winmm.dll")]
            public static extern uint waveOutReset(IntPtr handle);

            [DllImport("winmm.dll")]
            public static extern uint waveOutClose(IntPtr handle);
        }
    }
}
