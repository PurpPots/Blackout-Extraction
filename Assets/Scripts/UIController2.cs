using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class UIController2: MonoBehaviour
{
    public Button backButton;
    //public Button creditsButton;
    void Start()
    {

        UnityEngine.Cursor.visible = true; // Show cursor
        UnityEngine.Cursor.lockState = CursorLockMode.None; // Unlock cursor

        var root = GetComponent<UIDocument>().rootVisualElement;
        backButton = root.Q<Button>("back-button");
        //creditsButton = root.Q<Button>("credits-button");
        if (backButton == null)
        {
            return;
        }

        backButton.clicked += BackButtonPressed;
        //creditsButton.clicked += CreditsButtonPressed;
    }

    void BackButtonPressed()
    {
      //  Debug.Log("Back button clicked! Loading Main Menu...");
        SceneManager.LoadScene("Main Menu");
    }

}
