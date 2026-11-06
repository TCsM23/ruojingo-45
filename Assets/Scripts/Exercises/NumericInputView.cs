using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RuoJingo
{
    /// <summary>Type a subnet mask, host count or layer number.</summary>
    public class NumericInputView : ExerciseView
    {
        [SerializeField] internal TMP_InputField input;
        [SerializeField] internal Image inputBackground;

        public override string ExerciseType => ExerciseTypes.NumericInput;
        public override bool HasAnswer => !string.IsNullOrWhiteSpace(input.text);

        protected override void Build(ExerciseData d)
        {
            input.text = string.Empty;
            if (input.placeholder is TMP_Text placeholder)
                placeholder.text = string.IsNullOrEmpty(d.hint) ? "Type your answer" : d.hint;
            input.onValueChanged.AddListener(_ => NotifyChanged());
        }

        public override bool Evaluate() => AnswerChecker.CheckNumeric(Data, input.text);

        public override void SetInteractable(bool interactable) => input.interactable = interactable;

        public override void ShowResult(bool correct)
        {
            if (inputBackground != null) inputBackground.color = correct ? UiTheme.CorrectSoft : UiTheme.WrongSoft;
        }
    }
}
