using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using IllusionPlugin;

namespace BeatSaberLogger {
    public class Logger {
        private readonly Queue<string> _logQueue;
        private readonly FileInfo _logFile;
        private readonly Thread _watcherThread;
        private bool _threadRunning;
        
        public Logger(IPlugin plugin) {
            _logQueue = new Queue<string>();
            _logFile = GetPath(plugin);
            _watcherThread = new Thread(QueueWatcher) {IsBackground = true};
            _threadRunning = true;
            Start();
        }

        public void Log(string msg) {
            if(!_watcherThread.IsAlive) throw new Exception("Logger is Closed!");
            _logQueue.Enqueue($"[LOG @ {DateTime.Now:HH:mm:ss}] {msg}");
        }
        
        public void Error(string msg) {
            if(!_watcherThread.IsAlive) throw new Exception("Logger is Closed!");
            _logQueue.Enqueue($"[ERROR @ {DateTime.Now:HH:mm:ss}] {msg}");
        }
        
        public void Exception(string msg) {
            if(!_watcherThread.IsAlive) throw new Exception("Logger is Closed!");
            _logQueue.Enqueue($"[EXCEPTION @ {DateTime.Now:HH:mm:ss}] {msg}");
        }

        void QueueWatcher() {
            _logFile.Create().Close();
            while (_threadRunning) {
                if (_logQueue.Count > 0) {
                    using (var f = _logFile.AppendText()) {
                        while (_logQueue.Count > 0) {
                            f.WriteLine(_logQueue.Dequeue());
                        }
                    }
                }
            }
        }

        void Start() => _watcherThread.Start();

        public void Stop() {
            _threadRunning = false;
            _watcherThread.Join();
        }

        FileInfo GetPath(IPlugin plugin) {
            var logsDir = new DirectoryInfo($"./Logs/{plugin.Name}/{DateTime.Now:dd-MM-yy}");
            logsDir.Create();
            return new FileInfo($"{logsDir.FullName}/{logsDir.GetFiles().Length}.txt");
        }
    }
}