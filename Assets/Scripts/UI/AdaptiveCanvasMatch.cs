using UnityEngine;
using UnityEngine.UI;

namespace RuoJingo
{
    /// <summary>
    /// Keeps the 1080x1920 portrait design fully visible on any aspect ratio: on screens narrower than
    /// 9:16 it matches width, on wider screens (16:9 desktop) it matches height. Content lives in a
    /// centred 1080-unit column, so on desktop the column is centred with background on the sides.
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(CanvasScaler))]
    public class AdaptiveCanvasMatch : MonoBehaviour
    {
        private CanvasScaler scaler;

        private void OnEnable() => scaler = GetComponent<CanvasScaler>();

        private void Update()
        {
            if (scaler == null || Screen.height == 0) return;
            float screenAspect = (float)Screen.width / Screen.height;
            float refAspect = scaler.referenceResolution.x / scaler.referenceResolution.y;
            float match = screenAspect < refAspect ? 0f : 1f;
            if (!Mathf.Approximately(scaler.matchWidthOrHeight, match)) scaler.matchWidthOrHeight = match;
        }
    }
}
