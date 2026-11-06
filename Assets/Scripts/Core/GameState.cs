using System;
using System.Collections;
using UnityEngine;

namespace RuoJingo
{
    /// <summary>
    /// Prototype-level singleton that survives scene loads: holds the save data, the loaded content,
    /// the unit chosen on the SkillTree and the last lesson result. Created on first access, so any
    /// scene can be played directly from the Editor.
    /// </summary>
    public class GameState : MonoBehaviour
    {
        private static GameState instance;
        private bool loadStarted;

        public static GameState Instance
        {
            get
            {
                if (instance == null)
                {
                    var go = new GameObject("GameState");
                    instance = go.AddComponent<GameState>(); // Awake runs here
                }
                return instance;
            }
        }

        public SaveData Save { get; private set; }
        public ContentRepository Content { get; private set; }
        public int SelectedUnitId { get; set; } = 1;
        public LessonResult LastResult { get; set; }

        /// <summary>Local calendar day used by the streak.</summary>
        public static DateTime Today => DateTime.Now.Date;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;
            DontDestroyOnLoad(gameObject);
            Save = SaveSystem.Load();
            ProgressService.RollWeek(Save, Today);
            Content = new ContentRepository();
        }

        /// <summary>Yield on this from a scene's Start coroutine before touching Content.</summary>
        public IEnumerator EnsureContentLoaded()
        {
            if (!loadStarted)
            {
                loadStarted = true;
                StartCoroutine(Content.LoadAll());
            }
            while (!Content.IsLoaded && Content.Error == null) yield return null;
        }

        public void Persist() => SaveSystem.Save(Save);

        /// <summary>Used by the Editor menu "Delete Save File" and by tests.</summary>
        public void ResetProgress()
        {
            Save = new SaveData();
            Save.Sanitize();
            Persist();
        }

        public int DisplayStreak =>
            StreakSystem.GetCurrentStreak(Save.streak, DateUtil.ParseDay(Save.lastPlayedDate), Today);
    }

    public static class SceneNames
    {
        public const string MainMenu = "MainMenu";
        public const string SkillTree = "SkillTree";
        public const string Lesson = "Lesson";
        public const string Results = "Results";
        public static readonly string[] All = { MainMenu, SkillTree, Lesson, Results };
    }
}
