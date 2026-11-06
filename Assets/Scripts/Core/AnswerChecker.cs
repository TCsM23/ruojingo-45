using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RuoJingo
{
    /// <summary>Pure grading logic shared by the exercise views (kept out of MonoBehaviours so it is testable).</summary>
    public static class AnswerChecker
    {
        public static bool CheckMultipleChoice(ExerciseData data, int selectedOptionIndex) =>
            data != null && selectedOptionIndex >= 0 && selectedOptionIndex == data.correctIndex;

        /// <summary>Exact sequence match, ignoring case and surrounding whitespace.</summary>
        public static bool CheckOrder(IList<string> expected, IList<string> given)
        {
            if (expected == null || given == null || expected.Count != given.Count) return false;
            for (int i = 0; i < expected.Count; i++)
                if (!SameText(expected[i], given[i])) return false;
            return true;
        }

        /// <summary>chosenRightForLeft[i] is the right-hand value the player paired with pairLeft[i].</summary>
        public static bool CheckPairs(IList<string> expectedRightForLeft, IList<string> chosenRightForLeft) =>
            CheckOrder(expectedRightForLeft, chosenRightForLeft);

        public static bool CheckNumeric(ExerciseData data, string input)
        {
            if (data?.acceptedAnswers == null || string.IsNullOrWhiteSpace(input)) return false;
            string given = NormalizeAnswer(input);
            foreach (var accepted in data.acceptedAnswers)
            {
                string a = NormalizeAnswer(accepted);
                if (a == given) return true;
                // "062" == "62"
                if (long.TryParse(a, out var na) && long.TryParse(given, out var ng) && na == ng) return true;
            }
            return false;
        }

        public static bool CheckCableType(ExerciseData data, string chosenCableType) =>
            data != null && SameText(WireStandards.GetCableType(data.endA, data.endB), chosenCableType);

        /// <summary>Lower-case and strip all whitespace: " 255.255.255.0 " == "255.255.255.0".</summary>
        public static string NormalizeAnswer(string s)
        {
            if (s == null) return string.Empty;
            var sb = new StringBuilder(s.Length);
            foreach (char c in s)
                if (!char.IsWhiteSpace(c)) sb.Append(char.ToLowerInvariant(c));
            return sb.ToString();
        }

        /// <summary>Human-readable correct answer for the red feedback panel. ASCII only (default TMP font).</summary>
        public static string DescribeCorrectAnswer(ExerciseData d)
        {
            if (d == null) return string.Empty;
            switch (d.type)
            {
                case ExerciseTypes.MultipleChoice:
                    return d.options != null && d.correctIndex >= 0 && d.correctIndex < d.options.Length
                        ? d.options[d.correctIndex] : string.Empty;
                case ExerciseTypes.WireOrder:
                    return d.correctOrder == null ? string.Empty
                        : string.Join(", ", d.correctOrder.Select((w, i) => $"{i + 1}. {w}"));
                case ExerciseTypes.DeviceMatch:
                    if (d.pairLeft == null || d.pairRight == null) return string.Empty;
                    return string.Join("; ", d.pairLeft.Zip(d.pairRight, (l, r) => $"{l} = {r}"));
                case ExerciseTypes.NumericInput:
                    return d.acceptedAnswers != null && d.acceptedAnswers.Length > 0 ? d.acceptedAnswers[0] : string.Empty;
                case ExerciseTypes.CableTypeChoice:
                    string type = WireStandards.GetCableType(d.endA, d.endB);
                    return type == WireStandards.StraightThrough
                        ? $"{type} (same standard on both ends: {d.endA} / {d.endB})"
                        : $"{type} ({d.endA} on one end, {d.endB} on the other)";
                default:
                    return string.Empty;
            }
        }

        private static bool SameText(string a, string b) =>
            string.Equals((a ?? "").Trim(), (b ?? "").Trim(), StringComparison.OrdinalIgnoreCase);
    }
}
