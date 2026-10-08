using TMPro;
using UnityEngine;

public class GameMaster : MonoBehaviour
{

    [SerializeField] private GameObject Player1;
    [SerializeField] private GameObject Player2;
    [SerializeField] private Transform Player1Spawn;
    [SerializeField] private Transform Player2Spawn;
    [SerializeField] private TextMeshProUGUI Timer;
    private float player1Points = 0;
    private float player2Points = 0;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        CheckWin();
    }
    void KillSequence(GameObject WinningPlayer)
    {
        if (WinningPlayer != null)
        {
            if (WinningPlayer == Player1)
            {
                player1Points++;
            }
            if (WinningPlayer == Player2)
            {
                player2Points++;
            }
        }
        TimerSequence();
        Respawn();
    }
    void Respawn()
    {
        Player1.SetActive(true);
        Player2.SetActive(true);
        Player1.transform.position = Player1Spawn.transform.position;
        Player2.transform.position = Player1Spawn.transform.position;
    }
    void TimerSequence()
    {
        Timer.gameObject.SetActive(true);
        Timer.text = "3";
    }
    void CheckWin()
    {
        if (player1Points >= 3)
        {
            WinSequence(Player1);
        }
        if (player2Points >= 3)
        {
            WinSequence(Player2);
        }
    }
    void WinSequence(GameObject WinningPlayer)
    {
    }
    void SwitchLevel()
    {

    }
}
