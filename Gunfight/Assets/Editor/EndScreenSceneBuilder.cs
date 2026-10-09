using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// One-time helper: creates Assets/Scenes/End Screen.unity the first time Unity opens the
// project, and adds it (and Level3 if missing) to the Build Settings. After that the scene
// is a normal scene that anyone can edit. Use Tools > Rebuild End Screen Scene to reset it.
[InitializeOnLoad]
public static class EndScreenSceneBuilder
{
    private const string ScenePath = "Assets/Scenes/End Screen.unity";
    private const string Level3Path = "Assets/Scenes/Level3.unity";

    static EndScreenSceneBuilder()
    {
        EditorApplication.delayCall += () =>
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) == null) Build();
            EnsureInBuildSettings();
        };
    }

    [MenuItem("Tools/Rebuild End Screen Scene")]
    private static void Rebuild()
    {
        if (!EditorUtility.DisplayDialog("Rebuild End Screen",
                "This replaces Assets/Scenes/End Screen.unity with a fresh default one. Any edits to it will be lost.",
                "Rebuild", "Cancel")) return;
        Build();
        EnsureInBuildSettings();
    }

    private static void Build()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // Camera
        var camGo = new GameObject("Main Camera") { tag = "MainCamera" };
        var cam = camGo.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.08f, 0.08f, 0.1f);
        cam.orthographic = true;
        camGo.AddComponent<AudioListener>();
        camGo.transform.position = new Vector3(0, 0, -10);

        // Canvas
        var canvasGo = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasGo.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        // Background (swap the colour for a Source Image when the art is ready)
        var bg = NewUI("Background", canvasGo.transform, typeof(Image));
        var bgRect = bg.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = bgRect.offsetMax = Vector2.zero;
        bg.GetComponent<Image>().color = new Color(0.08f, 0.08f, 0.1f);

        var title = NewText("Title", canvasGo.transform, "GAME OVER", 140, new Vector2(0, 220), new Vector2(1400, 200));
        title.color = Color.white;

        var winner = NewText("Winner Text", canvasGo.transform, "Player Wins!", 80, new Vector2(0, 60), new Vector2(1400, 140));
        winner.color = new Color(1f, 0.85f, 0.3f);

        // Button
        var btnGo = NewUI("Main Menu Button", canvasGo.transform, typeof(Image), typeof(Button));
        var btnRect = btnGo.GetComponent<RectTransform>();
        btnRect.anchoredPosition = new Vector2(0, -160);
        btnRect.sizeDelta = new Vector2(520, 130);
        var btnImage = btnGo.GetComponent<Image>();
        var button = btnGo.GetComponent<Button>();
        button.targetGraphic = btnImage;
        var colors = button.colors;
        colors.highlightedColor = new Color(1f, 0.85f, 0.3f);
        colors.selectedColor = new Color(1f, 0.85f, 0.3f);
        colors.pressedColor = new Color(0.8f, 0.65f, 0.2f);
        button.colors = colors;

        var label = NewText("Text (TMP)", btnGo.transform, "MAIN MENU", 60, Vector2.zero, Vector2.zero);
        var labelRect = label.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = labelRect.offsetMax = Vector2.zero;
        label.color = Color.black;

        // Event system for mouse + gamepad
        var es = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));

        // Logic
        var managerGo = new GameObject("End Screen Menu");
        var menu = managerGo.AddComponent<EndScreenMenu>();
        var so = new SerializedObject(menu);
        so.FindProperty("winnerText").objectReferenceValue = winner;
        so.FindProperty("mainMenuButton").objectReferenceValue = button;
        so.ApplyModifiedPropertiesWithoutUndo();
        UnityEventTools.AddPersistentListener(button.onClick, menu.GoToMainMenu);

        EditorSceneManager.SaveScene(scene, ScenePath);
        Debug.Log("Created " + ScenePath);
    }

    private static void EnsureInBuildSettings()
    {
        var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        bool changed = false;

        foreach (string path in new[] { Level3Path, ScenePath })
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(path) == null) continue;
            int i = scenes.FindIndex(s => s.path == path);
            if (i < 0)
            {
                scenes.Add(new EditorBuildSettingsScene(path, true));
                changed = true;
            }
            else if (!scenes[i].enabled)
            {
                scenes[i].enabled = true;
                changed = true;
            }
        }

        if (changed) EditorBuildSettings.scenes = scenes.ToArray();
    }

    private static GameObject NewUI(string name, Transform parent, params System.Type[] components)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        foreach (var c in components) go.AddComponent(c);
        return go;
    }

    private static TMP_Text NewText(string name, Transform parent, string text, float size, Vector2 pos, Vector2 dims)
    {
        var go = NewUI(name, parent);
        var rect = go.GetComponent<RectTransform>();
        rect.anchoredPosition = pos;
        rect.sizeDelta = dims;
        var t = go.AddComponent<TextMeshProUGUI>();
        t.text = text;
        t.fontSize = size;
        t.alignment = TextAlignmentOptions.Center;
        t.raycastTarget = false;
        return t;
    }
}
