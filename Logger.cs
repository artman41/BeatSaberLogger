using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Globalization;
using System.IO;
using System.Threading;
using IllusionPlugin;

namespace BeatSaberVersionChecker {
    public class Logger {
        private readonly Queue<string> LogQueue;
        private readonly FileInfo LogFile;
        private readonly Thread WatcherThread;
        private bool ThreadRunning;
        
        public Logger(IPlugin plugin) {
            LogQueue = new Queue<string>();
            LogFile = GetPath(plugin);
            WatcherThread = new Thread(QueueWatcher) {IsBackground = true};
            ThreadRunning = true;
            Start();
        }

        public void Log(string msg) {
            if(!WatcherThread.IsAlive) throw new Exception("Logger is Closed!");
            LogQueue.Enqueue($"[LOG @ {DateTime.Now:HH:mm:ss}] {msg}");
        }
        
        public void Error(string msg) {
            if(!WatcherThread.IsAlive) throw new Exception("Logger is Closed!");
            LogQueue.Enqueue($"[ERROR @ {DateTime.Now:HH:mm:ss}] {msg}");
        }
        
        public void Exception(string msg) {
            if(!WatcherThread.IsAlive) throw new Exception("Logger is Closed!");
            LogQueue.Enqueue($"[EXCEPTION @ {DateTime.Now:HH:mm:ss}] {msg}");
        }

        void QueueWatcher() {
            LogFile.Create().Close();
            while (ThreadRunning) {
                if (LogQueue.Count > 0) {
                    using (var f = LogFile.AppendText()) {
                        while (LogQueue.Count > 0) {
                            f.WriteLine(LogQueue.Dequeue());
                        }
                    }
                }
            }
        }

        void Start() => WatcherThread.Start();

        public void Stop() {
            ThreadRunning = false;
            WatcherThread.Join();
        }

        FileInfo GetPath(IPlugin plugin) {
            var logsDir = new DirectoryInfo($"./Logs/{plugin.Name}/{DateTime.Now:dd-MM-yy}");
            logsDir.Create();
            return new FileInfo($"{logsDir.FullName}/{logsDir.GetFiles().Length}.txt");
        }
    }
}