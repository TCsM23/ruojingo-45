using System;

namespace RuoJingo
{
    /// <summary>Contract every exercise prefab implements. The LessonScreen only talks to this.</summary>
    public interface IExercise
    {
        /// <summary>Matches ExerciseData.type, used to pick the prefab.</summary>
        string ExerciseType { get; }
        ExerciseData Data { get; }

        /// <summary>True when the player has given a complete answer (enables Check).</summary>
        bool HasAnswer { get; }
        event Action AnswerChanged;

        void Setup(ExerciseData data);
        bool Evaluate();
        string CorrectAnswerText { get; }
        void SetInteractable(bool interactable);

        /// <summary>Optional visual marking after Check (green/red on options).</summary>
        void ShowResult(bool correct);
    }
}
