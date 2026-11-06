using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RuoJingo
{
    /// <summary>Shows the wire order on both cable ends; the player picks straight-through or crossover.</summary>
    public class CableTypeChoiceView : ExerciseView
    {
        [SerializeField] internal TMP_Text endALabel;
        [SerializeField] internal TMP_Text endBLabel;
        [SerializeField] internal RectTransform endARow;
        [SerializeField] internal RectTransform endBRow;
        [SerializeField] internal WireSwatch swatchTemplate;
        [SerializeField] internal Button straightButton;
        [SerializeField] internal Button crossoverButton;

        private string selected;

        public override string ExerciseType => ExerciseTypes.CableTypeChoice;
        public override bool HasAnswer => !string.IsNullOrEmpty(selected);

        protected override void Build(ExerciseData d)
        {
            swatchTemplate.gameObject.SetActive(false);
            endALabel.text = $"End A: <b>{d.endA}</b>";
            endBLabel.text = $"End B: <b>{d.endB}</b>";
            FillRow(endARow, d.endA);
            FillRow(endBRow, d.endB);

            SetLabel(straightButton, WireStandards.StraightThrough);
            SetLabel(crossoverButton, WireStandards.Crossover);
            straightButton.onClick.AddListener(() => Select(WireStandards.StraightThrough));
            crossoverButton.onClick.AddListener(() => Select(WireStandards.Crossover));
            Refresh();
        }

        private void FillRow(RectTransform row, string standard)
        {
            foreach (var wire in WireStandards.GetOrder(standard))
                Spawn(swatchTemplate, row).Set(wire);
        }

        private void Select(string cableType)
        {
            selected = cableType;
            Refresh();
            NotifyChanged();
        }

        private void Refresh()
        {
            Tint(straightButton, selected == WireStandards.StraightThrough ? UiTheme.OptionSelected : UiTheme.OptionNormal);
            Tint(crossoverButton, selected == WireStandards.Crossover ? UiTheme.OptionSelected : UiTheme.OptionNormal);
        }

        public override bool Evaluate() => AnswerChecker.CheckCableType(Data, selected);

        public override void SetInteractable(bool interactable)
        {
            straightButton.interactable = interactable;
            crossoverButton.interactable = interactable;
        }

        public override void ShowResult(bool correct)
        {
            string answer = WireStandards.GetCableType(Data.endA, Data.endB);
            Tint(straightButton, answer == WireStandards.StraightThrough ? UiTheme.CorrectSoft
                : selected == WireStandards.StraightThrough ? UiTheme.WrongSoft : UiTheme.OptionNormal);
            Tint(crossoverButton, answer == WireStandards.Crossover ? UiTheme.CorrectSoft
                : selected == WireStandards.Crossover ? UiTheme.WrongSoft : UiTheme.OptionNormal);
        }
    }
}
