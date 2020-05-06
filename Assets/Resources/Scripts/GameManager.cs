using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{

    private void OnEnable()
    {
        HexagonController.GameOver += StopGame;
    }

    private void OnDisable()
    {
        HexagonController.GameOver -= StopGame;
    }

    public void RestartLevel()
    {

        SceneManager.LoadSceneAsync(0);
        Time.timeScale = 1;
    }

    private void StopGame()
    {
        Time.timeScale = 0;
    }
}
