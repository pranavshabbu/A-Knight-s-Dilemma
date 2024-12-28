using UnityEngine;
using UnityEngine.UI;

public class StartScreen : MonoBehaviour
{
    public GameObject firstScreen;  
    public GameObject secondScreen; 

    private bool firstScreenShown = false;
    private bool secondScreenShown = false;

    void Start()
    {
        firstScreen.SetActive(true);
        secondScreen.SetActive(false);
        Time.timeScale = 0f; 
    }

    void Update()
    {
        if (!firstScreenShown && Input.GetMouseButtonDown(0)) 
        {
            ShowSecondScreen();
        }
        else if (firstScreenShown && !secondScreenShown && Input.GetMouseButtonDown(0)) 
        {
            StartGame();
        }
    }

    void ShowSecondScreen()
    {
        firstScreenShown = true;
        firstScreen.SetActive(false); 
        secondScreen.SetActive(true); 
    }

    void StartGame()
    {
        secondScreenShown = true;
        secondScreen.SetActive(false); 
        Time.timeScale = 1f; 
    }
}
