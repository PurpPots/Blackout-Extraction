using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using UnityEngine.Playables;
using System.Collections;


public class UIController : MonoBehaviour
{
    public Button startButton;
    public GameObject mainMenuUI;
    public GameObject cutsceneUI;
    public PlayableDirector cutsceneDirector;
    public Camera mainMenuCamera; 
    public Camera playerCamera;    

    void Start()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        if (root == null) { print("null"); }
        startButton = root.Q<Button>("start-button");
        startButton.clicked += StartButtonPressed;

        mainMenuUI.SetActive(true);
        cutsceneUI.SetActive(false);

        mainMenuCamera.gameObject.SetActive(true);
        playerCamera.gameObject.SetActive(false);  
    }

    void StartButtonPressed()
    {
        //Debug.Log("Start Game Button Clicked");
        mainMenuUI.SetActive(false);  
        cutsceneUI.SetActive(true);  

        mainMenuCamera.gameObject.SetActive(false);
        playerCamera.gameObject.SetActive(true);
        Debug.Log("pressed button");
        cutsceneDirector.Play();

        StartCoroutine(WaitForCutsceneAndStartGameplay());
    }

    private IEnumerator WaitForCutsceneAndStartGameplay()
    {

        yield return new WaitForSeconds((float)cutsceneDirector.duration);

        SceneManager.LoadScene("MainScene");

        playerCamera.gameObject.SetActive(false);  
        mainMenuCamera.gameObject.SetActive(true); 
    }
}
