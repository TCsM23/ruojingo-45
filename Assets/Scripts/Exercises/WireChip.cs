using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RuoJingo
{
    /// <summary>Tappable wire: swatch + name. Used for both the pin slots and the wire pool.</summary>
    public class WireChip : MonoBehaviour
    {
        [SerializeField] internal Button button;
        [SerializeField] internal Image background;
        [SerializeField] internal WireSwatch swatch;
        [SerializeField] internal TMP_Text label;

        public Button Button => button;

        public void SetWire(string wire, string text)
        {
            swatch.Set(wire);
            label.text = text;
            label.color = UiTheme.TextDark;
        }

        public void SetEmpty(string text)
        {
            swatch.SetEmpty();
            label.text = text;
            label.color = UiTheme.TextMuted;
        }

        public void SetBackground(Color c) => background.color = c;
    }
}
