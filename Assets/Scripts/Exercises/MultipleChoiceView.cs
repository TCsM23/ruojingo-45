using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace RuoJingo
{
    /// <summary>Terminology / OSI questions: pick one option. Options are shown in random order.</summary>
    public class MultipleChoiceView : ExerciseView
    {
        [SerializeField] internal Button optionTemplate;
        [SerializeField] internal RectTransform optionContainer;

        private readonly List<Button> buttons = new List<Button>();
        private readonly List<int> dataIndexOfButton = new List<int>();
        private int selected = -1; // index into Data.options

        public override string ExerciseType => ExerciseTypes.MultipleChoice;
        public override bool HasAnswer => selected >= 0;

        protected override void Build(ExerciseData d)
        {
            optionTemplate.gameObject.SetActive(false);
            var order = Enumerable.Range(0, d.options.Length).ToList();
            ShuffleUtil.Shuffle(order, Rng);
            foreach (int dataIndex in order)
            {
                var b = Spawn(optionTemplate, optionContainer);
                SetLabel(b, d.options[dataIndex]);
                b.onClick.AddListener(() => Select(dataIndex));
                buttons.Add(b);
                dataIndexOfButton.Add(dataIndex);
            }
            Refresh();
        }

        private void Select(int dataIndex)
        {
            selected = dataIndex;
            Refresh();
            NotifyChanged();
        }

        private void Refresh()
        {
            for (int i = 0; i < buttons.Count; i++)
                Tint(buttons[i], dataIndexOfButton[i] == selected ? UiTheme.OptionSelected : UiTheme.OptionNormal);
        }

        public override bool Evaluate() => AnswerChecker.CheckMultipleChoice(Data, selected);

        public override void SetInteractable(bool interactable)
        {
            foreach (var b in buttons) b.interactable = interactable;
        }

        public override void ShowResult(bool correct)
        {
            for (int i = 0; i < buttons.Count; i++)
            {
                int di = dataIndexOfButton[i];
                if (di == Data.correctIndex) Tint(buttons[i], UiTheme.CorrectSoft);
                else if (di == selected) Tint(buttons[i], UiTheme.WrongSoft);
            }
        }
    }
}
