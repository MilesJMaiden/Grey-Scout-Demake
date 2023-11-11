using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public static event Action<GameObject> OnPlayerRespawned;

    [Header("Score")]
    public int score = 0;
    public TextMeshProUGUI scoreText;

    [Header("Lives")]
    public int lives = 4;
    public TextMeshProUGUI livesText;

    [Header("Player")]
    public GameObject playerPrefab;
    public Transform spawnPoint;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        UpdateScoreText();
        UpdateLivesText();
    }

    public void AddScore(int amount)
    {
        score += amount;
        UpdateScoreText();
    }

    public void LoseLife()
    {
        lives--;
        UpdateLivesText();
        if (lives > 0)
        {
            RespawnPlayer();
        }
        else
        {
            GameOver();
        }
    }

    private void UpdateScoreText()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + score;
        }
    }

    private void UpdateLivesText()
    {
        if (livesText != null)
        {
            livesText.text = "Lives: " + lives;
        }
    }

    private void RespawnPlayer()
    {
        GameObject player = Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);
        ThirdPersonController controller = player.GetComponent<ThirdPersonController>();
        if (controller != null)
        {
            // Update the camera transform on the player
            controller.SetCamera(Camera.main.transform);
        }
        else
        {
            Debug.LogError("ThirdPersonController script not found on the respawned player.");
        }

        // Notify any listeners that the player has been respawned
        OnPlayerRespawned?.Invoke(player);
    }

    private void GameOver()
    {
        // Handle game over logic here (display game over screen, restart game, etc.)
        Debug.Log("Game Over!");
    }
}