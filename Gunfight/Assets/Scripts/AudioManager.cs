using UnityEngine;
using UnityEngine.SceneManagement;

// Persistent music/SFX player. It spawns itself before the first scene loads (from
// Assets/Resources/AudioManager.prefab), so nothing has to be added to any scene.
//   Main Menu  -> menu loop
//   Level1..3  -> Round_1..3 loop, and the round-start sound whenever the "StartTimer"
//                 countdown object becomes active
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Music")]
    [SerializeField] private AudioClip menuMusic;
    [SerializeField] private AudioClip[] levelMusic; // index 0 = Level1
    [SerializeField] private AudioClip endScreenMusic;
    [SerializeField] private bool loopEndScreenMusic = false;
    [SerializeField, Range(0f, 1f)] private float musicVolume = 0.6f;

    [Header("SFX")]
    [SerializeField] private AudioClip roundStartSfx;
    [SerializeField, Range(0f, 1f)] private float sfxVolume = 1f;

    private const string EndScreenSceneName = "End Screen";
    private const string CountdownObjectName = "StartTimer";

    private AudioSource musicSource;
    private AudioSource sfxSource;
    private GameObject countdown;
    private bool countdownWasActive;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        if (Instance != null) return;
        AudioManager prefab = Resources.Load<AudioManager>("AudioManager");
        if (prefab == null)
        {
            Debug.LogWarning("AudioManager prefab not found in a Resources folder.");
            return;
        }
        Instantiate(prefab);
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = true;
        musicSource.playOnAwake = false;
        musicSource.volume = musicVolume;

        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;

        SceneManager.sceneLoaded += OnSceneLoaded;
        // The first scene is already loading by the time we exist when started from the editor
        OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    void OnDestroy()
    {
        if (Instance == this) SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (mode == LoadSceneMode.Additive) return; // overlays (e.g. How To Play) keep the current music
        musicSource.loop = scene.name != EndScreenSceneName || loopEndScreenMusic;
        PlayMusic(MusicForScene(scene.name));

        countdown = null;
        countdownWasActive = false;
        foreach (Transform t in FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (t.name == CountdownObjectName)
            {
                countdown = t.gameObject;
                break;
            }
        }
    }

    void Update()
    {
        // Round-start sound on the countdown appearing, without touching GameMaster
        bool active = countdown != null && countdown.activeInHierarchy;
        if (active && !countdownWasActive) PlaySfx(roundStartSfx);
        countdownWasActive = active;
    }

    private AudioClip MusicForScene(string sceneName)
    {
        if (sceneName == "Main Menu") return menuMusic;
        if (sceneName == EndScreenSceneName) return endScreenMusic;

        if (sceneName.StartsWith("Level") && int.TryParse(sceneName.Substring(5), out int level))
        {
            int i = level - 1;
            if (levelMusic != null && i >= 0 && i < levelMusic.Length) return levelMusic[i];
        }
        return null;
    }

    public void PlayMusic(AudioClip clip)
    {
        if (clip == null)
        {
            musicSource.Stop();
            musicSource.clip = null;
            return;
        }
        if (musicSource.clip == clip && musicSource.isPlaying) return;
        musicSource.clip = clip;
        musicSource.Play();
    }

    public void PlaySfx(AudioClip clip, float pitchVariation = 0f)
    {
        if (clip == null) return;
        sfxSource.pitch = 1f + Random.Range(-pitchVariation, pitchVariation);
        sfxSource.PlayOneShot(clip, sfxVolume);
    }
}
