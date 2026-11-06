using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RuoJingo
{
    /// <summary>
    /// Tap-to-order: tap a wire in the pool to put it in the next empty pin; tap a filled pin to take it back.
    /// </summary>
    public class WireOrderView : ExerciseView
    {
        [SerializeField] internal WireChip chipTemplate;
        [SerializeField] internal RectTransform slotContainer;
        [SerializeField] internal RectTransform poolContainer;

        private readonly List<WireChip> slots = new List<WireChip>();
        private readonly List<WireChip> poolChips = new List<WireChip>();
        private readonly List<string> poolWires = new List<string>();
        private readonly List<int> placed = new List<int>(); // pool indices, in pin order

        public override string ExerciseType => ExerciseTypes.WireOrder;
        public override bool HasAnswer => placed.Count == slots.Count;

        public IReadOnlyList<string> CurrentOrder => placed.Select(i => poolWires[i]).ToList();

        protected override void Build(ExerciseData d)
        {
            chipTemplate.gameObject.SetActive(false);

            poolWires.AddRange(d.items != null && d.items.Length > 0 ? d.items : d.correctOrder);
            if (d.items == null || d.items.Length == 0)
            {
                // Shuffle, but never hand the player the answer already in order.
                for (int tries = 0; tries < 10 && AnswerChecker.CheckOrder(d.correctOrder, poolWires); tries++)
                    ShuffleUtil.Shuffle(poolWires, Rng);
                if (AnswerChecker.CheckOrder(d.correctOrder, poolWires)) poolWires.Reverse();
            }

            for (int i = 0; i < d.correctOrder.Length; i++)
            {
                var slot = Spawn(chipTemplate, slotContainer);
                int slotIndex = i;
                slot.Button.onClick.AddListener(() => RemoveAt(slotIndex));
                slots.Add(slot);
            }
            for (int p = 0; p < poolWires.Count; p++)
            {
                var chip = Spawn(chipTemplate, poolContainer);
                chip.SetWire(poolWires[p], WireColors.DisplayName(poolWires[p]));
                int poolIndex = p;
                chip.Button.onClick.AddListener(() => Place(poolIndex));
                poolChips.Add(chip);
            }
            Refresh();
        }

        private void Place(int poolIndex)
        {
            if (placed.Count >= slots.Count || placed.Contains(poolIndex)) return;
            placed.Add(poolIndex);
            poolChips[poolIndex].gameObject.SetActive(false);
            Refresh();
        }

        private void RemoveAt(int slotIndex)
        {
            if (slotIndex >= placed.Count) return;
            int poolIndex = placed[slotIndex];
            placed.RemoveAt(slotIndex); // later wires shift up one pin
            poolChips[poolIndex].gameObject.SetActive(true);
            Refresh();
        }

        private void Refresh()
        {
            for (int i = 0; i < slots.Count; i++)
            {
                slots[i].SetBackground(UiTheme.Card);
                if (i < placed.Count)
                {
                    string wire = poolWires[placed[i]];
                    slots[i].SetWire(wire, $"Pin {i + 1}: {WireColors.DisplayName(wire)}");
                }
                else slots[i].SetEmpty($"Pin {i + 1}: (empty)");
            }
            NotifyChanged();
        }

        public override bool Evaluate() => AnswerChecker.CheckOrder(Data.correctOrder, CurrentOrder.ToList());

        public override void SetInteractable(bool interactable)
        {
            foreach (var c in slots.Concat(poolChips)) c.Button.interactable = interactable;
        }

        public override void ShowResult(bool correct)
        {
            var order = CurrentOrder;
            for (int i = 0; i < slots.Count && i < order.Count; i++)
                slots[i].SetBackground(AnswerChecker.CheckOrder(new[] { Data.correctOrder[i] }, new[] { order[i] })
                    ? UiTheme.CorrectSoft : UiTheme.WrongSoft);
        }
    }
}
