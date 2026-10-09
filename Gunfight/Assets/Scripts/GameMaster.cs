using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameMaster : MonoBehaviour
{

    [SerializeField] private GameObject Player1;
    [SerializeField] private GameObject Player2;
    [SerializeField] private Transform Player1Spawn;
    [SerializeField] private Transform Player2Spawn;
    [SerializeField] private TextMeshProUGUI Timer;
    [SerializeField] private TextMeshProUGUI WinningPlayerText;
    [SerializeField] private TextMeshProUGUI Player1PointsText;
    [SerializeField] private TextMeshProUGUI Player2PointsText;
    private PlayerMovement Player1Movement;
    private PlayerMovement Player2Movement;
    private float player1Points = 0;
    private float player2Points = 0;
    private float winScore = 3;
    void Start()
    {
        Player1Movement = Player1.GetComponent<PlayerMovement>();
        Player2Movement = Player2.GetComponent<PlayerMovement>();

        Respawn();
    }

    // Update is called once per frame
    void Update()
    {
        CheckWin();
    }
    public void KillSequence(GameObject WinningPlayer)
    {
        if (WinningPlayer == null) return;

        GameObject loser = (WinningPlayer == Player1) ? Player2 : Player1;
        loser.GetComponent<PlayerMovement>().KnockDown();

        if (WinningPlayer == Player1)
        {
            player1Points++;
            Player1PointsText.text = player1Points.ToString();
        }
        if (WinningPlayer == Player2)
        {
            player2Points++;
            Player2PointsText.text = player2Points.ToString();
        }

        // Check now, before CheckWin resets the score
        bool matchWon = player1Points >= winScore || player2Points >= winScore;
        KillPause(matchWon);
    }

    async void KillPause(bool matchWon)
    {
        // Hitlag
        Time.timeScale = 0f;
        await Task.Delay(120);
        Time.timeScale = 1f;
        if (this == null) return;

        // Stop player's input while round ends
        Player1Movement.inputLocked = true;
        Player2Movement.inputLocked = true;

        // Wait before the next round
        await Task.Delay(1000);
        if (this == null) return;

        // The win screen handles the last kill
        if (!matchWon)
        {
            Respawn();
        }
    }
    void Respawn()
    {
        Player1Movement.inputLocked = false;
        Player2Movement.inputLocked = false;
        Player1Movement.ResetAnim();
        Player2Movement.ResetAnim();
        Player1.SetActive(true);
        Player2.SetActive(true);
        Player1.transform.position = Player1Spawn.transform.position;
        Player2.transform.position = Player2Spawn.transform.position;
        TimerSequence();
    }
    async void TimerSequence()
    {
        Player1Movement.enabled = false;
        Player2Movement.enabled = false;
        Timer.gameObject.SetActive(true);
        Timer.text = "3";
        await Task.Delay(1000);
        if (this == null) return;
        Timer.text = "2";
        await Task.Delay(1000);
        if (this == null) return;
        Timer.text = "1";
        await Task.Delay(1000);
        if (this == null) return;
        Timer.gameObject.SetActive(false);
        Player1Movement.enabled = true;
        Player2Movement.enabled = true;

    }
    void CheckWin()
    {
        if (player1Points >= winScore)
        {
            WinningPlayerText.text = "Player 1 Wins!";
            WinSequence(Player1);
            player1Points = 0;

        }
        if (player2Points >= winScore)
        {
            WinningPlayerText.text = "Player 2 Wins!";
            WinSequence(Player2);
            player2Points = 0;

        }
    }
    async void WinSequence(GameObject WinningPlayer)
    {
        WinningPlayerText.gameObject.SetActive(true);
        Player1Movement.enabled = false;
        Player2Movement.enabled = false;
        EndScreenLoader.LastWinner = (WinningPlayer == Player1) ? 1 : 2;
        EndScreenLoader.LastWinnerText = WinningPlayerText.text;
        await Task.Delay(3000);

        SwitchLevel();
    }
    void SwitchLevel()
    {
        int currentScene = SceneManager.GetActiveScene().buildIndex;
        Debug.Log(currentScene);
        if (currentScene + 1 < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
    }
}
