using System;
using System.IO;
using UnityEngine;

namespace RuoJingo
{
    /// <summary>JSON save/load in Application.persistentDataPath. Writes go to a temp file first so a crash can't corrupt the save.</summary>
    public static class SaveSystem
    {
        public const string FileName = "ruojingo_save.json";

        public static string DefaultPath => Path.Combine(Application.persistentDataPath, FileName);

        public static SaveData Load(string path = null)
        {
            path = path ?? DefaultPath;
            try
            {
                if (File.Exists(path))
                {
                    var data = JsonUtility.FromJson<SaveData>(File.ReadAllText(path));
                    if (data != null)
                    {
                        data.Sanitize();
                        return data;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[RuoJingo] Could not read save at {path}, starting fresh. {ex.Message}");
            }
            var fresh = new SaveData();
            fresh.Sanitize();
            return fresh;
        }

        public static void Save(SaveData data, string path = null)
        {
            if (data == null) return;
            path = path ?? DefaultPath;
            try
            {
                string dir = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
                string tmp = path + ".tmp";
                File.WriteAllText(tmp, JsonUtility.ToJson(data, true));
                if (File.Exists(path)) File.Delete(path);
                File.Move(tmp, path);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[RuoJingo] Could not write save to {path}: {ex.Message}");
            }
        }

        public static void Delete(string path = null)
        {
            path = path ?? DefaultPath;
            if (File.Exists(path)) File.Delete(path);
        }
    }
}
