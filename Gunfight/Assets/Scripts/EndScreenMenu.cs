using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Lives in the "End Screen" scene. Everything it needs is assigned in the Inspector,
// so the scene can be restyled freely without touching code.
public class EndScreenMenu : MonoBehaviour
{
    [SerializeField] private TMP_Text winnerText;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private string mainMenuSceneName = "Main Menu";

    [Header("Winner art (full-screen image, picked by who won)")]
    [SerializeField] private Image background;
    [SerializeField] private Sprite player1WinsSprite;
    [SerializeField] private Sprite player2WinsSprite;
    [Tooltip("Placeholder text objects that are hidden when the winner art is shown (the art already has the text)")]
    [SerializeField] private GameObject[] hideWhenArtShown;

    void Start()
    {
        if (winnerText != null && !string.IsNullOrEmpty(EndScreenLoader.LastWinnerText))
        {
            winnerText.text = EndScreenLoader.LastWinnerText;
        }

        ShowWinnerArt();

        // So a gamepad can press the button straight away
        if (mainMenuButton != null && EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(mainMenuButton.gameObject);
        }
    }

    private void ShowWinnerArt()
    {
        Sprite art = null;
        if (EndScreenLoader.LastWinner == 1) art = player1WinsSprite;
        else if (EndScreenLoader.LastWinner == 2) art = player2WinsSprite;

        if (art == null || background == null) return;

        background.sprite = art;
        background.color = Color.white; // no tint
        background.preserveAspect = true;

        foreach (GameObject go in hideWhenArtShown)
        {
            if (go != null) go.SetActive(false);
        }
    }

    // Wired to the button's On Click event in the Inspector
    public void GoToMainMenu()
    {
        SceneManager.LoadScene(mainMenuSceneName);
    }
}
