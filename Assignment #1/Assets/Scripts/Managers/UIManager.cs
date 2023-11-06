using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    // public Slider alertSlider;
    // public GameObject alertIndicator;
    public GameManager gameManager;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI livesText;

    private void Update()
    {
        // Assuming you have a reference to the Enemy script and can access its alert timer
        // Update the alert slider value and color here
        // Update the indicator active state based on whether the enemy is chasing
        scoreText.text = $"Score: {GameManager.Instance.score}";
        livesText.text = $"Lives: {GameManager.Instance.lives}";
    }
}