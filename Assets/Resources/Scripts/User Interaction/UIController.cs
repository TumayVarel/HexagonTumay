using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controls the UIs if there is any...
/// </summary>
public class UIController : MonoBehaviour
{
    public GameObject gameOverPanel;
    public Text scoreText;

    private void OnEnable()
    {
        HexagonController.GameOver += ActivateGameOverPanel;
        HexagonController.IncreaseScore += SetScoreUI;
    }

    private void OnDisable()
    {
        HexagonController.GameOver -= ActivateGameOverPanel;
        HexagonController.IncreaseScore -= SetScoreUI;
    }


    private void ActivateGameOverPanel()
    {
        gameOverPanel.SetActive(true);
    }

    private void SetScoreUI(int score)
    {
        scoreText.text = score.ToString();
    }
}
