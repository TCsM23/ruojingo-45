using System.Collections.Generic;
using System.Linq;

namespace RuoJingo
{
    /// <summary>Checks lesson JSON for structural mistakes and wiring facts. Used by the loader and the tests.</summary>
    public static class ContentValidator
    {
        public static List<string> Validate(ExerciseData e)
        {
            var errors = new List<string>();
            if (e == null) { errors.Add("null exercise"); return errors; }
            string tag = string.IsNullOrEmpty(e.id) ? "(no id)" : e.id;
            if (string.IsNullOrEmpty(e.id)) errors.Add($"{tag}: missing id");
            if (string.IsNullOrWhiteSpace(e.prompt)) errors.Add($"{tag}: missing prompt");

            switch (e.type)
            {
                case ExerciseTypes.MultipleChoice:
                    if (e.options == null || e.options.Length < 2) errors.Add($"{tag}: needs at least 2 options");
                    else if (e.correctIndex < 0 || e.correctIndex >= e.options.Length) errors.Add($"{tag}: correctIndex out of range");
                    break;

                case ExerciseTypes.WireOrder:
                    if (e.correctOrder == null || e.correctOrder.Length != 8)
                        errors.Add($"{tag}: correctOrder must list 8 wires");
                    else if (WireStandards.IdentifyOrder(e.correctOrder) == null)
                        errors.Add($"{tag}: correctOrder is neither T568A nor T568B");
                    if (e.items != null && e.items.Length > 0 && e.correctOrder != null &&
                        !e.items.OrderBy(x => x).SequenceEqual(e.correctOrder.OrderBy(x => x)))
                        errors.Add($"{tag}: items must be a permutation of correctOrder");
                    break;

                case ExerciseTypes.DeviceMatch:
                    if (e.pairLeft == null || e.pairRight == null || e.pairLeft.Length < 2 || e.pairLeft.Length != e.pairRight.Length)
                        errors.Add($"{tag}: pairLeft and pairRight need the same length (>= 2)");
                    break;

                case ExerciseTypes.NumericInput:
                    if (e.acceptedAnswers == null || e.acceptedAnswers.Length == 0 || e.acceptedAnswers.Any(string.IsNullOrWhiteSpace))
                        errors.Add($"{tag}: needs non-empty acceptedAnswers");
                    break;

                case ExerciseTypes.CableTypeChoice:
                    if (!WireStandards.IsValidStandard(e.endA) || !WireStandards.IsValidStandard(e.endB))
                        errors.Add($"{tag}: endA/endB must be T568A or T568B");
                    break;

                default:
                    errors.Add($"{tag}: unknown type '{e.type}'");
                    break;
            }
            return errors;
        }

        public static List<string> Validate(UnitData unit)
        {
            var errors = new List<string>();
            if (unit == null) { errors.Add("null unit"); return errors; }
            if (unit.unitId < 1) errors.Add($"unit '{unit.title}': unitId must be >= 1");
            if (unit.exercises == null || unit.exercises.Length == 0) { errors.Add($"unit {unit.unitId}: no exercises"); return errors; }
            foreach (var dup in unit.exercises.Where(x => x != null).GroupBy(x => x.id).Where(g => g.Count() > 1))
                errors.Add($"unit {unit.unitId}: duplicate id '{dup.Key}'");
            foreach (var e in unit.exercises) errors.AddRange(Validate(e));
            return errors;
        }
    }
}
