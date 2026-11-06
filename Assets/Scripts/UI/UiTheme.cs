using UnityEngine;

namespace RuoJingo
{
    /// <summary>All placeholder colours in one place, so swapping the art style later is a one-file change.</summary>
    public static class UiTheme
    {
        public static readonly Color Background = new Color32(0xF4, 0xF6, 0xF8, 0xFF);
        public static readonly Color Card = Color.white;
        public static readonly Color TextDark = new Color32(0x33, 0x3A, 0x40, 0xFF);
        public static readonly Color TextMuted = new Color32(0x77, 0x80, 0x88, 0xFF);
        public static readonly Color TextOnColor = Color.white;

        public static readonly Color Primary = new Color32(0x3C, 0xA5, 0x2E, 0xFF);   // buttons, unlocked nodes
        public static readonly Color Accent = new Color32(0x1C, 0x8F, 0xD6, 0xFF);    // selection, progress
        public static readonly Color Gold = new Color32(0xF2, 0xB7, 0x05, 0xFF);      // passed nodes, XP
        public static readonly Color Locked = new Color32(0xBC, 0xC3, 0xC9, 0xFF);
        public static readonly Color Track = new Color32(0xDD, 0xE2, 0xE6, 0xFF);

        public static readonly Color Correct = new Color32(0x3C, 0xA5, 0x2E, 0xFF);
        public static readonly Color CorrectSoft = new Color32(0xD9, 0xF5, 0xC9, 0xFF);
        public static readonly Color Wrong = new Color32(0xE5, 0x3E, 0x3E, 0xFF);
        public static readonly Color WrongSoft = new Color32(0xFC, 0xDB, 0xDB, 0xFF);

        public static readonly Color OptionNormal = Color.white;
        public static readonly Color OptionSelected = new Color32(0xCF, 0xEA, 0xFB, 0xFF);
        public static readonly Color Heart = new Color32(0xE5, 0x3E, 0x3E, 0xFF);
        public static readonly Color HeartEmpty = new Color32(0xDD, 0xE2, 0xE6, 0xFF);
        public static readonly Color Review = new Color32(0xB7, 0x79, 0x00, 0xFF);

        /// <summary>Colours for matched pairs in DeviceMatch (paired with a number label for colour-blind players).</summary>
        public static readonly Color[] PairColors =
        {
            new Color32(0xCF, 0xEA, 0xFB, 0xFF),
            new Color32(0xE6, 0xD9, 0xFB, 0xFF),
            new Color32(0xFD, 0xE7, 0xC7, 0xFF),
            new Color32(0xD3, 0xF2, 0xEC, 0xFF),
            new Color32(0xFB, 0xD9, 0xEC, 0xFF),
        };
    }
}
