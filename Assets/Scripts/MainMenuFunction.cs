using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuFunction : MonoBehaviour
{
    public GameObject fadeOut;
  //  public GameObject loadingText;
    public AudioSource buttonClick;
    public GameObject menuUI;
    public GameObject mutant;
    public GameObject controlsUI;

    public void startGameButton()
    {
        StartCoroutine(startGame());
    }

    public void quitGameButton()
    {
        Application.Quit();
    }

    IEnumerator startGame()
    {
        fadeOut.SetActive(true);
        buttonClick.Play();
        yield return new WaitforSeconds(5);
    //    loadingText.SetActive(true);
        SceneManager.LoadScene("MainScene");
    }

    public void enterControlsButton()
    {
        menuUI.SetActive(false);
        mutant.SetActive(false);
        controlsUI.SetActive(true);
    }

    public void exitControlButton()
    {
        controlsUI.SetActive(false);
        menuUI.SetActive(true);
        mutant.SetActive(true);
    }
}
