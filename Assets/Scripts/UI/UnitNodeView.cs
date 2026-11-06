using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RuoJingo
{
    public enum UnitNodeState { Locked, Unlocked, Passed }

    /// <summary>One circle on the skill-tree path.</summary>
    public class UnitNodeView : MonoBehaviour
    {
        [SerializeField] internal Button button;
        [SerializeField] internal Image circle;
        [SerializeField] internal TMP_Text numberText;
        [SerializeField] internal TMP_Text titleText;
        [SerializeField] internal TMP_Text statusText;

        public void Bind(UnitData unit, UnitNodeState state, Action onClick)
        {
            numberText.text = unit.unitId.ToString();
            titleText.text = $"Unit {unit.unitId}: {unit.title}";
            button.onClick.RemoveAllListeners();
            button.interactable = state != UnitNodeState.Locked;

            switch (state)
            {
                case UnitNodeState.Locked:
                    circle.color = UiTheme.Locked;
                    titleText.color = UiTheme.TextMuted;
                    statusText.color = UiTheme.TextMuted;
                    statusText.text = $"LOCKED - pass Unit {unit.unitId - 1} first";
                    break;
                case UnitNodeState.Unlocked:
                    circle.color = UiTheme.Primary;
                    titleText.color = UiTheme.TextDark;
                    statusText.color = UiTheme.Primary;
                    statusText.text = "START - tap the circle";
                    break;
                case UnitNodeState.Passed:
                    circle.color = UiTheme.Gold;
                    titleText.color = UiTheme.TextDark;
                    statusText.color = UiTheme.Review;
                    statusText.text = "PASSED - tap to practise again";
                    break;
            }
            if (state != UnitNodeState.Locked) button.onClick.AddListener(() => onClick?.Invoke());
        }

        public void Hide() => gameObject.SetActive(false);
    }
}
