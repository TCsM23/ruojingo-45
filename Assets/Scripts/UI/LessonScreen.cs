using System.Collections;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RuoJingo
{
    /// <summary>
    /// Thin controller for the Lesson scene. All rules live in LessonRunner / HeartSystem / SpacedRepetition /
    /// ProgressService; this class only spawns exercise prefabs and shows feedback.
    /// </summary>
    public class LessonScreen : MonoBehaviour
    {
        [Header("Exercises")]
        [SerializeField] internal RectTransform exerciseContainer;
        [SerializeField] internal ExerciseView[] exercisePrefabs;

        [Header("HUD")]
        [SerializeField] internal RectTransform progressFill;
        [SerializeField] internal HeartsView hearts;
        [SerializeField] internal TMP_Text headerText;
        [SerializeField] internal TMP_Text loadingText;
        [SerializeField] internal Button quitButton;
        [SerializeField] internal Button checkButton;
        [SerializeField] internal FeedbackPanel feedback;

        [Header("Out of hearts")]
        [SerializeField] internal GameObject failPanel;
        [SerializeField] internal TMP_Text failBodyText;
        [SerializeField] internal Button retryButton;
        [SerializeField] internal Button failMapButton;

        private GameState gs;
        private UnitData unit;
        private LessonRunner runner;
        private ExerciseView current;

        private IEnumerator Start()
        {
            gs = GameState.Instance;
            feedback.Hide();
            failPanel.SetActive(false);
            checkButton.gameObject.SetActive(false);
            quitButton.onClick.AddListener(Quit);
            checkButton.onClick.AddListener(OnCheck);
            retryButton.onClick.AddListener(() => SceneNav.Go(SceneNames.Lesson));
            failMapButton.onClick.AddListener(() => SceneNav.Go(SceneNames.SkillTree));

            loadingText.gameObject.SetActive(true);
            loadingText.text = "Loading...";
            yield return gs.EnsureContentLoaded();
            unit = gs.Content.GetUnit(gs.SelectedUnitId) ?? gs.Content.Units.FirstOrDefault();
            if (gs.Content.Error != null || unit == null)
            {
                loadingText.text = "Could not load this lesson.\n" + gs.Content.Error;
                yield break;
            }
            loadingText.gameObject.SetActive(false);
            StartLesson();
        }

        private void StartLesson()
        {
            gs.Save.lessonCounter++;
            int session = gs.Save.lessonCounter;
            var srs = new SpacedRepetition(gs.Save.leitner);
            var reviews = srs.GetDueReviews(unit.unitId, session)
                .Select(e => gs.Content.FindExercise(e.questionId))
                .Where(e => e != null)
                .ToList();

            runner = new LessonRunner(unit.unitId, unit.exercises, reviews, new HeartSystem(), srs, session);
            runner.Hearts.Changed += _ => RefreshHud();
            ShowCurrent();
        }

        private void ShowCurrent()
        {
            if (current != null) Destroy(current.gameObject);
            current = null;

            var item = runner.Current;
            if (item == null) { Finish(); return; }

            var prefab = exercisePrefabs.FirstOrDefault(p => p != null && p.ExerciseType == item.Data.type);
            if (prefab == null)
            {
                Debug.LogError($"[RuoJingo] No prefab for exercise type '{item.Data.type}' ({item.Data.id}); skipping.");
                runner.Skip();
                ShowCurrent();
                return;
            }

            current = Instantiate(prefab, exerciseContainer);
            var rt = (RectTransform)current.transform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            current.Setup(item.Data);
            current.AnswerChanged += RefreshCheckButton;

            headerText.text = item.IsReview
                ? $"<color=#B77900><b>REVIEW</b> - from Unit {item.Data.unitId}</color>"
                : $"Unit {unit.unitId}: {unit.title}";
            checkButton.gameObject.SetActive(true);
            RefreshCheckButton();
            RefreshHud();
        }

        private void RefreshCheckButton()
        {
            if (current != null) checkButton.interactable = current.HasAnswer;
        }

        private void RefreshHud()
        {
            hearts.Show(runner.Hearts.Current, runner.Hearts.MaxHearts);
            progressFill.anchorMax = new Vector2(Mathf.Clamp01(runner.Progress), 1f);
        }

        private void OnCheck()
        {
            if (current == null || !current.HasAnswer || runner.CurrentSubmitted) return;
            bool correct = current.Evaluate();
            current.SetInteractable(false);
            current.ShowResult(correct);
            runner.Submit(correct);
            RefreshHud();
            checkButton.gameObject.SetActive(false);
            feedback.Show(correct, current.CorrectAnswerText, current.Data.explanation, OnContinue);
        }

        private void OnContinue()
        {
            feedback.Hide();
            if (runner.IsFailed) { Finish(); return; }
            runner.Advance();
            if (runner.IsComplete) Finish();
            else ShowCurrent();
        }

        private void Finish()
        {
            var result = ProgressService.ApplyLessonResult(gs.Save, runner, GameState.Today, gs.Content.UnitCount);
            gs.LastResult = result;
            gs.Persist();

            if (result.Passed)
            {
                SceneNav.Go(SceneNames.Results);
                return;
            }
            ShowFailPanel(result);
        }

        private void ShowFailPanel(LessonResult result)
        {
            if (current != null) Destroy(current.gameObject);
            checkButton.gameObject.SetActive(false);
            failPanel.SetActive(true);
            var sb = new StringBuilder("Review these, then try again. They will also come back in later lessons.\n");
            foreach (var q in result.Missed.GroupBy(m => m.id).Select(g => g.First()))
                sb.Append($"\n<b>{q.prompt}</b>\n<color=#3CA52E>{AnswerChecker.DescribeCorrectAnswer(q)}</color>\n");
            failBodyText.text = sb.ToString();
        }

        private void Quit()
        {
            gs.Persist(); // keep Leitner updates from answers already given
            SceneNav.Go(SceneNames.SkillTree);
        }
    }
}
