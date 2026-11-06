using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RuoJingo
{
    /// <summary>Shared MonoBehaviour base for exercise prefabs. Grading itself lives in AnswerChecker.</summary>
    public abstract class ExerciseView : MonoBehaviour, IExercise
    {
        [SerializeField] internal TMP_Text promptText;

        protected static readonly System.Random Rng = new System.Random();

        public abstract string ExerciseType { get; }
        public ExerciseData Data { get; private set; }
        public abstract bool HasAnswer { get; }
        public event Action AnswerChanged;

        public void Setup(ExerciseData data)
        {
            Data = data ?? throw new ArgumentNullException(nameof(data));
            if (promptText != null) promptText.text = data.prompt;
            Build(data);
        }

        protected abstract void Build(ExerciseData data);
        public abstract bool Evaluate();
        public virtual string CorrectAnswerText => AnswerChecker.DescribeCorrectAnswer(Data);
        public abstract void SetInteractable(bool interactable);
        public virtual void ShowResult(bool correct) { }

        protected void NotifyChanged() => AnswerChanged?.Invoke();

        /// <summary>Clones an inactive template child and activates it.</summary>
        protected static T Spawn<T>(T template, Transform parent) where T : Component
        {
            var c = Instantiate(template, parent);
            c.gameObject.SetActive(true);
            return c;
        }

        protected static void SetLabel(Button b, string text)
        {
            var label = b.GetComponentInChildren<TMP_Text>(true);
            if (label != null) label.text = text;
        }

        protected static void Tint(Button b, Color c)
        {
            if (b.targetGraphic != null) b.targetGraphic.color = c;
        }
    }
}
