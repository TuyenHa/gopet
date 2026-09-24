using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gopet.Runtime
{
    public class DBBackup : IRuntime
    {
        private DateTime _lastBackup = DateTime.MinValue;
        private readonly object gate = new();
        private readonly Action<DateTime> export;
        private Task worker = Task.CompletedTask;
        private bool stopping;
        public Task Completion { get { lock (gate) return worker; } }
        public DBBackup() : this(Export) { }
        public DBBackup(Action<DateTime> export) => this.export = export;
        public void Update()
        {
            lock (gate)
            {
                if (stopping || !worker.IsCompleted || _lastBackup >= DateTime.Now) return;
                _lastBackup = DateTime.Now.AddMinutes(300);
                worker = Task.Run(() =>
                {
                    try
                    {
                        using var measurement = Gopet.Logging.PerformanceMetrics.Measure("db-backup");
                        export(DateTime.Now);
                    }
                    catch (Exception error)
                    {
                        GopetManager.ServerMonitor.LogError("Backup failed: " + error.Message);
                        lock (gate) _lastBackup = DateTime.Now.AddMinutes(5);
                    }
                });
            }
        }
        public void Stop()
        {
            Task active;
            lock (gate) { stopping = true; active = worker; }
            active.GetAwaiter().GetResult();
        }
        public static void Publish(string destination, Action<string> export)
        {
            // Same directory/filesystem makes publication an atomic rename. Failed/aborted
            // exports retain a clearly incomplete name and never replace a good backup.
            string temporary = destination + ".partial-" + Guid.NewGuid().ToString("N");
            export(temporary);
            File.Move(temporary, destination, overwrite: true);
        }
        private static void Export(DateTime now)
        {
                using (var gameConn = MYSQLManager.create())
                {
                    Publish(Path.Combine(Directory.GetCurrentDirectory() + "/backup_sql", $"game{now.Day}-{now.Month}-{now.Year}_{now.Hour}-{now.Minute}.sql"),
                        path => MYSQLManager.Backup(gameConn, path));
                }

                using (var webConn = MYSQLManager.createWebMySqlConnection())
                {
                    Publish(Path.Combine(Directory.GetCurrentDirectory() + "/backup_sql", $"web{now.Day}-{now.Month}-{now.Year}_{now.Hour}-{now.Minute}.sql"),
                        path => MYSQLManager.Backup(webConn, path));
                }
        }
    }
}
