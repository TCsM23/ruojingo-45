using System.Globalization;
using UnityEngine;

namespace RuoJingo
{
    /// <summary>Maps wire names like "white/orange" to placeholder colours (solid body + optional stripe).</summary>
    public static class WireColors
    {
        public static readonly Color White = new Color32(0xF5, 0xF5, 0xF5, 0xFF);

        public static Color ForBase(string colour)
        {
            switch ((colour ?? "").Trim().ToLowerInvariant())
            {
                case "green": return new Color32(0x2E, 0x9E, 0x44, 0xFF);
                case "orange": return new Color32(0xF5, 0x8A, 0x1F, 0xFF);
                case "blue": return new Color32(0x27, 0x63, 0xD8, 0xFF);
                case "brown": return new Color32(0x7A, 0x4A, 0x23, 0xFF);
                case "white": return White;
                default: return Color.magenta; // unknown colour: obvious placeholder
            }
        }

        /// <summary>"white/orange" -> white body with an orange stripe; "orange" -> solid orange.</summary>
        public static void Parse(string wire, out Color body, out Color? stripe)
        {
            string w = (wire ?? "").Trim().ToLowerInvariant();
            if (w.StartsWith("white/"))
            {
                body = White;
                stripe = ForBase(w.Substring("white/".Length));
            }
            else
            {
                body = ForBase(w);
                stripe = null;
            }
        }

        /// <summary>"white/orange" -> "White/Orange".</summary>
        public static string DisplayName(string wire)
        {
            var ti = CultureInfo.InvariantCulture.TextInfo;
            return ti.ToTitleCase((wire ?? "").Trim().ToLowerInvariant());
        }
    }
}
