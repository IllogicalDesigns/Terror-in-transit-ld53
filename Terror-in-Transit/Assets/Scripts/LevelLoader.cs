using UnityEngine.SceneManagement;
using UnityEngine;

public class LevelLoader : MonoBehaviour {

    public void LoadLevel(string levelName) {
        SceneManager.LoadScene(levelName);
    }

    public void RestartLevel() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
