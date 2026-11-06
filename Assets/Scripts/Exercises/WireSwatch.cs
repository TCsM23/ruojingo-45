using UnityEngine;
using UnityEngine.UI;

namespace RuoJingo
{
    /// <summary>A coloured rectangle with an optional centre stripe, representing one wire.</summary>
    public class WireSwatch : MonoBehaviour
    {
        [SerializeField] internal Image body;
        [SerializeField] internal Image stripe;

        public void Set(string wire)
        {
            WireColors.Parse(wire, out var b, out var s);
            body.color = b;
            stripe.enabled = s.HasValue;
            if (s.HasValue) stripe.color = s.Value;
        }

        public void SetEmpty()
        {
            body.color = UiTheme.Track;
            stripe.enabled = false;
        }
    }
}
