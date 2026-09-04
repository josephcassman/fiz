// Copyright 2026 Joseph Cassman
// SPDX-License-Identifier: Apache-2.0

using System.Diagnostics;
using System.IO;
using System.Text.Json;

namespace UI.ViewModel {
    public sealed class SettingsData {
        public double WebPageScaleFactor { get; set; } = 1.0;
        public double StartLocationLeft { get; set; } = 0.0;
        public double StartLocationTop { get; set; } = 0.0;
        public double MediaWindowLeft { get; set; } = 0.0;
        public double MediaWindowTop { get; set; } = 0.0;
        public double MediaWindowWidth { get; set; } = 0.0;
        public double MediaWindowHeight { get; set; } = 0.0;
        public bool MediaWindowMaximized { get; set; } = false;
        public string SingleVideoPath { get; set; } = "";
        public List<string> MediaListPaths { get; set; } = [];
    }

    public static class SettingsStorage {
        static string filePath = initializeFilePath();
        static readonly JsonSerializerOptions jsonOptions = new() { WriteIndented = true };
        static SettingsData data = new();
        static readonly object fileLock = new();

        internal static void UseCustomFilePath (string? customPath) {
            filePath = customPath ?? initializeFilePath();
        }

        internal static void ResetData (SettingsData? customData = null) {
            data = customData ?? new();
        }

        public static List<string> MediaListPaths => [.. data.MediaListPaths];

        public static double MediaWindowHeight {
            get => data.MediaWindowHeight;
            set { data.MediaWindowHeight = value; Save(); }
        }

        public static double MediaWindowLeft {
            get => data.MediaWindowLeft;
            set { data.MediaWindowLeft = value; Save(); }
        }

        public static bool MediaWindowMaximized {
            get => data.MediaWindowMaximized;
            set { data.MediaWindowMaximized = value; Save(); }
        }

        public static double MediaWindowTop {
            get => data.MediaWindowTop;
            set { data.MediaWindowTop = value; Save(); }
        }

        public static double MediaWindowWidth {
            get => data.MediaWindowWidth;
            set { data.MediaWindowWidth = value; Save(); }
        }

        public static double WebPageScaleFactor {
            get => data.WebPageScaleFactor;
            set { data.WebPageScaleFactor = value; Save(); }
        }

        public static string SingleVideoPath {
            get => data.SingleVideoPath;
            set { data.SingleVideoPath = value; Save(); }
        }

        public static double StartLocationLeft {
            get => data.StartLocationLeft;
            set {
                if (value < 0) value = 0;
                data.StartLocationLeft = value;
            }
        }

        public static double StartLocationTop {
            get => data.StartLocationTop;
            set {
                if (value < 0) value = 0;
                data.StartLocationTop = value;
            }
        }

        public static void ClearMediaListPaths () {
            data.MediaListPaths.Clear();
            Save();
        }

        public static void DeleteMediaListPath (string path) {
            if (data.MediaListPaths.Remove(path)) {
                Save();
            }
        }

        public static void SaveMediaListPath (string path) {
            if (!data.MediaListPaths.Contains(path)) {
                data.MediaListPaths.Add(path);
                Save();
            }
        }

        public static void Initialize () {
            try {
                if (File.Exists(filePath)) {
                    var json = File.ReadAllText(filePath);
                    data = JsonSerializer.Deserialize<SettingsData>(json) ?? new();
                }
                else {
                    data = new();
                    Save();
                }
            }
            catch (Exception ex) {
                Trace.TraceError("Failed to load settings from '{0}': {1}", filePath, ex);
                data = new();
            }
        }

        public static void Save () {
            try {
                lock (fileLock) {
                    var json = JsonSerializer.Serialize(data, jsonOptions);
                    var tempPath = filePath + ".tmp";
                    File.WriteAllText(tempPath, json);
                    File.Move(tempPath, filePath, overwrite: true);
                }
            }
            catch (Exception ex) {
                Trace.TraceWarning("Atomic settings save failed, falling back to direct write: {0}", ex);
                try {
                    var json = JsonSerializer.Serialize(data, jsonOptions);
                    File.WriteAllText(filePath, json);
                }
                catch (Exception fallbackEx) {
                    Trace.TraceError("Failed to save settings to '{0}': {1}", filePath, fallbackEx);
                }
            }
        }

        static string initializeFilePath () {
            var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Fiz");
            Directory.CreateDirectory(folder);
            return Path.Combine(folder, "settings.json");
        }
    }
}
