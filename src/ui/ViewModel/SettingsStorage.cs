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

        public static bool ShowMediaFullscreen {
            get => readBool("ShowMediaFullscreen");
            set { writeBool("ShowMediaFullscreen", value); }
        }

        public static string SingleVideoPath {
            get => readString("SingleVideoPath") ?? "";
            set { writeString("SingleVideoPath", value); }
        }

        public static bool StartLocationLowerLeft {
            get => readBool("StartLocationLowerLeft");
            set { writeBool("StartLocationLowerLeft", value); }
        }

        public static bool StartLocationUpperLeft {
            get => readBool("StartLocationUpperLeft");
            set { writeBool("StartLocationUpperLeft", value); }
        }

        public static bool StartLocationUpperRight {
            get => readBool("StartLocationUpperRight");
            set { writeBool("StartLocationUpperRight", value); }
        }

        public static bool StartLocationLowerRight {
            get => readBool("StartLocationLowerRight");
            set { writeBool("StartLocationLowerRight", value); }
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

        static SqliteCommand readCommand (string name, SqliteConnection con) {
            var sql = $@"
                SELECT Value
                  FROM Settings
                 WHERE Name = '{name}';";
            return new SqliteCommand(sql, con);
        }

        static bool readBool (string name) {
            using var con = Connection;
            var cmd = readCommand(name, con);
            int r = 0;
            try {
                var reader = cmd.ExecuteReader();
                if (!reader.HasRows) return false;
                reader.Read();
                r = reader.GetInt32(0);
            }
            catch { }
            return r == 1;
        }

        static string? readString (string name) {
            using var con = Connection;
            var cmd = readCommand(name, con);
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

        static SqliteCommand writeCommand (string name, SqliteConnection con) {
            var sql = $@"
                INSERT OR REPLACE INTO Settings (Name, Value)
                    VALUES (@Name, @Value);";
            return new SqliteCommand(sql, con);
        }

        static void writeBool (string name, bool value) {
            using var con = Connection;
            var cmd = writeCommand(name, con);
            cmd.Parameters.Add("@Name", SqliteType.Text).Value = name;
            cmd.Parameters.Add("@Value", SqliteType.Integer).Value = value ? 1 : 0;
            try { cmd.ExecuteNonQuery(); }
            catch { }
        }

        static void writeString (string name, string value) {
            using var con = Connection;
            var cmd = writeCommand(name, con);
            cmd.Parameters.Add("@Name", SqliteType.Text).Value = name;
            cmd.Parameters.Add("@Value", SqliteType.Text).Value = value;
            try { cmd.ExecuteNonQuery(); }
            catch { }
        }
    }
}
