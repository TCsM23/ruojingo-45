using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace RuoJingo
{
    /// <summary>
    /// Loads all units from StreamingAssets/lessons. On desktop it reads files directly; on WebGL
    /// (where StreamingAssets is a URL) it uses UnityWebRequest. Invalid exercises are dropped with an error log.
    /// </summary>
    public class ContentRepository
    {
        public const string LessonsFolder = "lessons";
        public const string IndexFile = "index.json";

        private readonly Dictionary<int, UnitData> units = new Dictionary<int, UnitData>();
        private readonly Dictionary<string, ExerciseData> exercisesById = new Dictionary<string, ExerciseData>();

        public bool IsLoaded { get; private set; }
        public string Error { get; private set; }
        public IReadOnlyList<UnitData> Units => units.Values.OrderBy(u => u.unitId).ToList();
        public int UnitCount => units.Count;

        public UnitData GetUnit(int unitId) => units.TryGetValue(unitId, out var u) ? u : null;
        public ExerciseData FindExercise(string id) =>
            id != null && exercisesById.TryGetValue(id, out var e) ? e : null;

        public IEnumerator LoadAll()
        {
            IsLoaded = false;
            Error = null;
            units.Clear();
            exercisesById.Clear();

            string root = Application.streamingAssetsPath + "/" + LessonsFolder + "/";
            string indexJson = null, err = null;
            yield return ReadText(root + IndexFile, (t, e) => { indexJson = t; err = e; });
            if (err != null) { Fail($"Could not read lesson index: {err}"); yield break; }

            var index = JsonUtility.FromJson<LessonIndex>(indexJson);
            if (index?.files == null || index.files.Length == 0) { Fail("Lesson index lists no files."); yield break; }

            foreach (var file in index.files)
            {
                string json = null;
                err = null;
                yield return ReadText(root + file, (t, e) => { json = t; err = e; });
                if (err != null) { Debug.LogError($"[RuoJingo] {file}: {err}"); continue; }
                try { AddUnit(ParseUnit(json, file)); }
                catch (Exception ex) { Debug.LogError($"[RuoJingo] {file}: {ex.Message}"); }
            }

            if (units.Count == 0) { Fail("No units could be loaded."); yield break; }
            IsLoaded = true;
        }

        /// <summary>Parses and validates one unit file. Invalid exercises are removed.</summary>
        public static UnitData ParseUnit(string json, string sourceName = "unit")
        {
            var unit = JsonUtility.FromJson<UnitData>(json);
            if (unit == null) throw new FormatException("Not a valid unit JSON.");
            var valid = new List<ExerciseData>();
            foreach (var e in unit.exercises ?? Array.Empty<ExerciseData>())
            {
                var problems = ContentValidator.Validate(e);
                if (problems.Count > 0)
                {
                    Debug.LogError($"[RuoJingo] {sourceName}: skipping exercise. {string.Join("; ", problems)}");
                    continue;
                }
                e.unitId = unit.unitId;
                valid.Add(e);
            }
            unit.exercises = valid.ToArray();
            return unit;
        }

        private void AddUnit(UnitData unit)
        {
            units[unit.unitId] = unit;
            foreach (var e in unit.exercises)
            {
                if (exercisesById.ContainsKey(e.id)) Debug.LogError($"[RuoJingo] Duplicate exercise id '{e.id}'.");
                exercisesById[e.id] = e;
            }
        }

        private void Fail(string message)
        {
            Error = message;
            Debug.LogError("[RuoJingo] " + message);
        }

        private static IEnumerator ReadText(string path, Action<string, string> done)
        {
            if (path.Contains("://"))
            {
                using (var req = UnityWebRequest.Get(path))
                {
                    yield return req.SendWebRequest();
                    if (req.result == UnityWebRequest.Result.Success) done(req.downloadHandler.text, null);
                    else done(null, req.error);
                }
            }
            else
            {
                string text = null, error = null;
                try { text = File.ReadAllText(path); }
                catch (Exception ex) { error = ex.Message; }
                done(text, error);
            }
        }
    }
}
