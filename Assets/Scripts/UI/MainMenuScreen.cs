using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RuoJingo
{
    public class MainMenuScreen : MonoBehaviour
    {
        [SerializeField] internal TMP_Text streakText;
        [SerializeField] internal TMP_Text xpText;
        [SerializeField] internal TMP_Text heartsText;
        [SerializeField] internal Button playButton;

        private void Start()
        {
            var gs = GameState.Instance;
            streakText.text = $"Streak\n<size=150%><b>{gs.DisplayStreak}</b></size>\nday(s)";
            xpText.text = $"Total XP\n<size=150%><b>{gs.Save.totalXp}</b></size>\nXP";
            heartsText.text = $"Hearts\n<size=150%><b>{gs.Save.lastHearts}/{HeartSystem.DefaultMaxHearts}</b></size>\nlast lesson";
            playButton.onClick.AddListener(() => SceneNav.Go(SceneNames.SkillTree));
        }
    }
}
