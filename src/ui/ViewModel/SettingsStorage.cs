using Microsoft.Data.Sqlite;
using System;
using System.IO;

namespace UI.ViewModel {
    public static class SettingsStorage {
        public static void Initialize () {
            var sql = @"
            CREATE TABLE IF NOT EXISTS Settings (
                Name TEXT PRIMARY KEY,
                Value TEXT NOT NULL) WITHOUT ROWID;";
            using var con = Connection;
            var cmd = new SqliteCommand(sql, con);
            try { cmd.ExecuteNonQuery(); }
            catch { }
        }

        public static bool ShowMediaOnSecondMonitor {
            get {
                using var con = Connection;
                var a = readValue(con, "ShowMediaOnSecondMonitor");
                return a == null || a == "1";
            }
            set {
                using var con = Connection;
                writeValue(con, "ShowMediaOnSecondMonitor", value ? "1" : "0");
            }
        }

        public static bool ShowMediaFullscreen {
            get {
                using var con = Connection;
                var a = readValue(con, "ShowMediaFullscreen");
                return a == null || a == "1";
            }
            set {
                using var con = Connection;
                writeValue(con, "ShowMediaFullscreen", value ? "1" : "0");
            }
        }

        static SqliteConnection Connection {
            get {
                var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.db");
                var cs = new SqliteConnectionStringBuilder() { DataSource = path }.ToString();
                var r = new SqliteConnection(cs);
                r.Open();
                return r;
            }
        }

        static string? readValue (SqliteConnection con, string name) {
            var sql = $@"
            SELECT Value
              FROM Settings
             WHERE Name = '{name}';";
            var cmd = new SqliteCommand(sql, con);
            string? r = null;
            try {
                var reader = cmd.ExecuteReader();
                if (!reader.HasRows) return null;
                reader.Read();
                r = reader.GetString(0);
            }
            catch { }
            return r;
        }

        static void writeValue (SqliteConnection con, string name, string value) {
            var sql = @"
                INSERT OR REPLACE INTO Settings (Name, Value)
                VALUES (@Name, @Value);";
            var cmd = new SqliteCommand(sql, con);
            cmd.Parameters.Add("@Name", SqliteType.Text).Value = name;
            cmd.Parameters.Add("@Value", SqliteType.Text).Value = value;
            try { cmd.ExecuteNonQuery(); } catch { }
        }
    }
}
