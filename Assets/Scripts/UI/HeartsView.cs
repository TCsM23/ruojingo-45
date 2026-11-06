using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RuoJingo
{
    public class HeartsView : MonoBehaviour
    {
        [SerializeField] internal Image[] icons;
        [SerializeField] internal TMP_Text countText;

        public void Show(int current, int max)
        {
            if (icons != null)
                for (int i = 0; i < icons.Length; i++)
                {
                    icons[i].gameObject.SetActive(i < max);
                    icons[i].color = i < current ? UiTheme.Heart : UiTheme.HeartEmpty;
                }
            if (countText != null) countText.text = current.ToString();
        }
    }
}
