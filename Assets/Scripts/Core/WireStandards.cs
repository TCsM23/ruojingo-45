using System;

namespace RuoJingo
{
    /// <summary>Canonical TIA/EIA-568 pin orders (pin 1 to pin 8) and the cable-type rule.</summary>
    public static class WireStandards
    {
        public const string T568A = "T568A";
        public const string T568B = "T568B";
        public const string StraightThrough = "Straight-through";
        public const string Crossover = "Crossover";

        private static readonly string[] T568AOrder =
            { "white/green", "green", "white/orange", "blue", "white/blue", "orange", "white/brown", "brown" };
        private static readonly string[] T568BOrder =
            { "white/orange", "orange", "white/green", "blue", "white/blue", "green", "white/brown", "brown" };

        public static bool IsValidStandard(string standard) =>
            Normalize(standard) == T568A || Normalize(standard) == T568B;

        /// <summary>Returns a copy of the pin order for the given standard.</summary>
        public static string[] GetOrder(string standard)
        {
            switch (Normalize(standard))
            {
                case T568A: return (string[])T568AOrder.Clone();
                case T568B: return (string[])T568BOrder.Clone();
                default: throw new ArgumentException($"Unknown wiring standard '{standard}'.", nameof(standard));
            }
        }

        /// <summary>Same standard on both ends = straight-through; T568A on one end and T568B on the other = crossover.</summary>
        public static string GetCableType(string endA, string endB)
        {
            if (!IsValidStandard(endA)) throw new ArgumentException($"Unknown wiring standard '{endA}'.", nameof(endA));
            if (!IsValidStandard(endB)) throw new ArgumentException($"Unknown wiring standard '{endB}'.", nameof(endB));
            return Normalize(endA) == Normalize(endB) ? StraightThrough : Crossover;
        }

        /// <summary>Returns "T568A"/"T568B" if the order exactly matches one, otherwise null.</summary>
        public static string IdentifyOrder(string[] order)
        {
            if (AnswerChecker.CheckOrder(T568AOrder, order)) return T568A;
            if (AnswerChecker.CheckOrder(T568BOrder, order)) return T568B;
            return null;
        }

        private static string Normalize(string s) => (s ?? string.Empty).Trim().ToUpperInvariant().Replace("-", "");
    }
}
