using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// Shows the "How To Play" scene on top of Level1 before the first round starts.
// It creates itself at startup. While the screen is up, the game is frozen: GameMaster is
// held back (so its 3-2-1 countdown only begins once the screen is closed), the players are
// disabled and time is paused. HowToPlayScreen calls Release() when the players dismiss it.
public class HowToPlayLoader : MonoBehaviour
{
    private const string MainMenuName = "Main Menu";
    private const string FirstLevelName = "Level1";
    private const string ScreenSceneName = "How To Play";

    private static bool shownThisRun;
    private static readonly List<Behaviour> held = new List<Behaviour>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        var go = new GameObject("HowToPlayLoader");
        DontDestroyOnLoad(go);
        go.AddComponent<HowToPlayLoader>();
    }

    void Awake()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (mode == LoadSceneMode.Additive) return;

        if (scene.name == MainMenuName)
        {
            // Back at the menu: a new run, so show the screen again next time
            shownThisRun = false;
            Release();
            return;
        }

        if (scene.name != FirstLevelName || shownThisRun) return;
        if (!Application.CanStreamedLevelBeLoaded(ScreenSceneName))
        {
            Debug.LogWarning("'" + ScreenSceneName + "' scene is not in the Build Settings, skipping the How To Play screen.");
            return;
        }

        shownThisRun = true;
        Hold();
        SceneManager.LoadScene(ScreenSceneName, LoadSceneMode.Additive);
    }

    // Runs after the scene's Awake but before any Start, so a disabled GameMaster
    // doesn't start its countdown until it is enabled again
    private static void Hold()
    {
        foreach (var p in FindObjectsByType<PlayerMovement>(FindObjectsSortMode.None)) Disable(p);
        foreach (var p in FindObjectsByType<Player>(FindObjectsSortMode.None)) Disable(p);
        foreach (var g in FindObjectsByType<GameMaster>(FindObjectsSortMode.None)) Disable(g);
        Time.timeScale = 0f;
    }

    private static void Disable(Behaviour b)
    {
        if (!b.enabled) return;
        b.enabled = false;
        held.Add(b);
    }

    public static void Release()
    {
        foreach (var b in held)
        {
            if (b != null) b.enabled = true;
        }
        held.Clear();
        Time.timeScale = 1f;
    }
}
