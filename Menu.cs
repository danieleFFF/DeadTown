using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour {
    [SerializeField] private LoadingScreen loadingScreen;
    
    public void PlayGame() {
        if (loadingScreen != null) loadingScreen.LoadScene("Map");
        else SceneManager.LoadScene("Map");
    }

    public void Training() {
        if (loadingScreen != null) loadingScreen.LoadScene("Training");
        else SceneManager.LoadScene("Training");
    }

    public void Options() {
        SceneManager.LoadScene("Settings", LoadSceneMode.Additive);
    }

    public void ExitGame() {
        Application.Quit();
    }
}
