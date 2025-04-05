using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameCompletedMenu : MonoBehaviour
{

    public GameObject fadeOut;
    //  public GameObject loadingText;
    public AudioSource buttonClick;
    public GameObject menuUI;
    public void returnButton()
    {
        StartCoroutine(returnGame());
    }
    IEnumerator returnGame()
    {
        fadeOut.SetActive(true);
        buttonClick.Play();
        yield return new WaitforSeconds(3);
        SceneManager.LoadScene("Main Menu");
    }
}
