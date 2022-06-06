using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.IO;

namespace UI.ViewModel {
    public static class SettingsStorage {
        public static void Initialize () {
            var sql = @"
            CREATE TABLE IF NOT EXISTS Settings (
                Name TEXT PRIMARY KEY,
                Value TEXT NOT NULL) WITHOUT ROWID;

            CREATE TABLE IF NOT EXISTS MediaList (
                Path TEXT PRIMARY KEY) WITHOUT ROWID;";
            using var con = Connection;
            var cmd = new SqliteCommand(sql, con);
            try { cmd.ExecuteNonQuery(); }
            catch { }
        }

        public static List<string> MediaListPaths {
            get {
                using var con = Connection;
                var sql = $@"
                SELECT Path
                  FROM MediaList;";
                var cmd = new SqliteCommand(sql, con);
                List<string> r = new();
                try {
                    var reader = cmd.ExecuteReader();
                    while (reader.Read())
                        r.Add(reader.GetString(0));
                }
                catch { }
                return r;
            }
        }

        public static void ClearMediaListPaths () {
            using var con = Connection;
            var sql = @"DELETE FROM MediaList;";
            var cmd = new SqliteCommand(sql, con);
            try { cmd.ExecuteNonQuery(); }
            catch { }
        }

        public static void DeleteMediaListPath (string path) {
            using var con = Connection;
            var sql = @"
            DELETE FROM MediaList (Path)
            VALUES (@Path);";
            var cmd = new SqliteCommand(sql, con);
            cmd.Parameters.Add("@Path", SqliteType.Text).Value = path;
            try { cmd.ExecuteNonQuery(); }
            catch { }
        }

        public static void SaveMediaListPath (string path) {
            using var con = Connection;
            var sql = @"
            INSERT OR REPLACE INTO MediaList (Path)
            VALUES (@Path);";
            var cmd = new SqliteCommand(sql, con);
            cmd.Parameters.Add("@Path", SqliteType.Text).Value = path;
            try { cmd.ExecuteNonQuery(); }
            catch { }
        }

        public static bool ShowMediaOnSecondMonitor {
            get {
                using var con = Connection;
                var a = readSetting(con, "ShowMediaOnSecondMonitor");
                return a == null || a == "1";
            }
            set {
                using var con = Connection;
                writeSetting(con, "ShowMediaOnSecondMonitor", value ? "1" : "0");
            }
        }

        public static bool ShowMediaFullscreen {
            get {
                using var con = Connection;
                var a = readSetting(con, "ShowMediaFullscreen");
                return a == null || a == "1";
            }
            set {
                using var con = Connection;
                writeSetting(con, "ShowMediaFullscreen", value ? "1" : "0");
            }
        }

        public static string SingleVideoPath {
            get {
                using var con = Connection;
                return readSetting(con, "SingleVideoPath");
            }
            set {
                using var con = Connection;
                writeSetting(con, "SingleVideoPath", value);
            }
        }

        public static bool StartLocationLowerLeft {
            get {
                using var con = Connection;
                var a = readSetting(con, "StartLocationLowerLeft");
                return a == null || a == "1";
            }
            set {
                using var con = Connection;
                writeSetting(con, "StartLocationLowerLeft", value ? "1" : "0");
            }
        }

        public static bool StartLocationUpperLeft {
            get {
                using var con = Connection;
                var a = readSetting(con, "StartLocationUpperLeft");
                return a == null || a == "1";
            }
            set {
                using var con = Connection;
                writeSetting(con, "StartLocationUpperLeft", value ? "1" : "0");
            }
        }

        public static bool StartLocationUpperRight {
            get {
                using var con = Connection;
                var a = readSetting(con, "StartLocationUpperRight");
                return a == null || a == "1";
            }
            set {
                using var con = Connection;
                writeSetting(con, "StartLocationUpperRight", value ? "1" : "0");
            }
        }

        public static bool StartLocationLowerRight {
            get {
                using var con = Connection;
                var a = readSetting(con, "StartLocationLowerRight");
                return a == null || a == "1";
            }
            set {
                using var con = Connection;
                writeSetting(con, "StartLocationLowerRight", value ? "1" : "0");
            }
        }

        static SqliteConnection Connection {
            get {
                var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "fiz.db");
                var cs = new SqliteConnectionStringBuilder() { DataSource = path }.ToString();
                var r = new SqliteConnection(cs);
                r.Open();
                return r;
            }
        }

        static string? readSetting (SqliteConnection con, string name) {
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

        static void writeSetting (SqliteConnection con, string name, string value) {
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
