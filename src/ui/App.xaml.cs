using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using UI.ViewModel;

namespace UI {
    public partial class App : Application {
        static TextWriterTraceListener? traceListener;

        static App () {
            ConfigureLogging();
        }

        public static MainViewModel ViewModel { get; } = new();

        protected override void OnStartup (StartupEventArgs e) {
            Trace.TraceInformation("Fiz starting up at {0}", DateTime.Now);

            DispatcherUnhandledException += (_, args) => {
                Trace.TraceError("Unhandled UI exception: {0}", args.Exception);
            };

            AppDomain.CurrentDomain.UnhandledException += (_, args) => {
                Trace.TraceError("Unhandled AppDomain exception: {0}", args.ExceptionObject);
            };

            base.OnStartup(e);
        }

        protected override void OnExit (ExitEventArgs e) {
            Trace.TraceInformation("Fiz exiting at {0}", DateTime.Now);
            try {
                Trace.Flush();
                traceListener?.Flush();
                traceListener?.Close();
            }
            catch (Exception ex) {
                Trace.WriteLine($"[App] Error closing trace listener: {ex.Message}");
            }
            base.OnExit(e);
        }

        static void ConfigureLogging () {
            try {
                var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Fiz");
                Directory.CreateDirectory(folder);
                var logPath = Path.Combine(folder, "fiz.log");

                if (File.Exists(logPath)) {
                    var fileInfo = new FileInfo(logPath);
                    if (fileInfo.Length > 2 * 1024 * 1024) {
                        var backupPath = Path.Combine(folder, "fiz.old.log");
                        File.Move(logPath, backupPath, overwrite: true);
                    }
                }

                var fileStream = new FileStream(logPath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
                var writer = new StreamWriter(fileStream) { AutoFlush = true };
                traceListener = new TextWriterTraceListener(writer, "FizFileListener");
                Trace.Listeners.Add(traceListener);
                Trace.AutoFlush = true;
            }
            catch (Exception ex) {
                Trace.WriteLine($"[App] Failed to configure file trace listener: {ex.Message}");
            }
        }
    }
}
