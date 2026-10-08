using System.Collections;
using UnityEngine;

public class GameMaster : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    [SerializeField] private GameObject Player1;
    [SerializeField] private GameObject Player2;
    [SerializeField] private Transform PlayerSpawnParent;
    private Transform[] PlayerSpawns;
    void Start()
    {

        PlayerSpawns = new Transform[PlayerSpawnParent.childCount];
        for (int i = 0; i < PlayerSpawns.Length; i++)
        {
            PlayerSpawns[i] = PlayerSpawnParent.GetChild(i);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!Player1.activeInHierarchy)
        {
            // Award player 2 a point
            StartCoroutine(ResetStage());
        }
        if (!Player2.activeInHierarchy)
        {
            // Award player 1 a point
            StartCoroutine(ResetStage());
        }
    }
    IEnumerator ResetStage()
    {
        Debug.Log("Timer..");
        yield return StartCoroutine(ResetTimer());
        Respawn();
    }
    IEnumerator ResetTimer()
    {
        yield return new WaitForSeconds(1f);
    }
    void Respawn()
    {
        Player1.SetActive(true);
        Player2.SetActive(true);
        Player1.transform.position = PlayerSpawns[0].transform.position;
        Player2.transform.position = PlayerSpawns[1].transform.position;
    }
}
