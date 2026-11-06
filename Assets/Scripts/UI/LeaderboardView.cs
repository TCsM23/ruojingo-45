using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

namespace RuoJingo
{
    /// <summary>Shows the MOCK weekly leaderboard. Clearly labelled as fake data.</summary>
    public class LeaderboardView : MonoBehaviour
    {
        [SerializeField] internal TMP_Text titleText;
        [SerializeField] internal TMP_Text rowsText;
        [SerializeField] internal TMP_Text disclaimerText;

        public void Show(IList<LeaderboardEntry> entries)
        {
            titleText.text = $"Weekly Leaderboard - {LeaderboardMock.SectionName}";
            disclaimerText.text = LeaderboardMock.Disclaimer;
            var sb = new StringBuilder();
            foreach (var e in entries)
            {
                string line = $"{e.Rank}.  {e.Name}<pos=75%>{e.Xp} XP";
                sb.AppendLine(e.IsPlayer ? $"<b><color=#1C8FD6>{line}</color></b>" : line);
            }
            rowsText.text = sb.ToString().TrimEnd();
        }
    }
}
