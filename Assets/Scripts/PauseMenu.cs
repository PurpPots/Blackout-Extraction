using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{

    public GameObject PausePanel;
    public GameObject reticlePanel;
    public GameObject controlsPanel;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Pause();
        }
    }

    public void Pause()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        Weapon gunShoot = FindObjectOfType<Weapon>();
        if (gunShoot != null) gunShoot.readyToShoot = false;
        PausePanel.SetActive(true);
        reticlePanel.SetActive(false);
        Time.timeScale = 0;
    }

    public void Continue()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        Weapon gunShoot = FindObjectOfType<Weapon>();
        if (gunShoot != null) gunShoot.readyToShoot = true;
        PausePanel.SetActive(false);
        reticlePanel.SetActive(true);
        Time.timeScale = 1;
    }

    public void QuitMenu()
    {
        SceneManager.LoadScene("Main Menu");
    }

    public void Controls()
    {
        PausePanel.SetActive(false);
        controlsPanel.SetActive(true);
    }

    public void QuitControls()
    {
        PausePanel.SetActive(true);
        controlsPanel.SetActive(false);
    }
}
