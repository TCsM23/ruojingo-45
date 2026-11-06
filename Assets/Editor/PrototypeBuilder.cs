using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace RuoJingo
{
    /// <summary>
    /// Builds every scene, canvas and exercise prefab of the prototype from code, wires all references
    /// and registers the scenes in Build Settings. Re-running it overwrites the generated assets.
    /// Menu: Tools > RuoJingo > Build Prototype Scenes.
    /// Batch mode: -executeMethod RuoJingo.PrototypeBuilder.BuildAll
    /// </summary>
    public static class PrototypeBuilder
    {
        private const string ScenesDir = "Assets/Scenes";
        private const string PrefabDir = "Assets/Prefabs/Exercises";
        private const float ColumnWidth = 1080f;

        private static TMP_FontAsset font;
        private static Sprite spriteRounded, spriteKnob, spriteInput;
        private static bool tmpImportAttempted;

        // ------------------------------------------------------------------ menu

        [MenuItem("Tools/RuoJingo/Build Prototype Scenes", priority = 0)]
        public static void BuildMenu()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            BuildAll();
        }

        [MenuItem("Tools/RuoJingo/Delete Save File", priority = 20)]
        public static void DeleteSave()
        {
            SaveSystem.Delete();
            Debug.Log($"[RuoJingo] Deleted save file at {SaveSystem.DefaultPath}");
        }

        [MenuItem("Tools/RuoJingo/Show Save Folder", priority = 21)]
        public static void RevealSave() => EditorUtility.RevealInFinder(Application.persistentDataPath);

        public static void BuildAll()
        {
            if (!EnsureTmpEssentials()) return;
            LoadSharedAssets();
            EnsureFolder(ScenesDir);
            EnsureFolder(PrefabDir);

            var prefabs = BuildExercisePrefabs();
            var scenePaths = new[]
            {
                BuildMainMenu(),
                BuildSkillTree(),
                BuildLesson(prefabs),
                BuildResults(),
            };

            EditorBuildSettings.scenes = scenePaths.Select(p => new EditorBuildSettingsScene(p, true)).ToArray();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            if (!Application.isBatchMode) EditorSceneManager.OpenScene(scenePaths[0]);
            Debug.Log("[RuoJingo] Prototype built: 4 scenes + 5 exercise prefabs, Build Settings updated. Open MainMenu and press Play.");
        }

        // ------------------------------------------------------------------ TMP essentials

        private static bool EnsureTmpEssentials()
        {
            if (AssetDatabase.FindAssets("t:TMP_Settings").Length > 0) return true;

            string package = FindTmpEssentialsPackage();
            if (package == null || tmpImportAttempted)
            {
                const string msg = "TextMesh Pro Essential Resources are missing.\n\nUse Window > TextMeshPro > Import TMP Essential Resources, " +
                                   "then run Tools > RuoJingo > Build Prototype Scenes again.";
                if (Application.isBatchMode) Debug.LogError("[RuoJingo] " + msg);
                else EditorUtility.DisplayDialog("RuoJingo", msg, "OK");
                return false;
            }

            tmpImportAttempted = true;
            Debug.Log("[RuoJingo] Importing TMP Essential Resources; the build continues automatically afterwards.");
            AssetDatabase.importPackageCompleted += OnTmpImported;
            AssetDatabase.ImportPackage(package, false);
            return false;
        }

        private static void OnTmpImported(string packageName)
        {
            AssetDatabase.importPackageCompleted -= OnTmpImported;
            AssetDatabase.Refresh();
            EditorApplication.delayCall += BuildAll;
        }

        private static string FindTmpEssentialsPackage()
        {
            // TMP 3.x ships as its own package; in Unity 6 it lives inside com.unity.ugui.
            foreach (var pkg in new[] { "com.unity.textmeshpro", "com.unity.ugui" })
            {
                string path = Path.GetFullPath($"Packages/{pkg}/Package Resources/TMP Essential Resources.unitypackage");
                if (File.Exists(path)) return path;
            }
            return null;
        }

        private static void LoadSharedAssets()
        {
            font = TMP_Settings.defaultFontAsset;
            if (font == null)
            {
                var guid = AssetDatabase.FindAssets("LiberationSans SDF t:TMP_FontAsset").FirstOrDefault();
                if (guid != null) font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AssetDatabase.GUIDToAssetPath(guid));
            }
            spriteRounded = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            spriteKnob = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
            spriteInput = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/InputFieldBackground.psd");
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = Path.GetDirectoryName(path)?.Replace('\\', '/');
            if (!string.IsNullOrEmpty(parent)) EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
        }

        // ------------------------------------------------------------------ exercise prefabs

        private static ExerciseView[] BuildExercisePrefabs()
        {
            // Build inside a throwaway scene so nothing leaks into a real one.
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            return new ExerciseView[]
            {
                BuildMultipleChoicePrefab(),
                BuildWireOrderPrefab(),
                BuildDeviceMatchPrefab(),
                BuildNumericInputPrefab(),
                BuildCableTypePrefab(),
            };
        }

        private static RectTransform ExerciseRoot(string name)
        {
            var root = NewRect(name, null);
            Stretch(root);
            VLayout(root.gameObject, 40, 26);
            return root;
        }

        private static TextMeshProUGUI Prompt(Transform parent) =>
            NewText("Prompt", parent, "Prompt", 50, UiTheme.TextDark, TextAlignmentOptions.Left, FontStyles.Bold);

        private static TextMeshProUGUI Hint(Transform parent, string name, string text) =>
            NewText(name, parent, text, 34, UiTheme.TextMuted, TextAlignmentOptions.Left);

        private static ExerciseView BuildMultipleChoicePrefab()
        {
            var root = ExerciseRoot("MultipleChoiceExercise");
            var view = root.gameObject.AddComponent<MultipleChoiceView>();
            view.promptText = Prompt(root);
            view.optionTemplate = OptionButton("OptionTemplate", root, "Option", 42);
            view.optionTemplate.gameObject.SetActive(false);
            view.optionContainer = root;
            return SavePrefab<ExerciseView>(root.gameObject, "MultipleChoiceExercise");
        }

        private static ExerciseView BuildWireOrderPrefab()
        {
            var root = ExerciseRoot("WireOrderExercise");
            root.GetComponent<VerticalLayoutGroup>().spacing = 16;
            var view = root.gameObject.AddComponent<WireOrderView>();
            view.promptText = Prompt(root);

            Hint(root, "SlotsLabel", "Pins 1-8 (tap a pin to take its wire back)");
            view.slotContainer = WireGrid("Slots", root);
            Hint(root, "PoolLabel", "Wires (tap to put in the next empty pin)");
            view.poolContainer = WireGrid("Pool", root);

            view.chipTemplate = WireChipTemplate(view.poolContainer);
            view.chipTemplate.gameObject.SetActive(false);
            return SavePrefab<ExerciseView>(root.gameObject, "WireOrderExercise");
        }

        private static RectTransform WireGrid(string name, Transform parent)
        {
            var rt = NewRect(name, parent);
            var grid = rt.gameObject.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(490, 86);
            grid.spacing = new Vector2(20, 10);
            grid.startAxis = GridLayoutGroup.Axis.Vertical; // pins 1-4 down the left column, 5-8 down the right
            grid.constraint = GridLayoutGroup.Constraint.FixedRowCount;
            grid.constraintCount = 4;
            grid.childAlignment = TextAnchor.UpperCenter;
            return rt;
        }

        private static WireChip WireChipTemplate(Transform parent)
        {
            var rt = NewRect("WireChipTemplate", parent);
            var bg = AddImage(rt, UiTheme.Card, spriteRounded, true);
            AddShadow(rt);
            var chip = rt.gameObject.AddComponent<WireChip>();
            chip.background = bg;
            chip.button = rt.gameObject.AddComponent<Button>();
            chip.button.targetGraphic = bg;
            SetDisabledColor(chip.button, false);

            var swatchRt = NewRect("Swatch", rt);
            swatchRt.anchorMin = new Vector2(0, 0);
            swatchRt.anchorMax = new Vector2(0, 1);
            swatchRt.pivot = new Vector2(0, 0.5f);
            swatchRt.offsetMin = new Vector2(12, 14);
            swatchRt.offsetMax = new Vector2(12 + 110, -14);
            chip.swatch = MakeSwatch(swatchRt, horizontalStripe: true);

            chip.label = NewText("Label", rt, "Wire", 34, UiTheme.TextDark, TextAlignmentOptions.Left);
            Stretch(chip.label.rectTransform, 138, 0, 10, 0);
            return chip;
        }

        private static WireSwatch MakeSwatch(RectTransform rt, bool horizontalStripe)
        {
            var swatch = rt.gameObject.AddComponent<WireSwatch>();
            swatch.body = AddImage(rt, WireColors.White);
            swatch.body.raycastTarget = false;
            var outline = rt.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(0, 0, 0, 0.35f);
            outline.effectDistance = new Vector2(2, -2);

            var stripeRt = NewRect("Stripe", rt);
            stripeRt.anchorMin = horizontalStripe ? new Vector2(0, 0.33f) : new Vector2(0.3f, 0);
            stripeRt.anchorMax = horizontalStripe ? new Vector2(1, 0.67f) : new Vector2(0.7f, 1);
            stripeRt.offsetMin = stripeRt.offsetMax = Vector2.zero;
            swatch.stripe = AddImage(stripeRt, Color.white);
            swatch.stripe.raycastTarget = false;
            return swatch;
        }

        private static ExerciseView BuildDeviceMatchPrefab()
        {
            var root = ExerciseRoot("DeviceMatchExercise");
            var view = root.gameObject.AddComponent<DeviceMatchView>();
            view.promptText = Prompt(root);
            Hint(root, "Hint", "Tap an item on the left, then its match on the right. Tap again to undo.");

            var columns = NewRect("Columns", root);
            var h = columns.gameObject.AddComponent<HorizontalLayoutGroup>();
            h.spacing = 30;
            h.childControlWidth = h.childControlHeight = true;
            h.childForceExpandWidth = true;
            h.childForceExpandHeight = false;

            view.leftColumn = NewRect("Left", columns);
            VLayout(view.leftColumn.gameObject, 0, 18);
            view.rightColumn = NewRect("Right", columns);
            VLayout(view.rightColumn.gameObject, 0, 18);

            view.itemTemplate = OptionButton("ItemTemplate", view.leftColumn, "Item", 38);
            view.itemTemplate.gameObject.SetActive(false);
            return SavePrefab<ExerciseView>(root.gameObject, "DeviceMatchExercise");
        }

        private static ExerciseView BuildNumericInputPrefab()
        {
            var root = ExerciseRoot("NumericInputExercise");
            var view = root.gameObject.AddComponent<NumericInputView>();
            view.promptText = Prompt(root);

            var fieldRt = NewRect("InputField", root);
            AddLayout(fieldRt, minHeight: 150, preferredHeight: 150);
            var bg = AddImage(fieldRt, Color.white, spriteInput, true);
            var input = fieldRt.gameObject.AddComponent<TMP_InputField>();
            input.targetGraphic = bg;

            var area = NewRect("Text Area", fieldRt);
            Stretch(area, 30, 10, 30, 10);
            area.gameObject.AddComponent<RectMask2D>();

            var placeholder = NewText("Placeholder", area, "Type your answer", 48, UiTheme.TextMuted, TextAlignmentOptions.Left, FontStyles.Italic);
            Stretch(placeholder.rectTransform);
            var text = NewText("Text", area, "", 52, UiTheme.TextDark, TextAlignmentOptions.Left);
            Stretch(text.rectTransform);
            text.enableWordWrapping = false;

            input.textViewport = area;
            input.textComponent = text;
            input.placeholder = placeholder;
            input.fontAsset = font;
            input.pointSize = 52;
            input.lineType = TMP_InputField.LineType.SingleLine;
            input.contentType = TMP_InputField.ContentType.Standard;
            input.characterLimit = 32;

            view.input = input;
            view.inputBackground = bg;
            Hint(root, "Tip", "Masks in dotted decimal (e.g. 255.255.255.0), counts as plain numbers.");
            return SavePrefab<ExerciseView>(root.gameObject, "NumericInputExercise");
        }

        private static ExerciseView BuildCableTypePrefab()
        {
            var root = ExerciseRoot("CableTypeChoiceExercise");
            var view = root.gameObject.AddComponent<CableTypeChoiceView>();
            view.promptText = Prompt(root);

            view.endALabel = NewText("EndALabel", root, "End A", 44, UiTheme.TextDark, TextAlignmentOptions.Left);
            view.endARow = SwatchRow("EndARow", root);
            view.endBLabel = NewText("EndBLabel", root, "End B", 44, UiTheme.TextDark, TextAlignmentOptions.Left);
            view.endBRow = SwatchRow("EndBRow", root);
            Hint(root, "PinNote", "Pins 1 to 8, left to right.");

            var tmpl = NewRect("SwatchTemplate", view.endARow);
            view.swatchTemplate = MakeSwatch(tmpl, horizontalStripe: false);
            view.swatchTemplate.gameObject.SetActive(false);

            view.straightButton = OptionButton("StraightButton", root, WireStandards.StraightThrough, 46);
            view.crossoverButton = OptionButton("CrossoverButton", root, WireStandards.Crossover, 46);
            return SavePrefab<ExerciseView>(root.gameObject, "CableTypeChoiceExercise");
        }

        private static RectTransform SwatchRow(string name, Transform parent)
        {
            var rt = NewRect(name, parent);
            AddLayout(rt, minHeight: 150, preferredHeight: 150);
            var h = rt.gameObject.AddComponent<HorizontalLayoutGroup>();
            h.spacing = 10;
            h.childControlWidth = h.childControlHeight = true;
            h.childForceExpandWidth = h.childForceExpandHeight = true;
            return rt;
        }

        private static T SavePrefab<T>(GameObject go, string fileName) where T : Component
        {
            string path = $"{PrefabDir}/{fileName}.prefab";
            var asset = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return asset.GetComponent<T>();
        }

        // ------------------------------------------------------------------ scenes

        private static string BuildMainMenu()
        {
            var (scene, canvas, column) = NewScreenScene();
            var screen = canvas.gameObject.AddComponent<MainMenuScreen>();

            var title = NewText("Title", column, "RuoJingo-45", 120, UiTheme.Primary, TextAlignmentOptions.Center, FontStyles.Bold);
            TopBand(title.rectTransform, 240, 180);
            var subtitle = NewText("Subtitle", column,
                "Networking fundamentals in bite-sized lessons.\nYour stepping stone to Cisco Packet Tracer.",
                40, UiTheme.TextMuted);
            TopBand(subtitle.rectTransform, 430, 150, 60);

            var row = StatRow(column, 680, 360);
            screen.streakText = StatCard(row, "Streak");
            screen.xpText = StatCard(row, "XP");
            screen.heartsText = StatCard(row, "Hearts");

            screen.playButton = NewButton("PlayButton", column, "PLAY", UiTheme.Primary, UiTheme.TextOnColor, 66);
            BottomBand((RectTransform)screen.playButton.transform, 300, 170, 120);

            var note = NewText("Footer", column, "Capstone research prototype - placeholder art", 30, UiTheme.TextMuted);
            BottomBand(note.rectTransform, 80, 80, 40);

            return SaveScene(scene, SceneNames.MainMenu);
        }

        private static string BuildSkillTree()
        {
            var (scene, canvas, column) = NewScreenScene();
            var screen = canvas.gameObject.AddComponent<SkillTreeScreen>();

            var header = NewRect("Header", column);
            TopBand(header, 20, 160, 30);
            screen.backButton = NewButton("BackButton", header, "Back", UiTheme.Track, UiTheme.TextDark, 42);
            var backRt = (RectTransform)screen.backButton.transform;
            backRt.anchorMin = backRt.anchorMax = new Vector2(0, 0.5f);
            backRt.pivot = new Vector2(0, 0.5f);
            backRt.anchoredPosition = Vector2.zero;
            backRt.sizeDelta = new Vector2(220, 110);
            var title = NewText("Title", header, "Skill Tree", 66, UiTheme.TextDark, TextAlignmentOptions.Center, FontStyles.Bold);
            Stretch(title.rectTransform, 240, 0, 240, 0);

            screen.statsText = NewText("Stats", column, "XP 0 | Streak 0", 40, UiTheme.TextMuted);
            TopBand(screen.statsText.rectTransform, 190, 70);

            var path = NewRect("Path", column);
            Stretch(path, 0, 780, 0, 290);

            var track = NewRect("Track", path);
            track.anchorMin = new Vector2(0, 0.17f);
            track.anchorMax = new Vector2(0, 0.83f);
            track.pivot = new Vector2(0.5f, 0.5f);
            track.sizeDelta = new Vector2(18, 0);
            track.anchoredPosition = new Vector2(250, 0);
            AddImage(track, UiTheme.Track).raycastTarget = false;

            float[] ys = { 0.83f, 0.5f, 0.17f }; // Unit 1 at the top
            screen.nodes = ys.Select((y, i) => UnitNode(path, i + 1, y)).ToArray();

            screen.loadingText = NewText("Loading", path, "Loading...", 44, UiTheme.TextMuted);
            Stretch(screen.loadingText.rectTransform);

            var board = NewRect("Leaderboard", column);
            BottomBand(board, 50, 700, 50);
            AddImage(board, UiTheme.Card, spriteRounded, true);
            AddShadow(board);
            VLayout(board.gameObject, 36, 10);
            var lb = board.gameObject.AddComponent<LeaderboardView>();
            lb.titleText = NewText("Title", board, "Weekly Leaderboard", 46, UiTheme.TextDark, TextAlignmentOptions.Left, FontStyles.Bold);
            lb.disclaimerText = NewText("Disclaimer", board, "MOCK DATA", 30, UiTheme.Wrong, TextAlignmentOptions.Left, FontStyles.Italic);
            lb.rowsText = NewText("Rows", board, "", 40, UiTheme.TextDark, TextAlignmentOptions.TopLeft);
            lb.rowsText.lineSpacing = 20;
            AddLayout(lb.rowsText.rectTransform, flexibleHeight: 1);
            screen.leaderboard = lb;

            return SaveScene(scene, SceneNames.SkillTree);
        }

        private static UnitNodeView UnitNode(Transform parent, int unitNumber, float anchorY)
        {
            var group = NewRect($"Unit{unitNumber}Node", parent);
            group.anchorMin = group.anchorMax = new Vector2(0, anchorY);
            group.pivot = new Vector2(0, 0.5f);
            group.anchoredPosition = new Vector2(100, 0);
            group.sizeDelta = new Vector2(880, 280);
            var node = group.gameObject.AddComponent<UnitNodeView>();

            var circle = NewRect("Circle", group);
            circle.anchorMin = circle.anchorMax = new Vector2(0, 0.5f);
            circle.pivot = new Vector2(0.5f, 0.5f);
            circle.anchoredPosition = new Vector2(150, 0);
            circle.sizeDelta = new Vector2(230, 230);
            node.circle = AddImage(circle, UiTheme.Primary, spriteKnob);
            AddShadow(circle);
            node.button = circle.gameObject.AddComponent<Button>();
            node.button.targetGraphic = node.circle;
            SetDisabledColor(node.button, false);
            node.numberText = NewText("Number", circle, unitNumber.ToString(), 100, UiTheme.TextOnColor, TextAlignmentOptions.Center, FontStyles.Bold);
            Stretch(node.numberText.rectTransform);

            node.titleText = NewText("Title", group, $"Unit {unitNumber}", 46, UiTheme.TextDark, TextAlignmentOptions.BottomLeft, FontStyles.Bold);
            node.titleText.rectTransform.anchorMin = new Vector2(0, 0.5f);
            node.titleText.rectTransform.anchorMax = new Vector2(1, 0.95f);
            node.titleText.rectTransform.offsetMin = new Vector2(300, 0);
            node.titleText.rectTransform.offsetMax = Vector2.zero;

            node.statusText = NewText("Status", group, "LOCKED", 34, UiTheme.TextMuted, TextAlignmentOptions.TopLeft);
            node.statusText.rectTransform.anchorMin = new Vector2(0, 0.05f);
            node.statusText.rectTransform.anchorMax = new Vector2(1, 0.48f);
            node.statusText.rectTransform.offsetMin = new Vector2(300, 0);
            node.statusText.rectTransform.offsetMax = Vector2.zero;
            return node;
        }

        private static string BuildLesson(ExerciseView[] prefabs)
        {
            var (scene, canvas, column) = NewScreenScene();
            var screen = canvas.gameObject.AddComponent<LessonScreen>();
            screen.exercisePrefabs = prefabs;

            // Top bar: quit, progress, hearts
            var top = NewRect("TopBar", column);
            TopBand(top, 30, 130, 30);
            screen.quitButton = NewButton("QuitButton", top, "X", UiTheme.Track, UiTheme.TextDark, 56);
            var quitRt = (RectTransform)screen.quitButton.transform;
            quitRt.anchorMin = quitRt.anchorMax = new Vector2(0, 0.5f);
            quitRt.pivot = new Vector2(0, 0.5f);
            quitRt.anchoredPosition = Vector2.zero;
            quitRt.sizeDelta = new Vector2(110, 110);

            var progress = NewRect("Progress", top);
            progress.anchorMin = new Vector2(0, 0.5f);
            progress.anchorMax = new Vector2(1, 0.5f);
            progress.offsetMin = new Vector2(140, -22);
            progress.offsetMax = new Vector2(-330, 22);
            AddImage(progress, UiTheme.Track, spriteRounded, true).raycastTarget = false;
            var fill = NewRect("Fill", progress);
            fill.anchorMin = Vector2.zero;
            fill.anchorMax = new Vector2(0, 1);
            fill.offsetMin = fill.offsetMax = Vector2.zero;
            AddImage(fill, UiTheme.Primary, spriteRounded, true).raycastTarget = false;
            screen.progressFill = fill;

            var heartsRt = NewRect("Hearts", top);
            heartsRt.anchorMin = heartsRt.anchorMax = new Vector2(1, 0.5f);
            heartsRt.pivot = new Vector2(1, 0.5f);
            heartsRt.anchoredPosition = Vector2.zero;
            heartsRt.sizeDelta = new Vector2(310, 70);
            var hl = heartsRt.gameObject.AddComponent<HorizontalLayoutGroup>();
            hl.spacing = 6;
            hl.childControlWidth = hl.childControlHeight = true;
            hl.childForceExpandWidth = hl.childForceExpandHeight = true;
            var hearts = heartsRt.gameObject.AddComponent<HeartsView>();
            hearts.icons = Enumerable.Range(1, HeartSystem.DefaultMaxHearts).Select(i =>
            {
                var img = AddImage(NewRect($"Heart{i}", heartsRt), UiTheme.Heart, spriteKnob);
                img.preserveAspect = true;
                img.raycastTarget = false;
                return img;
            }).ToArray();
            screen.hearts = hearts;

            screen.headerText = NewText("Header", column, "Unit", 38, UiTheme.TextMuted, TextAlignmentOptions.Left);
            TopBand(screen.headerText.rectTransform, 170, 70, 40);

            screen.exerciseContainer = NewRect("ExerciseContainer", column);
            Stretch(screen.exerciseContainer, 0, 250, 0, 250);

            screen.checkButton = NewButton("CheckButton", column, "CHECK", UiTheme.Primary, UiTheme.TextOnColor, 60);
            SetDisabledColor(screen.checkButton, true);
            BottomBand((RectTransform)screen.checkButton.transform, 60, 160, 60);

            screen.feedback = BuildFeedbackPanel(column);
            screen.failPanel = BuildFailPanel(column, screen);

            screen.loadingText = NewText("Loading", column, "Loading...", 44, UiTheme.TextMuted);
            Stretch(screen.loadingText.rectTransform);

            return SaveScene(scene, SceneNames.Lesson);
        }

        private static FeedbackPanel BuildFeedbackPanel(Transform column)
        {
            var rt = NewRect("FeedbackPanel", column);
            BottomBand(rt, 0, 620, 0);
            var panel = rt.gameObject.AddComponent<FeedbackPanel>();
            panel.background = AddImage(rt, UiTheme.CorrectSoft);

            panel.titleText = NewText("Title", rt, "Correct!", 62, UiTheme.Correct, TextAlignmentOptions.Left, FontStyles.Bold);
            TopBand(panel.titleText.rectTransform, 30, 90, 50);

            panel.detailText = NewText("Detail", rt, "", 38, UiTheme.TextDark, TextAlignmentOptions.TopLeft);
            Stretch(panel.detailText.rectTransform, 50, 250, 50, 130);
            panel.detailText.enableAutoSizing = true;
            panel.detailText.fontSizeMin = 22;
            panel.detailText.fontSizeMax = 38;

            panel.continueButton = NewButton("ContinueButton", rt, "CONTINUE", UiTheme.Correct, UiTheme.TextOnColor, 56);
            panel.continueButtonImage = (Image)panel.continueButton.targetGraphic;
            BottomBand((RectTransform)panel.continueButton.transform, 60, 160, 60);
            return panel;
        }

        private static GameObject BuildFailPanel(Transform column, LessonScreen screen)
        {
            var rt = NewRect("FailPanel", column);
            Stretch(rt);
            AddImage(rt, UiTheme.Background);

            var title = NewText("Title", rt, "Out of hearts!", 84, UiTheme.Wrong, TextAlignmentOptions.Center, FontStyles.Bold);
            TopBand(title.rectTransform, 120, 130);
            var subtitle = NewText("Subtitle", rt, "Review and try again", 50, UiTheme.TextDark);
            TopBand(subtitle.rectTransform, 260, 80);

            screen.failBodyText = NewText("Body", rt, "", 36, UiTheme.TextDark, TextAlignmentOptions.TopLeft);
            Stretch(screen.failBodyText.rectTransform, 70, 470, 70, 380);
            screen.failBodyText.enableAutoSizing = true;
            screen.failBodyText.fontSizeMin = 20;
            screen.failBodyText.fontSizeMax = 36;

            screen.retryButton = NewButton("RetryButton", rt, "TRY AGAIN", UiTheme.Primary, UiTheme.TextOnColor, 56);
            BottomBand((RectTransform)screen.retryButton.transform, 250, 160, 60);
            screen.failMapButton = NewButton("MapButton", rt, "BACK TO MAP", UiTheme.Track, UiTheme.TextDark, 50);
            BottomBand((RectTransform)screen.failMapButton.transform, 60, 150, 60);
            return rt.gameObject;
        }

        private static string BuildResults()
        {
            var (scene, canvas, column) = NewScreenScene();
            var screen = canvas.gameObject.AddComponent<ResultsScreen>();

            screen.titleText = NewText("Title", column, "Lesson complete!", 90, UiTheme.Primary, TextAlignmentOptions.Center, FontStyles.Bold);
            TopBand(screen.titleText.rectTransform, 220, 160, 40);

            var row = StatRow(column, 470, 380);
            screen.xpText = StatCard(row, "XP");
            screen.accuracyText = StatCard(row, "Accuracy");
            screen.heartsText = StatCard(row, "Hearts");

            screen.detailText = NewText("Detail", column, "", 40, UiTheme.TextDark);
            TopBand(screen.detailText.rectTransform, 900, 560, 70);

            screen.continueButton = NewButton("ContinueButton", column, "CONTINUE", UiTheme.Primary, UiTheme.TextOnColor, 60);
            BottomBand((RectTransform)screen.continueButton.transform, 200, 170, 120);

            return SaveScene(scene, SceneNames.Results);
        }

        // ------------------------------------------------------------------ scene scaffolding

        private static (Scene scene, RectTransform canvas, RectTransform column) NewScreenScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            camGo.transform.position = new Vector3(0, 0, -10);
            var cam = camGo.AddComponent<Camera>();
            cam.orthographic = true;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = UiTheme.Background;

            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));

            var canvasGo = new GameObject("Canvas", typeof(RectTransform));
            canvasGo.layer = LayerMask.NameToLayer("UI");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 1f;
            canvasGo.AddComponent<GraphicRaycaster>();
            canvasGo.AddComponent<AdaptiveCanvasMatch>();

            var bg = NewRect("Background", canvasGo.transform);
            Stretch(bg);
            AddImage(bg, UiTheme.Background).raycastTarget = false;

            // Every screen is laid out in a centred 1080-wide column (portrait design, centred on 16:9).
            var column = NewRect("Column", canvasGo.transform);
            column.anchorMin = new Vector2(0.5f, 0);
            column.anchorMax = new Vector2(0.5f, 1);
            column.pivot = new Vector2(0.5f, 0.5f);
            column.sizeDelta = new Vector2(ColumnWidth, 0);
            column.anchoredPosition = Vector2.zero;

            return (scene, (RectTransform)canvasGo.transform, column);
        }

        private static string SaveScene(Scene scene, string name)
        {
            string path = $"{ScenesDir}/{name}.unity";
            EditorSceneManager.SaveScene(scene, path);
            return path;
        }

        private static RectTransform StatRow(Transform column, float top, float height)
        {
            var row = NewRect("Stats", column);
            TopBand(row, top, height, 50);
            var h = row.gameObject.AddComponent<HorizontalLayoutGroup>();
            h.spacing = 30;
            h.childControlWidth = h.childControlHeight = true;
            h.childForceExpandWidth = h.childForceExpandHeight = true;
            return row;
        }

        private static TextMeshProUGUI StatCard(Transform row, string name)
        {
            var card = NewRect(name + "Card", row);
            AddImage(card, UiTheme.Card, spriteRounded, true);
            AddShadow(card);
            var text = NewText(name + "Text", card, name, 40, UiTheme.TextDark);
            Stretch(text.rectTransform, 16, 16, 16, 16);
            text.enableAutoSizing = true;
            text.fontSizeMin = 20;
            text.fontSizeMax = 40;
            return text;
        }

        // ------------------------------------------------------------------ UI primitives

        private static RectTransform NewRect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.layer = LayerMask.NameToLayer("UI");
            var rt = (RectTransform)go.transform;
            if (parent != null) rt.SetParent(parent, false);
            return rt;
        }

        private static void Stretch(RectTransform rt, float left = 0, float bottom = 0, float right = 0, float top = 0)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.offsetMin = new Vector2(left, bottom);
            rt.offsetMax = new Vector2(-right, -top);
        }

        /// <summary>Full-width band <paramref name="top"/> units below the top edge.</summary>
        private static void TopBand(RectTransform rt, float top, float height, float side = 0)
        {
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(0.5f, 1);
            rt.offsetMin = new Vector2(side, -(top + height));
            rt.offsetMax = new Vector2(-side, -top);
        }

        /// <summary>Full-width band <paramref name="bottom"/> units above the bottom edge.</summary>
        private static void BottomBand(RectTransform rt, float bottom, float height, float side = 0)
        {
            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(1, 0);
            rt.pivot = new Vector2(0.5f, 0);
            rt.offsetMin = new Vector2(side, bottom);
            rt.offsetMax = new Vector2(-side, bottom + height);
        }

        private static Image AddImage(RectTransform rt, Color color, Sprite sprite = null, bool sliced = false)
        {
            var img = rt.gameObject.AddComponent<Image>();
            img.sprite = sprite;
            img.type = sliced ? Image.Type.Sliced : Image.Type.Simple;
            img.color = color;
            return img;
        }

        private static void AddShadow(RectTransform rt)
        {
            var s = rt.gameObject.AddComponent<Shadow>();
            s.effectColor = new Color(0, 0, 0, 0.12f);
            s.effectDistance = new Vector2(0, -6);
        }

        private static TextMeshProUGUI NewText(string name, Transform parent, string text, float size, Color color,
            TextAlignmentOptions align = TextAlignmentOptions.Center, FontStyles style = FontStyles.Normal)
        {
            var t = NewRect(name, parent).gameObject.AddComponent<TextMeshProUGUI>();
            if (font != null) t.font = font;
            t.text = text;
            t.fontSize = size;
            t.color = color;
            t.alignment = align;
            t.fontStyle = style;
            t.enableWordWrapping = true;
            t.richText = true;
            t.raycastTarget = false;
            return t;
        }

        private static Button NewButton(string name, Transform parent, string label, Color bg, Color textColor, float fontSize)
        {
            var rt = NewRect(name, parent);
            var img = AddImage(rt, bg, spriteRounded, true);
            AddShadow(rt);
            var b = rt.gameObject.AddComponent<Button>();
            b.targetGraphic = img;
            SetDisabledColor(b, false);
            var text = NewText("Label", rt, label, fontSize, textColor, TextAlignmentOptions.Center, FontStyles.Bold);
            Stretch(text.rectTransform, 20, 8, 20, 8);
            return b;
        }

        /// <summary>Answer-option style button that grows with its text (layout-driven height).</summary>
        private static Button OptionButton(string name, Transform parent, string label, float fontSize)
        {
            var rt = NewRect(name, parent);
            var img = AddImage(rt, UiTheme.OptionNormal, spriteRounded, true);
            AddShadow(rt);
            var b = rt.gameObject.AddComponent<Button>();
            b.targetGraphic = img;
            SetDisabledColor(b, false);
            var v = VLayout(rt.gameObject, 24, 0, TextAnchor.MiddleCenter);
            v.padding = new RectOffset(28, 28, 22, 22);
            AddLayout(rt, minHeight: 120);
            NewText("Label", rt, label, fontSize, UiTheme.TextDark);
            return b;
        }

        /// <summary>
        /// Option buttons keep their full colour when disabled so the green/red result tint stays readable;
        /// the Check button dims when it can't be pressed.
        /// </summary>
        private static void SetDisabledColor(Button b, bool dim)
        {
            var c = b.colors;
            c.disabledColor = dim ? new Color(0.75f, 0.75f, 0.75f, 0.55f) : Color.white;
            b.colors = c;
        }

        private static VerticalLayoutGroup VLayout(GameObject go, int padding, float spacing, TextAnchor align = TextAnchor.UpperCenter)
        {
            var v = go.AddComponent<VerticalLayoutGroup>();
            v.padding = new RectOffset(padding, padding, padding, padding);
            v.spacing = spacing;
            v.childAlignment = align;
            v.childControlWidth = v.childControlHeight = true;
            v.childForceExpandWidth = true;
            v.childForceExpandHeight = false;
            return v;
        }

        private static LayoutElement AddLayout(Component c, float minHeight = -1, float preferredHeight = -1, float flexibleHeight = -1)
        {
            if (!c.gameObject.TryGetComponent<LayoutElement>(out var le)) le = c.gameObject.AddComponent<LayoutElement>();
            le.minHeight = minHeight;
            le.preferredHeight = preferredHeight;
            le.flexibleHeight = flexibleHeight;
            return le;
        }
    }
}
