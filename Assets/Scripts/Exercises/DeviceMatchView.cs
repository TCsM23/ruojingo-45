using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace RuoJingo
{
    /// <summary>
    /// Tap pairs: tap an item on the left, then its match on the right. Paired items share a colour and number.
    /// Tapping a paired item un-pairs it.
    /// </summary>
    public class DeviceMatchView : ExerciseView
    {
        [SerializeField] internal Button itemTemplate;
        [SerializeField] internal RectTransform leftColumn;
        [SerializeField] internal RectTransform rightColumn;

        private readonly List<Button> leftButtons = new List<Button>();
        private readonly List<Button> rightButtons = new List<Button>();
        private List<int> rightOrder;   // right button index -> index into Data.pairRight
        private int[] pairOfLeft;       // left index -> right button index, or -1
        private int selectedLeft = -1;

        public override string ExerciseType => ExerciseTypes.DeviceMatch;
        public override bool HasAnswer => pairOfLeft != null && pairOfLeft.All(p => p >= 0);

        protected override void Build(ExerciseData d)
        {
            itemTemplate.gameObject.SetActive(false);
            pairOfLeft = Enumerable.Repeat(-1, d.pairLeft.Length).ToArray();
            rightOrder = Enumerable.Range(0, d.pairRight.Length).ToList();
            for (int tries = 0; tries < 10 && rightOrder.SequenceEqual(Enumerable.Range(0, rightOrder.Count)); tries++)
                ShuffleUtil.Shuffle(rightOrder, Rng);

            for (int i = 0; i < d.pairLeft.Length; i++)
            {
                var b = Spawn(itemTemplate, leftColumn);
                int li = i;
                b.onClick.AddListener(() => OnLeft(li));
                leftButtons.Add(b);
            }
            for (int j = 0; j < rightOrder.Count; j++)
            {
                var b = Spawn(itemTemplate, rightColumn);
                int rj = j;
                b.onClick.AddListener(() => OnRight(rj));
                rightButtons.Add(b);
            }
            Refresh();
        }

        private void OnLeft(int li)
        {
            if (pairOfLeft[li] >= 0) pairOfLeft[li] = -1; // un-pair, then select it again
            selectedLeft = selectedLeft == li ? -1 : li;
            Refresh();
            NotifyChanged();
        }

        private void OnRight(int rj)
        {
            int owner = System.Array.IndexOf(pairOfLeft, rj);
            if (owner >= 0) pairOfLeft[owner] = -1;
            if (selectedLeft >= 0)
            {
                pairOfLeft[selectedLeft] = rj;
                selectedLeft = -1;
            }
            Refresh();
            NotifyChanged();
        }

        private void Refresh()
        {
            for (int i = 0; i < leftButtons.Count; i++)
            {
                bool paired = pairOfLeft[i] >= 0;
                SetLabel(leftButtons[i], paired ? $"({i + 1})  {Data.pairLeft[i]}" : Data.pairLeft[i]);
                Tint(leftButtons[i], i == selectedLeft ? UiTheme.Gold
                    : paired ? UiTheme.PairColors[i % UiTheme.PairColors.Length] : UiTheme.OptionNormal);
            }
            for (int j = 0; j < rightButtons.Count; j++)
            {
                int owner = System.Array.IndexOf(pairOfLeft, j);
                string text = Data.pairRight[rightOrder[j]];
                SetLabel(rightButtons[j], owner >= 0 ? $"({owner + 1})  {text}" : text);
                Tint(rightButtons[j], owner >= 0 ? UiTheme.PairColors[owner % UiTheme.PairColors.Length] : UiTheme.OptionNormal);
            }
        }

        private List<string> ChosenRightForLeft() =>
            pairOfLeft.Select(p => p >= 0 ? Data.pairRight[rightOrder[p]] : null).ToList();

        public override bool Evaluate() => AnswerChecker.CheckPairs(Data.pairRight, ChosenRightForLeft());

        public override void SetInteractable(bool interactable)
        {
            foreach (var b in leftButtons.Concat(rightButtons)) b.interactable = interactable;
        }

        public override void ShowResult(bool correct)
        {
            var chosen = ChosenRightForLeft();
            for (int i = 0; i < leftButtons.Count; i++)
            {
                bool ok = AnswerChecker.CheckOrder(new[] { Data.pairRight[i] }, new[] { chosen[i] });
                Tint(leftButtons[i], ok ? UiTheme.CorrectSoft : UiTheme.WrongSoft);
            }
        }
    }
}
