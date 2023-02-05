using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.IO;

namespace UI.ViewModel {
    public static class SettingsStorage {
        public static List<string> MediaListPaths {
            get {
                using var con = connection;
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

        public static double MediaWindowHeight {
            get => readDouble("MediaWindowHeight");
            set { writeDouble("MediaWindowHeight", value); }
        }

        public static double MediaWindowLeft {
            get => readDouble("MediaWindowLeft");
            set { writeDouble("MediaWindowLeft", value); }
        }

        public static double MediaWindowTop {
            get => readDouble("MediaWindowTop");
            set { writeDouble("MediaWindowTop", value); }
        }

        public static double MediaWindowWidth {
            get => readDouble("MediaWindowWidth");
            set { writeDouble("MediaWindowWidth", value); }
        }

        public static double WebPageScaleFactor {
            get => readDouble("WebPageScaleFactor");
            set { writeDouble("WebPageScaleFactor", value); }
        }

        public static string SingleVideoPath {
            get => readString("SingleVideoPath") ?? "";
            set { writeString("SingleVideoPath", value); }
        }

        public static double StartLocationLeft {
            get => readDouble("StartLocationLeft");
            set {
                if (value < 0) value = 0;
                writeDouble("StartLocationLeft", value);
            }
        }

        public static double StartLocationTop {
            get => readDouble("StartLocationTop");
            set {
                if (value < 0) value = 0;
                writeDouble("StartLocationTop", value);
            }
        }

        public static void ClearMediaListPaths () {
            using var con = connection;
            var sql = @"DELETE FROM MediaList;";
            var cmd = new SqliteCommand(sql, con);
            try { cmd.ExecuteNonQuery(); }
            catch { }
        }

        public static void DeleteMediaListPath (string path) {
            using var con = connection;
            var sql = @"
            DELETE FROM MediaList (Path)
            VALUES (@Path);";
            var cmd = new SqliteCommand(sql, con);
            cmd.Parameters.Add("@Path", SqliteType.Text).Value = path;
            try { cmd.ExecuteNonQuery(); }
            catch { }
        }

        public static void Initialize () {
            var sql = """
            CREATE TABLE IF NOT EXISTS Settings (
                Name TEXT PRIMARY KEY,
                Value TEXT NOT NULL) WITHOUT ROWID;

            INSERT INTO Settings (Name, Value)
            VALUES ('WebPageScaleFactor', 1.0),
                   ('MediaWindowLeft', 0.0),
                   ('MediaWindowTop', 0.0),
                   ('MediaWindowHeight', 0.0),
                   ('MediaWindowWidth', 0.0);

            CREATE TABLE IF NOT EXISTS MediaList (
                Path TEXT PRIMARY KEY) WITHOUT ROWID;
            """;
            using var con = connection;
            var cmd = new SqliteCommand(sql, con);
            try { cmd.ExecuteNonQuery(); }
            catch { }
        }

        public static void SaveMediaListPath (string path) {
            using var con = connection;
            var sql = @"
            INSERT OR REPLACE INTO MediaList (Path)
            VALUES (@Path);";
            var cmd = new SqliteCommand(sql, con);
            cmd.Parameters.Add("@Path", SqliteType.Text).Value = path;
            try { cmd.ExecuteNonQuery(); }
            catch { }
        }

        static SqliteConnection connection {
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
            using var con = connection;
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

        static double readDouble (string name) {
            using var con = connection;
            var cmd = readCommand(name, con);
            double r = 0.0;
            try {
                var reader = cmd.ExecuteReader();
                if (!reader.HasRows) return 0.0;
                reader.Read();
                r = reader.GetDouble(0);
            }
            catch { }
            return r;
        }

        static string? readString (string name) {
            using var con = connection;
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
            using var con = connection;
            var cmd = writeCommand(name, con);
            cmd.Parameters.Add("@Name", SqliteType.Text).Value = name;
            cmd.Parameters.Add("@Value", SqliteType.Integer).Value = value ? 1 : 0;
            try { cmd.ExecuteNonQuery(); }
            catch { }
        }

        static void writeDouble (string name, double value) {
            using var con = connection;
            var cmd = writeCommand(name, con);
            cmd.Parameters.Add("@Name", SqliteType.Text).Value = name;
            cmd.Parameters.Add("@Value", SqliteType.Real).Value = value;
            try { cmd.ExecuteNonQuery(); }
            catch { }
        }

        static void writeString (string name, string value) {
            using var con = connection;
            var cmd = writeCommand(name, con);
            cmd.Parameters.Add("@Name", SqliteType.Text).Value = name;
            cmd.Parameters.Add("@Value", SqliteType.Text).Value = value;
            try { cmd.ExecuteNonQuery(); }
            catch { }
        }
    }
}
