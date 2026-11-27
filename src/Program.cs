using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace SchoolBells
{
    internal static class Program
    {
        [STAThread]
        private static void Main(string[] args)
        {
            bool createdNew = false;
            Mutex mutex = new Mutex(true, @"Local\SchoolBells_SingleInstance", out createdNew);
            if (!createdNew)
            {
                MessageBox.Show(
                    "Программа «Школьные звонки» уже запущена.\n\nОна работает в фоне — посмотрите значок в области уведомлений (рядом с часами).",
                    "Школьные звонки",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
                Application.ThreadException += delegate(object sender, ThreadExceptionEventArgs e)
                {
                    Log.Error("Необработанная ошибка интерфейса", e.Exception);
                    try
                    {
                        MessageBox.Show(
                            "Произошла ошибка:\n\n" + e.Exception.Message +
                            "\n\nПрограмма продолжит работу. Подробности записаны в errors.log.",
                            "Школьные звонки",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);
                    }
                    catch
                    {
                    }
                };
                AppDomain.CurrentDomain.UnhandledException += delegate(object sender, UnhandledExceptionEventArgs e)
                {
                    Exception exception = e.ExceptionObject as Exception;
                    Log.Error("Необработанная ошибка", exception);
                };

                string exePath = Application.ExecutablePath;
                string dataDir = AppPaths.ResolveDataDir(exePath);
                Log.Configure(dataDir);

                SettingsStore store = new SettingsStore(dataDir);
                LoadOutcome outcome = store.Load();
                AppSettings settings = outcome.Settings;
                if (string.IsNullOrEmpty(settings.MelodyStartFile))
                {
                    settings.MelodyStartFile = "default_bell.wav";
                }
                DefaultBellGenerator.EnsureDefaultBell(dataDir);
                store.CleanupMelodies(settings);

                bool startMinimized = false;
                string initialTab = "";
                for (int i = 0; i < args.Length; i++)
                {
                    if (string.Equals(args[i], "--autostart", StringComparison.OrdinalIgnoreCase))
                    {
                        startMinimized = true;
                    }
                    else if (args[i].StartsWith("--tab=", StringComparison.OrdinalIgnoreCase))
                    {
                        initialTab = args[i].Substring("--tab=".Length);
                    }
                }

                MainForm form = new MainForm(store, settings, outcome.Warning, startMinimized, initialTab);
                Application.Run(form);
            }
            catch (Exception ex)
            {
                Log.Error("Критическая ошибка при запуске", ex);
                MessageBox.Show(
                    "Не удалось запустить программу:\n\n" + ex.Message +
                    "\n\nЕсли ошибка повторяется, удалите папку SchoolBellsData рядом с программой (настройки будут сброшены).",
                    "Школьные звонки",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                try
                {
                    mutex.ReleaseMutex();
                }
                catch
                {
                }
                mutex.Close();
            }
        }
    }
}
