using System;

namespace RuoJingo
{
    /// <summary>String keys used in the lesson JSON "type" field. Each maps to one exercise prefab.</summary>
    public static class ExerciseTypes
    {
        public const string MultipleChoice = "MultipleChoice";
        public const string WireOrder = "WireOrder";
        public const string DeviceMatch = "DeviceMatch";
        public const string NumericInput = "NumericInput";
        public const string CableTypeChoice = "CableTypeChoice";

        public static readonly string[] All =
            { MultipleChoice, WireOrder, DeviceMatch, NumericInput, CableTypeChoice };
    }

    /// <summary>
    /// One exercise as stored in StreamingAssets/lessons/*.json.
    /// JsonUtility has no polymorphism, so this is a flat record: each type reads only its own fields.
    /// </summary>
    [Serializable]
    public class ExerciseData
    {
        public string id;          // globally unique, e.g. "u1_q3"; used as the Leitner key
        public string type;        // one of ExerciseTypes
        public string prompt;
        public string explanation; // shown on the feedback panel

        // MultipleChoice
        public string[] options;
        public int correctIndex;

        // WireOrder: the correct pin 1..8 order. "items" is optional (defaults to correctOrder shuffled).
        public string[] correctOrder;
        public string[] items;

        // DeviceMatch: pairLeft[i] matches pairRight[i]. Right values may repeat.
        public string[] pairLeft;
        public string[] pairRight;

        // NumericInput: any of these (normalised) is accepted. The first is shown as the correct answer.
        public string[] acceptedAnswers;
        public string hint;

        // CableTypeChoice: wire standard on each end ("T568A" / "T568B"). The answer is derived, not stored.
        public string endA;
        public string endB;

        /// <summary>Set by the loader from the owning unit; not part of the JSON.</summary>
        [NonSerialized] public int unitId;
    }

    [Serializable]
    public class UnitData
    {
        public int unitId;
        public string title;
        public string description;
        public ExerciseData[] exercises;
    }

    /// <summary>StreamingAssets/lessons/index.json: the list of unit files to load.</summary>
    [Serializable]
    public class LessonIndex
    {
        public string[] files;
    }
}
