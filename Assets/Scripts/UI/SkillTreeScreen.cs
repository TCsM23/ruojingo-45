using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RuoJingo
{
    public class SkillTreeScreen : MonoBehaviour
    {
        [SerializeField] internal UnitNodeView[] nodes;
        [SerializeField] internal TMP_Text statsText;
        [SerializeField] internal TMP_Text loadingText;
        [SerializeField] internal Button backButton;
        [SerializeField] internal LeaderboardView leaderboard;

        private IEnumerator Start()
        {
            backButton.onClick.AddListener(() => SceneNav.Go(SceneNames.MainMenu));
            var gs = GameState.Instance;
            statsText.text = $"XP {gs.Save.totalXp}   |   Streak {gs.DisplayStreak}";
            leaderboard.Show(LeaderboardMock.GetWeekly(gs.Save.playerName, gs.Save.weeklyXp));

            loadingText.gameObject.SetActive(true);
            loadingText.text = "Loading lessons...";
            yield return gs.EnsureContentLoaded();
            if (gs.Content.Error != null)
            {
                loadingText.text = "Could not load lessons:\n" + gs.Content.Error;
                yield break;
            }
            loadingText.gameObject.SetActive(false);

            var units = gs.Content.Units;
            for (int i = 0; i < nodes.Length; i++)
            {
                if (i >= units.Count) { nodes[i].Hide(); continue; }
                var unit = units[i];
                var state = ProgressService.IsUnitPassed(gs.Save, unit.unitId) ? UnitNodeState.Passed
                    : ProgressService.IsUnitUnlocked(gs.Save, unit.unitId) ? UnitNodeState.Unlocked
                    : UnitNodeState.Locked;
                int id = unit.unitId;
                nodes[i].Bind(unit, state, () =>
                {
                    gs.SelectedUnitId = id;
                    SceneNav.Go(SceneNames.Lesson);
                });
            }
        }
    }
}
