using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
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

    [Header("Pause")]
    public bool isGamePaused = false;
    public GameObject pauseMenuUI;

    [Header("Timer")]
    public float gameTimer = 300f;
    private float timeRemaining;
    public TextMeshProUGUI timeText;

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

        timeRemaining = gameTimer;
        UpdateTimeText();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isGamePaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }

        // Timer countdown logic
        if (!isGamePaused)
        {
            if (timeRemaining > 0)
            {
                timeRemaining -= Time.deltaTime;
                UpdateTimeText();
            }
            else
            {
                TimeExpired();
            }
        }

        // Update the timer
        if (gameTimer > 0)
        {
            gameTimer -= Time.deltaTime;
            if (gameTimer <= 0)
            {
                // Time's up, restart the game
                RestartGame();
            }
        }
    }

    private void UpdateTimeText()
    {
        if (timeText != null)
        {
            TimeSpan timeSpan = TimeSpan.FromSeconds(timeRemaining);
            timeText.text = string.Format("Time: {0:D2}:{1:D2}", timeSpan.Minutes, timeSpan.Seconds);
        }
    }

    private void TimeExpired()
    {
        Debug.Log("Time's up!");
        GameOver();
    }

    public void RestartGame()
    {
        // Reload the current scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }


    public void PauseGame()
    {
        pauseMenuUI.SetActive(true); 
        Time.timeScale = 0f; 
        isGamePaused = true;
    }

    public void ResumeGame()
    {
        pauseMenuUI.SetActive(false); 
        Time.timeScale = 1f; 
        isGamePaused = false;
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
        GameObject newPlayer = Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);

        ThirdPersonController newPlayerController = newPlayer.GetComponent<ThirdPersonController>();
        PlayerInteraction newPlayerInteraction = newPlayer.GetComponent<PlayerInteraction>();

        if (newPlayerController != null)
        {
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                newPlayerController.SetCamera(mainCamera.transform);
            }
        }

        // Notify all captives of the new player
        Captive[] allCaptives = FindObjectsOfType<Captive>();
        foreach (Captive captive in allCaptives)
        {
            captive.AssignNewPlayer(newPlayer);
        }

        // Raise the event for listeners
        OnPlayerRespawned?.Invoke(newPlayer);
    }

    private void GameOver()
    {
        // Handle game over logic here (display game over screen, restart game, etc.)
        Debug.Log("Game Over!");
        RestartGame();
    }
}