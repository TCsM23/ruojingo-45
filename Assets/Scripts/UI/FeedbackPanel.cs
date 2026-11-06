using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RuoJingo
{
    /// <summary>Green / red panel shown after Check, with the correct answer on a wrong response.</summary>
    public class FeedbackPanel : MonoBehaviour
    {
        [SerializeField] internal Image background;
        [SerializeField] internal TMP_Text titleText;
        [SerializeField] internal TMP_Text detailText;
        [SerializeField] internal Button continueButton;
        [SerializeField] internal Image continueButtonImage;

        public void Show(bool correct, string correctAnswer, string explanation, Action onContinue)
        {
            gameObject.SetActive(true);
            background.color = correct ? UiTheme.CorrectSoft : UiTheme.WrongSoft;
            titleText.color = correct ? UiTheme.Correct : UiTheme.Wrong;
            titleText.text = correct ? "Correct!" : "Not quite.";
            string detail = correct ? "" : $"<b>Correct answer:</b> {correctAnswer}";
            if (!string.IsNullOrEmpty(explanation))
                detail += (detail.Length > 0 ? "\n" : "") + explanation;
            detailText.text = detail;
            if (continueButtonImage != null) continueButtonImage.color = correct ? UiTheme.Correct : UiTheme.Wrong;
            continueButton.onClick.RemoveAllListeners();
            continueButton.onClick.AddListener(() => onContinue?.Invoke());
        }

        public void Hide() => gameObject.SetActive(false);
    }
}
