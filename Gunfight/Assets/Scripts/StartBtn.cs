using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class StartBtn : MonoBehaviour
{
    public Button StartButton;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (StartButton != null)
        {
            StartButton.onClick.AddListener(OnButtonClicked);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void OnButtonClicked()
    {
        SceneManager.LoadScene("Level1");
    }
}
