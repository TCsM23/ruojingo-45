using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RuoJingo
{
    public class ResultsScreen : MonoBehaviour
    {
        [SerializeField] internal TMP_Text titleText;
        [SerializeField] internal TMP_Text xpText;
        [SerializeField] internal TMP_Text accuracyText;
        [SerializeField] internal TMP_Text heartsText;
        [SerializeField] internal TMP_Text detailText;
        [SerializeField] internal Button continueButton;

        private void Start()
        {
            continueButton.onClick.AddListener(() => SceneNav.Go(SceneNames.SkillTree));
            var r = GameState.Instance.LastResult;
            if (r == null)
            {
                titleText.text = "No lesson played yet";
                xpText.text = accuracyText.text = heartsText.text = detailText.text = "";
                return;
            }

            titleText.text = r.Passed ? (r.Perfect ? "Perfect lesson!" : "Lesson complete!") : "Lesson failed";
            titleText.color = r.Passed ? UiTheme.Primary : UiTheme.Wrong;
            xpText.text = $"XP earned\n<size=160%><b>+{r.XpEarned}</b></size>" + (r.Perfect ? $"\n(incl. +{XpSystem.PerfectBonus} perfect bonus)" : "");
            accuracyText.text = $"Accuracy\n<size=160%><b>{Mathf.RoundToInt(r.Accuracy * 100)}%</b></size>\n{r.CorrectCount} of {r.CorrectCount + r.WrongCount}";
            heartsText.text = $"Hearts left\n<size=160%><b>{r.HeartsLeft}/{HeartSystem.DefaultMaxHearts}</b></size>";

            string detail = $"Total XP: {r.TotalXp}    Streak: {r.Streak} day(s)";
            if (r.ReviewCount > 0) detail += $"\nIncluded {r.ReviewCount} review question(s) from earlier units.";
            if (r.UnlockedUnitId > 0) detail += $"\n\n<b><color=#3CA52E>Unit {r.UnlockedUnitId} unlocked!</color></b>";
            if (r.Missed.Count > 0) detail += $"\n{r.Missed.Count} missed question(s) will come back for review.";
            detailText.text = detail;
        }
    }
}
