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
        // Show the first screen at the beginning
        firstScreen.SetActive(true);
        secondScreen.SetActive(false);
        Time.timeScale = 0f; // Pause the game
    }

    void Update()
    {
        // Check for input and manage screens
        if (!firstScreenShown && Input.GetMouseButtonDown(0)) // Click to move to the second screen
        {
            ShowSecondScreen();
        }
        else if (firstScreenShown && !secondScreenShown && Input.GetMouseButtonDown(0)) // Click to start the game
        {
            StartGame();
        }
    }

    void ShowSecondScreen()
    {
        firstScreenShown = true;
        firstScreen.SetActive(false); // Hide the first screen
        secondScreen.SetActive(true); // Show the second screen
    }

    void StartGame()
    {
        secondScreenShown = true;
        secondScreen.SetActive(false); // Hide the second screen
        Time.timeScale = 1f; // Resume the game
    }
}
