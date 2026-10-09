using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

// Sends the game to the "End Screen" scene after the final level is won.
// It creates itself at startup, so it doesn't need to be placed in any scene.
// GameMaster is untouched: we just watch for its "Player Wins" text to appear.
public class EndScreenLoader : MonoBehaviour
{
    // Read by EndScreenMenu to show who won
    public static string LastWinnerText = "";
    // 1 or 2, or 0 if it could not be worked out
    public static int LastWinner = 0;

    private const string FinalLevelName = "Level3";
    private const string EndScreenSceneName = "End Screen";
    private const string WinTextObjectName = "Player Wins";
    private const float LoadDelay = 3f; // matches GameMaster's win-text delay

    private TMP_Text winText;
    private float winSeenTime = -1f;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        var go = new GameObject("EndScreenLoader");
        DontDestroyOnLoad(go);
        go.AddComponent<EndScreenLoader>();
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

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        winText = null;
        winSeenTime = -1f;
        if (scene.name != FinalLevelName) return;

        foreach (TMP_Text t in FindObjectsByType<TMP_Text>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (t.name == WinTextObjectName)
            {
                winText = t;
                break;
            }
        }
    }

    void Update()
    {
        if (winText == null || !winText.gameObject.activeInHierarchy) return;

        if (winSeenTime < 0f) winSeenTime = Time.unscaledTime;
        if (Time.unscaledTime - winSeenTime >= LoadDelay)
        {
            LastWinnerText = winText.text;
            LastWinner = LastWinnerText.Contains("1") ? 1 : LastWinnerText.Contains("2") ? 2 : 0;
            winText = null;
            SceneManager.LoadScene(EndScreenSceneName);
        }
    }
}
